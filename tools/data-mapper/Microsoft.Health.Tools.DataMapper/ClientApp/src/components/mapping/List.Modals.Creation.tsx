// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

import * as React from 'react';
import { FormGroup, Label, Input, Modal, ModalHeader, ModalBody, ModalFooter, Form, Col } from 'reactstrap';

export enum Action {
    Create
}

const MappingCreationModal = (props: { onSave: Function; action: Action }) => {
    const {
        onSave, action
    } = props;

    const [typename, setTypename] = React.useState('');
    const [typenameError, setTypenameError] = React.useState('');
    const [modal, setModal] = React.useState(false);
    const toggle = () => setModal(!modal);

    const modalTitleText = () => {
        switch (action) {
            case Action.Create:
                return "Create new mapping";
            default:
                return "";
        }
    }

    const buttonText = () => {
        switch (action) {
            case Action.Create:
                return "Add new mapping";
            default:
                return "";
        }
    }

    const renderModal = () => {
        return (
            <Modal isOpen={modal} toggle={toggle}>
                <ModalHeader toggle={toggle}>
                    {modalTitleText()}
                </ModalHeader>
                <ModalBody>
                    <Form>
                        <FormGroup row>
                            <Label for="typename" sm={3}>Type Name</Label>
                            <Col sm={9}>
                                <Input type="text" name="typename" id="typename"
                                    placeholder="input a unique type name"
                                    onChange={e => { setTypename(e.target.value) }}
                                />
                                <span>{typenameError}</span>
                            </Col>
                        </FormGroup>
                    </Form>
                </ModalBody>
                <ModalFooter>
                    <button className="btn btn-primary" onClick={() => onSave(typename, setTypenameError)}>Confirm</button>{' '}
                    <button className="btn btn-secondary" onClick={toggle}>Cancel</button>
                </ModalFooter>
            </Modal>
        );
    }

    return (
        <div>
            <button className="btn iomt-cm-btn" onClick={toggle}>
                {buttonText()}
            </button>
            {renderModal()}
        </div>
    );
}

export default MappingCreationModal;