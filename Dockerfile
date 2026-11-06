# syntax=docker/dockerfile:1
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY organizador-dietas-santo-tomas.csproj ./
RUN dotnet restore -r linux-x64
COPY . .
# El CSS compilado (wwwroot/css/site.css) esta versionado; no se necesita Node.
RUN dotnet publish organizador-dietas-santo-tomas.csproj -c Release -r linux-x64 --no-self-contained -o /app

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app ./
ENV ASPNETCORE_ENVIRONMENT=Production \
    DOTNET_gcServer=0
# Railway inyecta PORT; se enlaza en 0.0.0.0.
CMD ["sh", "-c", "ASPNETCORE_URLS=http://0.0.0.0:${PORT:-8080} exec dotnet organizador-dietas-santo-tomas.dll"]
