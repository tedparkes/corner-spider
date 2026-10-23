# Verification for 1.0.0-rc.7

Local Windows verification, October 23, 2026. The RC6 working folder and release
packages were preserved. The local Git repository is prepared for release;
no public GitHub release was published.

## Behavior and appearance

The Windows build and deterministic suites pass, including 183,042 corner-web
checks and 48 complete Portia web encounters. See [behavior output](behavior-checks.txt).
The geometry checks cover a continuous supported frame, multiple direct edge
attachments, curved spiral joins on real spokes, construction following the silk,
the moved hub and plucking point, and bounds at 20-180% size and 100-200% simulated
DPI. All sampled strands remain inside the triangle between the two edges.
Existing patrol reservation, single-prey limits, plucking/luring, capture and
fade-after-departure checks remain intact.

Inspected the [four-corner preview](prey-web-corners.png),
[dark/light preview](prey-web-light-dark.png), [edge-plucking detail](portia-web-edge.png)
and [nine-stage encounter](portia-web-encounter.png). Eleven direct attachments
span the two desktop edges. The open rim is a continuous bridge from one edge
to the other, bowed toward the corner. The resident waits at the visible hub.
This is a procedural illustration for generic prey, not an exact species web
or natural construction sequence. Portia's sleeping retreat remains separate.

## Windows and resources

The Windows diagnostics check transparent, nonactivating, click-through overlays,
settings, cursor restoration, resource handles, timer cleanup and shutdown.
The web-specific checks verify retained artwork, bounded image updates during
construction, transparency outside the footprint and visible silk contact with
both edges in all four mirrored corners. See [native checks](windows-checks.txt).

This session exposes one physical display at 192 DPI (200%). Other monitor
layouts, mixed DPI and disconnection are covered by deterministic simulations;
the earlier RC6 session's two-monitor measurements are not new RC7 measurements.

The completed silk reuses one image generated from vectors at display resolution.
Growth retains completed curves and updates the image in batches. Only the
growing strand and plucking ripple need fresh geometry. Preview caches use weak
world references and do not retain historical construction steps.

[Live web load samples](web-load-checks.txt) measure completed silk and active
construction at forced 30 fps on the current transparent overlay. These short
samples include the pet and display overlay. They are not a long-term leak test
or a guaranteed idle CPU figure. The larger native-test working set includes
rendering galleries and repeatedly opening settings before its final sample.

## Release and manual checks

The package report records ZIP entry checks, an extracted-source build, matching
portable artwork and the local Git commit/tag. Hosted GitHub CI remains untested
until publication. No runtime assets are downloaded.

To try it, select Portia and choose Settings > Behavior > Invite a web-building
spider. Watch the edge-to-edge bridge, attached frame, spokes and spiral develop.
The resident should wait at its hub, then approach Portia's plucking contact.
After the meal the web should remain until Portia leaves, then fade.

Automated complete hunts use simulated time. Watching the full multi-minute
sequence, physical mixed-DPI changes, actual sleep/unplug, hands-on cursor feel
and a long resource soak remain manual follow-up.
