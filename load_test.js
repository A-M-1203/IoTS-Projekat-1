import http from "k6/http";
import grpc from "k6/net/grpc";
import { Counter, Rate } from "k6/metrics";
import { check, sleep } from "k6";

// k6 za gRPC ima samo grpc_req_duration (nema grpc_reqs ni grpc_req_failed).
const grpcCalls = new Counter("grpc_calls");
const grpcFailed = new Rate("grpc_failed");

const REST_URL = __ENV.REST_URL || "http://localhost:8000";
const GRAPHQL_URL = __ENV.GRAPHQL_URL || "http://localhost:4000/graphql";
const GRPC_ADDR = __ENV.GRPC_ADDR || "localhost:8080";
const VUS = Number(__ENV.VUS || 10);

const grpcClient = new grpc.Client();
grpcClient.load(
  ["protos", "grpc_service/GrpcService/Protos"],
  "sensor_readings.proto",
);

export const options = {
  scenarios: {
    load_test: {
      executor: "ramping-vus",
      startVUs: 0,
      stages: [
        { duration: "30s", target: VUS },
        { duration: "1m", target: VUS },
        { duration: "10s", target: 0 },
      ],
      gracefulRampDown: "10s",
    },
  },
  summaryTrendStats: ["avg", "p(95)", "max"],
  // Sub-metrike po tagu `api` moraju biti u thresholds da uđu u handleSummary.
  thresholds: {
    "http_req_duration{api:rest}": ["avg<60000"],
    "http_req_duration{api:graphql}": ["avg<60000"],
    "grpc_req_duration{api:grpc}": ["avg<60000"],
    "http_reqs{api:rest}": ["count>=0"],
    "http_reqs{api:graphql}": ["count>=0"],
    "grpc_calls{api:grpc}": ["count>=0"],
    "http_req_failed{api:rest}": ["rate<1"],
    "http_req_failed{api:graphql}": ["rate<1"],
    "grpc_failed{api:grpc}": ["rate<1"],
  },
};

export default function () {
  testRest();
  testGraphql();
  testGrpc();
  sleep(1);
}

function testRest() {
  const listRes = http.get(`${REST_URL}/sensor-readings?limit=10&offset=0`, {
    tags: { api: "rest" },
  });
  check(listRes, { "REST list status 200": (r) => r.status === 200 });

  const id = 1 + Math.floor(Math.random() * 100);
  const getRes = http.get(`${REST_URL}/sensor-readings/${id}`, {
    tags: { api: "rest" },
  });
  check(getRes, {
    "REST get status 200 or 404": (r) => r.status === 200 || r.status === 404,
  });
}

function testGraphql() {
  const body = JSON.stringify({
    query: `{
      sensorReadings(limit: 10, offset: 0) {
        id
        deviceId
        temperature
      }
    }`,
  });

  const res = http.post(GRAPHQL_URL, body, {
    headers: { "Content-Type": "application/json" },
    tags: { api: "graphql" },
  });

  check(res, {
    "GraphQL status 200": (r) => r.status === 200,
    "GraphQL no errors": (r) => {
      try {
        return !JSON.parse(r.body).errors;
      } catch {
        return false;
      }
    },
  });
}

function isGrpcSuccess(res) {
  return (
    res &&
    (res.status === grpc.StatusOK || res.status === grpc.StatusNotFound)
  );
}

function recordGrpcCall(res) {
  grpcCalls.add(1, { api: "grpc" });
  grpcFailed.add(!isGrpcSuccess(res), { api: "grpc" });
}

function testGrpc() {
  grpcClient.connect(GRPC_ADDR, { plaintext: true, timeout: "10s" });

  const listRes = grpcClient.invoke(
    "sensorreadings.SensorReadingService/ListSensorReadings",
    { limit: 10, offset: 0 },
    { tags: { api: "grpc" } },
  );
  recordGrpcCall(listRes);
  check(listRes, { "gRPC list OK": (r) => r && r.status === grpc.StatusOK });

  const id = 1 + Math.floor(Math.random() * 100);
  const getRes = grpcClient.invoke(
    "sensorreadings.SensorReadingService/GetSensorReading",
    { id },
    { tags: { api: "grpc" } },
  );
  recordGrpcCall(getRes);
  check(getRes, {
    "gRPC get OK or NOT_FOUND": (r) => isGrpcSuccess(r),
  });

  grpcClient.close();
}

function formatMs(value) {
  if (value === undefined || value === null) {
    return "n/a";
  }
  return `${value.toFixed(2)} ms`;
}

function submetricKeys(metrics, baseName, api) {
  const exact = `${baseName}{api:${api}}`;
  if (metrics[exact]) {
    return [exact];
  }
  return Object.keys(metrics).filter(
    (key) => key.startsWith(`${baseName}{`) && key.includes(`api:${api}`),
  );
}

