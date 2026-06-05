import React, { useState, useEffect, useRef } from 'react';
import './index.css';
import './styles/Layout.css';
import './styles/HomePage.css';
import './styles/DailyReport.css';
import { getAvailableRooms, getAvailableBeds, createBooking } from './api';
import Layout from './components/Layout';
import HomePage from './pages/HomePage';
import DailyReportPage from './pages/DailyReportPage';
import CustomersPage from './pages/CustomersPage';

function Toast({ msg, type, onClose }) {
  useEffect(() => { const t = setTimeout(onClose, 4500); return () => clearTimeout(t); }, [onClose]);
  return <div className={`toast ${type === 'error' ? 'error' : ''}`}>{msg}</div>;
}

function BookingPage({ 
  scrolled, mobileMenu, setMobileMenu, toast, setToast, 
  heroRef, roomsRef, contactRef, searchRef, 
  dates, setDates, loading, setLoading, apiRooms, setApiRooms,
  displayRooms, roomsLoading, modal, setModal, step, setStep,
  selectedRoom, setSelectedRoom, beds, setBeds, selectedBed, setSelectedBed,
  form, setForm, resCode, setResCode, notify, scrollTo, openModal, closeModal,
  handleSearch, selectRoom, confirmBed, submitBooking, getRoomImg, today
}) {
  return (
    <>
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

      {/* Rest of booking page JSX will go here */}
    </>
  );
}

