import { DateRange } from 'react-date-range'
import { useState } from 'react'

import 'react-date-range/dist/styles.css';         // Main style file
import 'react-date-range/dist/theme/default.css';  // Theme CSS
import { useDateRange } from '../hooks/useDateRange';

const INTERVALS = [
    { value: '1m', label: '1 Minute' },
    { value: '5m', label: '5 Minutes' },
    { value: '15m', label: '15 Minutes' },
    { value: '30m', label: '30 Minutes' },
    { value: '1h', label: '1 Hour' },
    { value: '4h', label: '4 Hours' },
    { value: '1d', label: '1 Day' }
]

function Requests() {
    const [symbol, setSymbol] = useState('BTCUSDT')
    const [interval, setInterval] = useState('1h')

    const {
        dateRange,
        showCalendar,
        calendarRef,
        handleDateRangeChange,
        formatDateRange,
        toggleCalendar,
    } = useDateRange()


    return (
        <div className="app">
            <header>
                <h1>
                    <div className="brand-container">
                        <span className="brand-name">VeritasX</span>
                        <div className="brand-divider"></div>
                        <span className="brand-subtitle">Crypto Charts</span>
                    </div>
                </h1>
            </header>
            <main>
                <div className="controls">
                    <div className="control-group">
                        <label>Symbol:</label>
                        <input
                            type="text"
                            value={symbol}
                            onChange={(e) => setSymbol(e.target.value.toUpperCase())}
                            placeholder="BTCUSDT"
                        />
                    </div>
                    <div className="control-group">
                        <label>Interval:</label>
                        <select value={interval} onChange={(e) => setInterval(e.target.value)}>
                            {INTERVALS.map(int => (
                                <option key={int.value} value={int.value}>
                                    {int.label}
                                </option>
                            ))}
                        </select>
                    </div>
                    <div className="control-group">
                        <label>Date Range:</label>
                        <div className="date-input-wrapper" ref={calendarRef}>
                            <input
                                type="text"
                                value={formatDateRange()}
                                onClick={toggleCalendar}
                                readOnly
                                className="date-range-input"
                                placeholder="Select date range..."
                            />
                            {showCalendar &&
                                <div className="date-range-dropdown">
                                    <DateRange
                                        editableDateInputs={true}
                                        onChange={handleDateRangeChange}
                                        moveRangeOnFirstSelection={true}
                                        months={1}
                                        direction="horizontal"
                                        ranges={dateRange}
                                        maxDate={new Date()}
                                    />
                                </div>}
                        </div>
                    </div>
                    <button
                        // onClick={handleFetchData} 
                        // disabled={loading}
                        className="fetch-button"
                    >Create Request</button>
                </div>
                {/* {error && (
          <div className="error">
            {error}
          </div>
        )} */}
            </main>
        </div>
    );
}

export default Requests