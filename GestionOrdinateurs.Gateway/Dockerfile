FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["GestionOrdinateurs.Gateway/GestionOrdinateurs.Gateway.csproj", "GestionOrdinateurs.Gateway/"]
RUN dotnet restore "GestionOrdinateurs.Gateway/GestionOrdinateurs.Gateway.csproj"

COPY . .
WORKDIR /src/GestionOrdinateurs.Gateway
RUN dotnet publish "GestionOrdinateurs.Gateway.csproj" -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
ENV ASPNETCORE_ENVIRONMENT=Development
ENV ASPNETCORE_HTTP_PORTS=5010
EXPOSE 5010
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "GestionOrdinateurs.Gateway.dll"]
