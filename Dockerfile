FROM mcr.microsoft.com/dotnet/core/sdk:2.2 AS installer-env

COPY ./src/func/Microsoft.Health.Fhir.Ingest.Host/ /src/iomt
COPY ./src/lib/ /lib
ADD stylecop.json /
ADD CustomAnalysisRules.ruleset /

RUN cd /src/iomt && \
    mkdir -p /home/site/wwwroot && \
    dotnet publish *.csproj --output /home/site/wwwroot

FROM mcr.microsoft.com/azure-functions/dotnet:2.0

COPY --from=installer-env ["/home/site/wwwroot", "/home/site/wwwroot"]
