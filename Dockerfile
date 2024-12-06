#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Cursus.API/Cursus.API.csproj", "Cursus.API/"]
COPY ["Cursus.Repository/Cursus.Repository.csproj", "Cursus.Repository/"]
COPY ["Cursus.Common/Cursus.Common.csproj", "Cursus.Common/"]
COPY ["Cursus.Data/Cursus.Data.csproj", "Cursus.Data/"]
COPY ["Cursus.RepositoryContract/Cursus.RepositoryContract.csproj", "Cursus.RepositoryContract/"]
COPY ["Cursus.Service/Cursus.Service.csproj", "Cursus.Service/"]
COPY ["Cursus.ServiceContract/Cursus.ServiceContract.csproj", "Cursus.ServiceContract/"]
RUN dotnet restore "./Cursus.API/Cursus.API.csproj"
COPY . .
WORKDIR "/src/Cursus.API"
RUN dotnet build "./Cursus.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Cursus.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Cursus.API.dll"]	