﻿// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using Microsoft.Health.Common.Telemetry;
using Microsoft.Health.Common.Telemetry.Exceptions;

namespace Microsoft.Health.Fhir.Ingest.Service
{
    public class InvalidDataFormatException : ThirdPartyLoggedFormattableException
    {
        public InvalidDataFormatException()
            : base()
        {
        }

        public InvalidDataFormatException(string message)
            : base(message)
        {
        }

        public InvalidDataFormatException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public override string ErrName => nameof(InvalidDataFormatException);

        public override string ErrType => ErrorType.DeviceMessageError;

        public override string ErrSource => nameof(ErrorSource.User);
    }
}
