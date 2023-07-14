#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["nuget.config", "."]
COPY ["src/console/Microsoft.Health.Fhir.Ingest.Console.FhirTransformation/Microsoft.Health.Fhir.Ingest.Console.FhirTransformation.csproj", "src/console/Microsoft.Health.Fhir.Ingest.Console.FhirTransformation/"]
COPY ["src/console/Microsoft.Health.Fhir.Ingest.Console.Common/Microsoft.Health.Fhir.Ingest.Console.Common.csproj", "src/console/Microsoft.Health.Fhir.Ingest.Console.Common/"]
COPY ["src/lib", "src/lib"]
RUN dotnet restore "src/console/Microsoft.Health.Fhir.Ingest.Console.FhirTransformation/Microsoft.Health.Fhir.Ingest.Console.FhirTransformation.csproj"
COPY . .
WORKDIR "/src/src/console/Microsoft.Health.Fhir.Ingest.Console.FhirTransformation"
RUN dotnet build "Microsoft.Health.Fhir.Ingest.Console.FhirTransformation.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Microsoft.Health.Fhir.Ingest.Console.FhirTransformation.csproj" -c Release -o /app/publish /p:UseAppHost=false
 
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Microsoft.Health.Fhir.Ingest.Console.FhirTransformation.dll"]