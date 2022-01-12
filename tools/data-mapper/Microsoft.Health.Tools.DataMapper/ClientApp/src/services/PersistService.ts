// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

// -------------
// Persist Services.
import * as _ from "lodash";
import { DeviceMapping, FhirMapping, Mapping } from "../store/Mapping";
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
    const mappings = storage.mappingsById;
    if (!mappings) {
        return {} as Mapping;
    }
    return mappings[mappingId];
}

const createMappingsFromTemplates = (deviceTemplate: string, fhirTemplate: string): Promise<Mapping[]> => {
    return new Promise((resolve, reject) => {
        Promise.all([Utils.generateDeviceMappings(deviceTemplate), Utils.generateFhirMappings(fhirTemplate)])
            .then(mappings => {
                const currentMappings = getAllMappings();
                let newMappings: Mapping[] = [];

                if (mappings.length !== 2) {
                    reject(`System error: ${mappings.length} set(s) of mappings were generated.
                        Please ensure there are no other errors and try again.`);
                    return;
                }
                const deviceMappings = mappings[0];
                let fhirMappings = mappings[1];

                for (const deviceMapping of deviceMappings) {
                    const typename = deviceMapping.typeName;
                    if (_.find(currentMappings, { typeName: typename })) {
                        reject(`A mapping in the Data Mapper already has the typeName "${typename}".
                            Please enter another typeName for these templates or remove the templates with this typeName.`);
                        return;
                    }

                    // Group device and FHIR mappings with the same type name into the same mapping
                    const fhirMapping = _.remove(fhirMappings, { typeName: typename });
                    createMapping(typename, deviceMapping.device, fhirMapping.length ? fhirMapping[0].fhir : undefined)
                        .then((newMapping: Mapping) => {
                            newMappings.push(newMapping);
                        })
                        .catch(err => {
                            reject(err);
                            return;
                        });
                }

                for (const fhirMapping of fhirMappings) {
                    const typename = fhirMapping.typeName;
                    if (_.find(currentMappings, { typeName: typename })) {
                        reject(`A mapping in the Data Mapper already has the typeName "${typename}".
                            Please enter another typeName for these templates or remove the templates with this typeName.`);
                        return;
                    }

                    createMapping(typename, undefined, fhirMapping.fhir)
                        .then((newMapping: Mapping) => {
                            newMappings.push(newMapping);
                        })
                        .catch(err => {
                            reject(err);
                            return;
                        });
                }

                resolve(newMappings);
                return;
            })
            .catch(err => {
                reject(err);
                return;
            });
    });
}

const createMapping = (typename: string, device?: DeviceMapping, fhir?: FhirMapping): Promise<Mapping> => {
    const id = generateUID();
    return updateMapping(id, typename, device, fhir);
}

const renameMapping = (id: string, typename: string): Promise<Mapping> => {
    const mapping = getMapping(id);
    if (!mapping) {
        return Promise.reject(`The mapping with id ${id} does not exist`);
    }
    return updateMapping(id, typename, mapping.device, mapping.fhir);
}

const updateMapping = (id: string, typename: string, device?: DeviceMapping, fhir?: FhirMapping): Promise<Mapping> => {
    return new Promise((resolve, reject) => {
        if (!typename || typename.trim().length < 1) {
            reject(`The type name cannot be empty`);
            return;
        }

        const existingMapping = getMapping(id);
        if (existingMapping && existingMapping.typeName === typename) {
            resolve(existingMapping);
            return;
        }

        const mappings = getAllMappings();
        if (_.find(mappings, { typeName: typename })) {
            reject(`The type name "${typename}" already exists`);
            return;
        }

        const mapping = {
            id: id,
            typeName: typename,
            device: device,
            fhir: fhir
        } as Mapping;
        saveMappingToLocalStorage(id, mapping);

        resolve(mapping);
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
    createMappingsFromTemplates: createMappingsFromTemplates,
    getAllMappings: getAllMappings,
    getMapping: getMapping,
    renameMapping: renameMapping,
    saveMapping: saveMappingToLocalStorage,
    deleteMapping: deleteMappingInLocalStorage
}

export default PersistService;