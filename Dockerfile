FROM hub.nexdev.net/base/dotnet/core/sdk:7.0-alpine AS builder
WORKDIR /src

COPY ./Sources/DirectFantasy.Core ./DirectFantasy.Core
COPY ./Sources/DirectFantasy.Services ./DirectFantasy.Services
COPY ./Sources/DirectFantasy.WorkerService/DirectFantasy.WorkerService.csproj ./DirectFantasy.WorkerService/DirectFantasy.WorkerService.csproj

WORKDIR /src/DirectFantasy.WorkerService
RUN dotnet nuget add source http://nuget.nexdev.net/api/v2
RUN dotnet restore -v n

COPY ./Sources/DirectFantasy.WorkerService .

RUN dotnet publish -r linux-x64 --self-contained false -c Release -o /app 

FROM hub.nexdev.net/base/dotnet/core/aspnet:7.0-alpine AS base

WORKDIR /app
COPY --from=builder /app .

ENTRYPOINT ["dotnet", "DirectFantasy.WorkerService.dll"]
