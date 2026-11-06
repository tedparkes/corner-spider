# Web anti-aliasing: 1.1.0-alpha.4 local preview

Windows verification, November 5, 2026. Alpha.3 and the published releases are
preserved. This local build has not been published to GitHub.

## What changed

Silk is drawn at up to four samples per physical pixel in each axis, then filtered
with WPF's high-quality image scaling. The setting is attached only to retained
silk layers, not the whole moving pet. It covers retreats, capture webs, resident
nests, prey webs and settings thumbnails. Animated weaving tips and plucking
strands retain their existing native vector anti-aliasing.

Each cached layer is limited to 2,097,152 pixels and 2,048 pixels per side, including
its transparent border. This bounds pixel storage to 8 MiB per layer, excluding
WPF overhead. Only current layers are retained. Growth, size and display scale
invalidate the appropriate cache; ordinary redraws and opacity changes reuse it.
Sampling is aligned to a stable grid so growth does not displace existing threads.

The web geometry, construction sequence, thread colors and opacity controls are
unchanged. Thin strands blend more evenly with the background. See the
[magnified before/after comparison](web-smoothing-comparison.png), and the
[dark](web-smoothing-dark.png) and [light](web-smoothing-light.png) galleries.
These are app renders on plain backgrounds, not desktop captures.

## Completed checks

- Windows compilation and the full [behavior suite](behavior-checks-web-aa.txt) pass.
- The expanded [native silk suite](web-aa-checks.txt) passes without failures:
  all 46 skins, four corners, growth continuity, opacity, transparent bounds,
  preview controls, source-image allocation limits, cache reuse, construction
  and plucking. Existing tests were retained.
- Independent 8x area-sampled references test thin straight and curved strands
  at 20%, 55%, 100% and 180%. Filtering reduces alpha-coverage error at every
  tested scale compared with default scaling of the same cached source. This is
  a rendering-error measurement, not a percentage claim about perceived quality.
- Six [resident-rendering checks](colony-aa-checks.txt) pass at simulated 100% and
  200% DPI, including monitor offsets and removal of departed residents' visuals.
- Inspected before/after native renders on dark and light backgrounds.

## Controlled redraw timing

Same machine, same six scenes, 120 warmed redraws each to a 360 x 360 WPF bitmap.
The comparison uses the preserved alpha.3 executable and the alpha.4 executable.
Cold cache creation is excluded. These are offscreen silk timings, not whole-app
CPU percentages or desktop frame-rate claims.

| Scene | Alpha.3 ms/redraw | Alpha.4 ms/redraw |
| --- | ---: | ---: |
| Regal retreat | 5.845 | 2.491 |
| Garden orb | 11.597 | 1.774 |
| Brown widow | 9.287 | 1.271 |
| Barn funnel | 8.880 | 1.265 |
| Portia retreat | 6.050 | 2.533 |
| Prey web | 3.125 | 3.066 |

The new retained images avoid retessellating capture webs at every redraw.
The larger sampling source is created only when its cache changes.

## Repeat and remaining checks

Run `Build.cmd`, then `JumpingSpider.exe --silk-test diagnostics` for the expanded
silk suite, or `--silk-sampling-test diagnostics` for sampling/cache checks alone.
Full overlay click-through, cursor play, physical mixed-DPI changes and sleep/resume
were not rerun for this drawing-only change. Inspect the webs on ordinary wallpaper
at your preferred size and opacity; a long desktop or maximum-colony soak remains
manual QA. The [alpha.2 record](VERIFICATION-NEXT.md) describes earlier overlay tests.

Implementation reference: Microsoft's [BitmapScalingMode documentation](https://learn.microsoft.com/en-us/dotnet/api/system.windows.media.bitmapscalingmode?view=netframework-4.8.1)
describes the default linear filter and high-quality/Fant filter.
