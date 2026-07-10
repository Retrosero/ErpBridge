# syntax=docker/dockerfile:1.7
# ============================================================================
# ErpBridge multi-target Dockerfile. Builds either:
#   * the central API  (TARGET=CentralApi) - Web API + PostgreSQL backend
#   * the admin panel  (TARGET=Admin)      - Blazor Server
# into a single self-contained ASP.NET Core 8 runtime image.
#
# Usage (Coolify "Dockerfile" source):
#   build args:  TARGET=CentralApi (or Admin), VERSION=git-sha (optional)
#   port:        4001 - Coolify fronts the container with Traefik; external
#               traffic arrives at HTTPS 443 and is reverse-proxied to the
#               container port. Override the internal port at deploy time
#               by setting the `PORT` environment variable in Coolify.
#
# IMPORTANT: TARGET must be passed in PascalCase to match the project
# folder under src/ErpBridge.*. Coolify passes build args verbatim;
# we never downcase them, so callers must write `CentralApi` / `Admin`.
# ============================================================================

# ---------- shared base: ASP.NET Core 8 runtime ----------
FROM mcr.microsoft.com/dotnet/aspnet:8.0-jammy AS runtime
ENV ASPNETCORE_ENVIRONMENT=Production \
    DOTNET_RUNNING_IN_CONTAINER=true \
    DOTNET_NOLOGO=true \
    DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false \
    # Internal bind port. Most PaaS (Coolify, Render, Railway, Fly.io)
    # expose PORT to the runtime; we read it through the entrypoint shell
    # wrapper below. Default to 4001 for `docker run` / debug sessions
    # where no PORT is supplied.
    DEFAULT_PORT=4001
EXPOSE 4001
# Run as the built-in non-root user that the upstream image ships.
USER $APP_UID

# ---------- build stage ----------
FROM mcr.microsoft.com/dotnet/sdk:8.0-jammy AS build
ARG TARGET=CentralApi
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
ARG TARGET=CentralApi
WORKDIR /app

# Copy the publish output for the chosen target. The shared DLLs (Core,
# Shared) are pulled in transitively by dotnet publish.
COPY --from=build /app/publish ./

# Healthcheck hits the anonymous /health endpoint. We resolve the bind
# port the same way the entrypoint does so the probe never points to a
# port the application is not listening on.
HEALTHCHECK --interval=30s --timeout=5s --start-period=20s --retries=3 \
    CMD ["sh", "-c", "wget -q -O- http://127.0.0.1:${PORT:-$DEFAULT_PORT}/health || exit 1"]

# ENTRYPOINT uses a shell wrapper because the JSON-array form does not
# expand build args - see https://docs.docker.com/engine/reference/builder/#arg.
# The wrapper also resolves $PORT against $DEFAULT_PORT, so we stay
# portable across Coolify, Railway, Render, Fly.io, and a bare
# `docker run -e PORT=5000`.
ENV APP_DLL=ErpBridge.${TARGET}.dll
ENTRYPOINT ["sh", "-c", "exec dotnet \"$APP_DLL\" --urls \"http://+:${PORT:-$DEFAULT_PORT}\""]
