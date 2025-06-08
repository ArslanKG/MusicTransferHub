# 🎵 TuneSync - Playlist Transfer API

## 🚨 **GÜVENLİK UYARISI**

**⚠️ Bu repository PUBLIC olarak GitHub'da tutulabilir, ancak:**

- ✅ **appsettings.json** dosyasında API anahtarları boş bırakılmıştır
- ✅ **.gitignore** hassas dosyaları ignore eder
- ⚠️ **Production'da Environment Variables kullanın**
- ⚠️ **API anahtarlarınızı asla commit etmeyin**

### 📋 **Güvenli Deployment:**
1. **Render.com Environment Variables** kullanarak API anahtarlarını ekleyin
2. **Local development** için `.env` dosyası oluşturun (git'e eklenmez)
3. **API anahtarları** sadece production ortamında environment variables olarak

---

Spotify playlist'lerinizi YouTube Music'e transfer eden web uygulaması.

## 🚀 Render.com Deployment

### 1. API Anahtarları Alma

#### Spotify API
1. [Spotify Developer Dashboard](https://developer.spotify.com/dashboard)'a gidin
2. Yeni uygulama oluşturun
3. `Client ID` ve `Client Secret` alın
4. Redirect URI: `https://tunesync.onrender.com/api/auth/spotify/callback`

#### YouTube API
1. [Google Cloud Console](https://console.cloud.google.com/)'a gidin
2. Yeni proje oluşturun veya mevcut projeyi seçin
3. YouTube Data API v3'ü etkinleştirin
4. OAuth 2.0 Client ID oluşturun (Web application)
5. API Key oluşturun
6. Redirect URI: `https://tunesync.onrender.com/api/auth/youtube/callback`

### 2. Render.com Environment Variables

Dashboard → Environment bölümünde şu değişkenleri ayarlayın:

```
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://+:10000
SPOTIFY_CLIENT_ID=your_spotify_client_id
SPOTIFY_CLIENT_SECRET=your_spotify_client_secret
YOUTUBE_CLIENT_ID=your_youtube_client_id
YOUTUBE_CLIENT_SECRET=your_youtube_client_secret
YOUTUBE_API_KEY=your_youtube_api_key
```

### 3. Build & Deploy Ayarları

- **Repository**: `https://github.com/ArslanKG/MusicTransferHub`
- **Branch**: `main`
- **Root Directory**: `PlaylistTransferAPI`
- **Build Command**: `dotnet build`
- **Start Command**: `dotnet PlaylistTransferAPI.dll`

### 4. Dockerfile Kullanımı

Render.com otomatik olarak `Dockerfile`'ı algılar ve kullanır.

## 🛠️ Development

### Local Development

```bash
# Dependencies
dotnet restore

# Run application
dotnet run
```

Uygulama `http://localhost:5285` adresinde çalışacak.

### Environment Variables (Development)

Development için `appsettings.Development.json` kullanın:

```json
{
  "Spotify": {
    "ClientId": "your_dev_spotify_client_id",
    "RedirectUri": "http://localhost:5285/api/auth/spotify/callback"
  },
  "YouTube": {
    "ClientId": "your_dev_youtube_client_id",
    "RedirectUri": "http://localhost:5285/api/auth/youtube/callback"
  }
}
```

## 📋 Features

- ✅ Spotify OAuth authentication
- ✅ YouTube Music OAuth authentication
- ✅ Playlist browsing and selection
- ✅ Intelligent track matching algorithm
- ✅ Real-time transfer progress
- ✅ Failed tracks reporting with categories
- ✅ Rate limiting and caching
- ✅ Production-ready security
- ✅ Mock implementation for API quota management
- ✅ Health monitoring

## 🔧 Configuration

### Rate Limiting

Production ayarları:
```json
{
  "RateLimit": {
    "PermitLimit": 50,
    "WindowInMinutes": 1
  }
}
```

### Cache Settings

```json
{
  "Cache": {
    "SearchCacheMinutes": 120,
    "PlaylistCacheMinutes": 20
  }
}
```

## 📊 Monitoring

### Health Check
```bash
curl https://tunesync.onrender.com/health
```

### Render.com Logs
Dashboard → Logs bölümünden real-time logları görüntüleyin.

## 🔒 Security

- HTTPS zorlamalı
- Security headers (HSTS, X-Frame-Options, etc.)
- Rate limiting
- Input validation
- Environment-based configuration

## 🐛 Troubleshooting

### Common Issues

1. **API Key Issues**
   - Render.com Dashboard → Environment'da değişkenleri kontrol edin
   - API key'lerin doğru set edildiğinden emin olun

2. **OAuth Callback Issues**
   - Spotify/YouTube console'larda redirect URI'ları kontrol edin
   - `https://tunesync.onrender.com/api/auth/*/callback` formatında olmalı

3. **Build Issues**
   - Render.com Logs'ta build hatalarını kontrol edin
   - `PlaylistTransferAPI` root directory'sinin doğru set edildiğinden emin olun

## 📞 Support

- GitHub Repository: [MusicTransferHub](https://github.com/ArslanKG/MusicTransferHub)
- Live Demo: [https://tunesync.onrender.com](https://tunesync.onrender.com)

## 📄 License

MIT License - Detaylar için LICENSE dosyasına bakın.