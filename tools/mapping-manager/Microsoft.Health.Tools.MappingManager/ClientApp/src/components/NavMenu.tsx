// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

import * as React from 'react';
import { Navbar, NavbarBrand } from 'reactstrap';
import { Link } from 'react-router-dom';
import './NavMenu.css';

const NavMenu = (props: {}) => {
    return (
        <header>
            <Navbar className="navbar-expand-sm border-bottom box-shadow mb-1" light>
                <NavbarBrand tag={Link} to="/">
                    IoMT Mapping Manager
                </NavbarBrand>
            </Navbar>
        </header>
    );
}
export default NavMenu;
