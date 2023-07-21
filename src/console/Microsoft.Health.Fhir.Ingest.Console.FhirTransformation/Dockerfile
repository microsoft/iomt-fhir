FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS publish
COPY ["nuget.config", "."]
COPY ["src/console/Microsoft.Health.Fhir.Ingest.Console.Normalization", "src/console/Microsoft.Health.Fhir.Ingest.Console.Normalization"]
COPY ["src/console/Microsoft.Health.Fhir.Ingest.Console.Common", "src/console/Microsoft.Health.Fhir.Ingest.Console.Common"]
COPY ["src/lib", "src/lib"]
COPY ["CustomAnalysisRules.ruleset", "."]
COPY ["stylecop.json", "."]

WORKDIR "/src/console/Microsoft.Health.Fhir.Ingest.Console.FhirTransformation"
RUN dotnet publish "Microsoft.Health.Fhir.Ingest.Console.FhirTransformation.csproj" -c Release -o /app/publish /p:UseAppHost=false
 
FROM base AS final
WORKDIR /app

COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Microsoft.Health.Fhir.Ingest.Console.FhirTransformation.dll"]