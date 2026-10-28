import { useState } from 'react';
import { 
  ResponsiveContainer, 
  AreaChart, 
  Area, 
  XAxis, 
  YAxis, 
  Tooltip, 
  CartesianGrid 
} from 'recharts';
import type { CashFlowPoint } from '../types';

interface CashFlowChartProps {
  data: CashFlowPoint[];
}

export const CashFlowChart: React.FC<CashFlowChartProps> = ({ data }) => {
  const [timeframe, setTimeframe] = useState<'7D' | '30D' | '90D'>('30D');

  const formatCurrencyAxis = (value: number) => {
    if (value >= 1000000) return `₦${(value / 1000000).toFixed(1)}M`;
    if (value >= 1000) return `₦${(value / 1000).toFixed(0)}k`;
    return `₦${value}`;
  };

  const CustomTooltip = ({ active, payload, label }: any) => {
    if (active && payload && payload.length) {
      return (
        <div style={{
          background: 'rgba(19, 24, 37, 0.95)',
          border: '1px solid var(--border-medium)',
          borderRadius: '8px',
          padding: '10px 14px',
          boxShadow: 'var(--shadow-md)',
          backdropFilter: 'blur(10px)',
          fontSize: '0.85rem'
        }}>
          <div style={{ fontWeight: 700, color: '#fff', marginBottom: '6px' }}>{label}</div>
          <div style={{ color: 'var(--accent-emerald-light)', marginBottom: '3px', display: 'flex', gap: '8px' }}>
            <span>Inflow:</span>
            <strong>₦{payload[0].value.toLocaleString()}</strong>
          </div>
          <div style={{ color: 'var(--accent-rose)', display: 'flex', gap: '8px' }}>
            <span>Outflow:</span>
            <strong>₦{payload[1].value.toLocaleString()}</strong>
          </div>
        </div>
      );
    }
    return null;
  };

  return (
    <div className="glass-panel chart-card">
      <div className="chart-header">
        <div className="chart-title-group">
          <h3>Cash Flow Activity</h3>
          <p>Real-time tracking of money entering and leaving your accounts</p>
        </div>

        <div className="chart-timeframe-selector">
          {(['7D', '30D', '90D'] as const).map(tf => (
            <button
              key={tf}
              className={`timeframe-pill-btn ${timeframe === tf ? 'active' : ''}`}
              onClick={() => setTimeframe(tf)}
            >
              {tf}
            </button>
          ))}
        </div>
      </div>

      <div style={{ width: '100%', height: 280 }}>
        <ResponsiveContainer width="100%" height="100%">
          <AreaChart data={data} margin={{ top: 10, right: 10, left: 0, bottom: 0 }}>
            <defs>
              <linearGradient id="inflowGrad" x1="0" y1="0" x2="0" y2="1">
                <stop offset="5%" stopColor="#10b981" stopOpacity={0.4} />
                <stop offset="95%" stopColor="#10b981" stopOpacity={0.0} />
              </linearGradient>
              <linearGradient id="outflowGrad" x1="0" y1="0" x2="0" y2="1">
                <stop offset="5%" stopColor="#f43f5e" stopOpacity={0.3} />
                <stop offset="95%" stopColor="#f43f5e" stopOpacity={0.0} />
              </linearGradient>
            </defs>
            <CartesianGrid strokeDasharray="3 3" stroke="rgba(255,255,255,0.05)" vertical={false} />
            <XAxis 
              dataKey="date" 
              stroke="var(--text-muted)" 
              fontSize={12} 
              tickLine={false} 
              axisLine={{ stroke: 'rgba(255,255,255,0.08)' }} 
            />
            <YAxis 
              stroke="var(--text-muted)" 
              fontSize={12} 
              tickLine={false} 
              axisLine={false}
              tickFormatter={formatCurrencyAxis}
            />
            <Tooltip content={<CustomTooltip />} />
            <Area 
              type="monotone" 
              dataKey="inflow" 
              stroke="#10b981" 
              strokeWidth={2.5} 
              fillOpacity={1} 
              fill="url(#inflowGrad)" 
              name="Inflow"
            />
            <Area 
              type="monotone" 
              dataKey="outflow" 
              stroke="#f43f5e" 
              strokeWidth={2} 
              fillOpacity={1} 
              fill="url(#outflowGrad)" 
              name="Outflow"
            />
          </AreaChart>
        </ResponsiveContainer>
      </div>

      <div style={{ 
        display: 'flex', 
        alignItems: 'center', 
        justifyContent: 'center', 
        gap: '2rem', 
        marginTop: '1rem',
        fontSize: '0.8rem',
        color: 'var(--text-secondary)'
      }}>
        <div style={{ display: 'flex', alignItems: 'center', gap: '6px' }}>
          <span style={{ width: '10px', height: '10px', borderRadius: '50%', background: '#10b981' }} />
          <span>Inflow (Deposits & Credits)</span>
        </div>
        <div style={{ display: 'flex', alignItems: 'center', gap: '6px' }}>
          <span style={{ width: '10px', height: '10px', borderRadius: '50%', background: '#f43f5e' }} />
          <span>Outflow (Transfers & Bills)</span>
        </div>
      </div>
    </div>
  );
};
