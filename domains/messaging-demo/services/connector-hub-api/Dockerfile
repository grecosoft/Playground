FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

FROM  mcr.microsoft.com/dotnet/sdk:10.0 AS build

ENV SERVICE_SRC=services/connector-hub-api/src
ENV JSON_SCHEMAS=services/connector-hub-api/schemas

WORKDIR /src

# Copy csproj files and restore as distinct layers:
COPY ["Messaging.Demo.Common/Messaging.Demo.Common.csproj", "./Messaging.Demo.Common/"]
COPY ["${SERVICE_SRC}/Connector.Hub.Api/Connector.Hub.Api.csproj", "./${SERVICE_SRC}/Connector.Hub.Api/"]
COPY ["${SERVICE_SRC}/Connector.Hub.Domain/Connector.Hub.Domain.csproj", "./${SERVICE_SRC}/Connector.Hub.Domain/"]
COPY ["${SERVICE_SRC}/Connector.Hub.Infra/Connector.Hub.Infra.csproj", "./${SERVICE_SRC}/Connector.Hub.Infra/"]

COPY ["${SERVICE_SRC}/Connector.Hub.Tests/Connector.Hub.Tests.csproj", "./${SERVICE_SRC}/Connector.Hub.Tests/"]

RUN --mount=type=secret,id=READ_PACKAGE_TOKEN \
    export READ_PACKAGE_TOKEN=$(cat /run/secrets/READ_PACKAGE_TOKEN) && \
    dotnet nuget add source "https://nuget.pkg.github.com/grecosoft/index.json" \
      --name github \
      --username github \
      --password $READ_PACKAGE_TOKEN \
      --store-password-in-clear-text 

# # Restore dependencies for both the API and the tests:
RUN dotnet restore ./${SERVICE_SRC}/Connector.Hub.Api/Connector.Hub.Api.csproj
RUN dotnet restore ./${SERVICE_SRC}/Connector.Hub.Tests/Connector.Hub.Tests.csproj

# Copy all the source code:
COPY ["Messaging.Demo.Common/", "./Messaging.Demo.Common/"]  
COPY ["${SERVICE_SRC}/", "./${SERVICE_SRC}/"]


# Run tests and publish the API:
RUN dotnet test ./${SERVICE_SRC}/Connector.Hub.Tests/Connector.Hub.Tests.csproj \
  -c Release \
  --no-restore \
  --logger:trx \
  --results-directory /test-results \
  -p:UseProjectReferences=false

RUN dotnet publish ./${SERVICE_SRC}/Connector.Hub.Api/Connector.Hub.Api.csproj \
  -c Release \
  -o /service/publish \
  --no-restore \
  -p:UseProjectReferences=false

# Copy JSON schemas to the image for use at runtime.  
COPY ["${JSON_SCHEMAS}/", "/service/schemas"]

FROM scratch AS test-results
COPY --from=build /test-results .

FROM runtime AS image
ENV CommandSchemaRootPath=schemas/commands
WORKDIR /service
COPY --from=build /service/publish ./  
COPY --from=build /service/schemas ./schemas
ENTRYPOINT ["dotnet", "Connector.Hub.Api.dll"]


