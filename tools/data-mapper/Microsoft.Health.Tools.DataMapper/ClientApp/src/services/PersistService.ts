﻿// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

// -------------
// Persist Services.
import * as _ from "lodash";
import { Mapping } from "../store/Mapping";
import * as Utils from "./Utils";

const LocalStorageKey = 'iomt-mappings';

interface MappingsStorage {
    mappingsById: MappingByIdIndex
}

interface MappingByIdIndex {
    [id: string]: Mapping
}

const loadFromLocalStorage = (): MappingsStorage => {
    const persistedData = localStorage.getItem(LocalStorageKey);
    return (persistedData ? JSON.parse(Utils.sanitize(persistedData)) : {}) as MappingsStorage;
}

const saveMappingToLocalStorage = (mappingId: string, mappingData: Mapping) => {
    const storage = loadFromLocalStorage();
    storage.mappingsById = {
        ...storage.mappingsById,
        [mappingId]: mappingData
    }
    localStorage.setItem(LocalStorageKey, Utils.sanitize(JSON.stringify(storage)));
    return mappingData;
}

const deleteMappingInLocalStorage = (mappingId: string) => {
    const storage = loadFromLocalStorage();
    storage.mappingsById = _.omit(storage.mappingsById, mappingId);
    localStorage.setItem(LocalStorageKey, Utils.sanitize(JSON.stringify(storage)));
}

const getAllMappings = (): Mapping[] => {
    const mappingsById = loadFromLocalStorage().mappingsById;
    return _.values(mappingsById);
}

const getMapping = (mappingId: string): Mapping => {
    const storage = loadFromLocalStorage();
    return storage.mappingsById[mappingId];
}

const createMapping = (typename: string): Promise<Mapping> => {
    return new Promise((resolve, reject) => {
        const mappings = getAllMappings();

        if (_.find(mappings, { typeName: typename })) {
            reject(`The typename ${typename} existed`);
            return;
        }

        const newMappingId = generateUID();
        const newMapping = {
            id: newMappingId,
            typeName: typename
        } as Mapping;
        saveMappingToLocalStorage(newMappingId, newMapping);

        resolve(newMapping);
        return;
    });
}

const generateUID = () => {
    var dt = new Date().getTime();
    var uuid = 'xxxxxxxx'.replace(/[xy]/g, function (c) {
        var r = (dt + Math.random() * 16) % 16 | 0;
        dt = Math.floor(dt / 16);
        return (c === 'x' ? r : ((r & 0x3) | 0x8)).toString(16);
    });
    return uuid;
}

// The service is providing basic CRUD operations.
// This can be changed to Restful APIs hosted by
// the backend server.
const PersistService = {
    createMapping: createMapping,
    getAllMappings: getAllMappings,
    getMapping: getMapping,
    saveMapping: saveMappingToLocalStorage,
    deleteMapping: deleteMappingInLocalStorage
}

export default PersistService;