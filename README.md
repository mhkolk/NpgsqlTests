# TestContainers Setup for NpgsqlTestProject

This project uses **Testcontainers** to provide PostgreSQL database instances for integration testing.

## Overview

Testcontainers is a library that provides lightweight, throwaway instances of databases and other services that can run in Docker containers. This ensures that tests run in isolated, reproducible environments.

## Components

### 1. PostgresTestContainerFixture
Located in `PostgresTestContainerFixture.cs`, this fixture manages the lifecycle of a PostgreSQL container:
- **Container Configuration**: Uses `postgres:16-alpine` image
- **Database**: `testdb`
- **Credentials**: `testuser` / `testpass`
- **Lifecycle**: Implements `IAsyncLifetime` for proper startup and cleanup

### 2. PostgresTestCollection
Located in `PostgresTestCollection.cs`, this collection definition allows multiple test classes to share the same container instance, improving test performance.

### 3. Sample Tests
Located in `PostgresContainerTests.cs`, demonstrating:
- Basic connection validation
- Query execution
- Table creation and data manipulation

## Usage

### Basic Test Class

```csharp
[Collection("Postgres Collection")]
public class MyDatabaseTests
{
    private readonly PostgresTestContainerFixture _fixture;

    public MyDatabaseTests(PostgresTestContainerFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task MyTest()
    {
        // Get a connection
        await using var connection = await _fixture.GetConnectionAsync();
        
        // Use the connection string
        var connectionString = _fixture.ConnectionString;
        
        // Your test logic here
    }
}
```

### With Entity Framework Core

```csharp
[Collection("Postgres Collection")]
public class EfCoreTests
{
    private readonly PostgresTestContainerFixture _fixture;

    public EfCoreTests(PostgresTestContainerFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task CanUseWithEfCore()
    {
        var options = new DbContextOptionsBuilder<MyDbContext>()
            .UseNpgsql(_fixture.ConnectionString)
            .Options;

        await using var context = new MyDbContext(options);
        await context.Database.EnsureCreatedAsync();
        
        // Your test logic here
    }
}
```

## Prerequisites

- **Docker**: Must be installed and running on your machine
- **Docker Desktop** (Windows/Mac) or **Docker Engine** (Linux)

## Configuration Options

You can customize the PostgreSQL container in `PostgresTestContainerFixture.cs`:

```csharp
_postgresContainer = new PostgreSqlBuilder()
    .WithImage("postgres:16-alpine")        // Change PostgreSQL version
    .WithDatabase("testdb")                 // Change database name
    .WithUsername("testuser")               // Change username
    .WithPassword("testpass")               // Change password
    .WithCleanUp(true)                      // Auto-cleanup after tests
    .Build();
```

## Running Tests

```bash
# Run all tests
dotnet test

# Run tests in a specific class
dotnet test --filter "FullyQualifiedName~PostgresContainerTests"

# Run a specific test
dotnet test --filter "Name=Container_ShouldBeRunning_AndConnectionShouldWork"
```

## Troubleshooting

### Docker Not Running
If you get errors about Docker not being available:
1. Ensure Docker Desktop is running (Windows/Mac)
2. Check Docker service status: `docker ps`

### Container Startup Timeout
If containers take too long to start:
1. Check your Docker resources (CPU, Memory)
2. Pull the image manually: `docker pull postgres:16-alpine`

### Port Conflicts
Testcontainers automatically assigns random ports, so port conflicts should be rare. The container port is exposed through the connection string.

## Benefits

- ? **Isolated Tests**: Each test run gets a fresh database
- ? **Reproducible**: Same environment every time
- ? **Fast**: Containers start quickly
- ? **No External Dependencies**: No need for a persistent database server
- ? **Clean State**: Automatic cleanup after tests
