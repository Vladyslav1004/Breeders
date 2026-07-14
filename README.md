# Breeders API

This is a small ASP.NET Core Web API created as a test assignment.

The API represents a simplified part of a platform for dog breeders. A breeder can view their litters and publish an approved litter if they still have free publications available.

I kept the project intentionally small, but separated HTTP handling, business logic, database access and error handling so that the code remains easy to test and extend.

## Tech stack

- .NET 8
- ASP.NET Core Web API
- Entity Framework Core
- EF Core InMemory provider
- xUnit
- Swagger / OpenAPI

## Main endpoints

### Get breeder litters

```http
GET /api/litters
```

The breeder is identified using the request header:

```http
X-Breeder-Id: 11111111-1111-1111-1111-111111111111
```

Optional query parameters:

```text
status
pageNumber
pageSize
```

Example:

```http
GET /api/litters?status=Approved&pageNumber=1&pageSize=10
```

### Publish a litter

```http
POST /api/litters/{litterId}/publish
```

A litter can be published only when:

- it exists;
- it belongs to the current breeder;
- its status is `Approved`;
- the breeder still has a free publication available.

When publication succeeds, the API:

1. increments `UsedCount`;
2. changes the litter status to `Published`;
3. creates an audit log with the action `Published for free`;
4. calls the notification service.

If the free limit has already been used, the litter remains unchanged and the failed attempt is written to the audit log.

## Running the project

Requirements:

- .NET 8 SDK

Restore packages:

```bash
dotnet restore
```

Run the API:

```bash
dotnet run --project Breeders.Api
```

Swagger is opened automatically when the application is started with the development HTTPS profile.

## Development seed data

The project supports two seed modes.

Open:

```text
Breeders.Api/appsettings.Development.json
```

### Fixed mode

```json
{
  "SeedData": {
    "Mode": "Fixed"
  }
}
```

This mode uses predictable IDs and is convenient for Swagger checks and integration tests.

Primary breeder:

```text
11111111-1111-1111-1111-111111111111
```

Approved litter:

```text
aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa
```

Second breeder with an exceeded limit:

```text
22222222-2222-2222-2222-222222222222
```

Their approved litter:

```text
dddddddd-dddd-dddd-dddd-dddddddddddd
```

### Generated mode

```json
{
  "SeedData": {
    "Mode": "Generated"
  }
}
```

In this mode, new GUIDs are generated each time the application starts.

The generated breeder and litter IDs are written to the application console, so they can be copied into Swagger.

Because the application uses an InMemory database, all data are recreated after a restart.

## Error handling

Business errors are represented by custom exceptions such as:

- `NotFoundException`
- `ForbiddenException`
- `ValidationException`
- `ConflictException`
- `UnauthorizedException`

They inherit from `DomainException`.

A global middleware catches these exceptions and returns a consistent response:

```json
{
  "statusCode": 404,
  "errorCode": "not_found",
  "message": "Litter was not found.",
  "traceId": "..."
}
```

Controllers do not contain business logic and do not manually create error responses.

## Tests

The solution contains both unit and integration tests.

Unit tests cover:

- successful publication;
- ownership validation;
- litter status validation;
- missing breeder benefits;
- exceeded free limits;
- audit log creation;
- filtering;
- sorting;
- pagination;
- seed data generation.

Integration tests cover:

- HTTP headers;
- endpoint responses;
- status codes;
- global exception middleware;
- JSON serialization.

Run all tests:

```bash
dotnet test
```

## Project structure

```text
Breeders.Api
├── Controllers
├── Data
├── Enums
├── Exceptions
├── Extensions
├── Middleware
├── Models
│   └── DTOs
├── Services
│   └── Interfaces
└── Program.cs

Breeders.Tests
├── Controllers
├── Data
├── Extensions
├── Helpers
└── Services
```

## InMemory database note

The assignment allows either an InMemory database or SQLite, so this implementation uses EF Core InMemory.

The related publication changes are prepared and saved using one `SaveChangesAsync` call. However, the InMemory provider does not support real relational database transactions.

With a relational provider, I would use an explicit transaction for:

- changing the litter status;
- incrementing `UsedCount`;
- adding the audit log.

For a production version I would also consider a relational database, real authentication instead of `X-Breeder-Id`, and an outbox-based notification flow.