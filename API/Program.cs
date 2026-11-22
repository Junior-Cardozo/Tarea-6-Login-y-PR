using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using MonitorServidores.Data;
using Scalar.AspNetCore;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

// =====================================================
// 1. CONFIGURACIÓN DE POSTGRESQL
// =====================================================

var conexion = builder.Configuration.GetConnectionString("Postgres")
    ?? throw new InvalidOperationException(
        "No se configuró la conexión a PostgreSQL.");

builder.Services.AddDbContext<MonitorDbContext>(options =>
    options.UseNpgsql(conexion)
);

// =====================================================
// 2. CONTROLADORES Y SCALAR
// =====================================================

builder.Services.AddControllers();
builder.Services.AddOpenApi();

// =====================================================
// 3. AUTENTICACIÓN CON TWITCH Y GOOGLE
// =====================================================

builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)

    // -------------------------------------------------
    // AUTENTICACIÓN MEDIANTE COOKIES
    // -------------------------------------------------

    .AddCookie(options =>
    {
        options.Cookie.Name = "MonitorServidoresSesion";
        options.Cookie.HttpOnly = true;

        options.ExpireTimeSpan = TimeSpan.FromHours(2);
        options.SlidingExpiration = true;

        // Rechazar solicitudes sin autenticación
        options.Events.OnRedirectToLogin = context =>
        {
            context.Response.StatusCode = 401;
            return Task.CompletedTask;
        };

        // Rechazar solicitudes sin permisos
        options.Events.OnRedirectToAccessDenied = context =>
        {
            context.Response.StatusCode = 403;
            return Task.CompletedTask;
        };
    })

    // -------------------------------------------------
    // INICIO DE SESIÓN CON TWITCH
    // -------------------------------------------------

    .AddTwitch(options =>
    {
        options.ClientId =
            builder.Configuration["Authentication:Twitch:ClientId"]
            ?? throw new InvalidOperationException(
                "Falta el Client ID de Twitch.");

        options.ClientSecret =
            builder.Configuration["Authentication:Twitch:ClientSecret"]
            ?? throw new InvalidOperationException(
                "Falta el Client Secret de Twitch.");

        options.CallbackPath = "/signin-twitch";

        options.SignInScheme =
            CookieAuthenticationDefaults.AuthenticationScheme;

        options.Scope.Add("user:read:email");

        options.SaveTokens = false;

        // Identificar el proveedor utilizado
        options.Events.OnTicketReceived = context =>
        {
            if (context.Principal?.Identity is ClaimsIdentity identity)
            {
                identity.AddClaim(
                    new Claim("proveedor", "Twitch")
                );
            }

            return Task.CompletedTask;
        };
    })

    // -------------------------------------------------
    // INICIO DE SESIÓN CON GOOGLE
    // -------------------------------------------------

    .AddGoogle("Google", options =>
    {
        options.ClientId =
            builder.Configuration["Authentication:Google:ClientId"]
            ?? throw new InvalidOperationException(
                "Falta el Client ID de Google.");

        options.ClientSecret =
            builder.Configuration["Authentication:Google:ClientSecret"]
            ?? throw new InvalidOperationException(
                "Falta el Client Secret de Google.");

        options.CallbackPath = "/signin-google";

        options.SignInScheme =
            CookieAuthenticationDefaults.AuthenticationScheme;

        // Datos básicos de identidad
        options.Scope.Add("profile");
        options.Scope.Add("email");

        options.SaveTokens = false;

        // Identificar el proveedor utilizado
        options.Events.OnTicketReceived = context =>
        {
            if (context.Principal?.Identity is ClaimsIdentity identity)
            {
                identity.AddClaim(
                    new Claim("proveedor", "Google")
                );
            }

            return Task.CompletedTask;
        };
    });

// =====================================================
// 4. AUTORIZACIÓN
// =====================================================

builder.Services.AddAuthorization();

// =====================================================
// 5. CONSTRUIR APLICACIÓN
// =====================================================

var app = builder.Build();

// =====================================================
// 6. APLICAR MIGRACIONES DE POSTGRESQL
// =====================================================

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider
        .GetRequiredService<MonitorDbContext>();

    db.Database.Migrate();
}

// =====================================================
// 7. DOCUMENTACIÓN SCALAR
// =====================================================

app.MapOpenApi();
app.MapScalarApiReference();

