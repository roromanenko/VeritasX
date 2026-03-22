import { useEffect, useState } from "react"
import CandlestickChart from "../components/CandlestickChart"
import { CandleDto, CollectionState, DataCollectionJobDto } from "../api";
import { format } from "date-fns";
import { useParams, Link } from "react-router-dom";
import { useApiProvider } from "../services/apiProvider";
import { SimulationRun } from "../components/SimulationRun";
import { ChevronLeft } from "lucide-react";

const statusConfig: Record<string, { label: string; color: string; bgColor: string }> = {
    [CollectionState.Completed]:  { label: 'COMPLETED',  color: '#10B981', bgColor: 'rgba(16, 185, 129, 0.1)' },
    [CollectionState.InProgress]: { label: 'PROCESSING', color: '#3B82F6', bgColor: 'rgba(59, 130, 246, 0.1)' },
    [CollectionState.Pending]:    { label: 'PENDING',    color: '#F59E0B', bgColor: 'rgba(245, 158, 11, 0.1)' },
    [CollectionState.Failed]:     { label: 'FAILED',     color: '#EF4444', bgColor: 'rgba(239, 68, 68, 0.1)' },
    [CollectionState.Cancelled]:  { label: 'CANCELLED',  color: '#6B7280', bgColor: 'rgba(107, 114, 128, 0.1)' },
};

function StatusBadge({ state }: { state: CollectionState | undefined }) {
    const cfg = state ? statusConfig[state] : statusConfig[CollectionState.Pending];
    return (
        <span
            className="status-badge-inline"
            style={{ color: cfg.color, backgroundColor: cfg.bgColor, border: `1px solid ${cfg.color}40` }}
        >
            <span className="status-dot-inline" style={{ backgroundColor: cfg.color }} />
            {cfg.label}
        </span>
    );
}

function formatDate(iso: string | undefined): string {
    if (!iso) return '';
    return format(new Date(iso), 'MMM dd, yyyy');
}

export const Chart = () => {
    const { id } = useParams();
    if (!id) return <div>Id is required</div>;

    const [loading, setLoading] = useState(false);
    const [candles, setCandles] = useState<CandleDto[]>([]);
    const [requestInfo, setRequestInfo] = useState<DataCollectionJobDto>({});

    const dataCollectionApi = useApiProvider().getDataCollectionApi();

    useEffect(() => { fetchRequestInfo() }, []);

    async function fetchRequestInfo() {
        setLoading(true);
        try {
            const infoResponse = await dataCollectionApi.apiDataCollectionJobsJobIdGet(id!);
            if (infoResponse.data) setRequestInfo(infoResponse.data);
            const dataResponse = await dataCollectionApi.apiDataCollectionDataJobIdGet(id!);
            if (dataResponse.data) setCandles(dataResponse.data);
        } finally {
            setLoading(false);
        }
    }

    const highPrice = candles.length > 0
        ? Math.max(...candles.map(c => c.high ?? 0))
        : 0;
    const lowPrice = candles.length > 0
        ? Math.min(...candles.filter(c => c.low !== undefined).map(c => c.low!))
        : 0;

    return (
        <main>
            {/* Page Header */}
            <div className="chart-page-header">
                <Link to="/requests" className="back-link">
                    <ChevronLeft size={12} />
                    Back to Backtests
                </Link>
                <h1>{requestInfo.symbol ?? '—'} Historical Data</h1>
                <p className="chart-subtitle">
                    Binance
                    {requestInfo.interval ? ` • ${requestInfo.interval} interval` : ''}
                    {requestInfo.fromUtc && requestInfo.toUtc
                        ? ` • ${formatDate(requestInfo.fromUtc)} — ${formatDate(requestInfo.toUtc)}`
                        : ''}
                    {candles.length > 0 ? ` • ${candles.length.toLocaleString()} candles` : ''}
                </p>
                <StatusBadge state={requestInfo.state} />
            </div>

            {/* Chart Card */}
            <div className="chart-card">
                <div className="chart-card-header">
                    <span className="section-label">Price Chart</span>
                    {candles.length > 0 && (
                        <div className="chart-high-low">
                            <span>High: ${highPrice.toLocaleString(undefined, { maximumFractionDigits: 2 })}</span>
                            <span>Low: ${lowPrice.toLocaleString(undefined, { maximumFractionDigits: 2 })}</span>
                        </div>
                    )}
                </div>
                <div className="chart-body">
                    {loading && <p className="chart-loading">Loading chart...</p>}
                    {!loading && candles.length > 0 && (
                        <CandlestickChart candles={candles} symbol={requestInfo.symbol} />
                    )}
                    {!loading && candles.length === 0 && (
                        <p className="chart-loading">No data found</p>
                    )}
                </div>
            </div>

            {/* Strategies Section */}
            <div className="strategies-section">
                <div className="strategies-header">
                    <span className="section-label">Test Strategies on This Data</span>
                    <div className="section-divider" />
                </div>
                <SimulationRun jobId={id} />
            </div>
        </main>
    );
}
