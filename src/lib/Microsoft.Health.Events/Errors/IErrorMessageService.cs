// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;

namespace Microsoft.Health.Events.Errors
{
    public interface IErrorMessageService
    {
        /// <summary>
        /// Writes an ErrorMessage to a destination
        /// </summary>
        /// <remarks>
        /// The ErrorMessage Type property will be the ExceptionType.
        /// and Details property will be the Exception Message.
        /// </remarks>
        /// <param name="errorMessage">An ErrorMessage.</param>
        void ReportError(ErrorMessage errorMessage);

        /// <summary>
        /// Writes an IEnumerable of ErrorMessage to a destination.
        /// </summary>
        /// <remarks>
        /// The ErrorMessage Type property will be the ExceptionType.
        /// and Details property will be the Exception Message.
        /// </remarks>
        /// <param name="errorMessages">A collection of ErrorMessages.</param>
        void ReportError(IEnumerable<ErrorMessage> errorMessages);
    }
}
