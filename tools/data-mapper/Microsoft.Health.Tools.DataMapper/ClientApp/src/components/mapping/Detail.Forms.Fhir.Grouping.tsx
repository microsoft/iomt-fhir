// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

import * as React from 'react';
import { Label, FormGroup, Input } from 'reactstrap';

import * as Constants from '../Constants';

interface GroupingOption {
    displayText: string,
    value: number,
}

const GroupingOptionsConfig = [
    {
        displayText: "No Grouping",
        value: 0
    },
    {
        displayText: "Correlation ID Grouping",
        value: -1
    },
    {
        displayText: "1 Hour Grouping",
        value: 60
    },
    {
        displayText: "1 Day Grouping",
        value: 1440
    }
] as GroupingOption[];

const FhirGroupingGroup = (props: { data: number; onUpdate: Function }) => {
    return (
        <FormGroup>
            <Label for="fhirGrouping">{Constants.Text.LabelFhirGroupingWindow}</Label>
            <Input type="select" name="fhirGrouping" id="fhirGrouping"
                defaultValue={props.data} onChange={(e) => props.onUpdate(e.target.value)}
            >
                <option disabled></option>
                {
                    GroupingOptionsConfig.map( (option, index) => {
                        return <option value={option.value} key={`option-${index}`}>
                            {option.displayText}
                        </option>;
                    })
                }
            </Input>
        </FormGroup>
    )
}

export default FhirGroupingGroup;