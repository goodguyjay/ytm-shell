window.__ytmControls = {
    _bar: () => document.querySelector('ytmusic-player-bar'),

    isPlaying() {
        const api = document.querySelector('ytmusic-player')?.playerApi;
        return api?.getPlayerState?.() === 1;
    },

    togglePlayPause() {
        const btn = this._bar()
            ?.querySelector('yt-icon-button#play-pause-button button');
        btn?.click();
    },

    previous() {
        const btn = this._bar()
            ?.querySelector('yt-icon-button.previous-button button');
        btn?.click();
    },

    next() {
        const btn = this._bar()
            ?.querySelector('yt-icon-button.next-button button');
        btn?.click();
    },
};

function setupPlayerStateListener() {
    const api = document.querySelector('ytmusic-player')?.playerApi;
    if (!api) return false;

    api.addEventListener('onStateChange', (state) => {
        // 1 = playing, 2 = paused, 3 = buffering
        const playing = state === 1;
        window.chrome.webview.postMessage(
            JSON.stringify({ type: 'playState', playing })
        );
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