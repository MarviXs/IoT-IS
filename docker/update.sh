#!/usr/bin/env bash
set -euo pipefail

SERVICE=backend

echo "Pulling images..."
docker compose pull

echo "Starting containers..."
docker compose up -d

echo "Waiting for backend to be ready..."

echo "Running app migration..."
docker compose exec -T "$SERVICE" sh -c "./appMigration"

echo "Running Timescale migration..."
docker compose exec -T "$SERVICE" sh -c "./timescaleMigration"

echo "Update complete ✅"
