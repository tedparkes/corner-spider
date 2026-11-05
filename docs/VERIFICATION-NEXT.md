# Verification: 1.1.0-alpha.1

Local Windows development preview, November 5, 2026. Published 1.0 source,
packages and settings remain separate. This preview has not been published.

## Completed

- Windows .NET Framework compilation and the complete deterministic suite pass.
  [Behavior output](behavior-checks-next.txt) includes all 46 catalog choices,
  seven foods, same-species pair matching, both mating outcomes, every life phase,
  per-skin save/reload and a 48-hour **simulated active-time** run with repeated
  molts and broods. Size and population remain bounded. Three simulated monitors
  exercise baby departures past outer desktop edges, including internal corners.
- Native Windows tests pass **2,137 assertions**. [Native output](windows-checks-next.txt)
  covers every category/skin, live sliders and saves, all food menu commands,
  transparency, nonactivating overlays, OS click-through, cursor restoration,
  tray/overlay/hook cleanup and resource handling.
- Native render checks verify long-term silk adds to existing threads, retained
  artwork is reused, web opacity works, and all new-family sprites fit edge
  clearance at eight angles in four poses after a molt. Original jumping-spider
  behavior and native tests remain in place.
- Inspected app-rendered galleries for the new families, fresh/aged silk, life
  stages and settings. Galleries contain plain backgrounds, not desktop captures.
- The native test machine exposed one 3840x2100 working area at 200% DPI.
  Multiple layouts and 100-200% scaling are covered by simulation, not a physical
  mixed-DPI display test on this run.

The native diagnostic workload rendered large galleries and cycled through skins.
Its 16-second resource sample used 10.82% of one logical CPU, 610.4 MiB private
memory (falling 1.7 MiB during the sample). GDI handles changed 841 to 843 and USER
handles 44 to 41 after 60 icon replacements. This is not a normal-use benchmark;
rendering large galleries allocates much more memory than a fresh pet launch.
A normal launch resource sample is supplied beside the preview packages.

Package.ps1 verifies every ZIP entry against its staged source and produces
SHA-256 checksums. Package validation and normal-launch measurements are recorded
beside the archives. The source ZIP includes all tests and rebuild instructions.

## Still to check by hand

- Use the finger and click through the new species at 20%, 100% and 180%; assess
  the feel of their animation and confirm clicks reach the underlying app.
- On two physical monitors, cross a mixed-DPI seam, move the home screen, reconnect
  a display, lock/unlock and sleep/resume. Confirm the pet and web return sensibly.
- Invite a male from Life & home, then observe a complete family cycle across
  real sessions; check saved progress after quitting and reopening.
- Leave the pet running for days to judge the pace of silk aging and life events,
  and to measure a long resource soak. The 48-hour simulation is not a 48-hour
  native performance test.

Timing, mating outcome weights, six visible babies, repeating adult molts and
slow size normalization are desktop adaptations. The new family illustrations
are simplified procedural artwork, not anatomical identification plates.
