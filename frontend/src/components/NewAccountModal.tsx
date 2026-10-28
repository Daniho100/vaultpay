import { useState } from 'react';
import { X, FolderPlus, Sparkles } from 'lucide-react';
import type { Account } from '../types';

interface NewAccountModalProps {
  isOpen: boolean;
  onClose: () => void;
  onCreateAccount: (account: Account) => void;
  showToast: (msg: string) => void;
}

export const NewAccountModal = ({
  isOpen,
  onClose,
  onCreateAccount,
  showToast
}: NewAccountModalProps) => {
  const [accountName, setAccountName] = useState('');
  const [currency, setCurrency] = useState<'NGN' | 'USD' | 'EUR' | 'GBP'>('NGN');
  const [accountType, setAccountType] = useState<'checking' | 'vault_savings' | 'global_usd'>('vault_savings');

  if (!isOpen) return null;

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();

    // Generate random 10-digit NUBAN starting with 10
    const random8 = Math.floor(10000000 + Math.random() * 90000000).toString();
    const nuban = `10${random8}`;

    const newAcc: Account = {
      id: `acc-${Date.now()}`,
      accountNumber: nuban,
      accountName: accountName.trim() || (accountType === 'vault_savings' ? 'Target Wealth Vault' : 'Global Travel Vault'),
      currencyCode: currency,
      balance: 0.00,
      status: 'Active',
      createdAt: new Date().toISOString(),
      accountType: accountType,
      interestRate: accountType === 'vault_savings' ? 8.5 : undefined
    };

    onCreateAccount(newAcc);
    showToast(`New Vault account ${newAcc.accountName} created successfully!`);
    onClose();
  };

  return (
    <div className="modal-overlay" onClick={onClose}>
      <div className="modal-content" onClick={(e) => e.stopPropagation()}>
        <div className="modal-header-row">
          <div style={{ display: 'flex', alignItems: 'center', gap: '8px' }}>
            <div style={{
              width: '36px',
              height: '36px',
              borderRadius: '8px',
              background: 'rgba(6, 182, 212, 0.15)',
              color: 'var(--accent-cyan)',
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'center'
            }}>
              <FolderPlus size={18} />
            </div>
            <div>
              <h2 className="modal-title" style={{ fontSize: '1.25rem' }}>Open New Vault Account</h2>
              <p style={{ fontSize: '0.8rem', color: 'var(--text-muted)' }}>
                Create high-yield savings or international currency vaults
              </p>
            </div>
          </div>
          <button className="modal-close-btn" onClick={onClose}>
            <X size={20} />
          </button>
        </div>

        <form onSubmit={handleSubmit} style={{ display: 'flex', flexDirection: 'column', gap: '1.25rem' }}>
          <div className="form-group">
            <label className="form-label">Vault Type</label>
            <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '0.6rem' }}>
              <button
                type="button"
                className={`filter-pill ${accountType === 'vault_savings' ? 'active' : ''}`}
                style={{ textAlign: 'left', padding: '12px' }}
                onClick={() => {
                  setAccountType('vault_savings');
                  setCurrency('NGN');
                }}
              >
                <div style={{ fontWeight: 700, fontSize: '0.85rem' }}>High-Yield Vault</div>
                <div style={{ fontSize: '0.75rem', color: 'var(--accent-emerald-light)' }}>8.5% Annual APY</div>
              </button>

              <button
                type="button"
                className={`filter-pill ${accountType === 'global_usd' ? 'active' : ''}`}
                style={{ textAlign: 'left', padding: '12px' }}
                onClick={() => {
                  setAccountType('global_usd');
                  setCurrency('USD');
                }}
              >
                <div style={{ fontWeight: 700, fontSize: '0.85rem' }}>Global FX Vault</div>
                <div style={{ fontSize: '0.75rem', color: 'var(--text-muted)' }}>USD / International</div>
              </button>
            </div>
          </div>

          <div className="form-group">
            <label className="form-label">Account Label / Purpose</label>
            <input
              type="text"
              placeholder="e.g. Real Estate Downpayment, Tech Stack Subscription"
              className="form-input"
              value={accountName}
              onChange={(e) => setAccountName(e.target.value)}
            />
          </div>

          <div className="form-group">
            <label className="form-label">Currency</label>
            <select
              className="form-select"
              value={currency}
              onChange={(e) => setCurrency(e.target.value as any)}
            >
              <option value="NGN">NGN — Nigerian Naira (₦)</option>
              <option value="USD">USD — US Dollar ($)</option>
              <option value="EUR">EUR — Euro (€)</option>
              <option value="GBP">GBP — British Pound (£)</option>
            </select>
          </div>

          <div style={{
            padding: '12px',
            borderRadius: '8px',
            background: 'var(--bg-secondary)',
            border: '1px solid var(--border-subtle)',
            fontSize: '0.8rem',
            color: 'var(--text-secondary)'
          }}>
            <Sparkles size={14} color="var(--accent-emerald-light)" style={{ marginRight: '6px', verticalAlign: 'middle' }} />
            Each vault is protected with automated NUBAN issuance and integrated into your double-entry ledger.
          </div>

          <button type="submit" className="btn-primary">
            <span>Create Vault Account</span>
          </button>
        </form>
      </div>
    </div>
  );
};
