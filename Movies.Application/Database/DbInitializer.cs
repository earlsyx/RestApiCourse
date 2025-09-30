using Dapper;
using Microsoft.Extensions.Logging;
using System.Data;

namespace Movies.Application.Database;

public class DbInitializer
{
    private readonly IDbConnectionFactory _dbConnectionFactory;
    private readonly ILogger<DbInitializer> _logger;

    public DbInitializer(IDbConnectionFactory dbConnectionFactory, ILogger<DbInitializer> logger)
    {
        _dbConnectionFactory = dbConnectionFactory;
        _logger = logger;
    }

    public async Task InitializeAsync()
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync();

        // Create table explicitly in public
        await connection.ExecuteAsync("""
            create table if not exists public.movies (
                id UUID primary key,
                slug TEXT not null,
                title TEXT not null,
                yearofrelease integer not null
            );
        """);

        await connection.ExecuteAsync("""
            create unique index if not exists movies_slug_idx
            on movies
            using btree(slug);
        """);
    }
}
