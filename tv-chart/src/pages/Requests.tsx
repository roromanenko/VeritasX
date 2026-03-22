import { useEffect, useState } from 'react'
import { Plus, Database } from 'lucide-react'

import { useApiProvider } from '../services/apiProvider'

import type { DataCollectionJobDto } from '../api'
import { RequestItem } from '../components/RequestItem'
import { useNavigate } from 'react-router-dom'

const INTERVALS = [
    { value: 1,    label: '1 Minute' },
    { value: 5,    label: '5 Minutes' },
    { value: 15,   label: '15 Minutes' },
    { value: 30,   label: '30 Minutes' },
    { value: 60,   label: '1 Hour' },
    { value: 240,  label: '4 Hours' },
    { value: 1440, label: '1 Day' },
]

export const Requests = () => {
    const [symbol, setSymbol] = useState('BTCUSDT')
    const [interval, setInterval] = useState(60)
    const [loadingRequests, setLoadingRequests] = useState(false)
    const [requests, setRequests] = useState<DataCollectionJobDto[]>([])
    const [showCreateForm, setShowCreateForm] = useState(false)
    const today = new Date()
    const sevenDaysAgo = new Date(today)
    sevenDaysAgo.setDate(today.getDate() - 7)
    const [startDate, setStartDate] = useState(sevenDaysAgo.toISOString().split('T')[0])
    const [endDate, setEndDate] = useState(today.toISOString().split('T')[0])

    const navigate = useNavigate()
    const dataCollectionApi = useApiProvider().getDataCollectionApi()

    useEffect(() => {
        fetchRequestsData()
    }, [])

    async function fetchRequestsData() {
        setLoadingRequests(true)
        const response = await dataCollectionApi.apiDataCollectionJobsGet()
        setRequests(response.data)
        setLoadingRequests(false)
    }

    async function handleFetchData() {
        setLoadingRequests(true)
        const fromDate = new Date(startDate).toISOString()
        const toDate = new Date(endDate).toISOString()
        const postResponse = await dataCollectionApi.apiDataCollectionQueuePost({
            symbol,
            fromUtc: fromDate,
            toUtc: toDate,
            intervalMinutes: interval,
        })
        const newJobId = postResponse.data.jobId
        if (newJobId) {
            const newJobResponse = await dataCollectionApi.apiDataCollectionJobsJobIdGet(newJobId)
            setRequests(prev => [...prev, newJobResponse.data])
        }
        setLoadingRequests(false)
        setShowCreateForm(false)
    }

    function handleRequestClick(requestId: string) {
        navigate(`/requests/${requestId}`)
    }

    return (
        <main>
            {/* Page header */}
            <div className="page-header">
                <div className="page-header-text">
                    <h1>Backtest Library</h1>
                    <p>Load historical data and test your strategies</p>
                </div>
                <button
                    className="btn btn-primary"
                    onClick={() => setShowCreateForm(prev => !prev)}
                >
                    <Plus className="btn-icon" />
                    New Data Request
                </button>
            </div>

            {/* Collapsible create form */}
            {showCreateForm && (
                <div className="create-form-panel">
                    <h3>Load Historical Data</h3>
                    <div className="create-form-grid">
                        <div className="control-group">
                            <label>Symbol</label>
                            <input
                                type="text"
                                value={symbol}
                                onChange={(e) => setSymbol(e.target.value.toUpperCase())}
                                placeholder="BTCUSDT"
                            />
                        </div>
                        <div className="control-group">
                            <label>Interval</label>
                            <select
                                value={interval}
                                onChange={(e) => setInterval(Number(e.target.value))}
                            >
                                {INTERVALS.map(int => (
                                    <option key={int.value} value={int.value}>
                                        {int.label}
                                    </option>
                                ))}
                            </select>
                        </div>
                        <div className="control-group">
                            <label>Start Date</label>
                            <input
                                type="date"
                                value={startDate}
                                onChange={(e) => setStartDate(e.target.value)}
                            />
                        </div>
                        <div className="control-group">
                            <label>End Date</label>
                            <input
                                type="date"
                                value={endDate}
                                onChange={(e) => setEndDate(e.target.value)}
                            />
                        </div>
                    </div>
                    <div className="form-actions">
                        <button
                            className="btn btn-primary"
                            onClick={handleFetchData}
                            disabled={loadingRequests}
                        >
                            Load Data
                        </button>
                        <button
                            className="btn btn-ghost"
                            onClick={() => setShowCreateForm(false)}
                        >
                            Cancel
                        </button>
                        <span className="form-hint">Estimated candles vary by interval</span>
                    </div>
                </div>
            )}

            {/* Cards grid */}
            {requests.length > 0 ? (
                <div className="request-grid">
                    {requests.map((item) => (
                        <div
                            key={item.id}
                            onClick={() => item.id && handleRequestClick(item.id)}
                        >
                            <RequestItem
                                request={item}
                                onViewResults={(id) => navigate(`/requests/${id}`)}
                            />
                        </div>
                    ))}
                </div>
            ) : (
                !loadingRequests && (
                    <div className="empty-state">
                        <Database />
                        <p>No historical data loaded yet</p>
                        <button
                            className="btn btn-primary"
                            onClick={() => setShowCreateForm(true)}
                        >
                            <Plus className="btn-icon" />
                            Load Your First Dataset
                        </button>
                    </div>
                )
            )}
        </main>
    )
}
