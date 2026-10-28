import { useState, useEffect } from 'react';
import { 
  getStoredAccounts, 
  saveAccounts, 
  getStoredTransactions, 
  saveTransactions,
  INITIAL_CARDS,
  INITIAL_CASH_FLOW,
  INITIAL_LEDGER
} from './mockData';
import type { Account, Transaction, VirtualCard, CashFlowPoint, LedgerRecord } from './types';
import { Navbar } from './components/Navbar';
import { HeroOverview } from './components/HeroOverview';
import { AccountsSection } from './components/AccountsSection';
import { CashFlowChart } from './components/CashFlowChart';
import { TransactionsSection } from './components/TransactionsSection';
import { TransferModal } from './components/TransferModal';
import { DepositModal } from './components/DepositModal';
import { NewAccountModal } from './components/NewAccountModal';
import { ReceiptModal } from './components/ReceiptModal';
import { VirtualCardsTab } from './components/VirtualCardsTab';
import { LedgerTab } from './components/LedgerTab';
import { CheckCircle2, ShieldCheck } from 'lucide-react';
import './App.css';

export function App() {
  const [activeTab, setActiveTab] = useState<'overview' | 'transfers' | 'cards' | 'analytics' | 'ledger'>('overview');
  const [accounts, setAccounts] = useState<Account[]>(getStoredAccounts);
  const [transactions, setTransactions] = useState<Transaction[]>(getStoredTransactions);
  const [cards, setCards] = useState<VirtualCard[]>(INITIAL_CARDS);
  const [cashFlow, setCashFlow] = useState<CashFlowPoint[]>(INITIAL_CASH_FLOW);
  const [ledgerEntries, setLedgerEntries] = useState<LedgerRecord[]>(INITIAL_LEDGER);

  // Modals state
  const [isTransferOpen, setIsTransferOpen] = useState(false);
  const [isDepositOpen, setIsDepositOpen] = useState(false);
  const [isNewAccountOpen, setIsNewAccountOpen] = useState(false);
  const [selectedSourceAccount, setSelectedSourceAccount] = useState<Account | undefined>(undefined);
  const [receiptTxn, setReceiptTxn] = useState<Transaction | null>(null);

  // Toast banner
  const [toastMessage, setToastMessage] = useState<string | null>(null);

  // Check backend API connection (http://localhost:5033)
  const [apiConnected, setApiConnected] = useState(false);

  useEffect(() => {
    fetch('http://localhost:5033/weatherforecast')
      .then(res => {
        if (res.ok) setApiConnected(true);
      })
      .catch(() => {
        // Localhost backend might have CORS or be starting up
        setApiConnected(false);
      });
  }, []);

  const showToast = (msg: string) => {
    setToastMessage(msg);
    setTimeout(() => {
      setToastMessage(null);
    }, 3500);
  };

  // Execute Transfer
  const handleExecuteTransfer = (
    sourceAccountId: string,
    recipientNuban: string,
    recipientName: string,
    amount: number,
    note: string
  ) => {
    const sourceAcc = accounts.find(a => a.id === sourceAccountId) || accounts[0];
    const newSourceBalance = sourceAcc.balance - amount;

    // Update Accounts
    const updatedAccounts = accounts.map(a => {
      if (a.id === sourceAccountId) {
        return { ...a, balance: newSourceBalance };
      }
      return a;
    });

    setAccounts(updatedAccounts);
    saveAccounts(updatedAccounts);

    const ref = `VP-TX-${new Date().toISOString().slice(0, 10).replace(/-/g, '')}-${Math.floor(1000 + Math.random() * 9000)}`;

    const newTx: Transaction = {
      id: `tx-${Date.now()}`,
      reference: ref,
      fromAccountId: sourceAcc.id,
      fromAccountNumber: sourceAcc.accountNumber,
      fromAccountName: sourceAcc.accountName,
      toAccountNumber: recipientNuban,
      toAccountName: recipientName,
      amount: amount,
      currencyCode: sourceAcc.currencyCode,
      type: 'Transfer',
      status: 'Completed',
      description: note,
      createdAt: new Date().toISOString(),
      completedAt: new Date().toISOString(),
      isDebit: true,
      category: 'Transfer'
    };

    const updatedTxns = [newTx, ...transactions];
    setTransactions(updatedTxns);
    saveTransactions(updatedTxns);

    // Add Ledger Entry (Debit)
    const newLedger: LedgerRecord = {
      id: `leg-${Date.now()}`,
      transactionId: newTx.id,
      reference: ref,
      accountId: sourceAcc.id,
      accountName: sourceAcc.accountName,
      entryType: 'Debit',
      amount: amount,
      currencyCode: sourceAcc.currencyCode,
      balanceAfter: newSourceBalance,
      timestamp: new Date().toISOString()
    };
    setLedgerEntries(prev => [newLedger, ...prev]);

    // Update cash flow point
    setCashFlow(prev => {
      const copy = [...prev];
      const last = copy[copy.length - 1];
      if (last) {
        last.outflow += amount;
      }
      return copy;
    });

    // Show receipt modal and toast
    setReceiptTxn(newTx);
    showToast(`Transferred ₦${amount.toLocaleString()} to ${recipientName}!`);
  };

  // Execute Deposit
  const handleExecuteDeposit = (accountId: string, amount: number, memo: string) => {
    const targetAcc = accounts.find(a => a.id === accountId) || accounts[0];
    const newBalance = targetAcc.balance + amount;

    const updatedAccounts = accounts.map(a => {
      if (a.id === accountId) {
        return { ...a, balance: newBalance };
      }
      return a;
    });

    setAccounts(updatedAccounts);
    saveAccounts(updatedAccounts);

    const ref = `VP-DEP-${new Date().toISOString().slice(0, 10).replace(/-/g, '')}-${Math.floor(1000 + Math.random() * 9000)}`;

    const newTx: Transaction = {
      id: `tx-${Date.now()}`,
      reference: ref,
      toAccountId: targetAcc.id,
      toAccountNumber: targetAcc.accountNumber,
      toAccountName: targetAcc.accountName,
      amount: amount,
      currencyCode: targetAcc.currencyCode,
      type: 'Deposit',
      status: 'Completed',
      description: memo,
      createdAt: new Date().toISOString(),
      completedAt: new Date().toISOString(),
      isDebit: false,
      category: 'Salary'
    };

    const updatedTxns = [newTx, ...transactions];
    setTransactions(updatedTxns);
    saveTransactions(updatedTxns);

    // Add Ledger Entry (Credit)
    const newLedger: LedgerRecord = {
      id: `leg-${Date.now()}`,
      transactionId: newTx.id,
      reference: ref,
      accountId: targetAcc.id,
      accountName: targetAcc.accountName,
      entryType: 'Credit',
      amount: amount,
      currencyCode: targetAcc.currencyCode,
      balanceAfter: newBalance,
      timestamp: new Date().toISOString()
    };
    setLedgerEntries(prev => [newLedger, ...prev]);

    // Update cash flow point
    setCashFlow(prev => {
      const copy = [...prev];
      const last = copy[copy.length - 1];
      if (last) {
        last.inflow += amount;
      }
      return copy;
    });

    showToast(`Successfully deposited ₦${amount.toLocaleString()} to ${targetAcc.accountName}!`);
  };

  // Create new account
  const handleCreateAccount = (newAcc: Account) => {
    const updated = [...accounts, newAcc];
    setAccounts(updated);
    saveAccounts(updated);
  };

  // Freeze/unfreeze card
  const handleToggleFreezeCard = (cardId: string) => {
    setCards(prev => prev.map(c => c.id === cardId ? { ...c, isFrozen: !c.isFrozen } : c));
  };

  // Update card limit
  const handleUpdateLimit = (cardId: string, newLimit: number) => {
    setCards(prev => prev.map(c => c.id === cardId ? { ...c, dailyLimit: newLimit } : c));
  };

  return (
    <div className="app-container">
      {/* Navigation Header */}
      <Navbar 
        activeTab={activeTab} 
        setActiveTab={setActiveTab} 
        apiConnected={apiConnected}
      />

      {/* Main Content */}
      <main className="app-main">
        {activeTab === 'overview' && (
          <>
            {/* Net Worth & Quick Actions */}
            <HeroOverview
              accounts={accounts}
              transactions={transactions}
              onOpenTransfer={() => {
                setSelectedSourceAccount(undefined);
                setIsTransferOpen(true);
              }}
              onOpenDeposit={() => setIsDepositOpen(true)}
              onOpenNewAccount={() => setIsNewAccountOpen(true)}
              onOpenStatement={() => showToast('Monthly statement compiled and sent to your email!')}
            />

            {/* Accounts Section */}
            <AccountsSection
              accounts={accounts}
              onOpenNewAccount={() => setIsNewAccountOpen(true)}
              onSelectAccountTransfer={(acc) => {
                setSelectedSourceAccount(acc);
                setIsTransferOpen(true);
              }}
              showToast={showToast}
            />

            {/* Analytics & Cash Flow Grid */}
            <div className="analytics-section-grid">
              <CashFlowChart data={cashFlow} />
              
              {/* Security & Health widget */}
              <div className="glass-panel" style={{ padding: '1.5rem', display: 'flex', flexDirection: 'column', justifyContent: 'space-between' }}>
                <div>
                  <div style={{ display: 'flex', alignItems: 'center', gap: '8px', marginBottom: '1rem' }}>
                    <ShieldCheck size={20} color="var(--accent-emerald-light)" />
                    <h3 style={{ fontSize: '1.05rem', fontWeight: 700, color: '#fff' }}>Vault Security Shield</h3>
                  </div>
                  <p style={{ fontSize: '0.85rem', color: 'var(--text-secondary)', marginBottom: '1.25rem' }}>
                    Your accounts and transactions are protected by end-to-end encryption, multi-tier biometric signing, and automated NUBAN reconciliation.
                  </p>
                  
                  <div style={{ display: 'flex', flexDirection: 'column', gap: '0.75rem', fontSize: '0.8rem' }}>
                    <div style={{ display: 'flex', justifyContent: 'space-between', color: 'var(--text-secondary)' }}>
                      <span>Double-Entry Ledger</span>
                      <strong style={{ color: 'var(--accent-emerald-light)' }}>Verified Active</strong>
                    </div>
                    <div style={{ display: 'flex', justifyContent: 'space-between', color: 'var(--text-secondary)' }}>
                      <span>Idempotency Protection</span>
                      <strong style={{ color: 'var(--accent-emerald-light)' }}>Strict Zero-Duplication</strong>
                    </div>
                    <div style={{ display: 'flex', justifyContent: 'space-between', color: 'var(--text-secondary)' }}>
                      <span>Session Encryption</span>
                      <strong style={{ color: '#fff' }}>TLS 1.3 / AES-256-GCM</strong>
                    </div>
                  </div>
                </div>

                <button 
                  className="filter-pill"
                  style={{ width: '100%', textAlign: 'center', marginTop: '1.25rem', padding: '10px' }}
                  onClick={() => setActiveTab('ledger')}
                >
                  View Ledger Audit Logs →
                </button>
              </div>
            </div>

            {/* Recent Transactions List */}
            <TransactionsSection
              transactions={transactions}
              onSelectTransaction={(tx) => setReceiptTxn(tx)}
            />
          </>
        )}

        {activeTab === 'transfers' && (
          <div style={{ display: 'flex', flexDirection: 'column', gap: '2rem' }}>
            <div className="section-header-row">
              <div>
                <h2 className="section-heading">Instant Transfers & Payouts</h2>
                <p style={{ fontSize: '0.85rem', color: 'var(--text-muted)' }}>
                  Send money instantly to any Nigerian bank or VaultPay wallet with 0% fee
                </p>
              </div>
              <button
                className="btn-primary"
                style={{ width: 'auto', padding: '10px 20px' }}
                onClick={() => setIsTransferOpen(true)}
              >
                + Initiate Transfer
              </button>
            </div>

            <TransactionsSection
              transactions={transactions.filter(t => t.type === 'Transfer')}
              onSelectTransaction={(tx) => setReceiptTxn(tx)}
            />
          </div>
        )}

        {activeTab === 'cards' && (
          <VirtualCardsTab
            cards={cards}
            onToggleFreeze={handleToggleFreezeCard}
            onUpdateLimit={handleUpdateLimit}
            showToast={showToast}
          />
        )}

        {activeTab === 'analytics' && (
          <div style={{ display: 'flex', flexDirection: 'column', gap: '2rem' }}>
            <div className="section-header-row">
              <div>
                <h2 className="section-heading">Financial Analytics & Cash Flow</h2>
                <p style={{ fontSize: '0.85rem', color: 'var(--text-muted)' }}>
                  Visual breakdown of monthly inflows, recurring expenditures, and savings growth
                </p>
              </div>
            </div>

            <CashFlowChart data={cashFlow} />

            <TransactionsSection
              transactions={transactions}
              onSelectTransaction={(tx) => setReceiptTxn(tx)}
            />
          </div>
        )}

        {activeTab === 'ledger' && (
          <LedgerTab ledgerEntries={ledgerEntries} />
        )}
      </main>

      {/* Modals */}
      <TransferModal
        isOpen={isTransferOpen}
        onClose={() => setIsTransferOpen(false)}
        accounts={accounts}
        selectedSourceAccount={selectedSourceAccount}
        onExecuteTransfer={handleExecuteTransfer}
      />

      <DepositModal
        isOpen={isDepositOpen}
        onClose={() => setIsDepositOpen(false)}
        accounts={accounts}
        onExecuteDeposit={handleExecuteDeposit}
        showToast={showToast}
      />

      <NewAccountModal
        isOpen={isNewAccountOpen}
        onClose={() => setIsNewAccountOpen(false)}
        onCreateAccount={handleCreateAccount}
        showToast={showToast}
      />

      <ReceiptModal
        transaction={receiptTxn}
        onClose={() => setReceiptTxn(null)}
        showToast={showToast}
      />

      {/* Toast Notification */}
      {toastMessage && (
        <div className="toast-banner">
          <CheckCircle2 size={18} color="var(--accent-emerald-light)" />
          <span>{toastMessage}</span>
        </div>
      )}
    </div>
  );
}

export default App;
