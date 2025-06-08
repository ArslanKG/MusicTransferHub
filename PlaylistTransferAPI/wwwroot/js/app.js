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
    checkDevelopmentMode();
});

function initializeEventListeners() {
    // Auth buttons
    document.getElementById('spotifyLogin').addEventListener('click', handleSpotifyLogin);
    document.getElementById('youtubeLogin').addEventListener('click', handleYouTubeLogin);
    
    // Transfer form
    document.getElementById('transferForm').addEventListener('submit', handleTransferSubmit);
    
    // Cache temizleme butonu
    const clearCacheBtn = document.getElementById('clearCacheBtn');
    if (clearCacheBtn) {
        clearCacheBtn.addEventListener('click', clearCache);
    }
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
    const spotifyCard = document.getElementById('spotifyCard');
    const youtubeCard = document.getElementById('youtubeCard');
    const spotifyConnectionStatus = document.getElementById('spotifyConnectionStatus');
    const youtubeConnectionStatus = document.getElementById('youtubeConnectionStatus');
    const spotifyInfo = document.getElementById('spotifyInfo');
    const youtubeInfo = document.getElementById('youtubeInfo');
    
    if (spotifyToken) {
        spotifyStatus.innerHTML = '<span class="badge bg-success status-badge"><i class="bi bi-check-circle-fill"></i> Bağlı</span>';
        spotifyButton.innerHTML = '<i class="bi bi-check-circle-fill"></i> Bağlandı';
        spotifyButton.disabled = true;
        spotifyButton.classList.remove('btn-spotify');
        spotifyButton.classList.add('btn-outline-success');
        spotifyCard.classList.remove('pending');
        spotifyCard.classList.add('connected');
        spotifyConnectionStatus.classList.add('connected');
        spotifyInfo.classList.add('d-none');
    } else {
        spotifyStatus.innerHTML = '<span class="badge bg-secondary status-badge"><i class="bi bi-x-circle"></i> Bağlı değil</span>';
        spotifyButton.innerHTML = '<i class="bi bi-box-arrow-in-right"></i> Spotify\'a Bağlan';
        spotifyButton.disabled = false;
        spotifyButton.classList.add('btn-spotify');
        spotifyButton.classList.remove('btn-outline-success');
        spotifyCard.classList.add('pending');
        spotifyCard.classList.remove('connected');
        spotifyConnectionStatus.classList.remove('connected');
        spotifyInfo.classList.remove('d-none');
    }
    
    if (youtubeToken) {
        youtubeStatus.innerHTML = '<span class="badge bg-success status-badge"><i class="bi bi-check-circle-fill"></i> Bağlı</span>';
        youtubeButton.innerHTML = '<i class="bi bi-check-circle-fill"></i> Bağlandı';
        youtubeButton.disabled = true;
        youtubeButton.classList.remove('btn-youtube');
        youtubeButton.classList.add('btn-outline-success');
        youtubeCard.classList.remove('pending');
        youtubeCard.classList.add('connected');
        youtubeConnectionStatus.classList.add('connected');
        youtubeInfo.classList.add('d-none');
    } else {
        youtubeStatus.innerHTML = '<span class="badge bg-secondary status-badge"><i class="bi bi-x-circle"></i> Bağlı değil</span>';
        youtubeButton.innerHTML = '<i class="bi bi-box-arrow-in-right"></i> YouTube\'a Bağlan';
        youtubeButton.disabled = false;
        youtubeButton.classList.add('btn-youtube');
        youtubeButton.classList.remove('btn-outline-success');
        youtubeCard.classList.add('pending');
        youtubeCard.classList.remove('connected');
        youtubeConnectionStatus.classList.remove('connected');
        youtubeInfo.classList.remove('d-none');
    }
    
    updateStepIndicator();
    
    // Show ready indicator and load playlists if both are connected
    if (spotifyToken && youtubeToken) {
        document.getElementById('readyIndicator').classList.remove('d-none');
        loadUserPlaylists();
    } else {
        document.getElementById('readyIndicator').classList.add('d-none');
        document.getElementById('playlistSection').classList.add('d-none');
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
    updateStepIndicator();
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
    
    // Show transfer section
    document.getElementById('transferSection').classList.remove('d-none');
    
    updateStepIndicator();
    
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
            searchResultLimit: 3,
            minMatchConfidence: 0.6,
            maxRetryAttempts: 2,
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
    
    updateStepIndicator();
    
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
            
            if (transfer.status === 2 || transfer.status === 3 || transfer.status === 4) {
                clearInterval(pollInterval);
                showResults(transfer);
                return;
            }
            
            const stats = transfer.statistics;
            const total = stats.totalTracks || 0;
            const processed = (stats.successfulTracks || 0) + (stats.failedTracks || 0);
            if (total > 0 && processed >= total) {
                clearInterval(pollInterval);
                showResults(transfer);
                return;
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
        <div class="card mb-4 border-0 shadow-sm">
            <div class="card-body text-center py-4">
                <div class="mb-3">
                    <span class="badge bg-${transfer.success ? 'success' : 'danger'} p-3 rounded-circle">
                        <i class="bi bi-${transfer.success ? 'check-lg' : 'x-lg'} fs-4"></i>
                    </span>
                </div>
                <h4 class="mb-1">${transfer.success ? 'Transfer Başarılı' : 'Transfer Başarısız'}</h4>
                <p class="text-muted">${escapeHtml(transfer.message || '')}</p>
            </div>
        </div>
        
        <div class="row g-3 mb-4">
            <div class="col-md-4">
                <div class="card h-100 border-0 shadow-sm">
                    <div class="card-body text-center">
                        <div class="h2 text-info mb-1">${stats.totalTracks || 0}</div>
                        <div class="text-muted small">Toplam Şarkı</div>
                    </div>
                </div>
            </div>
            <div class="col-md-4">
                <div class="card h-100 border-0 shadow-sm">
                    <div class="card-body text-center">
                        <div class="h2 text-success mb-1">${stats.successfulTracks || 0}</div>
                        <div class="text-muted small">Başarılı</div>
                    </div>
                </div>
            </div>
            <div class="col-md-4">
                <div class="card h-100 border-0 shadow-sm">
                    <div class="card-body text-center">
                        <div class="h2 text-danger mb-1">${stats.failedTracks || 0}</div>
                        <div class="text-muted small">Başarısız</div>
                    </div>
                </div>
            </div>
        </div>
    `;
    
    if (transfer.youTubePlaylistUrl) {
        resultHtml += `
            <div class="text-center mb-4">
                <a href="${transfer.youTubePlaylistUrl}" target="_blank" class="btn btn-youtube btn-lg">
                    <i class="bi bi-youtube"></i> YouTube Music Playlist'i Aç
                </a>
            </div>
        `;
    }
    
    if (transfer.failedTracks && Array.isArray(transfer.failedTracks) && transfer.failedTracks.length > 0) {
        const tracksByCategory = {};
        const failedTracksToShow = transfer.failedTracks;
        
        failedTracksToShow.forEach((track, idx) => {
            const category = getFailureCategory(track.failureReason);
            if (!tracksByCategory[category]) {
                tracksByCategory[category] = [];
            }
            tracksByCategory[category].push(track);
        });
        
        resultHtml += `
            <div class="mt-4 mb-4">
                <div class="card border-0 shadow-sm">
                    <div class="card-header bg-light">
                        <div class="d-flex align-items-center">
                            <div class="me-2">
                                <span class="badge rounded-pill bg-warning text-dark fs-6">
                                    ${transfer.failedTracks.length}
                                </span>
                            </div>
                            <h5 class="mb-0">
                                <i class="bi bi-exclamation-triangle-fill text-warning me-2"></i>
                                Aktarılamayan Şarkılar
                            </h5>
                        </div>
                    </div>
                    <div class="list-group list-group-flush">
        `;
        
        // Her kategori için şarkıları göster
        Object.keys(tracksByCategory).forEach((category, categoryIndex) => {
            const tracks = tracksByCategory[category];
            const categoryColor = getFailureColor(tracks[0].failureReason);
            const categoryIcon = getFailureIcon(tracks[0].failureReason);
            
            resultHtml += `
                <div class="list-group-item list-group-item-action p-0 border-start-0 border-end-0">
                    <button class="btn btn-link w-100 text-start p-3 text-decoration-none"
                            data-bs-toggle="collapse" data-bs-target="#category${categoryIndex}">
                        <div class="d-flex align-items-center">
                            <i class="bi ${categoryIcon} text-${categoryColor} me-2"></i>
                            <span class="me-auto">${category} şarkılar (${tracks.length})</span>
                            <i class="bi bi-chevron-down ms-2"></i>
                        </div>
                    </button>
                    
                    <div id="category${categoryIndex}" class="collapse">
                        <div class="px-3 pb-2">
                            <div class="table-responsive">
                                <table class="table table-sm table-hover">
                                    <thead class="table-light text-muted small">
                                        <tr>
                                            <th>Şarkı</th>
                                            <th>Sanatçı</th>
                                            <th>Hata Sebebi</th>
                                        </tr>
                                    </thead>
                                    <tbody class="small">
            `;
            
            tracks.forEach(track => {
                resultHtml += `
                    <tr>
                        <td class="text-nowrap text-truncate" style="max-width: 200px;">
                            ${escapeHtml(track.trackName || 'Bilinmeyen Şarkı')}
                        </td>
                        <td class="text-nowrap text-truncate" style="max-width: 150px;">
                            ${escapeHtml(track.artist || 'Bilinmeyen Sanatçı')}
                        </td>
                        <td>
                            <span class="text-${categoryColor}">${escapeHtml(track.failureReason || 'Bilinmeyen Hata')}</span>
                        </td>
                    </tr>
                `;
            });
            
            resultHtml += `
                                    </tbody>
                                </table>
                            </div>
                            <div class="text-center mt-2 mb-1">
                                <small class="text-muted">
                                    ${getFailureSuggestion(tracks[0].failureReason)}
                                </small>
                            </div>
                        </div>
                    </div>
                </div>
            `;
        });
        
        resultHtml += `
                    </div>
                    <div class="card-footer bg-light">
                        <small class="text-muted">
                            <i class="bi bi-info-circle"></i>
                            Aktarılamayan şarkılar genellikle YouTube Music'te bulunamama veya
                            telif hakkı kısıtlamaları nedeniyle oluşur.
                        </small>
                    </div>
                </div>
            </div>
        `;
    }
    
    // Add new transfer button
    resultHtml += `
        <div class="text-center mt-4">
            <button onclick="location.reload()" class="btn btn-primary btn-lg">
                <i class="bi bi-arrow-clockwise"></i> Yeni Transfer Başlat
            </button>
        </div>
    `;
    
    resultsContent.innerHTML = resultHtml;
    
    updateStepIndicator();
    
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
}

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

function updateStepIndicator() {
    const step1 = document.getElementById('step1');
    const step2 = document.getElementById('step2');
    const step3 = document.getElementById('step3');
    const step4 = document.getElementById('step4');
    
    // Reset all steps
    [step1, step2, step3, step4].forEach(step => {
        step.classList.remove('active', 'completed', 'pending');
        step.classList.add('pending');
    });
    
    if (!spotifyToken || !youtubeToken) {
        step1.classList.remove('pending');
        step1.classList.add('active');
    } else if (!selectedPlaylist) {
        step1.classList.remove('pending');
        step1.classList.add('completed');
        step2.classList.remove('pending');
        step2.classList.add('active');
    } else if (!document.getElementById('progressSection').classList.contains('d-none')) {
        step1.classList.remove('pending');
        step1.classList.add('completed');
        step2.classList.remove('pending');
        step2.classList.add('completed');
        step3.classList.remove('pending');
        step3.classList.add('active');
    } else if (!document.getElementById('resultsSection').classList.contains('d-none')) {
        [step1, step2, step3].forEach(step => {
            step.classList.remove('pending', 'active');
            step.classList.add('completed');
        });
        step4.classList.remove('pending');
        step4.classList.add('active');
    } else if (selectedPlaylist) {
        step1.classList.remove('pending');
        step1.classList.add('completed');
        step2.classList.remove('pending');
        step2.classList.add('completed');
        step3.classList.remove('pending');
        step3.classList.add('active');
    }
}

function getFailureIcon(reason) {
    if (!reason) return 'bi-exclamation-circle';
    
    const reasonLower = reason.toLowerCase();
    
    if (reasonLower.includes('not found') || reasonLower.includes('bulunamadı') ||
        reasonLower.includes('suitable match') || reasonLower.includes('no match')) {
        return 'bi-search';
    } else if (reasonLower.includes('copyright') || reasonLower.includes('telif')) {
        return 'bi-shield-exclamation';
    } else if (reasonLower.includes('quota') || reasonLower.includes('limit')) {
        return 'bi-speedometer2';
    } else if (reasonLower.includes('network') || reasonLower.includes('ağ')) {
        return 'bi-wifi-off';
    } else {
        return 'bi-exclamation-circle';
    }
}

function getFailureColor(reason) {
    if (!reason) return 'danger';
    
    const reasonLower = reason.toLowerCase();
    
    if (reasonLower.includes('not found') || reasonLower.includes('bulunamadı') ||
        reasonLower.includes('suitable match') || reasonLower.includes('no match')) {
        return 'warning';
    } else if (reasonLower.includes('copyright') || reasonLower.includes('telif')) {
        return 'danger';
    } else if (reasonLower.includes('quota') || reasonLower.includes('limit')) {
        return 'info';
    } else if (reasonLower.includes('network') || reasonLower.includes('ağ')) {
        return 'secondary';
    } else {
        return 'danger';
    }
}

function getFailureCategory(reason) {
    if (!reason) return 'Bilinmeyen';
    
    const reasonLower = reason.toLowerCase();
    
    if (reasonLower.includes('not found') || reasonLower.includes('bulunamadı') ||
        reasonLower.includes('suitable match') || reasonLower.includes('no match')) {
        return 'Bulunamadı';
    } else if (reasonLower.includes('copyright') || reasonLower.includes('telif')) {
        return 'Telif Hakkı';
    } else if (reasonLower.includes('quota') || reasonLower.includes('limit')) {
        return 'API Sınırı';
    } else if (reasonLower.includes('network') || reasonLower.includes('ağ')) {
        return 'Ağ Hatası';
    } else {
        return 'Diğer';
    }
}

function getFailureSuggestion(reason) {
    if (!reason) return '<i class="bi bi-question-circle text-muted"></i> Beklenmeyen hata oluştu';
    
    const reasonLower = reason.toLowerCase();
    
    if (reasonLower.includes('not found') || reasonLower.includes('bulunamadı') ||
        reasonLower.includes('suitable match') || reasonLower.includes('no match')) {
        return '<i class="bi bi-lightbulb text-warning"></i> Bu şarkı YouTube Music\'te farklı isimle aranabilir';
    } else if (reasonLower.includes('copyright') || reasonLower.includes('telif')) {
        return '<i class="bi bi-shield-check text-danger"></i> Telif hakkı nedeniyle erişim kısıtlı';
    } else if (reasonLower.includes('quota') || reasonLower.includes('limit')) {
        return '<i class="bi bi-clock text-info"></i> API sınırı aşıldı, daha sonra tekrar deneyin';
    } else if (reasonLower.includes('network') || reasonLower.includes('ağ')) {
        return '<i class="bi bi-arrow-clockwise text-secondary"></i> İnternet bağlantısını kontrol edin';
    } else {
        return '<i class="bi bi-question-circle text-muted"></i> Beklenmeyen hata oluştu';
    }
}

async function clearCache() {
    const clearCacheBtn = document.getElementById('clearCacheBtn');
    
    try {
        clearCacheBtn.disabled = true;
        clearCacheBtn.innerHTML = '<i class="bi bi-hourglass-split"></i> Temizleniyor...';
        
        localStorage.clear();
        sessionStorage.clear();
        
        if ('serviceWorker' in navigator) {
            const registrations = await navigator.serviceWorker.getRegistrations();
            for (let registration of registrations) {
                await registration.unregister();
            }
        }
        
        if ('caches' in window) {
            const cacheNames = await caches.keys();
            await Promise.all(cacheNames.map(cacheName => caches.delete(cacheName)));
        }
        
        showSuccess('Browser cache temizlendi! Sayfa yenileniyor...');
        
        clearCacheBtn.innerHTML = '<i class="bi bi-check-circle-fill"></i> Temizlendi!';
        clearCacheBtn.style.background = 'linear-gradient(45deg, #28a745, #20c997)';
        
        setTimeout(() => {
            window.location.reload(true);
        }, 2000);
        
    } catch (error) {
        showError('Cache temizleme sırasında hata oluştu: ' + error.message);
        
        clearCacheBtn.innerHTML = '<i class="bi bi-trash"></i> Cache Temizle';
        clearCacheBtn.style.background = '';
        clearCacheBtn.disabled = false;
    }
}

function checkDevelopmentMode() {
    const clearCacheBtn = document.getElementById('clearCacheBtn');
    
    const isDevelopment =
        window.location.href.includes('127.0.0.1:5285') ||
        window.location.href.includes('localhost:5285') ||
        window.location.href.includes('localhost:5000') ||
        window.location.href.includes('localhost:5001');
    
    if (!isDevelopment && clearCacheBtn) {
        clearCacheBtn.style.display = 'none';
    }
}