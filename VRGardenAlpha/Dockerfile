#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["VRGardenAlpha/VRGardenAlpha.csproj", "VRGardenAlpha/"]
RUN dotnet restore "VRGardenAlpha/VRGardenAlpha.csproj"
COPY . .
WORKDIR "/src/VRGardenAlpha"
RUN dotnet build "VRGardenAlpha.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "VRGardenAlpha.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app

RUN mkdir -p /var/www/vrcg-storage
RUN mkdir -p /var/www/vrcg-trades-storage

COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "VRGardenAlpha.dll"]