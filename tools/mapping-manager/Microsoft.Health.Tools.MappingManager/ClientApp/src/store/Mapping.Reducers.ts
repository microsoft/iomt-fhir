// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

import { Reducer, Action } from "redux";
import { KnownAction } from "./Mapping.Actions";
import { MappingListStoreState, MappingDetailStoreState, Mapping } from "./Mapping.States";

// Mapping List Reducer
const initialMappingsState: MappingListStoreState = {
    mappings: [],
    hasLoaded: false,
};

export const ListReducer: Reducer<MappingListStoreState> =
    (state: MappingListStoreState | undefined, incomingAction: Action): MappingListStoreState => {
        if (state === undefined) {
            return initialMappingsState;
        }
        const action = incomingAction as KnownAction;
        switch (action.type) {
            case 'LIST_MAPPINGS':
                return {
                    mappings: action.payload,
                    hasLoaded: true,
                };
            default:
        }

        return state;
    };

// Mapping Detail Reducer
const initialMappingState: MappingDetailStoreState = {
    mapping: {} as Mapping,
    hasLoaded: false,
};

export const DetailReducer: Reducer<MappingDetailStoreState> =
    (state: MappingDetailStoreState | undefined, incomingAction: Action): MappingDetailStoreState => {
        if (state === undefined) {
            return initialMappingState;
        }
        const action = incomingAction as KnownAction;
        switch (action.type) {
            case 'GET_MAPPING':
            case 'SAVE_MAPPING':
                return {
                    mapping: action.payload,
                    hasLoaded: true,
                };
            default:
        }

        return state;
    }