FROM hub.nexdev.net/base/dotnet/core/sdk:7.0-alpine AS builder
WORKDIR /src

COPY ./Sources/DirectPayout.Fantasy ./DirectPayout.Fantasy
COPY ./Sources/DirectPayout.Fantasy.Service/DirectPayout.Fantasy.Service.csproj ./DirectPayout.Fantasy.Service/DirectPayout.Fantasy.Service.csproj

WORKDIR /src/DirectPayout.Fantasy.Service
RUN dotnet nuget add source http://nuget.nexdev.net/api/v2
RUN dotnet restore -v n

COPY ./Sources/DirectPayout.Fantasy.Service .

RUN dotnet publish -r linux-x64 --self-contained false -c Release -o /app 

FROM hub.nexdev.net/base/dotnet/core/aspnet:7.0-alpine AS base

WORKDIR /app
COPY --from=builder /app .

ENTRYPOINT ["dotnet", "DirectPayout.Fantasy.Service.dll"]
