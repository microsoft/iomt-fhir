// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Text;
using EnsureThat;
using Microsoft.Health.Events.Errors;
using Microsoft.Health.Events.Model;

namespace Microsoft.Health.Fhir.Ingest.Service
{
    public class MeasurementProcessingException : Exception
    {
        public MeasurementProcessingException(Exception ex, IEventMessage evt)
            : base(IncludeContext(ex, evt), ex)
        {
            EnsureArg.IsNotNull(ex, nameof(ex));
            EnsureArg.IsNotNull(evt, nameof(evt));
        }

        private static string IncludeContext(Exception ex, IEventMessage evt = null)
        {
            EnsureArg.IsNotNull(ex, nameof(ex));
            EnsureArg.IsNotNull(evt, nameof(evt));

            if (evt != null)
            {
                ex.AddEventContext(evt);
            }

            StringBuilder sb = new (ex.Message);

            Exception innerException = ex.InnerException;
            int exceptionCount = 0;
            while (innerException != null)
            {
                sb.Append($"\n{++exceptionCount}:{innerException.Message}");
                innerException = innerException.InnerException;
            }

            return sb.ToString();
        }
    }
}
