using System.Threading;
using System.Threading.Tasks;
using Microsoft.Health.Fhir.Ingest.Validation;

namespace Microsoft.Health.Tools.EventDebugger.EventProcessor
{
    public interface IConversionResultWriter
    {
        Task StoreConversionResult(ValidationResult conversionResult, CancellationToken cancellationToken = default);
    }
}