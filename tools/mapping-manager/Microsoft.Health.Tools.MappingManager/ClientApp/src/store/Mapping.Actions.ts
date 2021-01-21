// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

import { Mapping } from "./Mapping.States";
import { AppThunkAction } from ".";

import PersistService from "../services/PersistService";

// -----------------
// ACTIONS - These are serializable (hence replayable) descriptions of state transitions.
// They do not themselves have any side-effects; they just describe something that is going to happen.

export interface CreateMappingAction {
    type: 'CREATE_MAPPING';
    payload: Mapping;
}

export interface GetMappingAction {
    type: 'GET_MAPPING';
    payload: Mapping;
}

export interface ListMappingsAction {
    type: 'LIST_MAPPINGS';
    payload: Mapping[];
}

export interface SaveMappingAction {
    type: 'SAVE_MAPPING';
    payload: Mapping;
}

export interface DeleteMappingAction {
    type: 'DELETE_MAPPING';
}

export type KnownAction = ListMappingsAction | GetMappingAction | SaveMappingAction | DeleteMappingAction;

const listMappings = (): AppThunkAction<KnownAction> => (dispatch) => {
    // This is sync call. In future, this maybe an async call with 
    // Restful APIs.
    const mappings = PersistService.getAllMappings();
    dispatch({
        type: 'LIST_MAPPINGS',
        payload: mappings
    });
}

const getMapping = (mappingId: string): AppThunkAction<KnownAction> => (dispatch) => {
    // This is sync call. In future, this maybe an async call with 
    // Restful APIs.
    const mapping = PersistService.getMapping(mappingId);
    dispatch({
        type: 'GET_MAPPING',
        payload: mapping
    });
}

const saveMapping = (mappingId: string, mappingData: Mapping): AppThunkAction<KnownAction> => (dispatch) => {
    // This is sync call. In future, this maybe an async call with 
    // Restful APIs.
    const mapping = PersistService.saveMapping(mappingId, mappingData);
    dispatch({
        type: 'SAVE_MAPPING',
        payload: mapping
    });
}

const deleteMapping = (mappingId: string): AppThunkAction<KnownAction> => (dispatch) => {
    // This is sync call. In future, this maybe an async call with 
    // Restful APIs.
    PersistService.deleteMapping(mappingId);
    dispatch({
        type: 'DELETE_MAPPING'
    });
}

export const ActionCreators = {
    getMapping: getMapping,
    listMappings: listMappings,
    saveMapping: saveMapping,
    deleteMapping: deleteMapping,
};