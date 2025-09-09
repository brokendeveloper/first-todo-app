using Microsoft.EntityFrameworkCore;
using MyTodo.Data;
using MyTodo.Services;
using MyTodo.Services.Middleware;
using NpgsqlTypes;
using Serilog;
using Serilog.Sinks.PostgreSQL;


AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

Serilog.Debugging.SelfLog.Enable(Console.Error);

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

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
            .Enrich.With(services.GetRequiredService<UserEnricher>())
            .WriteTo.Console()
            .WriteTo.Logger(lc => lc
                .Filter.ByIncludingOnly("ActionId is not null")
                .WriteTo.PostgreSQL(
                    context.Configuration.GetConnectionString("DefaultConnection"),
                    "audit_logs",
                    columnWriters,
                    needAutoCreateTable: false,
                    useCopy: false
                )
            );
    });

    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseNpgsql(connectionString)
    );
    
    builder.Services.AddControllers();
    builder.Services.AddHttpContextAccessor();
    builder.Services.AddScoped<UserContext>();
    builder.Services.AddTransient<UserEnricher>();

    var app = builder.Build();
    
    app.UseMiddleware<UserContextMiddleware>();
    app.UseSerilogRequestLogging();

    app.MapControllers();
    
    Log.Information("Aplicação iniciando...");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "A aplicação falhou ao iniciar.");
}
finally
{
    Log.CloseAndFlush();
}