# Verification for 1.0.0-rc.10

Local Windows verification, October 23, 2026. RC9 source, executable, packages and
Git tag were preserved. An unpublished plucking-only RC10 draft was also preserved
before the subsequent changes.

## Automated behavior

All deterministic suites pass: 4,574 pounce hunts, 183,358 corner-web checks and
56 complete web hunts, plus the existing movement, play, morphology, persistence
and cleanup suites. Sizes span 20–180%, frame steps 16–100 ms, simulated mixed DPI,
adjacent/separated displays, all corners and fitted portrait/landscape webs.
See [behavior output](behavior-checks.txt).

Portia stands on silk beside an anchored end. One forefoot contacts an existing
strand while seven support feet remain planted. Each of five signals precedes a
prey advance and listening pause. Checks measure foot contact, joint lengths,
turning, capture clearance and departure before fading. RC9 web geometry remains.
Five signals are an animation choice, not a fixed biological rule.

Portia attacks in 130–190 ms with at most five logical pixels of lift. Tests check
the flight, frozen prey, silk and mouth contact. Travel/finger jumps and other
species retain their arcs; changing species in flight does not reshape an attack.
Spider prey triggers a bounded 1.35-second struggle before eating. It stays at the
landing, counts once, and manual feeding cannot replace the held meal.

Settings checks cover opacity defaults, invalid values, bounds and save/reload.
Nest progress is independent of visibility. Morphology checks cover all 20 skins,
all states, joint lengths, bounds, palp contacts and nesting.

## Windows and artwork

The actual desktop native suite passes with zero failures. See
[Windows diagnostics](windows-checks.txt). This run used one connected 192-DPI
screen; multiple-monitor and mixed-DPI geometry was also simulated. Native checks
cover transparent nonactivating overlays, OS click-through hit tests, tray actions,
mouse hooks, cursor hiding/restoration, heartbeat loss, crash recovery and cleanup.
Settings checks cover skins/size, live opacity, save/reset, the donation flyout
and its original QR. No payment page or Windows startup change is made by tests.

An initial restricted-desktop run failed eight cursor-hide checks. The actual
desktop rerun passed those checks without weakening them. It is not reported as a
passing first run.

Silk rendering checks cover 0/50/100% visibility, restoration of original alpha,
retained artwork, bounded construction allocations, attachments, transparent
backgrounds and no added off-web plucking tethers. All skins render unclipped in
multiple poses with distinct artwork and silhouettes. Settings, flyout, skin,
size, pounce and encounter previews were rendered and inspected. The QR in the
actual flyout capture decodes to the intended recipient.

An independent critic reviewed all 20 skins and the revised galleries; see
[review and references](SKIN-REVIEW.md). This is visual refinement, not biological
certification. Fine markings are intentionally simplified at tiny sizes.

Windows/source archives are verified entry by entry. Extracted source rebuilds;
the extracted portable executable reproduces the updated encounter, prey and skin
previews. Git source matches the source package. The desktop shortcut points to
the installed RC10 binary. The hosted download is private to the user's ChatGPT
account; no public GitHub release has been published.

Native resource diagnostics stress 60 icon replacements and report CPU, memory
and handles. They load every skin/preview and are not an idle-pet benchmark. A
separate short normal-launch sample is recorded in release verification. The older
[web resource soak](web-load-checks.txt) remains RC7 evidence, not a new RC10 soak.

## Manual follow-up

1. Select Portia and invite a web-building spider. Watch the full real-time build,
   five signals, advancing prey, direct attack, brief struggle and departure fade.
2. Try opacity on light/dark wallpaper and another monitor. Check tiny skins and
   confirm 100% matches the original silk appearance.
3. Scan the flyout QR with a phone, and dismiss it with an outside click or Escape.
   No actual donation was tested.
4. Check finger feel, physical mixed-DPI changes, unplug/reconnect and sleep/resume.
   These physical interactions and a long RC10 resource soak remain unverified;
   simulated tests do not replace them.
