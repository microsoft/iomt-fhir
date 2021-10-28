using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Health.Tools.EventDebugger.EventProcessor
{
    public interface IConversionResultWriter
    {
        Task StoreConversionResult(ConversionResult conversionResult, CancellationToken cancellationToken = default);
    }
}