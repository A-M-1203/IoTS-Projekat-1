export const dbConfig = {
  host: process.env.DB_HOST || "localhost",
  port: Number(process.env.DB_PORT || 5432),
  database: process.env.DB_NAME || "iots-projekat-1",
  user: process.env.DB_USER || "postgres",
  password: process.env.DB_PASSWORD || "vobo1234",
};

export const port = Number(process.env.PORT || 4000);

export const pagination = {
  defaultLimit: 100,
  maxLimit: 1000,
};
