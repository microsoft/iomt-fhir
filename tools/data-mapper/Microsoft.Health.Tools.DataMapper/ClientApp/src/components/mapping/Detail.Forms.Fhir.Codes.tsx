// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

import * as React from 'react';
import { FormGroup, Label, Input, Card, CardBody, CardTitle } from 'reactstrap';

import { FhirCoding, Mapping } from "../../store/Mapping"
import * as Utility from './Utility';
import * as Constants from '../Constants';
import { DeleteIcon } from '../Icons';

const FhirCodingCodeGroup = (props: { data: string; onUpdate: Function; modifier?: string }) => {
    return (
        <FormGroup>
            <Label for={`fhirCodingCode-${props.modifier}`}>{Constants.Text.LabelFhirCode}</Label>
            <Input type="text" name={`fhirCodingCode-${props.modifier}`} id={`fhirCodingCode-${props.modifier}`}
                value={props.data} onChange={(e) => props.onUpdate(e.target.value)}
            />
        </FormGroup>
    );
}

const FhirCodingSystemGroup = (props: { data: string; onUpdate: Function; modifier?: string }) => {
    return (
        <FormGroup>
            <Label for={`fhirCodingSystem-${props.modifier}`}>{Constants.Text.LabelFhirSystem}</Label>
            <Input type="text" name={`fhirCodingSystem-${props.modifier}`} id={`fhirCodingSystem-${props.modifier}`}
                value={props.data} onChange={(e) => props.onUpdate(e.target.value)}
            />
        </FormGroup>
    );
}

const FhirCodingDisplayGroup = (props: { data: string; onUpdate: Function; modifier?: string }) => {
    return (
        <FormGroup>
            <Label for={`fhirCodingDisplay-${props.modifier}`}>{Constants.Text.LabelFhirDisplay}</Label>
            <Input type="text" name={`fhirCodingDisplay-${props.modifier}`} id={`fhirCodingDisplay-${props.modifier}`}
                value={props.data} onChange={(e) => props.onUpdate(e.target.value)}
            />
        </FormGroup>
    );
}

const FhirCondingGroup = (props: { data: FhirCoding; context: Mapping; onUpdate: Function; modifier?: string }) => {
    return (
        <React.Fragment>
            <FhirCodingCodeGroup
                data={props.data.code}
                onUpdate={(updatedCode: string) => {
                    const updatedCoding = {
                        ...props.data,
                        code: updatedCode
                    } as FhirCoding;
                    props.onUpdate(updatedCoding);
                }}
                modifier={props.modifier}
            />
            <FhirCodingSystemGroup
                data={props.data.system}
                onUpdate={(updatedSystem: string) => {
                    const updatedCoding = {
                        ...props.data,
                        system: updatedSystem
                    } as FhirCoding;
                    props.onUpdate(updatedCoding);
                }}
                modifier={props.modifier}
            />
            <FhirCodingDisplayGroup
                data={props.data.display}
                onUpdate={(updatedDisplay: string) => {
                    const updatedCoding = {
                        ...props.data,
                        display: updatedDisplay
                    } as FhirCoding;
                    props.onUpdate(updatedCoding);
                }}
                modifier={props.modifier}
            />
        </React.Fragment>
    );
}

const FhirCodesForm = (props: { data: FhirCoding[]; context: Mapping; onUpdate: Function; modifier?: string }) => {
    // Fix the display of input array.
    const [keyModifier, refreshKeyModifier] = React.useState(Utility.getRandomString());

    const appendCoding = () => {
        const updatedInnerCodes = [...props.data, {} as FhirCoding];
        refreshKeyModifier(Utility.getRandomString());
        props.onUpdate(updatedInnerCodes);
    }

    const removeCoding = (removeIndex: number) => {
        const updatedInnerCodes = [...props.data.filter((_, index) => index !== removeIndex)];
        refreshKeyModifier(Utility.getRandomString());
        props.onUpdate(updatedInnerCodes);
    }

    const renderCoding = (coding: FhirCoding, index: number) => {
        const keySuffix = `-${props.modifier ?? ''}-${index}-${keyModifier}`;
        return (
            <Card className="mb-3" key={`fhirCoding-${keySuffix}`}>
                <CardTitle className="p-2 border-bottom">
                    Coding
                    <button className="btn iomt-cm-btn-link iomt-cm-right p-0" onClick={() => removeCoding(index)}>
                        <DeleteIcon />
                    </button>
                </CardTitle>
                <CardBody>
                    <FhirCondingGroup
                        data={coding}
                        context={props.context}
                        onUpdate={(updatedCoding: FhirCoding) => {
                            const updatedCodes = [...props.data];
                            updatedCodes[index] = updatedCoding;
                            props.onUpdate(updatedCodes);
                        }}
                        modifier={keySuffix}
                    />
                </CardBody>
            </Card>
        );
    }

    return (
        <React.Fragment>
            <div className="position-relative">
                <Label>{Constants.Text.LabelFhirCodes}</Label>
                <button className="btn iomt-cm-btn-link" onClick={appendCoding}> + Add </button>
            </div>
            {props.data.map(renderCoding)}
        </React.Fragment>
    )
}

export default FhirCodesForm;