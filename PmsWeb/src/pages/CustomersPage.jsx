import React, { useState, useEffect } from 'react';
import { getCustomers, getCustomerDetails, updateCustomer, addCustomerMessage } from '../api';
import '../styles/Layout.css'; // You can add custom styles or reuse existing

export default function CustomersPage() {
  const [customers, setCustomers] = useState([]);
  const [loading, setLoading] = useState(true);
  const [selectedCustomer, setSelectedCustomer] = useState(null);
  const [detailsLoading, setDetailsLoading] = useState(false);
  const [messageText, setMessageText] = useState('');

  // Edit form state
  const [editMode, setEditMode] = useState(false);
  const [editForm, setEditForm] = useState({});

  useEffect(() => {
    fetchCustomers();
  }, []);

  const fetchCustomers = async () => {
    setLoading(true);
    try {
      const res = await getCustomers();
      setCustomers(res.data);
    } catch (err) {
      console.error(err);
      alert('Müşteriler yüklenirken hata oluştu.');
    } finally {
      setLoading(false);
    }
  };

  const handleSelectCustomer = async (id) => {
    setDetailsLoading(true);
    setEditMode(false);
    try {
      const res = await getCustomerDetails(id);
      setSelectedCustomer(res.data);
      setEditForm(res.data);
    } catch (err) {
      console.error(err);
      alert('Müşteri detayları alınamadı.');
    } finally {
      setDetailsLoading(false);
    }
  };

  const handleUpdate = async () => {
    try {
      await updateCustomer(selectedCustomer.customerID, editForm);
      alert('Müşteri bilgileri güncellendi.');
      setEditMode(false);
      handleSelectCustomer(selectedCustomer.customerID); // Refresh
      fetchCustomers(); // Refresh list
    } catch (err) {
      console.error(err);
      alert('Güncelleme başarısız.');
    }
  };

  const handleSendMessage = async () => {
    if (!messageText.trim()) return;
    try {
      await addCustomerMessage(selectedCustomer.customerID, {
        messageText,
        direction: 'Outgoing' // Admin to Customer
      });
      setMessageText('');
      handleSelectCustomer(selectedCustomer.customerID); // Refresh
    } catch (err) {
      console.error(err);
      alert('Mesaj gönderilemedi.');
    }
  };

  return (
    <div style={{ display: 'flex', height: '100%', gap: '20px', padding: '20px' }}>
      {/* List Section */}
      <div style={{ flex: 1, backgroundColor: '#fff', borderRadius: '8px', padding: '15px', boxShadow: '0 2px 4px rgba(0,0,0,0.1)', overflowY: 'auto' }}>
        <h2 style={{ marginBottom: '15px' }}>Müşteri Listesi</h2>
        {loading ? (
          <p>Yükleniyor...</p>
        ) : (
          <ul style={{ listStyle: 'none', padding: 0 }}>
            {customers.map(c => (
              <li 
                key={c.customerID} 
                onClick={() => handleSelectCustomer(c.customerID)}
                style={{ 
                  padding: '10px', 
                  borderBottom: '1px solid #eee', 
                  cursor: 'pointer',
                  backgroundColor: selectedCustomer?.customerID === c.customerID ? '#f0f7ff' : 'transparent'
                }}
              >
                <strong>{c.firstName} {c.lastName}</strong>
                <div style={{ fontSize: '0.85em', color: '#666' }}>{c.phone || 'Telefon Yok'} | {c.email || 'E-Posta Yok'}</div>
              </li>
            ))}
          </ul>
        )}
      </div>

      {/* Details Section */}
      <div style={{ flex: 2, display: 'flex', flexDirection: 'column', gap: '20px', overflowY: 'auto' }}>
        {detailsLoading ? (
          <p>Detaylar yükleniyor...</p>
        ) : selectedCustomer ? (
          <>
            {/* Info Card */}
            <div style={{ backgroundColor: '#fff', borderRadius: '8px', padding: '20px', boxShadow: '0 2px 4px rgba(0,0,0,0.1)' }}>
              <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: '15px' }}>
                <h2>Müşteri Bilgileri</h2>
                <button onClick={() => setEditMode(!editMode)} style={{ padding: '5px 15px', cursor: 'pointer' }}>
                  {editMode ? 'İptal' : 'Düzenle'}
                </button>
              </div>

              {editMode ? (
                <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '15px' }}>
                  <div><label>Ad:</label><input value={editForm.firstName} onChange={e => setEditForm({...editForm, firstName: e.target.value})} style={{width: '100%', padding: '5px'}}/></div>
                  <div><label>Soyad:</label><input value={editForm.lastName} onChange={e => setEditForm({...editForm, lastName: e.target.value})} style={{width: '100%', padding: '5px'}}/></div>
                  <div><label>Telefon:</label><input value={editForm.phone || ''} onChange={e => setEditForm({...editForm, phone: e.target.value})} style={{width: '100%', padding: '5px'}}/></div>
                  <div><label>E-Posta:</label><input value={editForm.email || ''} onChange={e => setEditForm({...editForm, email: e.target.value})} style={{width: '100%', padding: '5px'}}/></div>
                  <div style={{ gridColumn: 'span 2' }}><label>Adres:</label><input value={editForm.address || ''} onChange={e => setEditForm({...editForm, address: e.target.value})} style={{width: '100%', padding: '5px'}}/></div>
                  <div style={{ gridColumn: 'span 2' }}><label>Notlar:</label><textarea value={editForm.notes || ''} onChange={e => setEditForm({...editForm, notes: e.target.value})} style={{width: '100%', padding: '5px', minHeight: '60px'}}/></div>
                  <button onClick={handleUpdate} style={{ gridColumn: 'span 2', padding: '10px', backgroundColor: '#4CAF50', color: 'white', border: 'none', borderRadius: '4px', cursor: 'pointer' }}>Kaydet</button>
                </div>
              ) : (
                <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '15px' }}>
                  <div><strong>Ad Soyad:</strong> {selectedCustomer.firstName} {selectedCustomer.lastName}</div>
                  <div><strong>Telefon:</strong> {selectedCustomer.phone || <span style={{color: 'red'}}>Eksik</span>}</div>
                  <div><strong>E-Posta:</strong> {selectedCustomer.email || '-'}</div>
                  <div><strong>TC/Pasaport:</strong> {selectedCustomer.identityNumber || '-'}</div>
                  <div style={{ gridColumn: 'span 2' }}><strong>Adres:</strong> {selectedCustomer.address || '-'}</div>
                  <div style={{ gridColumn: 'span 2' }}><strong>Notlar:</strong> {selectedCustomer.notes || '-'}</div>
                </div>
              )}
            </div>

            {/* Reservations Card */}
            <div style={{ backgroundColor: '#fff', borderRadius: '8px', padding: '20px', boxShadow: '0 2px 4px rgba(0,0,0,0.1)' }}>
              <h2>Rezervasyon Geçmişi</h2>
              {selectedCustomer.reservations?.length > 0 ? (
                <table style={{ width: '100%', textAlign: 'left', marginTop: '10px', borderCollapse: 'collapse' }}>
                  <thead>
                    <tr style={{ borderBottom: '2px solid #eee' }}>
                      <th style={{ padding: '8px 0' }}>Oda</th>
                      <th>Giriş</th>
                      <th>Çıkış</th>
                      <th>Tutar</th>
                      <th>Durum</th>
                    </tr>
                  </thead>
                  <tbody>
                    {selectedCustomer.reservations.map(r => (
                      <tr key={r.reservationID} style={{ borderBottom: '1px solid #eee' }}>
                        <td style={{ padding: '8px 0' }}>{r.roomNumber}</td>
                        <td>{new Date(r.checkInDate).toLocaleDateString()}</td>
                        <td>{new Date(r.checkOutDate).toLocaleDateString()}</td>
                        <td>{r.totalAmount} ₺</td>
                        <td>{r.status}</td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              ) : <p>Rezervasyon bulunamadı.</p>}
            </div>

            {/* Messages/Correspondence Card */}
            <div style={{ backgroundColor: '#fff', borderRadius: '8px', padding: '20px', boxShadow: '0 2px 4px rgba(0,0,0,0.1)', flex: 1, display: 'flex', flexDirection: 'column' }}>
              <h2>Yazışmalar</h2>
              <div style={{ flex: 1, overflowY: 'auto', border: '1px solid #eee', padding: '10px', margin: '15px 0', borderRadius: '4px', backgroundColor: '#f9f9f9', minHeight: '200px' }}>
                {selectedCustomer.messages?.length > 0 ? selectedCustomer.messages.map(m => (
                  <div key={m.messageID} style={{ 
                    marginBottom: '10px', 
                    padding: '10px', 
                    borderRadius: '8px', 
                    maxWidth: '80%',
                    backgroundColor: m.direction === 'Incoming' ? '#fff' : '#dcf8c6',
                    alignSelf: m.direction === 'Incoming' ? 'flex-start' : 'flex-end',
                    marginLeft: m.direction === 'Incoming' ? '0' : 'auto',
                    boxShadow: '0 1px 2px rgba(0,0,0,0.1)'
                  }}>
                    <div style={{ fontSize: '0.8em', color: '#888', marginBottom: '4px' }}>
                      {m.direction === 'Incoming' ? 'Müşteri' : 'Tesis'} - {new Date(m.createdAt).toLocaleString()}
                    </div>
                    <div>{m.messageText}</div>
                  </div>
                )) : <p style={{ color: '#888', textAlign: 'center', marginTop: '20px' }}>Henüz yazışma yok.</p>}
              </div>
              <div style={{ display: 'flex', gap: '10px' }}>
                <input 
                  type="text" 
                  value={messageText} 
                  onChange={e => setMessageText(e.target.value)} 
                  placeholder="Mesaj yazın..." 
                  style={{ flex: 1, padding: '10px', borderRadius: '4px', border: '1px solid #ccc' }}
                  onKeyPress={e => e.key === 'Enter' && handleSendMessage()}
                />
                <button onClick={handleSendMessage} style={{ padding: '10px 20px', backgroundColor: '#2196F3', color: 'white', border: 'none', borderRadius: '4px', cursor: 'pointer' }}>Gönder</button>
              </div>
            </div>

          </>
        ) : (
          <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'center', height: '100%', color: '#888' }}>
            Listeden bir müşteri seçin.
          </div>
        )}
      </div>
    </div>
  );
}
