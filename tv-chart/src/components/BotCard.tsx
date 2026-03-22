import { Bot, TrendingUp, Shield } from 'lucide-react';
import type { BotDto, BotStatus } from '../api';

type BotCardProps = {
    bot: BotDto;
    onClick: (id: string) => void;
    onStart?: (id: string) => void;
    onStop?: (id: string) => void;
};

const botStatusConfig: Record<string, { label: string; className: string }> = {
    Active:  { label: 'ACTIVE',   className: 'active'   },
    Pending: { label: 'PENDING',  className: 'pending'  },
    Stopped: { label: 'STOPPED',  className: 'stopped'  },
    Error:   { label: 'ERROR',    className: 'error'    },
};

function BotStatusBadge({ status }: { status: BotStatus | undefined }) {
    const cfg = status
        ? (botStatusConfig[status] ?? { label: status.toUpperCase(), className: 'stopped' })
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

export const BotCard = ({ bot, onClick, onStart, onStop }: BotCardProps) => {
    const canStart = bot.status === 'Stopped' || bot.status === 'Error';
    const canStop  = bot.status === 'Active'  || bot.status === 'Pending';
    const dateLabel = bot.startedAt
        ? `Started ${formatDate(bot.startedAt)}`
        : `Created ${formatDate(bot.createdAt)}`;

    return (
        <div
            className="request-container"
            onClick={() => bot.id && onClick(bot.id)}
        >
            <div className="request-card-header">
                <div className="request-card-header-top">
                    <div>
                        <div className="request-pair-row">
                            <span className="pair-name">{bot.symbol ?? '—'}</span>
                            <span className="exchange-badge">{bot.exchange}</span>
                        </div>
                        <p className="date-range">{dateLabel}</p>
                    </div>
                    <BotStatusBadge status={bot.status as BotStatus} />
                </div>

                <div className="request-card-stats">
                    <span className="stat-item">
                        <Bot />
                        {bot.name ?? 'Unnamed'}
                    </span>
                    <span className="stat-item">
                        <TrendingUp />
                        {bot.strategy?.type ?? '—'}
                    </span>
                    <span className="stat-item">
                        <Shield />
                        Risk: {bot.riskParameters?.positionSize ?? '—'}
                    </span>
                </div>
            </div>

            <div className="request-card-footer">
                <div className="footer-completed">
                    <div className="strategies-count">
                        <p>Strategy</p>
                        <p className="count-value">{bot.strategy?.type ?? '—'}</p>
                    </div>
                    <div style={{ display: 'flex', gap: '8px' }}>
                        {canStart && onStart && (
                            <button
                                className="btn btn-primary btn-sm"
                                onClick={(e) => { e.stopPropagation(); bot.id && onStart(bot.id); }}
                            >
                                Start
                            </button>
                        )}
                        {canStop && onStop && (
                            <button
                                className="btn btn-sm"
                                style={{ color: '#EF4444', borderColor: 'rgba(239,68,68,0.4)', background: 'rgba(239,68,68,0.08)' }}
                                onClick={(e) => { e.stopPropagation(); bot.id && onStop(bot.id); }}
                            >
                                Stop
                            </button>
                        )}
                        <button
                            className="btn btn-secondary btn-sm"
                            onClick={(e) => { e.stopPropagation(); bot.id && onClick(bot.id); }}
                        >
                            View Details
                        </button>
                    </div>
                </div>
            </div>

            {bot.status === 'Error' && bot.errorMessage && (
                <div className="request-card-footer footer-failed">
                    <p className="error-message">{bot.errorMessage}</p>
                </div>
            )}
        </div>
    );
};
