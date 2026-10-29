FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["AuthentificationApi/AuthentificationApi.csproj", "AuthentificationApi/"]
COPY ["GestionOrdinateurs.Shared/GestionOrdinateurs.Shared.csproj", "GestionOrdinateurs.Shared/"]
RUN dotnet restore "AuthentificationApi/AuthentificationApi.csproj"

COPY . .
WORKDIR /src/AuthentificationApi
RUN dotnet publish "AuthentificationApi.csproj" -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
ENV ASPNETCORE_ENVIRONMENT=Development
ENV ASPNETCORE_HTTP_PORTS=5292
EXPOSE 5292
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "AuthentificationApi.dll"]
