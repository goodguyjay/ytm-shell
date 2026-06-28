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

        const thumbs = data.thumbnail?.thumbnails ?? [];
        const best = thumbs.reduce((a, b) =>
            b.width * b.height > a.width * a.height ? b : a, thumbs[0] ?? {});

        window.chrome.webview.postMessage({
            type: 'trackChange',
            title: data.title,
            artist: data.author ?? '',
            thumbnailUrl: best?.url ?? '',
            durationSeconds: parseInt(data.lengthSeconds ?? '0')
        });
    });

    return true;
}