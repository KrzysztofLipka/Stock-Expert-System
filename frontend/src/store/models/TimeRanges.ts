import { createModel } from '@rematch/core';
import { IRootModel } from '../models';
import { getTickers } from '../../api/MockedApi';

export interface TimeRangesState {
    avaliableRanges: string[],
    selectedRange: string;
}

const avaliableRanges: string[] = ['1 day', '1 week', '2 weeks', '1 month']

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