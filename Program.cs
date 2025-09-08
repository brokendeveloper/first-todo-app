using Microsoft.EntityFrameworkCore;
using MyTodo.Data;
using MyTodo.Services;
using MyTodo.Services.Middleware;
using NpgsqlTypes;
using Serilog;
using Serilog.Sinks.PostgreSQL;

// Habilita o log interno do Serilog para nos ajudar a depurar
Serilog.Debugging.SelfLog.Enable(Console.Error);

// Logger "bootstrap" para capturar erros que acontecem durante a inicialização
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);
    
    // --- Configuração do Serilog ---
    builder.Host.UseSerilog((context, services, config) =>
    {
        // Dicionário que define o mapeamento para nossa tabela customizada
        var columnWriters = new Dictionary<string, ColumnWriterBase>
        {
            { "user_id", new SinglePropertyColumnWriter("UserId", PropertyWriteMethod.Raw, NpgsqlDbType.Integer) },
            { "action_id", new SinglePropertyColumnWriter("ActionId", PropertyWriteMethod.Raw, NpgsqlDbType.Integer) },
            { "created_at", new TimestampColumnWriter(NpgsqlDbType.TimestampTz) },
            { "description", new LogEventSerializedColumnWriter(NpgsqlDbType.Jsonb) }
        };

        config
            // Lê configurações básicas (MinimumLevel, Enrich) do appsettings.json
            .ReadFrom.Configuration(context.Configuration)
            // Adiciona nosso enricher que captura o UserId
            .Enrich.With(services.GetRequiredService<UserEnricher>())
            // Configura os Sinks (destinos) diretamente no código
            .WriteTo.Console() // Continuamos logando no console para facilitar o debug
            .WriteTo.PostgreSQL(
                context.Configuration.GetConnectionString("DefaultConnection"),
                "audit_logs", // Nome da nossa tabela gerenciada pelo EF Core
                columnWriters,
                needAutoCreateTable: false, // Muito importante: EF Core gerencia a tabela
                useCopy: false
            );
    });

    // --- Configuração dos Serviços da Aplicação ---
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseNpgsql(connectionString)
    );
    
    builder.Services.AddControllers();
    builder.Services.AddHttpContextAccessor();
    builder.Services.AddScoped<UserContext>();
    builder.Services.AddTransient<UserEnricher>();

    var app = builder.Build();

    // --- Configuração do Pipeline de Middlewares ---
    app.UseMiddleware<UserContextMiddleware>();
    // Este middleware de log de requisição continuará logando APENAS no Console,
    // pois não tem a propriedade "ActionId" para ser salvo no banco.
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