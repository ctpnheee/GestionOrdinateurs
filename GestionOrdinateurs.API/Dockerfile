FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["GestionOrdinateurs.API/GestionOrdinateurs.API.csproj", "GestionOrdinateurs.API/"]
COPY ["GestionOrdinateurs.Shared/GestionOrdinateurs.Shared.csproj", "GestionOrdinateurs.Shared/"]
RUN dotnet restore "GestionOrdinateurs.API/GestionOrdinateurs.API.csproj"

COPY . .
WORKDIR /src/GestionOrdinateurs.API
RUN dotnet publish "GestionOrdinateurs.API.csproj" -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
ENV ASPNETCORE_ENVIRONMENT=Development
ENV ASPNETCORE_HTTP_PORTS=5086
EXPOSE 5086
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "GestionOrdinateurs.API.dll"]
