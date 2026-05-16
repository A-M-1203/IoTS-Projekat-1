import { ApolloServer } from "@apollo/server";
import { expressMiddleware } from "@as-integrations/express4";
import express from "express";

import { port } from "./config.js";
import { pool } from "./db/pool.js";
import { resolvers } from "./schema/resolvers.js";
import { typeDefs } from "./schema/typeDefs.js";

async function startServer() {
  const app = express();
  const server = new ApolloServer({
    typeDefs,
    resolvers,
  });

  await server.start();

  app.get("/", (_req, res) => {
    res.send("IoTS Projekat 1 GraphQL servis. Endpoint: POST /graphql");
  });

  app.use(
    "/graphql",
    express.json(),
    expressMiddleware(server, {
      context: async () => ({ pool }),
    }),
  );

  app.listen(port, () => {
    console.log(`GraphQL server ready at http://localhost:${port}/graphql`);
  });
}

startServer().catch((error) => {
  console.error("Failed to start GraphQL server:", error);
  process.exit(1);
});
