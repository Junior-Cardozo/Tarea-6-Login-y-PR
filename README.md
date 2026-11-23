# Tarea-6-Login-y-PR — Monitor de Servidores

API en ASP.NET Core (.NET) con PostgreSQL para monitorear servidores, registrar incidentes y gestionar alertas. Incluye autenticación con proveedores externos (OAuth) y endpoints protegidos con `[Authorize]`.

## Equipo y reparto de responsabilidades

### Reparto de tablas

| Integrante | Módulo | Tablas / entidades | Controladores |
|---|---|---|---|
| **Junior** | Incidentes | `Incidentes`, `TiposIncidente`, `PrioridadesIncidente`, `EstadosIncidente` | `IncidentesController`, `TiposIncidenteController`, `PrioridadesIncidenteController`, `EstadosIncidenteController` |
| **Yanely** | Servidores | `servidores`, `tipos_servidor`, `sistemas_operativos`, `monitoreos` | `ServidoresController`, `TiposServidorController`, `SistemasOperativosController`, `MonitoreosController` |
| **Diego** | Alertas | `alertas`, `tipos_alerta`, `notificaciones`, `historial_alertas` | `AlertasController`, `TiposAlertaController`, `NotificacionesController`, `HistorialAlertasController` |

Relaciones entre módulos: `Incidentes` y `Alertas` dependen de `Servidores` (clave foránea `ServidorId`).

### Reparto de proveedores de autenticación

La API tiene configurados seis proveedores. Cada proveedor se activa solo si sus credenciales están definidas en el `.env`.

| Proveedor | Ruta de login | Callback | Responsable |
|---|---|---|---|
| Twitch | `/login/twitch` | `/signin-twitch` | _Por completar_ |
| Google | `/login/google` | `/signin-google` | _Por completar_ |
| Discord | `/login/discord` | `/signin-discord` | _Por completar_ |
| Spotify | `/login/spotify` | `/signin-spotify` | _Por completar_ |
| Dropbox | `/login/dropbox` | `/signin-dropbox` | _Por completar_ |
| Slack | `/login/slack` | `/signin-slack` | _Por completar_ |

## Forma de trabajo

- Todos trabajan desde su **propio fork**; nadie es colaborador del repositorio base.
- Las contribuciones llegan mediante **Pull Request** hacia `main`, revisada por el líder u otro integrante.
- Detalles de ramas, commits y revisión: ver [CONTRIBUTING.md](CONTRIBUTING.md).

## Requisitos

- Docker y Docker Compose
- Credenciales OAuth de los proveedores que se vayan a usar

## Puesta en marcha

1. Copiar `.env.example` a `.env` y completar los valores (no subir `.env` al repositorio).
2. Levantar los servicios:

   ```bash
   docker compose -f despliegue.yml up --build
   ```

3. La API queda disponible en `http://localhost:8080` (puerto configurable con `API_PORT`) y PostgreSQL en `127.0.0.1:5433`.

## Autenticación

- Sesión por cookie (`MonitorServidoresSesion`, 2 horas con renovación).
- Inicio de sesión: `GET /login/{proveedor}`.
- Usuario actual: `GET /auth/usuario`.
- Cierre de sesión: `POST /logout`.
- Todos los controladores de la API requieren sesión iniciada; sin sesión responden `401`.

## Estructura del proyecto

```
API/
├── Controllers/   # Endpoints por módulo
├── Data/          # MonitorDbContext
├── Migrations/    # Migraciones de EF Core
├── Models/        # Entidades
└── Program.cs     # Configuración, autenticación y rutas de login
```
