using Microsoft.EntityFrameworkCore;
using MyTodo.Data;
using MyTodo.Services;
using MyTodo.Services.Middleware;




try
{
    var builder = WebApplication.CreateBuilder(args);
    

    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));
    
    builder.Services.AddControllers();
    builder.Services.AddHttpContextAccessor();
    builder.Services.AddScoped<UserContext>();
    
    // Registrar todos os enrichers
    builder.Services.AddTransient<UserEnricher>();
    builder.Services.AddTransient<ActionEnricher>();
    builder.Services.AddTransient<RequestEnricher>();
    builder.Services.AddTransient<DescriptionEnricher>();

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