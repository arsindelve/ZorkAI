# One Dockerfile, four images — one per game backend, selected with `--target`.
#
# All four share a single `build` stage. Docker caches that stage, so building all four costs one
# restore and one publish pass rather than four; compose picks the game with `target:`.
#
# The build context is the repo root, not the Lambda directory: every Lambda csproj reaches up to
# ..\..\..\GameEngine, ..\..\..\Model and ..\..\..\DynamoDb, so the whole solution has to be visible.
# See .dockerignore for what is excluded (and for why the test projects are not).

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY . .

# PublishReadyToRun off on purpose: R2R buys cold-start time that matters in Lambda and not at all in
# a long-running container, and it makes every local build noticeably slower.
ARG PUBLISH_ARGS="-c Release --nologo /p:PublishReadyToRun=false"

RUN dotnet publish Lambda/src/Lambda                               $PUBLISH_ARGS -o /app/zork1 \
 && dotnet publish Planetfall-Lambda/src/Planetfall-Lambda         $PUBLISH_ARGS -o /app/planetfall \
 && dotnet publish EscapeRoom-Lambda/src/EscapeRoom-Lambda         $PUBLISH_ARGS -o /app/escaperoom \
 && dotnet publish Stationfall-Lambda/src/Stationfall-Lambda       $PUBLISH_ARGS -o /app/stationfall


FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# The explicit opt-in that swaps DynamoDB/Secrets Manager/CloudWatch for local equivalents. It is a
# dedicated variable rather than something inferred from OPENAI_BASE_URL on purpose: see
# GameEngine/SelfHostedMode.cs. Set here because this image exists only to run locally.
ENV ZORKAI_SELF_HOSTED=true

# Self-hosted play writes session state and saved games to disk instead of DynamoDB. This must be a
# mounted volume in compose — left inside the container, every session dies with it.
ENV ZORKAI_SAVE_DIR=/data

# The apps call app.UseHttpsRedirection(); with an HTTP-only binding the middleware logs "failed to
# determine the https port" once and passes the request through. Binding HTTP only is deliberate:
# these are local backends behind nothing.
ENV ASPNETCORE_URLS=http://+:8080

# Production because that is what a long-running backend wants: no developer exception page, less
# logging, no per-startup DI validation. It is no longer a workaround — Development used to be
# impossible (GameEngineInitializer consumed the scoped IGameEngine from a singleton, and the
# IGenerationClient descriptor could not be constructed at all), and both are fixed, so you can
# override this for debugging if you want to.
ENV ASPNETCORE_ENVIRONMENT=Production

# curl is here only so compose can run a real healthcheck — the aspnet image ships without it, and a
# 24/7 container that has wedged should not keep reporting "up".
RUN apt-get update \
 && apt-get install -y --no-install-recommends curl \
 && rm -rf /var/lib/apt/lists/*

# Runs as the built-in non-root user; /data must be writable by it, hence the chown.
RUN mkdir -p /data && chown -R app:app /data
USER app
EXPOSE 8080


FROM runtime AS zork1
COPY --from=build /app/zork1 .
ENTRYPOINT ["dotnet", "Lambda.dll"]

FROM runtime AS planetfall
COPY --from=build /app/planetfall .
ENTRYPOINT ["dotnet", "Planetfall-Lambda.dll"]

FROM runtime AS escaperoom
COPY --from=build /app/escaperoom .
ENTRYPOINT ["dotnet", "EscapeRoom-Lambda.dll"]

FROM runtime AS stationfall
COPY --from=build /app/stationfall .
ENTRYPOINT ["dotnet", "Stationfall-Lambda.dll"]
