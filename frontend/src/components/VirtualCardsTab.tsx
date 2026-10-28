import { useState } from 'react';
import { 
  Eye, 
  EyeOff, 
  Lock, 
  Unlock, 
  Wifi, 
  Plus
} from 'lucide-react';
import type { VirtualCard } from '../types';

interface VirtualCardsTabProps {
  cards: VirtualCard[];
  onToggleFreeze: (cardId: string) => void;
  onUpdateLimit: (cardId: string, newLimit: number) => void;
  showToast: (msg: string) => void;
}

export const VirtualCardsTab = ({
  cards,
  onToggleFreeze,
  onUpdateLimit,
  showToast
}: VirtualCardsTabProps) => {
  const [selectedCardId, setSelectedCardId] = useState(cards[0]?.id || '');
  const [showCvv, setShowCvv] = useState(false);

  const activeCard = cards.find(c => c.id === selectedCardId) || cards[0];

  return (
    <div style={{ display: 'flex', flexDirection: 'column', gap: '2rem' }}>
      <div className="section-header-row">
        <div>
          <h2 className="section-heading">Cards & Vault Access</h2>
          <p style={{ fontSize: '0.85rem', color: 'var(--text-muted)' }}>
            Manage physical and virtual multi-currency cards with instant security controls
          </p>
        </div>
        <div style={{ display: 'flex', gap: '0.75rem', alignItems: 'center' }}>
          <div className="filter-pills-row">
            {cards.map(c => (
              <button
                key={c.id}
                className={`filter-pill ${c.id === (activeCard?.id || cards[0]?.id) ? 'active' : ''}`}
                onClick={() => setSelectedCardId(c.id)}
              >
                {c.type}
              </button>
            ))}
          </div>
          <button
            onClick={() => showToast('New virtual Mastercard issued instantly to your vault!')}
            style={{
              display: 'flex',
              alignItems: 'center',
              gap: '6px',
              fontSize: '0.85rem',
              fontWeight: 600,
              color: 'var(--accent-emerald-light)',
              padding: '8px 14px',
              borderRadius: 'var(--radius-sm)',
              background: 'var(--accent-emerald-glow)',
              border: '1px solid rgba(16, 185, 129, 0.3)'
            }}
          >
            <Plus size={16} />
            <span>Issue New Card</span>
          </button>
        </div>
      </div>

      <div style={{ display: 'grid', gridTemplateColumns: 'minmax(320px, 440px) 1fr', gap: '2rem' }}>
        {/* Holographic 3D Card Display */}
        <div style={{ display: 'flex', flexDirection: 'column', gap: '1rem', alignItems: 'center' }}>
          <div 
            className="virtual-card-preview"
            style={{
              opacity: activeCard?.isFrozen ? 0.6 : 1,
              filter: activeCard?.isFrozen ? 'grayscale(0.8)' : 'none',
              transition: 'all 0.3s ease'
            }}
          >
            {/* Top row: Chip and Contactless */}
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
              <div style={{ display: 'flex', alignItems: 'center', gap: '1rem' }}>
                <div className="card-chip" />
                <Wifi size={20} color="rgba(255,255,255,0.7)" style={{ transform: 'rotate(90deg)' }} />
              </div>
              <div style={{ fontWeight: 800, fontSize: '1.1rem', letterSpacing: '0.05em', color: '#fff' }}>
                Vault<span style={{ color: 'var(--accent-emerald-light)' }}>Pay</span>
              </div>
            </div>

            {/* Card Number */}
            <div style={{ 
              fontFamily: 'var(--font-mono)', 
              fontSize: '1.25rem', 
              letterSpacing: '0.15em', 
              color: '#fff',
              textShadow: '0 2px 4px rgba(0,0,0,0.5)'
            }}>
              {showCvv ? activeCard?.cardNumber.replace(/•/g, '5') : activeCard?.cardNumber}
            </div>

            {/* Bottom Row: Holder, Expiry, CVV */}
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-end' }}>
              <div>
                <div style={{ fontSize: '0.65rem', textTransform: 'uppercase', color: 'rgba(255,255,255,0.6)' }}>
                  Cardholder
                </div>
                <div style={{ fontSize: '0.85rem', fontWeight: 700, color: '#fff' }}>
                  {activeCard?.cardHolder}
                </div>
              </div>

              <div>
                <div style={{ fontSize: '0.65rem', textTransform: 'uppercase', color: 'rgba(255,255,255,0.6)' }}>
                  Expires
                </div>
                <div style={{ fontSize: '0.85rem', fontWeight: 700, color: '#fff' }}>
                  {activeCard?.expiry}
                </div>
              </div>

              <div>
                <div style={{ fontSize: '0.65rem', textTransform: 'uppercase', color: 'rgba(255,255,255,0.6)' }}>
                  CVV
                </div>
                <div style={{ fontSize: '0.85rem', fontWeight: 700, color: 'var(--accent-emerald-light)' }}>
                  {showCvv ? activeCard?.cvv : '•••'}
                </div>
              </div>
            </div>

            {activeCard?.isFrozen && (
              <div style={{
                position: 'absolute',
                inset: 0,
                background: 'rgba(15, 23, 42, 0.7)',
                display: 'flex',
                flexDirection: 'column',
                alignItems: 'center',
                justifyContent: 'center',
                gap: '8px',
                color: '#fff'
              }}>
                <Lock size={32} color="var(--accent-rose)" />
                <span style={{ fontWeight: 700, letterSpacing: '0.05em' }}>CARD FROZEN</span>
              </div>
            )}
          </div>

          <div style={{ display: 'flex', gap: '0.75rem', width: '100%', maxWidth: '420px' }}>
            <button
              className="filter-pill"
              style={{ flex: 1, display: 'flex', justifyContent: 'center', alignItems: 'center', gap: '6px', padding: '10px' }}
              onClick={() => setShowCvv(!showCvv)}
            >
              {showCvv ? <EyeOff size={16} /> : <Eye size={16} />}
              <span>{showCvv ? 'Hide CVV' : 'Reveal CVV'}</span>
            </button>

            <button
              className="filter-pill"
              style={{ 
                flex: 1, 
                display: 'flex', 
                justifyContent: 'center', 
                alignItems: 'center', 
                gap: '6px', 
                padding: '10px',
                color: activeCard?.isFrozen ? 'var(--accent-emerald-light)' : 'var(--accent-rose)',
                borderColor: activeCard?.isFrozen ? 'rgba(16, 185, 129, 0.4)' : 'rgba(244, 63, 94, 0.4)'
              }}
              onClick={() => {
                onToggleFreeze(activeCard.id);
                showToast(activeCard.isFrozen ? 'Card has been unfrozen!' : 'Card successfully frozen!');
              }}
            >
              {activeCard?.isFrozen ? <Unlock size={16} /> : <Lock size={16} />}
              <span>{activeCard?.isFrozen ? 'Unfreeze' : 'Freeze Card'}</span>
            </button>
          </div>
        </div>

        {/* Card Controls and Limits Panel */}
        <div className="glass-panel" style={{ padding: '1.75rem', display: 'flex', flexDirection: 'column', gap: '1.5rem' }}>
          <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between' }}>
            <div>
              <h3 style={{ fontSize: '1.1rem', fontWeight: 700, color: '#fff' }}>Security & Spend Controls</h3>
              <p style={{ fontSize: '0.8rem', color: 'var(--text-muted)' }}>Configure limits, online transactions, and ATM allowances</p>
            </div>
            <span style={{
              fontSize: '0.75rem',
              fontWeight: 700,
              padding: '4px 10px',
              borderRadius: '999px',
              background: 'rgba(16, 185, 129, 0.1)',
              color: 'var(--accent-emerald-light)',
              border: '1px solid rgba(16, 185, 129, 0.3)'
            }}>
              3D Secure 2.0 Enabled
            </span>
          </div>

          {/* Daily Limit Slider */}
          <div className="form-group">
            <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: '4px' }}>
              <label className="form-label">Daily Spend Limit</label>
              <span style={{ fontWeight: 700, color: '#fff', fontSize: '0.9rem' }}>
                ₦{activeCard?.dailyLimit.toLocaleString()}
              </span>
            </div>
            <input
              type="range"
              min="100000"
              max="5000000"
              step="50000"
              value={activeCard?.dailyLimit}
              onChange={(e) => onUpdateLimit(activeCard.id, Number(e.target.value))}
              style={{ accentColor: 'var(--accent-emerald)', cursor: 'pointer' }}
            />
            <div style={{ display: 'flex', justifyContent: 'space-between', fontSize: '0.75rem', color: 'var(--text-muted)' }}>
              <span>Spent today: ₦{activeCard?.spentToday.toLocaleString()}</span>
              <span>Max: ₦5,000,000</span>
            </div>
          </div>

          {/* Toggles */}
          <div style={{ display: 'flex', flexDirection: 'column', gap: '1rem', paddingTop: '0.5rem' }}>
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
              <div>
                <div style={{ fontWeight: 600, fontSize: '0.9rem', color: '#fff' }}>Online / E-Commerce Payments</div>
                <div style={{ fontSize: '0.75rem', color: 'var(--text-muted)' }}>Allow purchases on Amazon, Netflix, Uber, etc.</div>
              </div>
              <input type="checkbox" defaultChecked style={{ accentColor: 'var(--accent-emerald)', width: '18px', height: '18px' }} />
            </div>

            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
              <div>
                <div style={{ fontWeight: 600, fontSize: '0.9rem', color: '#fff' }}>International POS & Cross-Border</div>
                <div style={{ fontSize: '0.75rem', color: 'var(--text-muted)' }}>Automatic conversion for foreign currencies</div>
              </div>
              <input type="checkbox" defaultChecked style={{ accentColor: 'var(--accent-emerald)', width: '18px', height: '18px' }} />
            </div>

            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
              <div>
                <div style={{ fontWeight: 600, fontSize: '0.9rem', color: '#fff' }}>Contactless / Tap-to-Pay</div>
                <div style={{ fontSize: '0.75rem', color: 'var(--text-muted)' }}>Apple Pay & Google Wallet integration</div>
              </div>
              <input type="checkbox" defaultChecked style={{ accentColor: 'var(--accent-emerald)', width: '18px', height: '18px' }} />
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};
