// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

import * as Mappings from './Mapping';

// The top-level state object
export interface ApplicationState {
    mappingList: Mappings.MappingListStoreState | undefined;
    mappingDetail: Mappings.MappingDetailStoreState | undefined;
}

// Whenever an action is dispatched, Redux will update each top-level application state property using
// the reducer with the matching name. It's important that the names match exactly, and that the reducer
// acts on the corresponding ApplicationState property type.
export const reducers = {
    mappingList: Mappings.ListReducer,
    mappingDetail: Mappings.DetailReducer,
};

// This type can be used as a hint on action creators so that its 'dispatch' and 'getState' params are
// correctly typed to match your store.
export interface AppThunkAction<TAction> {
    (dispatch: (action: TAction) => any, getState: () => ApplicationState): any;
}
