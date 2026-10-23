# Verification for 1.0.0-rc.5

Local Windows checks, October 22, 2026. The previous RC4 working project and
release packages were preserved. No public GitHub release was published.

## Behavior checked

The Windows build and all deterministic suites pass, including **220,653 new
finger-boarding assertions**. See [behavior output](behavior-checks.txt).

- Real invitation, preparation, jump and touchdown sequences at 20%, 100% and
  180% pet sizes, 100%, 150% and 200% simulated DPI, both finger orientations
  and 16/33/100 ms frame steps.
- Different surface spots for repeated invitations from the same approach.
  Base, upper and middle approaches land on the surface. Only exposed-tip-side
  approaches can dangle, and those can also choose a surface landing.
- A committed destination through preparation and flight, correct facing,
  landing compression, continuous adjustment for small pointer drift, gentle
  carrying, interruptions, click departures and fast escapes.
- Existing catch/climb leg-contact, suspension, walking, waves and cleanup
  checks. Catch fixtures now start on the exposed tip side, retaining the
  existing attachment assertions. The old all-direction dangle expectation was
  replaced with assertions for the requested side-aware boarding behavior.

The full suite still covers 4,574 pounce hunts, 48 complete resident-web hunts,
all twenty skins, persistence, monitor gaps, nest entry and web reservation/fade.

## Windows and visual checks

The native run passes **651 assertions, zero failures**, covering transparency,
nonactivation, OS click-through, both actual monitor overlays, seam crossing,
settings, cursor restoration, timer and native resource cleanup.
See [native output](windows-checks.txt).

Inspected [actual rendered boarding sequences](finger-boarding.png), including
base-side and overhead surface landings, fingertip catches and mirrored catches.
These use the same behavior and WPF renderer as the desktop pet.

The render-heavy native diagnostic's final 16.1-second sample measured 10.90% of
one logical CPU, 472.2 MiB working set and 595.2 MiB private memory (31.6 MiB below
the sample start, after bulk image generation). GDI handles fell 782 to 730 and
USER handles changed 37 to 38 after 60 icon replacements. This is a short
diagnostic workload, not a normal pet-session benchmark or long-term leak test.

## Packages and manual follow-up

Package.ps1 builds and checksums the Windows and source bundles. The separate
release verification report records extracted-source builds, matching portable
bytes and rendered artwork, and the local Git commit/tag. Hosted GitHub CI remains
untested until the repository is published and its workflow runs.

For a quick hands-on check:

1. Hold the pointer to the left of an awake spider so it approaches from the
   base side of the left-pointing finger. It should land on top, settle and walk.
2. Offer the finger to a spider beyond its exposed tip. Across repeated offers,
   it should sometimes catch and dangle and sometimes land on top.
3. Carry gently, click for a planned upward departure, then try a fast movement
   on another ride for a startled escape. Check that normal clicks still work.

Physical mixed-DPI changes, actual sleep/unplug, hands-on cursor feel, phone QR
scanning and a long resource soak remain manual checks. Both connected monitors
used for native verification are 96 DPI; mixed-DPI layouts were simulated.
