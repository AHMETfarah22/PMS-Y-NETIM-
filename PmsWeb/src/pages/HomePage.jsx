import React from 'react';
import '../styles/HomePage.css';

// Simple icon components
const CalendarIcon = () => <svg width="28" height="28" viewBox="0 0 24 24" fill="none" stroke="white" strokeWidth="2"><rect x="3" y="4" width="18" height="18" rx="2" ry="2"></rect><line x1="16" y1="2" x2="16" y2="6"></line><line x1="8" y1="2" x2="8" y2="6"></line><line x1="3" y1="10" x2="21" y2="10"></line></svg>;
const UsersIcon = () => <svg width="28" height="28" viewBox="0 0 24 24" fill="none" stroke="white" strokeWidth="2"><path d="M17 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2"></path><circle cx="9" cy="7" r="4"></circle><path d="M23 21v-2a4 4 0 0 0-3-3.87"></path><path d="M16 3.13a4 4 0 0 1 0 7.75"></path></svg>;
const HomeIcon = () => <svg width="28" height="28" viewBox="0 0 24 24" fill="none" stroke="white" strokeWidth="2"><path d="M3 9l9-7 9 7v11a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2z"></path><polyline points="9 22 9 12 15 12 15 22"></polyline></svg>;
const TrendIcon = () => <svg width="28" height="28" viewBox="0 0 24 24" fill="none" stroke="white" strokeWidth="2"><polyline points="23 6 13.5 15.5 8.5 10.5 1 17"></polyline><polyline points="17 6 23 6 23 12"></polyline></svg>;

export default function HomePage() {
  const stats = [
    { label: 'Bu Ay Rezervasyon', value: '24', icon: CalendarIcon, color: '#667eea' },
    { label: 'Aktif Konuklar', value: '8', icon: UsersIcon, color: '#f093fb' },
    { label: 'Müsait Odalar', value: '12', icon: HomeIcon, color: '#22c55e' },
    { label: 'Gelir (Bu Ay)', value: '12,500 ₺', icon: TrendIcon, color: '#f5576c' },
  ];

  return (
    <div className="home-page">
      <div className="page-header">
        <h1>Hoş Geldiniz, Sistem Yöneticisi</h1>
        <p>Pansiyon Yönetim Sistemi - Günlük Özet</p>
      </div>

      {/* Stats Grid */}
      <div className="stats-grid">
        {stats.map((stat, idx) => {
          const Icon = stat.icon;
          return (
            <div key={idx} className="stat-card">
              <div className="stat-icon" style={{ background: stat.color }}>
                <Icon />
              </div>
              <div className="stat-content">
                <p className="stat-value">{stat.value}</p>
                <p className="stat-label">{stat.label}</p>
              </div>
            </div>
          );
        })}
      </div>

      {/* Quick Actions */}
      <div className="quick-actions">
        <h2>Hızlı İşlemler</h2>
        <div className="actions-grid">
          <button className="action-btn">
            <span className="btn-icon">➕</span>
            <span className="btn-text">Yeni Rezervasyon</span>
          </button>
          <button className="action-btn">
            <span className="btn-icon">👥</span>
            <span className="btn-text">Konuk Ekle</span>
          </button>
          <button className="action-btn">
            <span className="btn-icon">🏠</span>
            <span className="btn-text">Odaları Görüntüle</span>
          </button>
          <button className="action-btn">
            <span className="btn-icon">📊</span>
            <span className="btn-text">Rapor Oluştur</span>
          </button>
        </div>
      </div>

      {/* Recent Activity */}
      <div className="recent-activity">
        <h2>Son İşlemler</h2>
        <div className="activity-list">
          <div className="activity-item">
            <div className="activity-icon success">✓</div>
            <div className="activity-content">
              <p className="activity-title">Ahmed Farah tarafından Oda 101'e giriş yaptı</p>
              <p className="activity-time">2 saat önce</p>
            </div>
          </div>
          <div className="activity-item">
            <div className="activity-icon">💳</div>
            <div className="activity-content">
              <p className="activity-title">Kredi kartı ile ödeme alındı</p>
              <p className="activity-time">3 saat önce</p>
            </div>
          </div>
          <div className="activity-item">
            <div className="activity-icon">📋</div>
            <div className="activity-content">
              <p className="activity-title">Yeni rezervasyon oluşturuldu</p>
              <p className="activity-time">5 saat önce</p>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}
