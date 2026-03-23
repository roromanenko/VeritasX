import { useEffect, useRef, useState } from 'react';
import { Link, useParams } from 'react-router-dom';
import { HubConnection, HubConnectionState } from '@microsoft/signalr';

import { useApiProvider } from '../services/apiProvider';
import type { BotDto, BotTradeRecordDto, BotStatus, OrderSide, UpdateBotRequest } from '../api';

type LogEntry = {
    id: number;
    timestamp: string;
    message: string;
    type: 'status' | 'trade' | 'error' | 'info';
};

type ConnectionStatus = 'idle' | 'connecting' | 'connected' | 'disconnected';

const botStatusConfig: Record<string, { label: string; className: string }> = {
    Active:  { label: 'ACTIVE',  className: 'active'  },
    Pending: { label: 'PENDING', className: 'pending' },
    Stopped: { label: 'STOPPED', className: 'stopped' },
    Error:   { label: 'ERROR',   className: 'error'   },
};

function BotStatusBadge({ status }: { status: BotStatus | undefined }) {
    const cfg = status
        ? (botStatusConfig[status] ?? { label: String(status).toUpperCase(), className: 'stopped' })
        : { label: 'UNKNOWN', className: 'stopped' };
    return (
        <span className={`status-badge ${cfg.className}`}>
            <span className="status-dot" />
            {cfg.label}
        </span>
    );
}

function formatDate(iso: string | null | undefined): string {
    if (!iso) return '—';
    return new Date(iso).toLocaleDateString(undefined, {
        year: 'numeric', month: 'short', day: 'numeric',
    });
}

function formatTime(iso: string | null | undefined): string {
    if (!iso) return '—';
    return new Date(iso).toLocaleString(undefined, {
        month: 'short', day: 'numeric',
        hour: '2-digit', minute: '2-digit',
    });
}

