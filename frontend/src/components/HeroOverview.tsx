import { useState } from 'react';
import { 
  Eye, 
  EyeOff, 
  ArrowUpRight, 
  ArrowDownLeft, 
  Send, 
  PlusCircle, 
  FolderPlus, 
  FileText,
  Lock
} from 'lucide-react';
import type { Account, Transaction } from '../types';

interface HeroOverviewProps {
  accounts: Account[];
  transactions: Transaction[];
  onOpenTransfer: () => void;
  onOpenDeposit: () => void;
  onOpenNewAccount: () => void;
  onOpenStatement: () => void;
}

export const HeroOverview = ({
  accounts,
  transactions,
  onOpenTransfer,
  onOpenDeposit,
  onOpenNewAccount,
  onOpenStatement
}: HeroOverviewProps) => {
  const [showBalance, setShowBalance] = useState(true);

  // Calculate total NGN & USD balances
  const totalNgn = accounts
    .filter(a => a.currencyCode === 'NGN')
    .reduce((sum, a) => sum + a.balance, 0);

  const totalUsd = accounts
    .filter(a => a.currencyCode === 'USD')
    .reduce((sum, a) => sum + a.balance, 0);

  // Calculate 30-day inflow and outflow
  const completedTxns = transactions.filter(t => t.status === 'Completed' && t.currencyCode === 'NGN');
  const totalInflow = completedTxns
    .filter(t => !t.isDebit)
    .reduce((sum, t) => sum + t.amount, 0);
  const totalOutflow = completedTxns
    .filter(t => t.isDebit)
    .reduce((sum, t) => sum + t.amount, 0);

  const formatNgn = (val: number) => {
    return new Intl.NumberFormat('en-NG', {
      style: 'currency',
      currency: 'NGN',
      maximumFractionDigits: 2
    }).format(val).replace('NGN', '₦');
  };

  const formatUsd = (val: number) => {
    return new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency: 'USD'
    }).format(val);
  };

  return (
    <div className="hero-overview-grid">
      {/* Net Worth Card */}
      <div className="glass-panel net-worth-card">
        <div className="card-top-row">
          <div className="metric-label-group">
            <Lock size={15} color="var(--accent-emerald-light)" />
            <span>Total Vault Net Worth</span>
          </div>
          <button 
            className="eye-toggle-btn"
            onClick={() => setShowBalance(!showBalance)}
            title={showBalance ? "Hide balance" : "Show balance"}
          >
            {showBalance ? <Eye size={18} /> : <EyeOff size={18} />}
          </button>
        </div>

        <div className="big-balance-wrapper">
          <div className="big-balance-amount">
            {showBalance ? formatNgn(totalNgn) : '₦••••••••'}
          </div>
          {totalUsd > 0 && (
            <div style={{ 
              fontSize: '1rem', 
              color: 'var(--text-secondary)',
              background: 'var(--bg-elevated)',
              padding: '4px 10px',
              borderRadius: '999px',
              border: '1px solid var(--border-subtle)'
            }}>
              + {showBalance ? formatUsd(totalUsd) : '$••••'}
            </div>
          )}
        </div>

        <div className="flow-stats-row">
          <div className="flow-item">
            <div className="flow-icon-circle inflow">
              <ArrowDownLeft size={18} />
            </div>
            <div>
              <div className="flow-details-title">Total Inflow (Credits)</div>
              <div className="flow-details-value" style={{ color: 'var(--accent-emerald-light)' }}>
                {showBalance ? `+${formatNgn(totalInflow)}` : '••••••'}
              </div>
            </div>
          </div>

          <div className="flow-item">
            <div className="flow-icon-circle outflow">
              <ArrowUpRight size={18} />
            </div>
            <div>
              <div className="flow-details-title">Total Outflow (Debits)</div>
              <div className="flow-details-value" style={{ color: 'var(--accent-rose)' }}>
                {showBalance ? `-${formatNgn(totalOutflow)}` : '••••••'}
              </div>
            </div>
          </div>
        </div>
      </div>

      {/* Quick Action Hub */}
      <div className="glass-panel quick-actions-panel">
        <div className="quick-actions-title">Instant Payment Actions</div>
        <div className="actions-buttons-grid">
          <button className="action-card-btn primary" onClick={onOpenTransfer}>
            <div className="action-icon-wrap" style={{ background: 'rgba(16, 185, 129, 0.25)', color: 'var(--accent-emerald-light)' }}>
              <Send size={19} />
            </div>
            <div>
              <div className="action-title">Send Money</div>
              <div className="action-subtitle">Instant NUBAN transfer</div>
            </div>
          </button>

          <button className="action-card-btn" onClick={onOpenDeposit}>
            <div className="action-icon-wrap" style={{ background: 'rgba(99, 102, 241, 0.2)', color: 'var(--accent-indigo-light)' }}>
              <PlusCircle size={19} />
            </div>
            <div>
              <div className="action-title">Add Funds</div>
              <div className="action-subtitle">Virtual account & cards</div>
            </div>
          </button>

          <button className="action-card-btn" onClick={onOpenNewAccount}>
            <div className="action-icon-wrap" style={{ background: 'rgba(6, 182, 212, 0.15)', color: 'var(--accent-cyan)' }}>
              <FolderPlus size={19} />
            </div>
            <div>
              <div className="action-title">New Vault</div>
              <div className="action-subtitle">Savings & USD accounts</div>
            </div>
          </button>

          <button className="action-card-btn" onClick={onOpenStatement}>
            <div className="action-icon-wrap" style={{ background: 'rgba(245, 158, 11, 0.15)', color: 'var(--accent-amber)' }}>
              <FileText size={19} />
            </div>
            <div>
              <div className="action-title">Statements</div>
              <div className="action-subtitle">Official PDF export</div>
            </div>
          </button>
        </div>
      </div>
    </div>
  );
};
