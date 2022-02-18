// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

import * as React from 'react';
import { Alert, Modal, ModalHeader, ModalBody, ModalFooter, Col, Row } from 'reactstrap';

import PersistService from '../../services/PersistService';
import JsonValidator from '../JsonValidator';

const MappingImportModal = (props: { onImported: Function }) => {
    const [modal, setModal] = React.useState(false);
    const [deviceTemplateContent, setDeviceTemplateContent] = React.useState('');
    const [fhirTemplateContent, setFhirTemplateContent] = React.useState('');
    const [importError, setImportError] = React.useState('');
    const [showImportError, setShowImportError] = React.useState(false);

    React.useEffect(() => {
        if (modal) {
            setDeviceTemplateContent('');
            setFhirTemplateContent('');
            setImportError('');
            setShowImportError(false);
        }
    }, [modal]);

    const toggle = () => setModal(!modal);

    const onDismissImportError = () => setShowImportError(false);

    const onConfirm = () => {
        PersistService.createMappingsFromTemplates(deviceTemplateContent, fhirTemplateContent)
            .then(() => {
                props.onImported();
                setImportError('');
                setShowImportError(false);
                setModal(false);
            })
            .catch(err => {
                setImportError(`Import failed. ${err}`);
                setShowImportError(true);
            });
    }

    const renderModal = () => {
        return (
            <Modal isOpen={modal} toggle={toggle} className="iomt-cm-import-modal">
                <ModalHeader toggle={toggle}>Enter mapping templates to import</ModalHeader>
                <ModalBody>
                    <div className="iomt-cm-modal-description">
                        Full example templates can be found under <a href="https://github.com/microsoft/iomt-fhir/tree/main/sample/templates">here</a>.
                    </div>
                    <Row>
                        {
                            <Col>
                                <h6>Device Mapping Template</h6>
                                <JsonValidator
                                    onTextChange={setDeviceTemplateContent}
                                    placeholder='device mapping template'
                                />
                            </Col>
                        }
                        {
                            <Col>
                                <h6>FHIR Mapping Template</h6>
                                <JsonValidator
                                    onTextChange={setFhirTemplateContent}
                                    placeholder='FHIR mapping template'
                                />
                            </Col>
                        }
                    </Row>
                    <Alert color="danger" isOpen={showImportError} toggle={onDismissImportError}>
                        {importError}
                    </Alert>
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
            <button className="btn iomt-cm-btn" onClick={toggle}>
                Import mappings
            </button>
            {renderModal()}
        </span>
    );
}

export default MappingImportModal;
