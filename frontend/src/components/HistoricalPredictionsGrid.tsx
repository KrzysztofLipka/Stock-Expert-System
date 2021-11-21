import { HistoricalPrediction } from '../store/models/Prices';
import { Table, ColumnDefinitionType } from './common/Table'

import { useSelector, useDispatch, shallowEqual } from 'react-redux'
import { RootState, Dispatch } from '../store/store';
import { useEffect, useState } from 'react';


export interface HistoricalPredictionsGridProps {
    predictions: HistoricalPrediction[];
}

const columns: ColumnDefinitionType<HistoricalPrediction, keyof HistoricalPrediction>[] = [
    {
        key: 'ticker',
        header: 'Ticker'
    },
    {
        key: 'startDate',
        header: 'Start Date',
    },
    {
        key: 'endDate',
        header: 'End Date'
    },
    {
        key: 'status',
        header: 'Status'
    }
]



export const HistoricalPredictionsGrid = (props: HistoricalPredictionsGridProps): JSX.Element => {

    const predictions = useSelector((state: RootState) => state.predictions, shallowEqual)
    const dispatch = useDispatch<Dispatch>()

    useEffect(
        () => {
            async function getHistoricalPredictions() {
                await dispatch.predictions.loadHistoricalPredictions();
            }

            getHistoricalPredictions();
        }, [dispatch.predictions]
    )

    const [clickedPrediction, setClickedPrediction] = useState<HistoricalPrediction | undefined>(undefined)


    const renderPredictionDetails = (clickedPrediction: HistoricalPrediction): JSX.Element => {
        if (!clickedPrediction) {
            return <></>
        }

        const isImportButtonDisabled = (): boolean => {
            return clickedPrediction.id === predictions?.selectedPrediction?.id || predictions?.isLoading
        }


        return (
            <>
                <h3>{clickedPrediction.ticker}</h3>
                <p> <strong>Start Date</strong> {clickedPrediction.startDate} <strong>End Date</strong> {clickedPrediction.endDate}</p>
                <p>{clickedPrediction.id}</p>
                <p>{clickedPrediction.status}</p>
                <button className='button' disabled={isImportButtonDisabled()} onClick={() => dispatch.predictions.loadHistoricalPredictionDetails(clickedPrediction.id)}>Load</button>
            </>
        )

    }

    function handleRowClick(row: HistoricalPrediction) {
        setClickedPrediction(row);

    }

    if (!props?.predictions) {
        return <></>
    }

    return (<>
        <h3>Historical</h3>
        <div className='historical-predictions'>
            <Table data={props.predictions} columns={columns} actionOnRowClick={handleRowClick} />
            {<div className='historical-prediction-details'>{clickedPrediction ? renderPredictionDetails(clickedPrediction) : "BUY 2020-11-16 / SELL 2020-12-01"}</div>}
        </div></>)
}