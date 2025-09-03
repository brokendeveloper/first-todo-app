using MyTodo.Data;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, config) => { config.ReadFrom.Configuration(context.Configuration); });

    builder.Services.AddControllers();
    builder.Services.AddDbContext<AppDbContext>();
    var app = builder.Build();
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
