// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

import * as React from 'react';
import { Row, Col } from 'reactstrap';
import { connect } from 'react-redux';
import { RouteComponentProps } from 'react-router';

import { ApplicationState } from '../../store';
import { Mapping, MappingDetailStoreState, ActionCreators } from '../../store/Mapping';
import DeviceMappingWidget from './Detail.Widgets.Device';
import FhirMappingWidget from './Detail.Widgets.Fhir';
import MappingTestWidget from './Detail.Widgets.Test';

import './Detail.css';

type MappingDetailProps =
    MappingDetailStoreState &
    typeof ActionCreators &
    RouteComponentProps<{ id: string }>;

const MappingDetailEditContainer = (props: { data: Mapping, onSave: Function }) => {
    const [currentMapping, setCurrentMapping] = React.useState(props.data);

    return (
        <React.Fragment >
            <Row className="mt-3 mb-2">
                <Col>
                    <DeviceMappingWidget data={currentMapping}
                        onUpdate={(updatedMapping: Mapping) => setCurrentMapping(updatedMapping)} />
                </Col>
                <Col>
                    <FhirMappingWidget data={currentMapping}
                        onUpdate={(updatedMapping: Mapping) => setCurrentMapping(updatedMapping)} />
                </Col>
            </Row>
            <Row className="mt-3 mb-2">
                <Col>
                    <MappingTestWidget data={currentMapping} />
                </Col>
            </Row>
            <Row className="pt-2 pb-2 mt-2">
                <Col>
                    <button className="btn btn-success pl-5 pr-5"
                        onClick={() => props.onSave(currentMapping.id, currentMapping)}>
                        Save
                    </button>
                    <button className="btn btn-secondary ml-2 pl-5 pr-5"
                        onClick={() => { window.location.href = "/"; }}>
                        Return
                    </button>
                </Col>
            </Row>
        </React.Fragment>
    );
}

class MappingDetailPage extends React.PureComponent<MappingDetailProps> {

    public componentDidMount() {
        if (!this.props.hasLoaded) {
            const mappingId = this.props.match.params.id;
            this.props.getMapping(mappingId);
        }
    }

    public render() {
        if (!this.props.hasLoaded) {
            return <></>;
        }
        return (
            <MappingDetailEditContainer
                data={this.props.mapping}
                onSave={this.props.saveMapping} />
        );
    }
}

export default connect(
    (state: ApplicationState) => state.mappingDetail,
    ActionCreators
)(MappingDetailPage);
