import React, { useState, useEffect, useRef } from 'react';
import './index.css';
import { getAvailableRooms, getAvailableBeds, createBooking } from './api';

function Toast({ msg, type, onClose }) {
  useEffect(() => { const t = setTimeout(onClose, 4500); return () => clearTimeout(t); }, [onClose]);
  return <div className={`toast ${type === 'error' ? 'error' : ''}`}>{msg}</div>;
}

export default function App() {
  const [scrolled, setScrolled] = useState(false);
  const [mobileMenu, setMobileMenu] = useState(false);
  const [toast, setToast] = useState(null);
  const [modal, setModal] = useState(false);
  const [step, setStep] = useState(1);
  const [loading, setLoading] = useState(false);
  const [dates, setDates] = useState({
    start: new Date().toISOString().split('T')[0],
    end: new Date(Date.now() + 86400000).toISOString().split('T')[0],
  });
  const [apiRooms, setApiRooms] = useState([]);
  const [selectedRoom, setSelectedRoom] = useState(null);
  const [beds, setBeds] = useState([]);
  const [selectedBed, setSelectedBed] = useState(null);
  const [form, setForm] = useState({ firstName: '', lastName: '', identityNumber: '', phone: '', email: '', notes: '' });
  const [resCode, setResCode] = useState('');
  const [displayRooms, setDisplayRooms] = useState([]);
  const [roomsLoading, setRoomsLoading] = useState(true);

  const heroRef = useRef();
  const roomsRef = useRef();
  const contactRef = useRef();
  const searchRef = useRef();

  const notify = (msg, type = 'info') => setToast({ msg, type });

  useEffect(() => {
    const fn = () => setScrolled(window.scrollY > 50);
    window.addEventListener('scroll', fn);

    // Load rooms for display on page
    setRoomsLoading(true);
    getAvailableRooms(
      new Date().toISOString().split('T')[0],
      new Date(Date.now() + 86400000).toISOString().split('T')[0]
    )
      .then(res => setDisplayRooms(res.data))
      .catch(() => setDisplayRooms([]))
      .finally(() => setRoomsLoading(false));

    return () => window.removeEventListener('scroll', fn);
  }, []);

  const scrollTo = (ref) => { ref.current?.scrollIntoView({ behavior: 'smooth', block: 'start' }); setMobileMenu(false); };

  const openModal = () => { setModal(true); setStep(1); setSelectedRoom(null); setBeds([]); setSelectedBed(null); };
  const closeModal = () => setModal(false);

  const handleSearch = async () => {
    if (!dates.start || !dates.end) return notify('Lütfen tarih seçiniz.', 'error');
    if (dates.end <= dates.start) return notify('Çıkış tarihi, giriş tarihinden sonra olmalıdır.', 'error');
    setLoading(true);
    try {
      const res = await getAvailableRooms(dates.start, dates.end);
      setApiRooms(res.data);
      openModal();
    } catch {
      notify('Sunucuya bağlanılamadı. Lütfen tekrar deneyin.', 'error');
    } finally {
      setLoading(false);
    }
  };

  // When a room is selected, fetch FRESH bed availability from server
  const selectRoom = async (room) => {
    setSelectedRoom(room);
    setLoading(true);
    try {
      const res = await getAvailableBeds(room.roomNumber, dates.start, dates.end);
      const availableBeds = res.data;
      if (availableBeds.length === 0) {
        notify('Bu oda için müsait yatak kalmadı. Başka oda seçiniz.', 'error');
        return;
      }
      setBeds(availableBeds);
      setSelectedBed(null);
      setStep(2);
    } catch {
      notify('Yatak bilgisi alınamadı.', 'error');
    } finally {
      setLoading(false);
    }
  };

  // When bed confirmed, re-check from server that it is still available (double-booking guard)
  const confirmBed = async () => {
    if (!selectedBed) return;
    setLoading(true);
    try {
      const res = await getAvailableBeds(selectedRoom.roomNumber, dates.start, dates.end);
      const stillAvailable = res.data.includes(selectedBed);
      if (!stillAvailable) {
        notify('Seçtiğiniz yatak az önce başkası tarafından alındı. Lütfen başka yatak seçin.', 'error');
        // Refresh beds list
        setBeds(res.data);
        setSelectedBed(null);
        return;
      }
      setStep(3);
    } catch {
      notify('Bağlantı hatası. Lütfen tekrar deneyin.', 'error');
    } finally {
      setLoading(false);
    }
  };

  const submitBooking = async (e) => {
    e.preventDefault();
    setLoading(true);
    try {
      const res = await createBooking({
        ...form,
        roomNumber: selectedRoom.roomNumber,
        bedNumber: selectedBed,
        checkInDate: dates.start,
        checkOutDate: dates.end,
      });
      if (res.data.success) {
        setResCode(res.data.reservationCode);
        setStep(4);
        setTimeout(() => {
          setModal(false);
          window.scrollTo({ top: 0, behavior: 'smooth' });
        }, 12000);
      } else {
        notify(res.data.message || 'Rezervasyon başarısız oldu.', 'error');
      }
    } catch (err) {
      const errorData = err.response?.data;
      const msg = typeof errorData === 'object' ? errorData.message : errorData;
      notify(msg || 'Rezervasyon sırasında bir hata oluştu.', 'error');
    } finally {
      setLoading(false);
    }
  };

  const nights = Math.max(1, Math.round((new Date(dates.end) - new Date(dates.start)) / 86400000));

  const getRoomImg = (roomNumber) => {
    const n = parseInt(roomNumber) || 1;
    if (n % 3 === 0) return '/room3.png';
    if (n % 2 === 0) return '/room2.png';
    return '/room1.png';
  };

  const today = new Date().toISOString().split('T')[0];

  return (
    <div className="app">

      {/* ── HEADER ── */}
      <header className={scrolled ? 'scrolled' : ''}>
        <div className="nav-container">
          <a href="/" className="logo footer-logo" onClick={e => { e.preventDefault(); scrollTo(heroRef); }}>
            <div className="logo-icon">SP</div>
            <div>
              <span className="logo-text">SOM PANSİYON</span>
              <span className="logo-sub">Premium Konaklama</span>
            </div>
          </a>

          <ul className="nav-links">
            <li><a href="#anasayfa" onClick={e => { e.preventDefault(); scrollTo(heroRef); }}>Anasayfa</a></li>
            <li><a href="#odalar" onClick={e => { e.preventDefault(); scrollTo(roomsRef); }}>Odalar</a></li>
            <li><a href="#rezervasyon" onClick={e => { e.preventDefault(); scrollTo(searchRef); }}>Rezervasyon</a></li>
            <li><a href="#iletisim" onClick={e => { e.preventDefault(); scrollTo(contactRef); }}>İletişim</a></li>
          </ul>

          <a
            href="#rezervasyon"
            className="nav-links nav-cta"
            onClick={e => { e.preventDefault(); scrollTo(searchRef); }}
          >
            Online Rezervasyon
          </a>
        </div>
      </header>

      {/* ── HERO ── */}
      <section className="hero" ref={heroRef} id="anasayfa">
        <div className="hero-orb hero-orb-1" />
        <div className="hero-orb hero-orb-2" />
        <div className="container">
          <div className="hero-content">
            <div className="hero-eyebrow">
              <span>✦</span> 15 Yıllık Deneyim · Çanakkale Merkez
            </div>
            <h1>
              Huzurun ve <span className="hl">Konforun</span><br />Buluşma Noktası
            </h1>
            <p>
              SOM Pansiyon'da her gece özel, her sabah taze bir başlangıç.
              Çanakkale'nin kalbinde otantik ve lüks konaklama deneyimi.
            </p>
            <div className="hero-btns">
              <button className="btn-primary" onClick={() => scrollTo(searchRef)}>
                🗓 Hemen Rezervasyon Yap
              </button>
              <button className="btn-ghost" onClick={() => scrollTo(roomsRef)}>
                Odaları Keşfet →
              </button>
            </div>
          </div>
        </div>

        <div className="hero-stats">
          <div className="stat-card">
            <span className="stat-num">30</span>
            <span className="stat-lbl">Toplam Oda</span>
          </div>
          <div className="stat-card">
            <span className="stat-num">15+</span>
            <span className="stat-lbl">Yıl Deneyim</span>
          </div>
          <div className="stat-card">
            <span className="stat-num">4.9</span>
            <span className="stat-lbl">Misafir Puanı</span>
          </div>
          <div className="stat-card">
            <span className="stat-num">7/24</span>
            <span className="stat-lbl">Resepsiyon</span>
          </div>
        </div>
      </section>

      {/* ── ROOMS ── */}
      <section className="section" ref={roomsRef} id="odalar">
        <div className="container">
          <div className="section-header">
            <div className="section-label">Konaklama Seçenekleri</div>
            <h3 className="section-title">Odalarımız</h3>
            <p className="section-sub">
              30 odamızın her biri konforunuz için özenle tasarlanmıştır.
              Müsait odaları aşağıda görebilirsiniz.
            </p>
          </div>

          <div className="rooms-grid">
            {roomsLoading && (
              <div className="rooms-empty">
                <span className="empty-icon">⏳</span>
                <p>Odalar yükleniyor...</p>
              </div>
            )}
            {!roomsLoading && displayRooms.length === 0 && (
              <div className="rooms-empty">
                <span className="empty-icon">🛏️</span>
                <p>Bugün için müsait oda bilgisi alınamadı.<br />Rezervasyon yapmak için tarih seçiniz.</p>
              </div>
            )}
            {displayRooms.map(r => (
              <div className="room-card" key={r.roomNumber}>
                <div className="room-img-wrap">
                  <img src={getRoomImg(r.roomNumber)} alt={r.roomType} />
                  <div className="room-img-overlay" />
                  <div className="room-badge">{r.totalCapacity} Kişilik</div>
                </div>
                <div className="room-body">
                  <h3>{r.roomType} – Oda {r.roomNumber}</h3>
                  <p>{r.description || 'Çanakkale manzaralı, rahat ve özenle döşenmiş konforlu oda.'}</p>
                  <div className="room-amenities">
                    <span className="amenity-tag">📶 Wi-Fi</span>
                    <span className="amenity-tag">🌡️ Klima</span>
                    <span className="amenity-tag">📺 TV</span>
                    <span className="amenity-tag">🚿 Özel Banyo</span>
                  </div>
                  <div className="room-footer">
                    <div className="room-price">
                      <span className="amount">{r.price} ₺</span>
                      <span className="per"> / gece</span>
                    </div>
                    <button className="btn-reserve" onClick={() => scrollTo(searchRef)}>
                      Rezervasyon Yap
                    </button>
                  </div>
                </div>
              </div>
            ))}
          </div>
        </div>
      </section>

      <div className="section-divider" />

      {/* ── CONTACT ── */}
      <section className="section section-alt" ref={contactRef} id="iletisim">
        <div className="container">
          <div className="section-header">
            <div className="section-label">Bize Ulaşın</div>
            <h3 className="section-title">İletişim</h3>
            <p className="section-sub">Sorularınız için her zaman yanınızdayız.</p>
          </div>

          <div className="contact-grid">
            <div className="contact-info">
              <h3>Bize Ulaşın</h3>
              {[
                { icon: '📍', label: 'Adres', value: 'Çanakkale Merkez' },
                { icon: '📞', label: 'Telefon', value: '+90 541 626 40 74' },
                { icon: '📧', label: 'E-Posta', value: 'guray0449@gmail.com' },
                { icon: '🕐', label: 'Çalışma Saatleri', value: 'Her gün 07:00 – 23:00' },
              ].map((c, i) => (
                <div className="contact-item" key={i}>
                  <div className="contact-icon">{c.icon}</div>
                  <div>
                    <div className="contact-label">{c.label}</div>
                    <div className="contact-value">{c.value}</div>
                  </div>
                </div>
              ))}
            </div>

            <div className="contact-form-card">
              <h3>Mesaj Gönderin</h3>
              <div className="guest-form">
                <div className="form-field"><label>Adınız</label><input type="text" placeholder="Ad Soyad" /></div>
                <div className="form-field"><label>E-Posta</label><input type="email" placeholder="email@ornek.com" /></div>
                <div className="form-field full"><label>Mesajınız</label><textarea placeholder="Mesajınızı yazın..." /></div>
                <div className="form-field full">
                  <button className="btn-next" style={{ width: '100%', padding: '16px' }}>
                    Mesaj Gönder ✉️
                  </button>
                </div>
              </div>
            </div>
          </div>
        </div>
      </section>

      {/* ── SEARCH / RESERVATION ── */}
      <section className="search-section" ref={searchRef} id="rezervasyon">
        <div className="container">
          <div className="search-section-inner">
            <h2>Online Rezervasyon</h2>
            <p>Giriş ve çıkış tarihlerini seçin, müsait odaları görüntüleyin.</p>
          </div>
          <div className="search-card">
            <div className="search-field">
              <label>Giriş Tarihi</label>
              <input type="date" value={dates.start} onChange={e => setDates({ ...dates, start: e.target.value })} />
            </div>
            <div className="search-field">
              <label>Çıkış Tarihi</label>
              <input type="date" value={dates.end} onChange={e => setDates({ ...dates, end: e.target.value })} />
            </div>
            <button className="btn-primary" onClick={handleSearch} disabled={loading}>
              {loading ? '⏳ Aranıyor...' : '🔍 Ara'}
            </button>
          </div>
        </div>
      </section>

      {/* ── MODAL ── */}
      {modal && (
        <div className="modal">
          <div className="modal-backdrop" onClick={closeModal} />
          <div className="modal-content">
            <button className="modal-close" onClick={closeModal}>✕</button>

            {step === 1 && (
              <>
                <h2>Odalar</h2>
                <p className="modal-sub">Giriş: <strong>{dates.start}</strong> · Çıkış: <strong>{dates.end}</strong> ({nights} gece)</p>
                <div className="modal-rooms">
                  {apiRooms.length === 0 ? (
                    <div className="empty-state">Seçtiğiniz tarihler için müsait oda yok.</div>
                  ) : (
                    apiRooms.map((room, idx) => (
                      <div className="modal-room-item" key={idx} onClick={() => selectRoom(room)}>
                        <div className="modal-room-type">{room.roomType}</div>
                        <div className="modal-room-det">Oda {room.roomNumber} · {room.totalCapacity} kişi</div>
                        <div className="modal-room-price">{room.price} ₺ / gece</div>
                      </div>
                    ))
                  )}
                </div>
              </>
            )}

            {step === 2 && selectedRoom && (
              <>
                <h2>Yatak Seçimi</h2>
                <p className="modal-sub">{selectedRoom.roomType} – Oda {selectedRoom.roomNumber}</p>
                <div className="modal-beds">
                  {beds.map((bed, idx) => (
                    <button
                      key={idx}
                      className={`bed-option ${selectedBed === bed ? 'selected' : ''}`}
                      onClick={() => setSelectedBed(bed)}
                    >
                      🛏️ Yatak {bed}
                    </button>
                  ))}
                </div>
                <div className="modal-footer">
                  <button className="btn-secondary" onClick={() => setStep(1)}>← Geri</button>
                  <button className="btn-primary" onClick={confirmBed} disabled={!selectedBed || loading}>
                    {loading ? '⏳' : '✓'} Devam
                  </button>
                </div>
              </>
            )}

            {step === 3 && selectedRoom && selectedBed && (
              <>
                <h2>Misafir Bilgileri</h2>
                <form onSubmit={submitBooking} className="guest-form">
                  <div className="form-field"><label>Ad</label><input type="text" required value={form.firstName} onChange={e => setForm({...form, firstName: e.target.value})} placeholder="Adınız" /></div>
                  <div className="form-field"><label>Soyad</label><input type="text" required value={form.lastName} onChange={e => setForm({...form, lastName: e.target.value})} placeholder="Soyadınız" /></div>
                  <div className="form-field"><label>Kimlik No</label><input type="text" value={form.identityNumber} onChange={e => setForm({...form, identityNumber: e.target.value})} placeholder="TC Kimlik No" /></div>
                  <div className="form-field"><label>Telefon</label><input type="tel" value={form.phone} onChange={e => setForm({...form, phone: e.target.value})} placeholder="+90 5XX XXX XX XX" /></div>
                  <div className="form-field"><label>E-Posta</label><input type="email" value={form.email} onChange={e => setForm({...form, email: e.target.value})} placeholder="email@example.com" /></div>
                  <div className="form-field"><label>Notlar</label><textarea value={form.notes} onChange={e => setForm({...form, notes: e.target.value})} placeholder="Özel istekler..." /></div>

                  <div className="booking-summary">
                    <div>
                      <strong>{selectedRoom.roomType} – Oda {selectedRoom.roomNumber}</strong><br/>
                      Yatak {selectedBed}<br/>
                      {dates.start} → {dates.end} ({nights} gece)
                    </div>
                    <div className="price"><strong>{selectedRoom.price * nights} ₺</strong></div>
                  </div>

                  <div className="modal-footer">
                    <button type="button" className="btn-secondary" onClick={() => setStep(2)}>← Geri</button>
                    <button type="submit" className="btn-primary" disabled={loading}>
                      {loading ? '⏳ İşleniyor...' : '✓ Rezervasyon Yap'}
                    </button>
                  </div>
                </form>
              </>
            )}

            {step === 4 && (
              <div className="confirmation-message">
                <span className="success-icon">✓</span>
                <h3>Rezervasyon Başarılı!</h3>
                <p>Rezervasyon Kodunuz:</p>
                <div className="res-code">{resCode}</div>
                <p className="confirmation-detail">
                  Reservasyon detayları e-posta adresinize gönderilmiştir.
                </p>
              </div>
            )}
          </div>
        </div>
      )}

      {/* ── FOOTER ── */}
      <footer>
        <div className="container">
          <div className="footer-content">
            <div className="footer-section">
              <a href="/" className="logo footer-logo" onClick={e => { e.preventDefault(); scrollTo(heroRef); }}>
                <div className="logo-icon">SP</div>
                <div>
                  <span className="logo-text">SOM PANSİYON</span>
                  <span className="logo-sub">Premium Konaklama</span>
                </div>
              </a>
            </div>
            <div className="footer-section">
              <h4>Hızlı Linkler</h4>
              <ul>
                <li><a href="#anasayfa" onClick={e => { e.preventDefault(); scrollTo(heroRef); }}>Anasayfa</a></li>
                <li><a href="#odalar" onClick={e => { e.preventDefault(); scrollTo(roomsRef); }}>Odalar</a></li>
                <li><a href="#iletisim" onClick={e => { e.preventDefault(); scrollTo(contactRef); }}>İletişim</a></li>
              </ul>
            </div>
            <div className="footer-section">
              <h4>İletişim</h4>
              <p>📍 Çanakkale Merkez<br/>📞 +90 541 626 40 74<br/>📧 guray0449@gmail.com</p>
            </div>
          </div>
          <div className="footer-bottom">
            <p>&copy; 2026 SOM PANSİYON. Tüm Hakları Saklıdır.</p>
          </div>
        </div>
      </footer>

      {toast && <Toast msg={toast.msg} type={toast.type} onClose={() => setToast(null)} />}
    </div>
  );
}
