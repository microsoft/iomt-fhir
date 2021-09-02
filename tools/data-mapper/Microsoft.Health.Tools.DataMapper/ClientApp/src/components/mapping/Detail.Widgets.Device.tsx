// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

import * as React from 'react';
import { Row, Col } from 'reactstrap';

import { DeviceMapping, Mapping } from '../../store/Mapping';
import DeviceEditForm from './Detail.Forms.Device';
import { DeviceEditFormTutorial } from './Detail.Forms.Tutorials';

import * as Constants from './../Constants';

const DeviceMappingWidget = (props: { data: Mapping; onUpdate: Function}) => {

    const DeviceForms = [
        {
            title: Constants.Text.LabelDeviceMapping,
            component: (
                <DeviceEditForm
                    data={props.data?.device || {} as DeviceMapping}
                    onUpdate={(updatedDeviceMapping: DeviceMapping) => {
                        const updatedMapping = {
                            ...props.data,
                            device: updatedDeviceMapping
                        }
                        props.onUpdate(updatedMapping);
                    }}
                />
            ),
            tutorial: (
                <DeviceEditFormTutorial />
            ),
        }
    ];

    return (
        <React.Fragment>
            <div className="iomt-cm-editor-title p-2 pb-3 m-0 border border-bottom-0">
                <span className="h5">Device Mapping</span>
            </div>
            <div className="iomt-cm-editor p-3 border">
                {
                    DeviceForms.map((form, index) => {
                        return (
                            <Row key={`form-${form.title}`}>
                                <Col sm="6"> {form.component} </Col>
                                <Col sm="6"> {form.tutorial} </Col>
                            </Row>
                        );
                    })
                }
            </div>
        </React.Fragment>
    );
}

export default DeviceMappingWidget;
