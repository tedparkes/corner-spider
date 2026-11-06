CORNER SPIDER 1.1.0-alpha.13 - COMMUNITY PRERELEASE

Extract the whole Windows ZIP, then open JumpingSpider.exe or Start-Spider.cmd.
Requires Windows 10/11 and .NET Framework 4.8. No installer or admin rights.
This unsigned preview is separate from the published 1.0 release.
Male orb weavers roam for mates and may wait at the edge of a female's web.
The spider hops between monitors and continues its route after landing.
Corner webs use new anchored frames, sheets, funnels and retreats. See
docs/VERIFICATION-CORNER-WEBS.md for the scored design review.
Web strands now use supersampling and higher-quality downscaling for smoother
edges. See docs/VERIFICATION-WEB-AA.md for this build's rendering checks.
Prey webs now complete their frame and straight spokes before the interior
spiral. The builder travels along existing silk and returns to the center.
See docs/VERIFICATION-WEB-BUILD.md for checks on prey-web construction.

Right-click the spider tray icon > Settings, or double-click the icon.
Appearance: five categories, 46 male/female choices, 20-180% size and web opacity.
Life & home: infrequent molts, female mating visits, interval sliders, current
stage and Invite a mate. Only an available female can accept a visit.
Choose food in the tray: fly, gnat, moth, cricket, mealworm, beetle, small spider.
At most one prey is present. Feeding waits during vulnerable life events.

Webs gain lasting support threads and small remains over active hours near home.
Orb weavers, widows and house spiders have appropriate capture-web styles and
stay near their silk. Jumpers keep their original retreats and stalking behavior.
Three fictional varieties: Nebula weaver, Crystal spinner and Copper clockwork.

Default active-time schedules:
- Molt every 6 hours; 45 seconds shedding and 90 seconds recovery.
- Emerge 16% larger, then slowly return to your chosen size before the next molt.
- Female mating visit after 8 hours, waiting for a quiet moment.
- Usually the male leaves; some real-species visits end in a meal.
- 30 minutes becoming gravid, 45 minutes incubation after laying a sac.
- Six young grow near the nest for 90 minutes, then disperse off screen.
Pausing, closed sessions and Windows suspension stop these clocks. Each skin
keeps its own progress. Babies never reproduce. Life events are stylized and
compressed; the repeating growth/shrink cycle is intentionally fictional.

Hold the cursor nearby for about five seconds to offer the finger. Move gently
to carry the spider. Clicking makes it plan an upward departure; fast motion
startles it into jumping away. Your clicks still reach the desktop beneath.
Use Go home, Wake, Pause or Quit from the tray. Startup remains opt-in.

PREVIEW SAVES
%LOCALAPPDATA%\CornerSpider-Next\settings.txt
The preview copies 1.0 preferences once, without changing the original save in
%LOCALAPPDATA%\CornerSpider. Quit this preview before opening 1.0 again.
Stop-Spider.cmd requests a graceful quit. Only one pet runs per Windows session.
To remove: disable startup, quit, then delete this folder and optionally its save.

SOURCE BUILD
Run Build.cmd. It runs the behavior tests before compiling the WPF executable.
Package.ps1 creates verified Windows and Source ZIPs and checksums, without
publishing. README.md has the complete instructions and diagnostics commands.

Read docs\VERIFICATION-NEXT.md for test results and remaining manual checks.
Read CHANGELOG.md for the complete version-by-version development history.
Plain-background galleries are in docs\new-families.png, living-webs.png,
life-cycle.png and skins-light.png. No desktop screenshots are included.

Free MIT-licensed software. Pet data stays local; no telemetry. Optional update
checks contact GitHub. Automatic checks are off by default.
The optional support button shows only the original Cash App QR.

UPDATES: Settings > Updates offers Check now and optional daily checks. Enable
the tray shortcut there to use Check for updates from the tray menu. Preview
builds enable the shortcut by default, and full releases default to disabled.
Include prereleases in regular checks to consider stable and preview builds.
Check for prerelease looks for the latest compatible preview once. Choose
Download and restart when a newer build is offered. Version 1.0 users can move
to the preview with a manual ZIP download.
Life & home adds Let babies stay and build their own nests, with a 1-12 resident
limit. Residents grow, move independently and persist across sessions. Switching
it off sends them away gradually. README.md explains updates and resident homes.
