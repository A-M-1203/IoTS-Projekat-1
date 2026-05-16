from contextlib import asynccontextmanager

from fastapi import FastAPI

from database import close_pool, init_pool
from routers.sensor_readings import router as sensor_readings_router


@asynccontextmanager
async def lifespan(app: FastAPI):
    init_pool()
    yield
    close_pool()


app = FastAPI(
    title="IoTS Projekat 1 API",
    description="REST servis za sensor_readings",
    version="1.0.0",
    lifespan=lifespan,
)

app.include_router(
    sensor_readings_router,
    prefix="/sensor-readings",
    tags=["Sensor readings"],
)
