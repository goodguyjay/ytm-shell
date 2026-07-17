window.__ytmControls = {
    _bar: () => document.querySelector('ytmusic-player-bar'),
    _api: () => document.querySelector('ytmusic-player')?.playerApi,

    isPlaying() {
        return this._api()?.getPlayerState?.() === 1;
    },

    togglePlayPause() {
        const api = this._api();
        if (!api) 
            return;
        api.getPlayerState?.() === 1 ? api.pauseVideo?.() : api.playVideo?.();
    },

    previous() {
        this._bar()?.querySelector('yt-icon-button.previous-button button')?.click();
    },

    next() {
        this._bar()?.querySelector('yt-icon-button.next-button button')?.click();
    },
};

if ('mediaSession' in navigator) {
    navigator.mediaSession.setActionHandler('play', () => window.__ytmControls.togglePlayPause());
    navigator.mediaSession.setActionHandler('pause', () => window.__ytmControls.togglePlayPause());
    navigator.mediaSession.setActionHandler('previoustrack', () => window.__ytmControls.previous());
    navigator.mediaSession.setActionHandler('nexttrack', () => window.__ytmControls.next());
}

document.addEventListener('keydown', (e) => {
    if (e.code !== 'Space') return;
    const tag = e.target?.tagName;
    if (tag === 'INPUT' || tag === 'TEXTAREA' || e.target?.isContentEditable) return;
    e.preventDefault();
    e.stopPropagation();
    window.__ytmControls.togglePlayPause();
}, true);

