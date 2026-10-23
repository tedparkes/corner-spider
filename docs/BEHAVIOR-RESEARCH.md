# Behavior reference notes — 1.0 RC1

Reviewed October 22, 2026. The app is a species-inspired companion, not an
ethological simulation. These sources informed the following small desktop
adaptations. Study conditions, age, sex and individual differences matter.
No photographs or papers are distributed with the app.

| Species / skins | Observation and source | Desktop adaptation and limits |
| --- | --- | --- |
| Regal, *Phidippus regius*, female/male | Edwards describes plant/palm-frond retreats and tactile courtship in retreats. [UF/IFAS account](https://entnemdept.ufl.edu/creatures/misc/regal_jumping_spider.htm) | Female lookout pauses and male silk-tapping poses. Lookout timing is an artistic habitat adaptation; solo tapping borrows a social display. |
| Bold, *P. audax*, male/female | Foreleg courtship displays and sex-related feeding differences are summarized in the [University of Michigan account](https://animaldiversity.org/accounts/Phidippus_audax/) and [Missouri field account](https://mdc.mo.gov/discover-nature/field-guide/bold-jumper). | Male foreleg drumming and shorter meals; female longer meals and watchful pauses. The exact seconds and alternating rhythm are animation choices. |
| Zebra, *Salticus scenicus*, female/male | [Dill 1975](https://www.sfu.ca/biology/faculty/dill/publications/z75-153.pdf) measured stalking speed declining near stationary prey; experimental animals were males. | Both skins progressively slow their final stalk. Applying the measured pattern to the female skin is an explicit generalization, not a demonstrated sex comparison. |
| Tan, *Platycryptus undatus*, female/male | Flattened form and bark habitat: [Missouri account](https://mdc.mo.gov/discover-nature/field-guide/tan-jumping-spider). Natural-history notes describe exposed retreats and relatively inactive foraging: [Journal of Arachnology notes, 1988](https://repository.si.edu/bitstreams/684298c9-de4c-4769-b555-a3f6a55cd47f/download). | Low, still bark-like pauses and slower short patrols; both sexes share the trait. Exact stance and duration are designed. |
| Red-backed, *P. johnsoni*, male/female | Male visual dancing: [Jackson 1978](https://www.americanarachnology.org/journal-joa/joa-all-volumes/detail/article/download/JoA_v5_p185.pdf/?no_cache=1). Prolonged retreat use and males found outside more often: [Jackson 1979](https://www.americanarachnology.org/journal-joa/joa-all-volumes/detail/article/download/JoA_v7_p47.pdf/?no_cache=1). | Male raised-foreleg zigzag; female more frequent hammock checks. A short solo dance simplifies real courtship. |
| Emerald, *Paraphidippus aurantius*, female/male | Female offspring guarding: [Mooney & Haloin 2006](https://research.fs.usda.gov/treesearch/59222). Males visiting a female molting sac: [Thurlow 2016](https://peckhamia.com/peckhamia/PECKHAMIA_144.1.pdf). | Female nest-side checks; male silk inspection/taps. No offspring are simulated. The male report is a limited field observation; the tap animation is an adaptation. |
| Gray wall, *Menemerus bivittatus*, female/male | Field observations of hunting bees around a wall nest entrance: [Trindade-Santos et al. 2023](https://www.scielo.cl/scielo.php?lng=en&pid=S0718-89942023000400701&script=sci_arttext). | Brief edge patrols and broad watchful turns. Both sexes share this habitat-inspired choice; bee prey is not implemented. |
| Pantropical, *Plexippus paykulli*, male/female | Visual and nest-associated displays, with dense complex nests: [Jackson & Macnab 1989](https://www.tandfonline.com/doi/abs/10.1080/03014223.1989.10422565). | Male foreleg salutes, female silk-house inspections. Dense nests are a species trait; assigning more housekeeping to the female is an artistic choice. |
| Apache, *P. apacheanus*, male/female | Historical observations summarized in [Edwards 1980, p. 195](https://ufdcimages.uflib.ufl.edu/UF/00/09/93/77/00001/taxonomyethology00edwa.pdf) describe males wandering on leaves/twigs and females staying near retreats. | Male short scouting trips and female stronger home preference. These observations motivate tendencies, not universal rules or measured animation speeds. |
| *Portia labiata*, female | Web invasion and signals: [Jackson 1995](https://zslpublications.onlinelibrary.wiley.com/doi/abs/10.1111/j.1469-7998.1995.tb01789.x). Trial-and-error signals in aggressive mimicry: [Jackson & Cross 2013](https://zslpublications.onlinelibrary.wiley.com/doi/full/10.1111/jzo.12036). | Cautious approach, initially ignored probes, repeated double plucks and a lured resident. The app uses scripted response rules, not learning; its generic prey web, 3–4 minute construction and 1–2 minute wait are design choices. |
| *Hyllus diardi*, stylized green female | No strong species-specific movement or sex-comparison evidence was established in this quick review. | Slower, weightier pauses suit the illustrated larger body. Explicitly stylized; neither green coloration nor movement tempo is claimed as a verified natural phenotype. |

All skins retain shared jumping-spider basics: look/turn/pause, burst walking,
eight articulated legs, stalking, foreleg preparation, silk jumps, palpal grooming,
corner resting and hammock entry. A personality does not imply that other species
never show similar behavior. Display frequency, prey range, sleep schedule and
finger play are chosen for a usable desktop pet.

For implementation, see SpeciesBehavior.cs, WebEncounter.cs, SpiderPose.cs and
PalpPose.cs. The Behavior settings switches independently control personalities
and Portia web visits. There is no breeding or pet-management layer.
