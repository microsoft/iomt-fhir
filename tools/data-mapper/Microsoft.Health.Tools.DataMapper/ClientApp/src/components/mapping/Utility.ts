// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

export const getRandomString = () => {
    return Math.random().toString(36).replace(/[^a-z]+/g, '').substr(0, 6);
}

export const sanitize = (text: string) => {
    var element = document.createElement('div');
    element.innerText = text;
    return element.innerHTML;
}