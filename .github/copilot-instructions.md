# Copilot Instructions

## Project Guidelines
- Prefer not to host RabbitMQ consumers inside the main web application; use a separate worker project for message consumption.
- Use clean architecture principles when applying project changes; prefer application abstractions over direct infrastructure usage in workers/services.