using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using WordSprint.Infrastructure.Persistence;

namespace WordSprint.Tests.Integration;

public static class SqliteDbContextFactory
{
    public static WordSprintDbContext Create()
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();

        var options = new DbContextOptionsBuilder<WordSprintDbContext>()
            .UseSqlite(connection)
            .Options;

        var context = new WordSprintDbContext(options);

        context.Database.EnsureCreated();

        return context;
    }
}
