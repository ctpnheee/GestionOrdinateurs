# GestionOrdinateurs

GestionOrdinateurs is a .NET 10 application for managing a computer fleet. It provides a Blazor WebAssembly interface, a secured REST API, repair tracking, and role-based access through Keycloak.

## Features

- View computers and their availability, price, specifications, and status
- Add, update, and delete computers as an administrator
- Search and sort the inventory and display fleet statistics
- Run computer diagnostics and repair simulations
- View repair history and SSD stock
- Authenticate with Keycloak using JWT access tokens
- Restrict sensitive operations to users with the `Admin` role

## Architecture

| Project / service | Purpose | Local URL |
|---|---|---|
| `GestionOrdinateurs.Client` | Blazor WebAssembly UI served by Nginx | <http://localhost:5000> |
| `GestionOrdinateurs.Gateway` | Secured YARP reverse proxy for business API requests | <http://localhost:5010> |
| `GestionOrdinateurs.API` | ASP.NET Core business API backed by SQLite | <http://localhost:5086> |
| `AuthentificationApi` | Exchanges credentials with Keycloak for OIDC tokens | <http://localhost:5292> |
| `GestionOrdinateurs.Shared` | Models shared by the client and APIs | — |
| `GestionOrdinateurs.Tests` | xUnit unit tests | — |
| Keycloak | Identity provider and role management | <http://localhost:8180> |
| PostgreSQL | Stores Keycloak configuration and users | `localhost:5432` |

Requests from the browser are authenticated by `AuthentificationApi`. Business requests then pass through the gateway, which validates the Keycloak token before forwarding them to `GestionOrdinateurs.API`.

## Technology stack

- .NET 10 and ASP.NET Core
- Blazor WebAssembly
- Entity Framework Core with SQLite
- YARP reverse proxy
- Keycloak and OpenID Connect
- PostgreSQL for Keycloak persistence
- xUnit, FluentAssertions, and EF Core InMemory
- Docker Compose and Nginx

## Prerequisites

For the recommended Docker setup:

- Docker with Docker Compose

To build and test the solution locally:

- .NET 10 SDK

## Getting started with Docker

1. Optionally create a local environment file if you want the built images to use your Docker Hub account name:

   ```bash
   cp .env.example .env
   ```

   Then replace `your-docker-hub-username` in `.env`. If this step is skipped, Docker Compose uses `gestionordinateurs` as the image namespace.

2. Build and start all services:

   ```bash
   docker compose up -d --build
   ```

3. Check that the containers are running:

   ```bash
   docker compose ps
   ```

4. Open <http://localhost:5000> and sign in with one of the test accounts below.

The Keycloak realm, client, roles, and test users are imported automatically from `keycloak/gestion-ordinateurs-realm.json` on the first startup.

## Test accounts

| Username | Password | Role | Permissions |
|---|---|---|---|
| `user-admin` | `123` | `Admin` | Read and manage computers; run administrator repair operations |
| `user-general` | `123` | `Gestionnaire` | Read computers and repair history |

The Keycloak administration console at <http://localhost:8180> uses `admin` / `admin` in this development environment.

> [!WARNING]
> These credentials and the configured client secret are intended for local development only. Change them before deploying the application to any shared or production environment.

## Build and test

From the repository root, run:

```bash
dotnet restore
dotnet build --no-restore
dotnet test GestionOrdinateurs.Tests/GestionOrdinateurs.Tests.csproj --no-build
```

When the Docker environment is running, Swagger UI is available for the business API at <http://localhost:5086/swagger> and for the authentication API at <http://localhost:5292/swagger>.

## Data storage

- Business data is stored in `GestionOrdinateurs.API/parcinfo.db` using SQLite. EF Core applies migrations when the API starts.
- Keycloak data is stored in PostgreSQL and persisted in the `keycloak_postgres_data` Docker volume.

## Stopping the application

Stop and remove the containers:

```bash
docker compose down
```

To also delete the PostgreSQL volume and reset Keycloak data:

```bash
docker compose down -v
```

Use the second command carefully because the Keycloak data in the Docker volume cannot be recovered afterward.

## Repository structure

```text
.
├── AuthentificationApi/          # Authentication API
├── GestionOrdinateurs.API/       # Business API, migrations, and SQLite database
├── GestionOrdinateurs.Client/    # Blazor WebAssembly client
├── GestionOrdinateurs.Gateway/   # YARP API gateway
├── GestionOrdinateurs.Shared/    # Shared domain models
├── GestionOrdinateurs.Tests/     # Unit tests
├── keycloak/                     # Keycloak realm configuration
├── docker-compose.yml
└── GestionOrdinateurs.slnx
```
