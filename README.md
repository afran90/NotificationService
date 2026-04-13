# Notification Service

A clean architecture-based ASP.NET Core Web API for managing notifications, user subscriptions, and notification delivery status.

## Description

This solution is built with `.NET 10` and C# and is organized into four projects:

- `NotificationService` (API layer)
- `NotificationService.Application` (application services/contracts)
- `NotificationService.Domain` (domain entities/enums)
- `NotificationService.Infrastructure` (PostgreSQL, Redis, RabbitMQ integrations)

The API currently provides endpoints for:

- Creating and listing notifications per user
- Creating and listing user subscriptions per user
- Updating and listing notification deliveries

## Tech Stack

- ASP.NET Core Web API
- Entity Framework Core + PostgreSQL
- Redis cache
- RabbitMQ messaging
- Swagger / OpenAPI

## Configuration

Main settings are in:

- `NotificationService/appsettings.json`
- `NotificationService/appsettings.Development.json`

Configure these sections before running:

- `ConnectionStrings:Postgres`
- `Redis:ConnectionString`
- `RabbitMq` settings (`HostName`, `Port`, `UserName`, `Password`, `Exchange`)

## Run the API

From the solution root:

1. Restore dependencies
2. Run the API project (`NotificationService`)
3. Open Swagger in development mode to test endpoints

Health check endpoint:

- `/health`

## AI-assisted implementation note

This project is being implemented using AI tools as an experiment to evaluate how efficiently end-to-end software development can be delivered with AI assistance.
