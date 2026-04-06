FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
ENV ASPNETCORE_URLS=http://+:5081
EXPOSE 5081

FROM  mcr.microsoft.com/dotnet/sdk:10.0 AS build

ENV COMMON_NAME=Messaging.Demo.Common
ENV SERVICE_SRC=services/messaging-hub-api/src
ENV SERVICE_Name=Messaging.Hub

WORKDIR /src



COPY services/messaging-hub-api/src/Messaging.Hub.Api/Messaging.Hub.Api.csproj ./services/messaging-hub-api/src/Messaging.Hub.Api/
RUN dotnet restore ./services/messaging-hub-api/src/Messaging.Hub.Api/Messaging.Hub.Api.csproj

COPY services/messaging-hub-api/src/Messaging.Hub.Api/ ./services/messaging-hub-api/src/Messaging.Hub.Api/
RUN dotnet publish ./services/messaging-hub-api/src/Messaging.Hub.Api/Messaging.Hub.Api.csproj -c Release -o /service/publish --no-restore   


# COPY ${SERVICE_SRC}/${SERVICE_Name}.Api/ ./services/messaging-hub-api/src/${SERVICE_Name}.Api/

#  RUN dotnet test ./services/messaging-hub-api/${SERVICE_Name}.Tests/${SERVICE_Name}.Tests.csproj -c Release --no-restore --logger:trx --results-directory /test-results   
#  RUN dotnet publish ./services/messaging-hub-api/src/${SERVICE_Name}.Api/${SERVICE_Name}.Api.csproj -c Release -o /service/publish --no-restore   


# # Copy solution and projects then restore as distinct layers:
# COPY ["src/${SERVICE_NAME}.sln", "./"]
# COPY ["src/${SERVICE_NAME}.WebApi/${SERVICE_NAME}.WebApi.csproj", "./${SERVICE_NAME}.WebApi/"]
# COPY ["src/${SERVICE_NAME}.Core/${SERVICE_NAME}.Core.csproj", "./${SERVICE_NAME}.Core/"]
# COPY ["src/${SERVICE_NAME}.Tests/${SERVICE_NAME}.Tests.csproj", "./${SERVICE_NAME}.Tests/"]

# RUN --mount=type=secret,id=READ_PACKAGE_TOKEN \
#     export READ_PACKAGE_TOKEN=$(cat /run/secrets/READ_PACKAGE_TOKEN) && \
#     dotnet nuget add source "https://nuget.pkg.github.com/grecosoft/index.json" \
#       --name github \
#       --username github \
#       --password $READ_PACKAGE_TOKEN \
#       --store-password-in-clear-text \
#     && dotnet restore "./${SERVICE_NAME}.sln"

# # Copy the solution's source code and build, test, and publish project:
# COPY ["src/", "./"]

# RUN dotnet build "${SERVICE_NAME}.sln" -c Release --no-restore
# RUN dotnet test "${SERVICE_NAME}.sln" -c Release --no-build --logger:trx --results-directory /test-results    
# RUN dotnet publish "${SERVICE_NAME}.WebApi/${SERVICE_NAME}.WebApi.csproj" -c Release -o /service/publish --no-build   

# FROM scratch AS test-results
# COPY --from=build /test-results .

# FROM runtime AS image
# WORKDIR /service
# COPY --from=build /service/publish ./   
# ENTRYPOINT ["dotnet", "Playground.Service.One.WebApi.dll"]
