import { PredictionPoint, SelectedPredictionDetails, HistoricalPrediction } from '../store/models/Prices';
import { getQuotesForTicker } from './MockedApi';
import { post, get, HttpResponse } from '../utils/RepositoryUtils';

export interface IPredictionsRepository {
    getPredictionsForTicker(ticker: string, range: string, predictionModel?: string): Promise<HttpResponse<SelectedPredictionDetails>>;
    getHistoricalPredictions(): Promise<HttpResponse<HistoricalPrediction[]>>
    getHistoricalPredictionDetails(id: string): Promise<SelectedPredictionDetails | null>;
    savePrediction(prediction: HistoricalPrediction): Promise<HttpResponse<number>>;
}

export class PredictionsRepository implements IPredictionsRepository {
    async getPredictionsForTicker(ticker: string, range: string, predictionModel?: string, startDate?: string): Promise<HttpResponse<SelectedPredictionDetails>> {
        //if (predictionModel && predictionModel === 'Forecasting') {
        return await post<SelectedPredictionDetails>("https://localhost:44378/api/predictions", {
            Ticker: ticker,
            PredictionModel: predictionModel,
            Range: range,
            StartDate: startDate
        })
        //} else {
        //    return getQutesForTicker(ticker, range)
        //}
    }

    async savePrediction(prediction: HistoricalPrediction) {
        console.log(prediction);
        return await post<number>("https://localhost:44378/api/historicalpredictions", {
            CompanyName: prediction.companyName,
            PredictedBuyPrice: prediction.predictedBuyPrice,
            PredictedSellPrice: prediction.predictedSellPrice,
            ActualBuyPrice: prediction.actualBuyPrice,
            ActualSellPrice: prediction.actualSellPrice,
            StartDateAsString: prediction.startDate,
            EndDateAsString: prediction.endDate

        })
    }

    async clearPredictions() {

        return await post<number>("https://localhost:44378/api/historicalpredictions/clearAll", {
        })
    }

    async getHistoricalPredictions(): Promise<HttpResponse<HistoricalPrediction[]>> {
        const result = await get<HistoricalPrediction[]>(
            "https://localhost:44378/api/predictions/historical")

        return result;


    }
    getHistoricalPredictionDetails(id: string): Promise<SelectedPredictionDetails | null> {
        throw new Error('Method not implemented.');
    }

}

export default new PredictionsRepository();

