import { CollectionState, DataCollectionJobDto } from "../api";
import { useEffect, useState } from "react";
import { HubConnectionState } from '@microsoft/signalr';
import { useApiProvider } from "../services/apiProvider";
import { Database, BarChart3, Clock } from 'lucide-react';

type RequestItemProps = {
    request: DataCollectionJobDto;
    onViewResults?: (id: string) => void;
};

const statusConfig: Record<string, { label: string; className: string }> = {
    [CollectionState.Completed]:  { label: 'COMPLETED',   className: 'completed'  },
    [CollectionState.InProgress]: { label: 'PROCESSING',  className: 'in-progress' },
    [CollectionState.Pending]:    { label: 'PENDING',     className: 'pending'    },
    [CollectionState.Failed]:     { label: 'FAILED',      className: 'failed'     },
    [CollectionState.Cancelled]:  { label: 'CANCELLED',   className: 'cancelled'  },
};

function StatusBadge({ state }: { state: CollectionState | undefined }) {
    const cfg = state ? statusConfig[state] : statusConfig[CollectionState.Pending];
    return (
        <span className={`status-badge ${cfg.className}`}>
            <span className="status-dot" />
            {cfg.label}
        </span>
    );
}

function formatDate(iso: string | undefined): string {
    if (!iso) return '';
    return new Date(iso).toLocaleDateString(undefined, {
        year: 'numeric', month: 'short', day: 'numeric'
    });
}

export const RequestItem = ({ request, onViewResults }: RequestItemProps) => {
    const [requestItem, setRequestItem] = useState<DataCollectionJobDto>(request);

    const connection = useApiProvider().getJobProgressHub();

    useEffect(() => {
        const startConnection = async () => {
            try {
                await connection.start();
                console.log('SignalR Connected');
                await connection.invoke('JoinGroup', requestItem.id);
                connection.on('JobProgress', (update) => {
                    const newJobStatus = update as DataCollectionJobDto;
                    setRequestItem(newJobStatus);
                });
            } catch (err) {
                console.error('SignalR Connection Error: ', err);
            }
        };

        if (connection.state == HubConnectionState.Disconnected) {
            startConnection();
        }

        return () => {
            if (connection && connection.state == HubConnectionState.Connected) {
                connection.invoke('LeaveGroup', requestItem.id);
                connection.stop();
            }
        };
    }, [requestItem.id]);

    const isCompleted = requestItem.state === CollectionState.Completed;
    const isInProgress = requestItem.state === CollectionState.InProgress;
    const isFailed = requestItem.state === CollectionState.Failed;

    const progressPct = requestItem.totalChunks && requestItem.totalChunks > 0
        ? Math.round(((requestItem.completedChunks ?? 0) / requestItem.totalChunks) * 100)
        : 0;

    return (
        <div className={`request-container ${!isCompleted ? 'not-clickable' : ''}`}>
            {/* Header */}
            <div className="request-card-header">
                <div className="request-card-header-top">
                    <div>
                        <div className="request-pair-row">
                            <span className="pair-name">{requestItem.symbol}</span>
                            <span className="exchange-badge">Binance</span>
                        </div>
                        <p className="date-range">
                            {formatDate(requestItem.fromUtc)} — {formatDate(requestItem.toUtc)}
                        </p>
                    </div>
                    <StatusBadge state={requestItem.state} />
                </div>
                <div className="request-card-stats">
                    <span className="stat-item">
                        <Database />
                        {((requestItem.totalChunks ?? 0)).toLocaleString()} chunks
                    </span>
                    <span className="stat-item">
                        <BarChart3 />
                        {requestItem.interval}
                    </span>
                    <span className="stat-item">
                        <Clock />
                        {formatDate(requestItem.createdAt)}
                    </span>
                </div>
            </div>

            {/* Footer: completed */}
            {isCompleted && (
                <div className="request-card-footer">
                    <div className="footer-completed">
                        <div className="strategies-count">
                            <p>Chunks Loaded</p>
                            <p className="count-value">
                                {requestItem.completedChunks ?? 0}/{requestItem.totalChunks ?? 0}
                            </p>
                        </div>
                        {onViewResults && requestItem.id && (
                            <button
                                className="btn btn-secondary btn-sm"
                                onClick={(e) => { e.stopPropagation(); onViewResults(requestItem.id!); }}
                            >
                                View Results
                            </button>
                        )}
                    </div>
                </div>
            )}

            {/* Footer: in progress */}
            {isInProgress && (
                <div className="request-card-footer">
                    <div className="progress-wrapper">
                        <div className="progress-track">
                            <div className="progress-fill" style={{ width: `${progressPct}%` }} />
                        </div>
                        <span className="progress-label">{progressPct}%</span>
                    </div>
                    <p className="progress-hint">Loading historical data...</p>
                </div>
            )}

            {/* Footer: failed */}
            {isFailed && (
                <div className="request-card-footer footer-failed">
                    <p className="error-message">
                        {requestItem.errorMessage
                            ? `Error: ${requestItem.errorMessage}`
                            : 'Failed to load data. Please try again.'}
                    </p>
                    <button className="btn btn-secondary btn-sm">Retry</button>
                </div>
            )}
        </div>
    );
};
