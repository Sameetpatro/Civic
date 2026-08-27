FROM python:3.11-slim

WORKDIR /app

# Install system dependencies
RUN apt-get update && apt-get install -y --no-install-recommends \
    build-essential \
    && rm -rf /var/lib/apt/lists/*

# Install Python requirements
COPY ml-engine/requirements.txt .
RUN pip install --no-cache-dir -r requirements.txt

# Copy ML source code
COPY ml-engine/ .

# Pre-train models if not baked
RUN python train_models.py

EXPOSE 8000

CMD ["uvicorn", "app:app", "--host", "0.0.0.0", "--port", "8000"]
