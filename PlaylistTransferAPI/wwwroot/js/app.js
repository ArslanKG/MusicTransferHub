// Playlist Transfer API Frontend
let spotifyToken = '';
let youtubeToken = '';
let selectedPlaylist = null;
let transferId = '';

// API Base URL
const API_BASE = '/api';

// Initialize the application
document.addEventListener('DOMContentLoaded', function() {
    initializeEventListeners();
    checkUrlParameters();
    checkAuthStatus();
});

function initializeEventListeners() {
    // Auth buttons
    document.getElementById('spotifyLogin').addEventListener('click', handleSpotifyLogin);
    document.getElementById('youtubeLogin').addEventListener('click', handleYouTubeLogin);
    
    // Transfer form
    document.getElementById('transferForm').addEventListener('submit', handleTransferSubmit);
}

function checkUrlParameters() {
    // Check if we have tokens in URL parameters (from OAuth callback)
    const urlParams = new URLSearchParams(window.location.search);
    
    // Spotify tokens
    const spotifyAccessToken = urlParams.get('spotify_access_token');
    if (spotifyAccessToken) {
        spotifyToken = spotifyAccessToken;
        localStorage.setItem('spotify_token', spotifyAccessToken);
        
        const refreshToken = urlParams.get('spotify_refresh_token');
        if (refreshToken) {
            localStorage.setItem('spotify_refresh_token', refreshToken);
        }
        
        // Clean URL
        window.history.replaceState({}, document.title, window.location.pathname);
        showSuccess('Spotify başarıyla bağlandı!');
    }
    
    // YouTube tokens
    const youtubeAccessToken = urlParams.get('youtube_access_token');
    if (youtubeAccessToken) {
        youtubeToken = youtubeAccessToken;
        localStorage.setItem('youtube_token', youtubeAccessToken);
        
        const refreshToken = urlParams.get('youtube_refresh_token');
        if (refreshToken) {
            localStorage.setItem('youtube_refresh_token', refreshToken);
        }
        
        // Clean URL
        window.history.replaceState({}, document.title, window.location.pathname);
        showSuccess('YouTube Music başarıyla bağlandı!');
    }
    
    // Check for errors
    const error = urlParams.get('error');
    if (error) {
        const message = urlParams.get('message') || 'Bilinmeyen hata';
        showError(`Yetkilendirme hatası: ${message}`);
        // Clean URL
        window.history.replaceState({}, document.title, window.location.pathname);
    }
}

function checkAuthStatus() {
    // Check if tokens exist in localStorage
    spotifyToken = localStorage.getItem('spotify_token') || '';
    youtubeToken = localStorage.getItem('youtube_token') || '';
    
    updateAuthStatus();
    
    if (spotifyToken) {
        loadUserPlaylists();
    }
}

function updateAuthStatus() {
    const spotifyStatus = document.getElementById('spotifyStatus');
    const youtubeStatus = document.getElementById('youtubeStatus');
    const spotifyButton = document.getElementById('spotifyLogin');
    const youtubeButton = document.getElementById('youtubeLogin');
    
    if (spotifyToken) {
        spotifyStatus.innerHTML = '<span class="badge bg-success status-badge">✓ Bağlandı</span>';
        spotifyButton.innerHTML = '<i class="bi bi-check-circle"></i> Bağlı';
        spotifyButton.disabled = true;
        spotifyButton.classList.remove('btn-spotify');
        spotifyButton.classList.add('btn-outline-success');
    } else {
        spotifyStatus.innerHTML = '<span class="badge bg-secondary status-badge">Bağlı değil</span>';
        spotifyButton.innerHTML = '<i class="bi bi-box-arrow-in-right"></i> Spotify\'a Bağlan';
        spotifyButton.disabled = false;
        spotifyButton.classList.add('btn-spotify');
        spotifyButton.classList.remove('btn-outline-success');
    }
    
    if (youtubeToken) {
        youtubeStatus.innerHTML = '<span class="badge bg-danger status-badge">✓ Bağlandı</span>';
        youtubeButton.innerHTML = '<i class="bi bi-check-circle"></i> Bağlı';
        youtubeButton.disabled = true;
        youtubeButton.classList.remove('btn-youtube');
        youtubeButton.classList.add('btn-outline-danger');
    } else {
        youtubeStatus.innerHTML = '<span class="badge bg-secondary status-badge">Bağlı değil</span>';
        youtubeButton.innerHTML = '<i class="bi bi-box-arrow-in-right"></i> YouTube\'a Bağlan';
        youtubeButton.disabled = false;
        youtubeButton.classList.add('btn-youtube');
        youtubeButton.classList.remove('btn-outline-danger');
    }
    
    // Show transfer section if both are connected
    if (spotifyToken && youtubeToken) {
        document.getElementById('transferSection').classList.remove('d-none');
    } else {
        document.getElementById('transferSection').classList.add('d-none');
    }
}

