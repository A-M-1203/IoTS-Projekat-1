import pg from "pg";

import { dbConfig } from "../config.js";

const { Pool } = pg;

export const pool = new Pool(dbConfig);
