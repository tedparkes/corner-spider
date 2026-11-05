# Corner Spider 1.1 preview

An offline spider companion for your actual Windows desktop. This is **1.1.0-alpha.1**,
a local preview of the next version. The published [1.0 release](https://github.com/tedparkes/corner-spider/releases/tag/v1.0.0)
remains available and unchanged. This preview has not been published on GitHub.

Extract the Windows ZIP and open **JumpingSpider.exe**. Windows 10/11 with .NET
Framework 4.8; no installer, administrator rights or asset downloads. Startup is
opt-in. This community build is unsigned.

## New in this preview

- **46 choices in five categories:** Jumping spiders, Orb weavers, Widows, House
  spiders and Fictional. All 23 varieties have male and female options.
- **A lived-in home:** persistent reinforcement, repairs and small remains build
  up over active hours near the nest. Existing threads stay in place. New web
  families use radial orbs, irregular tangles, sheets and funnel openings, attached
  to both edges of the desktop corner.
- **Occasional molts:** return home, shed a visible old cuticle, emerge 16% larger,
  then gradually settle back to your selected size. Growth never accumulates.
- **Female life cycles:** a same-species male approaches, courts and mates. He
  usually leaves; some real-species visits end in a meal. The female becomes
  visibly gravid, lays an egg sac, and six tiny spiderlings hatch, grow near home
  and eventually disperse off screen. Babies do not reproduce.
- **Seven foods:** fly, gnat, moth, cricket, mealworm, beetle and small spider.
  Select one from the tray's **Choose food** submenu. Automatic food varies by
  spider type. Only one prey animal is present at a time.

![Representative new spiders](docs/new-families.png)

[All 46 options](docs/skins-light.png) · [Dark gallery](docs/skins-dark.png) ·
[Fresh and lived-in webs](docs/living-webs.png) · [Life around the nest](docs/life-cycle.png)

## Start and settings

Extract the whole Windows ZIP to a permanent writable folder. Open
**JumpingSpider.exe** or **Start-Spider.cmd**. The Source ZIP needs **Build.cmd**
first. Right-click the spider tray icon and choose **Settings**, or double-click
it. A single pet runs per Windows session, shared between 1.0 and this preview.

- **Appearance:** category and species, live preview, 20–180% size, web visibility,
  light/dark background and preview rotation. The corner thumbnail uses the same
  web artwork as the desktop.
- **Life & home:** enable or disable future molts and mating visits, adjust their
  intervals, read the current stage, or **Invite a mate** for an available female.
- **Finger:** skin tone from deep brown to pale, with a live preview.
- **Behavior:** occasional prey, finger play, interaction distances, hover delay,
  movement tolerance, species personalities and Portia's web encounter.
- **Desktop:** nest corner and home monitor, one/all monitors, optional startup.
  The rightmost monitor remains the default home.

Defaults use **active simulation time**, not elapsed time while the PC is off:

| Event | Default |
| --- | --- |
| First extra home reinforcement | 30 minutes near home; later layers more slowly |
| Molt | Every 6 active hours, waiting for a quiet moment |
| Shedding and recovery | About 45 seconds, then 90 seconds |
| Female mating visit | After 8 active hours; another visit waits until after the brood |
| Becoming gravid | 30 minutes after the visit |
| Egg incubation | 45 minutes after laying |
| Spiderlings near the nest | 90 minutes, then about 75 seconds to disperse |

Molt interval is adjustable from 2–24 hours; mating interval from 4–48 hours.
Pausing, sleeping/locking Windows and closing the app stop the clocks. Switching
skins pauses that skin's life history. Returning to it restores progress. Disabling
an option prevents future events; an event already under way finishes. Resetting
preferences keeps accumulated silk, meals and life history.

These are species-inspired illustrations and deliberately compressed life events.
The repeating molt/size cycle is fictional, and mating outcome weights are artistic
choices, not biological statistics. Fictional males always leave. Orb-web aging
represents enduring supports and repairs; real capture webs may be replaced.
[Research and adaptations](docs/LIFE-RESEARCH.md) explain the choices.

## Species and webs

- **Jumpers:** Regal, Bold, Zebra, Tan, Red-backed, Emerald, Gray wall, Pantropical,
  Apache, Portia labiata and Hyllus diardi, all with both sexes. The green female
  Hyllus remains a labeled stylized variant; the new male is brown.
- **Orb weavers:** Yellow garden (Argiope aurantia), Cross (Araneus diadematus),
  Marbled (Araneus marmoreus) and Golden silk (Trichonephila clavipes).
- **Widows:** Southern black, Brown and Redback. The dorsal drawings do not place
  an underside hourglass on the back. Brown widows have spiky egg sacs.
- **House spiders:** Common house (Parasteatoda tepidariorum) and Barn funnel
  weaver (Tegenaria domestica).
- **Fictional:** Nebula weaver, Crystal spinner and Copper clockwork.

The new families have small eyes/front bodies and longer, thinner legs rather
than enlarged jumper faces. Web dwellers remain near home and walk to prey caught
in the web; jumping spiders retain long stalking approaches and pounces. Original
species IDs, procedural drawings and varied silk retreats are preserved.

## Play and food

Hold the cursor near an awake, available spider. The finger begins to appear after
4.5 seconds by default. Small hand jitter is tolerated. The spider chooses a
landing spot, walks around, sometimes waves, or catches the exposed fingertip and
climbs up. Carry it gently. Clicking plans an upward departure; fast movement
triggers an immediate escape with a silk dragline. Clicks still reach your apps.
The pointer returns on departure, pause or exit, with a watchdog for interruptions.

Life events briefly take priority over food and finger offers. A male visitor and
up to six babies are family decorations with behavior, separate from the single
prey slot. Feeding does not replace live prey. The quick tray feed remains a fly
(or a small spider for Portia); **Choose food** exposes all seven foods for any
species. Automatic web-dweller food is mainly winged insects and beetles; jumpers
also encounter crawling foods. Portia still favors small spiders.

Portia retains its longer corner-web hunt: **Behavior > Invite a web-building
spider** starts construction over 3–4 minutes, followed by a 1–2 minute wait.
Portia approaches an anchored side, plucks five times, watches the resident creep
closer, then attacks directly and briefly struggles with it. That web stays until
Portia leaves. The encounter shares the single prey slot and waits during life events.

## Saves, privacy and removal

This preview uses **%LOCALAPPDATA%\CornerSpider-Next\settings.txt** and a previous-save
`.bak`. On first launch it copies existing 1.0 preferences from
`%LOCALAPPDATA%\CornerSpider\settings.txt` if available; it leaves that original
save untouched. Normal quit saves progress. Use **Stop-Spider.cmd** or Quit in the
tray. To switch back to 1.0, quit the preview and start the original installation.

Everything runs locally. No telemetry, accounts, network calls, keyboard logging
or mouse injection. The optional **Buy me a coffee / Monster** button opens the
original bundled Cash App QR only, with no username label or payment link.
[Support details](docs/SUPPORT.md). Support is optional.

To uninstall, disable Start with Windows, quit, delete the app folder and optionally
its preview save folder. Startup is per-user and remains off by default. If moving
an installation with startup enabled, disable it first and re-enable from the new
location. Exclusive fullscreen apps and secure Windows screens can cover the pet.

## Build and verification

Run **Build.cmd** on Windows. It uses the installed .NET Framework compiler,
executes deterministic behavior/persistence tests and compiles WPF. No NuGet
packages or external artwork dependencies. Quit the pet before rebuilding.

```bat
Build.cmd
JumpingSpider.exe --self-test "C:\Temp\CornerSpiderChecks"
JumpingSpider.exe --life-preview "C:\Temp\CornerSpiderLife"
JumpingSpider.exe --release-preview "C:\Temp\CornerSpiderArt"
JumpingSpider.exe --desktop-test "C:\Temp\CornerSpiderManual"
```

Self-tests use isolated saves. Artwork commands render plain backgrounds without
capturing your desktop. The interactive fixture supports hands-on cursor testing.
[Preview verification](docs/VERIFICATION-NEXT.md) separates completed automated and
native tests from physical display/suspend and long-duration checks still needed.
The preserved [1.0 verification](docs/VERIFICATION.md) applies to 1.0 only.

**Package.ps1** creates entry-verified Windows and Source ZIPs, SHA-256 checksums
and draft release notes containing the full [changelog](CHANGELOG.md). It does not
publish. The next release retains WPF/.NET Framework for a small dependency-free
Windows app. See [contributing](CONTRIBUTING.md), [release instructions](RELEASING.md)
and [future work](ROADMAP.md).

Free software under the [MIT License](LICENSE). Original procedural artwork is
included; research photographs are not redistributed.
