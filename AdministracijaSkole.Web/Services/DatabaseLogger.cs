namespace AdministracijaSkole.Web.Services;

using AdministracijaSkole.DAL;
using AdministracijaSkole.Model;
using Microsoft.Extensions.Logging;
using System;

public class DatabaseLogger : ILogger
{
    private readonly SchoolManagerDbContext _context;
    private readonly string _categoryName;

    public DatabaseLogger(SchoolManagerDbContext context, string categoryName)
    {
        _context = context;
        _categoryName = categoryName;
    }

    public IDisposable BeginScope<TState>(TState state)
    {
        return null; 
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return true; 
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
    {
        if (formatter != null)
        {
            var logMessage = formatter(state, exception);

            var log = new Log
            {
                Timestamp = DateTime.UtcNow,
                LogLevel = logLevel.ToString(),
                Message = logMessage
            };

            _context.Logs.Add(log);
            _context.SaveChangesAsync(); 
        }
    }
}
