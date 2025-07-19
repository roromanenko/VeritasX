import { DateRange } from 'react-date-range'
import { useEffect, useState } from 'react'

import 'react-date-range/dist/styles.css';         // Main style file
import 'react-date-range/dist/theme/default.css';  // Theme CSS
import { useDateRange } from '../hooks/useDateRange';
import { useApiProvider } from '../services/apiProvider';

import type { DataCollectionJobDto } from '../api';
import { RequestItem } from '../components/RequestItem';

const INTERVALS = [
    { value: 1, label: '1 Minute' },
    { value: 5, label: '5 Minutes' },
    { value: 15, label: '15 Minutes' },
    { value: 30, label: '30 Minutes' },
    { value: 60, label: '1 Hour' },
    { value: 240, label: '4 Hours' },
    { value: 1440, label: '1 Day' }
]

function Requests() {
    const [symbol, setSymbol] = useState('BTCUSDT')
    const [interval, setInterval] = useState(60)
    const [loadingRquests, setLoadingRquests] = useState(false);

    const {
        dateRange,
        showCalendar,
        calendarRef,
        handleDateRangeChange,
        formatDateRange,
        toggleCalendar,
    } = useDateRange()

    const dataCollectionApi = useApiProvider().getDataCollectionApi();
    const [requests, setRequests] = useState<DataCollectionJobDto[]>([]);

    useEffect(() => {
        fetchRequestsData();
    }, []);

    async function fetchRequestsData(){
        setLoadingRquests(true);
        const response = await dataCollectionApi.apiDataCollectionJobsGet();
        const fetchRequests = response.data;
        setRequests(fetchRequests);
        setLoadingRquests(false);
    }

    async function handleFetchData(){
        const fromDate = dateRange[0].startDate.toISOString()
        const toDate = dateRange[0].endDate.toISOString()
        const postResponse = await dataCollectionApi.apiDataCollectionQueuePost(symbol, fromDate, toDate, interval)
        const newJobId = postResponse.data.jobId;
        if (newJobId){
            const newJobResponse = await dataCollectionApi.apiDataCollectionJobsJobIdGet(newJobId)
            setRequests(prev => [...prev, newJobResponse.data])
        }
    }

    return (
        <>
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
                        <select value={interval} onChange={(e) => setInterval(Number(e.target.value))}>
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
                        onClick={handleFetchData}
                        disabled={loadingRquests}
                        className="fetch-button"
                    >Create Request</button>
                </div>
                <div className="requests-list">
                    {requests.map((item, index) => (
                        <div className='request-container'>
                            <RequestItem request={item} />
                        </div>
                    ))}
                </div>
            </main>
        </>
    );
}

export default Requests