async function handleSpotifyLogin() {
    try {
        showLoading('Spotify yetkilendirme URL\'si alınıyor...');
        
        const response = await fetch(`${API_BASE}/auth/spotify/authorize`);
        const result = await response.json();
        
        if (result.success && result.data.authUrl) {
            // Show confirmation modal
            if (confirm('Spotify\'a bağlanmak için yetkilendirme sayfasına yönlendirileceksiniz. Devam etmek istiyor musunuz?')) {
                // Redirect to Spotify authorization
                window.location.href = result.data.authUrl;
            }
        } else {
            throw new Error(result.message || 'Yetkilendirme URL\'si alınamadı');
        }
        
    } catch (error) {
        console.error('Spotify auth error:', error);
        showError('Spotify yetkilendirme başlatılamadı: ' + error.message);
    }
}

async function handleYouTubeLogin() {
    try {
        showLoading('YouTube yetkilendirme URL\'si alınıyor...');
        
        const response = await fetch(`${API_BASE}/auth/youtube/authorize`);
        const result = await response.json();
        
        if (result.success && result.data.authUrl) {
            // Show confirmation modal
            if (confirm('YouTube Music\'e bağlanmak için yetkilendirme sayfasına yönlendirileceksiniz. Devam etmek istiyor musunuz?')) {
                // Redirect to YouTube authorization
                window.location.href = result.data.authUrl;
            }
        } else {
            throw new Error(result.message || 'Yetkilendirme URL\'si alınamadı');
        }
        
    } catch (error) {
        console.error('YouTube auth error:', error);
        showError('YouTube yetkilendirme başlatılamadı: ' + error.message);
    }
}

async function loadUserPlaylists() {
    try {
        showLoading('Playlist\'ler yükleniyor...');
        
        const response = await fetch(`${API_BASE}/playlist/spotify/user-playlists`, {
            headers: {
                'spotifyAccessToken': spotifyToken
            }
        });
        
        if (!response.ok) {
            // Token might be expired
            if (response.status === 401) {
                localStorage.removeItem('spotify_token');
                spotifyToken = '';
                updateAuthStatus();
                showError('Spotify token\'ı geçersiz. Lütfen tekrar bağlanın.');
                return;
            }
            throw new Error('Playlist\'ler yüklenemedi');
        }
        
        const result = await response.json();
        displayPlaylists(result.data || []);
        
    } catch (error) {
        console.error('Error loading playlists:', error);
        showError('Playlist\'ler yüklenirken hata oluştu: ' + error.message);
    }
}

function displayPlaylists(playlists) {
    const playlistList = document.getElementById('playlistList');
    const playlistSection = document.getElementById('playlistSection');
    
    playlistList.innerHTML = '';
    
    if (playlists.length === 0) {
        playlistList.innerHTML = '<div class="col-12"><p class="text-muted text-center">Hiç playlist bulunamadı.</p></div>';
        return;
    }
    
    playlists.forEach(playlist => {
        const playlistCard = document.createElement('div');
        playlistCard.className = 'col-md-6 col-lg-4';
        playlistCard.innerHTML = `
            <div class="card playlist-card h-100" data-playlist-id="${playlist.id}">
                <div class="card-body">
                    <h6 class="card-title">${escapeHtml(playlist.name)}</h6>
                    <p class="card-text text-muted small">${playlist.tracks?.total || 0} şarkı</p>
                    <p class="card-text small">${escapeHtml(playlist.owner?.displayName || 'Bilinmeyen')}</p>
                    <div class="text-center">
                        <button class="btn btn-outline-primary btn-sm select-playlist-btn">
                            <i class="bi bi-check-circle"></i> Seç
                        </button>
                    </div>
                </div>
            </div>
        `;
        
        const selectBtn = playlistCard.querySelector('.select-playlist-btn');
        selectBtn.addEventListener('click', (e) => {
            e.stopPropagation();
            selectPlaylist(playlist);
        });
        
        playlistList.appendChild(playlistCard);
    });
    
    playlistSection.classList.remove('d-none');
}

