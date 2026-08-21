FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["backend/Korp.BillingService/Korp.BillingService.csproj", "backend/Korp.BillingService/"]
RUN dotnet restore "backend/Korp.BillingService/Korp.BillingService.csproj"

COPY . .
RUN dotnet publish "backend/Korp.BillingService/Korp.BillingService.csproj" \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "Korp.BillingService.dll"]