export default function App() {
  const [currentPage, setCurrentPage] = useState('home');
  const [scrolled, setScrolled] = useState(false);
  const [mobileMenu, setMobileMenu] = useState(false);
  const [toast, setToast] = useState(null);
  const [modal, setModal] = useState(false);
  const [step, setStep] = useState(1);
  const [loading, setLoading] = useState(false);
  const [dates, setDates] = useState({
    start: new Date().toLocaleDateString('en-CA'),
    end: new Date(Date.now() + 86400000).toLocaleDateString('en-CA'),
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
      new Date().toLocaleDateString('en-CA'),
      new Date(Date.now() + 86400000).toLocaleDateString('en-CA')
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

  const today = new Date().toLocaleDateString("en-CA");

  // Simple Admin Routing
  if (window.location.pathname.startsWith('/admin')) {
    return (
      <Layout currentPage={currentPage} onPageChange={setCurrentPage}>
        {currentPage === 'home' && <HomePage />}
        {currentPage === 'daily-report' && <DailyReportPage />}
        {currentPage === 'reservations' && <div>Rezervasyonlar (Yapım Aşamasında)</div>}
        {currentPage === 'customers' && <CustomersPage />}
      </Layout>
    );
  }

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
            <h2 className="section-title">Odalarımız</h2>
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
            <h2 className="section-title">İletişim</h2>
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
              <input
                type="date"
                value={dates.start}
                min={today}
                onChange={e => setDates({ ...dates, start: e.target.value })}
              />
            </div>
            <div className="search-field">
              <label>Çıkış Tarihi</label>
              <input
                type="date"
                value={dates.end}
                min={dates.start}
                onChange={e => setDates({ ...dates, end: e.target.value })}
              />
            </div>
            <button className="search-btn" onClick={handleSearch} disabled={loading}>
              {loading ? <span className="spinner" /> : '🔍 MÜSAİTLİK KONTROL ET'}
            </button>
          </div>
        </div>
      </section>

      {/* ── FOOTER ── */}
      <footer>
        <div className="container">
          <div className="footer-grid">
            <div className="footer-brand">
              <a href="/" className="footer-logo" onClick={e => { e.preventDefault(); scrollTo(heroRef); }}>
                <div className="logo-icon">SP</div>
                <div><span className="logo-text" style={{ color: 'white' }}>SOM PANSİYON</span></div>
              </a>
              <p>
                Misafirlerimize unutulmaz konaklama deneyimi sunmak için 2010'dan beri
                Çanakkale'de hizmetinizdeyiz.
              </p>
            </div>
            <div className="footer-col">
              <h4>Hızlı Erişim</h4>
              <ul>
                <li><a href="#" onClick={e => { e.preventDefault(); scrollTo(heroRef); }}>Anasayfa</a></li>
                <li><a href="#" onClick={e => { e.preventDefault(); scrollTo(roomsRef); }}>Odalar</a></li>
                <li><a href="#" onClick={e => { e.preventDefault(); scrollTo(searchRef); }}>Rezervasyon</a></li>
                <li><a href="#" onClick={e => { e.preventDefault(); scrollTo(contactRef); }}>İletişim</a></li>
              </ul>
            </div>
            <div className="footer-col">
              <h4>Hizmetler</h4>
              <ul>
                <li><a href="#">Açık Büfe Kahvaltı</a></li>
                <li><a href="#">Ücretsiz Wi-Fi</a></li>
                <li><a href="#">Kapalı Otopark</a></li>
                <li><a href="#">24/7 Resepsiyon</a></li>
              </ul>
            </div>
            <div className="footer-col">
              <h4>İletişim</h4>
              <ul>
                <li><a href="tel:+905416264074">📞 +90 541 626 40 74</a></li>
                <li><a href="mailto:guray0449@gmail.com">📧 guray0449@gmail.com</a></li>
                <li><a href="#" onClick={e => { e.preventDefault(); scrollTo(contactRef); }}>📍 Çanakkale Merkez</a></li>
              </ul>
            </div>
          </div>
          <div className="footer-bottom">
            <p>© 2026 SOM Pansiyon. Tüm hakları saklıdır.</p>
            <p style={{ color: 'var(--gold)', fontSize: '0.8rem' }}>✦ Premium Konaklama Deneyimi</p>
          </div>
        </div>
      </footer>

      {/* ── BOOKING MODAL ── */}
      {modal && (
        <div className="booking-overlay" onClick={e => { if (e.target === e.currentTarget) closeModal(); }}>
          <div className="booking-modal">
            <div className="modal-header">
              <h2>
                {step === 1 && '🏨 Oda Seçimi'}
                {step === 2 && '🛏 Yatak Seçimi'}
                {step === 3 && '👤 Misafir Bilgileri'}
                {step === 4 && '✅ Rezervasyon Onaylandı'}
              </h2>
              <button className="modal-close" onClick={closeModal}>✕</button>
            </div>

            <div className="modal-body">
              <div className="step-indicator">
                {[1, 2, 3, 4].map(s => (
                  <div key={s} className={`step-dot ${step === s ? 'active' : step > s ? 'done' : ''}`} />
                ))}
              </div>

              {/* STEP 1 – Room Selection */}
              {step === 1 && (
                <div>
                  <p style={{ marginBottom: '18px', color: 'var(--text-muted)', fontSize: '0.88rem' }}>
                    📅 {dates.start} → {dates.end} &nbsp;·&nbsp; {nights} gece için müsait odalar:
                  </p>
                  {loading && (
                    <div style={{ textAlign: 'center', padding: '48px' }}>
                      <span className="spinner" style={{ borderColor: 'rgba(201,151,58,0.2)', borderTopColor: 'var(--gold)', width: '32px', height: '32px', borderWidth: '3px' }} />
                    </div>
                  )}
                  {!loading && apiRooms.length === 0 && (
                    <div style={{ textAlign: 'center', padding: '48px', color: 'var(--text-muted)' }}>
                      <div style={{ fontSize: '3rem', marginBottom: '12px' }}>😔</div>
                      <p>Seçilen tarihlerde müsait oda bulunamadı.</p>
                    </div>
                  )}
                  <div className="modal-rooms">
                    {apiRooms.map(room => (
                      <div
                        key={room.roomNumber}
                        className={`modal-room-card ${selectedRoom?.roomNumber === room.roomNumber ? 'selected' : ''}`}
                        onClick={() => !loading && selectRoom(room)}
                      >
                        <img src={getRoomImg(room.roomNumber)} alt="oda" className="modal-room-img" />
                        <div className="modal-room-info">
                          <h4>{room.roomType} – Oda {room.roomNumber}</h4>
                          <span>
                            {room.availableBedsCount} yatak müsait &nbsp;·&nbsp; {room.totalCapacity} kişi kapasiteli
                          </span>
                        </div>
                        <div className="modal-room-price">
                          <span className="amount">{room.price} ₺</span>
                          <span className="avail">✓ Müsait</span>
                        </div>
                      </div>
                    ))}
                  </div>
                </div>
              )}

              {/* STEP 2 – Bed Selection */}
              {step === 2 && (
                <div>
                  <p style={{ marginBottom: '20px', color: 'var(--text-muted)', fontSize: '0.88rem' }}>
                    <strong style={{ color: 'var(--gold)' }}>Oda {selectedRoom?.roomNumber}</strong> – {selectedRoom?.roomType} için müsait yataklar:
                  </p>
                  {loading && (
                    <div style={{ textAlign: 'center', padding: '32px' }}>
                      <span className="spinner" style={{ borderColor: 'rgba(201,151,58,0.2)', borderTopColor: 'var(--gold)', width: '28px', height: '28px', borderWidth: '3px' }} />
                    </div>
                  )}
                  {!loading && beds.length === 0 && (
                    <div style={{ textAlign: 'center', padding: '32px', color: 'var(--text-muted)' }}>
                      <p>Bu oda için müsait yatak bulunmuyor.</p>
                    </div>
                  )}
                  <div className="beds-grid">
                    {beds.map(bn => (
                      <div
                        key={bn}
                        className={`bed-card ${selectedBed === bn ? 'selected' : ''}`}
                        onClick={() => setSelectedBed(bn)}
                      >
                        <div className="bed-icon-wrapper">
                          <span className="bed-icon">🛏</span>
                          {selectedBed === bn && <span className="bed-check">✓</span>}
                        </div>
                        <span className="bed-label">Yatak {bn}</span>
                        <span className="bed-desc">Müsait</span>
                      </div>
                    ))}
                  </div>
                  <div className="modal-footer">
                    <button className="btn-back" onClick={() => setStep(1)}>← Geri</button>
                    <button
                      className="btn-next"
                      disabled={!selectedBed || loading}
                      onClick={confirmBed}
                    >
                      {loading ? <span className="spinner" /> : 'Devam Et →'}
                    </button>
                  </div>
                </div>
              )}

              {/* STEP 3 – Guest Info */}
              {step === 3 && (
                <form onSubmit={submitBooking}>
                  <div className="booking-summary">
                    <h4>📋 Rezervasyon Özeti</h4>
                    <div className="summary-row"><span>Oda</span><span>{selectedRoom?.roomType} – No: {selectedRoom?.roomNumber}</span></div>
                    <div className="summary-row"><span>Yatak</span><span>Yatak {selectedBed}</span></div>
                    <div className="summary-row"><span>Giriş</span><span>{dates.start}</span></div>
                    <div className="summary-row"><span>Çıkış</span><span>{dates.end}</span></div>
                    <div className="summary-row"><span>Süre</span><span>{nights} gece</span></div>
                    <div className="summary-row">
                      <span>Toplam</span>
                      <span style={{ color: 'var(--gold-light)', fontWeight: 700, fontSize: '1.1rem' }}>
                        {(selectedRoom?.price * nights).toLocaleString('tr-TR')} ₺
                      </span>
                    </div>
                  </div>

                  <div className="guest-form">
                    <div className="form-field"><label>Ad</label><input required value={form.firstName} onChange={e => setForm({ ...form, firstName: e.target.value })} placeholder="Adınız" /></div>
                    <div className="form-field"><label>Soyad</label><input required value={form.lastName} onChange={e => setForm({ ...form, lastName: e.target.value })} placeholder="Soyadınız" /></div>
                    <div className="form-field"><label>TC / Pasaport No</label><input required value={form.identityNumber} onChange={e => setForm({ ...form, identityNumber: e.target.value })} placeholder="TC Kimlik No" /></div>
                    <div className="form-field"><label>Telefon</label><input required type="tel" value={form.phone} onChange={e => setForm({ ...form, phone: e.target.value })} placeholder="+90 5xx xxx xx xx" /></div>
                    <div className="form-field full"><label>E-Posta</label><input required type="email" value={form.email} onChange={e => setForm({ ...form, email: e.target.value })} placeholder="email@ornek.com" /></div>
                    <div className="form-field full"><label>Notlar (İsteğe Bağlı)</label><textarea value={form.notes} onChange={e => setForm({ ...form, notes: e.target.value })} placeholder="Özel istekleriniz..." /></div>
                  </div>

                  <div className="modal-footer">
                    <button type="button" className="btn-back" onClick={() => setStep(2)}>← Geri</button>
                    <button type="submit" className="btn-next" disabled={loading}>
                      {loading ? <span className="spinner" /> : '✅ Rezervasyonu Tamamla'}
                    </button>
                  </div>
                </form>
              )}

              {/* STEP 4 – Success */}
              {step === 4 && (
                <div className="success-screen">
                  <span className="success-icon">🎉</span>
                  <h2>Rezervasyon Alındı!</h2>
                  <p>Talebiniz başarıyla oluşturuldu.<br />En kısa sürede sizinle iletişime geçeceğiz.</p>
                  <div className="res-code">{resCode}</div>
                  <div className="booking-summary" style={{ textAlign: 'left', maxWidth: '400px', margin: '0 auto 28px' }}>
                    <h4>📋 Özet</h4>
                    <div className="summary-row"><span>Oda</span><span>{selectedRoom?.roomType} – No: {selectedRoom?.roomNumber}</span></div>
                    <div className="summary-row"><span>Yatak</span><span>Yatak {selectedBed}</span></div>
                    <div className="summary-row"><span>Tarih</span><span>{dates.start} → {dates.end}</span></div>
                    <div className="summary-row"><span>Toplam</span><span style={{ color: 'var(--gold-light)' }}>{(selectedRoom?.price * nights).toLocaleString('tr-TR')} ₺</span></div>
                  </div>
                  <button className="btn-primary" onClick={() => { closeModal(); window.scrollTo({ top: 0, behavior: 'smooth' }); }}>
                    Anasayfaya Dön 🏠
                  </button>
                </div>
              )}

            </div>
          </div>
        </div>
      )}

      {toast && <Toast msg={toast.msg} type={toast.type} onClose={() => setToast(null)} />}
    </div>
  );
}

