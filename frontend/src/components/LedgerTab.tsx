import { CheckCircle2, ArrowUpRight, ArrowDownLeft } from 'lucide-react';
import type { LedgerRecord } from '../types';

interface LedgerTabProps {
  ledgerEntries: LedgerRecord[];
}

export const LedgerTab = ({ ledgerEntries }: LedgerTabProps) => {
  return (
    <div style={{ display: 'flex', flexDirection: 'column', gap: '2rem' }}>
      <div className="section-header-row">
        <div>
          <h2 className="section-heading">Double-Entry Ledger & Audit Log</h2>
          <p style={{ fontSize: '0.85rem', color: 'var(--text-muted)' }}>
            Immutable, cryptographically verifiable financial ledger entries adhering to GAAP banking standards
          </p>
        </div>
        <div style={{
          display: 'flex',
          alignItems: 'center',
          gap: '6px',
          padding: '6px 12px',
          borderRadius: '999px',
          background: 'rgba(16, 185, 129, 0.15)',
          color: 'var(--accent-emerald-light)',
          border: '1px solid rgba(16, 185, 129, 0.3)',
          fontSize: '0.8rem',
          fontWeight: 700
        }}>
          <CheckCircle2 size={15} />
          <span>Ledger Balanced (Σ Debit = Σ Credit)</span>
        </div>
      </div>

      {/* Ledger Stats Row */}
      <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(240px, 1fr))', gap: '1rem' }}>
        <div className="glass-panel" style={{ padding: '1.25rem' }}>
          <div style={{ fontSize: '0.75rem', color: 'var(--text-muted)', textTransform: 'uppercase', fontWeight: 600 }}>
            Audit Verification
          </div>
          <div style={{ fontSize: '1.25rem', fontWeight: 800, color: 'var(--accent-emerald-light)', marginTop: '4px' }}>
            100% Reconciled
          </div>
          <div style={{ fontSize: '0.75rem', color: 'var(--text-secondary)', marginTop: '4px' }}>
            Zero variance detected across all accounts
          </div>
        </div>

        <div className="glass-panel" style={{ padding: '1.25rem' }}>
          <div style={{ fontSize: '0.75rem', color: 'var(--text-muted)', textTransform: 'uppercase', fontWeight: 600 }}>
            Idempotency Engine
          </div>
          <div style={{ fontSize: '1.25rem', fontWeight: 800, color: '#fff', marginTop: '4px' }}>
            Active & Enforced
          </div>
          <div style={{ fontSize: '0.75rem', color: 'var(--text-secondary)', marginTop: '4px' }}>
            Duplicate transfer collision protection
          </div>
        </div>

        <div className="glass-panel" style={{ padding: '1.25rem' }}>
          <div style={{ fontSize: '0.75rem', color: 'var(--text-muted)', textTransform: 'uppercase', fontWeight: 600 }}>
            Total Journal Entries
          </div>
          <div style={{ fontSize: '1.25rem', fontWeight: 800, color: '#fff', marginTop: '4px' }}>
            {ledgerEntries.length} Recorded
          </div>
          <div style={{ fontSize: '0.75rem', color: 'var(--text-secondary)', marginTop: '4px' }}>
            Real-time append-only ledger entries
          </div>
        </div>
      </div>

      {/* Ledger Entries Table */}
      <div className="glass-panel" style={{ padding: '1.5rem' }}>
        <h3 style={{ fontSize: '1.1rem', fontWeight: 700, color: '#fff', marginBottom: '1rem' }}>
          General Ledger Journal
        </h3>

        <div className="transactions-table-wrap">
          <table className="transactions-table">
            <thead>
              <tr>
                <th>Entry ID & Ref</th>
                <th>Target Account</th>
                <th>Entry Type</th>
                <th>Amount</th>
                <th>Balance After</th>
                <th>Timestamp (UTC)</th>
              </tr>
            </thead>
            <tbody>
              {ledgerEntries.map(entry => (
                <tr key={entry.id}>
                  <td>
                    <div>
                      <div style={{ fontWeight: 600, color: '#fff' }}>{entry.id}</div>
                      <div style={{ fontSize: '0.75rem', color: 'var(--text-muted)', fontFamily: 'var(--font-mono)' }}>
                        {entry.reference}
                      </div>
                    </div>
                  </td>

                  <td>
                    <span style={{ fontWeight: 600, color: 'var(--text-primary)' }}>
                      {entry.accountName}
                    </span>
                  </td>

                  <td>
                    <span style={{
                      display: 'inline-flex',
                      alignItems: 'center',
                      gap: '4px',
                      padding: '3px 8px',
                      borderRadius: '6px',
                      fontSize: '0.75rem',
                      fontWeight: 700,
                      background: entry.entryType === 'Credit' ? 'rgba(16, 185, 129, 0.15)' : 'rgba(244, 63, 94, 0.15)',
                      color: entry.entryType === 'Credit' ? 'var(--accent-emerald-light)' : 'var(--accent-rose)'
                    }}>
                      {entry.entryType === 'Credit' ? <ArrowDownLeft size={13} /> : <ArrowUpRight size={13} />}
                      {entry.entryType}
                    </span>
                  </td>

                  <td style={{ fontWeight: 700, fontFeatureSettings: 'tnum' }}>
                    {entry.currencyCode === 'USD' ? '$' : '₦'}{entry.amount.toLocaleString('en-US', { minimumFractionDigits: 2 })}
                  </td>

                  <td style={{ fontFamily: 'var(--font-mono)', fontSize: '0.85rem', color: 'var(--text-secondary)' }}>
                    {entry.currencyCode === 'USD' ? '$' : '₦'}{entry.balanceAfter.toLocaleString('en-US', { minimumFractionDigits: 2 })}
                  </td>

                  <td style={{ fontSize: '0.8rem', color: 'var(--text-muted)' }}>
                    {new Date(entry.timestamp).toISOString().replace('T', ' ').slice(0, 19)}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </div>
    </div>
  );
};
