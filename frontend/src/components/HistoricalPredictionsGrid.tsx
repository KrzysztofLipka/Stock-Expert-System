import { HistoricalPrediction } from '../store/models/Prices';

import { useSelector, useDispatch, shallowEqual } from 'react-redux'
import { RootState, Dispatch } from '../store/store';
import { useEffect, useState } from 'react';
import { DataGrid, GridColumns, GridSortModel } from '@mui/x-data-grid';
import { width } from '@mui/system';
import { formatAmount } from '../utils/AmountFormat'
import { splitFormattedDateOnT } from '../utils/DateSplit'
import Stack from '@mui/material/Stack';
import Button from '@mui/material/Button';

export interface HistoricalPredictionsGridProps {
    predictions: HistoricalPrediction[];
}

export interface HistoricalPredctionGridDisplay {
    isProfit: boolean,
    balance: string,
    //actualDiference: number,
    predictedBuyPrice: string,
    predictedSellPrice: string,
    actualBuyPrice: string,
    actualSellPrice: string
    id: string
    startDate: string,
    endDate: string,
}

const mapToGridDisplay = (predictions: HistoricalPrediction[]): HistoricalPredctionGridDisplay[] => {
    return predictions.map(prediction => ({
        companyName: prediction.companyName,
        predictedBuyPrice: formatAmount(prediction.predictedBuyPrice),
        predictedSellPrice: formatAmount(prediction.predictedSellPrice),
        actualBuyPrice: formatAmount(prediction.actualBuyPrice),
        actualSellPrice: formatAmount(prediction.actualSellPrice),
        startDate: splitFormattedDateOnT(prediction.startDate),
        endDate: splitFormattedDateOnT(prediction.endDate),
        predictionId: prediction.predictionId,
        id: prediction?.predictionId ?? "0",
        isProfit: prediction.actualSellPrice > prediction.actualBuyPrice,
        balance: formatAmount(prediction.actualSellPrice - prediction.actualBuyPrice),
        //actualDiference: prediction.actualSellPrice - prediction.actualBuyPrice
    }))
}

const totalBalance = (predictions: HistoricalPrediction[]) => {
    if (!predictions) {
        return 0;
    }

    return predictions.reduce((a, b) => a + (b.actualSellPrice - b.actualBuyPrice || 0), 0);
}

//[profitsCount, losesCount]
const profitLosesCounts = (predictions: HistoricalPrediction[]): [number, number] => {
    let profitsCount = 0;
    let losesCount = 0;
    predictions.forEach(prediction => prediction.actualSellPrice > prediction.actualBuyPrice
        ? profitsCount++
        : losesCount++);
    return [profitsCount, losesCount]
}



const columns: any[] = [
    {
        field: 'id',
        headerName: 'Id',
        width: 75,
        editable: true,
        hide: true
    },
    {
        field: 'companyName',
        headerName: 'Name',
        width: 75,
        editable: true
    },
    {
        field: 'startDate',
        headerName: 'Start Date',
        width: 150,
        editable: true
    },
    {
        field: 'endDate',
        headerName: 'End Date',
        width: 150,
        editable: true
    },
    {
        field: 'predictedBuyPrice',
        headerName: 'Pred. Buy Price',
        width: 150,
        editable: true
    },
    {
        field: 'predictedSellPrice',
        headerName: 'Pred. Sell Price',
        width: 150,
        editable: true
    },
    {
        field: 'actualBuyPrice',
        headerName: 'Act. Buy Price',
        width: 150,
        editable: true
    },
    {
        field: 'actualSellPrice',
        headerName: 'Act. Sell Price',
        width: 150,
        editable: true
    },
    {
        field: 'isProfit',
        headerName: 'Is Profit',
        width: 150,
        editable: true
    },
    {
        field: 'balance',
        headerName: 'Balance',
        width: 150,
        editable: true
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
            return clickedPrediction.predictionId === predictions?.selectedPrediction?.id || predictions?.isLoading
        }


        return (
            <>
                <h3>{clickedPrediction.companyName}</h3>
                <p> <strong>Start Date</strong> {clickedPrediction.startDate} <strong>End Date</strong> {clickedPrediction.endDate}</p>
                <p>{clickedPrediction.predictionId}</p>
                <button className='button' disabled={isImportButtonDisabled()} onClick={() => dispatch.predictions.loadHistoricalPredictionDetails(clickedPrediction?.predictionId ?? "")}>Load</button>
            </>
        )

    }

    function handleRowClick(/*row: HistoricalPrediction*/params: any) {
        //setClickedPrediction(row);
        console.log(params)

    }

    const [sortModel, setSortModel] = useState<GridSortModel>([
        {
            field: 'id',
            sort: 'desc',
        },
    ]);

    if (!props?.predictions) {
        return <></>
    }
    const [profitsCount, losesCount] = profitLosesCounts(props.predictions);

    return (<>
        <Stack spacing={2} direction="row">
            <div className='historical-prediction-details' style={{ height: 400, width: '100%' }}>
                <h3>Historical</h3>

                <div style={{ height: 400, width: '100%' }}>
                    <DataGrid
                        rows={mapToGridDisplay(props.predictions)}
                        columns={columns}
                        pageSize={5}
                        rowsPerPageOptions={[5]}
                        checkboxSelection
                        onRowClick={handleRowClick}
                        sortModel={sortModel}
                        disableSelectionOnClick

                    />
                </div>
            </div>

            <div className='historical-predictions'>

                {<div style={{ height: "100%", minWidth: 250, padding: 5 }} className='historical-prediction-details'>

                    <span>Total Balance</span>
                    <h1>{formatAmount(totalBalance(props.predictions))}</h1>

                    <span>Profits</span>
                    <h2>{profitsCount}</h2>
                    <span>Loses</span>
                    <h2>{losesCount}</h2>

                    <span>Total</span>
                    <h2>{props?.predictions.length}</h2>


                    <Button
                        variant="outlined"
                        disabled={false}
                        className={'forecast-button'}
                        size='medium'
                        onClick={() => dispatch.predictions.clearPredictions()}
                    >Reset
                    </Button>




                </div>}
            </div>
        </Stack>
    </>)
}

//{clickedPrediction ? renderPredictionDetails(clickedPrediction) : "BUY 2020-11-16 / SELL 2020-12-01"}