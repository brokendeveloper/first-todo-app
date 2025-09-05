using MyTodo.Data;
using MyTodo.Services;
using MyTodo.Services.Middleware;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, config) =>
    {
        config
            .ReadFrom.Configuration(context.Configuration)
            .Enrich.With<UserEnricher>();
    });

    builder.Services.AddHttpContextAccessor();
    builder.Services.AddControllers();
    builder.Services.AddDbContext<AppDbContext>();
    builder.Services.AddScoped<UserContext>();
    
    var app = builder.Build();
    app.UseMiddleware<UserContextMiddleware>();
    app.UseSerilogRequestLogging();


    app.MapControllers();

    Log.Information("Application started");
    app.Run();
}
catch (Exception e)
{
    Log.Fatal(e, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
