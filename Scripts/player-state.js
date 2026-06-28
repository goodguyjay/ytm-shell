function setupPlayerStateListener() {
    const api = document.querySelector('ytmusic-player')?.playerApi;
    if (!api) return false;

    api.addEventListener('onStateChange', (state) => {
        window.chrome.webview.postMessage(JSON.stringify({
            type: 'playState',
            playing: state === 1,
            state: state  // 1=playing, 2=paused, 3=buffering, 0=ended
        }));
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