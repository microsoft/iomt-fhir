using System;
using System.IO;
using System.Threading.Tasks;
using EnsureThat;
using Microsoft.Health.Fhir.Ingest.Template;

namespace Microsoft.Health.Tools.EventDebugger.TemplateLoader
{
    public class DeviceTemplateLoader : ITemplateLoader
    {
        private readonly string _templatePath;
        private readonly CollectionTemplateFactory<IContentTemplate, IContentTemplate> _collectionTemplateFactory;

        public DeviceTemplateLoader(
            string templatePath,
            CollectionTemplateFactory<IContentTemplate, IContentTemplate> collectionTemplateFactory)
        {
            _templatePath = EnsureArg.IsNotNullOrWhiteSpace(templatePath, nameof(templatePath));
            _collectionTemplateFactory = EnsureArg.IsNotNull(collectionTemplateFactory, nameof(collectionTemplateFactory));
        }
        public async Task<IContentTemplate> LoadTemplate()
        {
            var text = await File.ReadAllTextAsync(_templatePath);
            var templateContext = _collectionTemplateFactory.Create(text);
            return templateContext.Template;
        }
    }
}