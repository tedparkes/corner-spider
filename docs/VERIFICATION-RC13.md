# Verification for 1.0.0-rc.13

Local Windows verification, October 27, 2026. RC12 source, executable, packages
and Git tag were preserved before changing retreat artwork.

## Behavior and geometry

The Windows .NET Framework build and complete deterministic behavior suite pass.
See [behavior output](behavior-checks.txt). This includes 184 travel routes,
4,574 pounce hunts, 56 complete Portia web hunts, cursor play, settings persistence,
species anatomy, multiple-monitor routing and nest entry/exit.

The hammock suite adds 4,060 checks across all twenty skins. Every new outline
stays within the existing route clearance and leaves room before both entry
waypoints. The full suite retains its size, simulated DPI, four-corner, fixed
joint, tuck, wake and display-reflow coverage. No behavior tests were weakened.

## Native WPF artwork

Run `JumpingSpider.exe --silk-test <output-folder>` for the focused silk diagnostic.
It exercises all twenty skins, four corners, edge anchoring, retained artwork,
opacity changes, species switching and monotonically increasing silk growth.
It also renders vector silk at 20%, 100% and 180%, checks all 55 pairs of the
eleven species silhouettes before texture, and checks that Portia's central
platform remains uncovered while the Regal retreat has a veil.

An initial check found Bold and Pantropical too similar. Pantropical's outline
and outer mesh were revised; the silhouette threshold was retained.
See [native results](silk-render-checks.txt), the retreat comparisons on
[dark](species-retreats-dark.png) and [light](species-retreats-light.png)
backgrounds, and [all twenty settings previews](retreat-settings.png).
The rendered comparisons and corner/tuck panels were visually inspected.

The two retained silk layers are replaced when species, growth level or corner
geometry changes. They are reused during ordinary animation and opacity changes;
there is no accumulating per-growth-stage cache. Artwork is generated locally.

## Packages and scope

Both release ZIPs are verified entry by entry. Extracted source rebuilds with the
complete suite. The extracted portable executable reproduces the species-retreat
and corner/tuck previews. Source bytes match the local Git repository. The
installed executable matches the portable ZIP; the existing Desktop shortcut
continues to target that build. The release folder records package verification
and a short launch/resource sample.

The full native overlay, tray, cursor-restoration and focus suite last passed on
RC10; [those results](VERIFICATION-RC10.md) remain available. It was not repeated
in full for this artwork change. Physical mixed-DPI changes, unplug/reconnect,
sleep/resume and a long resource soak remain manual checks.

Before a public release: compare the shelters in Appearance, try the web-visibility
and size sliders, send the spider home, and watch entry, sleep and exit at your
preferred size. Check the silk against your normal wallpaper and on both screens.
The hosted download remains private to the user's ChatGPT account; no public
GitHub release has been published.
