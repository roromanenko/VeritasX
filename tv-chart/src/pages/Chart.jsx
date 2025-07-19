import { useDataFetching } from '../hooks/useDataFetching'

const {
    candles,
    loading,
    error,
    fetchData,
} = useDataFetching()

const handleFetchData = () => {
    fetchData(symbol, dateRange, interval)
}

function Chart() {
    return (
        <div>
            <div className="chart-container">
                {loading && <p>Loading chart...</p>}
                {!loading && candles.length > 0 ? (
                    <CandlestickChart candles={candles} symbol={symbol} />
                ) : !loading && (
                    <p>Select parameters and click "Fetch Data" to load candlestick chart</p>
                )}
            </div>
            {candles.length > 0 && (
                <div className="stats">
                    <h3>Statistics</h3>
                    <p>Total candles: {candles.length}</p>
                    <p>Date range: {format(new Date(candles[0].openTime), 'MMM dd, yyyy')} - {format(new Date(candles[candles.length - 1].openTime), 'MMM dd, yyyy')}</p>
                </div>
            )}
        </div>
    )
}

export default Chart