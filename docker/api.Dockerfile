FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY apps/server/TuTien.sln ./
COPY apps/server/src ./src
COPY apps/server/tests ./tests
RUN dotnet restore src/TuTien.Api/TuTien.Api.csproj
RUN dotnet publish src/TuTien.Api/TuTien.Api.csproj -c Release -o /app --no-restore
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
RUN apt-get update && apt-get install -y --no-install-recommends curl && rm -rf /var/lib/apt/lists/*
COPY --from=build /app .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "TuTien.Api.dll"]
