import { 
  ShieldCheck, 
  LayoutDashboard, 
  ArrowLeftRight, 
  CreditCard, 
  BarChart3, 
  BookOpenCheck,
  CheckCircle2,
  Bell,
  ChevronDown
} from 'lucide-react';

interface NavbarProps {
  activeTab: 'overview' | 'transfers' | 'cards' | 'analytics' | 'ledger';
  setActiveTab: (tab: 'overview' | 'transfers' | 'cards' | 'analytics' | 'ledger') => void;
  apiConnected: boolean;
}

export const Navbar = ({ activeTab, setActiveTab, apiConnected }: NavbarProps) => {
  return (
    <header className="app-header">
      {/* Brand */}
      <div className="brand-wrapper">
        <div className="brand-logo-icon">
          <ShieldCheck size={24} strokeWidth={2.5} />
        </div>
        <div className="brand-title">
          <span>Vault<span style={{ color: 'var(--accent-emerald-light)' }}>Pay</span></span>
          <span className="brand-badge">Fintech</span>
        </div>
      </div>

      {/* Navigation Tabs */}
      <nav className="nav-links">
        <button 
          className={`nav-tab-btn ${activeTab === 'overview' ? 'active' : ''}`}
          onClick={() => setActiveTab('overview')}
        >
          <LayoutDashboard size={17} />
          <span>Overview</span>
        </button>

        <button 
          className={`nav-tab-btn ${activeTab === 'transfers' ? 'active' : ''}`}
          onClick={() => setActiveTab('transfers')}
        >
          <ArrowLeftRight size={17} />
          <span>Transfers</span>
        </button>

        <button 
          className={`nav-tab-btn ${activeTab === 'cards' ? 'active' : ''}`}
          onClick={() => setActiveTab('cards')}
        >
          <CreditCard size={17} />
          <span>Cards & Vaults</span>
        </button>

        <button 
          className={`nav-tab-btn ${activeTab === 'analytics' ? 'active' : ''}`}
          onClick={() => setActiveTab('analytics')}
        >
          <BarChart3 size={17} />
          <span>Analytics</span>
        </button>

        <button 
          className={`nav-tab-btn ${activeTab === 'ledger' ? 'active' : ''}`}
          onClick={() => setActiveTab('ledger')}
        >
          <BookOpenCheck size={17} />
          <span>Ledger & Audit</span>
        </button>
      </nav>

      {/* Right controls */}
      <div className="header-actions">
        {/* Backend Status indicator */}
        <div className="api-status-badge" title="ASP.NET Core API at http://localhost:5033">
          <span className="status-dot"></span>
          <span>{apiConnected ? 'API Live (5033)' : 'Local Host Ready'}</span>
        </div>

        <button 
          aria-label="Notifications" 
          style={{ 
            color: 'var(--text-secondary)', 
            padding: '8px', 
            borderRadius: '8px',
            background: 'var(--bg-surface)',
            border: '1px solid var(--border-subtle)',
            position: 'relative' 
          }}
        >
          <Bell size={18} />
          <span style={{
            position: 'absolute',
            top: '6px',
            right: '6px',
            width: '6px',
            height: '6px',
            borderRadius: '50%',
            background: 'var(--accent-emerald)'
          }} />
        </button>

        {/* User profile */}
        <div className="user-profile-btn">
          <div className="user-avatar">AM</div>
          <div className="user-info-text">
            <div className="user-name">Alex Morgan</div>
            <div className="user-tag" style={{ display: 'flex', alignItems: 'center', gap: '3px' }}>
              <CheckCircle2 size={11} color="var(--accent-emerald-light)" />
              <span>Tier 3 Verified</span>
            </div>
          </div>
          <ChevronDown size={14} color="var(--text-muted)" style={{ marginLeft: '4px' }} />
        </div>
      </div>
    </header>
  );
};
