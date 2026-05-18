FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["src/CashFlow.Consolidation/CashFlow.Consolidation.csproj", "src/CashFlow.Consolidation/"]
COPY ["src/CashFlow.Shared/CashFlow.Shared.csproj", "src/CashFlow.Shared/"]
RUN dotnet restore "src/CashFlow.Consolidation/CashFlow.Consolidation.csproj"
COPY . .
RUN dotnet build "src/CashFlow.Consolidation/CashFlow.Consolidation.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "src/CashFlow.Consolidation/CashFlow.Consolidation.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CashFlow.Consolidation.dll"]
