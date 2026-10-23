# Artwork and reference notes

All shipped artwork is original procedural code in PetArt.cs, MorphologyArt.cs, PreyArt.cs and HammockArt.cs,
with finger colors in FingerPalette.cs. No web photograph,
private reference photo, model or texture is included or loaded at runtime.
Species labels describe reference-informed adult illustrations. Body outlines,
head proportions, limb lengths, stance and palps now vary by profile, using a common
spatial joint solver. Body size is normalized for a desktop pet, not actual relative
size in millimeters. Color varies with age, individual and lighting. These are not
diagnostic models or distinct biological behavior simulations.

| Species / options | Shape and marking cues |
| --- | --- |
| Regal, male/female | Robust Phidippus head; female fuller warm abdomen; male black/ivory, smaller abdomen and fuller palp bulbs |
| Bold, male/female | Three pale abdominal marks, green chelicerae; fuller female abdomen, narrower male rear body |
| Zebra, male/female | Compact head, shorter limbs, transverse bands; male has longer chelicerae and smaller abdomen |
| Tan, male/female | Low elongated bark-colored body, slender abdomen, wavy pattern; male warm face band |
| Red-backed, male/female | Red abdomen, black prosoma; female fuller abdomen with dark median band |
| Emerald, male/female | Green sheen and pale edging; darker narrower male, broader green female |
| Gray wall, male/female | Flat elongated proportions; female gray leaf-like field; male dark median field, pale sides and head stripe |
| Pantropical, male/female | Male central white stripe; female broader brown abdomen with tan field, chevrons and rear marks |
| Apache, male/female | Orange-red body sections, green chelicerae; broader female with dark abdominal band |
| Portia labiata, female | Narrow mottled abdomen, smaller angular prosoma, long thin fringed legs, white clypeal hairs |
| Hyllus diardi, female, green variant | Broad prosoma, longer full abdomen, eye tufts and chevrons; green is an explicitly stylized requested palette |

The Hyllus proportions and tufts follow the female description in Hoang (2025).
That description gives brown/yellow coloration, so the green option is labeled
as a color variant in settings and documentation. It does not claim that uniform
bright green is the typical wild coloration.

Palps use independent two-segment kinematics and a rounded male or tapered female
terminal shape. Their sideways cleaning strokes are an illustrative animation,
not a motion-captured reconstruction. Their attachment points scale with the head.
Foreleg waving keeps its upward spatial hinge and is separate from cleaning.

References used to check these choices:

- [Wanless (1978): Revision of Portia, P. labiata description pp. 103-105](https://peckhamia.com/library/Wanless%201978%20A%20revision%20of%20the%20spider%20genus%20Portia.pdf)
- [Hoang (2025): Review of Hyllus in Vietnam, H. diardi redescription](https://europeanjournaloftaxonomy.eu/index.php/ejt/article/download/2831/12915/)
- [UF/IFAS: Regal jumping spider](https://ask.ifas.ufl.edu/publication/IN309)
- [Missouri Department of Conservation: Bold jumper](https://mdc.mo.gov/discover-nature/field-guide/bold-jumper)
- [Natural History Museum: Zebra jumping spider](https://www.nhm.ac.uk/take-part/identify-nature/spiders-in-your-home/zebra-jumping-spider.html)
- [British Arachnological Society: Zebra spider](https://britishspiders.org.uk/sites/default/files/2020-08/Salticus_spreads.pdf)
- [Missouri Department of Conservation: Tan jumping spider](https://mdc.mo.gov/discover-nature/field-guide/tan-jumping-spider)
- [Jackson: Prey of Phidippus johnsoni, with adult male illustration](https://www.americanarachnology.org/journal-joa/joa-all-volumes/detail/article/download/JoA_v5_p145.pdf/?no_cache=1)
- [Missouri Conservationist, October 2024: jumping spiders and Emerald jumper](https://mdc.mo.gov/sites/default/files/2024-09/MOCON_Oct24_508.pdf)
- [UF/IFAS: Gray wall and Pantropical jumpers](https://ask.ifas.ufl.edu/publication/IN315)
- [Iowa State BugGuide: Phidippus apacheanus](https://www.bugguide.net/node/view/27092)

The small prey spider is an original generic brown spider illustration, not a
named prey species. It has eight thin legs, a small prosoma, an oval abdomen and
short ground-crawling steps. It folds its legs when held as a meal. Portia's
spider-eating diet is informed by [Harland and Jackson (2006)](https://thehive.icipe.org/all-prp/2798/),
which tested Portia labiata with both flies and web-building spider prey. The
desktop's crawling prey and shared stalking sequence are illustrative. A separate
Portia web encounter now adapts web plucking and luring; its timing and response
rules are scripted, as described in BEHAVIOR-RESEARCH.md. The 65%
selection rate is an app design choice, not a biological measurement.

The finger is a stylized bent index finger. A privately supplied pose reference
informed the silhouette; that photo and its identifying surroundings are excluded.
The hand end fades to alpha, including its outline. It is not a photo cutout.
Its deep-brown to very-pale slider adjusts skin, shading, creases and nail colors
together. The original shade is retained at 80%.

The hammock uses an elongated silk pocket with two loose openings, informed by
the resting-nest description in the UF/IFAS reference above. Its diagonal corner
placement and readable translucency are desktop artwork choices. The RC2
attachment mesh and species silk variations are explained in SILK-REFERENCES.md. The folded
resting pose retains the existing spatial leg rig; it is not a physiological
sleep simulation. No reference imagery is shipped.

The source license covers project code and original art, not the linked reference
material. Names of institutions do not imply endorsement.
