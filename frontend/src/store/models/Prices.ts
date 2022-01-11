import { createModel } from '@rematch/core';
import { IRootModel } from '../models';
import { getQuotesForTicker, getHistoricalPredictions, getHistoricalPredictionDetails } from '../../api/MockedApi';
import repo from '../../api/Predictions.Repository';
import { splitFormattedDateOnT } from '../../utils/DateSplit'

export interface SelectedPredictionDetails {
    ticker: string,
    predictions: PredictionPoint[],
    id: string,
    buyPrice?: PredictionPoint,
    sellPrice?: PredictionPoint
    //actualQuotes?: PredictionPoint[]
}

export interface LoadPredictionDetailsRequest {
    ticker: string,
    timeRange: string,
    startDate?: string,
    predictionModel?: string
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
    date: string,
    isSuggestedBuy?: boolean,
    isSuggestedSell?: boolean,
}

export interface PricesState {
    selectedPrediction: SelectedPredictionDetails | undefined,
    historicalPredictions: HistoricalPrediction[],
    isLoading: boolean
}

const calculateBuySellSuggestions = (predictions: PredictionPoint[]): [PredictionPoint, PredictionPoint] => {
    //var maxA = a.reduce((a,b)=>a.y>b.y?a:b).y;
    let minValueIndex = 0
    let maxValueIndex = 0;


    const minValue = predictions.reduce((a, b, index) => /*a.predictedPrice > b.predictedPrice ? a : b*/ {
        if (a.predictedPrice < b.predictedPrice) {

            return a;
        } else {
            minValueIndex = index
            return b;
        }
    }
    );

    const maxValue = predictions.reduce((a, b, index) => /*a.predictedPrice > b.predictedPrice ? a : b*/ {
        if (a.predictedPrice < b.predictedPrice && index > minValueIndex) {
            maxValueIndex = index;
            return b;
        } else {

            return a;
        }
    }
    );




    console.log(minValueIndex);
    console.log(maxValueIndex)

    if (maxValueIndex < minValueIndex) {
        return [predictions[0], predictions[0]]
    }

    return [minValue, maxValue]

};

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
                const prediction: SelectedPredictionDetails | undefined = await (await repo.getPredictionsForTicker(request.ticker, request.timeRange, request.predictionModel, request.startDate)).parsedBody;

                if (prediction) {
                    console.log('add');
                    prediction.predictions.forEach(prediction => prediction.date = splitFormattedDateOnT(prediction.date));

                    dispatch.predictions.addCompany(prediction);
                    const [minValue, maxValue] = calculateBuySellSuggestions(prediction.predictions);
                    prediction.buyPrice = minValue;
                    prediction.sellPrice = maxValue;

                    console.log(minValue);
                    console.log(maxValue);
                    const updatedHistoricalPredictions = await getHistoricalPredictions();
                    dispatch.predictions.addHistoricalPredictions(updatedHistoricalPredictions);
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