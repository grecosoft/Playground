FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
ENV ASPNETCORE_URLS=http://+:5081
EXPOSE 5081

FROM  mcr.microsoft.com/dotnet/sdk:10.0 AS build

ENV SERVICE_SRC=services/messaging-hub-api/src

WORKDIR /src

COPY ["Messaging.Demo.Common/Messaging.Demo.Common.csproj", "./Messaging.Demo.Common/"]
COPY ["${SERVICE_SRC}/Messaging.Hub.Api/Messaging.Hub.Api.csproj", "./${SERVICE_SRC}/Messaging.Hub.Api/"]
COPY ["${SERVICE_SRC}/Messaging.Hub.Tests/Messaging.Hub.Tests.csproj", "./${SERVICE_SRC}/Messaging.Hub.Tests/"]

RUN --mount=type=secret,id=READ_PACKAGE_TOKEN \
    export READ_PACKAGE_TOKEN=$(cat /run/secrets/READ_PACKAGE_TOKEN) && \
    dotnet nuget add source "https://nuget.pkg.github.com/grecosoft/index.json" \
      --name github \
      --username github \
      --password $READ_PACKAGE_TOKEN \
      --store-password-in-clear-text 

RUN dotnet restore ./${SERVICE_SRC}/Messaging.Hub.Api/Messaging.Hub.Api.csproj
RUN dotnet restore ./${SERVICE_SRC}/Messaging.Hub.Tests/Messaging.Hub.Tests.csproj

COPY ["${SERVICE_SRC}/Messaging.Hub.Api/", "./${SERVICE_SRC}/Messaging.Hub.Api/"]
COPY ["${SERVICE_SRC}/Messaging.Hub.Tests/", "./${SERVICE_SRC}/Messaging.Hub.Tests/"]

RUN dotnet publish ./${SERVICE_SRC}/Messaging.Hub.Api/Messaging.Hub.Api.csproj -c Release -o /service/publish --no-restore   
RUN dotnet test ./${SERVICE_SRC}/Messaging.Hub.Tests/Messaging.Hub.Tests.csproj -c Release --no-restore --logger:trx --results-directory /test-results

FROM scratch AS test-results
COPY --from=build /test-results .

FROM runtime AS image
WORKDIR /service
COPY --from=build /service/publish ./   
ENTRYPOINT ["dotnet", "Messaging.Hub.Api.dll"]
