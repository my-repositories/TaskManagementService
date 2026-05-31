# TaskManagementService

[![CI](https://github.com/my-repositories/TaskManagementService/actions/workflows/main.yml/badge.svg)](https://github.com/my-repositories/TaskManagementService/actions/workflows/main.yml)
[![Coverage](https://raw.githubusercontent.com/my-repositories/TaskManagementService/refs/heads/badges/demo_coverage.svg)](https://my-repositories.github.io/TaskManagementService)


## Установка зависимостей

```
sudo pacman -Syu dotnet-sdk aspnet-runtime aspnet-targeting-pack dotnet-runtime dotnet-targeting-pack
dotnet tool install --global dotnet-ef
set -U fish_user_paths ~/.dotnet/tools $fish_user_paths
```

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
sudo docker compose up -d --build
```

## tmux
```bash
./scripts/tms.sh start
```

### Порты:
* API + Swagger:    http://localhost:5222/swagger
* HTTP Listener:    http://localhost:5260
* RabbitMQ UI:      http://localhost:15672 (guest / guest)
* PgAdmin4:         http://localhost:5050/browser/ (postgres@postgres.com / postgres / postgres)

---

## Локальный запуск (Разработка)

### Поднять БД и Брокер
```bash
sudo docker compose up -d tmspostgres tmsdbpga tmsrabbitmq 
```

### Запустить сервисы
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

---

## Добавление EF-миграций

```
dotnet ef migrations add Initial --project "src/TaskManagementService.Dal/TaskManagementService.Dal.csproj" --startup-project "src/TaskManagementService.Api/TaskManagementService.Api.csproj" -o "Migrations"
```