#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER root
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*
USER app
WORKDIR /app
EXPOSE 5142
EXPOSE 7203

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["ZasNet.WebApi/ZasNet.WebApi.csproj", "ZasNet.WebApi/"]
RUN dotnet restore "./ZasNet.WebApi/./ZasNet.WebApi.csproj"
COPY . .
WORKDIR "/src/ZasNet.WebApi"
RUN dotnet build "./ZasNet.WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./ZasNet.WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ZasNet.WebApi.dll"]