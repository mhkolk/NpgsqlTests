using Npgsql;
using System.Threading;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace NpgsqlTestProject;

/// <summary>
/// Sample tests demonstrating how to use the PostgreSQL TestContainer.
/// </summary>
[Collection("Postgres Collection")]
public class PostgresContainerTests
{
    private readonly PostgresTestContainerFixture _fixture;

    public PostgresContainerTests(PostgresTestContainerFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Container_ShouldBeRunning_AndConnectionShouldWork()
    {
        // Arrange & Act
        await using var connection = await _fixture.GetConnectionAsync();

        // Assert
        Assert.NotNull(connection);
        Assert.Equal(System.Data.ConnectionState.Open, connection.State);
    }

    [Fact]
    public async Task CanExecuteQuery_AgainstPostgresContainer()
    {
        // Arrange
        await using var connection = await _fixture.GetConnectionAsync();

        // Act
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT version()";
        var version = await command.ExecuteScalarAsync();

        // Assert
        Assert.NotNull(version);
        Assert.Contains("PostgreSQL", version.ToString());
    }

	[Fact]
	public async Task CanCreateTable_AndInsertData_OnNullNumericReturnsEmptyJsonValue()
	{
		await using var connection = await _fixture.GetConnectionAsync();

		await using (var createCommand = connection.CreateCommand())
		{
			createCommand.CommandText = @"
                CREATE TABLE IF NOT EXISTS test_users (
                    id SERIAL PRIMARY KEY,
                    name VARCHAR(100) NOT NULL,
                    email VARCHAR(100) NOT NULL,
                    age NUMERIC,
                    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
                )";
			await createCommand.ExecuteNonQueryAsync();
		}

        await using (var insertCommand = connection.CreateCommand())
        {
            insertCommand.CommandText = @"
                INSERT INTO test_users (name, email) 
                VALUES (@name, @email)";
            insertCommand.Parameters.AddWithValue("name", "John Doe");
            insertCommand.Parameters.AddWithValue("email", "john@example.com");
            await insertCommand.ExecuteNonQueryAsync();
        }

		var rowData = new Dictionary<string, object?>();

		await using (var selectCmd = connection.CreateCommand())
        {
            selectCmd.CommandText = "SELECT * FROM test_users";
			using (var reader = await selectCmd.ExecuteReaderAsync())
			{
				while (await reader.ReadAsync())
				{
					for (int i = 0; i < reader.FieldCount; i++)
					{
						rowData[reader.GetName(i)] = reader.GetValue(i);
					}
				}
			}
		}

		Assert.True(rowData.ContainsKey("age"));
		Assert.Equal(DBNull.Value, rowData["age"]); // this works as expected
		Assert.Null(rowData["age"] as string); // this works as expected

		// debugger will show {} for value of rowData["age"] but this assertion still fails
		// if we return rowData for further processing without checking for DBNull.Value first
		// or doing "as string" cast it causes issues downstream because it will serialize to "{}" instead of "null"
		Assert.Equal("{}", rowData["age"]);
		
        // this assertion also fails because rowData["age"] is actually DBNull.Value
		Assert.Null(rowData["age"]);
    }

	[Fact]
    public void ConnectionString_ShouldBeValid()
    {
        // Arrange & Act
        var connectionString = _fixture.ConnectionString;

        // Assert
        Assert.NotNull(connectionString);
        Assert.Contains("Host=", connectionString);
        Assert.Contains("Database=testdb", connectionString);
        Assert.Contains("Username=testuser", connectionString);
    }
}
