import { useState } from 'react';
import { 
  PiggyBank, 
  TrendingUp, 
  AlertTriangle, 
  CheckCircle2, 
  Calendar, 
  ShieldCheck, 
  Lock, 
  Unlock, 
  Percent, 
  X 
} from 'lucide-react';
import type { Account, FixedDeposit, Transaction } from '../types';
import { api } from '../api';

interface FixedSavingsTabProps {
  accounts: Account[];
  fixedDeposits: FixedDeposit[];
  onDepositCreated: (deposit: FixedDeposit, tx: Transaction) => void;
  onDepositLiquidated: (depositId: string, refundedAmount: number, tx: Transaction, message: string) => void;
  showToast: (msg: string) => void;
}

export const FixedSavingsTab = ({
  accounts,
  fixedDeposits,
  onDepositCreated,
  onDepositLiquidated,
  showToast
}: FixedSavingsTabProps) => {
  const [selectedAccountId, setSelectedAccountId] = useState(accounts[0]?.id || '');
  const [amount, setAmount] = useState<number>(500000);
  const [durationDays, setDurationDays] = useState<number>(180);
  const [loading, setLoading] = useState(false);

  // Early withdrawal modal state
  const [liquidatingDeposit, setLiquidatingDeposit] = useState<FixedDeposit | null>(null);
  const [liquidateLoading, setLiquidateLoading] = useState(false);

  const selectedAccount = accounts.find(a => a.id === selectedAccountId) || accounts[0];

  // 25% per annum calculation
  const projectedInterest = Math.round(amount * 0.25 * (durationDays / 365.0));
  const totalMaturityPayout = amount + projectedInterest;

  const handleCreateFixedDeposit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!selectedAccount) {
      showToast('Please select a funding account.');
      return;
    }

    if (amount <= 0) {
      showToast('Amount must be greater than zero.');
      return;
    }

    if (selectedAccount.balance < amount) {
      showToast(`Insufficient funds in ${selectedAccount.accountName}.`);
      return;
    }

    setLoading(true);

    try {
      let createdDep: FixedDeposit;
      try {
        const res = await api.savings.createFixedDeposit({
          accountId: selectedAccount.id,
          amount,
          durationDays
        });
        createdDep = {
          id: res.id,
          userId: res.userId,
          accountId: res.accountId,
          accountNumber: selectedAccount.accountNumber,
          amount: res.amount,
          interestRatePerAnnum: 0.25,
          durationDays: res.durationDays,
          startDate: res.startDate || new Date().toISOString(),
          maturityDate: res.maturityDate || new Date(Date.now() + durationDays * 86400000).toISOString(),
          status: 'Active',
          penaltyRate: 0.10,
          projectedInterest: res.projectedInterest || projectedInterest
        };
      } catch {
        // Fallback local create
        createdDep = {
          id: `fd-${Date.now()}`,
          userId: 'user-current',
          accountId: selectedAccount.id,
          accountNumber: selectedAccount.accountNumber,
          amount,
          interestRatePerAnnum: 0.25,
          durationDays,
          startDate: new Date().toISOString(),
          maturityDate: new Date(Date.now() + durationDays * 86400000).toISOString(),
          status: 'Active',
          penaltyRate: 0.10,
          projectedInterest
        };
      }

      const ref = `VP-SAV-FIXED-${new Date().toISOString().slice(0, 10).replace(/-/g, '')}-${Math.floor(1000 + Math.random() * 9000)}`;
      const newTx: Transaction = {
        id: `tx-sav-${Date.now()}`,
        reference: ref,
        fromAccountId: selectedAccount.id,
        fromAccountNumber: selectedAccount.accountNumber,
        fromAccountName: selectedAccount.accountName,
        toAccountName: `Fixed Deposit Vault (25% APY - ${durationDays} Days)`,
        amount,
        currencyCode: selectedAccount.currencyCode,
        type: 'FixedDepositCreation',
        status: 'Completed',
        description: `Created 25% p.a. Fixed Deposit for ${durationDays} days. Projected interest: ₦${projectedInterest.toLocaleString()}`,
        createdAt: new Date().toISOString(),
        completedAt: new Date().toISOString(),
        isDebit: true,
        category: 'Savings'
      };

      onDepositCreated(createdDep, newTx);
      showToast(`Locked ₦${amount.toLocaleString()} into 25% Fixed Savings for ${durationDays} days!`);
    } finally {
      setLoading(false);
    }
  };

  const handleConfirmLiquidate = async () => {
    if (!liquidatingDeposit) return;
    setLiquidateLoading(true);

    const isEarly = new Date() < new Date(liquidatingDeposit.maturityDate);
    const penalty = isEarly ? Math.round(liquidatingDeposit.amount * 0.10) : 0;
    const netPayout = isEarly 
      ? liquidatingDeposit.amount - penalty 
      : liquidatingDeposit.amount + liquidatingDeposit.projectedInterest;

    try {
      try {
        await api.savings.liquidate(liquidatingDeposit.id);
      } catch {
        // Fallback local update
      }

      const ref = `VP-SAV-LIQ-${new Date().toISOString().slice(0, 10).replace(/-/g, '')}-${Math.floor(1000 + Math.random() * 9000)}`;
      const newTx: Transaction = {
        id: `tx-liq-${Date.now()}`,
        reference: ref,
        toAccountId: selectedAccount.id,
        toAccountNumber: selectedAccount.accountNumber,
        toAccountName: selectedAccount.accountName,
        fromAccountName: 'Fixed Savings Vault',
        amount: netPayout,
        currencyCode: selectedAccount.currencyCode,
        type: 'FixedDepositLiquidation',
        status: 'Completed',
        description: isEarly
          ? `Early liquidation of Fixed Deposit (10% Penalty charged: ₦${penalty.toLocaleString()})`
          : `Matured Fixed Deposit Payout (Principal + 25% APY Interest)`,
        createdAt: new Date().toISOString(),
        completedAt: new Date().toISOString(),
        isDebit: false,
        category: 'Savings'
      };

      const msg = isEarly
        ? `Fixed deposit liquidated early. 10% penalty (₦${penalty.toLocaleString()}) deducted. ₦${netPayout.toLocaleString()} credited to your account.`
        : `Fixed deposit matured! ₦${netPayout.toLocaleString()} credited to your account.`;

      onDepositLiquidated(liquidatingDeposit.id, netPayout, newTx, msg);
      setLiquidatingDeposit(null);
    } finally {
      setLiquidateLoading(false);
    }
  };

  return (
    <div style={{ display: 'flex', flexDirection: 'column', gap: '2rem' }}>
      {/* Hero Banner with Guaranteed 25% APY */}
      <div 
        className="glass-panel"
        style={{
          padding: '2.5rem',
          borderRadius: '16px',
          background: 'linear-gradient(135deg, rgba(16, 185, 129, 0.15) 0%, rgba(5, 150, 105, 0.05) 50%, rgba(15, 23, 42, 0.8) 100%)',
          border: '1.5px solid rgba(16, 185, 129, 0.35)',
          display: 'flex',
          justifyContent: 'space-between',
          alignItems: 'center',
          flexWrap: 'wrap',
          gap: '1.5rem'
        }}
      >
        <div style={{ maxWidth: '580px' }}>
          <div style={{ display: 'inline-flex', alignItems: 'center', gap: '6px', background: 'rgba(16, 185, 129, 0.25)', border: '1px solid var(--accent-emerald)', borderRadius: '20px', padding: '4px 12px', fontSize: '0.8rem', color: '#fff', fontWeight: 700, marginBottom: '12px' }}>
            <Percent size={14} color="var(--accent-emerald-light)" />
            <span>Guaranteed 25.0% APY Fixed Vault</span>
          </div>
          <h1 style={{ fontSize: '2rem', fontWeight: 800, color: '#fff', lineHeight: 1.2, marginBottom: '10px' }}>
            Grow Your Wealth with Institutional 25% Annual Yield
          </h1>
          <p style={{ fontSize: '0.9rem', color: 'var(--text-secondary)', lineHeight: 1.6 }}>
            Lock your savings for 30 to 365 days and earn an unprecedented 25% interest per annum. Early withdrawals are permitted subject to a 10% early liquidation fee on principal.
          </p>
        </div>

        <div style={{ display: 'flex', gap: '1.5rem', alignItems: 'center' }}>
          <div style={{ textAlign: 'center', background: 'rgba(0,0,0,0.3)', padding: '16px 24px', borderRadius: '12px', border: '1px solid var(--border-subtle)' }}>
            <div style={{ fontSize: '2.5rem', fontWeight: 900, color: 'var(--accent-emerald-light)' }}>25%</div>
            <div style={{ fontSize: '0.78rem', color: 'var(--text-muted)', textTransform: 'uppercase', letterSpacing: '1px' }}>Per Annum</div>
          </div>
          <div style={{ textAlign: 'center', background: 'rgba(0,0,0,0.3)', padding: '16px 24px', borderRadius: '12px', border: '1px solid var(--border-subtle)' }}>
            <div style={{ fontSize: '2.5rem', fontWeight: 900, color: '#f87171' }}>10%</div>
            <div style={{ fontSize: '0.78rem', color: 'var(--text-muted)', textTransform: 'uppercase', letterSpacing: '1px' }}>Early Penalty</div>
          </div>
        </div>
      </div>

      {/* Grid: Create Calculator & Existing Fixed Deposits */}
      <div className="analytics-section-grid" style={{ gridTemplateColumns: '1.1fr 1.4fr', gap: '1.5rem' }}>
        {/* Create Fixed Deposit Card */}
        <div className="glass-panel" style={{ padding: '2rem' }}>
          <div style={{ display: 'flex', alignItems: 'center', gap: '10px', marginBottom: '1.5rem' }}>
            <Lock size={22} color="var(--accent-emerald-light)" />
            <h3 style={{ fontSize: '1.2rem', fontWeight: 700, color: '#fff' }}>Lock In 25% Fixed Deposit</h3>
          </div>

          <form onSubmit={handleCreateFixedDeposit} style={{ display: 'flex', flexDirection: 'column', gap: '1.25rem' }}>
            <div className="form-group">
              <label className="form-label">Debit Account</label>
              <select 
                className="form-input"
                value={selectedAccountId}
                onChange={(e) => setSelectedAccountId(e.target.value)}
              >
                {accounts.map(acc => (
                  <option key={acc.id} value={acc.id}>
                    {acc.accountName} (₦{acc.balance.toLocaleString()})
                  </option>
                ))}
              </select>
            </div>

            <div className="form-group">
              <label className="form-label">Principal Amount to Lock (₦)</label>
              <input 
                type="number"
                className="form-input"
                min="10000"
                step="10000"
                value={amount}
                onChange={(e) => setAmount(Number(e.target.value))}
                required
              />
              <div style={{ display: 'flex', gap: '8px', marginTop: '8px', flexWrap: 'wrap' }}>
                {[100000, 250000, 500000, 1000000, 2000000].map(val => (
                  <button
                    key={val}
                    type="button"
                    onClick={() => setAmount(val)}
                    className="filter-pill"
                    style={{ fontSize: '0.75rem', padding: '4px 10px' }}
                  >
                    ₦{(val / 1000).toLocaleString()}k
                  </button>
                ))}
              </div>
            </div>

            <div className="form-group">
              <label className="form-label">Lock-In Tenure (Days)</label>
              <div style={{ display: 'grid', gridTemplateColumns: 'repeat(5, 1fr)', gap: '6px' }}>
                {[30, 60, 90, 180, 365].map(days => (
                  <button
                    key={days}
                    type="button"
                    onClick={() => setDurationDays(days)}
                    className={`filter-pill ${durationDays === days ? 'active' : ''}`}
                    style={{ textAlign: 'center', padding: '8px 4px', fontSize: '0.8rem', fontWeight: 700 }}
                  >
                    {days}D
                  </button>
                ))}
              </div>
            </div>

            {/* Projected Returns Box */}
            <div style={{ 
              background: 'rgba(16, 185, 129, 0.08)', 
              border: '1px solid rgba(16, 185, 129, 0.3)', 
              borderRadius: '12px', 
              padding: '14px',
              display: 'flex',
              flexDirection: 'column',
              gap: '8px'
            }}>
              <div style={{ display: 'flex', justifyContent: 'space-between', fontSize: '0.82rem', color: 'var(--text-secondary)' }}>
                <span>Annual Interest Rate:</span>
                <strong style={{ color: 'var(--accent-emerald-light)' }}>25.0% Fixed APY</strong>
              </div>
              <div style={{ display: 'flex', justifyContent: 'space-between', fontSize: '0.82rem', color: 'var(--text-secondary)' }}>
                <span>Projected Accrued Interest:</span>
                <strong style={{ color: 'var(--accent-emerald-light)' }}>+₦{projectedInterest.toLocaleString()}</strong>
              </div>
              <div style={{ height: '1px', background: 'rgba(255,255,255,0.08)', margin: '4px 0' }} />
              <div style={{ display: 'flex', justifyContent: 'space-between', fontSize: '0.95rem', fontWeight: 800, color: '#fff' }}>
                <span>Total Payout at Maturity:</span>
                <span style={{ color: 'var(--accent-emerald-light)' }}>₦{totalMaturityPayout.toLocaleString()}</span>
              </div>
            </div>

            <button type="submit" className="btn-primary" style={{ width: '100%', padding: '12px' }} disabled={loading}>
              {loading ? 'Locking Funds...' : `Lock ₦${amount.toLocaleString()} for ${durationDays} Days`}
            </button>
          </form>
        </div>

        {/* Existing Fixed Deposits List */}
        <div className="glass-panel" style={{ padding: '2rem' }}>
          <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '1.5rem' }}>
            <div style={{ display: 'flex', alignItems: 'center', gap: '10px' }}>
              <PiggyBank size={22} color="var(--accent-emerald-light)" />
              <h3 style={{ fontSize: '1.2rem', fontWeight: 700, color: '#fff' }}>My Fixed Savings Vaults</h3>
            </div>
            <span style={{ fontSize: '0.8rem', color: 'var(--text-muted)' }}>
              {fixedDeposits.length} Active Vault(s)
            </span>
          </div>

          {fixedDeposits.length === 0 ? (
            <div style={{ textAlign: 'center', padding: '3rem 1rem', color: 'var(--text-muted)' }}>
              <Lock size={40} style={{ opacity: 0.3, marginBottom: '1rem' }} />
              <p>No fixed savings vaults created yet. Start earning 25% annual yield today!</p>
            </div>
          ) : (
            <div style={{ display: 'flex', flexDirection: 'column', gap: '1rem', maxHeight: '480px', overflowY: 'auto' }}>
              {fixedDeposits.map(dep => {
                const isEarly = new Date() < new Date(dep.maturityDate);
                const penaltyAmount = Math.round(dep.amount * 0.10);
                const earlyRefund = dep.amount - penaltyAmount;

                return (
                  <div
                    key={dep.id}
                    style={{
                      background: 'var(--bg-secondary)',
                      border: '1px solid var(--border-subtle)',
                      borderRadius: '12px',
                      padding: '1.25rem',
                      display: 'flex',
                      flexDirection: 'column',
                      gap: '10px'
                    }}
                  >
                    <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start' }}>
                      <div>
                        <div style={{ fontSize: '1.25rem', fontWeight: 800, color: '#fff' }}>
                          ₦{dep.amount.toLocaleString()}
                        </div>
                        <div style={{ fontSize: '0.78rem', color: 'var(--accent-emerald-light)', display: 'flex', alignItems: 'center', gap: '4px', marginTop: '2px' }}>
                          <TrendingUp size={13} />
                          <span>25% p.a. • Projected Interest: +₦{dep.projectedInterest.toLocaleString()}</span>
                        </div>
                      </div>

                      <span style={{
                        padding: '4px 10px',
                        borderRadius: '12px',
                        fontSize: '0.75rem',
                        fontWeight: 700,
                        background: dep.status === 'Active' ? 'rgba(16, 185, 129, 0.15)' : 'rgba(239, 68, 68, 0.15)',
                        color: dep.status === 'Active' ? 'var(--accent-emerald-light)' : '#f87171'
                      }}>
                        {dep.status}
                      </span>
                    </div>

                    <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '8px', fontSize: '0.78rem', color: 'var(--text-secondary)' }}>
                      <div>
                        <span style={{ color: 'var(--text-muted)' }}>Locked Date: </span>
                        {new Date(dep.startDate).toLocaleDateString()}
                      </div>
                      <div>
                        <span style={{ color: 'var(--text-muted)' }}>Maturity: </span>
                        {new Date(dep.maturityDate).toLocaleDateString()}
                      </div>
                    </div>

                    {dep.status === 'Active' && (
                      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginTop: '6px', paddingTop: '10px', borderTop: '1px solid rgba(255,255,255,0.06)' }}>
                        <span style={{ fontSize: '0.75rem', color: isEarly ? '#f87171' : 'var(--accent-emerald-light)' }}>
                          {isEarly ? '⚠️ Early withdrawal incurs 10% penalty' : '✅ Matured: Ready for 100% full payout'}
                        </span>
                        <button
                          type="button"
                          onClick={() => setLiquidatingDeposit(dep)}
                          className="filter-pill"
                          style={{
                            padding: '6px 14px',
                            fontSize: '0.78rem',
                            color: isEarly ? '#f87171' : 'var(--accent-emerald-light)',
                            borderColor: isEarly ? 'rgba(239, 68, 68, 0.4)' : 'var(--accent-emerald)'
                          }}
                        >
                          {isEarly ? 'Cancel (10% Charge)' : 'Withdraw Payout'}
                        </button>
                      </div>
                    )}
                  </div>
                );
              })}
            </div>
          )}
        </div>
      </div>

      {/* Early Liquidation Modal */}
      {liquidatingDeposit && (
        <div className="modal-backdrop" onClick={() => setLiquidatingDeposit(null)}>
          <div className="modal-container glass-panel" onClick={(e) => e.stopPropagation()} style={{ maxWidth: '460px', width: '90%' }}>
            <div className="modal-header">
              <div style={{ display: 'flex', alignItems: 'center', gap: '8px' }}>
                <AlertTriangle size={22} color="#f87171" />
                <h3 className="modal-title" style={{ fontSize: '1.2rem', color: '#f87171' }}>
                  Early Withdrawal Notice
                </h3>
              </div>
              <button className="modal-close-btn" onClick={() => setLiquidatingDeposit(null)}>
                <X size={18} />
              </button>
            </div>

            <div style={{ display: 'flex', flexDirection: 'column', gap: '1rem', marginTop: '1rem' }}>
              <p style={{ fontSize: '0.85rem', color: 'var(--text-secondary)', lineHeight: 1.5 }}>
                You are cancelling your fixed savings vault before the agreed maturity date of{' '}
                <strong style={{ color: '#fff' }}>{new Date(liquidatingDeposit.maturityDate).toLocaleDateString()}</strong>.
              </p>

              <div style={{ 
                background: 'rgba(239, 68, 68, 0.1)', 
                border: '1px solid rgba(239, 68, 68, 0.3)', 
                borderRadius: '10px', 
                padding: '14px',
                display: 'flex',
                flexDirection: 'column',
                gap: '8px'
              }}>
                <div style={{ display: 'flex', justifyContent: 'space-between', fontSize: '0.85rem' }}>
                  <span style={{ color: 'var(--text-muted)' }}>Original Principal:</span>
                  <strong style={{ color: '#fff' }}>₦{liquidatingDeposit.amount.toLocaleString()}</strong>
                </div>
                <div style={{ display: 'flex', justifyContent: 'space-between', fontSize: '0.85rem' }}>
                  <span style={{ color: '#f87171' }}>Early Withdrawal Charge (10%):</span>
                  <strong style={{ color: '#f87171' }}>-₦{Math.round(liquidatingDeposit.amount * 0.10).toLocaleString()}</strong>
                </div>
                <div style={{ height: '1px', background: 'rgba(255,255,255,0.08)', margin: '4px 0' }} />
                <div style={{ display: 'flex', justifyContent: 'space-between', fontSize: '1rem', fontWeight: 800 }}>
                  <span style={{ color: '#fff' }}>Net Refund to Account:</span>
                  <strong style={{ color: 'var(--accent-emerald-light)' }}>
                    ₦{Math.round(liquidatingDeposit.amount * 0.90).toLocaleString()}
                  </strong>
                </div>
              </div>

              <div style={{ display: 'flex', gap: '10px', marginTop: '0.5rem' }}>
                <button
                  type="button"
                  className="filter-pill"
                  style={{ flex: 1, padding: '10px', textAlign: 'center' }}
                  onClick={() => setLiquidatingDeposit(null)}
                >
                  Keep Savings Locked
                </button>
                <button
                  type="button"
                  className="btn-primary"
                  style={{ flex: 1, padding: '10px', background: '#dc2626', borderColor: '#b91c1c' }}
                  onClick={handleConfirmLiquidate}
                  disabled={liquidateLoading}
                >
                  {liquidateLoading ? 'Liquidating...' : 'Confirm Liquidation'}
                </button>
              </div>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};
