FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
ENV ASPNETCORE_URLS=http://+:5081
EXPOSE 5081

FROM  mcr.microsoft.com/dotnet/sdk:10.0 AS build

ENV SERVICE_SRC=services/messaging-demo-api/src

WORKDIR /src

# Copy csproj files and restore as distinct layers:
COPY ["Messaging.Demo.Common/Messaging.Demo.Common.csproj", "./Messaging.Demo.Common/"]
COPY ["${SERVICE_SRC}/Demo.One.Api/Demo.One.Api.csproj", "./${SERVICE_SRC}/Demo.One.Api/"]
COPY ["${SERVICE_SRC}/Demo.One.Api.Tests/Demo.One.Api.Tests.csproj", "./${SERVICE_SRC}/Demo.One.Api.Tests/"]

RUN --mount=type=secret,id=READ_PACKAGE_TOKEN \
    export READ_PACKAGE_TOKEN=$(cat /run/secrets/READ_PACKAGE_TOKEN) && \
    dotnet nuget add source "https://nuget.pkg.github.com/grecosoft/index.json" \
      --name github \
      --username github \
      --password $READ_PACKAGE_TOKEN \
      --store-password-in-clear-text 

# Restore dependencies for both the API and the tests:
RUN dotnet restore ./${SERVICE_SRC}/Demo.One.Api/Demo.One.Api.csproj
RUN dotnet restore ./${SERVICE_SRC}/Demo.One.Api.Tests/Demo.One.Api.Tests.csproj

# Copy the remaining source code:
COPY ["${SERVICE_SRC}/Demo.One.Api/", "./${SERVICE_SRC}/Demo.One.Api/"]
COPY ["${SERVICE_SRC}/Demo.One.Api.Tests/", "./${SERVICE_SRC}/Demo.One.Api.Tests/"]

# Run tests and publish the API:
RUN dotnet test ./${SERVICE_SRC}/Demo.One.Api.Tests/Demo.One.Api.Tests.csproj -c Release --no-restore --logger:trx --results-directory /test-results
RUN dotnet publish ./${SERVICE_SRC}/Demo.One.Api/Demo.One.Api.csproj -c Release -o /service/publish --no-restore   

FROM scratch AS test-results
COPY --from=build /test-results .

FROM runtime AS image
WORKDIR /service
COPY --from=build /service/publish ./   
ENTRYPOINT ["dotnet", "Demo.One.Api.dll"]
