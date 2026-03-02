FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
ENV ASPNETCORE_URLS=http://+:5081
EXPOSE 5081

FROM  mcr.microsoft.com/dotnet/sdk:10.0 AS build

ENV SERVICE_NAME=Playground.Service.One

WORKDIR /src

# Copy solution and projects then restore as distinct layers:
COPY ["${SERVICE_NAME}.sln", "./"]
COPY ["${SERVICE_NAME}.WebApi/${SERVICE_NAME}.WebApi.csproj", "./${SERVICE_NAME}.WebApi/"]
COPY ["${SERVICE_NAME}.Core/${SERVICE_NAME}.Core.csproj", "./${SERVICE_NAME}.Core/"]
COPY ["${SERVICE_NAME}.Tests/${SERVICE_NAME}.Tests.csproj", "./${SERVICE_NAME}.Tests/"]

RUN dotnet restore "./${SERVICE_NAME}.sln"

# Copy the solution's source code and build, test, and publish project:
COPY ["./", "./"]

RUN dotnet nuget update source github-packages \
    --username docker \
    --password $(cat /run/secrets/READ_PACKAGE_TOKEN) \
    --store-password-in-clear-text 

RUN dotnet build "./${SERVICE_NAME}.sln" -c Release --no-restore
RUN dotnet test "./${SERVICE_NAME}.sln" -c Release --no-build --logger:trx --results-directory /test-results    
RUN dotnet publish "./${SERVICE_NAME}.WebApi/${SERVICE_NAME}.WebApi.csproj" -c Release -o /service/publish --no-build   

FROM scratch AS test-results
COPY --from=build /test-results .

FROM runtime AS image
WORKDIR /service
COPY --from=build /service/publish ./   
ENTRYPOINT ["dotnet", "Playground.Service.One.WebApi.dll"]
