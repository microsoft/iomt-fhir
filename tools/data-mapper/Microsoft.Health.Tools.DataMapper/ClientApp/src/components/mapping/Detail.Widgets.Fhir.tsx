// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

import * as React from 'react';
import { Row, Col } from 'reactstrap';

import {
    Mapping, FhirValue, FhirMapping, FhirComponent, FhirCoding, FhirCodeableConcept
} from '../../store/Mapping';

import FhirValueForm from './Detail.Forms.Fhir.Value';
import FhirCodesForm from './Detail.Forms.Fhir.Codes';
import FhirComponentsForm from './Detail.Forms.Fhir.Components';
import FhirCategoryForm from './Detail.Forms.Fhir.Category';
import FhirGroupingGroup from './Detail.Forms.Fhir.Grouping';
import {
    FhirValueFormTutorial, FhirComponentsFormTutorial, FhirCodesFormTutorial, FhirCategoryFormTutorial, FhirGroupingGroupTutorial
} from './Detail.Forms.Tutorials';

import * as Constants from './../Constants';

const FhirMappingWidget = (props: { data: Mapping; onUpdate: Function }) => {
    const fhirForms = [
        {
            title: Constants.Text.LabelFhirValueTitle,
            component: (
                <FhirValueForm
                    data={props.data.fhir?.value || {} as FhirValue}
                    context={props.data}
                    onUpdate={(updatedFhirValue: FhirValue) => {
                        const updatedMapping = {
                            ...props.data,
                            fhir: props.data.fhir || {} as FhirMapping
                        };
                        updatedMapping.fhir.value = updatedFhirValue;
                        props.onUpdate(updatedMapping);
                    }} />
            ),
            tutorial: (
                <FhirValueFormTutorial />
            )
        },
        {
            title: Constants.Text.LabelFhirComponentsTitle,
            component: (
                <FhirComponentsForm
                    data={props.data.fhir?.components || [] as FhirComponent[]}
                    context={props.data}
                    onUpdate={(updatedFhirComponents: FhirComponent[]) => {
                        const updatedMapping = {
                            ...props.data,
                            fhir: props.data.fhir || {} as FhirMapping
                        };
                        updatedMapping.fhir.components = updatedFhirComponents;
                        props.onUpdate(updatedMapping);
                    }}
                />
            ),
            tutorial: (
                <FhirComponentsFormTutorial />
            )
        },
        {
            title: Constants.Text.LabelFhirCodesTitle,
            component: (
                <FhirCodesForm
                    data={props.data.fhir?.codes || [] as FhirCoding[]}
                    context={props.data}
                    onUpdate={(updatedFhirCodes: FhirCoding[]) => {
                        const updatedMapping = {
                            ...props.data,
                            fhir: props.data.fhir || {} as FhirMapping
                        };
                        updatedMapping.fhir.codes = updatedFhirCodes;
                        props.onUpdate(updatedMapping);
                    }}
                />
            ),
            tutorial: (
                <FhirCodesFormTutorial />
            ),
        },
        {
            title: Constants.Text.LabelFhirCategoryTitle,
            component: (
                <FhirCategoryForm
                    data={props.data.fhir?.category || [] as FhirCodeableConcept[]}
                    context={props.data}
                    onUpdate={(updatedFhirCategory: FhirCodeableConcept[]) => {
                        const updatedMapping = {
                            ...props.data,
                            fhir: props.data.fhir || {} as FhirMapping
                        };
                        updatedMapping.fhir.category = updatedFhirCategory;
                        props.onUpdate(updatedMapping);
                    }}
                />
            ),
            tutorial: (
                <FhirCategoryFormTutorial />
            ),
        },
        {
            title: Constants.Text.LabelFhirGroupingTitle,
            component: (
                <FhirGroupingGroup
                    data={props.data.fhir?.periodInterval}
                    onUpdate={(updatedPeriodInterval: number) => {
                        const updatedMapping = {
                            ...props.data,
                            fhir: props.data.fhir || {} as FhirMapping
                        };
                        updatedMapping.fhir.periodInterval = updatedPeriodInterval;
                        props.onUpdate(updatedMapping);
                    }}
                />
            ),
            tutorial: (
                <FhirGroupingGroupTutorial />
            ),
        },
    ];

    return (
        <React.Fragment>
            <div className="iomt-cm-editor-title p-2 pb-3 m-0 border border-bottom-0">
                <span className="h5">FHIR Mapping</span>
            </div>
            <div className="iomt-cm-editor border">
                {
                    fhirForms.map((form, index) => {
                        return (
                            <React.Fragment key={`form-${form.title}`}>
                                <div className="pl-4 pr-4 pt-4 h6">{form.title}</div>
                                <Row className="p-4 mb-2 border-bottom">
                                    <Col sm="6"> {form.component} </Col>
                                    <Col sm="6"> {form.tutorial} </Col>
                                </Row>
                            </React.Fragment>
                        );
                    })
                }
            </div>
        </React.Fragment>
    );
}

export default FhirMappingWidget;