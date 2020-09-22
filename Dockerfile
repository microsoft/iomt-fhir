FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS installer-env

ARG FhirVersion=R4

COPY ./src/func/Microsoft.Health.Fhir.Ingest.Host/ /src/iomt
COPY ./src/lib/ /lib
ADD stylecop.json /
ADD CustomAnalysisRules.ruleset /

RUN cd /src/iomt && \
    mkdir -p /home/site/wwwroot && \
    dotnet publish *.csproj --output /home/site/wwwroot /p:FhirVersion=${FhirVersion}

FROM mcr.microsoft.com/azure-functions/dotnet:3.0

COPY --from=installer-env ["/home/site/wwwroot", "/home/site/wwwroot"]
