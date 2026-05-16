import { GraphQLError, GraphQLScalarType, Kind } from "graphql";

import { pagination } from "../config.js";
import * as repository from "../repository/sensorReadingRepository.js";
import { getSelectedColumns } from "../utils/fieldSelection.js";

const dateTimeScalar = new GraphQLScalarType({
  name: "DateTime",
  description: "ISO-8601 DateTime string",
  serialize(value) {
    if (value instanceof Date) {
      return value.toISOString();
    }
    return value;
  },
  parseValue(value) {
    return new Date(value);
  },
  parseLiteral(ast) {
    if (ast.kind === Kind.STRING) {
      return new Date(ast.value);
    }
    throw new GraphQLError("DateTime must be a string", {
      extensions: { code: "BAD_USER_INPUT" },
    });
  },
});

function badUserInput(message) {
  return new GraphQLError(message, {
    extensions: { code: "BAD_USER_INPUT" },
  });
}

function notFound(id) {
  return new GraphQLError(`Očitavanje sa id=${id} nije pronađeno`, {
    extensions: { code: "NOT_FOUND" },
  });
}

function validatePagination(limit, offset) {
  const resolvedLimit = limit ?? pagination.defaultLimit;

  if (resolvedLimit < 1 || resolvedLimit > pagination.maxLimit) {
    throw badUserInput(`limit must be between 1 and ${pagination.maxLimit}`);
  }

  if (offset < 0) {
    throw badUserInput("offset must be greater than or equal to 0");
  }

  return { limit: resolvedLimit, offset };
}

function parseNumericId(id) {
  const numericId = Number(id);
  if (!Number.isInteger(numericId) || numericId <= 0) {
    throw badUserInput("id must be a positive integer");
  }
  return numericId;
}

export const resolvers = {
  DateTime: dateTimeScalar,

  Query: {
    async sensorReadings(_parent, args, context, info) {
      const { limit, offset } = validatePagination(args.limit, args.offset);
      const columns = getSelectedColumns(info);
      return repository.listReadings(context.pool, limit, offset, columns);
    },

    async sensorReading(_parent, args, context, info) {
      const id = parseNumericId(args.id);
      const columns = getSelectedColumns(info);
      const reading = await repository.getReadingById(context.pool, id, columns);

      if (!reading) {
        throw notFound(id);
      }

      return reading;
    },
  },

  Mutation: {
    async createSensorReading(_parent, args, context, info) {
      const columns = getSelectedColumns(info);
      return repository.createReading(context.pool, args.input, columns);
    },

    async updateSensorReading(_parent, args, context, info) {
      const id = parseNumericId(args.id);
      const columns = getSelectedColumns(info);
      const updated = await repository.updateReading(
        context.pool,
        id,
        args.input,
        columns,
      );

      if (!updated) {
        throw notFound(id);
      }

      return updated;
    },

    async deleteSensorReading(_parent, args, context) {
      const id = parseNumericId(args.id);
      const deleted = await repository.deleteReading(context.pool, id);

      if (!deleted) {
        throw notFound(id);
      }

      return true;
    },
  },
};
