import React, { useState } from 'react';
import './Dropdown.css';

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

    const handleDropdownOptionClick = (option: string) => {
        setIsDropdownOpen(false);
        setSelectedOption(option);
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

    return (
        <div className='dropdown'>
            {/*<label htmlFor={className}>Male</label>*/}
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
    )
}