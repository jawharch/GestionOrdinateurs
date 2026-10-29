FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY AuthentificationApi/AuthentificationApi.csproj AuthentificationApi/
RUN dotnet restore AuthentificationApi/AuthentificationApi.csproj

COPY AuthentificationApi/ AuthentificationApi/
RUN dotnet publish AuthentificationApi/AuthentificationApi.csproj \
    --configuration Release \
    --no-restore \
    --output /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:5120
EXPOSE 5120
ENTRYPOINT ["dotnet", "AuthentificationApi.dll"]
