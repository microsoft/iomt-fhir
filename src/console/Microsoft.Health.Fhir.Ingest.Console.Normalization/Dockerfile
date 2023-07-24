FROM mcr.microsoft.com/dotnet/runtime:6.0@sha256:3525f2e1a54f8e437ea98a2237e1db8d9dc1221b871a3b940eb5230723dc00fd AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0@sha256:5ecf5be1ce0aa12d623963336a54f588091797f75268d57d40dc01260fc58d09 AS publish
COPY ["nuget.config", "."]
COPY ["src/console/Microsoft.Health.Fhir.Ingest.Console.Normalization", "src/console/Microsoft.Health.Fhir.Ingest.Console.Normalization"]
COPY ["src/console/Microsoft.Health.Fhir.Ingest.Console.Common", "src/console/Microsoft.Health.Fhir.Ingest.Console.Common"]
COPY ["src/lib", "src/lib"]
COPY ["CustomAnalysisRules.ruleset", "."]
COPY ["stylecop.json", "."]

WORKDIR "/src/console/Microsoft.Health.Fhir.Ingest.Console.Normalization"
RUN dotnet publish "Microsoft.Health.Fhir.Ingest.Console.Normalization.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app

COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Microsoft.Health.Fhir.Ingest.Console.Normalization.dll"] 