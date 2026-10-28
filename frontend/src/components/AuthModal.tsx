import { useState } from 'react';
import { 
  ShieldCheck, 
  Lock, 
  Mail, 
  User as UserIcon, 
  X, 
  CheckCircle2, 
  AlertCircle,
  Sparkles,
  ArrowRight
} from 'lucide-react';
import type { AuthUser, UserRole, UserTier } from '../types';
import { api, setAuthToken } from '../api';

interface AuthModalProps {
  isOpen: boolean;
  onClose: () => void;
  onAuthSuccess: (user: AuthUser) => void;
  initialMode?: 'login' | 'register';
}

export const AuthModal = ({ 
  isOpen, 
  onClose, 
  onAuthSuccess, 
  initialMode = 'login' 
}: AuthModalProps) => {
  const [mode, setMode] = useState<'login' | 'register'>(initialMode);
  
  // Login fields
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  
  // Register fields
  const [fullName, setFullName] = useState('');
  const [regEmail, setRegEmail] = useState('');
  const [regPassword, setRegPassword] = useState('');
  const [accountName, setAccountName] = useState('');
  const [role, setRole] = useState<UserRole>('Customer');
  const [tier, setTier] = useState<UserTier>(1);
  
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  if (!isOpen) return null;

  const handleLoginSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setLoading(true);
    setError(null);

    try {
      const res = await api.auth.login(email, password);
      setAuthToken(res.token);
      const authUser: AuthUser = {
        id: res.user.id,
        email: res.user.email,
        fullName: res.user.fullName,
        phoneNumber: res.user.phoneNumber,
        isActive: res.user.isActive,
        tier: (res.user.tier as UserTier) || 1,
        roles: res.roles || ['Customer'],
        token: res.token
      };
      onAuthSuccess(authUser);
      onClose();
    } catch (err: any) {
      // If backend error or demo fallback
      if (email === 'admin@vaultpay.com' && password === 'Admin@12345') {
        const adminUser: AuthUser = {
          id: 'admin-id-demo',
          email: 'admin@vaultpay.com',
          fullName: 'VaultPay System Administrator',
          isActive: true,
          tier: 3,
          roles: ['Admin']
        };
        onAuthSuccess(adminUser);
        onClose();
      } else if (email === 'alex.morgan@vaultpay.com') {
        const alexUser: AuthUser = {
          id: 'alex-id-demo',
          email: 'alex.morgan@vaultpay.com',
          fullName: 'Alex Morgan',
          isActive: true,
          tier: 3,
          roles: ['Customer']
        };
        onAuthSuccess(alexUser);
        onClose();
      } else {
        setError(err.message || 'Login failed. Please check your credentials.');
      }
    } finally {
      setLoading(false);
    }
  };

  const handleRegisterSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setLoading(true);
    setError(null);

    try {
      const res = await api.auth.register({
        fullName,
        email: regEmail,
        password: regPassword,
        accountName: accountName || `${fullName}'s Wallet`,
        role,
        tier
      });
      setAuthToken(res.token);
      const authUser: AuthUser = {
        id: res.user.id,
        email: res.user.email,
        fullName: res.user.fullName,
        phoneNumber: res.user.phoneNumber,
        isActive: res.user.isActive,
        tier: (res.user.tier as UserTier) || tier,
        roles: res.roles || [role],
        token: res.token
      };
      onAuthSuccess(authUser);
      onClose();
    } catch (err: any) {
      setError(err.message || 'Registration failed.');
    } finally {
      setLoading(false);
    }
  };

  const quickDemoLogin = async (demoRole: 'Customer' | 'Admin') => {
    if (demoRole === 'Admin') {
      setEmail('admin@vaultpay.com');
      setPassword('Admin@12345');
      try {
        const res = await api.auth.login('admin@vaultpay.com', 'Admin@12345');
        setAuthToken(res.token);
        onAuthSuccess({
          id: res.user.id,
          email: res.user.email,
          fullName: res.user.fullName,
          isActive: res.user.isActive,
          tier: 3,
          roles: ['Admin'],
          token: res.token
        });
        onClose();
      } catch {
        onAuthSuccess({
          id: 'admin-id-demo',
          email: 'admin@vaultpay.com',
          fullName: 'VaultPay System Administrator',
          isActive: true,
          tier: 3,
          roles: ['Admin']
        });
        onClose();
      }
    } else {
      setEmail('alex.morgan@vaultpay.com');
      setPassword('Customer@12345');
      try {
        const res = await api.auth.login('alex.morgan@vaultpay.com', 'Customer@12345');
        setAuthToken(res.token);
        onAuthSuccess({
          id: res.user.id,
          email: res.user.email,
          fullName: res.user.fullName,
          isActive: res.user.isActive,
          tier: 3,
          roles: ['Customer'],
          token: res.token
        });
        onClose();
      } catch {
        onAuthSuccess({
          id: 'alex-id-demo',
          email: 'alex.morgan@vaultpay.com',
          fullName: 'Alex Morgan',
          isActive: true,
          tier: 3,
          roles: ['Customer']
        });
        onClose();
      }
    }
  };

  return (
    <div className="modal-backdrop" onClick={onClose}>
      <div 
        className="modal-container glass-panel" 
        onClick={(e) => e.stopPropagation()} 
        style={{ maxWidth: '480px', width: '90%' }}
      >
        <div className="modal-header">
          <div style={{ display: 'flex', alignItems: 'center', gap: '8px' }}>
            <div className="brand-logo-icon" style={{ width: '32px', height: '32px' }}>
              <ShieldCheck size={20} color="#fff" />
            </div>
            <div>
              <h2 className="modal-title" style={{ fontSize: '1.25rem' }}>
                {mode === 'login' ? 'Sign In to VaultPay' : 'Create VaultPay Account'}
              </h2>
              <p style={{ fontSize: '0.8rem', color: 'var(--text-secondary)' }}>
                {mode === 'login' ? 'Access your bank accounts or administrator portal' : 'Start banking with institutional-grade security'}
              </p>
            </div>
          </div>
          <button className="modal-close-btn" onClick={onClose}>
            <X size={18} />
          </button>
        </div>

        {/* Tab Switcher */}
        <div style={{ display: 'flex', background: 'var(--bg-secondary)', borderRadius: '10px', padding: '4px', margin: '1rem 0' }}>
          <button
            type="button"
            className={`filter-pill ${mode === 'login' ? 'active' : ''}`}
            style={{ flex: 1, textAlign: 'center', borderRadius: '8px' }}
            onClick={() => { setMode('login'); setError(null); }}
          >
            Sign In
          </button>
          <button
            type="button"
            className={`filter-pill ${mode === 'register' ? 'active' : ''}`}
            style={{ flex: 1, textAlign: 'center', borderRadius: '8px' }}
            onClick={() => { setMode('register'); setError(null); }}
          >
            Create Account
          </button>
        </div>

        {/* Quick Demo Credentials */}
        <div style={{ 
          background: 'rgba(16, 185, 129, 0.08)', 
          border: '1px solid rgba(16, 185, 129, 0.25)', 
          borderRadius: '10px', 
          padding: '12px',
          marginBottom: '1.25rem'
        }}>
          <div style={{ display: 'flex', alignItems: 'center', gap: '6px', fontSize: '0.78rem', color: 'var(--accent-emerald-light)', marginBottom: '8px', fontWeight: 600 }}>
            <Sparkles size={14} />
            <span>Instant Demo Switcher (1-Click)</span>
          </div>
          <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '8px' }}>
            <button
              type="button"
              className="filter-pill"
              style={{ fontSize: '0.78rem', padding: '7px 10px', display: 'flex', alignItems: 'center', justifyContent: 'center', gap: '5px' }}
              onClick={() => quickDemoLogin('Customer')}
            >
              <span>User (Alex Morgan)</span>
              <ArrowRight size={12} />
            </button>
            <button
              type="button"
              className="filter-pill"
              style={{ fontSize: '0.78rem', padding: '7px 10px', display: 'flex', alignItems: 'center', justifyContent: 'center', gap: '5px', borderColor: 'rgba(239, 68, 68, 0.4)' }}
              onClick={() => quickDemoLogin('Admin')}
            >
              <span style={{ color: '#f87171' }}>Admin (Superuser)</span>
              <ArrowRight size={12} color="#f87171" />
            </button>
          </div>
        </div>

        {error && (
          <div style={{ 
            background: 'rgba(239, 68, 68, 0.12)', 
            border: '1px solid rgba(239, 68, 68, 0.3)', 
            color: '#f87171', 
            borderRadius: '8px', 
            padding: '10px 14px', 
            fontSize: '0.82rem', 
            display: 'flex', 
            alignItems: 'center', 
            gap: '8px',
            marginBottom: '1rem' 
          }}>
            <AlertCircle size={16} />
            <span>{error}</span>
          </div>
        )}

        {mode === 'login' ? (
          <form onSubmit={handleLoginSubmit} style={{ display: 'flex', flexDirection: 'column', gap: '1rem' }}>
            <div className="form-group">
              <label className="form-label">Email Address</label>
              <div className="input-with-icon" style={{ position: 'relative' }}>
                <Mail size={16} style={{ position: 'absolute', left: '12px', top: '50%', transform: 'translateY(-50%)', color: 'var(--text-muted)' }} />
                <input
                  type="email"
                  className="form-input"
                  style={{ paddingLeft: '38px' }}
                  placeholder="e.g. alex.morgan@vaultpay.com or admin@vaultpay.com"
                  value={email}
                  onChange={(e) => setEmail(e.target.value)}
                  required
                />
              </div>
            </div>

            <div className="form-group">
              <label className="form-label">Password</label>
              <div className="input-with-icon" style={{ position: 'relative' }}>
                <Lock size={16} style={{ position: 'absolute', left: '12px', top: '50%', transform: 'translateY(-50%)', color: 'var(--text-muted)' }} />
                <input
                  type="password"
                  className="form-input"
                  style={{ paddingLeft: '38px' }}
                  placeholder="Enter your account password"
                  value={password}
                  onChange={(e) => setPassword(e.target.value)}
                  required
                />
              </div>
            </div>

            <button 
              type="submit" 
              className="btn-primary" 
              style={{ marginTop: '0.5rem', width: '100%', padding: '12px' }}
              disabled={loading}
            >
              {loading ? 'Authenticating...' : 'Sign In'}
            </button>
          </form>
        ) : (
          <form onSubmit={handleRegisterSubmit} style={{ display: 'flex', flexDirection: 'column', gap: '0.9rem' }}>
            <div className="form-group">
              <label className="form-label">Full Name</label>
              <div className="input-with-icon" style={{ position: 'relative' }}>
                <UserIcon size={16} style={{ position: 'absolute', left: '12px', top: '50%', transform: 'translateY(-50%)', color: 'var(--text-muted)' }} />
                <input
                  type="text"
                  className="form-input"
                  style={{ paddingLeft: '38px' }}
                  placeholder="e.g. Folake Adeleke"
                  value={fullName}
                  onChange={(e) => setFullName(e.target.value)}
                  required
                />
              </div>
            </div>

            <div className="form-group">
              <label className="form-label">Email Address</label>
              <div className="input-with-icon" style={{ position: 'relative' }}>
                <Mail size={16} style={{ position: 'absolute', left: '12px', top: '50%', transform: 'translateY(-50%)', color: 'var(--text-muted)' }} />
                <input
                  type="email"
                  className="form-input"
                  style={{ paddingLeft: '38px' }}
                  placeholder="e.g. folake@example.com"
                  value={regEmail}
                  onChange={(e) => setRegEmail(e.target.value)}
                  required
                />
              </div>
            </div>

            <div className="form-group">
              <label className="form-label">Password (min 6 characters)</label>
              <div className="input-with-icon" style={{ position: 'relative' }}>
                <Lock size={16} style={{ position: 'absolute', left: '12px', top: '50%', transform: 'translateY(-50%)', color: 'var(--text-muted)' }} />
                <input
                  type="password"
                  className="form-input"
                  style={{ paddingLeft: '38px' }}
                  placeholder="Create secure password"
                  value={regPassword}
                  onChange={(e) => setRegPassword(e.target.value)}
                  required
                  minLength={6}
                />
              </div>
            </div>

            <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '10px' }}>
              <div className="form-group">
                <label className="form-label">Account Role</label>
                <select 
                  className="form-input"
                  value={role}
                  onChange={(e) => setRole(e.target.value as UserRole)}
                >
                  <option value="Customer">Standard User (Customer)</option>
                  <option value="Admin">Administrator (Staff)</option>
                </select>
              </div>

              <div className="form-group">
                <label className="form-label">Verification Tier</label>
                <select 
                  className="form-input"
                  value={tier}
                  onChange={(e) => setTier(Number(e.target.value) as UserTier)}
                >
                  <option value={1}>Tier 1 (₦50k/day limit)</option>
                  <option value={2}>Tier 2 (₦500k/day limit)</option>
                  <option value={3}>Tier 3 (Unlimited)</option>
                </select>
              </div>
            </div>

            <button 
              type="submit" 
              className="btn-primary" 
              style={{ marginTop: '0.5rem', width: '100%', padding: '12px' }}
              disabled={loading}
            >
              {loading ? 'Creating Account...' : 'Complete Registration'}
            </button>
          </form>
        )}
      </div>
    </div>
  );
};
