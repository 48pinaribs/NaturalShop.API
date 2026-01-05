# ============================
# Build stage
# ============================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY . .
RUN dotnet restore "./NaturalShop.API.csproj"
RUN dotnet publish "./NaturalShop.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# ============================
# Runtime stage
# ============================
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

EXPOSE 8080
COPY --from=build /app/publish .

CMD ["sh", "-c", "ASPNETCORE_URLS=http://0.0.0.0:${PORT:-8080} dotnet NaturalShop.API.dll"]
