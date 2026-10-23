# Verification for 1.0.0-rc.6

Local Windows verification, October 22, 2026. The RC5 working folder and release
packages were preserved. The local Git repository is prepared for release;
no public GitHub release was published.

## Behavior and appearance

The Windows build and deterministic suites pass, including 178,137 new orb-web
checks and 48 complete Portia web encounters. See [behavior output](behavior-checks.txt).
The new checks cover connected curved spiral segments, joins on real radial
spokes, the builder following the visible strand, smooth construction, the hub
and Portia's plucking anchor, and bounds at 20-180% size and 100-200% simulated DPI.
Existing patrol reservation, single-prey limits, plucking/luring, capture and
fade-after-departure checks remain intact.

Inspected the [dark/light web preview](prey-web-light-dark.png),
[edge-plucking detail](portia-web-edge.png) and
[nine-stage encounter](portia-web-encounter.png). The generic resident builds an
orb-style illustration; no particular prey species or exact natural construction
sequence is claimed. Both the sleeping retreat and resident web remain distinct.

## Windows and resources

The Windows diagnostic checks actual transparent, nonactivating, click-through
overlays on both connected monitors, settings, cursor restoration, render bounds,
resource handles, timer cleanup and process shutdown. The web-specific checks
verify retained artwork across redraws and fading, bounded image updates while
building, visible silk and transparency outside the web footprint.
See [native checks](windows-checks.txt).

The completed silk is rendered from procedural vectors into one display-sized
image. During growth, newly completed strands reuse frozen vector artwork and
join the image in batches. Only the currently growing strand and local plucking
ripple need fresh geometry. Old build steps are not retained; temporary preview
worlds use weak cache keys.

[Live web load samples](web-load-checks.txt) measure a completed web and active
construction on the real transparent overlays at forced 30 fps. These short
samples include the pet and both monitor overlays, and are not a long-term leak
test or a guaranteed idle CPU figure. The initial uncached completed-web sample
used 32.88% of one logical CPU; generated-image caching reduced the equivalent
sample to 12.48%. Construction caching was subsequently batched and its fresh
strands retained to avoid a new bitmap or static curve allocation on every frame.
The final construction sample used 9.45% of one logical CPU and 169.7 MiB working
set over 16 seconds; private memory rose 1.8 MiB, with GDI handles 116 to 116 and
USER handles 34 to 34. Closing the harness removed all overlays and its cursor
helper. These figures are short local measurements, not hardware-independent
performance promises.

## Release and manual checks

The package report records ZIP entry checks, extracted-source builds, matching
portable artwork and the local Git commit/tag. Hosted GitHub CI remains untested
until publication. All assets remain procedural/local with no downloads or new
runtime dependencies.

To try the web, select Portia and choose Settings > Behavior > Invite a
web-building spider. Watch the anchors/frame, spokes and inward spiral develop;
the resident should wait at the hub. After the quiet wait, Portia should pluck
at the edge and lure it along a spoke. Silk should remain until Portia leaves.

Automated complete hunts use simulated time. The full multi-minute sequence,
physical mixed-DPI changes, actual sleep/unplug and a long resource soak remain
manual follow-up. Both real monitors used for the native checks are 96 DPI;
mixed-DPI layouts were simulated.
