# Coffee Shop API

This project demonstrates cloud design patterns in a coffee shop scenario using ASP.NET Core, Docker Compose, and static frontend assets.

## Architecture

- Frontend: vanilla JavaScript served by nginx
- API: ASP.NET Core Web API
- Storage emulator: Azurite
- Uploader: uploads static content to Azure Storage-compatible storage

## Run with Docker Compose

```bash
docker compose up --build
```

Then open:

- Frontend: http://localhost:8080
- API: http://localhost:5001
- Swagger: http://localhost:5001/swagger
- Azurite: http://localhost:10000

## Stop the stack

```bash
docker compose down
```

## Included design patterns

- Static Content Hosting Pattern
- Retry Pattern
- Circuit Breaker Pattern

## Main folders

- `Controllers/` — API endpoints
- `StaticAssets/` — frontend HTML/CSS/JS files
- `StaticContentUploader/` — uploads assets to Blob Storage
- `docker-compose.yml` — orchestrates all services
