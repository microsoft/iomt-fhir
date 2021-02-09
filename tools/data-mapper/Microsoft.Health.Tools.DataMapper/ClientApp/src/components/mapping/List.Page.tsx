﻿// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

import * as React from 'react';
import { connect } from 'react-redux';
import { RouteComponentProps } from 'react-router';

import { ApplicationState } from '../../store';
import * as MappingsStore from '../../store/Mapping';
import MappingCreationModal from './List.Modals.Creation';
import MappingExportModal from './List.Modals.Export';
import PersistService from '../../services/PersistService';
import { Mapping } from '../../store/Mapping';

import './List.css';

type MappingListProps =
    MappingsStore.MappingListStoreState &
    typeof MappingsStore.ActionCreators &
    RouteComponentProps<{}>;

class MappingListPage extends React.PureComponent<MappingListProps> {

    public componentDidMount() {
        this.createMapping = this.createMapping.bind(this);
        this.ensureMappingsFetched();
    }

    public render() {
        return (
            <div className="container mt-5">
                <span className="h1 mb-2">Mappings</span>
                <div className="float-right">
                    {this.renderMappingsToolbar()}
                </div>
                {this.renderMappingsTable()}
            </div>
        );
    }

    private ensureMappingsFetched() {
        this.props.listMappings();
    }

    private createMapping(typename: string, errorHandler?: Function) {
        PersistService.createMapping(typename)
            .then((newMapping: Mapping) => {
                window.location.href = `/mappings/${newMapping.id}`
            })
            .catch(err => {
                if (errorHandler) {
                    errorHandler(err);
                }
            });
    }

    private renderMappingsToolbar() {
        return (
            <React.Fragment>
                <div className="d-inline-block m-1 mb-3">
                    <MappingCreationModal
                        onSave={this.createMapping}
                    />
                </div>
                <div className="d-inline-block m-1 mb-3">
                    <MappingExportModal
                        mappings={this.props.mappings}
                    />
                </div>
            </React.Fragment>
        );
    }

    private renderMappingsTable() {
        const hasContent = this.props.mappings && this.props.mappings.length > 0;
        return (
            <React.Fragment>
                <table className='table table-striped' aria-labelledby="tabelLabel">
                    <thead>
                        <tr>
                            <th className="w-25">Type Name</th>
                            <th></th>
                        </tr>
                    </thead>
                    <tbody>
                        {
                            hasContent &&
                            this.props.mappings.map((mapping, index) => {
                                return (
                                    <tr key={index}>
                                        <td>{mapping.typeName}</td>
                                        <td className="text-right">
                                            <button className="m-1 btn iomt-cm-btn-link"
                                                onClick={() => { window.location.href = `/mappings/${mapping.id}` }}>
                                                Edit
                                                </button>
                                            <button className="m-1 btn iomt-cm-btn-link"
                                                onClick={() => { this.props.deleteMapping(mapping.id); this.props.listMappings() }}>
                                                Delete
                                                </button>
                                        </td>
                                    </tr>
                                );
                            })
                        }
                    </tbody>
                </table>
                {
                    !hasContent &&
                    (
                        <React.Fragment>
                            You don't have any mappings.
                        </React.Fragment>
                    )
                }

            </React.Fragment >
        );
    }
}

export default connect(
    (state: ApplicationState) => state.mappingList,
    MappingsStore.ActionCreators
)(MappingListPage);
