# Prey-web construction: 1.1.0-alpha.3 local preview

Verified on Windows, November 5, 2026. This build is not published to GitHub.
The public alpha.2 source, installation and release ZIPs remain preserved.

- Added a regression check before changing the algorithm. It failed on the
  original code because a radial segment was still added after the capture spiral.
- Windows compilation and the complete deterministic behavior suite pass.
  [Behavior output](behavior-checks-web-build.txt) includes 184,900 corner-web
  checks and 56 full-length encounter hunts. Tests cover frame-before-spokes,
  all spokes before the spiral, straight supports, travel along completed silk,
  the final return to the hub, smooth movement, four corners, size and DPI bounds.
- All 19 targeted native WPF [render checks](web-render-checks.txt) pass.
  Pixel comparisons confirm that transit to the spiral and the final return to
  the hub leave existing silk unchanged. Cache reuse, opacity, transparent bounds,
  attachment to both screen edges and the later plucking animation also pass.
- Inspected the app-rendered [six-stage construction preview](prey-web-construction.png).
  It shows the same finished concave corner shape, now built as a complete support
  skeleton followed by an inward capture spiral. The frames come from simulated
  world ticks rendered by the actual WPF drawing code, on a plain background.

The native full-overlay checks documented in [alpha.2 verification](VERIFICATION-NEXT.md)
belong to the previous build; they were not rerun for this geometry-only change.
Physical mixed-DPI display changes, sleep/resume and a long desktop soak remain
manual checks. This change does not redesign the main pets' species-specific webs.

To repeat: run `Build.cmd`, then `JumpingSpider.exe --encounter-preview diagnostics`.
The second command exports the construction gallery and `web-render-checks.txt`.
For an ordinary-use check, select Portia, enable web encounters and watch a full
3-4 minute build. The support skeleton must finish before the interior turns
appear; the builder should then return to the hub and wait for the encounter.
