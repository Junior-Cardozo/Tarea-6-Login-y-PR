using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.EntityFrameworkCore;
using MonitorServidores.Data;
using Scalar.AspNetCore;
using System.Security.Claims;
using System.Net.Http.Headers;
using System.Text.Json;

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

// ======================================================================
// 3. AUTENTICACIÓN CON TWITCH, GOOGLE, DISCORD, SPOTIFY, DROPBOX Y SLACK
// ======================================================================

var autenticacion = builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "MonitorServidoresSesion";
        options.Cookie.HttpOnly = true;
        options.ExpireTimeSpan = TimeSpan.FromHours(2);
        options.SlidingExpiration = true;

        options.Events.OnRedirectToLogin = context =>
        {
            context.Response.StatusCode = 401;
            return Task.CompletedTask;
        };

        options.Events.OnRedirectToAccessDenied = context =>
        {
            context.Response.StatusCode = 403;
            return Task.CompletedTask;
        };
    });

// =====================================================
// TWITCH
// =====================================================

if (!string.IsNullOrEmpty(builder.Configuration["Authentication:Twitch:ClientId"])
    && !string.IsNullOrEmpty(builder.Configuration["Authentication:Twitch:ClientSecret"]))
{
    autenticacion.AddTwitch(options =>
    {
        options.ClientId = builder.Configuration["Authentication:Twitch:ClientId"]!;
        options.ClientSecret = builder.Configuration["Authentication:Twitch:ClientSecret"]!;
        options.CallbackPath = "/signin-twitch";
        options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.Scope.Add("user:read:email");

        options.Events.OnTicketReceived = context =>
        {
            if (context.Principal?.Identity is ClaimsIdentity identity)
                identity.AddClaim(new Claim("proveedor", "Twitch"));

            return Task.CompletedTask;
        };
    });
}

// =====================================================
// GOOGLE
// =====================================================

if (!string.IsNullOrEmpty(builder.Configuration["Authentication:Google:ClientId"])
    && !string.IsNullOrEmpty(builder.Configuration["Authentication:Google:ClientSecret"]))
{
    autenticacion.AddGoogle("Google", options =>
    {
        options.ClientId = builder.Configuration["Authentication:Google:ClientId"]!;
        options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"]!;
        options.CallbackPath = "/signin-google";
        options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.Scope.Add("profile");
        options.Scope.Add("email");

        options.Events.OnTicketReceived = context =>
        {
            if (context.Principal?.Identity is ClaimsIdentity identity)
                identity.AddClaim(new Claim("proveedor", "Google"));

            return Task.CompletedTask;
        };
    });
}

// =====================================================
// DISCORD
// =====================================================

if (!string.IsNullOrEmpty(builder.Configuration["Authentication:Discord:ClientId"])
    && !string.IsNullOrEmpty(builder.Configuration["Authentication:Discord:ClientSecret"]))
{
    autenticacion.AddDiscord(options =>
    {
        options.ClientId = builder.Configuration["Authentication:Discord:ClientId"]!;
        options.ClientSecret = builder.Configuration["Authentication:Discord:ClientSecret"]!;
        options.CallbackPath = "/signin-discord";
        options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.Scope.Add("identify");
        options.Scope.Add("email");

        options.Events.OnTicketReceived = context =>
        {
            if (context.Principal?.Identity is ClaimsIdentity identity)
                identity.AddClaim(new Claim("proveedor", "Discord"));

            return Task.CompletedTask;
        };
    });
}

// =====================================================
// SPOTIFY
// =====================================================

