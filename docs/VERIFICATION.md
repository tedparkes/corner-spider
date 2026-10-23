# Verification for 1.0.0-rc.4

Local Windows checks, October 22, 2026. The previous RC3 working project and
release packages were preserved. No public GitHub release was published.

## Behavior checked

The Windows build and all deterministic suites pass. The encounter suite passes
1,931,877 assertions across 48 complete web hunts, with 20%, 100% and 180% pet
sizes, multiple simulated DPIs, four corner orientations and varied frame steps.
Existing prey-slot, interruption, naps, finger play, monitor-gap crossing and
settings persistence checks still pass. See [behavior output](behavior-checks.txt).

New checks cover:

- Three times the earlier web span on roomy displays; fitting down on smaller
  work areas while leaving an approach lane. Every junction remains on its screen.
- Waiting at the central junction, clear separation from the edge-plucking hunter,
  and complete capture of exactly one resident.
- Excluding the whole pet footprint from passive webs during ordinary patrols and
  homeward travel, including routes that would cross the web and already-pending
  patrols into the chosen corner.
- Preserving all silk during the meal and while the hunter remains on the web.
  A stationary hunter holds the web beyond the former fade duration. Calling it
  home preserves the web underfoot; departure eventually permits complete cleanup.

The former check that expected cleanup within 14 seconds of capture was replaced
with checks matching the requested departure-triggered fade. Existing capture,
continuity, exclusivity and interruption checks were retained.

## Windows and visual checks

The native run passes **651 assertions, zero failures**, covering transparency,
nonactivation, OS click-through, both actual monitor overlays, seam crossing,
settings, prey rendering at size/DPI combinations, cursor restoration, timer and
native resource cleanup. See [native output](windows-checks.txt).

Inspected the rendered nine-stage encounter and a larger edge-plucking view.
The resident is visibly smaller than Portia, rests at the central junction, and
has room to approach along the strand. Portia's contacting front feet are linked
to the outer anchor. The meal view retains the complete web.

The render-heavy native diagnostic's final 16-second sample measured 12.47% of
one logical CPU, 259.1 MiB working set and 360.8 MiB private memory (208.4 MiB below
the sample start, after bulk image generation). GDI handles fell 768 to 636 and
USER handles changed 37 to 38. This is a short diagnostic workload, not a normal
pet-session benchmark or a long-term leak test.

## Packages and manual follow-up

Package.ps1 builds and checksums the Windows and source bundles. The separate
release verification report records extracted-source builds, matching portable
bytes and rendered artwork, and the local Git commit/tag. Hosted GitHub CI remains
untested until the repository is published and its workflow runs.

To try the change, select Portia in Appearance, then choose Behavior > Invite a
web-building spider. Allow 3-4 minutes for building and another 1-2 minutes for the
quiet wait. Check that ordinary patrols avoid the corner, then watch the approach,
pluck, capture, meal and departure before the silk fades. Automated full hunts use
simulated time; the whole sequence has not been watched in real time on this build.

Physical mixed-DPI changes, actual sleep/unplug, hands-on cursor feel, phone QR
scanning and a long resource soak remain manual checks. Both connected monitors
used for native verification are 96 DPI; mixed-DPI layouts were simulated.
