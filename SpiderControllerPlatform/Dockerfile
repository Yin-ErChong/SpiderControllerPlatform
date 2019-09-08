FROM mcr.microsoft.com/dotnet/core/aspnet:2.1-stretch-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:2.1-stretch AS build
WORKDIR /src
COPY ["SpiderControllerPlatform/SpiderControllerPlatform.csproj", "SpiderControllerPlatform/"]
RUN dotnet restore "SpiderControllerPlatform/SpiderControllerPlatform.csproj"
COPY . .
WORKDIR "/src/SpiderControllerPlatform"
RUN dotnet build "SpiderControllerPlatform.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "SpiderControllerPlatform.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "SpiderControllerPlatform.dll"]