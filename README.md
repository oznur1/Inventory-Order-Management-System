# Inventory & Order Management System

**A Full-Stack Inventory and Order Management Platform**

A modern inventory and order management application designed to help businesses manage products, warehouses, stock levels, orders, and inventory operations through a centralized platform.

Built with **ASP.NET Core 10, Angular, SQL Server, and Docker**, this project focuses on reliable business workflows, data consistency, transactional operations, and maintainable full-stack architecture.

---

## Overview

Managing inventory is more than keeping track of product quantities. Businesses need to ensure that stock availability remains accurate while orders are being processed, inventory is reserved, and warehouse operations are performed.

This application addresses these challenges by providing a centralized system for managing inventory and order-related operations.

The platform implements business rules that help prevent invalid stock operations, maintain accurate inventory records, and provide visibility into stock movements and low-stock conditions.

## Key Features

### Product Management
- Create, update, and manage product information.
- Maintain unique product SKUs.
- Track product prices and active status.
- View products through a structured catalog.

### Warehouse Management
- Create and manage warehouse records.
- Maintain unique warehouse codes.
- Associate inventory records with specific warehouses.

### Inventory Management
- Track physical stock quantities.
- Monitor reserved and available quantities.
- Maintain inventory records for products across warehouses.
- Configure reorder levels.
- Identify products requiring replenishment.

### Order Management
- Create and manage customer orders.
- Support order items and order-related inventory operations.
- Reserve available stock for order processing.
- Apply business rules to prevent invalid stock allocation.

### Stock Movement Management
- Record inbound and outbound stock movements.
- Maintain movement history.
- Associate transactions with products and warehouses.
- Support filtering by product, warehouse, movement type, and date.

### Low-Stock Alerts & Reporting
- Identify products at or below their reorder levels.
- Display inventory summaries.
- Monitor stock quantities and reserved inventory.
- Provide stock movement reporting.

---

## Architecture & Engineering Practices

The application follows a structured backend and frontend architecture, separating API endpoints, business logic, data access, and user interface responsibilities.

**Backend**
- Controller-based REST API architecture.
- Service layer for business logic.
- DTO-based request and response models.
- Entity Framework Core for database operations.
- Dependency Injection.
- Centralized exception handling.
- Request validation and appropriate HTTP status codes.

**Data Consistency & Transaction Management**
- Database transactions for critical stock operations.
- Optimistic concurrency control using SQL Server RowVersion.
- Unique constraints and database check constraints.
- Foreign key relationships and referential integrity.
- Validation of stock availability before outbound operations.
- Protection against invalid reservations and negative stock quantities.

**Testing**
- Unit testing with xUnit.
- API integration testing.
- SQLite-based integration test infrastructure.
- Validation of inventory and product API behavior.
- Business rule tests for stock operations and order processing.
- Error scenario and invalid operation testing.

---

## Technology Stack

| Category | Technologies |
|---|---|
| Backend | C#, ASP.NET Core 10 Web API |
| Frontend | Angular, TypeScript, HTML, SCSS |
| Database | Microsoft SQL Server |
| ORM | Entity Framework Core |
| API | RESTful APIs |
| Validation | Data Annotations, Business Rule Validation |
| Testing | xUnit, API Integration Testing |
| Containerization | Docker, Docker Compose |
| Version Control | Git, GitHub |

---

## Application Screenshots

Screenshots of the application will be added here to demonstrate the dashboard, product catalog, warehouse management, inventory alerts, and stock movement screens.

---

## Running the Application with Docker

### Prerequisites

- Docker Desktop
- Git

### 1. Clone the Repository

```bash
git clone https://github.com/oznur1/Inventory-Order-Management-System.git
cd Inventory-Order-Management-System
```

### 2. Configure Environment Variables

Create a `.env` file in the project root using the example configuration:

```bash
cp .env.example .env
```

Update the environment variables with your own local configuration.

**Important:** Never commit your actual `.env` file or database credentials to version control.

### 3. Start the Application

```bash
docker compose up -d --build
```

Docker Compose starts the following services:

- SQL Server database
- ASP.NET Core Web API
- Angular frontend served through Nginx

### 4. Access the Application

| Service | URL |
|---|---|
| Angular Frontend | http://localhost:4200 |
| Backend API | http://localhost:5057 |
| Swagger UI | http://localhost:5057/swagger |

---

## Project Structure

```text
Inventory-Order-Management-System/
│
├── backend/
│   ├── Controllers/
│   ├── Data/
│   ├── DTOs/
│   ├── Entities/
│   ├── Services/
│   ├── Migrations/
│   ├── Dockerfile
│   └── backend.csproj
│
├── backend.Tests/
│   ├── Infrastructure/
│   ├── Integration/
│   └── Unit Tests
│
├── frontend/
│   ├── src/
│   │   └── app/
│   │       ├── models/
│   │       ├── pages/
│   │       └── services/
│   └── Dockerfile
│
├── docker-compose.yml
├── .env.example
└── README.md
```

---

## Development Journey

This project was developed through a structured 14-day implementation plan.

The development process covered:

- Requirements analysis and database design.
- Entity modeling and relational database configuration.
- Backend API development and business logic implementation.
- Angular frontend development.
- Order processing and inventory reservation workflows.
- Transaction management and concurrency control.
- Reporting, filtering, and low-stock alerts.
- Unit and integration testing.
- Error handling and validation.
- Docker containerization and deployment preparation.

The goal was to build a practical business application while applying software engineering principles throughout the development lifecycle.

---

## Future Improvements

Potential next development steps include:

- Role-based authentication and authorization.
- Advanced order lifecycle management.
- Automated notification services.
- Cloud deployment and managed database integration.
- CI/CD pipeline automation.
- Expanded reporting and analytics.

---



This project demonstrates practical full-stack development, REST API design, relational database modeling, business rule implementation, automated testing, and containerization.
