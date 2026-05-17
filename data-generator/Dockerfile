FROM python:3.14-slim-bookworm

WORKDIR /app

COPY requirements.txt .
RUN pip install --no-cache-dir -r requirements.txt

COPY generator.py .
COPY BIED_Smart_Agriculture_Dataset.csv .

CMD ["python", "generator.py"]