if (!string.IsNullOrEmpty(builder.Configuration["Authentication:Spotify:ClientId"])
    && !string.IsNullOrEmpty(builder.Configuration["Authentication:Spotify:ClientSecret"]))
{
    autenticacion.AddOAuth("Spotify", options =>
    {
        options.ClientId = builder.Configuration["Authentication:Spotify:ClientId"]!;
        options.ClientSecret = builder.Configuration["Authentication:Spotify:ClientSecret"]!;

        options.CallbackPath = "/signin-spotify";
        options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;

        options.AuthorizationEndpoint = "https://accounts.spotify.com/authorize";
        options.TokenEndpoint = "https://accounts.spotify.com/api/token";
        options.UserInformationEndpoint = "https://api.spotify.com/v1/me";

        options.Scope.Add("user-read-private");
        options.Scope.Add("user-read-email");

        options.ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, "id");
        options.ClaimActions.MapJsonKey(ClaimTypes.Name, "display_name");
        options.ClaimActions.MapJsonKey(ClaimTypes.Email, "email");

        options.Events.OnCreatingTicket = async context =>
        {
            var solicitud = new HttpRequestMessage(
                HttpMethod.Get,
                options.UserInformationEndpoint
            );

            solicitud.Headers.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    context.AccessToken
                );

            var respuesta = await context.Backchannel.SendAsync(solicitud);
            respuesta.EnsureSuccessStatusCode();

            using var usuario = JsonDocument.Parse(
                await respuesta.Content.ReadAsStringAsync()
            );

            context.RunClaimActions(usuario.RootElement);

            if (context.Identity is ClaimsIdentity identity)
                identity.AddClaim(new Claim("proveedor", "Spotify"));
        };
    });
}

// =====================================================
// DROPBOX
// =====================================================

if (!string.IsNullOrEmpty(builder.Configuration["Authentication:Dropbox:ClientId"])
    && !string.IsNullOrEmpty(builder.Configuration["Authentication:Dropbox:ClientSecret"]))
{
    autenticacion.AddOpenIdConnect("Dropbox", options =>
    {
        options.Authority = "https://www.dropbox.com";
        options.ClientId = builder.Configuration["Authentication:Dropbox:ClientId"]!;
        options.ClientSecret = builder.Configuration["Authentication:Dropbox:ClientSecret"]!;

        options.CallbackPath = "/signin-dropbox";
        options.ResponseType = "code";
        options.UsePkce = false;

        // Dropbox no devuelve el claim nonce en su ID Token.
        options.ProtocolValidator.RequireNonce = false;

        options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.SaveTokens = true;

        // Dropbox incluye los datos de identidad en el ID Token.
        options.GetClaimsFromUserInfoEndpoint = false;
        options.Events.OnRedirectToIdentityProvider = context =>
        {
        context.ProtocolMessage.RedirectUri = builder.Configuration["DROPBOX_REDIRECT_URI"]
            ?? "http://localhost:8081/signin-dropbox";

        return Task.CompletedTask;
        };

        options.Scope.Clear();
        options.Scope.Add("openid");
        options.Scope.Add("profile");
        options.Scope.Add("email");

        options.Events.OnTokenValidated = context =>
        {
            if (context.Principal?.Identity is ClaimsIdentity identity)
            {
                identity.AddClaim(new Claim("proveedor", "Dropbox"));

                var nombre = identity.FindFirst(ClaimTypes.Name)?.Value;

                if (string.IsNullOrWhiteSpace(nombre))
                {
                    var nombrePila = identity.FindFirst(ClaimTypes.GivenName)?.Value
                        ?? identity.FindFirst("given_name")?.Value;

                    var apellido = identity.FindFirst(ClaimTypes.Surname)?.Value
                        ?? identity.FindFirst("family_name")?.Value;

                    var nombreCompleto = $"{nombrePila} {apellido}".Trim();

                    if (!string.IsNullOrWhiteSpace(nombreCompleto))
                    {
                        identity.AddClaim(new Claim(ClaimTypes.Name, nombreCompleto));
                    }
                }
            }

            return Task.CompletedTask;
        };
    });
}

// =====================================================
// SLACK
// =====================================================

