from collections.abc import Generator
from typing import Annotated

from fastapi import Depends
from psycopg import Connection
from psycopg.rows import dict_row
from psycopg_pool import ConnectionPool

from config import DATABASE_URL

pool: ConnectionPool | None = None


def init_pool() -> None:
    global pool
    pool = ConnectionPool(
        conninfo=DATABASE_URL,
        kwargs={"row_factory": dict_row},
        min_size=1,
        max_size=10,
        open=False,
    )
    pool.open()


def close_pool() -> None:
    global pool
    if pool is not None:
        pool.close()
        pool = None


def get_db() -> Generator[Connection, None, None]:
    if pool is None:
        raise RuntimeError("Database pool is not initialized")
    with pool.connection() as conn:
        yield conn


DbConnection = Annotated[Connection, Depends(get_db)]
