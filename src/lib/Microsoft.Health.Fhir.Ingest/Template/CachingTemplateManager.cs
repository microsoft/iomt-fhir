using System;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace Microsoft.Health.Fhir.Ingest.Console.Template
{
	public class CachingTemplateManager : ITemplateManager
	{
		private ITemplateManager _wrappedTemplateManager;
        private IMemoryCache _templateCache;


        public CachingTemplateManager(
            ITemplateManager wrappedTemplateManager,
            IMemoryCache cache)
		{
			_wrappedTemplateManager = wrappedTemplateManager;
            _templateCache = cache;
		}

        public byte[] GetTemplate(string templateName)
        {
            var key = $"{templateName}Bytes";
            return _templateCache.GetOrCreate(key,
                e =>
                {
                    e.SetAbsoluteExpiration(TimeSpan.FromMinutes(1));
                    return _wrappedTemplateManager.GetTemplate(templateName);
                });
        }

        public string GetTemplateAsString(string templateName)
        {
            return _templateCache.GetOrCreate(templateName,
                e =>
                {
                    e.SetAbsoluteExpiration(TimeSpan.FromMinutes(1));
                    return _wrappedTemplateManager.GetTemplateAsString(templateName);
                });
        }
    }
}

