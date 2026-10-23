# Verification for 1.0.0-rc.3

## RC3 preview and repository checks

Windows build and all behavior suites pass. The final native run passes **651
assertions, zero failures**, including settings, preview timer shutdown, native
handle stress, nonactivation, click-through, both real monitors and exit cleanup.
See [native output](windows-checks.txt) and [behavior output](behavior-checks.txt).

Inspected the actual WPF Appearance window at normal and compact sizes, plus
selected-species previews on light/dark backgrounds and at 20%/180% size. The silk
stays attached to the top/right edges; changing the species updates its style.
The miniature shows a mature retreat independently of saved nest progress.

An earlier RC3 check failed its native-handle limit. The small preview initially
allocated full desktop-sized silk images; it now uses the same frozen vector
layers without those render targets, and releases its artwork when disposed.
The unchanged final resource check passed (GDI 780 to 768; USER 37 to 37). This
does not establish that the earlier count difference was solely caused by the
preview allocation, or prove long-term leak-free operation.

The render-heavy diagnostic sample measured 13.76% of one logical CPU, 554.1 MiB
working set and 688.9 MiB private memory, up 59.2 MiB during the 16-second sample.
It generates many full-resolution galleries and is not representative of an
ordinary pet session. The normal-pet figures in the RC2 baseline below are older
measurements, not a fresh RC3 measurement.

The source repository includes the license, build workflow, issue template,
release guide and public documentation. Prior releases and the pre-edit RC2
working project were preserved. No public GitHub release was published.

Manual follow-up: open Settings > Appearance, choose Portia, Zebra and Hyllus,
then try the size, angle and background controls. Check the silk at the top right
and the spider at the center. Physical mixed-DPI changes, actual sleep/unplug,
hands-on cursor feel, phone QR scanning and a long resource soak remain manual.

## RC2 baseline (historical)

The following sections record the earlier RC2 verification and performance.

Local Windows checks, October 22, 2026. The complete RC1 working project was
archived before editing; prior release ZIPs remain intact. No startup preference
was changed and no public release was published.

## Silk changes checked

- All twenty skins in all four corners: the rendered retreat has multiple visible
  attachments along both adjoining desktop edges, while the opposite corner stays
  fully transparent.
- Repeated drawing reuses retained artwork. A skin change replaces the current
  silk layers immediately. Growth changes the layer density without retaining
  an ever-growing set of old images.
- Pixel comparisons for every skin confirm that increasing silk progress retains
  existing thread coverage. Secondary weave and entrance rims have independent
  deterministic seeds, so added fibers no longer rearrange earlier ones.
- Native settings, transparency, focus preservation, OS click-through, both real
  monitor overlays, seam crossing, cursor restoration and exit cleanup pass.
- Inspected light/dark galleries of all eleven species' retreat styles and the
  generic web resident, the four-corner gallery, and the Portia encounter sequence.
  Existing end-entry/waking behavior remains in place.

The RC2 native run passed **651 assertions, zero failures**. The linked native
output above is now the RC3 run. No existing check was weakened, including
the overall transparent-background check.

## Behavior and build

The baseline and updated Windows .NET Framework builds pass. The complete suite
includes 1,481,284 species/encounter assertions and 48 complete simulated web
hunts using the new connected thread layout; 1,204,682 pounce assertions across
4,574 ordinary hunts; and 1,295,811 anatomy assertions. Existing corner, cursor,
settings, hammock, display reflow and end-entry regressions also pass. See
[behavior output](behavior-checks.txt).

Full web encounters were tested by advancing simulation time, rather than watching
every encounter for its entire real-time duration. Both physical monitors were
96 DPI; the mixed-DPI sizes/layouts were simulated.

## Resources and packaging

A separate normal-pet run took two consecutive 15-second samples: 3.33% and 3.44%
of one logical CPU, 142.43–142.55 MiB working set, and 145.93–145.96 MiB private
memory. Both samples remained responsive; handles fell from 607 to 606. Normal
quit returned exit code 0 and left no pet process.

The heavier native diagnostic workload includes many full-resolution silk caches
and preview images. Its subsequent 16-second sample measured 11.32% of one logical
CPU, 383.8 MiB working set and 454.7 MiB private memory (5.8 MiB above the sample's
start). GDI handles fell 797→686 and USER handles changed 37→38 after 60 icon
replacements. Native handle and helper/hook cleanup checks passed. These short
samples do not establish long-term leak-free operation.

Package.ps1 verifies every archive entry against staged bytes and writes SHA-256
checksums. Release acceptance rebuilds a fresh source extraction, renders the
portable executable's silk/skin/prey/encounter previews and compares them with the
documented images, then runs its native checks. The separate release verification
report records package acceptance. The Desktop shortcut points to the latest
working build. The executable remains unsigned; hosted CI is untested.

## Short manual checklist

1. Change between Regal, Tan, Pantropical, Portia and Hyllus. Check the attachment
   bundles against your usual wallpaper, and watch entry and waking from both ends.
2. Try all four corners and your preferred sizes. Confirm the silk looks connected
   to the screen edges and leaves the spider visible inside.
3. With Portia selected, invite a web-building spider in Behavior settings. Watch
   the full build/wait/pluck sequence, then try finger play and normal desktop clicks.

Hands-on cursor feel, physical mixed-DPI changes, actual sleep/unplug, phone QR
scanning and a long resource soak remain manual checks. The native and simulated
checks above do not substitute for those physical tests. No display or sleep
settings were changed.
