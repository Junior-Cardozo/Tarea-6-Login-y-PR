using Microsoft.EntityFrameworkCore;
using MonitorServidores.Data;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

var conexion = builder.Configuration.GetConnectionString("Postgres")
    ?? throw new InvalidOperationException(
        "No se configuró la conexión a PostgreSQL."
    );

builder.Services.AddDbContext<MonitorDbContext>(options =>
    options.UseNpgsql(conexion)
);

builder.Services.AddControllers();

builder.Services.AddOpenApi();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<MonitorDbContext>();
    db.Database.Migrate();
}

app.MapOpenApi();
app.MapScalarApiReference();

app.UseAuthorization();

app.MapControllers();

app.Run();
