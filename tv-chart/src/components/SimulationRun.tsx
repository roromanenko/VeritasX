import { useEffect, useState } from "react";
import { TradeOnHistoryDataRequest, TradingResultDto } from "../api";
import { useApiProvider } from "../services/apiProvider";
import { TrendingUp, CheckCircle2 } from "lucide-react";

type SimulationRunProps = {
    jobId: string;
};

export const SimulationRun = ({ jobId }: SimulationRunProps) => {
    if (!jobId) return <div>No job ID provided.</div>;

    const [simulationLoading, setSimulationLoading] = useState(false);
    const [simulationRequest, setSimulationRequest] = useState<TradeOnHistoryDataRequest>({
        targetWeight: 0.5,
        threshold: 0.1,
        minQty: undefined,
        minNotional: undefined,
        jobId: jobId,
        initBaselineQuantity: 1,
    });
    const [simulationResult, setSimulationResult] = useState<TradingResultDto | null>(null);
    const tradingApi = useApiProvider().getTradingApi();

    useEffect(() => {
        setSimulationRequest(prev => ({ ...prev, jobId }));
    }, [jobId]);

    const handleSimulationInputChange = (field: keyof TradeOnHistoryDataRequest, value: string) => {
        setSimulationRequest(prev => ({
            ...prev,
            [field]: value === ''
                ? (field === 'minQty' || field === 'minNotional' ? undefined : 0)
                : field === 'jobId' ? value : Number(value),
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
            jobId,
            initBaselineQuantity: 1,
        });
    };

    const vsHold = simulationResult
        ? (simulationResult.endTotalInBaseline ?? 0) - (simulationResult.justHoldTotalInBaseline ?? 0)
        : 0;

    return (
        <div className="strategy-card">
            {/* Card Header */}
            <div className="strategy-card-header">
                <div style={{ display: 'flex', alignItems: 'center', gap: '12px' }}>
                    <div className="strategy-icon-wrap">
                        <TrendingUp size={20} />
                    </div>
                    <div>
                        <h4 className="strategy-name">Mean Reversion Strategy</h4>
                        <p className="strategy-type">Baseline Rebalancing</p>
                    </div>
                </div>
                {simulationLoading && <div className="strategy-spinner" />}
                {simulationResult && !simulationLoading && (
                    <CheckCircle2 size={20} style={{ color: 'var(--color-primary)', flexShrink: 0 }} />
                )}
            </div>

            {/* Not-run: parameter form */}
            {!simulationResult && !simulationLoading && (
                <>
                    <div className="strategy-form-grid">
                        <div className="form-group">
                            <label htmlFor="targetWeight">Target Weight</label>
                            <input
                                id="targetWeight" type="number" step="0.01" min="0" max="1"
                                value={simulationRequest.targetWeight}
                                onChange={(e) => handleSimulationInputChange('targetWeight', e.target.value)}
                                className="form-input"
                            />
                        </div>
                        <div className="form-group">
                            <label htmlFor="threshold">Threshold</label>
                            <input
                                id="threshold" type="number" step="0.01" min="0"
                                value={simulationRequest.threshold}
                                onChange={(e) => handleSimulationInputChange('threshold', e.target.value)}
                                className="form-input"
                            />
                        </div>
                        <div className="form-group">
                            <label htmlFor="minQty">Min Quantity</label>
                            <input
                                id="minQty" type="number" step="0.00001" min="0"
                                value={simulationRequest.minQty || ''}
                                onChange={(e) => handleSimulationInputChange('minQty', e.target.value)}
                                className="form-input" placeholder="Optional"
                            />
                        </div>
                        <div className="form-group">
                            <label htmlFor="minNotional">Min Notional</label>
                            <input
                                id="minNotional" type="number" step="0.01" min="0"
                                value={simulationRequest.minNotional || ''}
                                onChange={(e) => handleSimulationInputChange('minNotional', e.target.value)}
                                className="form-input" placeholder="Optional"
                            />
                        </div>
                        <div className="form-group">
                            <label htmlFor="initBaselineQuantity">Initial Baseline Qty</label>
                            <input
                                id="initBaselineQuantity" type="number" step="0.1" min="1"
                                value={simulationRequest.initBaselineQuantity}
                                onChange={(e) => handleSimulationInputChange('initBaselineQuantity', e.target.value)}
                                className="form-input"
                            />
                        </div>
                    </div>
                    <button className="btn btn-primary btn-full" onClick={handleStartSimulation}>
                        Run Simulation
                    </button>
                </>
            )}

            {/* Running state */}
            {simulationLoading && (
                <div className="strategy-running">
                    <div className="strategy-progress-bar">
                        <div className="strategy-progress-fill" />
                    </div>
                    <p className="strategy-running-hint">Running simulation...</p>
                    <button className="btn btn-ghost btn-full" disabled>Running...</button>
                </div>
            )}

            {/* Completed state */}
            {simulationResult && !simulationLoading && (
                <>
                    <div className="strategy-results-grid">
                        <div>
                            <p className="metric-label">Profit</p>
                            <p className={`metric-value ${(simulationResult.profitInBaseline ?? 0) >= 0 ? '' : 'negative'}`}>
                                {(simulationResult.profitInBaseline ?? 0) >= 0 ? '+' : ''}
                                {simulationResult.profitInBaseline?.toFixed(4)}
                            </p>
                        </div>
                        <div>
                            <p className="metric-label">vs Hold</p>
                            <p className={`metric-value ${vsHold >= 0 ? '' : 'negative'}`}>
                                {vsHold >= 0 ? '+' : ''}{vsHold.toFixed(4)}
                            </p>
                        </div>
                        <div>
                            <p className="metric-label">Start Total</p>
                            <p className="metric-value neutral">
                                {simulationResult.startTotalInBaseline?.toFixed(4)}
                            </p>
                        </div>
                        <div>
                            <p className="metric-label">End Total</p>
                            <p className="metric-value neutral">
                                {simulationResult.endTotalInBaseline?.toFixed(4)}
                            </p>
                        </div>
                    </div>
                    <button className="btn btn-secondary btn-full" onClick={resetSimulation}>
                        Run New Simulation
                    </button>
                </>
            )}
        </div>
    );
};
