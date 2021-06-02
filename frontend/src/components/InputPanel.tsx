import React, { useEffect, useState } from 'react';
import '../App.css';
import { useSelector, useDispatch, shallowEqual } from 'react-redux'
import { RootState, Dispatch } from '../store/store';
import { Dropdown } from './common/Dropdown'
import { Datepicker } from './common/Datepicker'
import { mockedCurrentDate } from '../api/MockedApi';
import moment from 'moment';

export const InputPanel = (): JSX.Element => {

    const tickers = useSelector((state: RootState) => state.tickers, shallowEqual)
    const timeRanges = useSelector((state: RootState) => state.timeRanges, shallowEqual)
    const predictions = useSelector((state: RootState) => state.predictions, shallowEqual)
    const predictionModels = useSelector((state: RootState) => state.predictionModels, shallowEqual)
    const dispatch = useDispatch<Dispatch>()

    const [predictionStartDate, setPredictionStartDate] = useState(mockedCurrentDate);

    useEffect(() => {
        async function getTickers() {
            await dispatch.tickers.loadAvaliableTickers()
            await dispatch.predictionModels.loadAvaliablePredictionModels()
        }
        getTickers();

    }, [dispatch.tickers, dispatch.predictionModels])

    const setTicker = (ticker: string) => {
        dispatch.tickers.setSelectedTicker(ticker)
    }

    const setPredictionModel = (model: string) => {
        dispatch.predictionModels.setSelectedPredictionModel(model)
    }

    const setTimeRange = (ticker: string) => {
        dispatch.timeRanges.setSelectedRange(ticker);
    }

    function isEstimateButtonDisabled(): boolean {
        return !tickers.selectedTicker || !timeRanges.selectedRange || predictions.isLoading;
    }

    const setStartTime = (time: string): void => {
        console.log(time);
        setPredictionStartDate(moment(time));
        console.log(predictionStartDate);
    }

    return (
        <>
            <h3>Select Ticker, Range of estimations and estimation start date.</h3>
            <div className='input-panel'>
                <Dropdown
                    options={tickers.avaliableTickers}
                    selectedOption={tickers.selectedTicker}
                    setSelectedOption={setTicker}
                    defaultText={'Select Ticker'}
                    noDataText={'Loading Tickers'}
                    className='avaliable_tickers'
                />

                <Dropdown
                    options={timeRanges.avaliableRanges}
                    selectedOption={timeRanges.selectedRange}
                    setSelectedOption={setTimeRange}
                    defaultText={'Select Range'}
                    noDataText={'Loading Ranges'}
                    className='avaliable_timeranges'
                    disabled={predictionModels.selectedModel === "Forecasting"}
                />

                <Datepicker name={'test'} value={predictionStartDate.format('YYYY-MM-DD')} setDate={setStartTime} isDisabled={predictionModels.selectedModel === "Forecasting"} />

                <Dropdown
                    options={predictionModels.avaliableModels}
                    selectedOption={predictionModels.selectedModel}
                    setSelectedOption={setPredictionModel}
                    defaultText={'Select Ticker'}
                    noDataText={'Loading Tickers'}
                    className='avaliable_tickers'
                />



                <button
                    disabled={isEstimateButtonDisabled()}
                    className={'button'}
                    onClick={() => dispatch.predictions.loadPredictionDetails({ ticker: tickers.selectedTicker, timeRange: timeRanges.selectedRange, startDate: predictionStartDate.format() ?? null, predictionModel: predictionModels.selectedModel })}>Add</button>
                <h4>Today is {mockedCurrentDate.format('YYYY-MM-DD')}</h4>

            </div>
        </>
    )
}