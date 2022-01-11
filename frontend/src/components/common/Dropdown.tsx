import React, { useState } from 'react';
import './Dropdown.css';
import Box from '@mui/material/Box';
import InputLabel from '@mui/material/InputLabel';
import MenuItem from '@mui/material/MenuItem';
import FormControl from '@mui/material/FormControl';
import Select from '@mui/material/Select';

export interface DropdownProps {
    options: string[],
    selectedOption: string,
    defaultText: string,
    noDataText: string,
    setSelectedOption(option: string): void;
    className: string;
    disabled?: boolean;
}

export const Dropdown: React.FC<DropdownProps> = ({ options, selectedOption, setSelectedOption, defaultText, noDataText, className, disabled }) => {

    const [isDropdownOpen, setIsDropdownOpen] = useState<boolean>(false);
    const [value, setSelectedValue] = useState<string>('default')

    const handleDropdownOptionClick = (option: string) => {
        //setIsDropdownOpen(false);
        setSelectedOption(option);
        setSelectedValue(option);
        //setTitle(option);
        console.log(option);
    }

    const renderDropdownOption = (option: string) => {
        return <div key={option} className='dropdown-option' onClick={() => handleDropdownOptionClick(option)}> {option} </div>
    }

    const getButtonTitle = (): string => {

        if (selectedOption) {
            return selectedOption
        }

        if (options.length === 0) {
            return noDataText
        }

        return defaultText

    }

    const handleChange = (event: any) => {
        console.log(event.target.value);
        handleDropdownOptionClick(event.target.value);
    };

    return (
        <>
            <FormControl sx={{ m: 1, minWidth: 120 }}>
                <InputLabel id="demo-simple-select-label">{defaultText}</InputLabel>
                <Select
                    labelId="demo-simple-select-label"
                    id="demo-simple-select"
                    value={value}
                    label={defaultText}
                    onChange={handleChange}
                >
                    {options.map(option => <MenuItem value={option}>{option}</MenuItem>)}
                </Select>
            </FormControl>


        </>
    )
}

/* 
<div className='dropdown'>
                
                <button className={`dropdown-button ${className}`} disabled={options.length === 0 || disabled} onClick={() => setIsDropdownOpen(isDropdownOpen ? false : true)}>
                    {getButtonTitle()}
                </button>

                {isDropdownOpen
                    ? (
                        <div className="dropdown-menu">
                            {options.map(option => renderDropdownOption(option))}
                        </div>
                    )
                    : null}
            </div>
*/