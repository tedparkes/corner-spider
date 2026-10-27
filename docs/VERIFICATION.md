# Verification for 1.0.0

Local Windows verification, October 27, 2026. RC14 source, packages and Git tag
are preserved. The public release retains the tested RC14 behavior and QR-only
support interface, updates the version, and includes the full development changelog.

## Automated behavior

The Windows .NET Framework build and complete deterministic suite pass. See
[behavior output](behavior-checks.txt). Coverage includes 184 travel routes,
4,574 pounce hunts, 56 complete Portia web hunts, settings and save migration,
20 skins, finger play, nest entry/exit, 20-180% size, 100-200% simulated DPI,
display layout changes and interruptions. No behavior tests were weakened.

## Native Windows checks

The final 1.0 executable passed all 671 assertions in the full native self-test.
See [Windows results](windows-checks.txt). The machine presented one 3840-pixel-wide
4K display at 200% scaling, with a 3840x2100 working area. This run verifies:

- Nonactivating layered overlays, physical working-area and DPI matching, and
  OS hit-testing that passes through both the spider and illustrated finger.
- Tray actions, live settings, every skin, 20-180% size, finger tone, silk visibility,
  interaction controls, save/reload, preview disposal and opt-in startup protection.
- QR-only support, the unchanged QR image, compact layout and popup closure.
- Native cursor hiding and restoration after release, lost heartbeat, interrupted
  helpers, pause and exit; mouse-observer cleanup and removal of overlays/tray icons.
- Skin, prey and retreat rendering, anchored silk, opacity, retained caches and
  stable growth. Sixty icon replacements did not leak GDI or USER handles.

The native resource sample followed rendering and icon stress: 16 seconds,
12.69% of one logical CPU, 364.4 MiB private memory with memory falling during the
sample. This is a diagnostic workload, not a normal-use performance benchmark.
Normal-launch measurements are recorded beside the release packages.

This run did not exercise a physical two-monitor seam because only one display
was connected. Earlier native two-monitor checks and current simulated coverage
are documented in the preserved verification history. Physical mixed-DPI changes,
actual unplug/reconnect, sleep/resume and a long resource soak remain unverified
for 1.0. User interaction feedback informed development; phone QR scanning and a
real support payment were not repeated for this release.

## Release integrity

Both ZIPs are verified entry by entry. Extracted source rebuilds with the complete
suite. The portable executable version and files are checked against the packaged
build, and the installed copy matches that portable executable. The Desktop
shortcut targets the current build. Source bytes match the local Git repository.

The repository history was scanned for private local paths and credential patterns
before upload; no matches were found. Only project source, public documentation,
procedural artwork and the intentionally supplied support QR are included. Personal
saves, reference photographs, workspace backups and credentials are excluded.
The executable is unsigned; no code-signing or security certification is claimed.

## Quick manual check on another PC

Extract the Windows ZIP and run JumpingSpider.exe. Confirm ordinary clicks still
reach the desktop. Open Settings from the tray, change skin/size, invite a finger
ride, click to dismount, and send the spider home. Quit and confirm the cursor,
tray and overlay return to normal. On multiple monitors, check travel and the
chosen hammock corner, then test display reconnection and sleep/resume.

The public repository and release are available at
https://github.com/tedparkes/corner-spider/releases/tag/v1.0.0.
