// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

import * as React from 'react';
import { Route } from 'react-router';
import Layout from './components/Layout';

import MappingListPage from './components/mapping/List.Page';
import MappingDetailPage from './components/mapping/Detail.Page';
import * as Constants from './components/Constants';

import './custom.css'

export default () => (
    <Layout>
        <Route exact path='/' component={MappingListPage} />
        <Route exact path='/mappings' component={MappingListPage} />
        <Route path={`${Constants.Text.PathMappings}:id`} component={MappingDetailPage} />
    </Layout>
);