function aggregateTrend(metrics, keys) {
  if (keys.length === 0) {
    return null;
  }
  if (keys.length === 1) {
    return metrics[keys[0]]?.values ?? null;
  }

  let totalCount = 0;
  let weightedAvg = 0;
  let maxP95 = 0;

  for (const key of keys) {
    const values = metrics[key]?.values;
    if (!values) {
      continue;
    }
    const count = values.count ?? 0;
    if (count > 0) {
      weightedAvg += (values.avg ?? 0) * count;
      totalCount += count;
    }
    if (values["p(95)"] != null && values["p(95)"] > maxP95) {
      maxP95 = values["p(95)"];
    }
  }

  if (totalCount === 0) {
    return metrics[keys[0]]?.values ?? null;
  }

  return {
    avg: weightedAvg / totalCount,
    "p(95)": maxP95,
    count: totalCount,
  };
}

function aggregateCounter(metrics, keys) {
  if (keys.length === 0) {
    return null;
  }

  let count = 0;
  let rate = 0;

  for (const key of keys) {
    const values = metrics[key]?.values;
    if (!values) {
      continue;
    }
    count += values.count ?? 0;
    rate += values.rate ?? 0;
  }

  return { count, rate };
}

function aggregateRate(metrics, keys) {
  if (keys.length === 0) {
    return null;
  }
  if (keys.length === 1) {
    return metrics[keys[0]]?.values ?? null;
  }

  let totalPasses = 0;
  let totalFails = 0;

  for (const key of keys) {
    const values = metrics[key]?.values;
    if (!values) {
      continue;
    }
    totalPasses += values.passes ?? 0;
    totalFails += values.fails ?? 0;
  }

  const total = totalPasses + totalFails;
  return {
    rate: total > 0 ? totalFails / total : 0,
    passes: totalPasses,
    fails: totalFails,
  };
}

function getDurationMetric(data, api) {
  const prefix = api === "grpc" ? "grpc_req_duration" : "http_req_duration";
  const keys = submetricKeys(data.metrics, prefix, api);
  return aggregateTrend(data.metrics, keys);
}

function getRequestMetric(data, api) {
  const prefix = api === "grpc" ? "grpc_calls" : "http_reqs";
  const keys = submetricKeys(data.metrics, prefix, api);
  return aggregateCounter(data.metrics, keys);
}

function getFailedMetric(data, api) {
  const prefix = api === "grpc" ? "grpc_failed" : "http_req_failed";
  const keys = submetricKeys(data.metrics, prefix, api);
  return aggregateRate(data.metrics, keys);
}

function buildApiReport(data, api, label) {
  const duration = getDurationMetric(data, api);
  const reqs = getRequestMetric(data, api);
  const failed = getFailedMetric(data, api);

  const totalRate = reqs?.rate ?? 0;
  const failRate = failed?.rate ?? 0;
  const successRps = totalRate * (1 - failRate);

  return {
    api: label,
    avg_latency: duration?.avg ?? null,
    p95_latency: duration?.["p(95)"] ?? null,
    total_rps: totalRate,
    success_rps: successRps,
    fail_rate_pct: (failRate * 100).toFixed(2),
  };
}

export function handleSummary(data) {
  const reports = [
    buildApiReport(data, "rest", "REST"),
    buildApiReport(data, "graphql", "GraphQL"),
    buildApiReport(data, "grpc", "gRPC"),
  ];

  const lines = [
    "",
    "=== IoTS Load Test Summary ===",
    `Virtual users (target): ${VUS}`,
    "",
    "API          | avg latency | p95 latency | success RPS | total RPS | fail %",
    "-------------|-------------|-------------|-------------|-----------|-------",
  ];

  for (const r of reports) {
    lines.push(
      `${r.api.padEnd(12)} | ${formatMs(r.avg_latency).padStart(11)} | ${formatMs(r.p95_latency).padStart(11)} | ${r.success_rps.toFixed(2).padStart(11)} | ${r.total_rps.toFixed(2).padStart(9)} | ${r.fail_rate_pct.padStart(5)}%`,
    );
  }

  lines.push("");
  lines.push("Napomena: success RPS = ukupan RPS × (1 - stopa neuspeha)");
  lines.push("");

  const text = lines.join("\n");

  return {
    stdout: text,
    "load_test_summary.json": JSON.stringify(
      {
        vus: VUS,
        reports,
        raw_metrics: {
          rest: {
            duration: getDurationMetric(data, "rest"),
            reqs: getRequestMetric(data, "rest"),
            failed: getFailedMetric(data, "rest"),
          },
          graphql: {
            duration: getDurationMetric(data, "graphql"),
            reqs: getRequestMetric(data, "graphql"),
            failed: getFailedMetric(data, "graphql"),
          },
          grpc: {
            duration: getDurationMetric(data, "grpc"),
            reqs: getRequestMetric(data, "grpc"),
            failed: getFailedMetric(data, "grpc"),
          },
        },
        metric_keys_found: Object.keys(data.metrics).filter((k) =>
          k.includes("api:"),
        ),
      },
      null,
      2,
    ),
  };
}
