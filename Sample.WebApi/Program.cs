using Cleipnir.ResilientFunctions.AspNetCore.Core;
using Cleipnir.ResilientFunctions.AspNetCore.Postgres;
using Sample.WebApi.Ordering;
using Serilog;
using Serilog.Events;

namespace Sample.WebApi;

internal static class Program
{
    private static async Task Main(string[] args)
    {
        var port = args.Any() ? int.Parse(args[0]) : 5000;
        
        await Database.CreateIfNotExists();
        //await Database.RecreateDatabase();
        var builder = WebApplication.CreateBuilder(args);

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .CreateLogger();
        
        // Add order processing dependencies
        builder.Services.AddOrderProcessingBindings();
        
        //add Resilient Functions dependencies
        builder.Services.UseResilientFunctions( 
            Database.ConnectionString,
            _ => new Options(
                unhandledExceptionHandler: rfe => Log.Logger.Error(rfe, "ResilientFrameworkException occured"),
                crashedCheckFrequency: TimeSpan.FromSeconds(1)
            )
        );
        
        builder.Host.UseSerilog();
        builder.Services.AddControllers();

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        var app = builder.Build();
        
        app.UseSwagger();
        app.UseSwaggerUI();
        
        app.MapControllers();

        await app.RunAsync($"http://localhost:{port}");
    }
}