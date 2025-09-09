using Microsoft.EntityFrameworkCore;
using MyTodo.Data;
using MyTodo.Services;
using MyTodo.Services.Enrichers;
using MyTodo.Services.Middleware;
using NpgsqlTypes;
using Serilog;
using Serilog.Sinks.PostgreSQL;

Serilog.Debugging.SelfLog.Enable(Console.Error);

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, services, config) =>
    {
        var columnWriters = new Dictionary<string, ColumnWriterBase>
        {
            { "user_id", new SinglePropertyColumnWriter("UserId", PropertyWriteMethod.Raw, NpgsqlDbType.Integer) },
            { "action_id", new SinglePropertyColumnWriter("ActionId", PropertyWriteMethod.Raw, NpgsqlDbType.Integer) },
            { "created_at", new TimestampColumnWriter(NpgsqlDbType.TimestampTz) },
            { "description", new LogEventSerializedColumnWriter(NpgsqlDbType.Jsonb) }
        };
        
        config
            
            .ReadFrom.Configuration(context.Configuration)
            .ReadFrom.Services(services)
            .WriteTo.Console()
            .WriteTo.PostgreSQL(
                context.Configuration.GetConnectionString("DefaultConnection"),
                "audit_logs",
                columnWriters,
                needAutoCreateTable: false
            );
    });

    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));
    
    builder.Services.AddControllers();
    builder.Services.AddHttpContextAccessor();
    builder.Services.AddScoped<UserContext>();
    builder.Services.AddTransient<UserEnricher>();

    var app = builder.Build();
    
    app.UseMiddleware<UserContextMiddleware>();
    app.UseSerilogRequestLogging();
    app.MapControllers();
    
    app.Run();
}
catch (Exception ex)
{
    Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateLogger();
    Log.Fatal(ex, "A aplicação falhou ao iniciar.");
}
finally
{
    Log.CloseAndFlush();
}