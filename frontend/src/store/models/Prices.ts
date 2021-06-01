import { createModel } from '@rematch/core';
import { IRootModel } from '../models';
import { getQuotesForTicker, getHistoricalPredictions, getHistoricalPredictionDetails } from '../../api/MockedApi';
import repo from '../../api/Predictions.Repository';
import { splitFormattedDateOnT } from '../../utils/DateSplit'

export interface SelectedPredictionDetails {
    ticker: string,
    predictions: PredictionPoint[],
    id: string
    //actualQuotes?: PredictionPoint[]
}

export interface LoadPredictionDetailsRequest {
    ticker: string,
    timeRange: string,
    startDate?: string
}


export interface HistoricalPrediction {
    ticker: string,
    startDate: string,
    endDate: string,
    id: string,
    status: string
}

export interface PredictionPoint {
    predictedPrice: number,
    actualPrice?: number,
    date: string
}

export interface PricesState {
    selectedPrediction: SelectedPredictionDetails | undefined,
    historicalPredictions: HistoricalPrediction[],
    isLoading: boolean
}

export const predictions = createModel<IRootModel>()({
    state: {
        selectedPrediction: undefined,
        historicalPredictions: [],
        isLoading: false
    } as PricesState,

    reducers: {
        addCompany(state: PricesState, company: SelectedPredictionDetails) {
            return {
                ...state, selectedPrediction: company
            }
        },

        addHistoricalPredictions(state: PricesState, predictions: HistoricalPrediction[]) {
            return {
                ...state, historicalPredictions: predictions,
            }
        },

        setIsLoading(state: PricesState, isLoading: boolean) {
            console.log(isLoading);
            return {
                ...state, isLoading: isLoading,
            }
        }
    },

    effects: (dispatch: any) => ({
        async loadPredictionDetails(request: LoadPredictionDetailsRequest) {
            //dispatch.prices.addCompany([])
            dispatch.predictions.setIsLoading(true);
            //const prediction: SelectedPredictionDetails = await getQuotesForTicker(request.ticker, request.timeRange);
            try {
                const prediction: SelectedPredictionDetails | undefined = await (await repo.getPredictionsForTicker(request.ticker, request.timeRange, "", request.startDate)).parsedBody;

                if (prediction) {
                    console.log('add');
                    prediction.predictions.forEach(prediction => prediction.date = splitFormattedDateOnT(prediction.date));
                    dispatch.predictions.addCompany(prediction);
                    //const updatedHistoricalPredictions = await getHistoricalPredictions();
                    //dispatch.predictions.addHistoricalPredictions(updatedHistoricalPredictions);
                }
            } catch {

            } finally {
                dispatch.predictions.setIsLoading(false);
            }






        },

        async loadHistoricalPredictions() {
            dispatch.predictions.setIsLoading(true);
            const res = await getHistoricalPredictions();
            dispatch.predictions.addHistoricalPredictions(res);
            dispatch.predictions.setIsLoading(false);
        },

        async loadHistoricalPredictionDetails(id: string) {
            dispatch.predictions.setIsLoading(true);
            const historicalPredictionDetails = await getHistoricalPredictionDetails(id);
            if (historicalPredictionDetails) {
                dispatch.predictions.addCompany(historicalPredictionDetails);
            }
            dispatch.predictions.setIsLoading(false);
        }
    })

})