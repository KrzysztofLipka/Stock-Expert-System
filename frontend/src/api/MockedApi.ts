import { PredictionPoint, SelectedPredictionDetails, HistoricalPrediction } from '../store/models/Prices'

import { } from '../store/models/Tickers'
import moment from 'moment'

const delay = (ms = 0) => new Promise((resolve) => setTimeout(resolve, ms));

export const mockedCurrentDate = moment('11-12-2020');
export const mockedCurrentDateAsString = '11-12-2020';

console.log(Date.prototype);


export async function getQuotesForTicker(ticker: string, range: string): Promise<SelectedPredictionDetails> {
    console.log('mocked call for quotes');

    await delay(3000);

    //const range = '1 day', '1 week', '2 weeks', '1 month'
    let timeRange: number;

    switch (range) {
        case '1 day':
            timeRange = 1
            break;
        case '1 week':
            timeRange = 7
            break;
        case '2 weeks':
            timeRange = 14
            break;
        case '1 month':
            timeRange = 31
            break;
        default:
            timeRange = 1;

    }


    const quotes = generateFakeQuotes(timeRange);
    const id = `i${state.length}`

    const res =
    {
        ticker: ticker,
        predictions: [
            ...quotes
        ],
        id: id
    }

    state.push({
        companyName: ticker,
        startDate: '11.11.1111',
        endDate: '15.11.1111',
        predictionId: id,
        predictedBuyPrice: 11.11,
        predictedSellPrice: 12.11,
        actualBuyPrice: 12.12,
        actualSellPrice: 22.11,
        //id: "1"
        //status: 'pending'
    })

    historicalPredictionsState.set(
        id,
        {
            ticker: ticker,
            predictions: [
                ...quotes
            ],
            id: id
        }
    )

    return res;

}

const state: HistoricalPrediction[] = [
    {
        companyName: 'AAPL',
        startDate: '10.11.2020',
        endDate: '22.11.2020',
        predictionId: 'i123',
        predictedBuyPrice: 11.11,
        predictedSellPrice: 12.11,
        actualBuyPrice: 12.12,
        actualSellPrice: 22.11,
        //id: "2"
        //status: 'pending'
    },
    {
        companyName: 'AAPL',
        startDate: '05.11.2020',
        endDate: '09.11.2020',
        predictionId: 'i124',
        predictedBuyPrice: 11.11,
        predictedSellPrice: 12.11,
        actualBuyPrice: 12.12,
        actualSellPrice: 22.11,
        //id: "3"
        //status: 'expired'
    }
]


const historicalPredictionsState: Map<string, SelectedPredictionDetails> = new Map();

const initHistoricalPredictionsState = () => {
    historicalPredictionsState.set('i123',
        {
            ticker: 'PLN/USD',
            predictions: [
                {
                    date: '10.11.2020',
                    predictedPrice: 5.0013,
                    actualPrice: 5.0021
                }, {
                    date: '11.11.2020',
                    predictedPrice: 5.0213,
                    actualPrice: 5.0092
                }, {
                    date: '12.11.2020',
                    predictedPrice: 5.0013
                }, {
                    date: '13.11.2020',
                    predictedPrice: 5.0027
                }, {
                    date: '14.11.2020',
                    predictedPrice: 5.0016
                }, {
                    date: '15.11.2020',
                    predictedPrice: 5.0273
                }, {
                    date: '16.11.2020',
                    predictedPrice: 5.0283
                }, {
                    date: '17.11.2020',
                    predictedPrice: 5.0283
                }, {
                    date: '18.11.2020',
                    predictedPrice: 5.0243
                }, {
                    date: '19.11.2020',
                    predictedPrice: 5.0213
                }, {
                    date: '20.11.2020',
                    predictedPrice: 5.0263
                }, {
                    date: '21.11.2020',
                    predictedPrice: 5.0293
                }, {
                    date: '22.11.2020',
                    predictedPrice: 5.0221
                }

            ], id: 'i123'
        });

    historicalPredictionsState.set('i124',
        {
            ticker: 'PLN/USD',
            predictions: [
                {
                    date: '05.11.2020',
                    predictedPrice: 13.00,
                    actualPrice: 12.22
                }, {
                    date: '06.11.2020',
                    predictedPrice: 14.00,
                    actualPrice: 11.20
                }, {
                    date: '07.11.2020',
                    predictedPrice: 11.00,
                    actualPrice: 10.25
                }, {
                    date: '08.11.2020',
                    predictedPrice: 11.00,
                    actualPrice: 10.25
                }, {
                    date: '09.11.2020',
                    predictedPrice: 11.00,
                    actualPrice: 10.25
                }

            ], id: 'i124'
        })


}

/*export async function getHistoricalPrediction(id: string): Promise<SelectedPredictionDetails> {
    console.log('mocked call for quotes');

    await delay(3000);

    const res =
    {
        ticker: 'CDPR',
        quotes: [
            ...generateFakeQuotes(17)
        ]
    }

    return res;

}*/

export async function getHistoricalPredictions(): Promise<HistoricalPrediction[]> {
    console.log('mocked call for historical predictions');

    await delay(3000);

    /*const res: HistoricalPrediction[] = [
        {
            ticker: 'AAPL',
            startDate: '11.11.1111',
            endDate: '15.11.1111',
            id: 'i123',
            status: 'pending'
        },
        {
            ticker: 'NIKE',
            startDate: '15.11.1111',
            endDate: '19.11.1111',
            id: 'i124',
            status: 'pending'
        }
    ]*/

    return state;

}

export async function getHistoricalPredictionDetails(id: string): Promise<SelectedPredictionDetails | null> {
    await delay(1500);

    const res = historicalPredictionsState.get(id);
    return res ? res : null;
}

export async function getTickers(): Promise<string[]> {
    console.log('mocked call for tickers');
    await delay(1000);

    return [
        'AAPL',
        'RDS-A',
    ]

}


export async function getPredictionModels(): Promise<string[]> {
    console.log('mocked call for prediction models');
    await delay(1000);

    return [
        'SSA (WindowsSize + TrainSize)', 'SSA (WindowsSize)', 'SSA (Constant Parameters)', 'ARIMA'
    ]

}

function generateFakeQuotes(days: number): PredictionPoint[] {
    let res: PredictionPoint[] = [];
    let counter = 0
    for (let i = 0; i < days; i++) {
        const date = moment(mockedCurrentDate).add(i, 'days');
        //console.log(moment(date).isoWeekday());
        //console.log(moment(date).add(i, 'days').calendar());
        if (isWeekendDay(date)) {
            res[counter] = { predictedPrice: Math.random() * 10, date: date.calendar() }
            counter++;
        }

    }
    console.log(res);
    return res;
}

const isWeekendDay = (date: moment.Moment): boolean => {
    return moment(date).isoWeekday() !== 6 && moment(date).isoWeekday() !== 7
}

initHistoricalPredictionsState();

