// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

import * as React from 'react';
import { Label, Input, Modal, ModalHeader, ModalBody, ModalFooter, Col, Row } from 'reactstrap';

import { Mapping } from '../../store/Mapping';
import * as Utils from '../../services/Utils';

const MappingExportModal = (props: { mappings: Mapping[] }) => {
    const mappingsToExport = new Map<string, Mapping>();
    const [modal, setModal] = React.useState(false);
    const [deviceMappingContent, setDeviceMappingContent] = React.useState("");
    const [fhirMappingContent, setFhirMappingContent] = React.useState("");
    const toggle = () => {
        setModal(!modal);
        setDeviceMappingContent("");
        setFhirMappingContent("");
    }

    const onConfirm = () => {
        const mappings = [] as Mapping[];
        mappingsToExport.forEach((m) => {
            mappings.push(m);
        });
        setDeviceMappingContent(Utils.generateDeviceTemplate(mappings));
        setFhirMappingContent(Utils.generateFhirTemplate(mappings));
    }

    const renderModal = () => {
        return (
            <Modal isOpen={modal} toggle={toggle} className="iomt-cm-export-modal">
                <ModalHeader toggle={toggle}>Choose mappings to export</ModalHeader>
                <ModalBody>
                    {
                        props.mappings !== undefined &&
                        props.mappings.map((m, index) => {
                            return (
                                <div className="pl-4" key={`mappingToExport-${index}`}>
                                    <Input type="checkbox" name={`mappingToExport-${index}`} id={`mappingToExport-${index}`}
                                        onChange={e => e.target.checked ? mappingsToExport.set(m.id, m) : mappingsToExport.delete(m.id)} />
                                    <Label for={`mappingToExport-${index}`} className="ml-2">{m.typeName}</Label>
                                </div>
                            )
                        })
                    }
                    <Row>
                        {
                            deviceMappingContent &&
                            <Col className="border-top pt-2">
                                <h6>Device Mapping</h6>
                                <pre>{deviceMappingContent}</pre>
                            </Col>
                        }
                        {
                            fhirMappingContent &&
                            <Col className="border-top pt-2">
                                <h6>FHIR Mapping</h6>
                                <pre>{fhirMappingContent}</pre>
                            </Col>
                        }
                    </Row>
                </ModalBody>
                <ModalFooter>
                    <button className="btn btn-primary" onClick={onConfirm}>Generate</button>{' '}
                    <button className="btn btn-secondary" onClick={toggle}>Close</button>
                </ModalFooter>
            </Modal>
        );
    }

    return (
        <span>
            <button className="btn iomt-cm-btn" onClick={toggle}
                disabled={props.mappings?.length === 0}>Export mappings</button>
            {renderModal()}
        </span>
    );
}

export default MappingExportModal;