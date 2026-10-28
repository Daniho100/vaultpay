import type { Account, Transaction, CashFlowPoint, VirtualCard, LedgerRecord } from './types';

export const INITIAL_ACCOUNTS: Account[] = [
  {
    id: 'acc-1',
    accountNumber: '1084920194',
    accountName: 'Primary Spending Account',
    currencyCode: 'NGN',
    balance: 3450250.00,
    status: 'Active',
    createdAt: '2026-01-15T09:30:00Z',
    accountType: 'checking'
  },
  {
    id: 'acc-2',
    accountNumber: '1073829104',
    accountName: 'High-Yield Vault Saver',
    currencyCode: 'NGN',
    balance: 1850000.00,
    status: 'Active',
    createdAt: '2026-02-01T14:20:00Z',
    accountType: 'vault_savings',
    interestRate: 8.5
  },
  {
    id: 'acc-3',
    accountNumber: '1092837411',
    accountName: 'Global Travel & SaaS Vault',
    currencyCode: 'USD',
    balance: 2450.00,
    status: 'Active',
    createdAt: '2026-03-10T11:00:00Z',
    accountType: 'global_usd'
  }
];

export const INITIAL_TRANSACTIONS: Transaction[] = [
  {
    id: 'tx-1',
    reference: 'VP-TX-20261025-9481',
    fromAccountId: 'acc-1',
    fromAccountNumber: '1084920194',
    fromAccountName: 'Primary Spending Account',
    toAccountId: 'ext-99',
    toAccountNumber: '1038291047',
    toAccountName: 'Chinonso Okeke',
    amount: 75000.00,
    currencyCode: 'NGN',
    type: 'Transfer',
    status: 'Completed',
    description: 'Contract payment for Q3 Web Design',
    createdAt: '2026-10-25T14:15:00Z',
    completedAt: '2026-10-25T14:15:02Z',
    isDebit: true,
    category: 'Transfer'
  },
  {
    id: 'tx-2',
    reference: 'VP-DEP-20261024-1102',
    toAccountId: 'acc-1',
    toAccountNumber: '1084920194',
    toAccountName: 'Primary Spending Account',
    amount: 500000.00,
    currencyCode: 'NGN',
    type: 'Deposit',
    status: 'Completed',
    description: 'Direct Bank Deposit from Providus Bank',
    createdAt: '2026-10-24T10:30:00Z',
    completedAt: '2026-10-24T10:30:05Z',
    isDebit: false,
    category: 'Salary'
  },
  {
    id: 'tx-3',
    reference: 'VP-TX-20261023-4482',
    fromAccountId: 'acc-3',
    fromAccountNumber: '1092837411',
    fromAccountName: 'Global Travel & SaaS Vault',
    toAccountName: 'GitHub Inc.',
    amount: 42.00,
    currencyCode: 'USD',
    type: 'Transfer',
    status: 'Completed',
    description: 'GitHub Copilot Enterprise Monthly',
    createdAt: '2026-10-23T08:45:00Z',
    completedAt: '2026-10-23T08:45:01Z',
    isDebit: true,
    category: 'Bill'
  },
  {
    id: 'tx-4',
    reference: 'VP-TX-20261022-8172',
    fromAccountId: 'acc-1',
    fromAccountNumber: '1084920194',
    fromAccountName: 'Primary Spending Account',
    toAccountNumber: '1047291844',
    toAccountName: 'Adewale Adeleke',
    amount: 120000.00,
    currencyCode: 'NGN',
    type: 'Transfer',
    status: 'Completed',
    description: 'Office Equipment Reimbursement',
    createdAt: '2026-10-22T16:20:00Z',
    completedAt: '2026-10-22T16:20:04Z',
    isDebit: true,
    category: 'Transfer'
  },
  {
    id: 'tx-5',
    reference: 'VP-DEP-20261020-0091',
    toAccountId: 'acc-2',
    toAccountNumber: '1073829104',
    toAccountName: 'High-Yield Vault Saver',
    amount: 250000.00,
    currencyCode: 'NGN',
    type: 'Deposit',
    status: 'Completed',
    description: 'Automatic Monthly Vault Savings Plan',
    createdAt: '2026-10-20T00:01:00Z',
    completedAt: '2026-10-20T00:01:03Z',
    isDebit: false,
    category: 'Investment'
  },
  {
    id: 'tx-6',
    reference: 'VP-TX-20261018-9122',
    fromAccountId: 'acc-1',
    fromAccountNumber: '1084920194',
    fromAccountName: 'Primary Spending Account',
    toAccountNumber: '1059381729',
    toAccountName: 'Eko Electricity Distribution',
    amount: 35000.00,
    currencyCode: 'NGN',
    type: 'Transfer',
    status: 'Completed',
    description: 'Monthly Utility Power Recharge',
    createdAt: '2026-10-18T12:00:00Z',
    completedAt: '2026-10-18T12:00:02Z',
    isDebit: true,
    category: 'Bill'
  }
];

