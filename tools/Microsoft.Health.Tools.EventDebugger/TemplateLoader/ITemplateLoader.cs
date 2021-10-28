using System.Threading.Tasks;
using Microsoft.Health.Fhir.Ingest.Template;

namespace Microsoft.Health.Tools.EventDebugger.TemplateLoader
{
    public interface ITemplateLoader
    {
        Task<IContentTemplate> LoadTemplate();
    }
}