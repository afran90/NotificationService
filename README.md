# Notification Service

A clean architecture-based ASP.NET Core Web API for managing notifications, user subscriptions, and notification delivery status.

## Description

This solution is built with `.NET 10` and C# and is organized into four projects:

- `NotificationService` (API layer)
- `NotificationService.Application` (application services/contracts)
- `NotificationService.Domain` (domain entities/enums)
- `NotificationService.Infrastructure` (PostgreSQL, Redis, RabbitMQ integrations)

The API currently provides endpoints for:

- `POST /notifications/send` — send a notification to a user
- `GET /notifications/{userId}?page=1&pageSize=20` — get latest notifications for a user with pagination
- `POST /notifications/read` — mark a notification as read
- `POST /subscriptions/update` — subscribe or unsubscribe a user from a notification type
- Notification delivery status update/list endpoints

`POST /notifications/send` validates the user's subscription preferences before creating the notification. If the user has unsubscribed from that notification type, the API returns `400 Bad Request`.

## Tech Stack

- ASP.NET Core Web API
- Entity Framework Core + PostgreSQL
- Redis cache
- RabbitMQ messaging
- Swagger / OpenAPI

## Database Schema

### Tables

#### `notifications`
Stores user notifications across multiple channels (Push, Email, SMS, InApp).

| Column | Type | Constraints |
|--------|------|-----------|
| Id | UUID | Primary Key |
| UserId | UUID | Not Null, Indexed |
| Type | int (enum) | Not Null (0=Push, 1=Email, 2=SMS, 3=InApp) |
| Title | varchar(256) | Not Null |
| Message | text | Not Null |
| Metadata | jsonb | Nullable |
| Status | int (enum) | Not Null (0=Unread, 1=Read) |
| CreatedAtUtc | timestamp | Not Null, Indexed |
| DeliveredAtUtc | timestamp | Nullable |
| UpdatedAtUtc | timestamp | Nullable |

**Indexes:**
- `idx_notifications_userid` on UserId
- `idx_notifications_userid_status` on UserId, Status
- `idx_notifications_createdat` on CreatedAtUtc

#### `user_subscriptions`
Manages which notification types each user is subscribed to.

| Column | Type | Constraints |
|--------|------|-----------|
| Id | UUID | Primary Key |
| UserId | UUID | Not Null, Indexed |
| NotificationType | int (enum) | Not Null (0=Push, 1=Email, 2=SMS, 3=InApp) |
| IsSubscribed | boolean | Not Null, Default=true |
| CreatedAtUtc | timestamp | Not Null |
| UpdatedAtUtc | timestamp | Nullable |

**Indexes:**
- `idx_user_subscriptions_userid` on UserId
- `uk_user_subscriptions_userid_type` (Unique) on UserId, NotificationType

#### `notification_deliveries`
Tracks the delivery status of notifications to their destinations.

| Column | Type | Constraints |
|--------|------|-----------|
| Id | UUID | Primary Key |
| NotificationId | UUID | Not Null, Foreign Key → notifications.Id, Indexed |
| Destination | varchar(512) | Not Null |
| Status | int (enum) | Not Null (0=Pending, 1=Sent, 2=Failed, 3=Retrying) |
| FailureReason | varchar(1024) | Nullable |
| DeliveredAtUtc | timestamp | Nullable |
| CreatedAtUtc | timestamp | Not Null |
| UpdatedAtUtc | timestamp | Nullable |

**Indexes:**
- `idx_notification_deliveries_notificationid` on NotificationId
- `idx_notification_deliveries_status` on Status

#### `notification_templates` (Optional)
Template definitions for notification formatting and rendering.

| Column | Type | Constraints |
|--------|------|-----------|
| Id | UUID | Primary Key |
| Name | varchar(256) | Not Null, Unique |
| TitleTemplate | text | Not Null |
| BodyTemplate | text | Not Null |
| ChannelType | int (enum) | Not Null (0=Push, 1=Email, 2=SMS, 3=InApp) |
| CreatedAtUtc | timestamp | Not Null |
| UpdatedAtUtc | timestamp | Nullable |

**Indexes:**
- `uk_notification_templates_name` (Unique) on Name
- `idx_notification_templates_channeltype` on ChannelType

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

## Entity Relationships

```
Notification (1) ─── (Many) NotificationDelivery
     │
     └─ Tracks deliveries across multiple channels
     
UserSubscription ─── Manages notification preferences per user
```

## AI-assisted implementation note

This project is being implemented using AI tools as an experiment to evaluate how efficiently end-to-end software development can be delivered with AI assistance.