function selectPlaylist(playlist) {
    selectedPlaylist = playlist;
    
    // Remove previous selection
    document.querySelectorAll('.playlist-card').forEach(card => {
        card.classList.remove('border-primary');
        const btn = card.querySelector('.select-playlist-btn');
        btn.innerHTML = '<i class="bi bi-check-circle"></i> Seç';
        btn.classList.remove('btn-primary');
        btn.classList.add('btn-outline-primary');
    });
    
    // Add selection to current card
    const selectedCard = document.querySelector(`[data-playlist-id="${playlist.id}"]`);
    if (selectedCard) {
        selectedCard.classList.add('border-primary');
        const btn = selectedCard.querySelector('.select-playlist-btn');
        btn.innerHTML = '<i class="bi bi-check-circle-fill"></i> Seçildi';
        btn.classList.remove('btn-outline-primary');
        btn.classList.add('btn-primary');
    }
    
    // Update form with playlist name
    document.getElementById('playlistName').value = playlist.name + ' (YouTube Music)';
    document.getElementById('playlistDescription').value = `Spotify'dan transfer edilen playlist: ${playlist.name}`;
    
    showSuccess(`"${playlist.name}" seçildi. Transfer ayarlarını yapıp başlatabilirsiniz.`);
    
    // Scroll to transfer section
    document.getElementById('transferSection').scrollIntoView({ behavior: 'smooth' });
}

async function handleTransferSubmit(event) {
    event.preventDefault();
    
    if (!selectedPlaylist) {
        showError('Lütfen önce bir playlist seçin.');
        return;
    }
    
    const transferData = {
        spotifyPlaylistId: selectedPlaylist.id,
        spotifyAccessToken: spotifyToken,
        youTubeAccessToken: youtubeToken,
        newPlaylistName: document.getElementById('playlistName').value,
        playlistDescription: document.getElementById('playlistDescription').value,
        makePublic: document.getElementById('playlistPrivacy').value === 'public',
        userId: 'demo-user-' + Date.now(),
        options: {
            skipDuplicates: true,
            useArtistInSearch: true,
            useAlbumInSearch: false,
            searchResultLimit: 5,
            minMatchConfidence: 0.7,
            maxRetryAttempts: 3,
            createPlaylistEvenIfEmpty: true
        }
    };
    
    try {
        showProgress();
        
        const response = await fetch(`${API_BASE}/playlist/transfer`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(transferData)
        });
        
        if (!response.ok) {
            const errorResult = await response.json();
            throw new Error(errorResult.message || 'Transfer başlatılamadı');
        }
        
        const result = await response.json();
        
        if (result.success) {
            transferId = result.data.transferId;
            startProgressPolling(transferId);
        } else {
            throw new Error(result.message || 'Transfer başarısız');
        }
        
    } catch (error) {
        console.error('Transfer error:', error);
        showError('Transfer başlatılırken hata oluştu: ' + error.message);
        hideProgress();
    }
}

function showProgress() {
    document.getElementById('progressSection').classList.remove('d-none');
    document.getElementById('transferSection').classList.add('d-none');
    
    // Reset progress
    document.getElementById('progressBar').style.width = '0%';
    document.getElementById('progressBar').textContent = '0%';
    document.getElementById('totalTracks').textContent = '0';
    document.getElementById('successTracks').textContent = '0';
    document.getElementById('failedTracks').textContent = '0';
    document.getElementById('progressText').textContent = 'Transfer başlatılıyor...';
    
    // Scroll to progress section
    document.getElementById('progressSection').scrollIntoView({ behavior: 'smooth' });
}

function hideProgress() {
    document.getElementById('progressSection').classList.add('d-none');
    document.getElementById('transferSection').classList.remove('d-none');
}

function startProgressPolling(transferId) {
    const pollInterval = setInterval(async () => {
        try {
            const response = await fetch(`${API_BASE}/playlist/transfer-status/${transferId}`);
            
            if (!response.ok) {
                throw new Error('Transfer durumu alınamadı');
            }
            
            const result = await response.json();
            const transfer = result.data;
            
            updateProgress(transfer);
            
            if (transfer.status === 'Completed' || transfer.status === 'Failed' || transfer.status === 'Cancelled') {
                clearInterval(pollInterval);
                showResults(transfer);
            }
            
        } catch (error) {
            console.error('Progress polling error:', error);
            clearInterval(pollInterval);
            showError('Transfer durumu alınırken hata oluştu');
            hideProgress();
        }
    }, 2000); // Poll every 2 seconds
}

function updateProgress(transfer) {
    const stats = transfer.statistics;
    const total = stats.totalTracks || 0;
    const successful = stats.successfulTracks || 0;
    const failed = stats.failedTracks || 0;
    const processed = successful + failed;
    
    const progressPercent = total > 0 ? Math.round((processed / total) * 100) : 0;
    
    document.getElementById('progressBar').style.width = progressPercent + '%';
    document.getElementById('progressBar').textContent = progressPercent + '%';
    
    document.getElementById('totalTracks').textContent = total;
    document.getElementById('successTracks').textContent = successful;
    document.getElementById('failedTracks').textContent = failed;
    
    document.getElementById('progressText').textContent = 
        `${processed}/${total} şarkı işlendi (${progressPercent}%)`;
}

