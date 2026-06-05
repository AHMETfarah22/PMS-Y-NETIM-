# SOM-PMS - Property Management System 🎯

Kapsamlı bir pansiyon/otel/mülk yönetim sistemi! Hem masaüstü hem de web/API desteği ile tüm operasyonlarınızı tek bir merkezden yönetin.

---

## 📌 Proje Hakkında

SOM-PMS, otel, pansiyon ve küçük ölçekli işletmeler için tasarlanmış modern bir mülk yönetim sistemidir. Müşteri yönetimi, oda rezervasyonları, stok takibi, raporlama ve daha birçok özelliğiyle işletmenizi kolayca yönetmenizi sağlar.

### 🛠️ Kullanılan Teknolojiler

| Bileşen | Teknoloji |
|---------|-----------|
| Masaüstü Uygulaması | .NET 8.0 WinForms (C#) |
| API & Web | ASP.NET Core 8.0 Web API |
| Veritabanı | MySQL |
| Raporlama | QuestPDF |
| Grafikler | ScottPlot |
| E-Posta | SMTP |

---

## ✨ Özellikler

### 🏢 Masaüstü Uygulaması Özellikleri (PmsSystem)
- [x] Kullanıcı Girişi & Kaydı
- [x] Müşteri Yönetimi (Ekle, Sil, Güncelle)
- [x] Oda Yönetimi (Durum, Tip, Fiyat)
- [x] Rezervasyon Yönetimi (Oluştur, Güncelle, İptal)
- [x] Yatak & Kat Planlaması
- [x] Stok & Depo Takibi
- [x] Ödeme İşlemleri
- [x] Fatura & Dekont Oluşturma
- [x] Raporlama (Günlük, Aylık)
- [x] Grafiksel Veri Görselleştirme
- [x] E-Posta Bildirimleri
- [x] Veritabanı Yedekleme & Geri Yükleme

### 🌐 API & Web Özellikleri (PmsApi)
- [x] RESTful API
- [x] JWT Kimlik Doğrulama
- [x] Çapraz Kaynak Paylaşımı (CORS) Desteği
- [x] Online Rezervasyon Oluşturma
- [x] Müşteri API'sı
- [x] Swagger Dokümantasyonu
- [x] Responsive Web Arayüzü

---

## 📂 Proje Yapısı

```
📦 SOM-PMS
├── 📁 PmsSystem/           # WinForms Masaüstü Uygulaması
│   ├── 📁 Components/      # Özel Kontroller
│   ├── 📁 Database/        # Veritabanı İşlemleri
│   ├── 📁 Forms/           # Arayüz Ekranları
│   ├── 📁 Helpers/         # Yardımcı Sınıflar
│   └── 📁 Models/          # Veri Modelleri
├── 📁 PmsApi/              # ASP.NET Core Web API
│   ├── 📁 Controllers/     # API Kontrolleri
│   ├── 📁 Database/        # Veritabanı Yardımcıları
│   ├── 📁 Helpers/         # Yardımcı Sınıflar
│   └── 📁 Models/          # API Modelleri
├── 📁 PmsWeb/              # Vite + React Web Arayüzü (İsteğe Bağlı)
└── 📄 README.md            # Bu Dosya
```

---

## 🚀 Kurulum & Çalıştırma

### 1️⃣ Gereksinimler
- .NET 8.0 SDK
- MySQL Server
- Visual Studio 2022 (veya VS Code)

### 2️⃣ Adımlar

#### Adım 1: Depoyu Klonlayın
```bash
git clone https://github.com/AHMETfarah22/PMS-Y-NETIM-.git
cd PMS-Y-NETIM-
```

#### Adım 2: Veritabanı Kurulumu
1. MySQL'de `pms_system` adında bir veritabanı oluşturun
2. `pms_system.sql` dosyasını içe aktarın (veya uygulama ilk çalıştığında otomatik olarak oluşturulacaktır)

#### Adım 3: Yapılandırma
Her iki proje için `appsettings.json` dosyalarını oluşturun:
- `PmsSystem/appsettings.json` → `PmsSystem/appsettings.example.json` dosyasını kopyalayıp doldurun
- `PmsApi/appsettings.json` → `PmsApi/appsettings.example.json` dosyasını kopyalayıp doldurun

#### Adım 4: Çalıştırma

**Masaüstü Uygulamasını Çalıştırın:**
```bash
cd PmsSystem
dotnet run
```

**API'yi Çalıştırın:**
```bash
cd PmsApi
dotnet run
```
API tarayıcınızda `https://localhost:5001/swagger` adresinden erişilebilir.

---

## 🔒 Güvenlik Notları
- Gerçek şifre ve API anahtarlarınızı `appsettings.json` dosyasına kaydedin
- `appsettings.json` dosyası `.gitignore` ile takip edilmiyor, güvenliğiniz için örnek dosyasını kullanın
- JWT anahtarınızı güçlü ve benzersiz bir şifre olarak ayarlayın

---

## 📧 İletişim
Proje sahibi: **AHMETfarah22**

---

## 📄 Lisans
Bu proje eğitim amaçlı geliştirilmiştir.
