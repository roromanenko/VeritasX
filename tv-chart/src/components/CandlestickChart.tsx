import { AreaChart, Area, XAxis, YAxis, Tooltip, ResponsiveContainer, CartesianGrid } from 'recharts';
import { format } from 'date-fns';
import { CandleDto } from '../api';

interface CandlestickChartProps {
    candles: CandleDto[];
    symbol?: string | null | undefined;
}

interface TooltipPayload {
    value: number;
    payload: { date: string };
}

interface CustomTooltipProps {
    active?: boolean;
    payload?: TooltipPayload[];
}

function CustomTooltip({ active, payload }: CustomTooltipProps) {
    if (!active || !payload?.length) return null;
    return (
        <div style={{
            background: '#1A1A1A',
            border: '1px solid #1F2937',
            borderRadius: '8px',
            padding: '10px 12px',
            boxShadow: '0 4px 16px rgba(0,0,0,0.4)',
        }}>
            <p style={{ fontSize: '11px', color: '#6B7280', fontFamily: 'JetBrains Mono, monospace', marginBottom: '4px' }}>
                {payload[0].payload.date}
            </p>
            <p style={{ fontSize: '13px', color: '#10B981', fontFamily: 'JetBrains Mono, monospace', fontWeight: 600 }}>
                ${payload[0].value.toLocaleString('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 })}
            </p>
        </div>
    );
}

function CandlestickChart({ candles }: CandlestickChartProps) {
    const data = candles.map(c => ({
        date: c.openTime ? format(new Date(c.openTime), 'MMM d') : '',
        price: c.close ?? 0,
    }));

    const xInterval = Math.max(0, Math.ceil(data.length / 6) - 1);

    return (
        <div style={{ width: '100%', height: '350px', padding: '0 8px 8px' }}>
            <ResponsiveContainer width="100%" height={350}>
                <AreaChart data={data}>
                    <defs>
                        <linearGradient id="priceGrad" x1="0" y1="0" x2="0" y2="1">
                            <stop offset="5%" stopColor="#10B981" stopOpacity={0.3} />
                            <stop offset="95%" stopColor="#10B981" stopOpacity={0} />
                        </linearGradient>
                    </defs>
                    <CartesianGrid strokeDasharray="3 3" stroke="#1F2937" strokeOpacity={0.3} />
                    <XAxis
                        dataKey="date"
                        stroke="#6B7280"
                        tick={{ fontSize: 11, fontFamily: 'JetBrains Mono, monospace', fill: '#6B7280' }}
                        tickLine={false}
                        axisLine={{ stroke: '#1F2937' }}
                        interval={xInterval}
                    />
                    <YAxis
                        stroke="#6B7280"
                        tick={{ fontSize: 11, fontFamily: 'JetBrains Mono, monospace', fill: '#6B7280' }}
                        tickLine={false}
                        axisLine={{ stroke: '#1F2937' }}
                        tickFormatter={(v: number) => `$${(v / 1000).toFixed(0)}k`}
                        width={48}
                    />
                    <Tooltip content={<CustomTooltip />} />
                    <Area
                        type="monotone"
                        dataKey="price"
                        stroke="#10B981"
                        strokeWidth={2}
                        fill="url(#priceGrad)"
                        dot={false}
                        activeDot={{ r: 4, fill: '#10B981', stroke: '#0A0A0A', strokeWidth: 2 }}
                        isAnimationActive={false}
                    />
                </AreaChart>
            </ResponsiveContainer>
        </div>
    );
}

export default CandlestickChart;
