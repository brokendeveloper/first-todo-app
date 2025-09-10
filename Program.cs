using Microsoft.EntityFrameworkCore;
using MyTodo.Data;
using MyTodo.Services;
using MyTodo.Services.Enrichers;
using MyTodo.Services.Middleware;
using NpgsqlTypes;
using Serilog;
using Serilog.Context;
using Serilog.Sinks.PostgreSQL;

Serilog.Debugging.SelfLog.Enable(Console.Error);

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, services, config) =>
    {
        // ✅ Configuração otimizada com tipos corretos
        var columnWriters = new Dictionary<string, ColumnWriterBase>
        {
            // IDs como integer com conversão garantida
            { "user_id", new SinglePropertyColumnWriter("UserId", PropertyWriteMethod.Raw, NpgsqlDbType.Integer) },
            { "action_id", new SinglePropertyColumnWriter("ActionId", PropertyWriteMethod.Raw, NpgsqlDbType.Integer) },
            
            // Timestamp otimizado
            { "created_at", new TimestampColumnWriter(NpgsqlDbType.Timestamp) },
            
            // Informações da requisição (otimizado)
            { "request_path", new SinglePropertyColumnWriter("RequestPath", PropertyWriteMethod.ToString, NpgsqlDbType.Varchar, 500) },
            { "http_method", new SinglePropertyColumnWriter("HttpMethod", PropertyWriteMethod.ToString, NpgsqlDbType.Varchar, 10) },
            { "ip_address", new SinglePropertyColumnWriter("IpAddress", PropertyWriteMethod.ToString, NpgsqlDbType.Varchar, 45) },
            
            // Level e mensagem
            { "level", new LevelColumnWriter(NpgsqlDbType.Varchar, 20) },
            { "message", new RenderedMessageColumnWriter(NpgsqlDbType.Text) },
            
            // Dados extras como JSONB (para queries eficientes)
            { "data", new LogEventSerializedColumnWriter(NpgsqlDbType.Jsonb) }
        };

        config
            .ReadFrom.Configuration(context.Configuration)
            .Enrich.With(services.GetRequiredService<UserEnricher>())
            .Enrich.With(services.GetRequiredService<ActionEnricher>()) // ✅ Novo enricher
            .Enrich.With(services.GetRequiredService<RequestEnricher>()) // ✅ Captura dados da requisição
            .WriteTo.Console(outputTemplate: 
                "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} | User:{UserId} Action:{ActionId} Path:{RequestPath} {NewLine}{Exception}")
            .WriteTo.Logger(lc => lc
                .Filter.ByIncludingOnly("ActionId is not null") // Só logs de auditoria
                .WriteTo.PostgreSQL(
                    context.Configuration.GetConnectionString("DefaultConnection"),
                    "custom_audit_logs", // ✅ Nome mais descritivo
                    columnWriters,
                    needAutoCreateTable: true,
                    useCopy: false,
                    respectCase: false
                )
            );
    });

    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));
    
    builder.Services.AddControllers();
    builder.Services.AddHttpContextAccessor();
    builder.Services.AddScoped<UserContext>();
    
    // ✅ Registrar todos os enrichers
    builder.Services.AddTransient<UserEnricher>();
    builder.Services.AddTransient<ActionEnricher>();
    builder.Services.AddTransient<RequestEnricher>();

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