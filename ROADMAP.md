# Roadmap

Status of planned work across releases. Updated as things ship or get reprioritized.

---

## Pre-release

### Bugs
- [ ] Split-screen mode shows incorrect maximize icon state
- [ ] Three-dot menu is invisible due to transparency; needs stronger blur or contrast fallback
- [ ] Equalizer animation on the top bar is jittery and loops incorrectly; sync with play/pause state
- [ ] Home screen section labels need a UX pass
- [ ] Playlist menu buttons rendering as solid white; likely a liquid glass value overflow
- [ ] Artist pages have a broken layout; likely due to missing CSS rules for the new DOM structure

### Bug / Feature
- [ ] Spacebar behavior: should always toggle play/pause regardless of focused element

### Features
- [ ] Replace "YouTube Music" in the title bar with current track and artist
- [ ] Discord Rich Presence integration

### Nitpicks
- [ ] Revisit app icon
- [ ] Lock theme selector to debug builds only; other themes aren't release-ready

---

## Post-release alpha

### Bugs
- [ ] Double-click on title bar doesn't maximize; single click does

### Improvements
- [ ] Reduce blue intensity in liquid glass effect
- [ ] Native media key support (pause, next, previous, volume)
- [ ] Cache built CSS in `ThemeInjector`; invalidate only on theme change

### Features
- [ ] Polish non-default themes (Aurora, Mica, Depth) with distinct visual identities
- [ ] Persist last visited page across sessions
- [ ] Make CSS and JS injection more resilient to YouTube Music DOM updates
- [ ] Integrate title bar styling with the app's CSS system
- [ ] Sync equalizer animation with playback state
- [ ] Unit tests for complex subsystems

---

## Beta

### Bugs
- [ ] Inconsistent zoom/resolution state; triggered by resizing the window (low priority, documented)

### Features
- [ ] Collapse the YouTube Music sidebar (requires WebView investigation)
- [ ] Separate the player from the WebView for deeper Windows integration (PoC needed, high complexity)
- [ ] Full UI overhaul; first target: accessibility-focused theme as a design PoC
- [ ] Taskbar thumbnail: show album art instead of minimized window preview
- [ ] Separate builds: self-contained and installer

### Audio (PoC required)
- [ ] Audio stream interception via WASAPI
- [ ] Loudness normalization (RMS/LUFS-based)
- [ ] Equalizer
- [ ] Playback device detection; display current output device (e.g. "Playing on JBL GO")

---

## Unplanned / Exploring

- Synchronized lyrics; investigating what YouTube Music exposes internally; high value, unknown complexity
- Smart caching; low priority, unclear if necessary
- Windows audio device metadata via MMDevice API
- Internationalization; minimal hardcoded strings, low priority

---

## Known Inconsistencies (not actively fixing)

- Zoom state inconsistency when resizing; only reproducible in specific conditions, no current fix planned

---

## Notes

**On separate WebViews:** The idea of splitting the player from the main WebView is architecturally interesting but remains experimental for the foreseeable future. RAM overhead, fluidity impact, and failure modes under real usage are unknowns that unit and integration tests won't surface. Extensive real-world use is the only reliable signal.

**On CSS stability:** YouTube Music uses internal custom elements (`ytmusic-*`) with no public API contract. Selectors were documented manually via DevTools. Stability depends entirely on Google not changing their internal DOM structure.