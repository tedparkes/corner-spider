# Cross orb and corner retreat, alpha.6

The previous Cross design squeezed sixteen capture turns into a short sector.
The new design leaves more room for the spiral inside its corner frame. Wider
spacing and a clearer hub help the web read at desktop and preview sizes.
The design follows the species' slightly asymmetric orb shape.
[Zschokke, 2011](https://edoc.unibas.ch/entities/publication/94b4380b-28f6-4b8e-97c2-4a64bc111e7b)

The cocoon now sits 116 logical units from the side edge and 144 from the
horizontal edge at normal size. The extra space beside the horizontal edge
lets the spider walk around the diagonal sleeve and line up with its opening.
Curved frame strands run between attachment points along both edges. Short
silk ties join that frame to the sleeve.

[Before and after at the same scale](cross-orb-cocoon-comparison.png)

The behavior suite exercises the new position and the approach and exit paths
in all four corners, at multiple sizes, DPI settings, and step intervals. The
test for a usable approach checks clearance beyond the actual sleeve and room
for the entire spider. The prior 90-unit minimum corresponded to the old inset.

Run `Build.cmd` for behavior checks, `JumpingSpider.exe --silk-test diagnostics`
for native silk rendering checks, and `JumpingSpider.exe --web-review previews`
for the app-rendered plates.
