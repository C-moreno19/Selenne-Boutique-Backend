FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
RUN mkdir -p /app/wwwroot/uploads

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["SelenneApi.csproj", "."]
RUN dotnet restore "SelenneApi.csproj"
COPY . .
RUN dotnet publish "SelenneApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_ENVIRONMENT=Production

# Render inyecta la variable PORT (por defecto 10000).
# El CMD en forma de shell permite leer $PORT en tiempo de ejecución.
CMD dotnet SelenneApi.dll --urls "http://+:${PORT:-8080}"
