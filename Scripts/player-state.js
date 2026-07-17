function setupPlayerStateListener() {
    const api = document.querySelector('ytmusic-player')?.playerApi;
    if (!api) return false;

    api.addEventListener('onStateChange', (state) => {
        window.chrome.webview.postMessage({ type: 'playState', playing: state === 1, state });

        if (state === 1) {
            startPositionUpdates(api);
        } else {
            stopPositionUpdates();
        }
    });

    return true;
}

function waitForPlayer() {
    if (setupPlayerStateListener()) return;

    const observer = new MutationObserver(() => {
        if (setupPlayerStateListener())
            observer.disconnect();
    });

    observer.observe(document.body, {
        childList: true,
        subtree: true
    });
}

if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', waitForPlayer);
} else {
    waitForPlayer();
}

function setupPlayerStateListener() {
    const api = document.querySelector('ytmusic-player')?.playerApi;
    if (!api) return false;

    api.addEventListener('onStateChange', (state) => {
        window.chrome.webview.postMessage({
            type: 'playState', playing: state === 1, state: state
        });
    });

    api.addEventListener('onVideoDataChange', () => {
        const data = api.getVideoData?.();
        if (!data?.title) return;
        const videoId = data.video_id ?? data.videoId;
        const thumbImg = document.querySelector('img.image.style-scope.ytmusic-player-bar');
        let thumbnailUrl = '';
        if (thumbImg?.src) {
            thumbnailUrl = thumbImg.src.replace(/=w\d+-h\d+[^"]*$/, '=w512-h512-l90-rj');
        }
        if (!thumbnailUrl && videoId) {
            thumbnailUrl = `https://i.ytimg.com/vi/${videoId}/hqdefault.jpg`;
        }

        if ('mediaSession' in navigator) {
            navigator.mediaSession.metadata = new MediaMetadata({
                title: data.title,
                artist: data.author ?? '',
                artwork: thumbnailUrl ? [{ src: thumbnailUrl, sizes: '512x512', type: 'image/jpeg' }] : []
            });
        }

        window.chrome.webview.postMessage({
            type: 'trackChange',
            title: data.title,
            artist: data.author ?? '',
            thumbnailUrl: thumbnailUrl,
            durationSeconds: parseInt(data.lengthSeconds ?? '0')
        });
    });

    return true;
}

let __positionInterval = null;

function stopPositionUpdates() {
    if (__positionInterval) {
        clearInterval(__positionInterval);
        __positionInterval = null;
    }
}

function startPositionUpdates(api) {
    stopPositionUpdates();c
    __positionInterval = setInterval(() => {
        if (!('mediaSession' in navigator)) return;
        const duration = api.getDuration?.() ?? 0;
        const position = api.getCurrentTime?.() ?? 0;
        if (duration > 0) {
            navigator.mediaSession.setPositionState({ duration, position, playbackRate: 1 });
        }
    }, 1000);
}