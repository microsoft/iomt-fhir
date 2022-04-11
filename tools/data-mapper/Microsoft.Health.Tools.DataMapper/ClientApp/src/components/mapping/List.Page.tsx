// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

import * as React from 'react';
import { connect } from 'react-redux';
import { RouteComponentProps } from 'react-router';

import { ApplicationState } from '../../store';
import * as MappingsStore from '../../store/Mapping';
import { default as MappingNameModal, Action } from './List.Modals.Name';
import MappingExportModal from './List.Modals.Export';
import MappingImportModal from './List.Modals.Import';
import PersistService from '../../services/PersistService';
import { Mapping } from '../../store/Mapping';
import * as Constants from '../Constants';

import './List.css';

type MappingListProps =
    MappingsStore.MappingListStoreState &
    typeof MappingsStore.ActionCreators &
    RouteComponentProps<{}>;

class MappingListPage extends React.PureComponent<MappingListProps> {

    public componentDidMount() {
        this.ensureMappingsFetched = this.ensureMappingsFetched.bind(this);
        this.createMapping = this.createMapping.bind(this);
        this.renameMapping = this.renameMapping.bind(this);
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
                window.location.href = `${Constants.Text.PathMappings}${newMapping.id}`
            })
            .catch(err => {
                if (errorHandler) {
                    errorHandler(err);
                }
            });
    }

    private renameMapping(id: string, typename: string, errorHandler?: Function, setModal?: Function) {
        PersistService.renameMapping(id, typename)
            .then(() => {
                this.ensureMappingsFetched();
                if (setModal) {
                    setModal(false);
                }
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
                    <MappingNameModal
                        onSave={this.createMapping}
                        action={Action.Create}
                    />
                </div>
                <div className="d-inline-block m-1 mb-3">
                    <MappingImportModal
                        onImported={this.ensureMappingsFetched}
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
                                            <MappingNameModal
                                                onSave={(typename: string, errorHandler: Function, setModal: Function) => this.renameMapping(mapping.id, typename, errorHandler, setModal)}
                                                action={Action.Rename}
                                                inputDefaultValue={mapping.typeName}
                                                buttonClassName="m-1 btn iomt-cm-btn-link"
                                            />
                                            <button className="m-1 btn iomt-cm-btn-link"
                                                onClick={() => { window.location.href = `${Constants.Text.PathMappings}${mapping.id}` }}>
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