if (!string.IsNullOrEmpty(builder.Configuration["Authentication:Slack:ClientId"])
    && !string.IsNullOrEmpty(builder.Configuration["Authentication:Slack:ClientSecret"]))
{
    autenticacion.AddOpenIdConnect("Slack", options =>
    {
        options.Authority = "https://slack.com";
        options.ClientId = builder.Configuration["Authentication:Slack:ClientId"]!;
        options.ClientSecret = builder.Configuration["Authentication:Slack:ClientSecret"]!;

        options.CallbackPath = "/signin-slack";
        options.ResponseType = "code";
        options.ResponseMode = "query";
        options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.SaveTokens = true;
        options.GetClaimsFromUserInfoEndpoint = false;

        options.Events.OnRedirectToIdentityProvider = context =>
        {
        context.ProtocolMessage.RedirectUri = builder.Configuration["SLACK_REDIRECT_URI"]
            ?? "https://localhost:8443/signin-slack";

        return Task.CompletedTask;
        };
        options.Scope.Clear();
        options.Scope.Add("openid");
        options.Scope.Add("profile");
        options.Scope.Add("email");

        options.Events.OnTokenValidated = context =>
        {
            if (context.Principal?.Identity is ClaimsIdentity identity)
            {
                identity.AddClaim(new Claim("proveedor", "Slack"));
            }

            return Task.CompletedTask;
        };
    });
}

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

            .discord {
                background-color: #5865F2;
            }

            .spotify {
                background-color: #1DB954;
            }

            .dropbox {
                background-color: #0061FF;
            }

            .slack {
                background-color: #611f69;
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

            <a href="/login/discord" class="boton discord">
                Iniciar sesión con Discord
            </a>

            <a href="/login/spotify" class="boton spotify">
                Iniciar sesión con Spotify
            </a>

            <a href="/login/dropbox" class="boton dropbox">
                Iniciar sesión con Dropbox
            </a>

            <a href="/login/slack" class="boton slack">
                Iniciar sesión con Slack
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
        new AuthenticationProperties { RedirectUri = "/" },
        new[] { "Twitch" }
    )
);

// =====================================================
// 11. INICIAR SESIÓN CON GOOGLE
// =====================================================

app.MapGet("/login/google", () =>
    Results.Challenge(
        new AuthenticationProperties { RedirectUri = "/" },
        new[] { "Google" }
    )
);

// =====================================================
// 12. INICIAR SESIÓN CON DISCORD
// =====================================================

app.MapGet("/login/discord", () =>
    Results.Challenge(
        new AuthenticationProperties { RedirectUri = "/" },
        new[] { "Discord" }
    )
);

// =====================================================
// 13. INICIAR SESIÓN CON SPOTIFY
// =====================================================

app.MapGet("/login/spotify", () =>
    Results.Challenge(
        new AuthenticationProperties { RedirectUri = "/" },
        new[] { "Spotify" }
    )
);

// =====================================================
// INICIAR SESIÓN CON DROPBOX
// =====================================================

app.MapGet("/login/dropbox", () =>
    Results.Challenge(
        new AuthenticationProperties { RedirectUri = "/" },
        new[] { "Dropbox" }
    )
);

// =====================================================
// INICIAR SESIÓN CON SLACK
// =====================================================

app.MapGet("/login/slack", () =>
    Results.Challenge(
        new AuthenticationProperties { RedirectUri = "/" },
        new[] { "Slack" }
    )
);

// =====================================================
// 14. CONSULTAR USUARIO AUTENTICADO
// =====================================================

app.MapGet("/auth/usuario", (HttpContext context) =>
{
    var usuario = context.User;

    return Results.Ok(new
    {
        Autenticado = usuario.Identity?.IsAuthenticated ?? false,
        Nombre = usuario.Identity?.Name,
        Proveedor = usuario.FindFirst("proveedor")?.Value ?? "Desconocido",
        Email = usuario.FindFirst(ClaimTypes.Email)?.Value
    });
})
.RequireAuthorization();

// =====================================================
// 15. CERRAR SESIÓN
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