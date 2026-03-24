import { useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import { Wallet, Bot, TrendingUp, TrendingDown, Activity, ChevronDown } from 'lucide-react'
import { PieChart, Pie, Cell, Tooltip, ResponsiveContainer } from 'recharts'

import { useApiProvider } from '../services/apiProvider'
import type {
    GetAccountStatisticsResponse,
    GetPortfolioSnapshotResponse,
    BotStatisticsSummaryDto,
    ExchangePortfolioDto,
    BotDto,
} from '../api'
import '../styles/dashboard.css'

// ── Formatting helpers ────────────────────────────────────────

function fmtUsd(val: number | null | undefined, decimals = 2): string {
    if (val == null) return '—'
    return '$' + val.toLocaleString(undefined, {
        minimumFractionDigits: decimals,
        maximumFractionDigits: decimals,
    })
}

function fmtPct(val: number | null | undefined): string {
    if (val == null) return '—'
    return (val * 100).toFixed(1) + '%'
}

function fmtSigned(val: number | null | undefined): string {
    if (val == null) return '—'
    const prefix = val >= 0 ? '+' : ''
    return prefix + fmtUsd(val)
}

function fmtAlloc(val: number | null | undefined): string {
    if (val == null) return '—'
    return val.toFixed(1) + '%'
}

// ── Chart colors ─────────────────────────────────────────────

const CHART_COLORS = ['#10B981', '#3B82F6', '#F59E0B', '#8B5CF6', '#EC4899', '#14B8A6']

// ── Sub-components ────────────────────────────────────────────

interface SummaryCardProps {
    icon: React.ReactNode
    iconBg: string
    label: string
    value: string
    valueClass?: string
    subtext: string
    loading: boolean
}

function SummaryCard({ icon, iconBg, label, value, valueClass, subtext, loading }: SummaryCardProps) {
    return (
        <div className="dashboard-metric-card">
            <div className="dashboard-metric-card-header">
                <div className="dashboard-metric-icon" style={{ background: iconBg }}>
                    {icon}
                </div>
                <span className="dashboard-metric-label">{label}</span>
            </div>
            {loading ? (
                <div className="dashboard-skeleton" />
            ) : (
                <div className={`dashboard-metric-value ${valueClass ?? ''}`}>{value}</div>
            )}
            <div className="dashboard-metric-subtext">{subtext}</div>
        </div>
    )
}

interface ExchangeBreakdownProps {
    portfolio: GetPortfolioSnapshotResponse | null
    accountStats: GetAccountStatisticsResponse | null
    loading: boolean
}

function ExchangeBreakdown({ portfolio, accountStats, loading }: ExchangeBreakdownProps) {
    const pieData = (portfolio?.byExchange ?? []).map(e => ({
        name: e.exchange ?? '?',
        value: e.equityUsd ?? 0,
    }))

    return (
        <div className="dashboard-card">
            <div className="dashboard-card-header">
                <span className="dashboard-card-title">Wallet Allocation</span>
            </div>
            <div className="dashboard-card-body">
                {loading ? (
                    <p style={{ color: 'var(--color-text-secondary)', fontSize: 13 }}>Loading...</p>
                ) : pieData.length === 0 ? (
                    <p style={{ color: 'var(--color-text-secondary)', fontSize: 13 }}>
                        No connected exchanges
                    </p>
                ) : (
                    <div className="dashboard-exchange-breakdown">
                        <div className="dashboard-exchange-chart">
                            <ResponsiveContainer width={200} height={200}>
                                <PieChart>
                                    <Pie
                                        data={pieData}
                                        cx="50%"
                                        cy="50%"
                                        innerRadius={62}
                                        outerRadius={90}
                                        paddingAngle={3}
                                        dataKey="value"
                                        strokeWidth={0}
                                    >
                                        {pieData.map((_, i) => (
                                            <Cell key={i} fill={CHART_COLORS[i % CHART_COLORS.length]} />
                                        ))}
                                    </Pie>
                                    <Tooltip
                                        formatter={(v: number) => [fmtUsd(v), 'Equity']}
                                        contentStyle={{
                                            background: '#1A1A1A',
                                            border: '1px solid #1F2937',
                                            borderRadius: 8,
                                            fontFamily: 'JetBrains Mono',
                                            fontSize: 12,
                                        }}
                                        labelStyle={{ color: '#E5E7EB' }}
                                    />
                                </PieChart>
                            </ResponsiveContainer>
                            <div className="dashboard-exchange-chart-label">
                                <span className="dashboard-exchange-chart-label-value">
                                    {fmtUsd(portfolio?.totalEquityUsd)}
                                </span>
                                <span className="dashboard-exchange-chart-label-sub">total</span>
                            </div>
                        </div>

                        <div className="dashboard-exchange-list">
                            {(portfolio?.byExchange ?? []).map((ex, i) => {
                                const acctEx = accountStats?.byExchange?.find(
                                    e => e.exchange === ex.exchange
                                )
                                return (
                                    <div key={ex.exchange ?? i} className="dashboard-exchange-item">
                                        <div
                                            className="dashboard-exchange-dot"
                                            style={{ background: CHART_COLORS[i % CHART_COLORS.length] }}
                                        />
                                        <span className="dashboard-exchange-name">{ex.exchange}</span>
                                        <div className="dashboard-exchange-meta">
                                            <span className="dashboard-exchange-equity">{fmtUsd(ex.equityUsd)}</span>
                                            <div style={{ display: 'flex', gap: 6, alignItems: 'center' }}>
                                                <span className="dashboard-alloc-badge">{fmtAlloc(ex.allocationPercent)}</span>
                                                {acctEx && (
                                                    <span style={{ fontSize: 10, color: 'var(--color-text-tertiary)' }}>
                                                        {acctEx.activeBotCount ?? 0} active / {acctEx.botCount ?? 0} bots
                                                    </span>
                                                )}
                                            </div>
                                        </div>
                                    </div>
                                )
                            })}
                        </div>
                    </div>
                )}
            </div>
        </div>
    )
}

function BotStatusBadge({ status }: { status: string | null | undefined }) {
    const s = (status ?? 'Stopped').toLowerCase() as 'active' | 'pending' | 'stopped' | 'error'
    return (
        <span className={`dashboard-bot-status dashboard-bot-status--${s}`}>
            <span className="dashboard-bot-status-dot" />
            {status ?? 'Stopped'}
        </span>
    )
}

interface BotTableProps {
    bots: BotStatisticsSummaryDto[]
    allBots: BotDto[]
    loading: boolean
}

function BotTable({ bots, allBots, loading }: BotTableProps) {
    return (
        <div className="dashboard-card">
            <div className="dashboard-card-header">
                <span className="dashboard-card-title">Bot Positions</span>
            </div>
            {loading ? (
                <div style={{ padding: '20px 24px', color: 'var(--color-text-secondary)', fontSize: 13 }}>
                    Loading...
                </div>
            ) : bots.length === 0 ? (
                <div className="empty-state" style={{ padding: '32px' }}>
                    <Bot />
                    <p>No bot statistics yet</p>
                </div>
            ) : (
                <>
                    <div className="dashboard-bot-header">
                        <span className="dashboard-bot-header-cell" style={{ minWidth: 80 }}>Symbol</span>
                        <span className="dashboard-bot-header-cell" style={{ minWidth: 90 }}>Status</span>
                        <span className="dashboard-bot-header-cell" style={{ flex: 1 }}></span>
                        <div className="dashboard-bot-stats" style={{ justifyContent: 'flex-end' }}>
                            <span className="dashboard-bot-header-cell" style={{ minWidth: 56, textAlign: 'right' }}>Win Rate</span>
                            <span className="dashboard-bot-header-cell" style={{ minWidth: 80, textAlign: 'right' }}>PnL</span>
                            <span className="dashboard-bot-header-cell" style={{ minWidth: 48, textAlign: 'right' }}>Trades</span>
                            <span className="dashboard-bot-header-cell" style={{ minWidth: 32 }}></span>
                        </div>
                    </div>
                    <div className="dashboard-bot-table">
                        {bots.map(bot => {
                            const wr = bot.winRate ?? 0
                            const pnl = bot.realizedPnl ?? 0
                            const botFull = allBots.find(b => b.id === bot.botId)
                            return (
                                <div key={bot.botId} className="dashboard-bot-row">
                                    <span className="dashboard-bot-symbol">{bot.symbol}</span>
                                    <BotStatusBadge status={botFull?.status} />
                                    <span className="dashboard-bot-exchange-badge">{bot.exchange}</span>
                                    <div className="dashboard-bot-stats">
                                        <span className={`dashboard-win-rate ${wr >= 0.5 ? 'dashboard-win-rate--good' : 'dashboard-win-rate--bad'}`}>
                                            {fmtPct(wr)}
                                        </span>
                                        <span className={`dashboard-bot-stat ${pnl >= 0 ? 'dashboard-bot-stat--positive' : 'dashboard-bot-stat--negative'}`}>
                                            {fmtSigned(pnl)}
                                        </span>
                                        <span className="dashboard-bot-stat">{bot.tradeCount ?? 0}</span>
                                        <Link to={`/bots/${bot.botId}`} className="dashboard-bot-view-link">
                                            View →
                                        </Link>
                                    </div>
                                </div>
                            )
                        })}
                    </div>
                </>
            )}
        </div>
    )
}

interface AssetPositionsProps {
    portfolio: GetPortfolioSnapshotResponse | null
    loading: boolean
}

function AssetPositions({ portfolio, loading }: AssetPositionsProps) {
    const exchanges = portfolio?.byExchange ?? []
    const [collapsed, setCollapsed] = useState<Record<string, boolean>>({})

    function toggle(name: string) {
        setCollapsed(prev => ({ ...prev, [name]: !prev[name] }))
    }

    if (loading) {
        return (
            <div className="dashboard-assets-section">
                <div className="dashboard-card-header">
                    <span className="dashboard-card-title">Asset Positions</span>
                </div>
                <div style={{ padding: '20px 24px', color: 'var(--color-text-secondary)', fontSize: 13 }}>
                    Loading...
                </div>
            </div>
        )
    }

    if (exchanges.length === 0) return null

    return (
        <div className="dashboard-assets-section">
            <div className="dashboard-card-header">
                <span className="dashboard-card-title">Asset Positions</span>
            </div>
            {exchanges.map((ex: ExchangePortfolioDto) => {
                const name = ex.exchange ?? '?'
                const isOpen = collapsed[name] !== true
                const assets = ex.assets ?? []

                return (
                    <div key={name} className="dashboard-asset-group">
                        <div className="dashboard-asset-group-header" onClick={() => toggle(name)}>
                            <span className="dashboard-asset-group-name">{name}</span>
                            <span className="dashboard-asset-group-total">{fmtUsd(ex.equityUsd)}</span>
                            <span className="dashboard-alloc-badge">{fmtAlloc(ex.allocationPercent)}</span>
                            <ChevronDown
                                size={14}
                                className={`dashboard-asset-chevron ${isOpen ? 'dashboard-asset-chevron--open' : ''}`}
                            />
                        </div>
                        {isOpen && assets.length > 0 && (
                            <>
                                <div className="dashboard-asset-header">
                                    <span className="dashboard-asset-header-cell">Asset</span>
                                    <span className="dashboard-asset-header-cell">Free</span>
                                    <span className="dashboard-asset-header-cell">Locked</span>
                                    <span className="dashboard-asset-header-cell">Total</span>
                                    <span className="dashboard-asset-header-cell">USD Value</span>
                                </div>
                                {assets.map(a => (
                                    <div key={a.asset} className="dashboard-asset-row">
                                        <span className="dashboard-asset-name">{a.asset}</span>
                                        <span className="dashboard-asset-amount">{(a.free ?? 0).toFixed(8).replace(/\.?0+$/, '') || '0'}</span>
                                        <span className="dashboard-asset-amount">{(a.locked ?? 0).toFixed(8).replace(/\.?0+$/, '') || '0'}</span>
                                        <span className="dashboard-asset-amount">{(a.total ?? 0).toFixed(8).replace(/\.?0+$/, '') || '0'}</span>
                                        <span className="dashboard-asset-usd">{fmtUsd(a.usdValue)}</span>
                                    </div>
                                ))}
                            </>
                        )}
                        {isOpen && assets.length === 0 && (
                            <div style={{ padding: '12px 24px 12px 48px', fontSize: 12, color: 'var(--color-text-tertiary)' }}>
                                No assets
                            </div>
                        )}
                    </div>
                )
            })}
        </div>
    )
}

// ── Dashboard page ────────────────────────────────────────────

export function Dashboard() {
    const apiProvider = useApiProvider()

    const [accountStats, setAccountStats] = useState<GetAccountStatisticsResponse | null>(null)
    const [portfolio, setPortfolio] = useState<GetPortfolioSnapshotResponse | null>(null)
    const [allBots, setAllBots] = useState<BotDto[]>([])
    const [accountLoading, setAccountLoading] = useState(true)
    const [portfolioLoading, setPortfolioLoading] = useState(true)
    const [error, setError] = useState<string | null>(null)

    useEffect(() => {
        async function loadAll() {
            setAccountLoading(true)
            setPortfolioLoading(true)
            setError(null)

            const [acct, port, botsRes] = await Promise.allSettled([
                apiProvider.getStatisticsApi().apiStatisticsAccountGet(),
                apiProvider.getStatisticsApi().apiStatisticsPortfolioGet(),
                apiProvider.getBotsApi().apiBotsGet(),
            ])

            if (acct.status === 'fulfilled') {
                setAccountStats(
                    (acct.value.data as unknown as { data: GetAccountStatisticsResponse }).data ?? null
                )
            }
            setAccountLoading(false)

            if (port.status === 'fulfilled') {
                setPortfolio(
                    (port.value.data as unknown as { data: GetPortfolioSnapshotResponse }).data ?? null
                )
            }
            setPortfolioLoading(false)

            if (botsRes.status === 'fulfilled') {
                setAllBots(
                    (botsRes.value.data as unknown as { data: BotDto[] }).data ?? []
                )
            }

            if (acct.status === 'rejected' && port.status === 'rejected') {
                setError('Failed to load dashboard data.')
            }
        }
        loadAll()
    }, [])

    // Derived values
    const botManagedTotal = (accountStats?.bots ?? []).reduce(
        (sum, b) => sum + (b.currentEquityUsd ?? 0), 0
    )
    const freeCapital = (portfolio?.totalEquityUsd ?? 0) - botManagedTotal
    const totalActiveBots = allBots.filter(b => b.status === 'Active').length
    const totalBots = allBots.length
    const pnl = accountStats?.totalRealizedPnl ?? 0

    if (error) {
        return (
            <main className="dashboard-page">
                <div className="dashboard-error">
                    <p>{error}</p>
                    <button className="btn btn-secondary btn-sm" onClick={() => window.location.reload()}>
                        Retry
                    </button>
                </div>
            </main>
        )
    }

    return (
        <main className="dashboard-page">
            <div className="page-header">
                <div className="page-header-text">
                    <h1>Dashboard</h1>
                    <p>Portfolio overview and bot performance</p>
                </div>
            </div>

            {/* Summary cards */}
            <div className="dashboard-summary-grid">
                <SummaryCard
                    icon={<Wallet size={16} color="#10B981" />}
                    iconBg="rgba(16, 185, 129, 0.12)"
                    label="Total Balance"
                    value={fmtUsd(portfolio?.totalEquityUsd)}
                    subtext="Across all exchange wallets"
                    loading={portfolioLoading}
                />
                <SummaryCard
                    icon={<Bot size={16} color="#3B82F6" />}
                    iconBg="rgba(59, 130, 246, 0.12)"
                    label="In Bots"
                    value={fmtUsd(botManagedTotal)}
                    subtext={`Free: ${fmtUsd(freeCapital < 0 ? 0 : freeCapital)}`}
                    loading={accountLoading || portfolioLoading}
                />
                <SummaryCard
                    icon={pnl >= 0
                        ? <TrendingUp size={16} color="#10B981" />
                        : <TrendingDown size={16} color="#EF4444" />
                    }
                    iconBg={pnl >= 0 ? 'rgba(16, 185, 129, 0.12)' : 'rgba(239, 68, 68, 0.12)'}
                    label="Realized PnL"
                    value={fmtSigned(accountStats?.totalRealizedPnl)}
                    valueClass={accountStats != null
                        ? pnl >= 0 ? 'dashboard-metric-value--positive' : 'dashboard-metric-value--negative'
                        : ''
                    }
                    subtext="All time"
                    loading={accountLoading}
                />
                <SummaryCard
                    icon={<Activity size={16} color={totalActiveBots > 0 ? '#10B981' : '#F59E0B'} />}
                    iconBg={totalActiveBots > 0 ? 'rgba(16, 185, 129, 0.12)' : 'rgba(245, 158, 11, 0.12)'}
                    label="Active Bots"
                    value={accountLoading ? '—' : String(totalActiveBots)}
                    subtext={`${totalBots} total configured`}
                    loading={accountLoading}
                />
            </div>

            {/* Middle row */}
            <div className="dashboard-middle-row">
                <ExchangeBreakdown
                    portfolio={portfolio}
                    accountStats={accountStats}
                    loading={portfolioLoading}
                />
                <BotTable
                    bots={accountStats?.bots ?? []}
                    allBots={allBots}
                    loading={accountLoading}
                />
            </div>

            {/* Asset positions */}
            <AssetPositions portfolio={portfolio} loading={portfolioLoading} />
        </main>
    )
}
