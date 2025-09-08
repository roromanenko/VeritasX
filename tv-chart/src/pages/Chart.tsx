import { useEffect, useState } from "react"
import CandlestickChart from "../components/CandlestickChart"
import { CandleDto, DataCollectionJobDto, TradeOnHistoryDataRequest, TradingResultDto } from "../api";
import { format } from "date-fns";
import { useParams } from "react-router-dom";
import { useApiProvider } from "../services/apiProvider";

export const Chart = () => {
    const { id } = useParams();
    if (!id) {
        return (
            <div>Id is required</div>
        )
    }
    const [loading, setLoading] = useState(false);
    const [candles, setCandels] = useState<CandleDto[]>([]);
    const [requestInfo, setRequestInfo] = useState<DataCollectionJobDto>({});

    const [simulationLoading, setSimulationLoading] = useState(false);
    const [simulationRequest, setSimulationRequest] = useState<TradeOnHistoryDataRequest>({
        targetWeight: 0.5,
        threshold: 0.1,
        minQty: undefined,
        minNotional: undefined,
        jobId: id,
        initBaselineQuantity: 1
    });
    const [simulationResult, setSimulationResult] = useState<TradingResultDto | null>(null);

    const dataCollectionApi = useApiProvider().getDataCollectionApi();
    const tradingApi = useApiProvider().getTradingApi();

    useEffect(() => {
        fetchRequestInfo()
    }, [])

    useEffect(() => {
        setSimulationRequest(prev => ({ ...prev, jobId: id }));
    }, [id]);

    async function fetchRequestInfo() {
        setLoading(true)
        try {
            const infoResponse = await dataCollectionApi.apiDataCollectionJobsJobIdGet(id!);
            if (infoResponse.data) {
                setRequestInfo(infoResponse.data)
            }
            const dataResponse = await dataCollectionApi.apiDataCollectionDataJobIdGet(id!)
            if (dataResponse.data) {
                setCandels(dataResponse.data)
            }
        }
        finally {
            setLoading(false)
        }
    }

    const handleSimulationInputChange = (field: keyof TradeOnHistoryDataRequest, value: string) => {
        setSimulationRequest(prev => ({
            ...prev,
            [field]: value === '' ? (field === 'minQty' || field === 'minNotional' ? undefined : 0) :
                field === 'jobId' ? value : Number(value)
        }));
    };
    const handleStartSimulation = async () => {
        setSimulationLoading(true);
        setSimulationResult(null);
        try {
            const result = await tradingApi.apiTradingStartHistoryCheckPost(simulationRequest);
            setSimulationResult(result.data);
        } catch (error) {
            console.error('Simulation failed:', error);
        } finally {
            setSimulationLoading(false);
        }
    };

    const resetSimulation = () => {
        setSimulationResult(null);
        setSimulationRequest({
            targetWeight: 0.5,
            threshold: 0.1,
            minQty: undefined,
            minNotional: undefined,
            jobId: id,
            initBaselineQuantity: 1
        });
    };

    return (
        <div>
            <div className="chart-container">
                {loading && <p>Loading chart...</p>}
                {!loading && candles.length > 0 ? (
                    <CandlestickChart candles={candles} symbol={requestInfo.symbol} />
                ) : !loading && (
                    <p>Data not found</p>
                )}
            </div>

            {candles.length > 0 && (
                <div className="stats">
                    <h3>Statistics</h3>
                    <p>Total candles: {candles.length}</p>
                    <p>Date range: {format(new Date(requestInfo.fromUtc!), 'MMM dd, yyyy')} - {format(new Date(requestInfo.toUtc!), 'MMM dd, yyyy')}</p>
                    <p>Interval: {requestInfo.interval}</p>
                </div>
            )}

            {/* Trading Simulation Section */}
            <div className="simulation-section">

                {!simulationResult && (
                    <div className="simulation-form">
                        <h3>Trading Simulation Parameters</h3>

                        <div className="form-grid">
                            <div className="form-group">
                                <label htmlFor="targetWeight">Target Weight</label>
                                <input
                                    id="targetWeight"
                                    type="number"
                                    step="0.01"
                                    min="0"
                                    max="1"
                                    value={simulationRequest.targetWeight}
                                    onChange={(e) => handleSimulationInputChange('targetWeight', e.target.value)}
                                    className="form-input"
                                />
                            </div>

                            <div className="form-group">
                                <label htmlFor="threshold">Threshold</label>
                                <input
                                    id="threshold"
                                    type="number"
                                    step="0.01"
                                    min="0"
                                    value={simulationRequest.threshold}
                                    onChange={(e) => handleSimulationInputChange('threshold', e.target.value)}
                                    className="form-input"
                                />
                            </div>

                            <div className="form-group">
                                <label htmlFor="minQty">Min Quantity (Optional)</label>
                                <input
                                    id="minQty"
                                    type="number"
                                    step="0.00001"
                                    min="0"
                                    value={simulationRequest.minQty || ''}
                                    onChange={(e) => handleSimulationInputChange('minQty', e.target.value)}
                                    className="form-input"
                                    placeholder="Optional"
                                />
                            </div>

                            <div className="form-group">
                                <label htmlFor="minNotional">Min Notional (Optional)</label>
                                <input
                                    id="minNotional"
                                    type="number"
                                    step="0.01"
                                    min="0"
                                    value={simulationRequest.minNotional || ''}
                                    onChange={(e) => handleSimulationInputChange('minNotional', e.target.value)}
                                    className="form-input"
                                    placeholder="Optional"
                                />
                            </div>

                            <div className="form-group">
                                <label htmlFor="initBaselineQuantity">Initial Baseline Quantity</label>
                                <input
                                    id="initBaselineQuantity"
                                    type="number"
                                    step="0.1"
                                    min="1"
                                    value={simulationRequest.initBaselineQuantity}
                                    onChange={(e) => handleSimulationInputChange('initBaselineQuantity', e.target.value)}
                                    className="form-input"
                                />
                            </div>
                        </div>

                        <div className="form-actions">
                            <button
                                className="simulation-button secondary"
                                onClick={resetSimulation}
                                disabled={simulationLoading}
                            >
                                Cancel
                            </button>
                            <button
                                className="simulation-button primary"
                                onClick={handleStartSimulation}
                                disabled={simulationLoading}
                            >
                                {simulationLoading ? 'Running Simulation...' : 'Start Simulation'}
                            </button>
                        </div>
                    </div>
                )}

                {simulationResult && (
                    <div className="simulation-results">
                        <h3>Simulation Results</h3>

                        <div className="results-grid">
                            <div className="result-item">
                                <span className="result-label">Start Total (Baseline)</span>
                                <span className="result-value">{simulationResult.startTotalInBaseline?.toFixed(2)}</span>
                            </div>

                            <div className="result-item">
                                <span className="result-label">End Total (Baseline)</span>
                                <span className="result-value">{simulationResult.endTotalInBaseline?.toFixed(2)}</span>
                            </div>

                            <div className="result-item profit">
                                <span className="result-label">Profit (Baseline)</span>
                                <span className={`result-value ${simulationResult.profitInBaseline! >= 0 ? 'positive' : 'negative'}`}>
                                    {simulationResult.profitInBaseline! >= 0 ? '+' : ''}{simulationResult.profitInBaseline?.toFixed(2)}
                                </span>
                            </div>

                            <div className="result-item">
                                <span className="result-label">Just Hold Total (Baseline)</span>
                                <span className="result-value">{simulationResult.justHoldTotalInBaseline?.toFixed(2)}</span>
                            </div>
                        </div>

                        <div className="performance-comparison">
                            <div className="comparison-item">
                                <span className="comparison-label">Strategy vs Hold</span>
                                <span className={`comparison-value ${((simulationResult.endTotalInBaseline ?? 0) - (simulationResult.justHoldTotalInBaseline ?? 0)) >= 0 ? 'positive' : 'negative'
                                    }`}>
                                    {((simulationResult.endTotalInBaseline ?? 0) - (simulationResult.justHoldTotalInBaseline ?? 0)) >= 0 ? '+' : ''}
                                    {((simulationResult.endTotalInBaseline ?? 0) - (simulationResult.justHoldTotalInBaseline ?? 0)).toFixed(2)}
                                </span>
                            </div>
                        </div>

                        <div className="form-actions">
                            <button
                                className="simulation-button secondary"
                                onClick={resetSimulation}
                            >
                                Run New Simulation
                            </button>
                        </div>
                    </div>
                )}
            </div>
        </div>
    )
}
