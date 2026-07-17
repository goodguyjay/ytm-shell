# Roadmap

Status of planned work across releases. Updated as things ship or get reprioritized.

---

## Shipped

- [x] Fixed incorrect maximize icon state in split-screen mode
- [x] Fixed spacebar not reliably toggling play/pause
- [x] Discord Rich Presence integration
- [x] Title bar shows current track/artist instead of static "YouTube Music"
- [x] Back button added to the top bar
- [x] Fixed serrated/aliased text rendering (`TextRenderingMode="Grayscale"` + `TextFormattingMode="Display"`)
- [x] Settings page: implement custom top bar
- [x] Equalizer animation synced with actual playback state (currently loops regardless of play/pause)
- [x] SMTC integration (Windows lock screen / media overlay controls)
- [x] Taskbar preview overhaul: show album art instead of app window preview, and show "artist - song" in the preview title instead of "YouTube Music" when a track is playing (title logic already implemented elsewhere, needs wiring into the taskbar preview)

---

## Alpha exit criteria

The only things blocking GitHub release. Everything else is post-alpha.

- [ ] installer + updater

---

## Post-alpha musts

Flagship differentiators. No YTM client on the market does deep Windows integration or audio normalization; this is the whole pitch post-alpha.

- [ ] **Audio normalization** (RMS/LUFS). Flagship feature, but effort vs. impact needs evaluation against Last.fm and clipboard URL detection before locking in an approach (Web Audio API injection vs. WASAPI loopback).
- [ ] Last.fm scrobbling
- [ ] Clipboard URL detection (opt-in)
- [ ] Fix window preview on the taskbar, It's pretty inconsistent right now.
- [ ] Do something about quick track changes unnecessarily changing the taskbar album cover. I don't think anyone really appreciates unnecessary network requests (google specially).

---

## Windows integration differentiators (beta)

The deep-integration angle no competitor or Spotify can match.

- [ ] Taskbar preview with blur-fill
- [ ] Taskbar progress bar (`ITaskbarList3`)
- [ ] Jump lists (`ICustomDestinationList`)
- [ ] Wake lock during playback (`SetThreadExecutionState`)
- [ ] Title bar color sync with dominant page color (via JS postMessage; WPF can't overlay WebView2 directly due to airspace limitation)
- [ ] Separate the player from the WebView for deeper native integration (PoC needed, high complexity, backlog until there's bandwidth)
- [ ] Taskbar hover thumbnail shows album art instead of minimized window preview

---

## Discontinued (theming)

Zero further dev effort. CSS injection into YTM's DOM is inherently fragile and not worth maintaining. Toggle stays visible in settings, marked "discontinued / experimental."

- [x] ~~Liquid glass CSS theming~~ discontinued, kept as documented snapshot only
- [x] ~~Aurora, Mica, Depth, Cyberpunk themes~~ discontinued
- [x] ~~Full UI overhaul~~ out of scope entirely, not even a beta target anymore
- [x] ~~Reduce blue intensity in liquid glass~~ discontinued
- [x] ~~Integrate title bar styling with liquid glass CSS~~ discontinued

---

## Deprioritized / not fixing right now

Conscious calls, not forgotten bugs.

- [ ] ~~Three-dot expand menu nearly invisible due to transparency (accessibility issue); open to a stronger blur later, not active~~ (theming stuff is discontinued, so this is a non-issue)
- [ ] ~~Abrupt visual cut between liquid glass and player area~~ (theming stuff is discontinued, so this is a non-issue)
- [ ] ~~Top nav labels ("Podcasts", "Para treinar", "Energia"...) need a UX pass~~ (theming stuff is discontinued, so this is a non-issue)
- [ ] ~~Playlist menu buttons render solid white (liquid glass value overflow, low priority now that theming is discontinued)~~ (theming stuff is discontinued, so this is a non-issue)
- [ ] ~~Artist page cover art has a black bar artifact~~ (theming stuff is discontinued, so this is a non-issue)
- [ ] ~~Cache built CSS instead of rebuilding on every `BuildScriptAsync` call; no perceived fluidity loss despite SPA re-triggering it, so not urgent~~ (theming stuff is discontinued, so this is a non-issue)
- [ ] Memory leak (not yet confirmed. via `PrivateMemorySize64` growth on overnight suspension); investigation deferred to isolated VM with Edge installed, to isolate WebView2 Evergreen Runtime overhead from app code

---

## Nitpicks

- [ ] Revisit app icon;
- [ ] ~~Add a loading screen on startup~~ (not needed for now, startup is almost instant)

---

## Uncertain / needs design before committing

- [ ] Persist last visited page across sessions. Not as trivial as it sounds: need to decide where to store the state and confirm it doesn't add startup latency
- [ ] ~~Roboto font for visual consistency between WPF chrome and WebView2 content. In analysis;~~ Segoe UI already looks fine

---

## Other bugs (post-alpha)

- [ ] Double-click on title bar doesn't maximize; single click does
- [ ] Scrollbar eats screen space without slightly compressing the app layout
- [ ] Visible "black" artifact around the maximize icon
- [ ] Inconsistent zoom/resolution state triggered by window resizing (low priority, documented, see Known Inconsistencies)

---

## Beta (other)

- [ ] Collapse the YouTube Music sidebar (requires WebView investigation)
- [ ] Separate builds: self-contained and installer
- [ ] Native media key support (pause, next, previous, volume)
- [ ] Unit tests for complex subsystems, especially audio normalization and Windows integration
- [ ] Make JS injection more resilient to YouTube Music DOM updates (functional injection, not theming)
- [ ] Playback device detection; display current output device (e.g. "Playing on JBL GO")

---

## Unplanned / exploring

- Synchronized lyrics; investigating what YouTube Music exposes internally, unknown complexity
- Smart caching; low priority, unclear if necessary
- Internationalization; minimal hardcoded strings, low priority

---

## Known inconsistencies (not actively fixing)

- Zoom state inconsistency when resizing; only reproducible in specific conditions, no fix planned

---

## Notes

**On separate WebViews:** splitting the player from the main WebView is architecturally interesting but stays experimental for the foreseeable future. RAM overhead, fluidity impact, and failure modes under real usage won't surface from unit or integration tests. Extensive real-world use is the only reliable signal.

**On CSS stability:** YouTube Music uses internal custom elements with no public API contract. Selectors were documented manually via DevTools. Stability depends entirely on Google not changing their internal DOM structure. This is why theming was discontinued but functional JS/CSS hooks (equalizer, title extraction, player state) remain, since those are worth the maintenance cost.

**On audio normalization:** flagship feature and the reason to ship post-alpha, but not a blank check. Weigh against Last.fm and clipboard URL detection, which are cheaper to implement and still meaningfully differentiate from stock YTM.