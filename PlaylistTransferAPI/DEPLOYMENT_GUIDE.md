# 🚀 Production Deployment Rehberi

Bu rehber, Playlist Transfer API'sini production ortamına deploy etmek için farklı seçenekleri açıklar.

## 📋 Deployment Seçenekleri

### 🌐 **Option 1: Azure App Service (Önerilen - Microsoft)**

#### **Avantajlar:**
- ✅ .NET için optimize edilmiş
- ✅ SSL sertifikası otomatik
- ✅ Kolay environment variables yönetimi
- ✅ $200 ücretsiz kredit (yeni hesaplar)

#### **Adımlar:**
1. **Azure hesabı oluşturun:** https://azure.microsoft.com/free/
2. **Azure CLI yükleyin:** https://docs.microsoft.com/cli/azure/install-azure-cli
3. **Deploy komutları:**
   ```bash
   # Login
   az login
   
   # Resource group oluştur
   az group create --name PlaylistTransferRG --location "East US"
   
   # App Service plan oluştur (ücretsiz tier)
   az appservice plan create --name PlaylistTransferPlan --resource-group PlaylistTransferRG --sku FREE
   
   # Web app oluştur
   az webapp create --resource-group PlaylistTransferRG --plan PlaylistTransferPlan --name playlist-transfer-api-[UNIQUE_NAME] --runtime "DOTNET|9.0"
   
   # Deploy
   cd PlaylistTransferAPI
   dotnet publish -c Release
   az webapp deploy --resource-group PlaylistTransferRG --name playlist-transfer-api-[UNIQUE_NAME] --src-path bin/Release/net9.0/publish --type zip
   ```

4. **URL'niz:** `https://playlist-transfer-api-[UNIQUE_NAME].azurewebsites.net`

---

### ⚡ **Option 2: Render.com (ÖNERİLEN - En İyi)**

