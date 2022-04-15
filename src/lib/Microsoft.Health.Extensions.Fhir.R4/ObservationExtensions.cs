// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using EnsureThat;
using Hl7.Fhir.Model;

namespace Microsoft.Health.Extensions.Fhir
{
    public static class ObservationExtensions
    {
        /// <summary>
        /// Compares two observations to see if they are different.  If they are different the <paramref name="updatedObservation"/> status is changed to Amended.
        /// </summary>
        /// <param name="originalObservation">The original unmodified observation.</param>
        /// <param name="updatedObservation">The potentially modified observation.</param>
        /// <returns>Returns true if the <paramref name="updatedObservation"/> is different than the <paramref name="originalObservation"/>.  Otherwise false is returned.</returns>
        public static bool AmendIfChanged(this Observation originalObservation, Observation updatedObservation)
        {
            EnsureArg.IsNotNull(originalObservation, nameof(originalObservation));
            EnsureArg.IsNotNull(updatedObservation, nameof(updatedObservation));
            EnsureArg.IsFalse(originalObservation == updatedObservation, optsFn: o => o.WithMessage($"Parameters {nameof(originalObservation)} and {nameof(updatedObservation)} are the same reference."));

            if (!originalObservation.IsExactly(updatedObservation))
            {
                updatedObservation.Status = ObservationStatus.Amended;
                return true;
            }

            return false;
        }
    }
}
