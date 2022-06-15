// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Health.Common.Errors
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
        /// <param name="exception">An Exception.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns><see cref="Task"/>Returns the successfully stored error message.</returns>
        Task ReportError(Exception exception, CancellationToken cancellationToken);

        /// <summary>
        /// Writes an IEnumerable of ErrorMessage to a destination.
        /// </summary>
        /// <remarks>
        /// The ErrorMessage Type property will be the ExceptionType.
        /// and Details property will be the Exception Message.
        /// </remarks>
        /// <param name="exceptions">A collection of exceptions.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns><see cref="Task"/>Returns the successfully stored error message.</returns>
        Task<IEnumerable<ErrorMessage>> ReportError(IEnumerable<Exception> exceptions, CancellationToken cancellationToken);
    }
}