function showResults(transfer) {
    document.getElementById('progressSection').classList.add('d-none');
    document.getElementById('resultsSection').classList.remove('d-none');
    
    const stats = transfer.statistics;
    const resultsContent = document.getElementById('resultsContent');
    
    let resultHtml = `
        <div class="alert alert-${transfer.success ? 'success' : 'danger'}" role="alert">
            <h6><i class="bi bi-${transfer.success ? 'check-circle' : 'x-circle'}"></i> 
                Transfer ${transfer.success ? 'Tamamlandı' : 'Başarısız'}</h6>
            <p class="mb-0">${escapeHtml(transfer.message || '')}</p>
        </div>
        
        <div class="row text-center mb-3">
            <div class="col-md-4">
                <div class="h3 text-info">${stats.totalTracks || 0}</div>
                <small>Toplam Şarkı</small>
            </div>
            <div class="col-md-4">
                <div class="h3 text-success">${stats.successfulTracks || 0}</div>
                <small>Başarılı</small>
            </div>
            <div class="col-md-4">
                <div class="h3 text-danger">${stats.failedTracks || 0}</div>
                <small>Başarısız</small>
            </div>
        </div>
    `;
    
    if (transfer.youTubePlaylistUrl) {
        resultHtml += `
            <div class="text-center mb-3">
                <a href="${transfer.youTubePlaylistUrl}" target="_blank" class="btn btn-youtube">
                    <i class="bi bi-youtube"></i> YouTube Music Playlist'i Aç
                </a>
            </div>
        `;
    }
    
    if (transfer.failedTracks && transfer.failedTracks.length > 0) {
        resultHtml += `
            <div class="mt-4">
                <h6>Başarısız Şarkılar:</h6>
                <div class="list-group list-group-flush">
        `;
        
        transfer.failedTracks.forEach(track => {
            resultHtml += `
                <div class="list-group-item">
                    <div class="d-flex w-100 justify-content-between">
                        <h6 class="mb-1">${escapeHtml(track.trackName)}</h6>
                        <small class="text-danger">${escapeHtml(track.failureReason)}</small>
                    </div>
                    <small class="text-muted">${escapeHtml(track.artist)}</small>
                </div>
            `;
        });
        
        resultHtml += '</div></div>';
    }
    
    // Add new transfer button
    resultHtml += `
        <div class="text-center mt-4">
            <button onclick="location.reload()" class="btn btn-primary">
                <i class="bi bi-arrow-clockwise"></i> Yeni Transfer Başlat
            </button>
        </div>
    `;
    
    resultsContent.innerHTML = resultHtml;
    
    // Scroll to results
    document.getElementById('resultsSection').scrollIntoView({ behavior: 'smooth' });
}

// Utility functions
function escapeHtml(text) {
    if (!text) return '';
    const div = document.createElement('div');
    div.textContent = text;
    return div.innerHTML;
}

function showError(message) {
    // Simple toast-like error display
    const toast = document.createElement('div');
    toast.className = 'position-fixed top-0 end-0 p-3';
    toast.style.zIndex = '9999';
    toast.innerHTML = `
        <div class="alert alert-danger alert-dismissible fade show" role="alert">
            <i class="bi bi-exclamation-triangle"></i> ${escapeHtml(message)}
            <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
        </div>
    `;
    document.body.appendChild(toast);
    
    setTimeout(() => {
        if (toast.parentNode) {
            toast.parentNode.removeChild(toast);
        }
    }, 5000);
}

function showSuccess(message) {
    const toast = document.createElement('div');
    toast.className = 'position-fixed top-0 end-0 p-3';
    toast.style.zIndex = '9999';
    toast.innerHTML = `
        <div class="alert alert-success alert-dismissible fade show" role="alert">
            <i class="bi bi-check-circle"></i> ${escapeHtml(message)}
            <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
        </div>
    `;
    document.body.appendChild(toast);
    
    setTimeout(() => {
        if (toast.parentNode) {
            toast.parentNode.removeChild(toast);
        }
    }, 3000);
}

function showLoading(message) {
    console.log('Loading:', message);
    // Could add a loading spinner here if needed
}

// Add logout functionality
function logoutSpotify() {
    localStorage.removeItem('spotify_token');
    localStorage.removeItem('spotify_refresh_token');
    spotifyToken = '';
    selectedPlaylist = null;
    document.getElementById('playlistSection').classList.add('d-none');
    document.getElementById('transferSection').classList.add('d-none');
    updateAuthStatus();
    showSuccess('Spotify bağlantısı kesildi');
}

function logoutYouTube() {
    localStorage.removeItem('youtube_token');
    localStorage.removeItem('youtube_refresh_token');
    youtubeToken = '';
    document.getElementById('transferSection').classList.add('d-none');
    updateAuthStatus();
    showSuccess('YouTube Music bağlantısı kesildi');
}