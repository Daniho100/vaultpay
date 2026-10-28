const API_BASE = 'http://localhost:5033/api';

function getToken(): string | null {
  return localStorage.getItem('vaultpay_token');
}

export function setAuthToken(token: string | null) {
  if (token) {
    localStorage.setItem('vaultpay_token', token);
  } else {
    localStorage.removeItem('vaultpay_token');
  }
}

async function request<T>(endpoint: string, options: RequestInit = {}): Promise<T> {
  const token = getToken();
  const headers: Record<string, string> = {
    'Content-Type': 'application/json',
    ...(options.headers as Record<string, string> || {}),
  };

  if (token) {
    headers['Authorization'] = `Bearer ${token}`;
  }

  const res = await fetch(`${API_BASE}${endpoint}`, {
    ...options,
    headers,
  });

  if (!res.ok) {
    let errorMsg = `Request failed (${res.status})`;
    try {
      const errorJson = await res.json();
      if (errorJson.error) errorMsg = errorJson.error;
      else if (errorJson.title) errorMsg = errorJson.title;
    } catch {
      // ignore
    }
    throw new Error(errorMsg);
  }

  return res.json();
}

export const api = {
  auth: {
    login: (email: string, password: string) =>
      request<any>('/auth/login', {
        method: 'POST',
        body: JSON.stringify({ email, password }),
      }),
    register: (data: { fullName: string; email: string; password: string; accountName?: string; role?: string; tier?: number }) =>
      request<any>('/auth/register', {
        method: 'POST',
        body: JSON.stringify(data),
      }),
    me: () => request<any>('/auth/me'),
  },

  accounts: {
    getAll: () => request<any[]>('/accounts'),
    lookup: (nuban: string) => request<any>(`/accounts/lookup/${encodeURIComponent(nuban)}`),
  },

  transfers: {
    transfer: (data: { sourceAccountId: string; recipientAccountNumber: string; recipientAccountName?: string; amount: number; description?: string }) =>
      request<any>('/transfers', {
        method: 'POST',
        body: JSON.stringify(data),
      }),
  },

  deposits: {
    deposit: (data: { accountId: string; amount: number; channel?: string; reference?: string; description?: string }) =>
      request<any>('/deposits', {
        method: 'POST',
        body: JSON.stringify(data),
      }),
  },

  bills: {
    pay: (data: { accountId: string; type: number; provider: string; customerIdentifier: string; amount: number; packageName?: string }) =>
      request<any>('/bills/pay', {
        method: 'POST',
        body: JSON.stringify(data),
      }),
  },

  savings: {
    getFixedDeposits: () => request<any[]>('/savings/fixed-deposits'),
    createFixedDeposit: (data: { accountId: string; amount: number; durationDays: number }) =>
      request<any>('/savings/fixed-deposits', {
        method: 'POST',
        body: JSON.stringify(data),
      }),
    liquidate: (id: string) =>
      request<any>(`/savings/fixed-deposits/${id}/liquidate`, {
        method: 'POST',
      }),
  },

  loans: {
    getLoans: () => request<any[]>('/loans'),
    apply: (data: { accountId: string; amount: number; purpose: string; durationMonths: number }) =>
      request<any>('/loans/apply', {
        method: 'POST',
        body: JSON.stringify(data),
      }),
  },

  admin: {
    getStats: () => request<any>('/admin/stats'),
    getUsers: (search?: string) => request<any>(`/admin/users?pageSize=50${search ? `&search=${encodeURIComponent(search)}` : ''}`),
    searchUserWithTransactions: (query: string) =>
      request<any>(`/admin/users/search?query=${encodeURIComponent(query)}`),
    banUser: (userId: string) =>
      request<any>(`/admin/users/${userId}/ban`, {
        method: 'POST',
      }),
    unbanUser: (userId: string) =>
      request<any>(`/admin/users/${userId}/unban`, {
        method: 'POST',
      }),
    editUser: (userId: string, data: { fullName: string; phoneNumber?: string }) =>
      request<any>(`/admin/users/${userId}`, {
        method: 'PUT',
        body: JSON.stringify(data),
      }),
    deleteUser: (userId: string) =>
      request<any>(`/admin/users/${userId}`, {
        method: 'DELETE',
      }),
    upgradeTier: (userId: string, tier: number) =>
      request<any>(`/admin/users/${userId}/tier`, {
        method: 'POST',
        body: JSON.stringify({ tier }),
      }),
    getLoans: () => request<any[]>('/admin/loans'),
    approveLoan: (loanId: string, adminNotes?: string) =>
      request<any>(`/admin/loans/${loanId}/approve`, {
        method: 'POST',
        body: JSON.stringify({ adminNotes }),
      }),
    rejectLoan: (loanId: string, adminNotes?: string) =>
      request<any>(`/admin/loans/${loanId}/reject`, {
        method: 'POST',
        body: JSON.stringify({ adminNotes }),
      }),
  },
};
