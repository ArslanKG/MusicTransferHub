# 🎵 TuneSync - Playlist Transfer API

A modern web application that transfers your Spotify playlists to YouTube Music seamlessly.

## 🚨 **SECURITY NOTICE**

**⚠️ This repository is safe for PUBLIC GitHub hosting:**

- ✅ **API keys are removed** from `appsettings.json`
- ✅ **Sensitive files** are protected by `.gitignore`
- ⚠️ **Use Environment Variables** in production
- ⚠️ **Never commit API keys** to version control

### 🔐 **Secure Deployment:**
1. **Use Render.com Environment Variables** for API keys
2. **Create `.env` file** for local development (git-ignored)
3. **API keys** should only exist as environment variables in production

---

## 🚀 **Quick Deploy to Render.com**

### 1. Get API Keys

#### Spotify API
1. Go to [Spotify Developer Dashboard](https://developer.spotify.com/dashboard)
2. Create a new application
3. Get `Client ID` and `Client Secret`
4. Add Redirect URI: `https://your-app.onrender.com/api/auth/spotify/callback`

#### YouTube API
1. Go to [Google Cloud Console](https://console.cloud.google.com/)
2. Create new project or select existing one
3. Enable YouTube Data API v3
4. Create OAuth 2.0 Client ID (Web application)
5. Create API Key
6. Add Redirect URI: `https://your-app.onrender.com/api/auth/youtube/callback`

### 2. Deploy on Render.com

#### Environment Variables
Set these in Render.com Dashboard → Environment:

```bash
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://+:10000

Spotify__ClientId=your_spotify_client_id
Spotify__ClientSecret=your_spotify_client_secret

YouTube__ClientId=your_youtube_client_id
YouTube__ClientSecret=your_youtube_client_secret
YouTube__ApiKey=your_youtube_api_key
```

#### Build Settings
- **Environment**: Docker
- **Root Directory**: `PlaylistTransferAPI`
- **Auto-Deploy**: Enabled

---

## 🛠️ **Local Development**

### Prerequisites
- .NET 9.0 SDK
- Valid Spotify and YouTube API credentials

### Setup

1. **Clone the repository**
   ```bash
   git clone https://github.com/ArslanKG/MusicTransferHub.git
   cd tunesync/PlaylistTransferAPI
   ```

2. **Restore dependencies**
   ```bash
   dotnet restore
   ```

3. **Configure API keys**
   
   Create `appsettings.Development.json`:
   ```json
   {
     "Spotify": {
       "ClientId": "your_spotify_client_id",
       "ClientSecret": "your_spotify_client_secret",
       "RedirectUri": "http://localhost:5285/api/auth/spotify/callback"
     },
     "YouTube": {
       "ClientId": "your_youtube_client_id",
       "ClientSecret": "your_youtube_client_secret",
       "ApiKey": "your_youtube_api_key",
       "RedirectUri": "http://localhost:5285/api/auth/youtube/callback"
     }
   }
   ```

4. **Run the application**
   ```bash
   dotnet run
   ```

   The app will be available at `http://localhost:5285`

---

## ✨ **Features**

- 🔐 **OAuth Authentication** - Secure login with Spotify and YouTube
- 📋 **Playlist Management** - Browse and select playlists
- 🎯 **Smart Matching** - Intelligent track matching algorithm
- 📊 **Real-time Progress** - Live transfer status updates
- 📈 **Detailed Reports** - Failed tracks with categorized reasons
- ⚡ **Performance** - Rate limiting and intelligent caching
- 🐳 **Production Ready** - Docker support and security headers
- 🔍 **Health Monitoring** - Built-in health checks

---

## 🏗️ **Architecture**

### Tech Stack
- **Backend**: ASP.NET Core 9.0
- **Architecture**: Stateless API (No Database)
- **Authentication**: OAuth 2.0 (Spotify & YouTube)
- **Caching**: In-Memory Cache
- **Logging**: Serilog
- **API Documentation**: Swagger/OpenAPI

### Project Structure
```
PlaylistTransferAPI/
├── Controllers/          # API endpoints
├── Services/            # Business logic
├── Models/              # Data models and DTOs
├── Data/                # Database context
├── wwwroot/             # Static files (frontend)
└── Dockerfile           # Container configuration
```

---

## ⚙️ **Configuration**

### Rate Limiting
```json
{
  "RateLimit": {
    "PermitLimit": 100,
    "WindowInMinutes": 1
  }
}
```

### Transfer Options
```json
{
  "Transfer": {
    "DefaultSearchResultLimit": 5,
    "DefaultMinMatchConfidence": 0.7,
    "DelayBetweenRequestsMs": 500,
    "BatchSize": 10
  }
}
```

### Cache Settings
```json
{
  "Cache": {
    "SearchCacheMinutes": 60,
    "PlaylistCacheMinutes": 10,
    "UserProfileCacheMinutes": 30
  }
}
```

---

## 🔍 **Monitoring & Health Checks**

### Health Endpoint
```bash
curl https://your-app.onrender.com/health
```

### API Documentation
Visit `/swagger` for interactive API documentation

### Logging
- **Development**: Console + File logs
- **Production**: Structured logging with Serilog

---

## 🛡️ **Security Features**

- **HTTPS Enforcement** - Redirects HTTP to HTTPS
- **Security Headers** - HSTS, X-Frame-Options, CSP
- **Rate Limiting** - Prevents API abuse
- **Input Validation** - Sanitized user inputs
- **OAuth 2.0** - Secure third-party authentication
- **Environment Variables** - Secure credential management

---

## 🐛 **Troubleshooting**

### Common Issues

**1. OAuth Callback Errors**
- Verify redirect URIs in Spotify/YouTube consoles
- Ensure URIs match exactly: `https://your-app.onrender.com/api/auth/{provider}/callback`

**2. API Rate Limits**
- YouTube API has daily quotas
- Implement exponential backoff for retries
- Monitor usage in Google Cloud Console

**3. Build Failures**
- Check Render.com build logs
- Verify `PlaylistTransferAPI` is set as root directory
- Ensure all dependencies are included

**4. Token Validation Issues**
- Check API key permissions
- Verify OAuth scopes are correct
- Ensure tokens haven't expired

---

## 🚀 **Deployment Options**

### Render.com (Recommended)
- Free tier available
- Automatic deploys from GitHub
- Built-in SSL certificates
- Environment variable management

### Docker
```bash
docker build -t tunesync .
docker run -p 10000:80 tunesync
```

### Manual Deployment
```bash
dotnet publish -c Release
# Deploy to your preferred hosting platform
```

---

## 🤝 **Contributing**

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests if applicable
5. Submit a pull request

---

## 📝 **License**

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

---

## 🌟 **Acknowledgments**

- [Spotify Web API](https://developer.spotify.com/documentation/web-api)
- [YouTube Data API v3](https://developers.google.com/youtube/v3)
- [ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core)

---

## 👨‍💻 **Developer**

**Arslan Kemal Gündüz**
- 🌐 Portfolio: [arkegu-portfolio.vercel.app](https://arkegu-portfolio.vercel.app/)
- 💼 LinkedIn: [Arslan Kemal Gündüz](https://www.linkedin.com/in/arslan-kemal-gunduz/)

---

**⭐ If this project helped you, please give it a star on GitHub!**