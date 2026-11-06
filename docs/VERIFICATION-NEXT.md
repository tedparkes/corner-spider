# Verification: Corner Spider 1.1 previews

## 1.1.0-alpha.8

- The Windows build and deterministic behavior suite passed. The local Alpha 8
  executable passed 1,829 native silk checks. The desktop launch and normal quit
  passed, and the desktop shortcut and saved preferences were verified.
- [Same-scale cocoon comparison](roomier-cocoon.png),
  [corner web design review](VERIFICATION-CORNER-WEBS.md), and
  [web sampling checks](VERIFICATION-WEB-AA.md).
- Physical mixed-DPI transitions, sleep/resume and long-term colony use remain
  open for hands-on testing.

## 1.1.0-alpha.2

For the subsequent local prey-web correction, see [alpha.3 checks](VERIFICATION-WEB-BUILD.md).
The results below are the preserved alpha.2 verification record.

Windows prerelease verification, November 5, 2026. Published 1.0 and the earlier
local preview packages remain preserved. These checks were completed before
publication; the GitHub release page records subsequent live updater checks.

## Completed

- Windows compilation and the complete deterministic suite pass. [Behavior
  output](behavior-checks-next.txt) includes 48 colony and 112 updater checks,
  alongside the original suites and the 48-hour simulated life-cycle test.
- Colonies are checked for adoption, independent travel, bounded growth/population,
  saved species/homes/ages, main-nest relocation, mixed-DPI geometry, display
  changes, limits and gradual departures. Residents do not reproduce or own the
  cursor/prey slot. Invalid saved records are rejected.
- The 2,147 [native Windows checks](windows-checks-next.txt) cover all 46 skins, settings and
  persistence, food commands, transparency, nonactivating overlays, OS click-through,
  cursor restoration and tray/window/hook cleanup. New checks exercise the resident
  checkbox/slider and Updates tab. Diagnostics disable network access; a separate
  live probe covers GitHub.
- Native resident rendering is tested at 100% and 200% scale with a monitor offset.
  Removed residents leave no retained artwork. Stationary resident silk uses
  separate retained visuals; smaller secondary body poses use a bounded cache.
  The main pet keeps its continuous procedural animation.
- Inspected app-rendered galleries and settings screenshots, including the resident
  checkbox, Updates tab and independent homes. These have plain backgrounds, not
  captures of a personal desktop.
- [Updater verification](update-checks-next.txt): actual network code fetched the
  public release feed, downloaded the official 1.0 Windows ZIP, verified SHA-256
  and executable version, and declined a downgrade. No public release or user
  installation was changed by that probe.
- Local fixtures exercised the actual hidden helper: wait for old-process exit,
  refuse replacement while another pet runs, preserve old/unrelated files, install,
  exit, and restart a harmless replacement fixture in its installation folder.
  Injected copy failures verify rollback. Invalid paths, versions, hashes, release
  links and stable/preview ordering are covered by deterministic tests.

The [12-resident load sample](colony-load-next.txt) records early travel and later
activity separately, CPU as a percentage of one logical core, private memory and
handles. It includes the main pet plus twelve residents. Diagnostic garbage
collection is a measurement aid, not periodic forced collection in the app.
More spiders cost more resources; the default limit is six and the feature starts
off. A short sample cannot establish long-term memory stability. The single-pet
launch measurement is beside the release ZIPs.

The native machine exposed one 3840x2100 working area at 200% DPI. Multiple layouts
and 100-200% scaling are simulated, not physical mixed-DPI QA. Package.ps1 verifies
every ZIP entry against its staged source and emits SHA-256 checksums.

## Still to check by hand

- Enable resident offspring, complete a brood, quit/reopen, lower the limit and
  turn the checkbox off. Assess growing nests and departures during ordinary use.
  Observe several days for pacing and resource use.
- Use finger play and click-through at 20%, 100% and 180%. Test two physical
  monitors with mixed DPI, changing the home screen, display reconnect, lock/unlock
  and sleep/resume. Check resident homes as well as the main pet.
- Use Download and restart from a normal installation when a newer preview is
  available. Alpha.2 is the first public build with an updater; an existing alpha.2
  installation should report up to date. A controlled older-version fixture is
  used to exercise the public download without downgrading personal saves.
- Try manual checks offline and automatic checking over an actual day. Version and
  channel decisions are tested; a day-long native scheduling/outage test is pending.

Timing, mating weights, six visible babies, repeating adult molts and slow size
normalization are desktop adaptations. Procedural illustrations are simplified,
not anatomical identification plates. The 48-hour simulation is not a native soak.
