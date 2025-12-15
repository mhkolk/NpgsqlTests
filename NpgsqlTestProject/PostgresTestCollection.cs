namespace NpgsqlTestProject;

/// <summary>
/// Test collection definition for PostgreSQL TestContainer.
/// All test classes using this collection will share the same container instance.
/// </summary>
[CollectionDefinition("Postgres Collection")]
public class PostgresTestCollection : ICollectionFixture<PostgresTestContainerFixture>
{
    // This class is intentionally empty.
    // It's used only to define the collection and attach the fixture.
}
