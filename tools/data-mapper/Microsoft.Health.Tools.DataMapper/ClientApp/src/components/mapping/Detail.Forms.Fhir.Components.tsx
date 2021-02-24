// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

import * as React from 'react';
import { Label, Card, CardBody, CardTitle } from 'reactstrap';

import { Mapping, FhirComponent, FhirValue, FhirCoding } from "../../store/Mapping";
import FhirValueForm from './Detail.Forms.Fhir.Value';
import FhirCodesForm from './Detail.Forms.Fhir.Codes';
import * as Utility from './Utility';
import * as Constants from '../Constants';
import { DeleteIcon } from '../Icons';

const FhirComponentForm = (props: { data: FhirComponent; context: Mapping; onUpdate: Function; modifier?: string }) => {
    return (
        <React.Fragment>
            <FhirValueForm
                data={props.data.value || {} as FhirValue}
                context={props.context}
                onUpdate={(updatedValue: FhirValue) => {
                    const updatedComponent = {
                        ...props.data,
                        value: updatedValue,
                    } as FhirComponent;
                    props.onUpdate(updatedComponent);
                }}
                modifier={props.modifier}
            />
            <FhirCodesForm
                data={props.data.codes || [] as FhirCoding[]}
                context={props.context}
                onUpdate={(updatedCodes: FhirCoding[]) => {
                    const updatedComponent = {
                        ...props.data,
                        codes: updatedCodes,
                    } as FhirComponent;
                    props.onUpdate(updatedComponent);
                }}
                modifier={props.modifier}
            />
        </React.Fragment>
    )
}

const FhirComponentsForm = (props: { data: FhirComponent[]; context: Mapping; onUpdate: Function }) => {
    // Fix the display of input array.
    const [keyModifier, refreshKeyModifier] = React.useState(Utility.getRandomString());

    const appendComponent = () => {
        const updatedInnerComponents = [...props.data, {} as FhirComponent];
        refreshKeyModifier(Utility.getRandomString());
        props.onUpdate(updatedInnerComponents);
    }

    const removeComponent = (removeIndex: number) => {
        const updatedInnerComponents = [...props.data.filter((_, index) => index !== removeIndex)];
        refreshKeyModifier(Utility.getRandomString());
        props.onUpdate(updatedInnerComponents);
    }

    const renderComponent = (component: FhirComponent, index: number) => {
        const keySuffix = `-${index}-${keyModifier}`;
        return (
            <Card className="mb-3" key={`fhirComponent-${keySuffix}`}>
                <CardTitle className="p-2 border-bottom">
                    Component
                    <button className="btn iomt-cm-btn-link iomt-cm-right p-0" onClick={() => removeComponent(index)}>
                        <DeleteIcon />
                    </button>
                </CardTitle>
                <CardBody>
                    <FhirComponentForm
                        data={component}
                        context={props.context}
                        onUpdate={(updatedComponent: FhirComponent) => {
                            const updatedComponents = [...props.data];
                            updatedComponents[index] = updatedComponent;
                            props.onUpdate(updatedComponents);
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
                <Label>{Constants.Text.LabelFhirComponents}</Label>
                <button className="btn iomt-cm-btn-link" onClick={appendComponent}> + Add </button>
            </div>
            {props.data.map(renderComponent)}
        </React.Fragment>
    )
}

export default FhirComponentsForm;