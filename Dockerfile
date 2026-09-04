# syntax=docker/dockerfile:1.7
# ============================================================================
# ErpBridge multi-target Dockerfile. Builds either:
#   * the central API  (target=centralapi) - Web API + PostgreSQL backend
#   * the admin panel  (target=admin)      - Blazor Server
# into a single self-contained ASP.NET Core 10 runtime image.
#
# Usage (Coolify "Dockerfile" source):
#   build args:  target=centralapi (or admin), version=git-sha (optional)
#   port:        5080 (centralapi) / 4002 (admin) - chosen at build time
#               and pinned in /tmp/bind_port so the entrypoint ignores the
#               PORT env var that Coolify may inject. This guarantees the
#               bind port matches the auto-generated Caddy/Traefik labels
#               (Coolify emits `{{upstreams 5080}}` for the central API by
#               default). See commit history if you need to override.
#
# IMPORTANT: Coolify restricts Dockerfile build-arg values to
# [a-z 0-9 . - _] only. The `target` arg therefore arrives in lowercase
# ("centralapi" or "admin"); the Dockerfile maps it to the PascalCase
# project folder ("ErpBridge.CentralApi" / "ErpBridge.Admin") and the
# PascalCase DLL name internally. The csproj files on disk are not
# renamed - only the docker-side mapping changes.
# ============================================================================

# ---------- shared base: ASP.NET Core 10 runtime ----------
FROM mcr.microsoft.com/dotnet/aspnet:10.0-jammy AS runtime
ENV ASPNETCORE_ENVIRONMENT=Production \
    DOTNET_RUNNING_IN_CONTAINER=true \
    DOTNET_NOLOGO=true \
    DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false \
    # Default to 5080 so the central API matches Coolify's auto-generated
    # Caddy/Traefik upstreams port. The entrypoint below pins the actual
    # bind port per build target, so this value is a fallback only.
    DEFAULT_PORT=5080
EXPOSE 5080
# Coolify executes the image healthcheck inside this runtime image. Install
# wget explicitly; the upstream ASP.NET image does not include it.
USER root
RUN apt-get update \
    && apt-get install -y --no-install-recommends wget \
    && rm -rf /var/lib/apt/lists/*

# Run as the built-in non-root user that the upstream image ships.
USER $APP_UID

# ---------- build stage ----------
FROM mcr.microsoft.com/dotnet/sdk:10.0-jammy AS build
# Coolify restricts build-arg values to [a-z 0-9 . - _]. We accept the
# lowercase form ("centralapi" / "admin") and map it to the PascalCase
# project folder name via build-time shell normalisation. The hard-coded
# mapping below is the source of truth - add a branch here whenever the
# solution gains a new buildable target.
ARG target=centralapi
RUN if [ "$target" = "centralapi" ]; then \
        export PROJECT_DIR="ErpBridge.CentralApi"; \
    elif [ "$target" = "admin" ]; then \
        export PROJECT_DIR="ErpBridge.Admin"; \
    else \
        echo "Unsupported target '$target' - expected 'centralapi' or 'admin'" >&2; \
        exit 1; \
    fi && \
    echo "PROJECT_DIR=${PROJECT_DIR}" > /tmp/project_dir.env
WORKDIR /src

# Copy NuGet inputs first for better layer caching. The solution file
# references every project, but restore only needs the csproj files; we
# copy them explicitly. The project-specific csproj copies use the
# literal PascalCase folder names because they live on disk in that
# case.
COPY ErpBridge.sln ./
COPY src/ErpBridge.Core/ErpBridge.Core.csproj                            src/ErpBridge.Core/
COPY src/ErpBridge.Shared/ErpBridge.Shared.csproj                        src/ErpBridge.Shared/
COPY src/ErpBridge.LocalStore/ErpBridge.LocalStore.csproj                src/ErpBridge.LocalStore/
COPY src/ErpBridge.Erp.Abstractions/ErpBridge.Erp.Abstractions.csproj    src/ErpBridge.Erp.Abstractions/
COPY src/ErpBridge.Erp.Mikro/ErpBridge.Erp.Mikro.csproj                   src/ErpBridge.Erp.Mikro/
COPY src/ErpBridge.RemoteApi/ErpBridge.RemoteApi.csproj                  src/ErpBridge.RemoteApi/
COPY src/ErpBridge.Agent.Service/ErpBridge.Agent.Service.csproj          src/ErpBridge.Agent.Service/
COPY src/ErpBridge.Agent.UI/ErpBridge.Agent.UI.csproj                    src/ErpBridge.Agent.UI/
COPY src/ErpBridge.CentralApi/ErpBridge.CentralApi.csproj                src/ErpBridge.CentralApi/
COPY src/ErpBridge.Admin/ErpBridge.Admin.csproj                          src/ErpBridge.Admin/

# Restore against the PascalCase project folder resolved above.
RUN PROJECT_DIR=$(cut -d= -f2 /tmp/project_dir.env) && \
    dotnet restore "src/${PROJECT_DIR}/${PROJECT_DIR}.csproj"

# Now copy the rest of the source. Changing source files after this layer
# will invalidate only this and later layers, keeping restore cached.
COPY src/ src/

RUN PROJECT_DIR=$(cut -d= -f2 /tmp/project_dir.env) && \
    dotnet publish "src/${PROJECT_DIR}/${PROJECT_DIR}.csproj" \
        -c Release \
        -o /app/publish \
        --no-restore \
        /p:UseAppHost=false \
        /p:Version=${version:-0.0.0}

# ---------- final stage ----------
FROM runtime AS final
FROM runtime AS centralapi
ARG target=centralapi
WORKDIR /app

# Copy the publish output for the chosen target. The shared DLLs (Core,
# Shared) are pulled in transitively by dotnet publish.
COPY --from=build /app/publish ./

# The central API exposes /health while the admin panel's liveness endpoint
# is its root page. Resolve the bind port at runtime so one image serves both.
HEALTHCHECK --interval=30s --timeout=5s --start-period=20s --retries=3 \
    CMD ["sh", "-c", "BIND_PORT=$(cat /tmp/bind_port); PATH_SUFFIX=/health; [ \"$BIND_PORT\" = \"4002\" ] && PATH_SUFFIX=/; wget -q -O /dev/null http://127.0.0.1:${BIND_PORT}${PATH_SUFFIX} || exit 1"]

# ENTRYPOINT uses a shell wrapper because the JSON-array form does not
# expand build args - see https://docs.docker.com/engine/reference/builder/#arg.
# The wrapper pins the bind port per build target, ignoring the PORT env
# var that Coolify may inject. This makes the bind port stable across
# redeploys and matches Coolify's auto-generated reverse-proxy labels.
RUN if [ "$target" = "centralapi" ]; then \
        echo "ErpBridge.CentralApi.dll" > /tmp/app_dll; \
        echo "5080" > /tmp/bind_port; \
    elif [ "$target" = "admin" ]; then \
        echo "ErpBridge.Admin.dll" > /tmp/app_dll; \
        echo "4002" > /tmp/bind_port; \
    else \
        echo "ErpBridge.dll" > /tmp/app_dll; \
        echo "8080" > /tmp/bind_port; \
    fi
ENTRYPOINT ["sh", "-c", "APP_DLL=$(cat /tmp/app_dll); BIND_PORT=$(cat /tmp/bind_port); exec dotnet \"$APP_DLL\" --urls \"http://+:${BIND_PORT}\""]

