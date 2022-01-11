import React from 'react';
import './Datepicker.css';

import TextField from '@mui/material/TextField';
import AdapterMoment from '@mui/lab/AdapterMoment';
import LocalizationProvider from '@mui/lab/LocalizationProvider';
import DatePicker from '@mui/lab/DatePicker';
import InputLabel from '@mui/material/InputLabel';
import moment from 'moment';


export interface DatepickerProps {
    name: string,
    className: string
    setDate(date: string): void,
    value?: moment.Moment,
    isDisabled?: boolean,
}

const getCurrentDate = (): string => {
    let today = new Date();
    let dd = today.getDate();

    let mm = today.getMonth() + 1;
    var yyyy = today.getFullYear();


    const daysFormatted = dd < 10 ? '0' + dd.toString : dd.toString();
    const monthsFormatted = mm < 10 ? '0' + mm.toString() : mm.toString();

    return `${yyyy}-${monthsFormatted}-${daysFormatted}`

}

export const Datepicker: React.FC<DatepickerProps> = ({ name, className, setDate, value, isDisabled }) => {

    const [datepickerValue, setDatepickerValue] = React.useState(value);

    const handleDateChange = (newValue: any) => {
        setDate(newValue);
        setDatepickerValue(newValue);
    }

    return (
        <LocalizationProvider dateAdapter={AdapterMoment}>
            <DatePicker
                className={className}
                label="Select Date"
                value={datepickerValue}
                onChange={(newValue) => {
                    handleDateChange(newValue);
                }}
                renderInput={(params) => <TextField {...params} helperText={null} />}
            />
        </LocalizationProvider>

    )
}

//<div className='datePicker'><input type='date' name='name' defaultValue={value ? value : getCurrentDate()} onChange={(e) => setDate(e.target.value)} onClick={(e) => { console.log(e) }} disabled={isDisabled} ></input></div>
