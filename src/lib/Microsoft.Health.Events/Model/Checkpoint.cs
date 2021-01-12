// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;

namespace Microsoft.Health.Events.Model
{
    public class Checkpoint
    {
        public string Prefix { get; set; }

        public string Id { get; set; }

        public DateTimeOffset LastProcessed { get; set; }
    }
}
