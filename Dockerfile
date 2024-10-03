FROM hub.nexdev.net/base/dotnet/core/sdk:8.0-alpine AS builder
WORKDIR /src

COPY ./Sources/DirectFantasy ./DirectFantasy
COPY ./Sources/DirectFantasy.WorkerService/DirectFantasy.WorkerService.csproj ./DirectFantasy.WorkerService/DirectFantasy.WorkerService.csproj

WORKDIR /src/DirectFantasy.WorkerService
RUN dotnet nuget add source https://nuget.nexdev.net/api/v2
RUN dotnet restore -v n

COPY ./Sources/DirectFantasy.WorkerService .

RUN dotnet publish -r linux-x64 --self-contained false -c Release -o /app 

FROM hub.nexdev.net/base/dotnet/core/aspnet:8.0-alpine AS base

WORKDIR /app
COPY --from=builder /app .

ENTRYPOINT ["dotnet", "DirectFantasy.WorkerService.dll"]
