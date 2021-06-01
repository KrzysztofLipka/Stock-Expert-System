import { Models } from '@rematch/core';
import { predictions } from './Prices'
import { tickers } from './Tickers'
import { timeRanges } from './TimeRanges'
import { predictionModels } from './PredictionModels'


export interface IRootModel extends Models<IRootModel> {
    predictions: typeof predictions,
    tickers: typeof tickers,
    timeRanges: typeof timeRanges,
    predictionModels: typeof predictionModels
}

export const models: IRootModel = { predictions, tickers, timeRanges, predictionModels }