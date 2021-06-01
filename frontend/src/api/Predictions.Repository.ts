import { PredictionPoint, SelectedPredictionDetails, HistoricalPrediction } from '../store/models/Prices';
import { getQuotesForTicker } from './MockedApi';
import { post, get, HttpResponse } from '../utils/RepositoryUtils';

export interface IPredictionsRepository {
    getPredictionsForTicker(ticker: string, range: string, predictionModel?: string): Promise<HttpResponse<SelectedPredictionDetails>>;
    getHistoricalPredictions(): Promise<HistoricalPrediction[]>
    getHistoricalPredictionDetails(id: string): Promise<SelectedPredictionDetails | null>;
}

export class PredictionsRepository implements IPredictionsRepository {
    async getPredictionsForTicker(ticker: string, range: string, predictionModel?: string, startDate?: string): Promise<HttpResponse<SelectedPredictionDetails>> {
        //if (predictionModel && predictionModel === 'Forecasting') {
        return await post<SelectedPredictionDetails>("https://localhost:44378/api/predictions", { Ticker: ticker, PredictionModel: predictionModel, Range: range, StartDate: startDate })
        //} else {
        //    return getQutesForTicker(ticker, range)
        //}
    }
    getHistoricalPredictions(): Promise<HistoricalPrediction[]> {
        throw new Error('Method not implemented.');
    }
    getHistoricalPredictionDetails(id: string): Promise<SelectedPredictionDetails | null> {
        throw new Error('Method not implemented.');
    }

}

export default new PredictionsRepository();

