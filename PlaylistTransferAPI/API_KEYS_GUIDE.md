# 🔑 API Anahtarları Alma Rehberi

Bu rehber, Spotify ve YouTube Music Playlist Transfer uygulaması için gerekli API anahtarlarını nasıl alacağınızı adım adım açıklar.

## 🎵 SPOTIFY WEB API ANAHTARLARI

### 1. Spotify Developer Hesabı Oluşturma

1. **Spotify Developer Dashboard'a gidin:**
   - https://developer.spotify.com/dashboard

2. **Giriş yapın:**
   - Spotify hesabınızla giriş yapın
   - Hesabınız yoksa önce Spotify'da hesap oluşturun

3. **Developer hesabı onaylayın:**
   - Terms of Service'i kabul edin
   - Developer olarak kaydolun

### 2. Spotify App Oluşturma

1. **"Create app" butonuna tıklayın**

2. **App bilgilerini doldurun:**
   ```
   App name: Playlist Transfer API
   App description: Transfer playlists from Spotify to YouTube Music
   Website: http://localhost:5285 (veya kendi domain'iniz)
   Redirect URI: http://localhost:5285/api/auth/spotify/callback
   ```

3. **API/SDKs seçin:**
   - ✅ Web API seçeneğini işaretleyin

4. **"Save" butonuna tıklayın**

### 3. Spotify Anahtarlarını Alma

1. **Oluşturduğunuz app'e tıklayın**

2. **"Settings" butonuna tıklayın**

3. **Anahtarları kopyalayın:**
   ```
   Client ID: [Burada görünecek - kopyalayın]
   Client Secret: [Show client secret'a tıklayın ve kopyalayın]
   ```

4. **Redirect URIs kontrolü:**
   - `http://localhost:5285/api/auth/spotify/callback` ekli olmalı
   - Yoksa "Edit Settings" → "Redirect URIs" → Ekleyin

---

## 🎬 YOUTUBE DATA API V3 ANAHTARLARI

### 1. Google Cloud Console Hesabı

1. **Google Cloud Console'a gidin:**
   - https://console.cloud.google.com/

2. **Google hesabınızla giriş yapın**

3. **Terms of Service'i kabul edin**

### 2. Yeni Proje Oluşturma

1. **Üst menüden proje seçiciye tıklayın**

2. **"NEW PROJECT" butonuna tıklayın**

3. **Proje bilgilerini doldurun:**
   ```
   Project name: Playlist Transfer API
   Location: No organization (veya varsa organization'ınız)
   ```

4. **"CREATE" butonuna tıklayın**

5. **Proje oluşturulduktan sonra seçin**

### 3. YouTube Data API v3'ü Etkinleştirme

1. **Sol menüden "APIs & Services" → "Library"**

2. **"YouTube Data API v3" arayın**

3. **YouTube Data API v3'e tıklayın**

4. **"ENABLE" butonuna tıklayın**

### 4. OAuth 2.0 Credentials Oluşturma

1. **Sol menüden "APIs & Services" → "Credentials"**

2. **"+ CREATE CREDENTIALS" → "OAuth client ID"**

3. **OAuth consent screen yapılandırması (ilk kez ise):**
   ```
   User Type: External (kişisel kullanım için)
   App name: Playlist Transfer API
   User support email: [e-mail adresiniz]
   Developer contact information: [e-mail adresiniz]
   ```

4. **OAuth client ID oluşturma:**
   ```
   Application type: Web application
   Name: Playlist Transfer Web Client
   
   Authorized redirect URIs:
   http://localhost:5285/api/auth/youtube/callback
   ```

5. **"CREATE" butonuna tıklayın**

6. **Anahtarları kopyalayın:**
   ```
   Client ID: [Kopyalayın]
   Client secret: [Kopyalayın]
   ```

### 5. API Key Oluşturma

1. **"+ CREATE CREDENTIALS" → "API key"**

2. **API key oluşturulacak - kopyalayın**

3. **"RESTRICT KEY" (Önerilen):**
   ```
   API restrictions: YouTube Data API v3
   ```

