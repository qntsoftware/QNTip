<!-- ========================================================= -->
<!--                   QNTSOFTWARE — QNTip                      -->
<!--      Tor IP Rotator & Encrypted Tunnel for Windows         -->
<!-- ========================================================= -->

<div align="center">

# 🛰️ QNTSOFTWARE — QNTip

### Tor IP Rotator & Encrypted Tunnel

![Project Name](https://img.shields.io/badge/PROJECT%20NAME-QNTip-red?style=for-the-badge)
![Build](https://img.shields.io/badge/BUILD-PASSING-brightgreen?style=for-the-badge)
![Version](https://img.shields.io/badge/VERSION-1.0.0-blue?style=for-the-badge)
![License](https://img.shields.io/badge/LICENSE-MIT-purple?style=for-the-badge)
![Platform](https://img.shields.io/badge/PLATFORM-WINDOWS%2010%20%7C%2011-lightgrey?style=for-the-badge)
![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![Language](https://img.shields.io/badge/LANGUAGE-C%23-239120?style=for-the-badge&logo=c-sharp&logoColor=white)
![Status](https://img.shields.io/badge/STATUS-ACTIVE-success?style=for-the-badge)

<img width="900" alt="QNTip Preview" src="https://i.hizliresim.com/zlfhpgfg.png" />

</div>

---

## 📌 Kısa Açıklama

> **QNTip**, Tor ağı üzerinden otomatik IP rotasyonu yapan, yerel HTTP/SOCKS5 proxy sunan, şifreli ve şifresiz iki modda çalışabilen, Windows için geliştirilmiş gelişmiş bir gizlilik aracıdır. Firefox gibi tarayıcıları tek tıkla Tor üzerinden çıkarır, belirlediğiniz aralıklarla IP adresinizi otomatik değiştirir ve her şeyi canlı terminal paneli ile gösterir.

---

## 🚀 Çalıştırma

```bash
# 1) Projeyi indir
dosyayı ayıkla

# 2) Derle
dotnet build -c Release

# 3) Çalıştır (yönetici izni istenecek)
dotnet run -c Release

# 4) EXE olarak dağıtmak istersen
dotnet publish -c Release -r win-x64 --self-contained true `
  -p:PublishSingleFile=true -p:PublishTrimmed=true
```

> 🛡️ Program ilk açılışta **Windows UAC** tarafından "Yönetici olarak çalıştırılsın mı?" diye **sorar**. Onaylarsan açılır, reddedersen açılmaz. Sessiz yükseltme yoktur.

---

## 📚 İçindekiler

- [Tanım](#-tanım)
- [Özellikler](#-özellikler)
- [Sistem Gereksinimleri](#-sistem-gereksinimleri)
- [Kurulum](#-kurulum)
- [Kullanım](#-kullanım)
- [Modlar](#-modlar)
- [Terminal Kısayolları](#-terminal-kısayolları)
- [Firefox Proxy Ayarları](#-firefox-proxy-ayarları)
- [Paket İzleyici](#-paket-i̇zleyici)
- [Otomatik Kurtarma](#-otomatik-kurtarma)
- [Güvenli Çıkış](#-güvenli-çıkış)
- [Proje Yapısı](#-proje-yapısı)
- [Sorun Giderme](#-sorun-giderme)
- [SSS](#-sıkça-sorulan-sorular-sss)
- [Lisans](#-lisans)
- [Yasal Uyarı](#-yasal-uyarı)

---

## 📖 Tanım

**QNTip**, Tor ağı üzerinden IP rotasyonu yapan, yerel proxy sunan ve Firefox gibi tarayıcıları tek tuşla Tor üzerinden çıkaran bir Windows aracıdır. `.NET 8` ile yazılmış, terminal üzerinden çalışan bir gizlilik aracıdır.

---

## ✨ Özellikler

<details>
<summary><b>1. 🔄 Otomatik IP Rotasyonu</b></summary>

- Belirlenen saniye aralığında **SIGNAL NEWNYM** ile yeni Tor circuit
- Her değişimde **yeni IP + ülke + bölge + şehir + ISP** bilgisi
- Sonsuz mod veya belirli döngü sayısı desteği

</details>

<details>
<summary><b>2. 🚀 Yerel Proxy Sunucusu (8080)</b></summary>

- HTTP CONNECT + normal HTTP destekli
- SOCKS5 olarak da kullanılabilir
- Firefox / Chrome / Edge tek ayarla bağlanır
- Trafik Tor SOCKS5 (9050) üzerinden otomatik yönlendirilir

</details>

<details>
<summary><b>3. 🔐 İki Mod: Şifreli ve Şifresiz</b></summary>

- **Mod 1 — Şifreli:** TLS 1.3 + AES-256-GCM ek şifreleme katmanı
- **Mod 2 — Şifresiz:** Yalnızca Tor, sadece IP gizleme
- Her iki modda da Tor ağı kullanılır, IP gizlenir

</details>

<details>
<summary><b>4. 📦 Canlı Paket İzleyici</b></summary>

- Her istek terminalde canlı görünür
- Method, host, port, gelen/giden bayt, süre, HTTP status
- Bağlantı istatistikleri 30 saniyede bir özetlenir

</details>

<details>
<summary><b>5. 🌍 Tek Çağrıda IP + Konum</b></summary>

- Tek API çağrısı ile tüm bilgiler: `ip-api.com`
- Fallback: `ipinfo.io`
- 3 retry + exponential backoff

</details>

<details>
<summary><b>6. 🔤 TR / ENG Dil Değiştirme</b></summary>

- Program çalışırken **E** ile İngilizceye, **T** ile Türkçeye geçiş
- Anlık buton değişimi (sağ üst köşe)

</details>

<details>
<summary><b>7. 🛑 Anında Durdur Butonu</b></summary>

- Program çalışırken **S** tuşu ile anında durdurma
- Sağ üst köşede görsel **STOP** butonu

</details>

<details>
<summary><b>8. 🛡️ Otomatik Kurtarma</b></summary>

- 30 saniyede bir Tor sağlık kontrolü
- 2 başarısız kontrolden sonra **otomatik restart**
- NEWNYM başarısız olursa 3 kez dener

</details>

<details>
<summary><b>9. 🚪 Güvenli Çıkış (Alt+F4 / X Koruması)</b></summary>

- **Alt+F4** ve **X (çarpı)** yakalanır
- Program çıkıştan önce **Tor'u ve proxy'yi kapatır**
- Bu sayede bir sonraki açılışta **IP'ler uyuşur**
- Ctrl+C / S ile de güvenli çıkış

</details>

<details>
<summary><b>10. 📥 Otomatik Tor İndirme</b></summary>

- Sistemde `tor.exe` yoksa **Tor Expert Bundle** indirilir
- `%LOCALAPPDATA%\QNTip\tor-bundle` altına çıkarılır
- Önbellekten bir daha indirilmez

</details>

---

## 💻 Sistem Gereksinimleri

| Bileşen | Minimum |
|---------|---------|
| İşletim Sistemi | Windows 10 / 11 |
| .NET | .NET 8 SDK |
| RAM | 2 GB |
| Disk | 100 MB (Tor bundle dahil) |
| İnternet | Tor bağlantısı için gerekli |
| Yetki | Yönetici (UAC) |

---

## 🛠️ Kurulum

### 1) .NET 8 SDK Kurulumu

[dotnet.microsoft.com/download](https://dotnet.microsoft.com/download) adresinden **.NET 8 SDK**'yı indir ve kur.

```bash
dotnet --version
# 8.0.x çıktısı görmelisin
```

### 2) Projeyi Klonla

```bash
git clone https://github.com/https://github.com/qntsoftware/QNTip.git
cd QNTip
```

### 3) Derle

```bash
dotnet build -c Release
```

### 4) Çalıştır

```bash
dotnet run -c Release
```

---

## 🎯 Kullanım

### Temel Akış

```bash
dotnet run -c Release
```

Program sırayla şunları sorar:

| Soru | Varsayılan | Açıklama |
|------|------------|----------|
| İşlem modu | 1 | 1 = Şifreli, 2 = Şifresiz |
| IP değişim aralığı | 60 sn | Kaç saniyede bir IP değişsin |
| Döngü sayısı | 0 | 0 = sonsuz, N = N kez |

### Örnek Çıktı

```
   ___  _   _ _____ _     
  / _ \| \ | |_   _(_)_ __
 | | | |  \| | | | | | '_ \
 | |_| | |\  | | | | | |_) |
  \__\_\_| \_| |_| |_| .__/
                     |_|

  ============================================
   QNTip  -  Tor IP Rotator & Encrypted Tunnel
   Developer: QNT SOFTWARE
  ============================================

[+] Tor baslatiliyor...
[+] Tor hazir.

  ╔══════════════════════════════════════════════════════════╗
  ║  🚀  PROXY AKTIF                                          ║
  ╠══════════════════════════════════════════════════════════╣
  ║  Dinleniyor    : 127.0.0.1:8080                           ║
  ║  Yonlendirme   : Tor SOCKS5 127.0.0.1:9050                ║
  ║  Mod           : SIFRELI (TLS+Tor)                        ║
  ╚══════════════════════════════════════════════════════════╝

  ╔══════════════════════════════════════════════════════════╗
  ║                🌍  YENI IP ADRESI TESPIT EDILDI          ║
  ╠══════════════════════════════════════════════════════════╣
  ║  IP Adresi : 185.220.101.42                               ║
  ║  Ulke      : Germany                                      ║
  ║  Bolge     : Berlin                                       ║
  ║  Sehir     : Berlin                                       ║
  ║  ISP / Org : TORSERVERS-NET                               ║
  ╚══════════════════════════════════════════════════════════╝

  [1] Sonraki IP degisimi icin 60 sn:  60  59  58  57 ...
```

---

## 🔐 Modlar

| Mod | Adı | Açıklama | Kullanım |
|-----|-----|----------|----------|
| **1** | Şifreli | TLS 1.3 + AES-256-GCM + Tor | Maksimum gizlilik |
| **2** | Şifresiz | Yalnızca Tor | Sadece IP gizleme |

**Fark:** Şifreli modda ek olarak **TLS 1.3** zorunlu ve **AES-256-GCM** ile her payload tekrar şifrelenir. Şifresiz modda TLS 1.2 + 1.3 kullanılır ve ek katman yoktur.

---

## ⌨️ Terminal Kısayolları

| Tuş | İşlev |
|-----|-------|
| **S** | Programı durdur (güvenli çıkış, Tor kapatılır) |
| **E** | Arayüzü İngilizce yap (buton `[T] TR` olur) |
| **T** | Arayüzü Türkçe yap (buton `[E] ENG` olur) |
| **Ctrl + C** | Güvenli çıkış |
| **Alt + F4** | Yakalanır → Tor kapat → sonra çık |
| **X (çarpı)** | Yakalanır → Tor kapat → sonra çık |

---

## 🦊 Firefox Proxy Ayarları

Firefox → **Ayarlar** → **Ağ Ayarları**:

| Alan | Değer |
|------|-------|
| ○ Manuel proxy yapılandırması | **SEÇ** |
| **SOCKS Host** | `127.0.0.1` |
| **Port** | `8080` |
| ○ **SOCKS v5** | **SEÇ** |
| ☑ SOCKS v5 kullanırken vekil DNS | **İŞARETLE** |
| Bypass List | `localhost, 127.0.0.1` |

> 📌 "SOCKS v5 kullanırken vekil DNS" işaretli olmalı, yoksa DNS sızıntısı olur.

---

## 📦 Paket İzleyici

Terminalde her istek şöyle görünür:

```
[14:32:11.421] #0001 → GET  HTTPS://api.ipify.org:443/         [giden: 0 B]
[14:32:13.882]      ← 200 OK                     [gelen: 14 B]   2462 ms
[14:32:13.891] #0002 → CONNECT  www.google.com:443
[14:32:14.112]      ← 200 Connection Established
```

- **#NNNN** — Sıra numarası
- **→** — Giden istek
- **←** — Gelen yanıt
- **giden / gelen** — Bayt cinsinden boyut
- **ms** — Yanıt süresi

---

## 🛡️ Otomatik Kurtarma

| Olay | Davranış |
|------|----------|
| Tor process ölür | 30 sn içinde tespit, otomatik restart |
| SOCKS portu yanıt vermez | 2 başarısız kontrol → restart |
| NEWNYM başarısız | 3 kez dener (2s, 4s, 6s aralıkla) |
| API timeout | 3 kez dener (2s, 4s aralıkla) |
| Proxy bağlantısı askıda | 10 sn ilk satır timeout, 5 dk idle timeout |

---

## 🚪 Güvenli Çıkış

QNTip her kapanışta:

1. Proxy sunucusunu durdurur (8080 serbest)
2. Tor process'ini öldürür (9050/9051 serbest)
3. `torrc` cache'ini bırakır ama eski process kalmaz
4. Sonra process'i kapatır

**Neden önemli?** Çünkü bir sonraki açılışta eski `tor.exe` çalışmaya devam ederse Firefox farklı circuit, QNTip farklı circuit kullanır → **IP'ler uyuşmaz**.

---

## 📁 Proje Yapısı

```
QNTip/
├── QNTip.csproj                    # Proje dosyası
├── app.manifest                    # UAC (yönetici) manifest
├── Program.cs                      # Ana akış
├── Localization/
│   ├── Loc.cs                      # TR/ENG sözlük
│   └── Buttons.cs                  # Sağ üst butonlar
├── UI/
│   └── Banner.cs                   # ASCII banner
├── Modes/
│   └── OperationMode.cs            # Enum: Encrypted / Plain
├── Crypto/
│   └── AesGcmCrypto.cs             # AES-256-GCM
├── Tor/
│   ├── TorManager.cs               # Tor başlat/durdur/health
│   ├── TorControlClient.cs         # SIGNAL NEWNYM
│   └── TorDownloader.cs            # Otomatik Tor indirici
└── Net/
    ├── Socks5Client.cs             # SOCKS5 protocol
    ├── TorHttpClient.cs            # HttpClient (Tor üzerinden)
    ├── IpChecker.cs                # IP + konum çekici
    ├── PacketLogger.cs             # Canlı log
    ├── LoggingHandler.cs           # HTTP middleware
    └── HttpProxyServer.cs          # 8080 proxy sunucusu
```

---

## 🔧 Sorun Giderme

| Sorun | Sebep | Çözüm |
|-------|-------|-------|
| `tor.exe bulunamadi` | Sistemde Tor yok | Program otomatik indirir |
| `Tor baslatilamadi` | 9050/9051 dolu | Tor Browser'ı kapat |
| `Proxy baslatilamadi` | Port 8080 dolu | `netstat -ano \| findstr :8080` ile kontrol |
| Firefox bağlanamıyor | Proxy kapalı veya yanlış | 127.0.0.1:8080 SOCKS5 |
| Firefox'ta IP farklı | Stream isolation | `torrc`'de `NoIsolateDestAddr` var mı kontrol |
| İlk çalıştırmada yavaş | Tor bundle indiriliyor | ~30 MB, bir kere iner |
| DNS sızıntısı | Firefox'ta "vekil DNS" kapalı | Ayarlardan işaretle |
| NEWNYM başarısız | Tor meşgul | Program 3 kez dener, log atar |

---

## ❓ Sıkça Sorulan Sorular (SSS)

<details>
<summary><b>QNTip yasal mı?</b></summary>

Bu araç yalnızca **eğitim** ve **yetkili güvenlik testleri** için tasarlanmıştır. İzinsiz sistemlerde kullanmak yasa dışıdır. Kullanıcı, aracın kullanımından doğacak tüm sorumluluğu kabul eder.

</details>

<details>
<summary><b>Tor Browser açık olmalı mı?</b></summary>

**Hayır.** QNTip kendi Tor process'ini başlatır. Tor Browser açıksa 9050/9051 portları dolu olur ve QNTip başlatamaz. **Tor Browser'ı kapatın.**

</details>

<details>
<summary><b>Firefox'ta gördüğüm IP ile QNTip'in gösterdiği IP farklı, neden?</b></summary>

Önceki sürümlerde Tor'un **stream isolation** özelliği her hedef için ayrı circuit açıyordu. Bu sorun **`NoIsolateDestAddr`** ve **`NoIsolateSOCKSAuth`** flag'leri ile çözüldü. En güncel sürümü kullanın.

</details>

<details>
<summary><b>Programı Alt+F4 ile kapatınca Tor neden kalıyor?</b></summary>

Eski sürümlerde kalıyordu. Yeni sürümde **`SetConsoleCtrlHandler`** ile Alt+F4 ve X yakalanır, **Tor düzgün kapatılır.** Böylece bir sonraki açılışta eski `tor.exe` kalmaz.

</details>

<details>
<summary><b>Gerçek VPN gibi tüm sistem trafiğini şifreliyor mu?</b></summary>

Hayır. Sistem geneli VPN için **TUN/TAP (WinTun)** sürücüsü gerekir. QNTip **uygulama katmanı VPN** sağlar: Firefox gibi proxy destekli uygulamalar Tor üzerinden çıkar. Sistemin geri kalanı (Windows Update, oyunlar vs.) doğrudan interneti kullanır.

</details>

<details>
<summary><b>Mod 1 (şifreli) ile mod 2 (şifresiz) arasındaki fark nedir?</b></summary>

- **Mod 1:** TLS 1.3 zorunlu + AES-256-GCM ile payload tekrar şifrelenir
- **Mod 2:** Yalnızca Tor, ek katman yok (daha hızlı)

Her iki modda da **IP gizlenir**. Fark, ek şifreleme katmanıdır.

</details>

---

## ⚖️ Lisans

Bu proje **MIT Lisansı** altında açık kaynaklıdır ve eğitim amaçlı kullanılabilir.

---

## ⚠️ Yasal Uyarı

Bu araç yalnızca **eğitim** ve **yetkili güvenlik testleri** için tasarlanmıştır. İzinsiz sistemlerde kullanmak yasa dışıdır. Kullanıcı, aracın kullanımından doğacak tüm sorumluluğu kabul eder. Geliştirici hiçbir kötüye kullanımdan sorumlu tutulamaz.

---

<div align="center">

**⭐ Projeyi beğendiyseniz yıldız vermeyi unutmayın! ⭐**

**Made with ❤️ by QNT SOFTWARE**

</div>

---

-------------------------ENGLİSH------------------------

<div align="center">

# 🛰️ QNTSOFTWARE — QNTip

### Tor IP Rotator & Encrypted Tunnel

![Project Name](https://img.shields.io/badge/PROJECT%20NAME-QNTSOFTWARE-red?style=for-the-badge)
![Build](https://img.shields.io/badge/BUILD-PASSING-brightgreen?style=for-the-badge)
![Version](https://img.shields.io/badge/VERSION-1.0.0-blue?style=for-the-badge)
![License](https://img.shields.io/badge/LICENSE-MIT-purple?style=for-the-badge)
![Platform](https://img.shields.io/badge/PLATFORM-WINDOWS%2010%20%7C%2011-lightgrey?style=for-the-badge)
![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![Language](https://img.shields.io/badge/LANGUAGE-C%23-239120?style=for-the-badge&logo=c-sharp&logoColor=white)
![Status](https://img.shields.io/badge/STATUS-ACTIVE-success?style=for-the-badge)

<img width="900" alt="QNTip Preview" src="https://i.hizliresim.com/zlfhpgfg.png" />

</div>

---

## 📌 Short Description

> **QNTip** is an advanced privacy tool for Windows that performs automatic IP rotation through the Tor network, provides a local HTTP/SOCKS5 proxy, and supports both encrypted and unencrypted modes. It routes browsers such as Firefox through Tor with a single click, automatically rotates your IP at your chosen interval, and displays everything via a live terminal panel.

---

## 🚀 Quick Start

```bash
# 1) Clone the repository
git clone https://github.com/<your-username>/QNTip.git
cd QNTip

# 2) Build
dotnet build -c Release

# 3) Run (admin permission will be requested)
dotnet run -c Release

# 4) Publish as standalone EXE
dotnet publish -c Release -r win-x64 --self-contained true `
  -p:PublishSingleFile=true -p:PublishTrimmed=true
```

> 🛡️ On first launch, **Windows UAC** asks "Do you want to allow this app to make changes?". If you approve, it launches as admin. If you decline, it does not launch. No silent elevation.

---

## 📚 Table of Contents

- [Definition](#-definition)
- [Features](#-features)
- [Requirements](#-requirements)
- [Installation](#-installation)
- [Usage](#-usage)
- [Modes](#-modes)
- [Terminal Shortcuts](#-terminal-shortcuts)
- [Firefox Proxy Settings](#-firefox-proxy-settings)
- [Packet Logger](#-packet-logger)
- [Auto Recovery](#-auto-recovery)
- [Safe Exit](#-safe-exit)
- [Project Structure](#-project-structure)
- [Troubleshooting](#-troubleshooting)
- [FAQ](#-frequently-asked-questions-faq)
- [License](#-license)
- [Legal Disclaimer](#-legal-disclaimer)

---

## 📖 Definition

**QNTip** is a Windows tool that rotates your IP address through the Tor network, provides a local proxy, and routes browsers like Firefox through Tor with a single click. Written in `.NET 8`, it's a privacy tool that runs entirely in the terminal.

---

## ✨ Features

<details>
<summary><b>1. 🔄 Automatic IP Rotation</b></summary>

- **SIGNAL NEWNYM** triggers a new Tor circuit at your chosen interval
- Each change shows **new IP + country + region + city + ISP**
- Infinite mode or fixed cycle count

</details>

<details>
<summary><b>2. 🚀 Local Proxy Server (8080)</b></summary>

- HTTP CONNECT + plain HTTP support
- Can also be used as SOCKS5
- Works with Firefox / Chrome / Edge
- Traffic is forwarded through Tor SOCKS5 (9050)

</details>

<details>
<summary><b>3. 🔐 Two Modes: Encrypted & Plain</b></summary>

- **Mode 1 — Encrypted:** TLS 1.3 + AES-256-GCM additional layer
- **Mode 2 — Plain:** Tor only, IP hiding only
- Both modes route through Tor and hide your IP

</details>

<details>
<summary><b>4. 📦 Live Packet Logger</b></summary>

- Every request visible in terminal in real-time
- Method, host, port, bytes in/out, duration, HTTP status
- Connection stats summarized every 30 seconds

</details>

<details>
<summary><b>5. 🌍 IP + Location in One Call</b></summary>

- Single API call: `ip-api.com`
- Fallback: `ipinfo.io`
- 3 retries + exponential backoff

</details>

<details>
<summary><b>6. 🔤 TR / ENG Language Toggle</b></summary>

- Press **E** for English, **T** for Turkish at runtime
- Instant button update (top right corner)

</details>

<details>
<summary><b>7. 🛑 Instant Stop Button</b></summary>

- Press **S** to stop instantly
- Visual **STOP** button in top right

</details>

<details>
<summary><b>8. 🛡️ Auto Recovery</b></summary>

- Tor health check every 30 seconds
- Auto restart after 2 failed checks
- NEWNYM retries 3 times on failure

</details>

<details>
<summary><b>9. 🚪 Safe Exit (Alt+F4 / X Protection)</b></summary>

- **Alt+F4** and **X (close)** intercepted
- Program closes Tor and proxy before exiting
- Prevents IP mismatch on next launch
- Ctrl+C / S also safe exit

</details>

<details>
<summary><b>10. 📥 Auto Tor Download</b></summary>

- If `tor.exe` missing, downloads **Tor Expert Bundle**
- Extracts to `%LOCALAPPDATA%\QNTip\tor-bundle`
- Cached for future runs

</details>

---

## 💻 Requirements

| Component | Minimum |
|-----------|---------|
| OS | Windows 10 / 11 |
| .NET | .NET 8 SDK |
| RAM | 2 GB |
| Disk | 100 MB (with Tor bundle) |
| Internet | Required for Tor |
| Privileges | Administrator (UAC) |

---

## 🛠️ Installation

### 1) Install .NET 8 SDK

Download from [dotnet.microsoft.com/download](https://dotnet.microsoft.com/download).

```bash
dotnet --version
# Should output 8.0.x
```

### 2) Clone the Repository

```bash
git clone https://github.com/<your-username>/QNTip.git
cd QNTip
```

### 3) Build

```bash
dotnet build -c Release
```

### 4) Run

```bash
dotnet run -c Release
```

---

## 🎯 Usage

### Basic Flow

```bash
dotnet run -c Release
```

The program asks:

| Question | Default | Description |
|----------|---------|-------------|
| Operation mode | 1 | 1 = Encrypted, 2 = Plain |
| IP change interval | 60 sec | How often to change IP |
| Cycle count | 0 | 0 = infinite, N = N times |

### Example Output

```
   ___  _   _ _____ _     
  / _ \| \ | |_   _(_)_ __
 | | | |  \| | | | | | '_ \
 | |_| | |\  | | | | | |_) |
  \__\_\_| \_| |_| |_| .__/
                     |_|

  ============================================
   QNTip  -  Tor IP Rotator & Encrypted Tunnel
   Developer: QNT SOFTWARE
  ============================================

[+] Starting Tor...
[+] Tor ready.

  ╔══════════════════════════════════════════════════════════╗
  ║  🚀  PROXY ACTIVE                                         ║
  ╠══════════════════════════════════════════════════════════╣
  ║  Listening    : 127.0.0.1:8080                            ║
  ║  Forwarding   : Tor SOCKS5 127.0.0.1:9050                 ║
  ║  Mode         : ENCRYPTED (TLS+Tor)                       ║
  ╚══════════════════════════════════════════════════════════╝

  ╔══════════════════════════════════════════════════════════╗
  ║                🌍  NEW IP ADDRESS DETECTED               ║
  ╠══════════════════════════════════════════════════════════╣
  ║  IP Address : 185.220.101.42                              ║
  ║  Country    : Germany                                     ║
  ║  Region     : Berlin                                      ║
  ║  City       : Berlin                                      ║
  ║  ISP / Org  : TORSERVERS-NET                              ║
  ╚══════════════════════════════════════════════════════════╝
```

---

## 🔐 Modes

| Mode | Name | Description | Use |
|------|------|-------------|-----|
| **1** | Encrypted | TLS 1.3 + AES-256-GCM + Tor | Maximum privacy |
| **2** | Plain | Tor only | IP hiding only |

**Difference:** Encrypted mode forces **TLS 1.3** and adds an **AES-256-GCM** layer on top. Plain mode uses TLS 1.2 + 1.3 with no extra layer.

---

## ⌨️ Terminal Shortcuts

| Key | Action |
|-----|--------|
| **S** | Stop program (safe exit, Tor shut down) |
| **E** | Switch UI to English (button becomes `[T] TR`) |
| **T** | Switch UI to Turkish (button becomes `[E] ENG`) |
| **Ctrl + C** | Safe exit |
| **Alt + F4** | Intercepted → shutdown Tor → then exit |
| **X (close)** | Intercepted → shutdown Tor → then exit |

---

## 🦊 Firefox Proxy Settings

Firefox → **Settings** → **Network Settings**:

| Field | Value |
|-------|-------|
| ○ Manual proxy configuration | **SELECT** |
| **SOCKS Host** | `127.0.0.1` |
| **Port** | `8080` |
| ○ **SOCKS v5** | **SELECT** |
| ☑ Proxy DNS when using SOCKS v5 | **CHECK** |
| Bypass List | `localhost, 127.0.0.1` |

> 📌 "Proxy DNS when using SOCKS v5" must be checked, otherwise DNS leaks occur.

---

## 📦 Packet Logger

Each request appears in the terminal like this:

```
[14:32:11.421] #0001 → GET  HTTPS://api.ipify.org:443/         [out: 0 B]
[14:32:13.882]      ← 200 OK                     [in: 14 B]   2462 ms
[14:32:13.891] #0002 → CONNECT  www.google.com:443
[14:32:14.112]      ← 200 Connection Established
```

- **#NNNN** — Sequence number
- **→** — Outgoing request
- **←** — Incoming response
- **out / in** — Size in bytes
- **ms** — Response time

---

## 🛡️ Auto Recovery

| Event | Behavior |
|-------|----------|
| Tor process dies | Detected within 30s, auto restart |
| SOCKS port unresponsive | 2 failed checks → restart |
| NEWNYM fails | Retries 3 times (2s, 4s, 6s interval) |
| API timeout | Retries 3 times (2s, 4s interval) |
| Proxy connection stuck | 10s first-line timeout, 5 min idle timeout |

---

## 🚪 Safe Exit

QNTip on every close:

1. Stops proxy server (8080 released)
2. Kills Tor process (9050/9051 released)
3. Leaves `torrc` cache but no stale process
4. Then closes the process

**Why important?** Because if the old `tor.exe` keeps running, Firefox uses a different circuit than QNTip → **IPs won't match**.

---

## 📁 Project Structure

```
QNTip/
├── QNTip.csproj                    # Project file
├── app.manifest                    # UAC (admin) manifest
├── Program.cs                      # Main flow
├── Localization/
│   ├── Loc.cs                      # TR/ENG dictionary
│   └── Buttons.cs                  # Top right buttons
├── UI/
│   └── Banner.cs                   # ASCII banner
├── Modes/
│   └── OperationMode.cs            # Enum: Encrypted / Plain
├── Crypto/
│   └── AesGcmCrypto.cs             # AES-256-GCM
├── Tor/
│   ├── TorManager.cs               # Start/stop/health
│   ├── TorControlClient.cs         # SIGNAL NEWNYM
│   └── TorDownloader.cs            # Auto Tor downloader
└── Net/
    ├── Socks5Client.cs             # SOCKS5 protocol
    ├── TorHttpClient.cs            # HttpClient (over Tor)
    ├── IpChecker.cs                # IP + location fetcher
    ├── PacketLogger.cs             # Live log
    ├── LoggingHandler.cs           # HTTP middleware
    └── HttpProxyServer.cs          # 8080 proxy server
```

---

## 🔧 Troubleshooting

| Issue | Cause | Fix |
|-------|-------|-----|
| `tor.exe not found` | No Tor installed | Program auto-downloads |
| `Tor start failed` | 9050/9051 busy | Close Tor Browser |
| `Proxy start failed` | Port 8080 busy | Check `netstat -ano \| findstr :8080` |
| Firefox can't connect | Proxy off or wrong | Use 127.0.0.1:8080 SOCKS5 |
| Firefox IP mismatch | Stream isolation | Verify `NoIsolateDestAddr` in torrc |
| Slow first run | Tor bundle downloading | ~30 MB, one-time |
| DNS leak | Firefox "proxy DNS" off | Enable in settings |
| NEWNYM failed | Tor busy | Program retries 3x |

---

## ❓ Frequently Asked Questions (FAQ)

<details>
<summary><b>Is QNTip legal?</b></summary>

This tool is designed for **educational** and **authorized security testing** purposes only. Using it on systems without permission is illegal. The user accepts all responsibility arising from the use of this tool.

</details>

<details>
<summary><b>Should Tor Browser be running?</b></summary>

**No.** QNTip starts its own Tor process. If Tor Browser is open, ports 9050/9051 are busy and QNTip can't start. **Close Tor Browser.**

</details>

<details>
<summary><b>Firefox shows a different IP than QNTip, why?</b></summary>

Earlier versions had Tor's **stream isolation** opening a separate circuit for each destination. This was fixed with **`NoIsolateDestAddr`** and **`NoIsolateSOCKSAuth`** flags. Use the latest version.

</details>

<details>
<summary><b>Why does Tor stay alive after Alt+F4?</b></summary>

Older versions had this bug. The new version intercepts Alt+F4 and X via **`SetConsoleCtrlHandler`** and **shuts down Tor properly.** No stale `tor.exe` remains.

</details>

<details>
<summary><b>Does it encrypt all system traffic like a real VPN?</b></summary>

No. Full-system VPN requires **TUN/TAP (WinTun)** driver. QNTip provides an **application-layer VPN**: proxy-aware apps like Firefox route through Tor. The rest of the system (Windows Update, games, etc.) uses your direct connection.

</details>

<details>
<summary><b>What's the difference between Mode 1 (encrypted) and Mode 2 (plain)?</b></summary>

- **Mode 1:** TLS 1.3 enforced + AES-256-GCM additional layer
- **Mode 2:** Tor only, no extra layer (faster)

Both modes **hide your IP**. The difference is the extra encryption layer.

</details>

---

## ⚖️ License

This project is open source under the **MIT License** and can be used for educational purposes.

---

## ⚠️ Legal Disclaimer

This tool is intended for **educational** and **authorized security testing** purposes only. Using it on systems without permission is illegal. The user accepts all responsibility arising from the use of this tool. The developer cannot be held liable for any misuse.

---

<div align="center">

**⭐ If you like the project, don't forget to star it! ⭐**

**Made with ❤️ by QNT SOFTWARE**

</div>