# HotelHub API

HotelHub is a RESTful Web API built with **ASP.NET Core 10** for managing countries, hotels, rooms, and hotel bookings.

The project also includes authentication, authorization, email-based password reset, validation, caching, rate limiting, health checks, and centralized exception handling.

## Features

* Country management
* Hotel management
* Room management
* Booking management
* User registration and login
* JWT-based authentication
* Role-based authorization
* Admin and User roles
* Role management
* Hotel admin management
* Refresh tokens
* Password reset via email
* Change password
* FluentValidation
* AutoMapper
* Entity Framework Core
* SQL Server
* Redis output caching
* API rate limiting
* Global exception handling
* Health checks
* OpenAPI documentation
* Scalar API reference

## Technologies

* **.NET 10**
* **ASP.NET Core 10**
* **Entity Framework Core 10**
* **ASP.NET Core Identity**
* **JWT Bearer Authentication**
* **SQL Server**
* **Redis**
* **FluentValidation**
* **AutoMapper**
* **MailKit**
* **Serilog**
* **OpenAPI**
* **Scalar**

## Project Structure

```text
HotelHub
│
├── HotelHub.API
│   │
│   ├── Controllers
│   ├── Data
│   ├── DTOs
│   ├── Entity
│   ├── Exceptions
│   ├── Models
│   ├── Services
│   ├── Validators
│   │
│   ├── Program.cs
│   └── HotelHub.API.csproj
│
└── README.md
```

## Main API Resources

### Countries

```text
GET    /api/countries
GET    /api/countries/{id}
POST   /api/countries
PUT    /api/countries/{id}
DELETE /api/countries/{id}
```

### Hotels

```text
GET    /api/hotels
GET    /api/hotels/{id}
POST   /api/hotels
PUT    /api/hotels/{id}
DELETE /api/hotels/{id}
```

### Rooms

```text
GET    /api/hotels/{hotelId}/rooms
GET    /api/hotels/{hotelId}/rooms/{roomId}
POST   /api/hotels/{hotelId}/rooms
PUT    /api/hotels/{hotelId}/rooms/{roomId}
DELETE /api/hotels/{hotelId}/rooms/{roomId}
```

### Bookings

```text
GET    /api/hotels/{hotelId}/rooms/{roomId}/bookings
GET    /api/hotels/{hotelId}/rooms/{roomId}/bookings/{bookingId}
POST   /api/hotels/{hotelId}/rooms/{roomId}/bookings
PUT    /api/hotels/{hotelId}/rooms/{roomId}/bookings/{bookingId}
DELETE /api/hotels/{hotelId}/rooms/{roomId}/bookings/{bookingId}
POST   /api/hotels/{hotelId}/rooms/{roomId}/bookings/{bookingId}/approve
```

## Authentication

The API uses **JWT Bearer Authentication**.

Authentication endpoints:

```text
POST /api/auth/register
POST /api/auth/login
POST /api/auth/refresh-token
POST /api/auth/revoke
POST /api/auth/forgot-password
POST /api/auth/reset-password
POST /api/auth/change-password
```

Protected endpoints require a valid JWT access token:

```http
Authorization: Bearer <access-token>
```

## Authorization

The API uses role-based authorization with the following roles:

* `Admin`
* `User`

The application defines the following authorization policies:

* `AdminOnly`
* `UserOnly`
* `AuthenticatedUser`

Administrative operations such as creating, updating, and deleting hotels, rooms, and countries require the `Admin` role.

User booking operations require the `User` role.

## Validation

Request DTOs are validated using **FluentValidation**.

Validation is applied to incoming requests before processing the corresponding service operations.

## Pagination, Filtering, Searching, and Sorting

List endpoints support common query capabilities such as:

* Pagination
* Searching
* Filtering
* Sorting

Example:

```text
GET /api/hotels?pageNumber=1&pageSize=10
```

Query parameters can also be used for searching, filtering, and sorting depending on the resource.

## Caching

The API uses **ASP.NET Core Output Caching** with Redis.

Cached resources include:

* Countries
* Hotels
* Rooms
* Bookings

Cache entries are invalidated after relevant create, update, or delete operations.

## Rate Limiting

The API uses ASP.NET Core Rate Limiting.

Two rate-limit policies are configured:

### General API

```text
100 requests per minute
```

### Authentication

```text
10 requests per minute
```

Authentication endpoints use the stricter authentication rate limit.

## Error Handling

The API uses centralized exception handling through ASP.NET Core's `IExceptionHandler`.

Common exceptions are mapped to appropriate HTTP status codes, including:

* `400 Bad Request`
* `401 Unauthorized`
* `404 Not Found`
* `409 Conflict`
* `500 Internal Server Error`

API responses follow a consistent `ApiResponse<T>` structure.

## Database

The project uses:

* **Entity Framework Core**
* **SQL Server**
* **ASP.NET Core Identity**

Entity configurations are separated into configuration classes and automatically registered using:

```csharp
modelBuilder.ApplyConfigurationsFromAssembly(
    typeof(ApplicationDbContext).Assembly
);
```

## Email

Email functionality is implemented using **MailKit**.

The email service is used for password reset functionality.

SMTP settings are configured through the application's configuration.

## Health Check

A database health check is available at:

```text
GET /health
```

## API Documentation

In the Development environment, the project exposes:

* OpenAPI document
* Scalar API Reference

Scalar provides an interactive interface for exploring and testing the API endpoints.

## Getting Started

### Requirements

Before running the project, make sure you have:

* .NET 10 SDK
* SQL Server
* Redis
* An SMTP server or SMTP testing service

### Configuration

Configure the required settings in your application configuration and User Secrets.

The application requires configuration for:

* SQL Server connection string
* Redis connection string
* JWT settings
* SMTP settings
* Initial admin account

### Database Migrations

Create a migration:

```bash
dotnet ef migrations add MigrationName
```

Apply migrations:

```bash
dotnet ef database update
```

### Run the Application

```bash
dotnet run
```

The API can then be accessed through the configured application URL.

## Initial Admin

On application startup, the project seeds:

* `Admin` role
* `User` role
* Initial administrator account

The administrator account is created from the configured `AdminUser` settings.

## API Response

The API uses a consistent response model:

```json
{
  "success": true,
  "statusCode": 200,
  "message": "Operation completed successfully.",
  "data": {},
  "errors": null,
  "timeStamp": "2026-01-01T00:00:00Z"
}
```

## License

This project is for learning and development purposes.
