import { useState } from 'react';
import { X, PlusCircle, Copy, Check, Building2, Sparkles } from 'lucide-react';
import type { Account } from '../types';

interface DepositModalProps {
  isOpen: boolean;
  onClose: () => void;
  accounts: Account[];
  onExecuteDeposit: (accountId: string, amount: number, memo: string) => void;
  showToast: (msg: string) => void;
}

export const DepositModal = ({
  isOpen,
  onClose,
  accounts,
  onExecuteDeposit,
  showToast
}: DepositModalProps) => {
  const [selectedAccountId, setSelectedAccountId] = useState(accounts[0]?.id || '');
  const [customAmount, setCustomAmount] = useState('');
  const [copiedField, setCopiedField] = useState<string | null>(null);

  if (!isOpen) return null;

  const currentAccount = accounts.find(a => a.id === selectedAccountId) || accounts[0];

  const handleCopy = (text: string, label: string) => {
    navigator.clipboard.writeText(text);
    setCopiedField(label);
    showToast(`${label} copied to clipboard!`);
    setTimeout(() => setCopiedField(null), 2500);
  };

  const handleSimulatedDeposit = (amount: number) => {
    onExecuteDeposit(
      currentAccount.id,
      amount,
      `Instant Top-up via Providus Bank Transfer`
    );
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
              background: 'rgba(99, 102, 241, 0.2)',
              color: 'var(--accent-indigo-light)',
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'center'
            }}>
              <PlusCircle size={18} />
            </div>
            <div>
              <h2 className="modal-title" style={{ fontSize: '1.25rem' }}>Add Funds / Top-Up</h2>
              <p style={{ fontSize: '0.8rem', color: 'var(--text-muted)' }}>
                Deposit funds directly into your VaultPay account
              </p>
            </div>
          </div>
          <button className="modal-close-btn" onClick={onClose}>
            <X size={20} />
          </button>
        </div>

        {/* Account Selector */}
        <div className="form-group">
          <label className="form-label">Destination Account</label>
          <select
            className="form-select"
            value={selectedAccountId}
            onChange={(e) => setSelectedAccountId(e.target.value)}
          >
            {accounts.map(acc => (
              <option key={acc.id} value={acc.id}>
                {acc.accountName} — {acc.currencyCode} {acc.balance.toLocaleString()}
              </option>
            ))}
          </select>
        </div>

        {/* Bank Transfer Details Box */}
        <div style={{
          background: 'var(--bg-secondary)',
          border: '1px solid var(--border-medium)',
          borderRadius: 'var(--radius-md)',
          padding: '1.25rem',
          display: 'flex',
          flexDirection: 'column',
          gap: '1rem'
        }}>
          <div style={{ display: 'flex', alignItems: 'center', gap: '6px', fontSize: '0.8rem', color: 'var(--text-secondary)' }}>
            <Building2 size={16} color="var(--accent-emerald-light)" />
            <span>Transfer from any Nigerian Bank app or USSD:</span>
          </div>

          <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
            <div>
              <div style={{ fontSize: '0.75rem', color: 'var(--text-muted)' }}>Bank Name</div>
              <div style={{ fontSize: '0.95rem', fontWeight: 700, color: '#fff' }}>Providus Bank / VaultPay MFB</div>
            </div>
          </div>

          <div style={{ 
            display: 'flex', 
            justifyContent: 'space-between', 
            alignItems: 'center',
            padding: '8px 12px',
            background: 'var(--bg-elevated)',
            borderRadius: '6px'
          }}>
            <div>
              <div style={{ fontSize: '0.7rem', color: 'var(--text-muted)' }}>Dedicated Virtual Account Number</div>
              <div style={{ 
                fontFamily: 'var(--font-mono)', 
                fontSize: '1.2rem', 
                fontWeight: 800, 
                color: 'var(--accent-emerald-light)',
                letterSpacing: '0.08em'
              }}>
                {currentAccount?.accountNumber || '1084920194'}
              </div>
            </div>
            <button
              className="copy-mini-btn"
              onClick={() => handleCopy(currentAccount?.accountNumber || '1084920194', 'Account Number')}
              style={{ padding: '6px' }}
            >
              {copiedField === 'Account Number' ? <Check size={16} color="var(--accent-emerald-light)" /> : <Copy size={16} />}
            </button>
          </div>

          <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
            <div>
              <div style={{ fontSize: '0.75rem', color: 'var(--text-muted)' }}>Beneficiary Name</div>
              <div style={{ fontSize: '0.9rem', fontWeight: 600, color: '#fff' }}>Alex Morgan — VaultPay</div>
            </div>
          </div>
        </div>

        {/* Quick Simulator / Instant Credit for Testing */}
        <div style={{
          display: 'flex',
          flexDirection: 'column',
          gap: '0.75rem',
          paddingTop: '0.5rem'
        }}>
          <div style={{ display: 'flex', alignItems: 'center', gap: '6px', fontSize: '0.8rem', fontWeight: 600, color: 'var(--accent-indigo-light)' }}>
            <Sparkles size={15} />
            <span>Instant Sandbox Deposit Simulation:</span>
          </div>

          <div style={{ display: 'grid', gridTemplateColumns: 'repeat(3, 1fr)', gap: '0.5rem' }}>
            <button
              className="filter-pill"
              style={{ textAlign: 'center', padding: '10px 0' }}
              onClick={() => handleSimulatedDeposit(50000)}
            >
              + ₦50,000
            </button>
            <button
              className="filter-pill"
              style={{ textAlign: 'center', padding: '10px 0' }}
              onClick={() => handleSimulatedDeposit(200000)}
            >
              + ₦200,000
            </button>
            <button
              className="filter-pill"
              style={{ textAlign: 'center', padding: '10px 0' }}
              onClick={() => handleSimulatedDeposit(1000000)}
            >
              + ₦1,000,000
            </button>
          </div>

          <div style={{ display: 'flex', gap: '0.5rem', marginTop: '0.5rem' }}>
            <input
              type="number"
              placeholder="Custom amount (e.g. 75000)"
              className="form-input"
              value={customAmount}
              onChange={(e) => setCustomAmount(e.target.value)}
            />
            <button
              className="btn-primary"
              style={{ width: 'auto', padding: '0 20px', whiteSpace: 'nowrap' }}
              onClick={() => {
                const val = parseFloat(customAmount);
                if (val > 0) {
                  handleSimulatedDeposit(val);
                }
              }}
            >
              Deposit
            </button>
          </div>
        </div>
      </div>
    </div>
  );
};
