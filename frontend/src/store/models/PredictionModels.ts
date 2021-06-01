import { createModel } from '@rematch/core';
import { IRootModel } from '../models';
import { getPredictionModels } from '../../api/MockedApi';

export interface PredictionModelsState {
    avaliableModels: string[],
    selectedModel: string
}

export const predictionModels = createModel<IRootModel>()({
    state: {
        avaliableModels: [],
        selectedModel: ''
    } as PredictionModelsState,

    reducers: {
        addPredictionModels(state: PredictionModelsState, predictionModels: string[]) {
            return { avaliableModels: predictionModels, selectedModel: state.selectedModel }
        },

        setSelectedPredictionModel(state: PredictionModelsState, predictionModel: string) {
            return { avaliableModels: state.avaliableModels, selectedModel: predictionModel }
        }
    },

    effects: (dispatch: any) => ({
        async loadAvaliablePredictionModels() {
            const request = await getPredictionModels();
            dispatch.predictionModels.addPredictionModels(request);
        }
    })
})