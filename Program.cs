using Microsoft.EntityFrameworkCore;
using MyTodo.Data;
using MyTodo.Services;
using MyTodo.Services.Middleware;
using MyTodo.Services.Enrichers;
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
        // Dicionário com nosso mapeamento de colunas customizadas (permanece o mesmo)
        var columnWriters = new Dictionary<string, ColumnWriterBase>
        {
            { "user_id", new SinglePropertyColumnWriter("UserId", PropertyWriteMethod.Raw, NpgsqlDbType.Integer) },
            { "action_id", new SinglePropertyColumnWriter("ActionId", PropertyWriteMethod.Raw, NpgsqlDbType.Integer) },
            { "created_at", new TimestampColumnWriter(NpgsqlDbType.TimestampTz) },
            { "description", new LogEventSerializedColumnWriter(NpgsqlDbType.Jsonb) }
        };

        config
            // Lê configurações básicas como MinimumLevel e Enrich do appsettings.json
            .ReadFrom.Configuration(context.Configuration)
            // Adiciona nosso enricher que captura o UserId
            .Enrich.With(services.GetRequiredService<UserEnricher>())

            // --- CONFIGURAÇÃO DOS SINKS ---

            // Escreve TODOS os logs para o Console
            .WriteTo.Console()

            // Cria um "sub-pipeline" com um filtro APENAS para o PostgreSQL
            .WriteTo.Logger(lc => lc
                // O "porteiro" que só deixa passar logs que tenham a propriedade ActionId
                .Filter.ByIncludingOnly("ActionId is not null")
            
                // Os logs que passam pelo filtro são escritos no banco
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
  //  builder.Services.AddTransient<UtcTimestampEnricher>(); 

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