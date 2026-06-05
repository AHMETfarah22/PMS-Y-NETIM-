import React, { useState, useEffect } from 'react';
import '../styles/DailyReport.css';

// Simple icon components
const DownloadIcon = () => <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2"><path d="M21 15v4a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2v-4"></path><polyline points="7 10 12 15 17 10"></polyline><line x1="12" y1="15" x2="12" y2="3"></line></svg>;
const LockIcon = () => <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2"><rect x="3" y="11" width="18" height="11" rx="2" ry="2"></rect><path d="M7 11V7a5 5 0 0 1 10 0v4"></path></svg>;

export default function DailyReportPage() {
  const today = new Date().toLocaleDateString('tr-TR', {
    day: '2-digit',
    month: '2-digit',
    year: 'numeric',
    weekday: 'long'
  });

  const [reportData, setReportData] = useState({
    totalCash: 2500.00,
    openCash: 0.00,
    dailyWithdrawal: 0.00,
  });

  const [transactions, setTransactions] = useState([
    { id: 1, date: '31.05 10:56', type: 'GELİR', category: 'Konaklamq', description: 'Oda Ödemesi - Nakit Ödeme', amount: 1500.00, status: 'Nakit Ödeme (TRY - Final)' },
    { id: 2, date: '31.05 10:56', type: 'GELİR', category: 'Konaklamq', description: 'Oda Ödemesi - Nakit Ödeme', amount: 0.00, status: 'Nakit Ödeme (TRY - Final)' },
    { id: 3, date: '31.05 10:55', type: 'GELİR', category: 'Konaklamq', description: 'Oda Ödemesi - Nakit Ödeme', amount: 1000.00, status: 'Nakit Ödeme (TRY - Final)' },
  ]);

  const [pastReports, setPastReports] = useState([
    { id: 1, date: '31.05 2026', cash: 2500.00, creditCard: 0.00, expenses: 0.00, revenue: 1500.00, completedBy: 'Sistem Yöneticisi', createdAt: '2026-05-25 10:56' },
    { id: 2, date: '31.05.2026', cash: 0.00, creditCard: 0.00, expenses: 0.00, revenue: 1000.00, completedBy: 'Sistem Yöneticisi', createdAt: '2026-05-25 17:37' },
    { id: 3, date: '31.05.2055', cash: 1000.00, creditCard: 0.00, expenses: 0.00, revenue: 1000.00, completedBy: 'Sistem Yöneticisi', createdAt: '2026-05-25 18:54' },
  ]);

  return (
    <div className="daily-report-container">
      {/* Header */}
      <div className="report-header">
        <h1>GÜNSONU</h1>
        <span className="report-date">{today}</span>
      </div>

      {/* Action Buttons */}
      <div className="action-buttons">
        <button className="btn btn-secondary">
          <DownloadIcon />
          <span>KASA ARŞIV (PDF)</span>
        </button>
        <button className="btn btn-secondary">
          <DownloadIcon />
          <span>İŞLEM DETAYı (PDF)</span>
        </button>
        <button className="btn btn-danger">
          <LockIcon />
          <span>GÜNÜ KAPAT (Z-RAPORU)</span>
        </button>
      </div>

      {/* Summary Cards */}
      <div className="summary-cards">
        <div className="card">
          <div className="card-value positive">{reportData.totalCash.toLocaleString('tr-TR', { minimumFractionDigits: 2, maximumFractionDigits: 2 })} ₺</div>
          <div className="card-label">Açık Kasa</div>
          <div className="card-icon">📈</div>
        </div>
        <div className="card">
          <div className="card-value neutral">{reportData.openCash.toLocaleString('tr-TR', { minimumFractionDigits: 2, maximumFractionDigits: 2 })} ₺</div>
          <div className="card-label">Açık Kasa</div>
        </div>
        <div className="card">
          <div className="card-value negative">{reportData.dailyWithdrawal.toLocaleString('tr-TR', { minimumFractionDigits: 2, maximumFractionDigits: 2 })} ₺</div>
          <div className="card-label">Dün Devreden</div>
        </div>
      </div>

      {/* Today's Transactions */}
      <div className="section">
        <div className="section-header">
          <h2>📋 Bugünlü Hareketler</h2>
        </div>
        <div className="table-wrapper">
          <table className="transactions-table">
            <thead>
              <tr>
                <th>Tarih</th>
                <th>Tip</th>
                <th>Kategori</th>
                <th>Açıklama</th>
                <th>Tutar</th>
                <th>Yöntem</th>
              </tr>
            </thead>
            <tbody>
              {transactions.map((tx) => (
                <tr key={tx.id}>
                  <td>{tx.date}</td>
                  <td><span className={`badge badge-${tx.type.toLowerCase()}`}>{tx.type}</span></td>
                  <td>{tx.category}</td>
                  <td>{tx.description}</td>
                  <td className="amount">{tx.amount.toLocaleString('tr-TR', { minimumFractionDigits: 2 })}</td>
                  <td><span className="badge badge-status">{tx.status}</span></td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </div>

      {/* Past Reports */}
      <div className="section">
        <div className="section-header">
          <h2>📊 Geçmiş Gün Sonu Raporları</h2>
        </div>
        <div className="table-wrapper">
          <table className="reports-table">
            <thead>
              <tr>
                <th>ReportID</th>
                <th>ReportDate</th>
                <th>TotalCash</th>
                <th>TotalCreditCard</th>
                <th>TotalExpenses</th>
                <th>TotalRevenue</th>
                <th>CompletedBy</th>
                <th>CreatedAt</th>
              </tr>
            </thead>
            <tbody>
              {pastReports.map((report) => (
                <tr key={report.id}>
                  <td>{report.id}</td>
                  <td>{report.date}</td>
                  <td>{report.cash.toLocaleString('tr-TR', { minimumFractionDigits: 2 })}</td>
                  <td>{report.creditCard.toLocaleString('tr-TR', { minimumFractionDigits: 2 })} ₺</td>
                  <td>{report.expenses.toLocaleString('tr-TR', { minimumFractionDigits: 2 })} ₺</td>
                  <td>{report.revenue.toLocaleString('tr-TR', { minimumFractionDigits: 2 })}</td>
                  <td>{report.completedBy}</td>
                  <td>{report.createdAt}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </div>
    </div>
  );
}

export default function DailyReportPage() {
  const today = new Date().toLocaleDateString('tr-TR', {
    day: '2-digit',
    month: '2-digit',
    year: 'numeric',
    weekday: 'long'
  });

  const [reportData, setReportData] = useState({
    totalCash: 2500.00,
    openCash: 0.00,
    dailyWithdrawal: 0.00,
  });

  const [transactions, setTransactions] = useState([
    { id: 1, date: '31.05 10:56', type: 'GELİR', category: 'Konaklamq', description: 'Oda Ödemesi - Nakit Ödeme', amount: 1500.00, status: 'Nakit Ödeme (TRY - Final)' },
    { id: 2, date: '31.05 10:56', type: 'GELİR', category: 'Konaklamq', description: 'Oda Ödemesi - Nakit Ödeme', amount: 0.00, status: 'Nakit Ödeme (TRY - Final)' },
    { id: 3, date: '31.05 10:55', type: 'GELİR', category: 'Konaklamq', description: 'Oda Ödemesi - Nakit Ödeme', amount: 1000.00, status: 'Nakit Ödeme (TRY - Final)' },
  ]);

  const [pastReports, setPastReports] = useState([
    { id: 1, date: '31.05 2026', cash: 2500.00, creditCard: 0.00, expenses: 0.00, revenue: 1500.00, completedBy: 'Sistem Yöneticisi', createdAt: '2026-05-25 10:56' },
    { id: 2, date: '31.05.2026', cash: 0.00, creditCard: 0.00, expenses: 0.00, revenue: 1000.00, completedBy: 'Sistem Yöneticisi', createdAt: '2026-05-25 17:37' },
    { id: 3, date: '31.05.2055', cash: 1000.00, creditCard: 0.00, expenses: 0.00, revenue: 1000.00, completedBy: 'Sistem Yöneticisi', createdAt: '2026-05-25 18:54' },
  ]);

  return (
    <div className="daily-report-container">
      {/* Header */}
      <div className="report-header">
        <h1>GÜNSONU</h1>
        <span className="report-date">{today}</span>
      </div>

      {/* Action Buttons */}
      <div className="action-buttons">
        <button className="btn btn-secondary">
          <Download size={18} />
          <span>KASA ARŞIV (PDF)</span>
        </button>
        <button className="btn btn-secondary">
          <Download size={18} />
          <span>İŞLEM DETAYı (PDF)</span>
        </button>
        <button className="btn btn-danger">
          <Lock size={18} />
          <span>GÜNÜ KAPAT (Z-RAPORU)</span>
        </button>
      </div>

      {/* Summary Cards */}
      <div className="summary-cards">
        <div className="card">
          <div className="card-value positive">{reportData.totalCash.toLocaleString('tr-TR', { minimumFractionDigits: 2, maximumFractionDigits: 2 })} ₺</div>
          <div className="card-label">Açık Kasa</div>
          <div className="card-icon">📈</div>
        </div>
        <div className="card">
          <div className="card-value neutral">{reportData.openCash.toLocaleString('tr-TR', { minimumFractionDigits: 2, maximumFractionDigits: 2 })} ₺</div>
          <div className="card-label">Açık Kasa</div>
        </div>
        <div className="card">
          <div className="card-value negative">{reportData.dailyWithdrawal.toLocaleString('tr-TR', { minimumFractionDigits: 2, maximumFractionDigits: 2 })} ₺</div>
          <div className="card-label">Dün Devreden</div>
        </div>
      </div>

      {/* Today's Transactions */}
      <div className="section">
        <div className="section-header">
          <h2>📋 Bugünlü Hareketler</h2>
        </div>
        <div className="table-wrapper">
          <table className="transactions-table">
            <thead>
              <tr>
                <th>Tarih</th>
                <th>Tip</th>
                <th>Kategori</th>
                <th>Açıklama</th>
                <th>Tutar</th>
                <th>Yöntem</th>
              </tr>
            </thead>
            <tbody>
              {transactions.map((tx) => (
                <tr key={tx.id}>
                  <td>{tx.date}</td>
                  <td><span className={`badge badge-${tx.type.toLowerCase()}`}>{tx.type}</span></td>
                  <td>{tx.category}</td>
                  <td>{tx.description}</td>
                  <td className="amount">{tx.amount.toLocaleString('tr-TR', { minimumFractionDigits: 2 })}</td>
                  <td><span className="badge badge-status">{tx.status}</span></td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </div>

      {/* Past Reports */}
      <div className="section">
        <div className="section-header">
          <h2>📊 Geçmiş Gün Sonu Raporları</h2>
        </div>
        <div className="table-wrapper">
          <table className="reports-table">
            <thead>
              <tr>
                <th>ReportID</th>
                <th>ReportDate</th>
                <th>TotalCash</th>
                <th>TotalCreditCard</th>
                <th>TotalExpenses</th>
                <th>TotalRevenue</th>
                <th>CompletedBy</th>
                <th>CreatedAt</th>
              </tr>
            </thead>
            <tbody>
              {pastReports.map((report) => (
                <tr key={report.id}>
                  <td>{report.id}</td>
                  <td>{report.date}</td>
                  <td>{report.cash.toLocaleString('tr-TR', { minimumFractionDigits: 2 })}</td>
                  <td>{report.creditCard.toLocaleString('tr-TR', { minimumFractionDigits: 2 })} ₺</td>
                  <td>{report.expenses.toLocaleString('tr-TR', { minimumFractionDigits: 2 })} ₺</td>
                  <td>{report.revenue.toLocaleString('tr-TR', { minimumFractionDigits: 2 })}</td>
                  <td>{report.completedBy}</td>
                  <td>{report.createdAt}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </div>
    </div>
  );
}
