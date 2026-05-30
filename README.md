# TaskManagementService

[![CI](https://github.com/my-repositories/TaskManagementService/actions/workflows/main.yml/badge.svg)](https://github.com/my-repositories/TaskManagementService/actions/workflows/main.yml)
[![Coverage](https://raw.githubusercontent.com/my-repositories/TaskManagementService/refs/heads/badges/demo_coverage.svg)](https://my-repositories.github.io/TaskManagementService)


# Task Management Service

## Проекты
* `src/TaskManagementService.Api` — Основное CRUD API. Содержит фоновый воркер OutboxProcessorBackgroundService.
* `src/TaskManagementService.Dal` — База данных (EF Core + PostgreSQL). Таблицы задач и outbox-сообщений.
* `src/TaskManagementService.Domain` — Модели, DTO, интерфейсы.
* `src/TaskManagementService.Listener.Http.Api` — Синхронное взаимодействие между сервисами.
* `src/TaskManagementService.Listener.Rabbit.Api` — Асинхронное взаимодействие между сервисами через очередь
сообщений.
* `tests/` — xUnit тесты.

## Запуск через Docker (Все сервисы + БД + Брокер)
```bash
docker compose up -d --build
```

### Порты:
* API + Swagger: `http://localhost:5222/swagger`
* HTTP Listener: `http://localhost:5260`
* RabbitMQ UI: `http://localhost:15672` (guest/guest)

---

## Локальный запуск (Разработка)

### 1. Поднять БД и Брокер
```bash
docker compose up -d postgres rabbitmq
```

### 2. Накатить миграции
```bash
dotnet ef database update -p src/TaskManagementService.Dal -s src/TaskManagementService.Api
```

### 3. Запустить сервисы
```bash
dotnet run --project src/TaskManagementService.Api
dotnet run --project src/TaskManagementService.Listener.Http.Api
dotnet run --project src/TaskManagementService.Listener.Rabbit.Api
```

---

## Тесты
```bash
dotnet test
```
