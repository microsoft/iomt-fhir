// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

import * as React from 'react';
import { Navbar, NavbarBrand, NavbarText } from 'reactstrap';
import { Link, useLocation } from 'react-router-dom';
import PersistService from '../services/PersistService';
import { Mapping } from '../store/Mapping';
import { default as MappingCreationModal, Action } from './mapping/List.Modals.Creation';
import './NavMenu.css';

const NavMenu = (props: {}) => {
    const [mappingId, setMappingId] = React.useState('');
    const [mappingTypeName, setMappingTypeName] = React.useState('');

    const location = useLocation();

    React.useEffect(() => {
        const pathname = location.pathname;
        const id = pathname.substr(pathname.lastIndexOf('/') + 1);
        const typeName = PersistService.getMapping(id)?.typeName;
        setMappingId(id);
        setMappingTypeName(typeName);
    }, [location]);

    const renameMapping = (id: string, typename: string, errorHandler?: Function) => {
        PersistService.renameMapping(id, typename)
            .then((mapping: Mapping) => {
                setMappingTypeName(mapping.typeName);
            })
            .catch(err => {
                if (errorHandler) {
                    errorHandler(err);
                }
            });
    }

    return (
        <header>
            <Navbar className="navbar-expand-sm border-bottom box-shadow mb-1" light>
                <NavbarBrand tag={Link} to="/">
                    IoMT Connector Data Mapper
                </NavbarBrand>
                {mappingTypeName !== undefined &&
                    <NavbarText>
                        <span className="page-heading" style={{ margin: "0px 10px 0px 50px" }}>
                            {mappingTypeName}
                        </span>
                        <MappingCreationModal
                            onSave={(typename: string, errorHandler: Function) => renameMapping(mappingId, typename, errorHandler)}
                            action={Action.Rename}
                            inputDefaultValue={mappingTypeName}
                        />
                    </NavbarText>
                }
            </Navbar>
        </header>
    );
}
export default NavMenu;
