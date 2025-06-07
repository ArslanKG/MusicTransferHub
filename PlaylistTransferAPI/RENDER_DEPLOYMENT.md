# 🚀 Render.com Deployment Rehberi

## ⚡ Render.com ile Hızlı Deployment

### 🎯 **Adım 1: GitHub Repository Hazırla**

1. **Projeyi GitHub'a push edin:**
   ```bash
   git init
   git add .
   git commit -m "Initial commit: Playlist Transfer API"
   git branch -M main
   git remote add origin https://github.com/YOURUSERNAME/playlist-transfer-api.git
   git push -u origin main
   ```

### 🌐 **Adım 2: Render.com'da Deployment**

1. **Render hesabı oluşturun:** https://render.com
2. **GitHub ile giriş yapın**
3. **Dashboard → New → Web Service**
4. **Connect GitHub repository**
5. **Repository'nizi seçin: `playlist-transfer-api`**

### ⚙️ **Adım 3: Build Ayarları**

```
Name: playlist-transfer-api
Environment: Docker
Region: Frankfurt (Avrupa için) veya Oregon (Genel)
Branch: main
Root Directory: ./PlaylistTransferAPI (eğer alt klasör ise)
```

### 🔧 **Adım 4: Environment Variables**

**Environment Variables** sekmesinde şunları ekleyin:

```bash
# Required for Render
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://+:10000

# Bu değerleri gerçek API anahtarlarınızla değiştirin
Spotify__ClientId=YOUR_SPOTIFY_CLIENT_ID
Spotify__ClientSecret=YOUR_SPOTIFY_CLIENT_SECRET
Spotify__RedirectUri=https://playlist-transfer-api.onrender.com/api/auth/spotify/callback

YouTube__ClientId=YOUR_YOUTUBE_CLIENT_ID
YouTube__ClientSecret=YOUR_YOUTUBE_CLIENT_SECRET
YouTube__ApiKey=YOUR_YOUTUBE_API_KEY
YouTube__RedirectUri=https://playlist-transfer-api.onrender.com/api/auth/youtube/callback

# Database (SQLite file için)
ConnectionStrings__DefaultConnection=Data Source=/app/data/playlist_transfer.db

# Logging
Serilog__MinimumLevel__Default=Information
```

### 🚀 **Adım 5: Deploy**

1. **"Create Web Service"** butonuna tıklayın
2. **Build otomatik başlar** (5-10 dakika sürer)
3. **URL'niz:** `https://playlist-transfer-api.onrender.com`

### 🎯 **Adım 6: OAuth Redirect URIs Güncellemesi**

Deploy tamamlandıktan sonra:

#### **Spotify Developer Dashboard:**
1. https://developer.spotify.com/dashboard
2. App'inizi seçin → Settings
3. **Redirect URIs** bölümüne ekleyin:
   ```
   https://playlist-transfer-api.onrender.com/api/auth/spotify/callback
   ```

#### **Google Cloud Console:**
1. https://console.cloud.google.com
2. APIs & Services → Credentials
3. OAuth 2.0 Client'ınızı seçin
4. **Authorized redirect URIs** bölümüne ekleyin:
   ```
   https://playlist-transfer-api.onrender.com/api/auth/youtube/callback
   ```

---

## ✅ **Test Etme**

Deploy tamamlandıktan sonra test edin:

1. **Ana sayfa:** https://playlist-transfer-api.onrender.com
2. **API Docs:** https://playlist-transfer-api.onrender.com/swagger
3. **Health Check:** https://playlist-transfer-api.onrender.com/health (eğer health endpoint'i varsa)

### 🎵 **OAuth Test:**
1. **"Spotify'a Bağlan"** → Spotify authorization page
2. **İzin ver** → Geri yönlendirme
3. **"YouTube'a Bağlan"** → YouTube authorization page  
4. **İzin ver** → Geri yönlendirme
5. **Playlist transfer test!**

---

## 🔄 **Otomatik Re-deployment**

Render otomatik olarak GitHub'daki değişiklikleri takip eder:

- ✅ **GitHub'a her push** → Otomatik re-deployment
- ✅ **Build logs** → Render dashboard'da izlenebilir
- ✅ **Rollback** → Önceki versiyona dönüş mümkün

---

## 💰 **Render.com Ücretsiz Tier**

- ✅ **750 saat/ay** (31 günde yaklaşık 24 saat/gün)
- ✅ **Otomatik HTTPS**
- ✅ **Custom domain** (ücretsiz)
- ✅ **GitHub integration**
- ⚠️ **Sleep after 15 min inactivity** (ücretsiz tier)
- ⚠️ **Cold start** ~ 30 saniye (ilk istek)

### **Upgrade ($7/ay):**
- ✅ **Always on** (sleep yok)
- ✅ **Instant cold starts**
- ✅ **More resources**

---

## 🎯 **Deployment Checklist**

- [ ] GitHub repository hazır
- [ ] Render.com hesabı oluşturuldu
- [ ] Web service yapılandırıldı
- [ ] Environment variables eklendi
- [ ] Build başarılı
- [ ] URL'e erişim sağlandı
- [ ] Spotify OAuth redirect URI güncellendi
- [ ] YouTube OAuth redirect URI güncellendi
- [ ] OAuth akışı test edildi
- [ ] Playlist transfer test edildi

---

## 🚨 **Troubleshooting**

### **Build Fails:**
- Dockerfile syntax kontrol edin
- GitHub repository'de PlaylistTransferAPI klasörü doğru yerde mi?

### **503 Error:**
- Environment variables doğru mu?
- Port 10000 kullanıldığından emin olun

### **OAuth Hatası:**
- Redirect URIs tam olarak eşleşiyor mu?
- https:// kullanıldığından emin olun

### **Database Hatası:**
- SQLite dosyası yazılabilir klasörde mi? (`/app/data/`)
- Connection string doğru mu?

---

## 🎉 **Sonuç**

Render.com deployment tamamlandı! Artık:

✅ **Production URL'niz var**
✅ **HTTPS otomatik aktif**
✅ **OAuth redirect URIs güncellendi**
✅ **Otomatik deployment GitHub'dan**

**URL'niz:** `https://playlist-transfer-api.onrender.com`

Artık gerçek API anahtarlarını ekleyip uygulamayı tam işlevsel hale getirebilirsiniz! 🚀