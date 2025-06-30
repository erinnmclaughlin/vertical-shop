# Vertical Shop - Vertical Slice Architecture Sample

This is a sample application demonstrating **Vertical Slice Architecture** in .NET. The application is a simple e-commerce system that manages products and inventory using a modular, domain-driven approach.

## What is Vertical Slice Architecture?

Vertical Slice Architecture is an architectural pattern that organizes code around business features rather than technical concerns. Each "slice" contains all the code needed to implement a specific business capability - from the API endpoints down to the data access layer.

### Key Benefits

- **Feature Cohesion**: All code related to a business feature is grouped together
- **Reduced Coupling**: Features are isolated from each other, making the system more maintainable
- **Clear Boundaries**: Each module has well-defined responsibilities and interfaces
- **Easier Testing**: Features can be tested in isolation
- **Team Autonomy**: Different teams can work on different features without conflicts

## Project Structure

The application is organized into vertical slices, each representing a business capability:

```
src/
├── Api/                   # Main API gateway and shared infrastructure
├── Catalog/               # Product catalog management
├── Inventory/             # Inventory and stock management
├── AppHost/               # Application host and configuration
├── ServiceDefaults/       # Shared service configuration
└── SharedKernel/          # Shared domain models and interfaces
```

### Modules

#### Catalog Module
Manages the product catalog with features for creating, retrieving, and listing products.

- **Create Product**: Add new products with unique slugs and attributes
- **Get Product**: Retrieve products by ID or slug
- **List Products**: Paginated product listing

#### Inventory Module
Manages product availability and stock levels.

- **Restock Items**: Increase product quantities in stock
- **Check Quantity**: Query current stock levels
- **Event Consumer**: Automatically registers new products with zero stock

## Architecture Patterns

### Vertical Slices
Each module (Catalog, Inventory) is a complete vertical slice containing:
- API endpoints
- Request/Command handlers
- Domain models
- Repository implementations
- Event publishers/consumers

### Event-Driven Communication
Modules communicate through integration events:
- Catalog module publishes `ProductCreated` events
- Inventory module consumes these events to maintain consistency

### CQRS Pattern
Commands and Queries are separated:
- **Commands**: Modify state (CreateProduct, RestockInventoryItem)
- **Queries**: Read data (GetProduct, ListProducts, CheckQuantityInStock)

### Repository Pattern
Each module has its own repository interface and implementation, ensuring data access is encapsulated within the slice.

## Technology Stack

- **.NET 9**: Latest .NET framework
- **ASP.NET Core**: Web API framework
- **.NET Aspire**: Cloud-native application orchestration
- **Dapper**: Lightweight ORM for data access
- **FluentValidation**: Request validation
- **FluentMigrator**: Database migrations
- **MassTransit**: Message bus for integration events
- **PostgreSQL**: Database (managed by Aspire)

## Getting Started

### Prerequisites
- .NET 9 SDK
- Docker Desktop (running)

### Running the Application

Ensure Docker is running, then run the following command:

```bash
dotnet run --project src/AppHost
```

The Aspire AppHost will provision the necessary local resources for you. 

After the project builds, your terminal should display a link the view the Aspire dashboard:

```
info: Aspire.Hosting.DistributedApplication[0]
      Login to the dashboard at https://localhost:12345/login?t={access_token}
```

Click on this link to view the Aspire dashboard.
