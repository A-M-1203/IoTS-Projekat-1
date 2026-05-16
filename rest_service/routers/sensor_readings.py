from typing import Annotated

from fastapi import APIRouter, HTTPException, Query, status

import repository
from database import DbConnection
from schemas import (
    SensorReadingCreate,
    SensorReadingResponse,
    SensorReadingUpdate,
)

router = APIRouter()

MAX_LIMIT = 1000
DEFAULT_LIMIT = 100


@router.get(
    "",
    response_model=list[SensorReadingResponse],
    summary="Lista svih očitavanja",
)
def list_sensor_readings(
    conn: DbConnection,
    limit: Annotated[int, Query(ge=1, le=MAX_LIMIT)] = DEFAULT_LIMIT,
    offset: Annotated[int, Query(ge=0)] = 0,
) -> list[dict]:
    return repository.list_readings(conn, limit, offset)


@router.get(
    "/{reading_id}",
    response_model=SensorReadingResponse,
    summary="Jedno očitavanje po ID-u",
)
def get_sensor_reading(conn: DbConnection, reading_id: int) -> dict:
    row = repository.get_reading(conn, reading_id)
    if row is None:
        raise HTTPException(
            status_code=status.HTTP_404_NOT_FOUND,
            detail=f"Očitavanje sa id={reading_id} nije pronađeno",
        )
    return row


@router.post(
    "",
    response_model=SensorReadingResponse,
    status_code=status.HTTP_201_CREATED,
    summary="Kreiranje novog očitavanja",
)
def create_sensor_reading(
    conn: DbConnection, payload: SensorReadingCreate
) -> dict:
    return repository.create_reading(conn, payload)


@router.put(
    "/{reading_id}",
    response_model=SensorReadingResponse,
    summary="Potpuna zamena očitavanja",
)
def update_sensor_reading(
    conn: DbConnection, reading_id: int, payload: SensorReadingUpdate
) -> dict:
    row = repository.update_reading(conn, reading_id, payload)
    if row is None:
        raise HTTPException(
            status_code=status.HTTP_404_NOT_FOUND,
            detail=f"Očitavanje sa id={reading_id} nije pronađeno",
        )
    return row


@router.delete(
    "/{reading_id}",
    status_code=status.HTTP_204_NO_CONTENT,
    summary="Brisanje očitavanja",
)
def delete_sensor_reading(conn: DbConnection, reading_id: int) -> None:
    deleted = repository.delete_reading(conn, reading_id)
    if not deleted:
        raise HTTPException(
            status_code=status.HTTP_404_NOT_FOUND,
            detail=f"Očitavanje sa id={reading_id} nije pronađeno",
        )
