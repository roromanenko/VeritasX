import { useEffect, useRef, useState } from 'react';
import { HubConnection, HubConnectionState } from '@microsoft/signalr';
import { useApiProvider } from '../services/apiProvider';

type LogEntry = {
    id: number;
    timestamp: string;
    message: string;
    type: 'status' | 'trade' | 'error' | 'info';
};

type ConnectionStatus = 'idle' | 'connecting' | 'connected' | 'disconnected';

export const BotMonitor = () => {
    const [botId, setBotId] = useState('');
    const [logEntries, setLogEntries] = useState<LogEntry[]>([]);
    const [connectionStatus, setConnectionStatus] = useState<ConnectionStatus>('idle');
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

    useEffect(() => {
        logEndRef.current?.scrollIntoView({ behavior: 'smooth' });
    }, [logEntries]);

    useEffect(() => {
        return () => {
            if (connectionRef.current?.state === HubConnectionState.Connected) {
                connectionRef.current.stop();
            }
        };
    }, []);

    const connect = async () => {
        if (!botId.trim()) {
            addLog('Enter a Bot ID before connecting.', 'error');
            return;
        }

        const connection = apiProvider.getBotProgressHub();
        connectionRef.current = connection;

        connection.onreconnecting(() => {
            setConnectionStatus('connecting');
            addLog('Reconnecting...', 'error');
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
            const msg = `STATUS: ${data.status}${data.error ? ` | Error: ${data.error}` : ''}`;
            addLog(msg, 'status');
        });

        connection.on('TradeExecuted', (data: { side: string; quantity: number; symbol: string; price: number; reason: string }) => {
            addLog(`TRADE: ${data.side} ${data.quantity} ${data.symbol} @ ${data.price} | ${data.reason}`, 'trade');
        });

        setConnectionStatus('connecting');
        addLog(`Connecting to bot ${botId}...`, 'info');

        try {
            await connection.start();
            await connection.invoke('JoinGroup', botId);
            setConnectionStatus('connected');
            addLog(`Connected and joined group for bot ${botId}.`, 'status');
        } catch (err) {
            setConnectionStatus('disconnected');
            addLog(`Connection failed: ${err}`, 'error');
        }
    };

    const disconnect = async () => {
        const connection = connectionRef.current;
        if (connection?.state === HubConnectionState.Connected) {
            await connection.invoke('LeaveGroup', botId);
            await connection.stop();
        }
        setConnectionStatus('disconnected');
        addLog('Disconnected.', 'info');
    };

    const isConnected = connectionStatus === 'connected';
    const isConnecting = connectionStatus === 'connecting';

    return (
        <div className="bot-monitor">
            <h2 className="bot-monitor-title">Bot Monitor</h2>

            <div className="controls bot-monitor-controls">
                <div className="control-group">
                    <label htmlFor="botId">Bot ID</label>
                    <input
                        id="botId"
                        type="text"
                        placeholder="Enter Bot ID"
                        value={botId}
                        onChange={e => setBotId(e.target.value)}
                        disabled={isConnected || isConnecting}
                    />
                </div>

                <div className="control-group bot-monitor-actions">
                    <label>Connection</label>
                    <div className="bot-monitor-buttons">
                        <button
                            className="primary-button fetch-button"
                            onClick={connect}
                            disabled={isConnected || isConnecting}
                        >
                            {isConnecting ? 'Connecting...' : 'Connect'}
                        </button>
                        <button
                            className="primary-button fetch-button bot-monitor-disconnect"
                            onClick={disconnect}
                            disabled={!isConnected && !isConnecting}
                        >
                            Disconnect
                        </button>
                    </div>
                </div>

                <div className="control-group">
                    <label>Status</label>
                    <span className={`bot-monitor-status bot-monitor-status--${connectionStatus}`}>
                        {connectionStatus.charAt(0).toUpperCase() + connectionStatus.slice(1)}
                    </span>
                </div>
            </div>

            <div className="bot-monitor-log">
                {logEntries.length === 0 && (
                    <div className="bot-monitor-log-empty">No activity yet. Connect to a bot to start monitoring.</div>
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
    );
};
