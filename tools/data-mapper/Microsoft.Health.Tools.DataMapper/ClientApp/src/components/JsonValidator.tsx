// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

import * as React from 'react';
import * as JsonLint from 'jsonlint-mod';
import * as CodeMirror from 'codemirror';

import 'codemirror/addon/display/placeholder.js'
import 'codemirror/addon/edit/matchbrackets.js';
import 'codemirror/lib/codemirror.css';
import 'codemirror/mode/javascript/javascript.js';

import './JsonValidator.css';

const JsonValidator = (props: { onTextChange: Function; placeholder: string }) => {
    const [validationResult, setValidationResult] = React.useState('');
    const [isValid, setIsValid] = React.useState(true);

    const textAreaRef = React.useRef<HTMLTextAreaElement>() as React.RefObject<HTMLTextAreaElement>;

    var codeEditor: CodeMirror.EditorFromTextArea;
    var errorLine: number | null = null;

    React.useEffect(() => {
        if (textAreaRef.current) {
            codeEditor = CodeMirror.fromTextArea(
                textAreaRef.current,
                {
                    mode: "javascript",
                    lineNumbers: true,
                    matchBrackets: true,
                    placeholder: `Paste your ${props.placeholder} here...`
                }
            );

            codeEditor.on('change', () => handleDataSampleChange(codeEditor.getValue()));

            return () => {
                codeEditor.toTextArea();
            };
        }
    }, []);

    const handleDataSampleChange = (newDataSample: string) => {
        props.onTextChange(newDataSample);
        try {
            JsonLint.parse(newDataSample);
            setValidationResult('Valid JSON');
            setIsValid(true);
            highlightErrorLine(null);
        }
        catch (err) {
            setValidationResult(err.toString());
            setIsValid(false);
            const lineMatches = err.message.match(/line ([0-9]+)/);
            if (lineMatches) {
                highlightErrorLine(Number(lineMatches[1]) - 1);
            }
        }
    }

    const highlightErrorLine = (line: number | null) => {
        if (line === errorLine || !codeEditor) {
            return;
        }
        if (typeof line === 'number') {
            codeEditor.addLineClass(line, 'background', 'line-color-error');
        }
        if (typeof errorLine === 'number') {
            codeEditor.removeLineClass(errorLine, 'background', 'line-color-error');
        }
        errorLine = line;
    }

    return (
        <div>
            <textarea className="border overflow-auto p-2" ref={textAreaRef}
                onChange={e => { handleDataSampleChange(e.target.value) }}>
            </textarea>
            <pre className={`validation-result overflow-auto p-2 ${isValid ? "text-success" : "text-danger"}`}>
                {validationResult}
            </pre>
        </div>
    );
}

export default JsonValidator;
