// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

import * as React from 'react';
import { Input, Col, Row } from 'reactstrap';
import * as JsonLint from 'jsonlint-mod';
import * as CodeMirror from 'codemirror';

import 'codemirror/addon/display/placeholder.js'
import 'codemirror/addon/edit/matchbrackets.js';
import 'codemirror/lib/codemirror.css';
import 'codemirror/mode/javascript/javascript.js';

import { Mapping } from '../../store/Mapping';
import TestService from '../../services/TestService';
import * as Constants from '../Constants';
import { PlayCircleIcon } from '../Icons';
import * as Utility from './Utility';

const MappingTestWidget = (props: { data: Mapping }) => {
    const [dataSample, setDataSample] = React.useState('');
    const [dataSampleResult, setDataSampleResult] = React.useState('');
    const [dataSampleValid, setDataSampleValid] = React.useState(true);

    const [normTestResult, setNormTestResult] = React.useState('Normalization test result output...');
    const [normTestResultBadge, setNormTestResultBadge] = React.useState('');
    const [normTestInProgress, setNormTestInProgress] = React.useState(false);

    const [fhirTestResult, setFhirTestResult] = React.useState('FHIR transformation test result output...');
    const [fhirTestResultBadge, setFhirTestResultBadge] = React.useState('');
    const [fhirTestInProgress, setFhirTestInProgress] = React.useState(false);

    const dataSampleRef = React.useRef<HTMLTextAreaElement>() as React.RefObject<HTMLTextAreaElement>;
    const identityResolutionTypeRef = React.useRef<HTMLInputElement>() as React.RefObject<HTMLInputElement>;

    var codeEditor: CodeMirror.EditorFromTextArea;
    var dataSampleErrorLine: number | null = null;

    React.useEffect(() => {
        if (dataSampleRef.current) {
            codeEditor = CodeMirror.fromTextArea(
                dataSampleRef.current,
                {
                    mode: "javascript",
                    lineNumbers: true,
                    matchBrackets: true,
                    placeholder: 'Paste your device data sample here...'
                }
            );

            codeEditor.on('change', () => handleDataSampleChange(codeEditor.getValue()));

            return () => {
                codeEditor.toTextArea();
            };
        }
    }, []);

    const handleDataSampleChange = (newDataSample: string) => {
        setDataSample(newDataSample);
        try {
            JsonLint.parse(newDataSample);
            setDataSampleResult('Valid JSON');
            setDataSampleValid(true);
            highlightDataSampleErrorLine(null);
        }
        catch (err) {
            setDataSampleResult(err.toString());
            setDataSampleValid(false);
            const lineMatches = err.message.match(/line ([0-9]+)/);
            if (lineMatches) {
                highlightDataSampleErrorLine(Number(lineMatches[1]) - 1);
            }
        }
    }

    const highlightDataSampleErrorLine = (line: number | null) => {
        if (line === dataSampleErrorLine) {
            return;
        }
        if (typeof line === 'number') {
            codeEditor.addLineClass(line, 'background', 'iomt-cm-data-error');
        }
        if (typeof dataSampleErrorLine === 'number') {
            codeEditor.removeLineClass(dataSampleErrorLine, 'background', 'iomt-cm-data-error');
        }
        dataSampleErrorLine = line;
    }

    const startNormalizationTest = () => {
        setNormTestInProgress(true);
        TestService.testNormalization(props.data, dataSample ?? '')
            .then(res => {
                return res.json();
            })
            .then(resBody => {
                if (resBody.result === "Success") {
                    setNormTestResult(resBody.normalizedData);
                } else {
                    setNormTestResult(resBody.reason);
                }
                setNormTestResultBadge(resBody.result);
                setNormTestInProgress(false);
            })
            .catch(err => {
                setNormTestResultBadge(Constants.Text.LabelTestResultFail);
                setNormTestResult(err);
                setNormTestInProgress(false);
            })
    }

    const startTransformationTest = () => {
        let testNormData: any;

        try {
            const normalizedData = JSON.parse(normTestResult);
            testNormData = {
                count: normalizedData.length,
                data: normalizedData,
                patientId: normalizedData[0].PatientId,
                deviceId: normalizedData[0].DeviceId,
                measuretype: normalizedData[0].Type,
            }
        }
        catch (err) {
            setFhirTestResultBadge(Constants.Text.LabelTestResultFail);
            setFhirTestResult(`${err.message}. Did you already get successful normalization result?`);
            return;
        }

        const testingMapping = {
            ...props.data,
            identityResolutionType: identityResolutionTypeRef.current?.value
        } as Mapping;

        setFhirTestInProgress(true);
        TestService.testFhirTransformation(testingMapping, JSON.stringify(testNormData))
            .then(res => {
                return res.json();
            })
            .then(resBody => {
                if (resBody.result === "Success") {
                    setFhirTestResult(resBody.fhirData);
                } else {
                    setFhirTestResult(resBody.reason);
                }
                setFhirTestResultBadge(resBody.result);
                setFhirTestInProgress(false);
            })
            .catch(err => {
                setFhirTestResultBadge(Constants.Text.LabelTestResultFail);
                setFhirTestResult(err);
                setFhirTestInProgress(false);
            })
    }

    return (
        <div className="iomt-cm-test-area">
            <div className="iomt-cm-test border">
                <div className="iomt-cm-test-title p-2 m-0 position-relative">
                    <span className="h5">Dataflow Simulation</span>
                </div>
                <Row>
                    <Col sm={4}>
                        <div className="p-2 h-100">
                            <div className="pt-2 pb-3">
                                <span className="h6">Device Data Sample</span>
                            </div>
                            <textarea className="border overflow-auto p-2" ref={dataSampleRef}>
                            </textarea>
                            <pre className={`iomt-cm-data-result overflow-auto p-2 ${dataSampleValid ? "text-success" : "text-danger"}`}>
                                {dataSampleResult}
                            </pre>
                        </div>
                    </Col>
                    <Col sm={4}>
                        <div className="p-2 h-100">
                            <div className="pt-2 pb-3">
                                <span className="h6">Normalization</span>
                                <div className="float-right">
                                    <span className={normTestResultBadge === "Success" ? "text-success" : "text-danger"}>
                                        {normTestInProgress ? '' : normTestResultBadge}
                                    </span>
                                    <button className="ml-2 btn iomt-cm-btn" onClick={startNormalizationTest}>
                                        <PlayCircleIcon /> Run
                                    </button>
                                </div>
                            </div>
                            <pre className="iomt-cm-test-result border overflow-auto p-2">
                                {normTestInProgress ? 'Testing...' : normTestResult}
                            </pre>
                        </div>
                    </Col>
                    <Col sm={4}>
                        <div className="p-2 h-100">
                            <div className="pt-2 pb-3">
                                <span className="h6">FHIR Transformation </span>
                                <div className="float-right text-right w-75">
                                    <span className={fhirTestResultBadge === "Success" ? "text-success" : "text-danger"}>
                                        {fhirTestInProgress ? '' : fhirTestResultBadge}
                                    </span>
                                    <Input type="select" className="d-inline-block w-50 form-control p-1 mr-2 ml-2"
                                        innerRef={identityResolutionTypeRef}
                                        defaultValue={"Create"}
                                    >
                                        <option disabled>Select identity resolution type...</option>
                                        <option value="Create">Create</option>
                                        <option value="Lookup">Lookup</option>
                                    </Input>
                                    <button className="btn iomt-cm-btn" onClick={startTransformationTest}>
                                        <PlayCircleIcon /> Run
                                    </button>
                                </div>
                            </div>
                            <pre className="iomt-cm-test-result border overflow-auto p-2">
                                {fhirTestInProgress ? 'Testing...' : fhirTestResult}
                            </pre>
                        </div>
                    </Col>
                </Row>
            </div>
        </div>
    );
}

export default MappingTestWidget;