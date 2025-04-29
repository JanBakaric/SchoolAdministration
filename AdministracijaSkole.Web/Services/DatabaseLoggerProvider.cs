namespace AdministracijaSkole.Web.Services;

using AdministracijaSkole.DAL;
using AdministracijaSkole.Model;
using AdministracijaSkole.Web.Models;
using Microsoft.Extensions.Logging;

using System;

public class DatabaseLoggerProvider : ILoggerProvider
{
    private readonly IServiceProvider _serviceProvider;

    public DatabaseLoggerProvider(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider; // This is used to resolve scoped services
    }

    public ILogger CreateLogger(string categoryName)
    {
        // Use IServiceProvider to get the scoped ApplicationDbContext
        var dbContext = _serviceProvider.GetRequiredService<SchoolManagerDbContext>();
        return new DatabaseLogger(dbContext, categoryName);
    }

    public void Dispose()
    {
        // Clean up any resources here if necessary (but not needed for this example)
    }
}
