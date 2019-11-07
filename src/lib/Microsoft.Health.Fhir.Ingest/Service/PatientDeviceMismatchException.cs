﻿// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;

namespace Microsoft.Health.Fhir.Ingest.Service
{
    public class PatientDeviceMismatchException : Exception
    {
        public PatientDeviceMismatchException(string message)
            : base(message)
        {
        }

        public PatientDeviceMismatchException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public PatientDeviceMismatchException()
        {
        }
    }
}
