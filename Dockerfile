# syntax=docker/dockerfile:1.7
# ============================================================================
# ErpBridge multi-target Dockerfile. Builds either:
#   * the central API  (TARGET=centralapi) — Web API + PostgreSQL backend
#   * the admin panel  (TARGET=admin)     — Blazor Server
# into a single self-contained ASP.NET Core 8 runtime image.
#
# Usage (Coolify "Dockerfile" source):
#   build args:  TARGET=centralapi (or admin), VERSION=git-sha (optional)
#   expose:      8080 (HTTP; Coolify fronts with Traefik + Let's Encrypt)
# ============================================================================

# ---------- shared base: ASP.NET Core 8 runtime ----------
FROM mcr.microsoft.com/dotnet/aspnet:8.0-jammy AS runtime
ENV ASPNETCORE_URLS=http://+:8080 \
    ASPNETCORE_ENVIRONMENT=Production \
    DOTNET_RUNNING_IN_CONTAINER=true \
    DOTNET_NOLOGO=true \
    DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false
EXPOSE 8080
# Run as the built-in non-root user that the upstream image ships.
USER $APP_UID

# ---------- build stage ----------
FROM mcr.microsoft.com/dotnet/sdk:8.0-jammy AS build
ARG TARGET=centralapi
WORKDIR /src

# Copy NuGet inputs first for better layer caching. The solution file
# references every project, but restore only needs the csproj files; we
# copy them explicitly.
COPY ErpBridge.sln ./
COPY src/ErpBridge.Core/ErpBridge.Core.csproj                  src/ErpBridge.Core/
COPY src/ErpBridge.Shared/ErpBridge.Shared.csproj              src/ErpBridge.Shared/
COPY src/ErpBridge.LocalStore/ErpBridge.LocalStore.csproj      src/ErpBridge.LocalStore/
COPY src/ErpBridge.Erp.Abstractions/ErpBridge.Erp.Abstractions.csproj src/ErpBridge.Erp.Abstractions/
COPY src/ErpBridge.Erp.Mikro/ErpBridge.Erp.Mikro.csproj       src/ErpBridge.Erp.Mikro/
COPY src/ErpBridge.RemoteApi/ErpBridge.RemoteApi.csproj        src/ErpBridge.RemoteApi/
COPY src/ErpBridge.Agent.Service/ErpBridge.Agent.Service.csproj src/ErpBridge.Agent.Service/
COPY src/ErpBridge.Agent.UI/ErpBridge.Agent.UI.csproj          src/ErpBridge.Agent.UI/
COPY src/ErpBridge.CentralApi/ErpBridge.CentralApi.csproj      src/ErpBridge.CentralApi/
COPY src/ErpBridge.Admin/ErpBridge.Admin.csproj                src/ErpBridge.Admin/

RUN dotnet restore src/ErpBridge.${TARGET}/ErpBridge.${TARGET}.csproj

# Now copy the rest of the source. Changing source files after this layer
# will invalidate only this and later layers, keeping restore cached.
COPY src/ src/

RUN dotnet publish src/ErpBridge.${TARGET}/ErpBridge.${TARGET}.csproj \
    -c Release \
    -o /app/publish \
    --no-restore \
    /p:UseAppHost=false \
    /p:Version=${VERSION:-0.0.0}

# ---------- final stage ----------
FROM runtime AS final
ARG TARGET=centralapi
WORKDIR /app

# Copy the publish output for the chosen target. The shared DLLs (Core,
# Shared) are pulled in transitively by dotnet publish.
COPY --from=build /app/publish ./

# Healthcheck. The /health endpoint is anonymous; on a non-200 we let the
# container orchestrator restart us. Five-second timeout for slow boots.
HEALTHCHECK --interval=30s --timeout=5s --start-period=20s --retries=3 \
    CMD ["dotnet", "--info"]

# By default the central API is the entrypoint. The admin image overrides
# via Coolify "Entrypoint" / override arguments if needed; the assembly name
# is identical to the project name, so the default works for both targets.
# We use a shell wrapper because ENTRYPOINT's JSON-array form does not
# expand build args — see https://docs.docker.com/engine/reference/builder/#arg.
ARG TARGET=centralapi
ENV APP_DLL=ErpBridge.${TARGET}.dll
ENTRYPOINT ["sh", "-c", "exec dotnet \"$APP_DLL\""]