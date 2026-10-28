import { useState, useEffect } from 'react';
import { X, Send, CheckCircle2, AlertCircle, ShieldCheck } from 'lucide-react';
import type { Account } from '../types';

interface TransferModalProps {
  isOpen: boolean;
  onClose: () => void;
  accounts: Account[];
  selectedSourceAccount?: Account;
  onExecuteTransfer: (
    sourceAccountId: string,
    recipientNuban: string,
    recipientName: string,
    amount: number,
    note: string
  ) => void;
}

export const TransferModal = ({
  isOpen,
  onClose,
  accounts,
  selectedSourceAccount,
  onExecuteTransfer
}: TransferModalProps) => {
  const [sourceId, setSourceId] = useState(selectedSourceAccount?.id || accounts[0]?.id || '');
  const [recipientNuban, setRecipientNuban] = useState('');
  const [recipientName, setRecipientName] = useState<string | null>(null);
  const [isVerifying, setIsVerifying] = useState(false);
  const [amount, setAmount] = useState('');
  const [note, setNote] = useState('');
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (selectedSourceAccount) {
      setSourceId(selectedSourceAccount.id);
    } else if (accounts.length > 0 && !sourceId) {
      setSourceId(accounts[0].id);
    }
  }, [selectedSourceAccount, accounts]);

  // Recipient verification mock
  useEffect(() => {
    const cleaned = recipientNuban.trim();
    if (cleaned.length === 10) {
      setIsVerifying(true);
      setError(null);
      const timer = setTimeout(() => {
        // Known account matches
        const internal = accounts.find(a => a.accountNumber === cleaned);
        if (internal) {
          setRecipientName(`${internal.accountName} (Self)`);
        } else if (cleaned === '1038291047') {
          setRecipientName('Chinonso Okeke');
        } else if (cleaned === '1047291844') {
          setRecipientName('Adewale Adeleke');
        } else {
          setRecipientName('Babatunde Fashola (Verified VaultPay Account)');
        }
        setIsVerifying(false);
      }, 400);
      return () => clearTimeout(timer);
    } else {
      setRecipientName(null);
      setIsVerifying(false);
    }
  }, [recipientNuban, accounts]);

  if (!isOpen) return null;

  const currentSource = accounts.find(a => a.id === sourceId) || accounts[0];
  const numAmount = parseFloat(amount) || 0;

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);

    if (!recipientNuban || recipientNuban.trim().length !== 10) {
      setError('Please enter a valid 10-digit NUBAN account number.');
      return;
    }

    if (numAmount <= 0) {
      setError('Please enter a transfer amount greater than 0.');
      return;
    }

    if (currentSource && numAmount > currentSource.balance) {
      setError(`Insufficient balance. Current balance is ₦${currentSource.balance.toLocaleString()}.`);
      return;
    }

    onExecuteTransfer(
      sourceId,
      recipientNuban.trim(),
      recipientName || 'External Recipient',
      numAmount,
      note.trim() || 'VaultPay Transfer'
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
              background: 'rgba(16, 185, 129, 0.15)',
              color: 'var(--accent-emerald-light)',
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'center'
            }}>
              <Send size={18} />
            </div>
            <div>
              <h2 className="modal-title" style={{ fontSize: '1.25rem' }}>Send Money</h2>
              <p style={{ fontSize: '0.8rem', color: 'var(--text-muted)' }}>
                Instant, zero-fee transfer within VaultPay
              </p>
            </div>
          </div>
          <button className="modal-close-btn" onClick={onClose}>
            <X size={20} />
          </button>
        </div>

        {error && (
          <div style={{
            display: 'flex',
            alignItems: 'center',
            gap: '8px',
            padding: '10px 14px',
            borderRadius: '8px',
            background: 'rgba(244, 63, 94, 0.15)',
            border: '1px solid rgba(244, 63, 94, 0.3)',
            color: 'var(--accent-rose)',
            fontSize: '0.85rem'
          }}>
            <AlertCircle size={16} />
            <span>{error}</span>
          </div>
        )}

        <form onSubmit={handleSubmit} style={{ display: 'flex', flexDirection: 'column', gap: '1.25rem' }}>
          {/* Source Account */}
          <div className="form-group">
            <label className="form-label">Debit From Account</label>
            <select
              className="form-select"
              value={sourceId}
              onChange={(e) => setSourceId(e.target.value)}
            >
              {accounts.map(acc => (
                <option key={acc.id} value={acc.id}>
                  {acc.accountName} — {acc.currencyCode} {acc.balance.toLocaleString()} ({acc.accountNumber})
                </option>
              ))}
            </select>
          </div>

          {/* Recipient NUBAN */}
          <div className="form-group">
            <label className="form-label">Recipient 10-digit NUBAN Number</label>
            <input
              type="text"
              maxLength={10}
              placeholder="e.g. 1084920194 or 1038291047"
              className="form-input"
              value={recipientNuban}
              onChange={(e) => setRecipientNuban(e.target.value.replace(/\D/g, ''))}
              style={{ fontFamily: 'var(--font-mono)' }}
            />
            {isVerifying && (
              <span style={{ fontSize: '0.75rem', color: 'var(--text-muted)' }}>
                Verifying account holder...
              </span>
            )}
            {recipientName && (
              <div style={{
                display: 'flex',
                alignItems: 'center',
                gap: '6px',
                fontSize: '0.8rem',
                color: 'var(--accent-emerald-light)',
                background: 'rgba(16, 185, 129, 0.1)',
                padding: '6px 10px',
                borderRadius: '6px',
                border: '1px solid rgba(16, 185, 129, 0.2)'
              }}>
                <CheckCircle2 size={14} />
                <span>Verified: <strong>{recipientName}</strong></span>
              </div>
            )}
          </div>

          {/* Amount */}
          <div className="form-group">
            <label className="form-label">Amount (NGN)</label>
            <div style={{ position: 'relative' }}>
              <span style={{
                position: 'absolute',
                left: '12px',
                top: '50%',
                transform: 'translateY(-50%)',
                color: 'var(--text-muted)',
                fontWeight: 700
              }}>
                ₦
              </span>
              <input
                type="number"
                min="100"
                step="any"
                placeholder="0.00"
                className="form-input"
                style={{ paddingLeft: '28px', fontSize: '1.1rem', fontWeight: 700 }}
                value={amount}
                onChange={(e) => setAmount(e.target.value)}
              />
            </div>
            {currentSource && (
              <span style={{ fontSize: '0.75rem', color: 'var(--text-muted)' }}>
                Available balance: ₦{currentSource.balance.toLocaleString('en-NG', { minimumFractionDigits: 2 })}
              </span>
            )}
          </div>

          {/* Note / Narration */}
          <div className="form-group">
            <label className="form-label">Payment Description / Memo (Optional)</label>
            <input
              type="text"
              placeholder="e.g. Project milestone, groceries, dinner"
              className="form-input"
              value={note}
              onChange={(e) => setNote(e.target.value)}
            />
          </div>

          {/* Fee & Security assurance */}
          <div style={{
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'space-between',
            padding: '10px 14px',
            borderRadius: '8px',
            background: 'var(--bg-secondary)',
            border: '1px solid var(--border-subtle)',
            fontSize: '0.8rem'
          }}>
            <div style={{ display: 'flex', alignItems: 'center', gap: '6px', color: 'var(--text-secondary)' }}>
              <ShieldCheck size={16} color="var(--accent-emerald-light)" />
              <span>VaultPay Double-Entry Ledger Protection</span>
            </div>
            <span style={{ color: 'var(--accent-emerald-light)', fontWeight: 700 }}>Fee: ₦0.00</span>
          </div>

          <button type="submit" className="btn-primary">
            <Send size={16} />
            <span>Confirm & Send ₦{numAmount > 0 ? numAmount.toLocaleString() : '0.00'}</span>
          </button>
        </form>
      </div>
    </div>
  );
};
