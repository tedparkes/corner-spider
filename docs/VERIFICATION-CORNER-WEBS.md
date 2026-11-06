# Corner web redesign, alpha.5

Each preview uses the app's WPF renderer on a plain desktop-sized panel. A line
marks each nearby desktop edge. The review covered representative designs from
all 46 skin choices, including male silk, fictional variants and edge residents.

[Orbs and prey](corner-web-orbs.png) · [Cobwebs and sheets](corner-web-sheets.png) ·
[Retreats](corner-web-retreats.png) · [Light desktop](corner-web-light.png)

## Three critic rounds

Each pair is realism / biological accuracy out of 10. Fictional designs were
judged on believable silk mechanics. The cutoff is strictly above 8 on both axes.

| Design group | Round 1 | Round 2 | Round 3 |
| --- | ---: | ---: | ---: |
| Female orbs | 7.8 / 8.3 | 7.9 / 8.2 | 8.6 / 8.7 |
| Male garden travel silk | 7.0 / 6.8 | 8.5 / 8.6 | 8.5 / 8.6 |
| Fictional orbs | 7.8 / 8.1 | 7.9 / 8.3 | 8.5 / 8.6 |
| Portia's prey orb | 8.1 / 8.6 | 8.6 / 8.7 | 8.6 / 8.7 |
| Widow cobwebs | 7.0 / 7.9 | 8.0 / 8.3 | 8.3 / 8.5 |
| Common house cobweb | 7.1 / 7.8 | 8.0 / 8.2 | 8.2 / 8.4 |
| Barn funnel | 7.5 / 8.0 (joint) | 8.0 / 8.4 | 8.4 / 8.6 |
| Copper funnel | 7.5 / 8.0 (joint) | 8.1 / 8.4 | 8.3 / 8.5 |
| Male widow and funnel | 6.9 / 7.3 | 8.3 / 8.4 | 8.3 / 8.4 |
| Jumping spider retreats | 7.4 / 8.1 | 8.0 / 8.3 | 8.4 / 8.5 |
| Portia's own webs | 7.7 / 8.0 | 8.4 / 8.7 | 8.4 / 8.7 |
| Resident edge nests | 7.9 / 8.2 | 8.4 / 8.5 | 8.4 / 8.5 |
| **Gate** | **Fail** | **Fail** | **Pass** |

The first review identified rigid triangular capture bands, flat cobweb meshes,
and stiff retreat cuffs. The second revision curved the supporting frames, separated
widow gumfeet from the upper scaffold, thinned retreat mounts, and differentiated
male and female silk. Its orbs still compressed their capture turns near the open
span. The final revision recovered that clearance, softened the web and tunnel
texture, and added selective backing threads for contrast on light desktops.

The first Windows run after round three found 32 faint edge contacts in the
widow and house designs. Small adhesion seams now meet the existing visibility
threshold in all four corners. The critic rechecked the changed renders and
confirmed the round-three scores.

The app projects the web onto a flat desktop. The critic noted a soft haze in the
sheet highlights and the use of stand-alone resting sites for male skins. Residents
on the middle of an edge attach to the available edge. The scores evaluate the
rendered designs under those constraints.

## Reference features

- Orb frames, load-bearing radii, capture spiral and the free zone around the hub.
  [Sensenig et al., 2010](https://onlinelibrary.wiley.com/doi/10.1111/j.1420-9101.2010.02048.x)
- Peripheral widow retreats and central house spider retreats on gumfoot webs.
  [Benjamin and Zschokke, 2003](https://edoc.unibas.ch/entities/publication/9f6de06d-ecb4-4ff6-affd-62efc4c80678)
- Funnel-weaver sheets taper into a protected opening.
  [University of Minnesota Extension](https://extension.umn.edu/yard-and-garden-news/falls-hidden-architects-funnel-weavers-minnesota-gardens)
- Tubular jumping spider retreats and Portia's open platforms and layered space webs.
  [Jackson, 1979](https://www.americanarachnology.org/journal-joa/joa-all-articles/article/download/JoA_v7_p47.pdf/?no_cache=1)
  · [Jackson and Hallas, 1986](https://gwern.net/doc/biology/portia/1986-jackson.pdf)

## Repeat the checks

Run `Build.cmd` for the behavior suite. Run `JumpingSpider.exe --silk-test diagnostics`
for Windows art checks and `JumpingSpider.exe --web-review previews` for the
critique plates.