#### **Avantajlar:**
- ✅ **Çok kolay deployment**
- ✅ **GitHub integration**
- ✅ **750 saat ücretsiz/ay** (Railway'den daha fazla)
- ✅ **Otomatik HTTPS ve CDN**
- ✅ **Çok stabil ve hızlı**
- ✅ **Environment variables kolay yönetim**

#### **Adımlar:**
1. **GitHub'a push edin:** Projenizi GitHub repository'sine yükleyin
2. **Render hesabı:** https://render.com/ - GitHub ile giriş yapın
3. **New → Web Service**
4. **Connect repository'nizi seçin**
5. **Build ayarları:**
   ```
   Name: playlist-transfer-api
   Environment: Docker
   Branch: main
   Root Directory: PlaylistTransferAPI (eğer alt klasörde ise)
   ```
6. **Environment Variables ekleyin:**
   ```
   ASPNETCORE_ENVIRONMENT=Production
   ASPNETCORE_URLS=http://+:10000
   Spotify__ClientId=YOUR_SPOTIFY_CLIENT_ID
   Spotify__ClientSecret=YOUR_SPOTIFY_CLIENT_SECRET
   YouTube__ClientId=YOUR_YOUTUBE_CLIENT_ID
   YouTube__ClientSecret=YOUR_YOUTUBE_CLIENT_SECRET
   YouTube__ApiKey=YOUR_YOUTUBE_API_KEY
   ```
7. **Create Web Service** - Deploy otomatik başlar

8. **URL'niz:** `https://playlist-transfer-api.onrender.com`

---

### 🐳 **Option 3: Railway (Kolay + Ücretsiz)**

#### **Avantajlar:**
- ✅ Çok kolay deployment
- ✅ GitHub integration
- ✅ $5 ücretsiz kredit/ay
- ✅ Otomatik HTTPS

#### **Adımlar:**
1. **GitHub'a push edin:** Projenizi GitHub repository'sine yükleyin
2. **Railway hesabı:** https://railway.app/ - GitHub ile giriş yapın
3. **New Project → Deploy from GitHub repo**
4. **Repository'nizi seçin**
5. **Environment Variables ekleyin:**
   ```
   ASPNETCORE_ENVIRONMENT=Production
   Spotify__ClientId=YOUR_SPOTIFY_CLIENT_ID
   Spotify__ClientSecret=YOUR_SPOTIFY_CLIENT_SECRET
   YouTube__ClientId=YOUR_YOUTUBE_CLIENT_ID
   YouTube__ClientSecret=YOUR_YOUTUBE_CLIENT_SECRET
   YouTube__ApiKey=YOUR_YOUTUBE_API_KEY
   ```
6. **Deploy otomatik başlar**

7. **URL'niz:** `https://[PROJECT_NAME].railway.app`

---

### 🔮 **Option 3: Heroku**

#### **Adımlar:**
1. **Heroku hesabı:** https://heroku.com
2. **Heroku CLI yükleyin**
3. **Deploy:**
   ```bash
   # Login
   heroku login
   
   # App oluştur
   heroku create playlist-transfer-api-[UNIQUE_NAME]
   
   # Buildpack ayarla
   heroku buildpacks:set mcr.microsoft.com/dotnet/sdk:9.0
   
   # Environment variables
   heroku config:set ASPNETCORE_ENVIRONMENT=Production
   heroku config:set Spotify__ClientId=YOUR_VALUE
   heroku config:set Spotify__ClientSecret=YOUR_VALUE
   heroku config:set YouTube__ClientId=YOUR_VALUE
   heroku config:set YouTube__ClientSecret=YOUR_VALUE
   heroku config:set YouTube__ApiKey=YOUR_VALUE
   
   # Deploy
   git push heroku main
   ```

4. **URL'niz:** `https://playlist-transfer-api-[UNIQUE_NAME].herokuapp.com`

---

### 🌊 **Option 4: DigitalOcean App Platform**

#### **Adımlar:**
1. **DigitalOcean hesabı:** https://digitalocean.com
2. **Apps → Create App**
3. **GitHub repository'nizi bağlayın**
4. **App info:**
   ```
   Name: playlist-transfer-api
   Region: New York
   Branch: main
   Source Directory: /PlaylistTransferAPI
   ```
5. **Environment Variables ekleyin**
6. **Create App**

7. **URL'niz:** `https://playlist-transfer-api-[HASH].ondigitalocean.app`

---

### 🐧 **Option 5: Kendi VPS/Sunucu (Docker)**

#### **Adımlar:**
1. **Docker yükleyin**
2. **Build ve Run:**
   ```bash
   # Build image
   cd PlaylistTransferAPI
   docker build -t playlist-transfer-api .
   
   # Run container
   docker run -d \
     --name playlist-transfer \
     -p 80:8080 \
     -e ASPNETCORE_ENVIRONMENT=Production \
     -e Spotify__ClientId=YOUR_VALUE \
     -e Spotify__ClientSecret=YOUR_VALUE \
     -e YouTube__ClientId=YOUR_VALUE \
     -e YouTube__ClientSecret=YOUR_VALUE \
     -e YouTube__ApiKey=YOUR_VALUE \
     playlist-transfer-api
   ```

3. **Domain bağlayın ve SSL sertifikası ekleyin (Cloudflare/Let's Encrypt)**

---

## ⚙️ Production Configuration

### **Environment Variables**
Production'da bu değişkenleri ayarlayın:

```bash
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://+:8080

# Spotify
Spotify__ClientId=YOUR_SPOTIFY_CLIENT_ID
Spotify__ClientSecret=YOUR_SPOTIFY_CLIENT_SECRET
Spotify__RedirectUri=https://YOUR_DOMAIN.com/api/auth/spotify/callback

# YouTube  
YouTube__ClientId=YOUR_YOUTUBE_CLIENT_ID
YouTube__ClientSecret=YOUR_YOUTUBE_CLIENT_SECRET
YouTube__ApiKey=YOUR_YOUTUBE_API_KEY
YouTube__RedirectUri=https://YOUR_DOMAIN.com/api/auth/youtube/callback

# Database (production'da external database önerilir)
ConnectionStrings__DefaultConnection=Data Source=/app/data/playlist_transfer.db

# Logging
Serilog__MinimumLevel__Default=Information
```

### **Production appsettings.json**
```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft.AspNetCore": "Warning"
      }
    }
  },
  "AllowedHosts": "*",
  "Demo": {
    "Enabled": false
  }
}
```

---

## 🔐 OAuth Redirect URIs

Deploy ettikten sonra gerçek URL'nizi alıp şu adresleri güncelleyin:

### **Spotify Developer Dashboard**
- **Settings → Redirect URIs:**
  ```
  https://YOUR_DOMAIN.com/api/auth/spotify/callback
  ```

### **Google Cloud Console**
- **Credentials → OAuth 2.0 Client → Authorized redirect URIs:**
  ```
  https://YOUR_DOMAIN.com/api/auth/youtube/callback
  ```

---

## 🎯 Deployment Sonrası Test

1. **Health Check:** `https://YOUR_DOMAIN.com/health`
2. **API Docs:** `https://YOUR_DOMAIN.com/swagger`  
3. **Main App:** `https://YOUR_DOMAIN.com`
4. **OAuth Test:** Spotify ve YouTube bağlantılarını test edin

---

## 💰 Maliyet Karşılaştırması

| Platform | Ücretsiz Tier | Aylık Maliyet | SSL | Custom Domain |
|----------|---------------|---------------|-----|---------------|
| **Railway** | $5 kredit | $0-20 | ✅ | ✅ |
| **Azure** | $200 kredit | $13+ | ✅ | ✅ |
| **Heroku** | 1000 saat | $7+ | ✅ | ✅ |
| **DigitalOcean** | $200 kredit | $12+ | ✅ | ✅ |
| **VPS** | - | $5+ | Manuel | ✅ |

---

## 🚨 Production Checklist

- [ ] Environment variables doğru ayarlandı
- [ ] OAuth redirect URIs güncellendi  
- [ ] HTTPS aktif
- [ ] Database production-ready (external DB önerilir)
- [ ] Logging yapılandırıldı
- [ ] Rate limiting test edildi
- [ ] Error monitoring eklendi (isteğe bağlı)
- [ ] Backup stratejisi planlandı

---

## 🎉 Sonuç

**En Kolay:** Railway (GitHub integration + otomatik deployment)
**En Güçlü:** Azure App Service (.NET için optimize)
**En Ucuz:** Railway veya kendi VPS

Hangi seçeneği tercih ederseniz edin, deploy sonrası gerçek URL'nizi alıp OAuth ayarlarını güncelleyebiliriz!