// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

import * as React from 'react';
import { FormGroup, Label, Input, Card, CardBody, CardTitle } from 'reactstrap';

import { Mapping, FhirCoding, FhirCodeableConcept } from "../../store/Mapping";
import FhirCodesForm from './Detail.Forms.Fhir.Codes';
import * as Utility from './Utility';
import * as Constants from '../Constants';
import { DeleteIcon } from '../Icons';

const FhirCodeableConceptTextGroup = (props: { data: string; context: Mapping; onUpdate: Function; modifier?: string }) => {
    return (
        <FormGroup>
            <Label for={`fhirCodeableConceptText-${props.modifier}`}>{Constants.Text.LabelFhirCategoryText}</Label>
            <Input type="text" name={`fhirCodeableConceptText-${props.modifier}`} id={`fhirCodeableConceptText-${props.modifier}`}
                value={props.data} onChange={(e) => props.onUpdate(e.target.value)}
            />
        </FormGroup>
    );
}

const FhirCodeableConceptGroup = (props: { data: FhirCodeableConcept; context: Mapping; onUpdate: Function; modifier?: string }) => {
    return (
        <React.Fragment>
            <FhirCodeableConceptTextGroup
                data={props.data.text || ''}
                context={props.context}
                onUpdate={(updatedValue: string) => {
                    const updatedCodeableConcept = {
                        ...props.data,
                        text: updatedValue,
                    } as FhirCodeableConcept;
                    props.onUpdate(updatedCodeableConcept);
                }}
                modifier={props.modifier}
            />
            <FhirCodesForm
                data={props.data.codes || [] as FhirCoding[]}
                context={props.context}
                onUpdate={(updatedValue: FhirCoding[]) => {
                    const updatedCodeableConcept = {
                        ...props.data,
                        codes: updatedValue,
                    } as FhirCodeableConcept;
                    props.onUpdate(updatedCodeableConcept);
                }}
                modifier={props.modifier}
            />
        </React.Fragment>
    )
}

const FhirCategoryForm = (props: { data: FhirCodeableConcept[]; context: Mapping; onUpdate: Function }) => {
    // Fix the display of input array.
    const [keyModifier, refreshKeyModifier] = React.useState(Utility.getRandomString());

    const appendCodeableConcept = () => {
        const updatedInnerCategory = [...props.data, {} as FhirCodeableConcept];
        refreshKeyModifier(Utility.getRandomString());
        props.onUpdate(updatedInnerCategory);
    }

    const removeCodeableConcept = (removeIndex: number) => {
        const updatedInnerCategory = props.data.filter((_, index) => index !== removeIndex);
        refreshKeyModifier(Utility.getRandomString());
        props.onUpdate(updatedInnerCategory);
    }

    const renderCodeableConcept = (codeableConcept: FhirCodeableConcept, index: number) => {
        const keySuffix = `-${index}-${keyModifier}`;
        return (
            <Card className="mb-3" key={`fhirCodeableConcept-${keySuffix}`}>
                <CardTitle className="p-2 border-bottom">
                    CodeableConcept
                    <button className="btn iomt-cm-btn-link iomt-cm-right p-0" onClick={() => removeCodeableConcept(index)}>
                        <DeleteIcon />
                    </button>
                </CardTitle>
                <CardBody>
                    <FhirCodeableConceptGroup
                        data={codeableConcept}
                        context={props.context}
                        onUpdate={(updatedCodeableConcept: FhirCodeableConcept) => {
                            const updatedCategory = props.data;
                            updatedCategory[index] = updatedCodeableConcept;
                            props.onUpdate(updatedCategory);
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
                <Label>{Constants.Text.LabelFhirCategory}</Label>
                <button className="btn iomt-cm-btn-link" onClick={appendCodeableConcept}> + Add </button>
            </div>
            {props.data.map(renderCodeableConcept)}
        </React.Fragment>
    )
}

export default FhirCategoryForm;
