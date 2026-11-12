# Inventory & Order Management System

A full-stack enterprise-style Inventory & Order Management System developed using **ASP.NET Core 10, Angular, SQL Server, and Docker**.

This project was built through a structured 14-day development workflow, focusing on inventory accuracy, order processing, transactional consistency, and reliable stock management.

## Project Objectives

The main objective is to build a reliable inventory and order management platform that maintains data consistency across products, warehouses, inventory records, and stock movements.

## Key Features

- **Product Management:** Create, update, and manage product records.
- **Warehouse Management:** Manage warehouses and their operational information.
- **Inventory Tracking:** Monitor stock quantities, reserved quantities, and available inventory.
- **Order Management:** Manage customer orders and order items.
- **Stock Reservations:** Reserve inventory for order processing.
- **Stock Movements:** Record inbound and outbound stock transactions.
- **Low-Stock Alerts:** Identify products that reach or fall below their reorder levels.
- **Reporting:** View inventory summaries and stock movement information.
- **Filtering:** Filter stock movements by product, warehouse, movement type, and date.
- **Data Consistency:** Use database transactions and optimistic concurrency control with RowVersion.
- **Automated Testing:** Validate business rules and API behavior with xUnit tests and integration tests.
- **Docker Integration:** Containerize the backend, frontend, and SQL Server environment.

## Technology Stack

**Backend**
- C#
- ASP.NET Core 10 Web API
- Entity Framework Core
- SQL Server
- RESTful API

**Frontend**
- Angular
- TypeScript
- HTML
- SCSS
- Reactive Forms
- HttpClient

**Testing & DevOps**
- xUnit
- API Integration Testing
- Docker
- Docker Compose
- Git & GitHub

## Development Workflow

The project was developed over 14 days, progressing through:

1. Project setup and database modeling
2. Entity relationships and stock movement architecture
3. Product, warehouse, and inventory APIs
4. Angular product and warehouse interfaces
5. Order processing, stock reservations, and transactions
6. Low-stock alerts, reporting, and automated tests
7. Error handling, integration testing, Docker, and deployment preparation

## Project Focus

The primary engineering focus is maintaining inventory accuracy and data consistency during concurrent operations, stock reservations, and inbound/outbound transactions.

This project demonstrates practical full-stack development, REST API design, relational database modeling, business rule implementation, automated testing, and containerization.
