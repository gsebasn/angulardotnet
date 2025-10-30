# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY ["StudyShop.Api/StudyShop.Api.csproj", "StudyShop.Api/"]
COPY ["StudyShop.sln", "."]
RUN dotnet restore "StudyShop.Api/StudyShop.Api.csproj"

# Copy everything else and build
COPY StudyShop.Api/ StudyShop.Api/
WORKDIR "/src/StudyShop.Api"
RUN dotnet build "StudyShop.Api.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "StudyShop.Api.csproj" -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# Create a non-root user
RUN groupadd -r appuser && useradd -r -g appuser appuser

# Copy published app
COPY --from=publish /app/publish .
RUN chown -R appuser:appuser /app

USER appuser

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "StudyShop.Api.dll"]

