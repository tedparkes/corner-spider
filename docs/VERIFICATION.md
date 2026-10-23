# Verification for 1.0.0-rc.8

Local Windows verification, October 23, 2026. RC7 source, executable and release
packages were preserved before changing the resident web's outer curve.

## Checked for RC8

The deterministic suites pass, including 183,049 corner-web checks and 48 full
Portia encounters. See [behavior output](behavior-checks.txt). The open edge is
one smooth quadratic arc split at the radial spokes; checks verify matching
tangents at joins, a visible inward bow, fixed edge attachments, supported bounds,
construction following the visible silk, plucking, capture and departure fading.
Sizes of 20-180% and simulated DPI of 100-200% remain covered.

The targeted WPF checks verify visible silk, transparent bounds, contact with
both desktop edges in all four corners, retained rendering and bounded image
updates during construction. See [web rendering checks](web-render-checks.txt).
These are the same web checks called by the full --self-test diagnostic.

Inspected the [four-corner preview](prey-web-corners.png),
[light/dark preview](prey-web-light-dark.png), [plucking detail](portia-web-edge.png)
and [full encounter gallery](portia-web-encounter.png). The shared curve moves
the spoke endpoints, building path and Portia's contact together. Both desktop
anchors remain fixed. The web is an illustration for generic prey, not an exact
species web or natural construction sequence.

The packages are checked entry by entry, the extracted source rebuilds, and the
portable executable reproduces the documented preview images. The desktop
shortcut targets the current runnable build. The local Git tag is v1.0.0-rc.8;
no public GitHub release was published.

## Previous checks and manual follow-up

The broader [Windows diagnostics](windows-checks.txt) and
[resource samples](web-load-checks.txt) are retained RC7 results, not new RC8 runs.
Those checks covered transparency, click-through behavior, focus, settings,
cursor restoration, shutdown and stable resource handles. They used one active
192-DPI display. This small geometry change does not alter that platform code.

For a manual look, select Portia, invite a web-building spider in Settings >
Behavior, and check the open edge as the silk grows. It should curve gently toward
the corner, stay attached to both edges, and keep its shape while Portia plucks.
Automated complete hunts use simulated time. A full real-time encounter,
physical mixed-DPI changes, sleep/unplug and a long resource soak remain manual.
