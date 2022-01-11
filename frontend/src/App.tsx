import './App.css';
import { useSelector, shallowEqual } from 'react-redux'
import { RootState } from './store/store';
import { Chart } from './components/Chart'
import { Header } from './components/Header'
import { InputPanel } from './components/InputPanel';
import { HistoricalPredictionsGrid } from './components/HistoricalPredictionsGrid'
import Box from '@mui/material/Box';
import Paper from '@mui/material/Paper';
import Stack from '@mui/material/Stack';
import { styled } from '@mui/material/styles';



function App() {
  const predictions = useSelector((state: RootState) => state.predictions, shallowEqual)
  let isLogin = false;


  const loginForm = (): JSX.Element => {
    return <div className='login-window'>
      <div className='login-container'>fffffffff</div>
    </div>
  }
  return (
    <div className="App">

      {isLogin ? loginForm() :
        <><Header />
          <div className='app-wraper'>
            <Stack spacing={2}>
              <InputPanel />
              <Chart prices={predictions.selectedPrediction} isLoading={predictions.isLoading} />
              <HistoricalPredictionsGrid predictions={predictions.historicalPredictions} />
            </Stack>
          </div>
        </>}
    </div>
  );
}

export default App;
