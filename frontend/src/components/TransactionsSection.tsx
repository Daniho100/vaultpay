import { useState } from 'react';
import { 
  Search, 
  ArrowUpRight, 
  ArrowDownLeft, 
  SlidersHorizontal,
  ChevronRight
} from 'lucide-react';
import type { Transaction } from '../types';

interface TransactionsSectionProps {
  transactions: Transaction[];
  onSelectTransaction: (tx: Transaction) => void;
}

export const TransactionsSection = ({
  transactions,
  onSelectTransaction
}: TransactionsSectionProps) => {
  const [searchQuery, setSearchQuery] = useState('');
  const [filterType, setFilterType] = useState<'all' | 'credit' | 'debit'>('all');

  const filtered = transactions.filter(tx => {
    // Search filter
    const q = searchQuery.toLowerCase();
    const matchesSearch = 
      tx.description.toLowerCase().includes(q) ||
      tx.reference.toLowerCase().includes(q) ||
      (tx.toAccountName && tx.toAccountName.toLowerCase().includes(q)) ||
      (tx.fromAccountName && tx.fromAccountName.toLowerCase().includes(q));

    // Direction filter
    if (filterType === 'credit') return matchesSearch && !tx.isDebit;
    if (filterType === 'debit') return matchesSearch && tx.isDebit;
    return matchesSearch;
  });

  const formatDate = (iso: string) => {
    const d = new Date(iso);
    return d.toLocaleDateString('en-US', {
      month: 'short',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    });
  };

  const formatAmount = (amount: number, currency: string, isDebit: boolean) => {
    const sign = isDebit ? '-' : '+';
    const symbol = currency === 'USD' ? '$' : '₦';
    return `${sign}${symbol}${amount.toLocaleString('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 })}`;
  };

  return (
    <div className="glass-panel transactions-card">
      <div className="section-header-row" style={{ marginBottom: '1.25rem' }}>
        <div className="section-heading-group">
          <h2 className="section-heading">Transaction Activity</h2>
          <span className="count-pill">{filtered.length} Recorded</span>
        </div>
      </div>

      {/* Search and filters bar */}
      <div className="transactions-controls-bar">
        <div className="search-input-wrap">
          <Search size={16} className="search-icon" />
          <input
            type="text"
            className="table-search-input"
            placeholder="Search by recipient, reference, or note..."
            value={searchQuery}
            onChange={(e) => setSearchQuery(e.target.value)}
          />
        </div>

        <div className="filter-pills-row">
          <SlidersHorizontal size={14} color="var(--text-muted)" style={{ marginRight: '4px' }} />
          <button
            className={`filter-pill ${filterType === 'all' ? 'active' : ''}`}
            onClick={() => setFilterType('all')}
          >
            All Activity
          </button>
          <button
            className={`filter-pill ${filterType === 'credit' ? 'active' : ''}`}
            onClick={() => setFilterType('credit')}
          >
            Credits (Inflow)
          </button>
          <button
            className={`filter-pill ${filterType === 'debit' ? 'active' : ''}`}
            onClick={() => setFilterType('debit')}
          >
            Debits (Outflow)
          </button>
        </div>
      </div>

      {/* Transactions Table */}
      <div className="transactions-table-wrap">
        <table className="transactions-table">
          <thead>
            <tr>
              <th>Transaction / Details</th>
              <th>Category</th>
              <th>Date & Time</th>
              <th>Status</th>
              <th style={{ textAlign: 'right' }}>Amount</th>
              <th></th>
            </tr>
          </thead>
          <tbody>
            {filtered.length === 0 ? (
              <tr>
                <td colSpan={6} style={{ textAlign: 'center', padding: '3rem 1rem', color: 'var(--text-muted)' }}>
                  No transactions match your search or filter criteria.
                </td>
              </tr>
            ) : (
              filtered.map(tx => (
                <tr 
                  key={tx.id} 
                  onClick={() => onSelectTransaction(tx)}
                  style={{ cursor: 'pointer' }}
                >
                  <td>
                    <div className="txn-party-cell">
                      <div className={`txn-avatar ${tx.isDebit ? 'debit' : 'credit'}`}>
                        {tx.isDebit ? <ArrowUpRight size={17} /> : <ArrowDownLeft size={17} />}
                      </div>
                      <div>
                        <div className="txn-primary-name">
                          {tx.isDebit 
                            ? (tx.toAccountName || 'External Recipient')
                            : (tx.description || 'Deposit')}
                        </div>
                        <div className="txn-secondary-ref">
                          {tx.reference} • {tx.fromAccountName || 'Direct Inward'}
                        </div>
                      </div>
                    </div>
                  </td>

                  <td>
                    <span style={{
                      fontSize: '0.75rem',
                      fontWeight: 600,
                      color: 'var(--text-secondary)',
                      background: 'var(--bg-elevated)',
                      padding: '3px 8px',
                      borderRadius: '4px',
                      border: '1px solid var(--border-subtle)'
                    }}>
                      {tx.category || (tx.isDebit ? 'Transfer' : 'Deposit')}
                    </span>
                  </td>

                  <td style={{ color: 'var(--text-secondary)', fontSize: '0.8rem' }}>
                    {formatDate(tx.createdAt)}
                  </td>

                  <td>
                    <span className={`status-badge ${tx.status.toLowerCase()}`}>
                      <span style={{
                        width: '6px',
                        height: '6px',
                        borderRadius: '50%',
                        backgroundColor: tx.status === 'Completed' ? 'var(--accent-emerald)' : 'var(--accent-amber)'
                      }} />
                      {tx.status}
                    </span>
                  </td>

                  <td className={`txn-amount-text ${tx.isDebit ? 'debit' : 'credit'}`}>
                    {formatAmount(tx.amount, tx.currencyCode, tx.isDebit)}
                  </td>

                  <td style={{ textAlign: 'right', color: 'var(--text-muted)' }}>
                    <ChevronRight size={16} />
                  </td>
                </tr>
              ))
            )}
          </tbody>
        </table>
      </div>
    </div>
  );
};
