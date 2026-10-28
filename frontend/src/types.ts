export type AccountStatus = 'Active' | 'Suspended' | 'Closed';
export type TransactionType = 'Transfer' | 'Deposit' | 'BillPayment' | 'LoanDisbursement' | 'FixedDepositCreation' | 'FixedDepositLiquidation';
export type TransactionStatus = 'Completed' | 'Pending' | 'Failed';
export type LedgerEntryType = 'Debit' | 'Credit';
export type UserTier = 1 | 2 | 3;
export type UserRole = 'Admin' | 'Customer';
export type BillType = 'Airtime' | 'Data' | 'Electricity' | 'Betting';

export interface AuthUser {
  id: string;
  email: string;
  fullName: string;
  phoneNumber?: string;
  isActive: boolean;
  tier: UserTier;
  roles: string[];
  token?: string;
}

export interface Account {
  id: string;
  accountNumber: string;
  accountName: string;
  currencyCode: 'NGN' | 'USD' | 'EUR' | 'GBP';
  balance: number;
  status: AccountStatus;
  createdAt: string;
  accountType?: 'checking' | 'vault_savings' | 'global_usd';
  interestRate?: number;
  ownerEmail?: string;
  ownerFullName?: string;
}

export interface Transaction {
  id: string;
  reference: string;
  fromAccountId?: string;
  fromAccountNumber?: string;
  fromAccountName?: string;
  toAccountId?: string;
  toAccountNumber?: string;
  toAccountName?: string;
  amount: number;
  currencyCode: string;
  type: TransactionType | string;
  status: TransactionStatus;
  description: string;
  createdAt: string;
  completedAt?: string;
  isDebit: boolean;
  category?: 'Transfer' | 'Deposit' | 'Bill' | 'Shopping' | 'Salary' | 'Investment' | 'Loan' | 'Savings';
}

export interface CashFlowPoint {
  date: string;
  inflow: number;
  outflow: number;
}

export interface VirtualCard {
  id: string;
  cardNumber: string;
  cardHolder: string;
  expiry: string;
  cvv: string;
  isFrozen: boolean;
  dailyLimit: number;
  spentToday: number;
  type: 'Vault Titanium' | 'Vault Virtual Emerald';
}

export interface LedgerRecord {
  id: string;
  transactionId: string;
  reference: string;
  accountId: string;
  accountName: string;
  entryType: LedgerEntryType;
  amount: number;
  currencyCode: string;
  balanceAfter: number;
  timestamp: string;
}

export interface Loan {
  id: string;
  userId: string;
  userFullName: string;
  userEmail: string;
  accountId: string;
  accountNumber: string;
  amount: number;
  purpose: string;
  durationMonths: number;
  interestRate: number;
  status: 'Pending' | 'Approved' | 'Rejected' | 'Repaid';
  adminNotes?: string;
  createdAt: string;
  approvedAt?: string;
  repaidAt?: string;
}

export interface FixedDeposit {
  id: string;
  userId: string;
  accountId: string;
  accountNumber: string;
  amount: number;
  interestRatePerAnnum: number; // 0.25 (25% per annum)
  durationDays: number;
  startDate: string;
  maturityDate: string;
  status: 'Active' | 'LiquidatedEarly' | 'Matured';
  penaltyRate: number; // 0.10 (10% penalty for early withdrawal)
  projectedInterest: number;
  actualPayout?: number;
  closedAt?: string;
}

export interface AdminUser {
  id: string;
  email: string;
  fullName: string;
  phoneNumber?: string;
  isActive: boolean;
  tier: UserTier;
  createdAt: string;
  accountsCount: number;
  totalBalance: number;
  roles: string[];
}

export interface AdminUserDetails {
  id: string;
  email: string;
  fullName: string;
  phoneNumber?: string;
  isActive: boolean;
  tier: UserTier;
  createdAt: string;
  accounts: Account[];
  transactions: Transaction[];
}
