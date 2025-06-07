# 🎵 Spotify to YouTube Music Playlist Transfer API

Modern, secure, and feature-rich ASP.NET Core Web API for transferring playlists from Spotify to YouTube Music with intelligent track matching algorithms.

![License](https://img.shields.io/badge/license-MIT-blue.svg)
![.NET](https://img.shields.io/badge/.NET-9.0-purple.svg)
![Platform](https://img.shields.io/badge/platform-Windows%20%7C%20macOS%20%7C%20Linux-lightgrey.svg)

## ✨ Features

### 🔐 Secure Authentication
- **OAuth 2.0 Integration** for both Spotify and YouTube APIs
- **CSRF Protection** with state parameters  
- **Secure Token Management** with session storage
- **Auto-refresh Mechanism** for expired tokens

### 🧠 Smart Track Matching
- **Fuzzy String Matching** using Levenshtein distance algorithm
- **Multi-query Search Strategy** (Artist + Title combinations)
- **Duration Tolerance Matching** (±10% variance allowed)
- **Confidence Scoring System** (configurable threshold)
- **Intelligent Retry Logic** for failed matches

### 📊 Real-time Monitoring
- **Progress Tracking** with live updates
- **Transfer Analytics** and success rate statistics  
- **Detailed Error Reporting** with failure reasons
- **Transfer History** with searchable logs

### 🚀 Production Ready
- **Rate Limiting** compliance with API quotas
- **Memory Caching** for optimal performance
- **Structured Logging** with Serilog
- **SQLite Database** for persistence
- **Swagger Documentation** with OpenAPI 3.0
- **Responsive Web UI** with Bootstrap 5

## 🛠️ Technology Stack

- **Backend:** ASP.NET Core 9.0 Web API
- **Database:** SQLite with Entity Framework Core
- **Authentication:** OAuth 2.0 (Spotify Web API + YouTube Data API v3)
- **Caching:** In-Memory Cache
- **Logging:** Serilog with file and console outputs
- **Documentation:** Swagger/OpenAPI
- **Frontend:** HTML5, CSS3, JavaScript ES6+, Bootstrap 5
- **Testing:** Built-in validation and error handling

## 📁 Project Structure

```
PlaylistTransferAPI/
├── Controllers/
│   ├── AuthController.cs           # OAuth 2.0 authentication
│   ├── PlaylistController.cs       # Playlist operations
│   └── AnalyticsController.cs      # Transfer analytics
├── Services/
│   ├── Interfaces/                 # Service contracts
│   ├── SpotifyService.cs           # Spotify Web API integration
│   ├── YouTubeService.cs           # YouTube Data API integration
│   └── TransferService.cs          # Core transfer logic
├── Models/
│   ├── DTOs/                       # Data transfer objects
│   ├── Entities/                   # Database entities
│   └── Responses/                  # API response models
├── Data/
│   └── ApplicationDbContext.cs     # Entity Framework context
├── wwwroot/
│   ├── index.html                  # Modern responsive UI
│   ├── css/                        # Custom styles
│   └── js/app.js                   # OAuth-enabled frontend
└── Middleware/                     # Custom middleware
```

## 🚀 Quick Start

### Prerequisites

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download)
- [Spotify Developer Account](https://developer.spotify.com/)
- [Google Cloud Console Account](https://console.cloud.google.com/)

### 1. Clone the Repository

```bash
git clone https://github.com/yourusername/playlist-transfer-api.git
cd playlist-transfer-api/PlaylistTransferAPI
```

### 2. Get API Keys

**📖 Detailed guide:** See [`API_KEYS_GUIDE.md`](API_KEYS_GUIDE.md) for step-by-step instructions.

**Quick overview:**
- **Spotify:** Create app at [Spotify Developer Dashboard](https://developer.spotify.com/dashboard)
- **YouTube:** Enable YouTube Data API v3 at [Google Cloud Console](https://console.cloud.google.com/)

### 3. Configure API Keys

Update `appsettings.json`:

```json
{
  "Spotify": {
    "ClientId": "YOUR_SPOTIFY_CLIENT_ID",
    "ClientSecret": "YOUR_SPOTIFY_CLIENT_SECRET",
    "RedirectUri": "http://localhost:5285/api/auth/spotify/callback"
  },
  "YouTube": {
    "ClientId": "YOUR_YOUTUBE_CLIENT_ID",
    "ClientSecret": "YOUR_YOUTUBE_CLIENT_SECRET",
    "ApiKey": "YOUR_YOUTUBE_API_KEY",
    "RedirectUri": "http://localhost:5285/api/auth/youtube/callback"
  }
}
```

### 4. Run the Application

```bash
dotnet restore
dotnet run
```

### 5. Access the Application

- **Web UI:** http://localhost:5285
- **API Documentation:** http://localhost:5285/swagger
- **Health Check:** http://localhost:5285/health

## 🎯 Usage Flow

1. **🔑 Authentication**
   - Click "Connect to Spotify" → OAuth authorization → Auto token retrieval
   - Click "Connect to YouTube" → OAuth authorization → Auto token retrieval

2. **📋 Playlist Selection**
   - Browse your Spotify playlists
   - Select playlist to transfer
   - Preview track count and details

3. **⚙️ Transfer Configuration**
   - Set new playlist name
   - Add description (optional)
   - Choose privacy settings (public/private)
   - Configure advanced matching options

4. **🚀 Transfer Execution**
   - Real-time progress tracking
   - Live statistics (successful/failed tracks)
   - ETA calculation

5. **📊 Results & Analytics**
   - Transfer completion summary
   - Failed tracks with reasons
   - Direct link to YouTube Music playlist
   - Transfer history and analytics

## 📡 API Endpoints

### Authentication
```http
GET  /api/auth/spotify/authorize     # Initiate Spotify OAuth
GET  /api/auth/spotify/callback      # Handle Spotify callback
GET  /api/auth/youtube/authorize     # Initiate YouTube OAuth  
GET  /api/auth/youtube/callback      # Handle YouTube callback
```

### Playlist Operations
```http
GET  /api/playlist/spotify/user-playlists           # Get user playlists
GET  /api/playlist/spotify/{playlistId}             # Get playlist details
POST /api/playlist/transfer                         # Start transfer
GET  /api/playlist/transfer-status/{transferId}     # Check progress
GET  /api/playlist/transfer-history                 # Get transfer history
POST /api/playlist/cancel-transfer/{transferId}     # Cancel transfer
```

### Analytics
```http
GET  /api/analytics/transfer-stats                  # Transfer statistics
GET  /api/analytics/popular-playlists               # Most transferred playlists
GET  /api/analytics/success-rate                    # Overall success rate
```

## ⚙️ Configuration Options

### Transfer Settings
```json
{
  "Transfer": {
    "DefaultSearchResultLimit": 5,        # YouTube search results per track
    "DefaultMinMatchConfidence": 0.7,     # Minimum match confidence (0.0-1.0)
    "DefaultMaxRetryAttempts": 3,         # Retry attempts for failed tracks
    "DelayBetweenRequestsMs": 500,        # Rate limiting delay
    "BatchSize": 10                       # Tracks processed per batch
  }
}
```

### Caching Configuration
```json
{
  "Cache": {
    "DefaultExpirationMinutes": 15,       # Default cache expiration
    "PlaylistCacheMinutes": 10,           # Playlist data cache
    "SearchCacheMinutes": 60,             # Search results cache
    "UserProfileCacheMinutes": 30         # User profile cache
  }
}
```

## 🔒 Security Features

- **OAuth 2.0 Compliance** with PKCE support
- **CSRF Protection** using state parameters
- **Rate Limiting** (100 requests/minute/IP)
- **Input Validation** and sanitization
- **SQL Injection Prevention** with parameterized queries
- **XSS Protection** headers
- **Secure Token Storage** (never logged)
- **HTTPS Enforcement** in production

## 📈 Performance Optimizations

- **Async/Await Pattern** throughout the application
- **Memory Caching** for API responses
- **Connection Pooling** for database operations
- **Batch Processing** for multiple track operations
- **Circuit Breaker Pattern** for external API calls
- **Compression** for API responses
- **CDN-ready** static assets

## 🧪 Algorithm Details

### Track Matching Process

1. **Query Generation**
   ```
   Primary: "Artist - Track Title"
   Secondary: "Track Title Artist"  
   Fallback: "Track Title"
   ```

2. **Similarity Scoring**
   ```
   Title Match: 50% weight
   Artist Match: 40% weight  
   Duration Match: 10% weight
   ```

3. **Confidence Calculation**
   ```
   Levenshtein Distance + Duration Tolerance + Popularity Score
   Final Score: 0.0 (no match) to 1.0 (perfect match)
   ```

## 📊 Monitoring & Analytics

### Logged Metrics
- Transfer success/failure rates
- Average processing time per track
- API response times and errors
- User engagement patterns
- Popular playlist categories

### Database Schema
```sql
TransferLogs: Id, UserId, SpotifyPlaylistId, YouTubePlaylistId, Status, 
              CreatedAt, CompletedAt, TotalTracks, SuccessfulTracks, FailedTracks

FailedTracks: Id, TransferLogId, TrackName, Artist, FailureReason, AttemptedAt

UserSessions: Id, UserId, SpotifyToken, YouTubeToken, CreatedAt, ExpiresAt
```

## 🚨 Troubleshooting

### Common Issues

**"Invalid client" error:**
- Verify Client ID and Secret in appsettings.json
- Check OAuth app configuration

**"Invalid redirect URI" error:**
- Ensure redirect URIs match exactly in OAuth app settings
- Check for trailing slashes and protocol (http vs https)

**"Rate limit exceeded" error:**
- Application automatically handles rate limiting
- Consider increasing delay between requests in configuration

**"Track not found" errors:**
- Adjust `MinMatchConfidence` threshold
- Enable `UseAlbumInSearch` option for better matching

### Debug Mode

Enable detailed logging:
```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Debug"
    }
  }
}
```

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🙏 Acknowledgments

- [Spotify Web API](https://developer.spotify.com/documentation/web-api/) for playlist data
- [YouTube Data API v3](https://developers.google.com/youtube/v3) for video search and playlist creation
- [ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core/) for the robust web framework
- [Entity Framework Core](https://docs.microsoft.com/en-us/ef/core/) for data persistence
- [Serilog](https://serilog.net/) for structured logging
- [Bootstrap](https://getbootstrap.com/) for responsive UI components

## 📞 Support

For support, email support@playlisttransfer.com or create an issue in this repository.

---

**⭐ If this project helped you, please give it a star!**