FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

FROM  mcr.microsoft.com/dotnet/sdk:10.0 AS build

ENV SERVICE_SRC=services/acme-agent-api/src

WORKDIR /src

# Copy csproj files and restore as distinct layers:
COPY ["${SERVICE_SRC}/Acme.Agent.Api/Acme.Agent.Api.csproj", "./${SERVICE_SRC}/Acme.Agent.Api/"]
COPY ["${SERVICE_SRC}/Acme.Agent.Tests/Acme.Agent.Tests.csproj", "./${SERVICE_SRC}/Acme.Agent.Tests/"]

RUN --mount=type=secret,id=READ_PACKAGE_TOKEN \
    export READ_PACKAGE_TOKEN=$(cat /run/secrets/READ_PACKAGE_TOKEN) && \
    dotnet nuget add source "https://nuget.pkg.github.com/grecosoft/index.json" \
      --name github \
      --username github \
      --password $READ_PACKAGE_TOKEN \
      --store-password-in-clear-text 
      
# Restore dependencies for both the API and the tests:
RUN dotnet restore ./${SERVICE_SRC}/Acme.Agent.Api/Acme.Agent.Api.csproj
RUN dotnet restore ./${SERVICE_SRC}/Acme.Agent.Tests/Acme.Agent.Tests.csproj

# Copy all the source code:
COPY ["${SERVICE_SRC}/", "./${SERVICE_SRC}/"]

# Run tests and publish the API:
RUN dotnet test ./${SERVICE_SRC}/Acme.Agent.Tests/Acme.Agent.Tests.csproj \
  -c Release \
  --no-restore \
  --logger:trx \
  --results-directory /test-results \
  -p:UseProjectReferences=false

RUN dotnet publish ./${SERVICE_SRC}/Acme.Agent.Api/Acme.Agent.Api.csproj \
  -c Release \
  -o /service/publish \
  --no-restore \
  -p:UseProjectReferences=false   

FROM scratch AS test-results
COPY --from=build /test-results .

FROM runtime AS image
WORKDIR /service
COPY --from=build /service/publish ./   
ENTRYPOINT ["dotnet", "Acme.Agent.Api.dll"]
