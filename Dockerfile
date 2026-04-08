FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
# Copy only the API project folder and restore/publish from there
COPY EcommerceAPI/ ./EcommerceAPI/
WORKDIR /src/EcommerceAPI
RUN dotnet restore "EcommerceAPI.csproj"
RUN dotnet publish "EcommerceAPI.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
# Create uploads directory (will be mounted as Railway Volume for persistence)
RUN mkdir -p /app/wwwroot/uploads
ENTRYPOINT ["dotnet", "EcommerceAPI.dll"]
