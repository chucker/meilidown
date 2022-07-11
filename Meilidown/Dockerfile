﻿#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0-alpine AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0-alpine AS build
WORKDIR /src
COPY ["Meilidown.csproj", "."]
RUN dotnet restore "Meilidown.csproj"
COPY . .
RUN dotnet build "Meilidown.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Meilidown.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Meilidown.dll"]