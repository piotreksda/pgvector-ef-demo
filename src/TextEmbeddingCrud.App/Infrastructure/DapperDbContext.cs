using System.Data;
using Dapper;
using Npgsql;
using Pgvector.Dapper;

namespace TextEmbeddingCrud.App.Infrastructure;

public class DapperDbContext : IDisposable
{
    private readonly IDbConnection _connection;

    public DapperDbContext(IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        SqlMapper.AddTypeHandler(new VectorTypeHandler());

        var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
        dataSourceBuilder.UseVector();
        using var dataSource = dataSourceBuilder.Build();

        var conn = dataSource.OpenConnection();

        _connection = conn;
    }

    public IDbConnection Connection => _connection;

    public void Dispose()
    {
        _connection?.Dispose();
    }
}