import React from 'react';
import './Datepicker.css';


export interface DatepickerProps {
    name: string,
    setDate(date: string): void,
    value?: string,
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

export const Datepicker: React.FC<DatepickerProps> = ({ name, setDate, value, isDisabled }) => {
    return (
        <div className='datePicker'><input type='date' name='name' defaultValue={value ? value : getCurrentDate()} onChange={(e) => setDate(e.target.value)} onClick={(e) => { console.log(e) }} disabled={isDisabled} ></input></div>
    )
}
