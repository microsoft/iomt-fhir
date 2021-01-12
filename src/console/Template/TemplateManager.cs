// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using Microsoft.Health.Events.Repository;
using System.Text;

namespace Microsoft.Health.Fhir.Ingest.Console.Template
{
    public class TemplateManager : ITemplateManager
    {
        private IRepositoryManager _respositoryManager;
        public TemplateManager(IRepositoryManager repositoryManager)
        {
            _respositoryManager = repositoryManager;
        }

        public byte[] GetTemplate(string templateName)
        {
            return _respositoryManager.GetItem(templateName);
        }

        public string GetTemplateAsString(string templateName)
        {
            var templateBuffer = GetTemplate(templateName);
            string templateContent = Encoding.UTF8.GetString(templateBuffer, 0, templateBuffer.Length);
            return templateContent;
        }
    }
}
