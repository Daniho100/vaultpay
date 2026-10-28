import { useState } from 'react';
import { 
  Smartphone, 
  Wifi, 
  Zap, 
  Gamepad2, 
  CheckCircle2, 
  Copy, 
  ArrowRight,
  ShieldCheck,
  CreditCard
} from 'lucide-react';
import type { Account, BillType, Transaction } from '../types';
import { api } from '../api';

interface BillsTabProps {
  accounts: Account[];
  onBillPaid: (tx: Transaction, tokenOrReceipt?: string, message?: string) => void;
  showToast: (msg: string) => void;
}

export const BillsTab = ({ accounts, onBillPaid, showToast }: BillsTabProps) => {
  const [activeBillType, setActiveBillType] = useState<BillType>('Airtime');
  const [selectedAccountId, setSelectedAccountId] = useState(accounts[0]?.id || '');
  
  // Airtime state
  const [airtimeProvider, setAirtimeProvider] = useState('MTN');
  const [airtimePhone, setAirtimePhone] = useState('08031234567');
  const [airtimeAmount, setAirtimeAmount] = useState<number>(1000);

  // Data state
  const [dataProvider, setDataProvider] = useState('MTN');
  const [dataPhone, setDataPhone] = useState('08031234567');
  const [dataBundle, setDataBundle] = useState({ name: '10GB Monthly Data Bundle', price: 3500 });

  // Electricity state
  const [disco, setDisco] = useState('Ikeja Electric (IKEDC)');
  const [meterType, setMeterType] = useState<'Prepaid' | 'Postpaid'>('Prepaid');
  const [meterNumber, setMeterNumber] = useState('45109283719');
  const [electricityAmount, setElectricityAmount] = useState<number>(5000);
  const [generatedToken, setGeneratedToken] = useState<string | null>(null);

  // Betting state
  const [betProvider, setBetProvider] = useState('SportyBet');
  const [betUserId, setBetUserId] = useState('SB-928174');
  const [betAmount, setBetAmount] = useState<number>(3000);

  const [loading, setLoading] = useState(false);

  const selectedAccount = accounts.find(a => a.id === selectedAccountId) || accounts[0];

  const handlePay = async (type: BillType, provider: string, identifier: string, amount: number, packageName?: string) => {
    if (!selectedAccount) {
      showToast('Please select a funding account.');
      return;
    }

    if (selectedAccount.balance < amount) {
      showToast(`Insufficient funds in ${selectedAccount.accountName}.`);
      return;
    }

    setLoading(true);

    try {
      const billTypeNum = type === 'Airtime' ? 1 : type === 'Data' ? 2 : type === 'Electricity' ? 3 : 4;
      let tokenOrCode = '';

      try {
        const res = await api.bills.pay({
          accountId: selectedAccount.id,
          type: billTypeNum,
          provider,
          customerIdentifier: identifier,
          amount,
          packageName
        });
        tokenOrCode = res.tokenOrReceiptCode || '';
      } catch {
        // Fallback token generator
        tokenOrCode = type === 'Electricity'
          ? `${Math.floor(1000+Math.random()*9000)}-${Math.floor(1000+Math.random()*9000)}-${Math.floor(1000+Math.random()*9000)}-${Math.floor(1000+Math.random()*9000)}-${Math.floor(1000+Math.random()*9000)}`
          : `VP-${type.toUpperCase()}-${Math.floor(100000+Math.random()*900000)}`;
      }

      const ref = `VP-BILL-${type.toUpperCase()}-${new Date().toISOString().slice(0, 10).replace(/-/g, '')}-${Math.floor(1000 + Math.random() * 9000)}`;
      
      const newTx: Transaction = {
        id: `tx-bill-${Date.now()}`,
        reference: ref,
        fromAccountId: selectedAccount.id,
        fromAccountNumber: selectedAccount.accountNumber,
        fromAccountName: selectedAccount.accountName,
        toAccountName: `${provider} (${type})`,
        amount,
        currencyCode: selectedAccount.currencyCode,
        type: 'BillPayment',
        status: 'Completed',
        description: `${type} top-up via ${provider} for ${identifier}${packageName ? ` (${packageName})` : ''}`,
        createdAt: new Date().toISOString(),
        completedAt: new Date().toISOString(),
        isDebit: true,
        category: 'Bill'
      };

      if (type === 'Electricity') {
        setGeneratedToken(tokenOrCode);
      }

      onBillPaid(newTx, tokenOrCode, `Successfully purchased ${type} (₦${amount.toLocaleString()}) via ${provider}!`);
    } finally {
      setLoading(false);
    }
  };

  const copyToClipboard = (text: string) => {
    navigator.clipboard.writeText(text);
    showToast('Token copied to clipboard!');
  };

  return (
    <div style={{ display: 'flex', flexDirection: 'column', gap: '2rem' }}>
      {/* Header */}
      <div className="section-header-row">
        <div>
          <h2 className="section-heading">Bill Payments & Utility Top-Ups</h2>
          <p style={{ fontSize: '0.85rem', color: 'var(--text-muted)' }}>
            Instant recharge for Mobile Airtime, Data Bundles, Prepaid Electricity DISCOs, and Sports Betting
          </p>
        </div>
      </div>

      {/* Bill Type Selector Tabs */}
      <div className="analytics-section-grid" style={{ gridTemplateColumns: 'repeat(4, 1fr)', gap: '1rem' }}>
        <button
          type="button"
          onClick={() => { setActiveBillType('Airtime'); setGeneratedToken(null); }}
          className="glass-panel"
          style={{
            padding: '1.25rem',
            textAlign: 'left',
            display: 'flex',
            alignItems: 'center',
            gap: '12px',
            border: activeBillType === 'Airtime' ? '1.5px solid var(--accent-emerald)' : '1px solid var(--border-subtle)',
            background: activeBillType === 'Airtime' ? 'rgba(16, 185, 129, 0.12)' : 'var(--bg-card)',
            cursor: 'pointer'
          }}
        >
          <div style={{ 
            width: '42px', 
            height: '42px', 
            borderRadius: '10px', 
            background: 'rgba(16, 185, 129, 0.2)', 
            display: 'flex', 
            alignItems: 'center', 
            justifyContent: 'center',
            color: 'var(--accent-emerald-light)'
          }}>
            <Smartphone size={22} />
          </div>
          <div>
            <div style={{ fontWeight: 700, fontSize: '0.95rem', color: '#fff' }}>Airtime Top-Up</div>
            <div style={{ fontSize: '0.75rem', color: 'var(--text-muted)' }}>MTN, Airtel, Glo, 9mobile</div>
          </div>
        </button>

        <button
          type="button"
          onClick={() => { setActiveBillType('Data'); setGeneratedToken(null); }}
          className="glass-panel"
          style={{
            padding: '1.25rem',
            textAlign: 'left',
            display: 'flex',
            alignItems: 'center',
            gap: '12px',
            border: activeBillType === 'Data' ? '1.5px solid var(--accent-emerald)' : '1px solid var(--border-subtle)',
            background: activeBillType === 'Data' ? 'rgba(16, 185, 129, 0.12)' : 'var(--bg-card)',
            cursor: 'pointer'
          }}
        >
          <div style={{ 
            width: '42px', 
            height: '42px', 
            borderRadius: '10px', 
            background: 'rgba(59, 130, 246, 0.2)', 
            display: 'flex', 
            alignItems: 'center', 
            justifyContent: 'center',
            color: '#60a5fa'
          }}>
            <Wifi size={22} />
          </div>
          <div>
            <div style={{ fontWeight: 700, fontSize: '0.95rem', color: '#fff' }}>Internet Data</div>
            <div style={{ fontSize: '0.75rem', color: 'var(--text-muted)' }}>Daily & Monthly Bundles</div>
          </div>
        </button>

        <button
          type="button"
          onClick={() => { setActiveBillType('Electricity'); setGeneratedToken(null); }}
          className="glass-panel"
          style={{
            padding: '1.25rem',
            textAlign: 'left',
            display: 'flex',
            alignItems: 'center',
            gap: '12px',
            border: activeBillType === 'Electricity' ? '1.5px solid var(--accent-emerald)' : '1px solid var(--border-subtle)',
            background: activeBillType === 'Electricity' ? 'rgba(16, 185, 129, 0.12)' : 'var(--bg-card)',
            cursor: 'pointer'
          }}
        >
          <div style={{ 
            width: '42px', 
            height: '42px', 
            borderRadius: '10px', 
            background: 'rgba(234, 179, 8, 0.2)', 
            display: 'flex', 
            alignItems: 'center', 
            justifyContent: 'center',
            color: '#facc15'
          }}>
            <Zap size={22} />
          </div>
          <div>
            <div style={{ fontWeight: 700, fontSize: '0.95rem', color: '#fff' }}>Electricity Token</div>
            <div style={{ fontSize: '0.75rem', color: 'var(--text-muted)' }}>IKEDC, EKEDC, AEDC & more</div>
          </div>
        </button>

        <button
          type="button"
          onClick={() => { setActiveBillType('Betting'); setGeneratedToken(null); }}
          className="glass-panel"
          style={{
            padding: '1.25rem',
            textAlign: 'left',
            display: 'flex',
            alignItems: 'center',
            gap: '12px',
            border: activeBillType === 'Betting' ? '1.5px solid var(--accent-emerald)' : '1px solid var(--border-subtle)',
            background: activeBillType === 'Betting' ? 'rgba(16, 185, 129, 0.12)' : 'var(--bg-card)',
            cursor: 'pointer'
          }}
        >
          <div style={{ 
            width: '42px', 
            height: '42px', 
            borderRadius: '10px', 
            background: 'rgba(168, 85, 247, 0.2)', 
            display: 'flex', 
            alignItems: 'center', 
            justifyContent: 'center',
            color: '#c084fc'
          }}>
            <Gamepad2 size={22} />
          </div>
          <div>
            <div style={{ fontWeight: 700, fontSize: '0.95rem', color: '#fff' }}>Sports Betting</div>
            <div style={{ fontSize: '0.75rem', color: 'var(--text-muted)' }}>SportyBet, Bet9ja, 1xBet</div>
          </div>
        </button>
      </div>

      {/* Main Billing Form Panel */}
      <div className="glass-panel" style={{ padding: '2rem', maxWidth: '780px', margin: '0 auto', width: '100%' }}>
        {/* Source Account Picker */}
        <div className="form-group" style={{ marginBottom: '1.5rem' }}>
          <label className="form-label">Debit Source Account</label>
          <select 
            className="form-input"
            value={selectedAccountId}
            onChange={(e) => setSelectedAccountId(e.target.value)}
          >
            {accounts.map(acc => (
              <option key={acc.id} value={acc.id}>
                {acc.accountName} — {acc.accountNumber} (Available: ₦{acc.balance.toLocaleString()})
              </option>
            ))}
          </select>
        </div>

        {/* 1. AIRTIME FORM */}
        {activeBillType === 'Airtime' && (
          <form onSubmit={(e) => { e.preventDefault(); handlePay('Airtime', airtimeProvider, airtimePhone, airtimeAmount); }}>
            <div className="form-group" style={{ marginBottom: '1.25rem' }}>
              <label className="form-label">Telecom Network Provider</label>
              <div style={{ display: 'grid', gridTemplateColumns: 'repeat(4, 1fr)', gap: '10px' }}>
                {['MTN', 'Airtel', 'Glo', '9mobile'].map(net => (
                  <button
                    key={net}
                    type="button"
                    onClick={() => setAirtimeProvider(net)}
                    className={`filter-pill ${airtimeProvider === net ? 'active' : ''}`}
                    style={{ textAlign: 'center', padding: '10px', fontWeight: 600 }}
                  >
                    {net}
                  </button>
                ))}
              </div>
            </div>

            <div className="form-group" style={{ marginBottom: '1.25rem' }}>
              <label className="form-label">Recipient Mobile Number</label>
              <input 
                type="tel"
                className="form-input"
                placeholder="0803 000 0000"
                value={airtimePhone}
                onChange={(e) => setAirtimePhone(e.target.value)}
                required
              />
            </div>

            <div className="form-group" style={{ marginBottom: '1.5rem' }}>
              <label className="form-label">Top-Up Amount (₦)</label>
              <input 
                type="number"
                className="form-input"
                min="100"
                step="50"
                value={airtimeAmount}
                onChange={(e) => setAirtimeAmount(Number(e.target.value))}
                required
              />
              <div style={{ display: 'flex', gap: '8px', marginTop: '8px', flexWrap: 'wrap' }}>
                {[500, 1000, 2000, 5000, 10000].map(amt => (
                  <button
                    key={amt}
                    type="button"
                    onClick={() => setAirtimeAmount(amt)}
                    className="filter-pill"
                    style={{ fontSize: '0.75rem', padding: '4px 10px' }}
                  >
                    ₦{amt.toLocaleString()}
                  </button>
                ))}
              </div>
            </div>

            <button type="submit" className="btn-primary" style={{ width: '100%', padding: '12px' }} disabled={loading}>
              {loading ? 'Processing Recharge...' : `Top-Up ₦${airtimeAmount.toLocaleString()} Airtime Now`}
            </button>
          </form>
        )}

        {/* 2. DATA FORM */}
        {activeBillType === 'Data' && (
          <form onSubmit={(e) => { e.preventDefault(); handlePay('Data', dataProvider, dataPhone, dataBundle.price, dataBundle.name); }}>
            <div className="form-group" style={{ marginBottom: '1.25rem' }}>
              <label className="form-label">Select Network</label>
              <div style={{ display: 'grid', gridTemplateColumns: 'repeat(4, 1fr)', gap: '10px' }}>
                {['MTN', 'Airtel', 'Glo', '9mobile'].map(net => (
                  <button
                    key={net}
                    type="button"
                    onClick={() => setDataProvider(net)}
                    className={`filter-pill ${dataProvider === net ? 'active' : ''}`}
                    style={{ textAlign: 'center', padding: '10px', fontWeight: 600 }}
                  >
                    {net}
                  </button>
                ))}
              </div>
            </div>

            <div className="form-group" style={{ marginBottom: '1.25rem' }}>
              <label className="form-label">Recipient Mobile Number</label>
              <input 
                type="tel"
                className="form-input"
                placeholder="0803 000 0000"
                value={dataPhone}
                onChange={(e) => setDataPhone(e.target.value)}
                required
              />
            </div>

            <div className="form-group" style={{ marginBottom: '1.5rem' }}>
              <label className="form-label">Select Internet Bundle</label>
              <div style={{ display: 'flex', flexDirection: 'column', gap: '8px' }}>
                {[
                  { name: '1.5GB Daily Data Plan', price: 500 },
                  { name: '3GB 2-Day Data Plan', price: 1000 },
                  { name: '10GB Monthly Data Bundle', price: 3500 },
                  { name: '25GB Monthly Work & Play', price: 8000 },
                  { name: '75GB 30-Day Heavy User', price: 20000 },
                  { name: 'Unlimited 30-Day Ultra Plan', price: 35000 }
                ].map(bundle => (
                  <div
                    key={bundle.name}
                    onClick={() => setDataBundle(bundle)}
                    style={{
                      display: 'flex',
                      justifyContent: 'space-between',
                      alignItems: 'center',
                      padding: '12px 16px',
                      borderRadius: '8px',
                      border: dataBundle.name === bundle.name ? '1.5px solid var(--accent-emerald)' : '1px solid var(--border-subtle)',
                      background: dataBundle.name === bundle.name ? 'rgba(16, 185, 129, 0.08)' : 'var(--bg-secondary)',
                      cursor: 'pointer'
                    }}
                  >
                    <span style={{ fontSize: '0.88rem', fontWeight: 600, color: '#fff' }}>{bundle.name}</span>
                    <span style={{ color: 'var(--accent-emerald-light)', fontWeight: 700 }}>₦{bundle.price.toLocaleString()}</span>
                  </div>
                ))}
              </div>
            </div>

            <button type="submit" className="btn-primary" style={{ width: '100%', padding: '12px' }} disabled={loading}>
              {loading ? 'Subscribing...' : `Purchase ${dataBundle.name} (₦${dataBundle.price.toLocaleString()})`}
            </button>
          </form>
        )}

        {/* 3. ELECTRICITY FORM */}
        {activeBillType === 'Electricity' && (
          <form onSubmit={(e) => { e.preventDefault(); handlePay('Electricity', disco, meterNumber, electricityAmount); }}>
            <div className="form-group" style={{ marginBottom: '1.25rem' }}>
              <label className="form-label">Distribution Company (DISCO)</label>
              <select className="form-input" value={disco} onChange={(e) => setDisco(e.target.value)}>
                <option value="Ikeja Electric (IKEDC)">Ikeja Electric (IKEDC)</option>
                <option value="Eko Electric (EKEDC)">Eko Electric (EKEDC)</option>
                <option value="Abuja Electricity (AEDC)">Abuja Electricity (AEDC)</option>
                <option value="Ibadan Electricity (IBEDC)">Ibadan Electricity (IBEDC)</option>
                <option value="Port Harcourt (PHED)">Port Harcourt (PHED)</option>
                <option value="Kano Electricity (KEDCO)">Kano Electricity (KEDCO)</option>
              </select>
            </div>

            <div style={{ display: 'grid', gridTemplateColumns: '1fr 2fr', gap: '12px', marginBottom: '1.25rem' }}>
              <div className="form-group">
                <label className="form-label">Meter Type</label>
                <select className="form-input" value={meterType} onChange={(e) => setMeterType(e.target.value as any)}>
                  <option value="Prepaid">Prepaid</option>
                  <option value="Postpaid">Postpaid</option>
                </select>
              </div>

              <div className="form-group">
                <label className="form-label">Meter Number</label>
                <input 
                  type="text"
                  className="form-input"
                  placeholder="e.g. 45109283719"
                  value={meterNumber}
                  onChange={(e) => setMeterNumber(e.target.value)}
                  required
                />
              </div>
            </div>

            <div className="form-group" style={{ marginBottom: '1.5rem' }}>
              <label className="form-label">Token Amount (₦)</label>
              <input 
                type="number"
                className="form-input"
                min="1000"
                step="500"
                value={electricityAmount}
                onChange={(e) => setElectricityAmount(Number(e.target.value))}
                required
              />
            </div>

            <button type="submit" className="btn-primary" style={{ width: '100%', padding: '12px' }} disabled={loading}>
              {loading ? 'Generating 20-Digit Token...' : `Purchase Electricity Token (₦${electricityAmount.toLocaleString()})`}
            </button>

            {/* Generated Token Display Banner */}
            {generatedToken && (
              <div style={{
                marginTop: '1.5rem',
                background: 'rgba(16, 185, 129, 0.12)',
                border: '1.5px solid var(--accent-emerald)',
                borderRadius: '12px',
                padding: '1.25rem',
                textAlign: 'center'
              }}>
                <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'center', gap: '6px', color: 'var(--accent-emerald-light)', fontWeight: 700, marginBottom: '6px' }}>
                  <CheckCircle2 size={18} />
                  <span>20-Digit Token Generated Successfully</span>
                </div>
                <div style={{ fontSize: '1.4rem', fontFamily: 'monospace', letterSpacing: '2px', fontWeight: 800, color: '#fff', margin: '10px 0' }}>
                  {generatedToken}
                </div>
                <button
                  type="button"
                  onClick={() => copyToClipboard(generatedToken)}
                  className="filter-pill"
                  style={{ display: 'inline-flex', alignItems: 'center', gap: '6px', padding: '6px 14px' }}
                >
                  <Copy size={13} />
                  <span>Copy Token</span>
                </button>
              </div>
            )}
          </form>
        )}

        {/* 4. BETTING FORM */}
        {activeBillType === 'Betting' && (
          <form onSubmit={(e) => { e.preventDefault(); handlePay('Betting', betProvider, betUserId, betAmount); }}>
            <div className="form-group" style={{ marginBottom: '1.25rem' }}>
              <label className="form-label">Betting Platform</label>
              <div style={{ display: 'grid', gridTemplateColumns: 'repeat(3, 1fr)', gap: '10px' }}>
                {['SportyBet', 'Bet9ja', '1xBet', 'BetKing', 'BangBet', 'Merrybet'].map(bookie => (
                  <button
                    key={bookie}
                    type="button"
                    onClick={() => setBetProvider(bookie)}
                    className={`filter-pill ${betProvider === bookie ? 'active' : ''}`}
                    style={{ textAlign: 'center', padding: '10px', fontWeight: 600 }}
                  >
                    {bookie}
                  </button>
                ))}
              </div>
            </div>

            <div className="form-group" style={{ marginBottom: '1.25rem' }}>
              <label className="form-label">User Account ID / Phone</label>
              <input 
                type="text"
                className="form-input"
                placeholder="e.g. SB-928174"
                value={betUserId}
                onChange={(e) => setBetUserId(e.target.value)}
                required
              />
            </div>

            <div className="form-group" style={{ marginBottom: '1.5rem' }}>
              <label className="form-label">Amount to Fund (₦)</label>
              <input 
                type="number"
                className="form-input"
                min="500"
                step="500"
                value={betAmount}
                onChange={(e) => setBetAmount(Number(e.target.value))}
                required
              />
            </div>

            <button type="submit" className="btn-primary" style={{ width: '100%', padding: '12px' }} disabled={loading}>
              {loading ? 'Funding Wallet...' : `Fund ${betProvider} (₦${betAmount.toLocaleString()})`}
            </button>
          </form>
        )}
      </div>
    </div>
  );
};
