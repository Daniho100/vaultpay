import { useState } from 'react';
import { 
  Wallet, 
  PiggyBank, 
  Globe, 
  Copy, 
  Check, 
  Plus, 
  ArrowRight,
  TrendingUp
} from 'lucide-react';
import type { Account } from '../types';

interface AccountsSectionProps {
  accounts: Account[];
  onOpenNewAccount: () => void;
  onSelectAccountTransfer: (account: Account) => void;
  showToast: (msg: string) => void;
}

export const AccountsSection: React.FC<AccountsSectionProps> = ({
  accounts,
  onOpenNewAccount,
  onSelectAccountTransfer,
  showToast
}) => {
  const [copiedId, setCopiedId] = useState<string | null>(null);

  const handleCopy = (accountNumber: string, id: string) => {
    navigator.clipboard.writeText(accountNumber);
    setCopiedId(id);
    showToast(`Account number ${accountNumber} copied to clipboard!`);
    setTimeout(() => setCopiedId(null), 2500);
  };

  const getAccountIcon = (type?: string) => {
    switch (type) {
      case 'vault_savings':
        return <PiggyBank size={18} />;
      case 'global_usd':
        return <Globe size={18} />;
      default:
        return <Wallet size={18} />;
    }
  };

  const formatBalance = (balance: number, currency: string) => {
    if (currency === 'USD') {
      return `$${balance.toLocaleString('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 })}`;
    }
    return `₦${balance.toLocaleString('en-NG', { minimumFractionDigits: 2, maximumFractionDigits: 2 })}`;
  };

  return (
    <section>
      <div className="section-header-row">
        <div className="section-heading-group">
          <h2 className="section-heading">My Vault Accounts</h2>
          <span className="count-pill">{accounts.length} Active</span>
        </div>
        <button 
          onClick={onOpenNewAccount}
          style={{
            display: 'flex',
            alignItems: 'center',
            gap: '6px',
            fontSize: '0.85rem',
            fontWeight: 600,
            color: 'var(--accent-emerald-light)',
            padding: '6px 12px',
            borderRadius: 'var(--radius-sm)',
            background: 'var(--accent-emerald-glow)',
            border: '1px solid rgba(16, 185, 129, 0.3)'
          }}
        >
          <Plus size={16} />
          <span>Add Account</span>
        </button>
      </div>

      <div className="accounts-grid">
        {accounts.map(acc => (
          <div key={acc.id} className="account-card glass-panel-hover">
            <div>
              <div className="account-card-header">
                <div className="account-badge-title">
                  <div className="account-type-icon">
                    {getAccountIcon(acc.accountType)}
                  </div>
                  <span className="account-name-text">{acc.accountName}</span>
                </div>
                <span className="currency-badge">{acc.currencyCode}</span>
              </div>

              {acc.interestRate && (
                <div style={{
                  display: 'inline-flex',
                  alignItems: 'center',
                  gap: '4px',
                  fontSize: '0.75rem',
                  fontWeight: 600,
                  color: 'var(--accent-emerald-light)',
                  background: 'rgba(16, 185, 129, 0.1)',
                  padding: '2px 8px',
                  borderRadius: '999px',
                  marginTop: '0.75rem'
                }}>
                  <TrendingUp size={12} />
                  <span>{acc.interestRate}% APY Compounded Daily</span>
                </div>
              )}

              <div className="account-balance-large" style={{ marginTop: '0.75rem' }}>
                {formatBalance(acc.balance, acc.currencyCode)}
              </div>
            </div>

            <div className="account-card-footer">
              <div className="nuban-copy-group">
                <span className="nuban-text">{acc.accountNumber}</span>
                <button 
                  className="copy-mini-btn"
                  onClick={() => handleCopy(acc.accountNumber, acc.id)}
                  title="Copy NUBAN account number"
                >
                  {copiedId === acc.id ? <Check size={14} color="var(--accent-emerald-light)" /> : <Copy size={14} />}
                </button>
              </div>

              <button 
                onClick={() => onSelectAccountTransfer(acc)}
                style={{
                  display: 'flex',
                  alignItems: 'center',
                  gap: '4px',
                  fontSize: '0.8rem',
                  fontWeight: 600,
                  color: 'var(--text-secondary)'
                }}
                className="hover-white"
              >
                <span>Transfer</span>
                <ArrowRight size={14} />
              </button>
            </div>
          </div>
        ))}
      </div>
    </section>
  );
};