// =====================================================
// 8. MIDDLEWARE DE AUTENTICACIÓN
// =====================================================

app.UseAuthentication();
app.UseAuthorization();

// =====================================================
// 9. PÁGINA PRINCIPAL
// =====================================================

app.MapGet("/", () => Results.Content(
    """
    <!DOCTYPE html>
    <html lang="es">

    <head>
        <meta charset="UTF-8">
        <meta name="viewport" content="width=device-width, initial-scale=1.0">

        <title>Monitoreo de Servidores</title>

        <style>
            body {
                font-family: Arial, sans-serif;
                background-color: #f4f6f9;
                text-align: center;
                padding: 50px;
            }

            .contenedor {
                background: white;
                max-width: 550px;
                margin: auto;
                padding: 30px;
                border-radius: 12px;
                box-shadow: 0 4px 15px rgba(0,0,0,0.1);
            }

            h1 {
                color: #202938;
            }

            .boton {
                display: inline-block;
                padding: 12px 20px;
                margin: 10px;
                border: none;
                border-radius: 6px;
                color: white;
                font-size: 15px;
                cursor: pointer;
                text-decoration: none;
            }

            .twitch {
                background-color: #9146ff;
            }

            .google {
                background-color: #4285f4;
            }

            .logout {
                background-color: #dc3545;
            }

            pre {
                background-color: #eef1f5;
                padding: 20px;
                border-radius: 6px;
                text-align: left;
                overflow-x: auto;
            }
        </style>
    </head>

    <body>

        <div class="contenedor">

            <h1>Monitoreo de Servidores</h1>

            <p>Proyecto ASP.NET Core .NET 10</p>

            <h2>Inicio de sesión</h2>

            <a href="/login/twitch" class="boton twitch">
                Iniciar sesión con Twitch
            </a>

            <a href="/login/google" class="boton google">
                Iniciar sesión con Google
            </a>

            <br>

            <button class="boton logout" onclick="cerrarSesion()">
                Cerrar sesión
            </button>

            <h2>Estado de la sesión</h2>

            <pre id="estado">Comprobando...</pre>

            <p>
                <a href="/scalar/v1">
                    Abrir documentación Scalar
                </a>
            </p>

        </div>

        <script>

            async function consultarSesion() {

                try {

                    const respuesta = await fetch("/auth/usuario");

                    if (respuesta.ok) {

                        const usuario = await respuesta.json();

                        document.getElementById("estado").textContent =
                            JSON.stringify(usuario, null, 2);

                    } else {

                        document.getElementById("estado").textContent =
                            "No has iniciado sesión";
                    }

                } catch (error) {

                    document.getElementById("estado").textContent =
                        "Error al consultar la sesión";
                }
            }

            async function cerrarSesion() {

                await fetch("/logout", {
                    method: "POST"
                });

                window.location.href = "/";
            }

            consultarSesion();
            

        </script>

    </body>

    </html>
    """,
    "text/html; charset=utf-8"
));

// =====================================================
// 10. INICIAR SESIÓN CON TWITCH
// =====================================================

app.MapGet("/login/twitch", () =>
    Results.Challenge(
        new AuthenticationProperties
        {
            RedirectUri = "/"
        },
        new[] { "Twitch" }
    )
);

// =====================================================
// 11. INICIAR SESIÓN CON GOOGLE
// =====================================================

app.MapGet("/login/google", () =>
    Results.Challenge(
        new AuthenticationProperties
        {
            RedirectUri = "/"
        },
        new[] { "Google" }
    )
);

// =====================================================
// 12. CONSULTAR USUARIO AUTENTICADO
// =====================================================

app.MapGet("/auth/usuario", (HttpContext context) =>
{
    var usuario = context.User;

    return Results.Ok(new
    {
        Autenticado = usuario.Identity?.IsAuthenticated ?? false,

        Nombre = usuario.Identity?.Name,

        Proveedor = usuario.FindFirst("proveedor")?.Value
            ?? "Desconocido",

        Email = usuario.FindFirst(ClaimTypes.Email)?.Value
    });
})
.RequireAuthorization();

// =====================================================
// 13. CERRAR SESIÓN
// =====================================================

app.MapPost("/logout", () =>
    Results.SignOut(
        new AuthenticationProperties
        {
            RedirectUri = "/"
        },
        new[]
        {
            CookieAuthenticationDefaults.AuthenticationScheme
        }
    )
);


app.MapControllers();

app.Run();