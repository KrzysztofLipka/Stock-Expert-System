import { init, RematchDispatch, RematchRootState } from '@rematch/core';
import { models, IRootModel } from './models';
import immerPlugin from '@rematch/immer';

export const store = init({
    /*plugins: [immerPlugin()]*/
    models,
})

export type Store = typeof store
export type Dispatch = RematchDispatch<IRootModel>
export type RootState = RematchRootState<IRootModel>