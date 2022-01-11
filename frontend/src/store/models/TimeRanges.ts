import { createModel } from '@rematch/core';
import { IRootModel } from '../models';
import { getTickers } from '../../api/MockedApi';

export interface TimeRangesState {
    avaliableRanges: string[],
    selectedRange: string;
}

const avaliableRanges: string[] = ['1 days', '1 week', '1 month', '1 year']

export const timeRanges = createModel<IRootModel>()({
    state: {
        avaliableRanges: avaliableRanges,
        selectedRange: ''
    } as TimeRangesState,

    reducers: {
        //addRanges(state: TimeRangesState) {
        //    return { avaliableRanges: avaliableRanges, selectedRange: state.selectedRange }
        //},

        setSelectedRange(state: TimeRangesState, range: string) {
            return { avaliableRanges: state.avaliableRanges, selectedRange: range }
        }
    }
})