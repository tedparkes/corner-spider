# Verification for 1.0.0-rc.9

Local Windows verification, October 23, 2026. RC8 source, executable and release
packages were preserved before deepening the resident web's outer curve.

## Checked for RC9

The deterministic suites pass, including 183,160 corner-web checks and 56 full
Portia encounters: 48 across sizes/frame steps/DPI plus eight fitted-web hunts
across all four corners on landscape and portrait displays. Both outer attachment
choices complete hunts. See [behavior output](behavior-checks.txt).

The open edge remains one smooth quadratic arc split at radial spokes. Its middle
is about 30% closer to the corner than in RC8; fixed end anchors, tangent continuity,
bounds and construction paths are checked. Portia chooses an outer attachment on
an actual display edge, faces it to pluck, and keeps its body inside the desktop.
The resident follows the connected spoke. On fitted webs it stops earlier when
needed for the normal mouth-offset capture to remain reachable. Tests retain
resident/hunter separation, capture, patrol avoidance and departure-triggered fading.

Ten targeted WPF checks pass: visible silk, transparent bounds, both-edge contact
in all four corners, retained artwork and bounded image updates during building.
See [web rendering checks](web-render-checks.txt). These are the same web checks
called by the full --self-test diagnostic.

Inspected the [four-corner preview](prey-web-corners.png) and
[anchor-plucking detail](portia-web-edge.png). Also generated updated
[light/dark artwork](prey-web-light-dark.png) and the
[encounter gallery](portia-web-encounter.png). The web illustrates generic prey;
it is not an exact species web or natural construction sequence.

The Windows/source packages are checked entry by entry. Extracted source rebuilds;
the portable executable reproduces the updated web previews. The desktop shortcut
targets RC9. A normal launch/resource sample is recorded separately from the
broader earlier diagnostics. No public GitHub release was published.

## Previous checks and manual follow-up

The broader [Windows diagnostics](windows-checks.txt) and
[web resource samples](web-load-checks.txt) are retained RC7 results, not RC9 runs.
Those covered transparency, click-through behavior, focus, settings, cursor
restoration, shutdown and stable resource handles on one active 192-DPI display.

For a manual look, select Portia and invite a web-building spider in Settings >
Behavior. Check the deeper curve, direct edge attachments and Portia approaching
one anchored end. Automated complete hunts use simulated time. A full real-time
encounter, physical mixed-DPI changes, sleep/unplug and a long resource soak remain
manual checks for this release.
