DB_HOST = "localhost"
DB_PORT = 5432
DB_NAME = "iots-projekat-1"
DB_USER = "postgres"
DB_PASSWORD = "vobo1234"

DATABASE_URL = (
    f"postgresql://{DB_USER}:{DB_PASSWORD}@{DB_HOST}:{DB_PORT}/{DB_NAME}"
)
