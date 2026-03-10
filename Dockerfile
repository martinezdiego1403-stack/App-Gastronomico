# ==============================
# Etapa 1: Compilar la aplicacion
# ==============================
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copiar archivos de proyecto primero (para cachear dependencias)
COPY SandwicheriaWalterio.Api/SandwicheriaWalterio.Api.csproj SandwicheriaWalterio.Api/
COPY SandwicheriaWalterio.Shared/SandwicheriaWalterio.Shared.csproj SandwicheriaWalterio.Shared/

# Restaurar dependencias (NuGet packages)
RUN dotnet restore SandwicheriaWalterio.Api/SandwicheriaWalterio.Api.csproj

# Copiar todo el codigo fuente
COPY SandwicheriaWalterio.Api/ SandwicheriaWalterio.Api/
COPY SandwicheriaWalterio.Shared/ SandwicheriaWalterio.Shared/

# Compilar en modo Release
RUN dotnet publish SandwicheriaWalterio.Api/SandwicheriaWalterio.Api.csproj -c Release -o /app/out

# ==============================
# Etapa 2: Imagen final (liviana)
# ==============================
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app

# Copiar solo los archivos compilados
COPY --from=build /app/out .

# Puerto (Railway lo asigna via variable PORT)
EXPOSE 8080

# Comando de inicio
ENTRYPOINT ["dotnet", "SandwicheriaWalterio.Api.dll"]