---

## ⚙️ API ANAHTARLARINI UYGULAMAYA EKLEME

### 1. appsettings.json Güncelleme

```json
{
  "Spotify": {
    "ClientId": "SPOTIFY_CLIENT_ID_BURAYA",
    "ClientSecret": "SPOTIFY_CLIENT_SECRET_BURAYA",
    "RedirectUri": "http://localhost:5285/api/auth/spotify/callback",
    "BaseUrl": "https://api.spotify.com/v1"
  },
  "YouTube": {
    "ClientId": "YOUTUBE_CLIENT_ID_BURAYA",
    "ClientSecret": "YOUTUBE_CLIENT_SECRET_BURAYA", 
    "ApiKey": "YOUTUBE_API_KEY_BURAYA",
    "RedirectUri": "http://localhost:5285/api/auth/youtube/callback",
    "BaseUrl": "https://www.googleapis.com/youtube/v3"
  }
}
```

### 2. Güvenlik Notları

⚠️ **ÖNEMLİ GÜVENLİK UYARILARI:**

1. **Client Secret'ları asla paylaşmayın**
2. **GitHub'a commit etmeyin**
3. **appsettings.json'u .gitignore'a ekleyin**
4. **Production'da environment variables kullanın**

### 3. Environment Variables (Önerilen)

Production için:
```bash
export SPOTIFY_CLIENT_ID="your_spotify_client_id"
export SPOTIFY_CLIENT_SECRET="your_spotify_client_secret"
export YOUTUBE_CLIENT_ID="your_youtube_client_id"
export YOUTUBE_CLIENT_SECRET="your_youtube_client_secret"
export YOUTUBE_API_KEY="your_youtube_api_key"
```

---

## 🔄 TEST ETME

### 1. Anahtarları Ekledikten Sonra

```bash
cd PlaylistTransferAPI
dotnet run
```

### 2. Tarayıcıda Test

1. **http://localhost:5285** adresine gidin
2. **"Spotify'a Bağlan" butonuna tıklayın**
3. **Spotify yetkilendirme sayfası açılmalı**
4. **İzin verdikten sonra geri yönlendirilmeli**
5. **"YouTube'a Bağlan" için aynı süreci tekrarlayın**

---

## 🚨 SORUN GİDERME

### Spotify Sorunları

**Problem:** "Invalid client" hatası
**Çözüm:** Client ID ve Secret'ı tekrar kontrol edin

**Problem:** "Invalid redirect URI" hatası  
**Çözüm:** Redirect URI'nin tam olarak eşleştiğinden emin olun

### YouTube Sorunları

**Problem:** "Access blocked" hatası
**Çözüm:** OAuth consent screen'i yapılandırın

**Problem:** "API key restriction" hatası
**Çözüm:** API key kısıtlamalarını kontrol edin

### Genel Sorunlar

**Problem:** "Configuration missing" hatası
**Çözüm:** appsettings.json'da tüm değerlerin doğru olduğunu kontrol edin

---

## 💰 MALIYET BİLGİSİ

### Spotify Web API
- ✅ **Ücretsiz:** 100,000 istek/ay
- ✅ **Rate limit:** 100 istek/dakika
- ✅ **Commercial use:** İzin veriliyor

### YouTube Data API v3
- ✅ **Ücretsiz quota:** 10,000 units/gün
- ⚠️ **Maliyet:** Ek kullanım için $0.0002/unit
- ✅ **Tipik kullanım:** 1 transfer ≈ 5-20 units

---

## 🎯 SON ADIMLAR

1. ✅ Her iki API'den anahtarları alın
2. ✅ appsettings.json'u güncelleyin  
3. ✅ Uygulamayı yeniden başlatın
4. ✅ OAuth akışını test edin
5. 🎉 **Playlist transferini deneyin!**

**Not:** API anahtarları aldıktan sonra uygulama tam işlevsel olacak ve gerçek Spotify playlist'lerinizi YouTube Music'e transfer edebileceksiniz.