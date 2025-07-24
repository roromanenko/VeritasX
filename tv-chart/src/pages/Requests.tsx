import { MouseEventHandler, useEffect, useState } from 'react'

import 'react-date-range/dist/styles.css';         // Main style file
import 'react-date-range/dist/theme/default.css';  // Theme CSS
import { useApiProvider } from '../services/apiProvider';

import type { DataCollectionJobDto } from '../api';
import { RequestItem } from '../components/RequestItem';
import { CustomDateRange, useCustomDateRange } from '../components/CustomDateRange';
import { useNavigate } from 'react-router-dom';

const INTERVALS = [
    { value: 1, label: '1 Minute' },
    { value: 5, label: '5 Minutes' },
    { value: 15, label: '15 Minutes' },
    { value: 30, label: '30 Minutes' },
    { value: 60, label: '1 Hour' },
    { value: 240, label: '4 Hours' },
    { value: 1440, label: '1 Day' }
]

export const Requests = () => {
    const [symbol, setSymbol] = useState('BTCUSDT')
    const [interval, setInterval] = useState(60)
    const [loadingRequests, setLoadingRequests] = useState(false);
    const [requests, setRequests] = useState<DataCollectionJobDto[]>([]);
    const {dateRange, onDateRangeChange} = useCustomDateRange();

    const navigate = useNavigate();
    const dataCollectionApi = useApiProvider().getDataCollectionApi();

    useEffect(() => {
        fetchRequestsData();
    }, []);

    async function fetchRequestsData(){
        setLoadingRequests(true);
        const response = await dataCollectionApi.apiDataCollectionJobsGet();
        const fetchRequests = response.data;
        setRequests(fetchRequests);
        setLoadingRequests(false);
    }

    async function handleFetchData(){
        setLoadingRequests(true);
        const fromDate = dateRange.startDate.toISOString()
        const toDate = dateRange.endDate.toISOString()
        const postResponse = await dataCollectionApi.apiDataCollectionQueuePost({
            symbol,
            fromUtc: fromDate,
            toUtc: toDate,
            intervalMinutes: interval
        })
        const newJobId = postResponse.data.jobId;
        if (newJobId){
            const newJobResponse = await dataCollectionApi.apiDataCollectionJobsJobIdGet(newJobId)
            setRequests(prev => [...prev, newJobResponse.data])
        }
        setLoadingRequests(false);
    }

    function handleRequestClick(requestId: string) {
        navigate(`/requests/${requestId}`);
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
                        <CustomDateRange 
                            dateRange={dateRange}
                            onDateRangeChange={onDateRangeChange}
                        />
                    </div>
                    <button
                        onClick={handleFetchData}
                        disabled={loadingRequests}
                        className="primary-button fetch-button">
                            Create Request
                    </button>
                </div>
                <div className="request-list">
                    {requests.map((item, index) => (
                        <div className='request-container'
                            onClick={() => handleRequestClick(item.id!)}>
                            <RequestItem request={item} />
                        </div>
                    ))}
                </div>
            </main>
        </>
    );
}
