import { createModel } from '@rematch/core';
import { IRootModel } from '../models';
import { getTickers } from '../../api/MockedApi';

export interface TickersState {
    avaliableTickers: string[],
    selectedTicker: string
}

export const tickers = createModel<IRootModel>()({
    state: {
        avaliableTickers: [],
        selectedTicker: ''
    } as TickersState,

    reducers: {
        addTickers(state: TickersState, tickers: string[]) {
            return { avaliableTickers: tickers, selectedTicker: state.selectedTicker }
        },

        setSelectedTicker(state: TickersState, ticker: string) {
            return { avaliableTickers: state.avaliableTickers, selectedTicker: ticker }
        }
    },

    effects: (dispatch: any) => ({
        async loadAvaliableTickers() {
            const p = await getTickers();
            dispatch.tickers.addTickers(p);
        }
    })
})