export const INITIAL_CASH_FLOW: CashFlowPoint[] = [
  { date: 'Oct 01', inflow: 380000, outflow: 95000 },
  { date: 'Oct 05', inflow: 120000, outflow: 140000 },
  { date: 'Oct 10', inflow: 450000, outflow: 80000 },
  { date: 'Oct 15', inflow: 200000, outflow: 310000 },
  { date: 'Oct 20', inflow: 650000, outflow: 125000 },
  { date: 'Oct 24', inflow: 500000, outflow: 75000 },
  { date: 'Today',  inflow: 180000, outflow: 45000 }
];

export const INITIAL_CARDS: VirtualCard[] = [
  {
    id: 'card-1',
    cardNumber: '5399 •••• •••• 8842',
    cardHolder: 'ALEX MORGAN',
    expiry: '09/29',
    cvv: '619',
    isFrozen: false,
    dailyLimit: 1500000,
    spentToday: 117000,
    type: 'Vault Titanium'
  },
  {
    id: 'card-2',
    cardNumber: '4242 •••• •••• 1928',
    cardHolder: 'ALEX MORGAN',
    expiry: '12/28',
    cvv: '394',
    isFrozen: false,
    dailyLimit: 3000,
    spentToday: 42,
    type: 'Vault Virtual Emerald'
  }
];

export const INITIAL_LEDGER: LedgerRecord[] = [
  {
    id: 'leg-1',
    transactionId: 'tx-1',
    reference: 'VP-TX-20261025-9481',
    accountId: 'acc-1',
    accountName: 'Primary Spending Account',
    entryType: 'Debit',
    amount: 75000.00,
    currencyCode: 'NGN',
    balanceAfter: 3450250.00,
    timestamp: '2026-10-25T14:15:02Z'
  },
  {
    id: 'leg-2',
    transactionId: 'tx-2',
    reference: 'VP-DEP-20261024-1102',
    accountId: 'acc-1',
    accountName: 'Primary Spending Account',
    entryType: 'Credit',
    amount: 500000.00,
    currencyCode: 'NGN',
    balanceAfter: 3525250.00,
    timestamp: '2026-10-24T10:30:05Z'
  },
  {
    id: 'leg-3',
    transactionId: 'tx-3',
    reference: 'VP-TX-20261023-4482',
    accountId: 'acc-3',
    accountName: 'Global Travel & SaaS Vault',
    entryType: 'Debit',
    amount: 42.00,
    currencyCode: 'USD',
    balanceAfter: 2450.00,
    timestamp: '2026-10-23T08:45:01Z'
  }
];

// Helper functions for storage persistence
export function getStoredAccounts(): Account[] {
  try {
    const raw = localStorage.getItem('vaultpay_accounts');
    return raw ? JSON.parse(raw) : INITIAL_ACCOUNTS;
  } catch {
    return INITIAL_ACCOUNTS;
  }
}

export function saveAccounts(accounts: Account[]) {
  localStorage.setItem('vaultpay_accounts', JSON.stringify(accounts));
}

export function getStoredTransactions(): Transaction[] {
  try {
    const raw = localStorage.getItem('vaultpay_transactions');
    return raw ? JSON.parse(raw) : INITIAL_TRANSACTIONS;
  } catch {
    return INITIAL_TRANSACTIONS;
  }
}

export function saveTransactions(transactions: Transaction[]) {
  localStorage.setItem('vaultpay_transactions', JSON.stringify(transactions));
}
