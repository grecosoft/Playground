FROM mcr.microsoft.com/dotnet/aspnet:10.0@sha256:52dcfb4225fda614c38ba5997a4ec72cbd5260a624125174416e547ff9eb9b8c AS runtime
ENV ASPNETCORE_URLS=http://+:5081
EXPOSE 5081

FROM  mcr.microsoft.com/dotnet/sdk:10.0@sha256:0a506ab0c8aa077361af42f82569d364ab1b8741e967955d883e3f23683d473a AS build

ENV SERVICE_NAME=Playground.Service.One

WORKDIR /src

# Copy solution and projects then restore as distinct layers:
COPY ["src/${SERVICE_NAME}.sln", "./"]
COPY ["src/${SERVICE_NAME}.WebApi/${SERVICE_NAME}.WebApi.csproj", "./${SERVICE_NAME}.WebApi/"]
COPY ["src/${SERVICE_NAME}.Core/${SERVICE_NAME}.Core.csproj", "./${SERVICE_NAME}.Core/"]
COPY ["src/${SERVICE_NAME}.Tests/${SERVICE_NAME}.Tests.csproj", "./${SERVICE_NAME}.Tests/"]

RUN dotnet restore "./${SERVICE_NAME}.sln"

# Copy the solution's source code and build, test, and publish project:
COPY ["src/", "./"]

RUN dotnet build "./${SERVICE_NAME}.sln" -c Release --no-restore
RUN dotnet test "./${SERVICE_NAME}.sln" -c Release --no-build --logger:trx --results-directory /test-results    
RUN dotnet publish "./${SERVICE_NAME}.WebApi/${SERVICE_NAME}.WebApi.csproj" -c Release -o /service/publish --no-build   

FROM scratch AS build-export
COPY --from=build /test-results .

FROM runtime AS image
WORKDIR /service
COPY --from=build /service/publish ./   
ENTRYPOINT ["dotnet", "Playground.Service.One.WebApi.dll"]
