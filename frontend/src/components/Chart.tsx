import { LineChart, XAxis, Tooltip, CartesianGrid, Line, ResponsiveContainer, YAxis, Legend, Curve } from 'recharts'
import React from 'react';
import { SelectedPredictionDetails } from '../store/models/Prices'
import '../App.css'

interface CompanyProps {
    prices: SelectedPredictionDetails | undefined;
    isLoading: boolean;
}

export const Chart: React.FC<CompanyProps> = ({ prices, isLoading }) => {

    //if (prices?.length === 0) {
    //    return <></>
    //}
    //console.log(data)
    //console.log(prices.quotes)

    return (
        <div style={{ width: '100%', height: '500px', backgroundColor: 'white', border: '2px solid #e8ecef' }}>
            {isLoading && <div style={{ float: 'none', height: '500px', backgroundColor: 'black', width: '100%', zIndex: 20, opacity: 0.3, position: 'absolute', right: '0', left: '0' }}>
                <div className="lds-ring"><div></div><div></div><div></div><div></div></div>
            </div>}
            <ResponsiveContainer>
                <LineChart
                    onClick={(e: any) => console.log(e)}
                    //width={1500}
                    //height={300}
                    data={prices?.predictions}
                    margin={{
                        top: 5,
                        right: 30,
                        left: 20,
                        bottom: 5,
                    }}
                >
                    <CartesianGrid strokeDasharray="3 3" />
                    <XAxis dataKey="date" />
                    <YAxis domain={['dataMin - 0.01', 'dataMax + 0.01']} />
                    <Tooltip />
                    <Legend />
                    <Line type="monotone" dataKey="predictedPrice" stroke="#FF6A3D" activeDot={{ r: 2 }} strokeWidth={3} dot={{ r: 2 }} />
                    <Line type="monotone" dataKey="actualPrice" stroke="#1A2238" activeDot={{ r: 4 }} strokeWidth={3} dot={{ r: 2 }} />
                    <Curve type='basis' />


                </LineChart>
            </ResponsiveContainer>
        </div>

    )

}