export const BotDetail = () => {
    const { id } = useParams<{ id: string }>();

    const [bot, setBot] = useState<BotDto | null>(null);
    const [trades, setTrades] = useState<BotTradeRecordDto[]>([]);
    const [loading, setLoading] = useState(false);
    const [actionLoading, setActionLoading] = useState(false);
    const [logEntries, setLogEntries] = useState<LogEntry[]>([]);
    const [connectionStatus, setConnectionStatus] = useState<ConnectionStatus>('idle');
    const [isEditing, setIsEditing] = useState(false);
    const [editForm, setEditForm] = useState<{
        name: string;
        positionSize: string;
        stopLoss: string;
        takeProfit: string;
        strategyParameters: Record<string, string>;
    }>({ name: '', positionSize: '', stopLoss: '', takeProfit: '', strategyParameters: {} });

    const connectionRef = useRef<HubConnection | null>(null);
    const logEndRef = useRef<HTMLDivElement>(null);
    const logIdRef = useRef(0);

    const apiProvider = useApiProvider();

    const addLog = (message: string, type: LogEntry['type'] = 'info') => {
        const entry: LogEntry = {
            id: ++logIdRef.current,
            timestamp: new Date().toLocaleTimeString(),
            message,
            type,
        };
        setLogEntries(prev => [...prev, entry]);
    };

    // Auto-scroll log
    useEffect(() => {
        logEndRef.current?.scrollIntoView({ behavior: 'smooth' });
    }, [logEntries]);

    // Fetch bot data and trades
    useEffect(() => {
        if (!id) return;
        fetchData();
    }, [id]);

    async function fetchData() {
        setLoading(true);
        try {
            const [botRes, tradesRes] = await Promise.all([
                apiProvider.getBotsApi().apiBotsIdGet(id!),
                apiProvider.getBotsApi().apiBotsIdTradesGet(id!, 5),
            ]);
            const botData = (botRes.data as unknown as { data: BotDto }).data;
            const tradesData = (tradesRes.data as unknown as { data: BotTradeRecordDto[] }).data ?? [];
            if (botData) setBot(botData);
            setTrades(tradesData);
        } finally {
            setLoading(false);
        }
    }

    // SignalR auto-connect
    useEffect(() => {
        if (!id) return;
        connectToHub();
        return () => { teardownHub(); };
    }, [id]);

    async function connectToHub() {
        const connection = apiProvider.getBotProgressHub();
        connectionRef.current = connection;

        connection.onreconnecting(() => {
            setConnectionStatus('connecting');
            addLog('Reconnecting…', 'error');
        });
        connection.onreconnected(() => {
            setConnectionStatus('connected');
            addLog('Reconnected.', 'status');
        });
        connection.onclose(() => {
            setConnectionStatus('disconnected');
            addLog('Connection closed.', 'error');
        });

        connection.on('BotStatusChanged', (data: { status: string; error?: string }) => {
            const msg = `STATUS: ${data.status}${data.error ? ` — ${data.error}` : ''}`;
            addLog(msg, 'status');
            setBot(prev => prev ? { ...prev, status: data.status as BotStatus, errorMessage: data.error } : prev);
        });

        connection.on('TradeExecuted', (data: { side: string; quantity: number; symbol: string; price: number; reason: string }) => {
            addLog(`TRADE: ${data.side} ${data.quantity} ${data.symbol} @ ${data.price} — ${data.reason}`, 'trade');
            const newTrade: BotTradeRecordDto = {
                side: data.side as OrderSide,
                quantity: data.quantity,
                symbol: data.symbol,
                price: data.price,
                reason: data.reason,
                executedAt: new Date().toISOString(),
            };
            setTrades(prev => [newTrade, ...prev].slice(0, 5));
        });

        setConnectionStatus('connecting');
        addLog(`Connecting to bot ${id}…`, 'info');

        try {
            await connection.start();
            await connection.invoke('JoinGroup', id);
            setConnectionStatus('connected');
            addLog('Connected.', 'status');
        } catch (err) {
            setConnectionStatus('disconnected');
            addLog(`Connection failed: ${err}`, 'error');
        }
    }

    async function handleStart() {
        setActionLoading(true);
        try {
            const res = await apiProvider.getBotsApi().apiBotsIdStartPost(id!);
            const updated = (res.data as unknown as { data: BotDto }).data;
            if (updated) setBot(updated);
            addLog('Bot started.', 'status');
        } finally {
            setActionLoading(false);
        }
    }

    async function handleStop() {
        setActionLoading(true);
        try {
            const res = await apiProvider.getBotsApi().apiBotsIdStopPost(id!);
            const updated = (res.data as unknown as { data: BotDto }).data;
            if (updated) setBot(updated);
            addLog('Bot stopped.', 'status');
        } finally {
            setActionLoading(false);
        }
    }

    function handleEdit() {
        if (!bot) return;
        setEditForm({
            name: bot.name ?? '',
            positionSize: bot.riskParameters?.positionSize != null ? String(bot.riskParameters.positionSize) : '',
            stopLoss: bot.riskParameters?.stopLoss != null ? String(bot.riskParameters.stopLoss) : '',
            takeProfit: bot.riskParameters?.takeProfit != null ? String(bot.riskParameters.takeProfit) : '',
            strategyParameters: bot.strategy?.parameters
                ? Object.fromEntries(Object.entries(bot.strategy.parameters).map(([k, v]) => [k, String(v)]))
                : {},
        });
        setIsEditing(true);
    }

    function handleCancelEdit() {
        setIsEditing(false);
    }

    async function handleSave() {
        setActionLoading(true);
        try {
            const request: UpdateBotRequest = {
                name: editForm.name || undefined,
                riskParameters: {
                    positionSize: editForm.positionSize !== '' ? parseFloat(editForm.positionSize) : undefined,
                    stopLoss: editForm.stopLoss !== '' ? parseFloat(editForm.stopLoss) : null,
                    takeProfit: editForm.takeProfit !== '' ? parseFloat(editForm.takeProfit) : null,
                },
                strategyParameters: Object.keys(editForm.strategyParameters).length > 0
                    ? editForm.strategyParameters
                    : undefined,
            };
            const res = await apiProvider.getBotsApi().apiBotsIdPut(id!, request);
            const updated = (res.data as unknown as { data: BotDto }).data;
            if (updated) setBot(updated);
            setIsEditing(false);
            addLog('Bot configuration updated.', 'status');
        } catch (err) {
            addLog(`Update failed: ${err}`, 'error');
        } finally {
            setActionLoading(false);
        }
    }

    async function teardownHub() {
        const connection = connectionRef.current;
        if (connection?.state === HubConnectionState.Connected) {
            try {
                await connection.invoke('LeaveGroup', id);
                await connection.stop();
            } catch {
                // ignore errors on unmount
            }
        }
        connectionRef.current = null;
    }

    if (!id) return null;

    const strategyParams = bot?.strategy?.parameters
        ? Object.entries(bot.strategy.parameters)
        : [];

    return (
        <main>
            {/* Page header */}
            <div className="chart-page-header">
                <Link to="/bots" className="back-link">← Back to Bots</Link>
                <div style={{ display: 'flex', alignItems: 'center', gap: '12px', flexWrap: 'wrap' }}>
                    <h1>{bot?.name ?? bot?.symbol ?? 'Bot'}</h1>
                    <BotStatusBadge status={bot?.status as BotStatus} />
                    {bot && (bot.status === 'Stopped' || bot.status === 'Error') && (
                        <button
                            className="btn btn-primary btn-sm"
                            disabled={actionLoading}
                            onClick={handleStart}
                        >
                            {actionLoading ? 'Starting…' : 'Start'}
                        </button>
                    )}
                    {bot && (bot.status === 'Active' || bot.status === 'Pending') && (
                        <button
                            className="btn btn-sm"
                            style={{ color: '#EF4444', borderColor: 'rgba(239,68,68,0.4)', background: 'rgba(239,68,68,0.08)' }}
                            disabled={actionLoading}
                            onClick={handleStop}
                        >
                            {actionLoading ? 'Stopping…' : 'Stop'}
                        </button>
                    )}
                    {bot && (
                        <button
                            className="btn btn-sm btn-secondary"
                            disabled={actionLoading || bot.status === 'Active' || bot.status === 'Pending'}
                            onClick={isEditing ? handleCancelEdit : handleEdit}
                        >
                            {isEditing ? 'Cancel' : 'Edit'}
                        </button>
                    )}
                </div>
                <p className="chart-subtitle">
                    {[bot?.exchange, bot?.symbol, bot?.strategy?.type].filter(Boolean).join(' · ')}
                </p>
            </div>

            {loading && !bot && (
                <div className="chart-loading" style={{ padding: '40px 0' }}>Loading bot details…</div>
            )}

            {bot && isEditing && (
                <div className="bot-edit-panel">
                    {/* Name */}
                    <div className="control-group" style={{ marginBottom: '20px' }}>
                        <label>Bot Name</label>
                        <input
                            type="text"
                            value={editForm.name}
                            onChange={e => setEditForm(f => ({ ...f, name: e.target.value }))}
                            placeholder="Bot name"
                        />
                    </div>

                    {/* Risk Parameters */}
                    <p className="bot-edit-section-label">Risk Parameters</p>
                    <div className="bot-edit-params-grid">
                        <div className="control-group">
                            <label>Position Size</label>
                            <input
                                type="number"
                                value={editForm.positionSize}
                                onChange={e => setEditForm(f => ({ ...f, positionSize: e.target.value }))}
                                placeholder="e.g. 0.01"
                                min="0"
                                step="any"
                            />
                        </div>
                        <div className="control-group">
                            <label>Stop Loss</label>
                            <input
                                type="number"
                                value={editForm.stopLoss}
                                onChange={e => setEditForm(f => ({ ...f, stopLoss: e.target.value }))}
                                placeholder="e.g. 0.02"
                                min="0"
                                step="any"
                            />
                        </div>
                        <div className="control-group">
                            <label>Take Profit</label>
                            <input
                                type="number"
                                value={editForm.takeProfit}
                                onChange={e => setEditForm(f => ({ ...f, takeProfit: e.target.value }))}
                                placeholder="e.g. 0.04"
                                min="0"
                                step="any"
                            />
                        </div>
                    </div>

                    {/* Strategy Parameters */}
                    {Object.keys(editForm.strategyParameters).length > 0 && (
                        <>
                            <p className="bot-edit-section-label">Strategy Parameters</p>
                            <div className="bot-edit-strategy-grid">
                                {Object.entries(editForm.strategyParameters).map(([key, value]) => (
                                    <div key={key} className="control-group">
                                        <label>{key}</label>
                                        <input
                                            type="text"
                                            value={value}
                                            onChange={e => setEditForm(f => ({
                                                ...f,
                                                strategyParameters: { ...f.strategyParameters, [key]: e.target.value },
                                            }))}
                                        />
                                    </div>
                                ))}
                            </div>
                        </>
                    )}

                    <div className="form-actions">
                        <button
                            className="btn btn-primary btn-sm"
                            disabled={actionLoading}
                            onClick={handleSave}
                        >
                            {actionLoading ? 'Saving…' : 'Save Changes'}
                        </button>
                        <button
                            className="btn btn-secondary btn-sm"
                            disabled={actionLoading}
                            onClick={handleCancelEdit}
                        >
                            Cancel
                        </button>
                    </div>
                </div>
            )}

            {bot && (
                <div className="bot-detail-layout">
                    {/* Left column */}
                    <div className="bot-detail-left">

                        {/* Configuration */}
                        <div className="chart-card" style={{ marginBottom: 0 }}>
                            <div className="chart-card-header">
                                <span className="section-label">Configuration</span>
                            </div>
                            <div className="bot-detail-params-grid">
                                <div>
                                    <p className="metric-label">Exchange</p>
                                    <p className="metric-value neutral">{bot.exchange ?? '—'}</p>
                                </div>
                                <div>
                                    <p className="metric-label">Symbol</p>
                                    <p className="metric-value neutral">{bot.symbol ?? '—'}</p>
                                </div>
                                <div>
                                    <p className="metric-label">Created</p>
                                    <p className="metric-value neutral">{formatDate(bot.createdAt)}</p>
                                </div>
                                <div>
                                    <p className="metric-label">Started</p>
                                    <p className="metric-value neutral">{formatDate(bot.startedAt)}</p>
                                </div>
                                {bot.stoppedAt && (
                                    <div>
                                        <p className="metric-label">Stopped</p>
                                        <p className="metric-value neutral">{formatDate(bot.stoppedAt)}</p>
                                    </div>
                                )}
                            </div>
                            {bot.status === 'Error' && bot.errorMessage && (
                                <div style={{ padding: '0 20px 16px' }}>
                                    <p className="error-message" style={{ margin: 0 }}>{bot.errorMessage}</p>
                                </div>
                            )}
                        </div>

                        {/* Strategy */}
                        <div className="chart-card" style={{ marginBottom: 0 }}>
                            <div className="chart-card-header">
                                <span className="section-label">Strategy</span>
                                <span className="exchange-badge">{bot.strategy?.type ?? '—'}</span>
                            </div>
                            {strategyParams.length > 0 ? (
                                <div className="bot-detail-params-grid">
                                    {strategyParams.map(([key, value]) => (
                                        <div key={key}>
                                            <p className="metric-label">{key}</p>
                                            <p className="metric-value neutral">{value}</p>
                                        </div>
                                    ))}
                                </div>
                            ) : (
                                <div style={{ padding: '20px' }}>
                                    <p className="date-range">No parameters configured</p>
                                </div>
                            )}
                        </div>

                        {/* Risk Parameters */}
                        <div className="chart-card" style={{ marginBottom: 0 }}>
                            <div className="chart-card-header">
                                <span className="section-label">Risk Parameters</span>
                            </div>
                            <div className="bot-detail-params-grid">
                                <div>
                                    <p className="metric-label">Position Size</p>
                                    <p className="metric-value">{bot.riskParameters?.positionSize ?? '—'}</p>
                                </div>
                                <div>
                                    <p className="metric-label">Stop Loss</p>
                                    <p className="metric-value neutral">
                                        {bot.riskParameters?.stopLoss != null ? bot.riskParameters.stopLoss : '—'}
                                    </p>
                                </div>
                                <div>
                                    <p className="metric-label">Take Profit</p>
                                    <p className="metric-value neutral">
                                        {bot.riskParameters?.takeProfit != null ? bot.riskParameters.takeProfit : '—'}
                                    </p>
                                </div>
                            </div>
                        </div>

                    </div>

                    {/* Right column */}
                    <div className="bot-detail-right">

                        {/* Recent Trades */}
                        <div className="chart-card" style={{ marginBottom: 0 }}>
                            <div className="chart-card-header">
                                <span className="section-label">Recent Trades</span>
                                <span className="date-range">{trades.length > 0 ? `Last ${trades.length}` : 'No trades yet'}</span>
                            </div>
                            {trades.length > 0 ? (
                                <div className="bot-trade-list">
                                    {trades.map((trade, i) => (
                                        <div key={trade.id ?? i} className="bot-trade-row">
                                            <span className={`trade-side trade-side--${trade.side?.toLowerCase()}`}>
                                                {trade.side}
                                            </span>
                                            <span className="pair-name">{trade.symbol}</span>
                                            <span className="bot-trade-amount">
                                                {trade.quantity} @ {trade.price}
                                            </span>
                                            <div className="bot-trade-row-meta">
                                                <div>{trade.reason}</div>
                                                <div>{formatTime(trade.executedAt)}</div>
                                            </div>
                                        </div>
                                    ))}
                                </div>
                            ) : (
                                <div style={{ padding: '24px 20px' }}>
                                    <p className="date-range">No trades recorded</p>
                                </div>
                            )}
                        </div>

                        {/* Live Activity */}
                        <div className="chart-card" style={{ marginBottom: 0, overflow: 'visible' }}>
                            <div className="chart-card-header">
                                <span className="section-label">Live Activity</span>
                                <span className={`bot-connection-pill bot-connection-pill--${connectionStatus}`}>
                                    <span className="bot-connection-dot" />
                                    {connectionStatus}
                                </span>
                            </div>
                            <div className="bot-monitor-log">
                                {logEntries.length === 0 && (
                                    <div className="bot-monitor-log-empty">Connecting…</div>
                                )}
                                {logEntries.map(entry => (
                                    <div key={entry.id} className={`bot-monitor-log-entry bot-monitor-log-entry--${entry.type}`}>
                                        <span className="bot-monitor-log-time">[{entry.timestamp}]</span>
                                        <span className="bot-monitor-log-message">{entry.message}</span>
                                    </div>
                                ))}
                                <div ref={logEndRef} />
                            </div>
                        </div>

                    </div>
                </div>
            )}
        </main>
    );
};
