FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["NoughtsAndCrosses.API/NoughtsAndCrosses.API.csproj", "NoughtsAndCrosses.API/"]
COPY ["NoughtsAndCrosses.Infrastructure.Data/NoughtsAndCrosses.Infrastructure.Data.csproj", "NoughtsAndCrosses.Infrastructure.Data/"]
COPY ["NoughtsAndCrosses.Application/NoughtsAndCrosses.Application.csproj", "NoughtsAndCrosses.Application/"]
RUN dotnet restore "NoughtsAndCrosses.API/NoughtsAndCrosses.API.csproj"
COPY . .
WORKDIR "/src/NoughtsAndCrosses.API"
RUN dotnet build "NoughtsAndCrosses.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "NoughtsAndCrosses.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "NoughtsAndCrosses.API.dll"]
