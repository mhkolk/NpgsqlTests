using Npgsql;
using Testcontainers.PostgreSql;

namespace NpgsqlTestProject;

/// <summary>
/// Fixture for managing a PostgreSQL TestContainer instance.
/// Implements IAsyncLifetime to start the container before tests and dispose after.
/// </summary>
public class PostgresTestContainerFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgresContainer;

    public PostgresTestContainerFixture()
    {
        _postgresContainer = new PostgreSqlBuilder()
            .WithImage("postgres:16-alpine")
            .WithDatabase("testdb")
            .WithUsername("testuser")
            .WithPassword("testpass")
            .WithCleanUp(true)
            .Build();
    }

    /// <summary>
    /// Gets the connection string for the PostgreSQL container.
    /// </summary>
    public string ConnectionString => _postgresContainer.GetConnectionString();

    /// <summary>
    /// Initializes the container by starting it.
    /// Called once before any tests in the collection run.
    /// </summary>
    public async Task InitializeAsync()
    {
        await _postgresContainer.StartAsync();
    }

    /// <summary>
    /// Disposes the container after all tests have completed.
    /// </summary>
    public async Task DisposeAsync()
    {
        await _postgresContainer.DisposeAsync();
    }

    /// <summary>
    /// Creates and returns an open NpgsqlConnection to the test database.
    /// </summary>
    public async Task<NpgsqlConnection> GetConnectionAsync()
    {
        var connection = new NpgsqlConnection(ConnectionString);
        await connection.OpenAsync();
        return connection;
    }
}
