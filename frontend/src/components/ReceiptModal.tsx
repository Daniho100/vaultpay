import { X, CheckCircle2, ShieldCheck, Download, Share2, Copy } from 'lucide-react';
import type { Transaction } from '../types';

interface ReceiptModalProps {
  transaction: Transaction | null;
  onClose: () => void;
  showToast: (msg: string) => void;
}

export const ReceiptModal = ({
  transaction,
  onClose,
  showToast
}: ReceiptModalProps) => {
  if (!transaction) return null;

  const handleCopyRef = () => {
    navigator.clipboard.writeText(transaction.reference);
    showToast(`Reference ${transaction.reference} copied!`);
  };

  const handleShare = () => {
    if (navigator.share) {
      navigator.share({
        title: 'VaultPay Transaction Receipt',
        text: `Transaction ${transaction.reference} for ₦${transaction.amount.toLocaleString()} was successful.`
      }).catch(() => {});
    } else {
      handleCopyRef();
    }
  };

  const formatAmount = (amount: number, currency: string) => {
    const symbol = currency === 'USD' ? '$' : '₦';
    return `${symbol}${amount.toLocaleString('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 })}`;
  };

  const formatDate = (iso: string) => {
    const d = new Date(iso);
    return d.toLocaleString('en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit',
      second: '2-digit'
    });
  };

  return (
    <div className="modal-overlay" onClick={onClose}>
      <div 
        className="modal-content" 
        onClick={(e) => e.stopPropagation()}
        style={{ maxWidth: '460px' }}
      >
        <div className="modal-header-row">
          <div style={{ display: 'flex', alignItems: 'center', gap: '6px' }}>
            <ShieldCheck size={18} color="var(--accent-emerald-light)" />
            <span style={{ fontSize: '0.85rem', fontWeight: 700, color: 'var(--accent-emerald-light)', letterSpacing: '0.05em' }}>
              OFFICIAL VAULTPAY RECEIPT
            </span>
          </div>
          <button className="modal-close-btn" onClick={onClose}>
            <X size={20} />
          </button>
        </div>

        {/* Big Amount and Success Icon */}
        <div style={{ textAlign: 'center', margin: '0.5rem 0' }}>
          <div style={{
            width: '56px',
            height: '56px',
            borderRadius: '50%',
            background: 'rgba(16, 185, 129, 0.15)',
            color: 'var(--accent-emerald-light)',
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'center',
            margin: '0 auto 1rem auto'
          }}>
            <CheckCircle2 size={32} />
          </div>

          <div style={{ fontSize: '0.85rem', color: 'var(--text-muted)' }}>
            Transaction Successful
          </div>
          <div style={{
            fontSize: '2.2rem',
            fontWeight: 800,
            color: '#fff',
            letterSpacing: '-0.03em',
            margin: '0.25rem 0'
          }}>
            {formatAmount(transaction.amount, transaction.currencyCode)}
          </div>
          <div style={{ fontSize: '0.85rem', color: 'var(--text-secondary)' }}>
            {transaction.description}
          </div>
        </div>

        {/* Details Breakdown */}
        <div style={{
          background: 'var(--bg-secondary)',
          borderRadius: 'var(--radius-md)',
          padding: '1.25rem',
          display: 'flex',
          flexDirection: 'column',
          gap: '0.9rem',
          fontSize: '0.85rem',
          border: '1px solid var(--border-subtle)'
        }}>
          <div style={{ display: 'flex', justifyContent: 'space-between' }}>
            <span style={{ color: 'var(--text-muted)' }}>Recipient / Beneficiary</span>
            <strong style={{ color: '#fff' }}>{transaction.toAccountName || 'External Account'}</strong>
          </div>

          {transaction.toAccountNumber && (
            <div style={{ display: 'flex', justifyContent: 'space-between' }}>
              <span style={{ color: 'var(--text-muted)' }}>Recipient Account (NUBAN)</span>
              <span style={{ fontFamily: 'var(--font-mono)', color: 'var(--text-secondary)' }}>
                {transaction.toAccountNumber}
              </span>
            </div>
          )}

          <div style={{ display: 'flex', justifyContent: 'space-between' }}>
            <span style={{ color: 'var(--text-muted)' }}>Sender Account</span>
            <span style={{ color: '#fff' }}>{transaction.fromAccountName || 'Direct Deposit'}</span>
          </div>

          <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
            <span style={{ color: 'var(--text-muted)' }}>Transaction Reference</span>
            <div style={{ display: 'flex', alignItems: 'center', gap: '6px' }}>
              <span style={{ fontFamily: 'var(--font-mono)', fontSize: '0.8rem', color: 'var(--text-secondary)' }}>
                {transaction.reference}
              </span>
              <button 
                onClick={handleCopyRef} 
                style={{ color: 'var(--text-muted)', padding: '2px' }}
                title="Copy reference"
              >
                <Copy size={13} />
              </button>
            </div>
          </div>

          <div style={{ display: 'flex', justifyContent: 'space-between' }}>
            <span style={{ color: 'var(--text-muted)' }}>Payment Date</span>
            <span style={{ color: 'var(--text-secondary)' }}>{formatDate(transaction.createdAt)}</span>
          </div>

          <div style={{ display: 'flex', justifyContent: 'space-between' }}>
            <span style={{ color: 'var(--text-muted)' }}>Ledger Status</span>
            <span style={{ color: 'var(--accent-emerald-light)', fontWeight: 600 }}>
              ✓ Reconciled & Audited
            </span>
          </div>
        </div>

        {/* Action Buttons */}
        <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '0.75rem' }}>
          <button 
            className="filter-pill"
            style={{ display: 'flex', alignItems: 'center', justifyContent: 'center', gap: '6px', padding: '12px' }}
            onClick={() => showToast('Receipt image downloaded to your files!')}
          >
            <Download size={16} />
            <span>Download</span>
          </button>

          <button 
            className="filter-pill"
            style={{ display: 'flex', alignItems: 'center', justifyContent: 'center', gap: '6px', padding: '12px' }}
            onClick={handleShare}
          >
            <Share2 size={16} />
            <span>Share Receipt</span>
          </button>
        </div>
      </div>
    </div>
  );
};
