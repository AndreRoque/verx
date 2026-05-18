FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["src/CashFlow.Transactions/CashFlow.Transactions.csproj", "src/CashFlow.Transactions/"]
COPY ["src/CashFlow.Shared/CashFlow.Shared.csproj", "src/CashFlow.Shared/"]
RUN dotnet restore "src/CashFlow.Transactions/CashFlow.Transactions.csproj"
COPY . .
RUN dotnet build "src/CashFlow.Transactions/CashFlow.Transactions.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "src/CashFlow.Transactions/CashFlow.Transactions.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CashFlow.Transactions.dll"]
