using Microsoft.EntityFrameworkCore;
using MyTodo.Data;
using MyTodo.Services;
using MyTodo.Services.Middleware;
using Serilog;

try
{
    var builder = WebApplication.CreateBuilder(args);

    // Configurar Serilog (assumindo que usará Serilog para logs)
    Log.Logger = new LoggerConfiguration()
        .ReadFrom.Configuration(builder.Configuration)
        .WriteTo.Console()
        .CreateLogger();

    builder.Host.UseSerilog();

    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));

    builder.Services.AddControllers();

    builder.Services.AddHttpContextAccessor();
    builder.Services.AddScoped<UserContext>();

    // Registrar CustomLogger
    builder.Services.AddScoped<ICustomLogger, CustomLogger>();
    

    var app = builder.Build();

    app.UseMiddleware<UserContextMiddleware>();

    app.UseSerilogRequestLogging();

    app.MapControllers();

    app.Run();
}
catch (Exception ex)
{
    Log.Logger = new LoggerConfiguration()
        .WriteTo.Console()
        .CreateLogger();

    Log.Fatal(ex, "A aplicação falhou ao iniciar.");
}
finally
{
    Log.CloseAndFlush();
}