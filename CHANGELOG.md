# Changelog

Corner Spider grew through repeated human feedback, AI-assisted implementation,
Windows builds, visual review and testing. Read the
[development history](docs/DEVELOPMENT-HISTORY.md) for the problems and decisions
behind these revisions. This log includes the early prototype work as well as
the numbered releases. The local Git repository began at RC3; earlier entries
were reconstructed from preserved project files and development feedback.

## Unreleased

- Document the early prototype and the iterative development process. Include
  the complete changelog and development history in the first public release.
- Generate a ready-to-post release body and standalone history documents with
  each package, alongside the Windows/source ZIPs and checksums.

## 1.0.0-rc.8

- Give the resident web's open edge a smooth, gently concave arc toward the
  corner. Keep both desktop anchors fixed and preserve smooth tangents where
  spokes meet the edge. The builder follows that same continuous arc.
- Keep the resident, plucking contact and capture silk attached to the revised
  frame. Verify curve continuity, inward bow, complete hunts and WPF rendering.

## 1.0.0-rc.7

- Reshape the resident prey web into a supported corner web with eleven direct
  attachments along the two desktop edges. Join the open rim continuously from
  one edge to the other, bowed inward within the supported span.
- Move the resident hub and Portia's plucking contact with the shared artwork;
  retain progressive weaving, corner reservation, hunting and departure fading.
- Check frame connectivity, physical edge contact, supported bounds, all four
  mirrored corners and native rendering. Add a four-corner preview.

## 1.0.0-rc.6

- Replace the resident prey's crossed-line capture mesh with an orb-style web:
  anchored frame, uneven radial spokes, curved spiral silk and a clear hub.
- Weave along the same curves used by the artwork. Weight construction by strand
  and travel length to preserve smooth movement during the multi-minute build.
- Keep the enlarged footprint, central resident, edge plucking, reserved corner
  and departure-triggered fade. Portia's plucking point lies on a real radial spoke.
- Retain completed strands as one image generated from vectors per world, sized
  for the display. Growth updates it in batches; ordinary redraws and fading reuse
  it. Add geometry, construction,
  native rendering/cache checks and dark/light web previews.

## 1.0.0-rc.5

- Choose a varied upper-surface finger landing near the approach, rather than
  making every spider grab the same tip. Briefly absorb the landing before
  exploring, keeping the chosen position and travel-facing direction.
- Allow a front-leg catch and dangle only from the exposed fingertip side.
  Those approaches can also choose to land on top. Base, upper and middle
  approaches always land on the surface; mirrored fingers follow the same rule.
- Ease small pointer drift into the landing instead of snapping to a fixed
  perch. Keep gentle carrying, waving, upward click departures and fast escapes.
- Add deterministic boarding coverage and a rendered approach/jump/touchdown
  gallery. Update hanging-leg fixtures to approach from the exposed tip side;
  retain their foot contact, suspension and departure assertions.

## 1.0.0-rc.4

- Enlarge the resident spider's web to 300% of its previous span, fitting down
  only when a small work area or large pet size needs an approach lane.
- Enlarge spider prey by 50%, still distinctly smaller than Portia. The resident
  waits at the central web junction while Portia approaches an outer anchor,
  touches strands with its forelegs, and lures the resident along a connected line.
- Reserve the occupied corner during building and waiting. Replan pending patrols
  and route passing patrols and homeward walks around the web's footprint.
- Keep the spent web fully visible while Portia eats or rests on it. Its 12-second
  fade starts after the entire spider has left; calling the hunter home preserves
  the web during departure. Settings/display cancellation still cleans up promptly.
- Extend deterministic checks for full-size and fitted webs, central waiting,
  plucking clearance, patrol avoidance, delayed fade and complete encounters.

## 1.0.0-rc.3

- Appearance now previews the selected species' silk retreat in the upper-right
  corner, attached to both preview edges. Skin, size and background changes update
  it live; rotating the spider leaves the retreat in its corner.
- The preview uses the same retained silk artwork as the desktop and keeps the
  existing nest progress untouched. Narrow settings windows retain a fitted view.
- Use frozen vectors for the small silk preview, avoiding full desktop-sized image
  allocations. Closing the preview releases its artwork and stops its timer.
- Prepared the local source repository and updated release packages.

## 1.0.0-rc.2

- Reworked the sleeping hammock's support: spread attachment bundles across both
  desktop edges, add connecting cross ties and contact patches, and join the
  support mesh into the sleeping sleeve. End entrances and saved silk remain.
- Added species silk profiles, including lighter crevice sleeves, a denser
  Pantropical tube and an open Portia scaffold. Notes distinguish observed
  structure from artistic choices; most male/female pairs share a profile.
- Species changes replace the retained silk artwork immediately. Silk growth adds
  threads without rearranging existing ones; only the current two layers are cached.
- Replaced the small resident's polygon-like web with an irregular network that
  reaches the corner edges. Its construction path uses the updated thread geometry.
- Added native image checks for both-edge attachment across all twenty skins/four
  corners, preserved transparency, artwork reuse and stable thread growth.

## 1.0.0-rc.1

- Portia's occasional web resident builds connected silk for 3–4 minutes, waits
  1–2 minutes, then responds to Portia's exploratory plucks before an ambush.
  The sequence uses one prey slot, supports naps and finger play while building,
  works across monitors and cleans up on settings/display changes.
- Added a manual web invitation and separate personality/web-visit switches in
  Behavior settings. The normal feed action remains an immediate ordinary hunt.
- Added researched personality variations across all twenty skins, including
  foreleg displays, zigzag steps, cautious stalking and retreat preferences.
  Appearance explains each choice; research notes separate evidence from styling.
- Added the creator's original Cash App QR and optional coffee / Monster banner,
  enlarged QR view and explicitly clicked browser link to $tedparkes. The original
  phone screenshot, payment credentials and payment software are not bundled.
- Fixed early hunting after naps during web construction and approach resumption
  after crossing a gap between monitors. Retained long pounces for normal prey.
- Added 48 complete timed web-hunt scenarios plus interruption, personality,
  single-prey, persistence, anatomy and native settings checks. Validated the
  supplied QR at small sizes and from actual WPF-rendered settings windows.
- Prepared local Windows/source release candidates. No public release is uploaded;
  physical mixed-DPI, suspend/unplug and hands-on play checks remain before 1.0.

## 0.7.2

- All twenty skins launch hunting pounces from farther back. The shared aim
  radius increases from 82 to 225 logical pixels: after the mouth offset, the
  actual leap is about 200 pixels instead of 57, scaled for pet size and DPI.
  Both fly and small-spider prey use the change; nearby prey permits shorter hops.
- Retained distant stalking, aim, foreleg reach, crouch, quick flight, landing
  and eating. Long pounces have a slightly higher arc, capped at the desktop
  ceiling so hunting along the top edge keeps the entire spider visible.
- Fixed a monitor-seam routing error that mistook planted turning/braking feet
  for a blocked edge and could trigger an unnecessary transit jump.
- Added 4,574 hunt scenarios across every skin, both prey, eight directions,
  sizes, DPI scales and frame intervals, plus ceiling and mixed-DPI seam cases.
  Existing behavior checks remain intact. Added a fixed-camera pounce preview.

## 0.7.1

- Portia's automatic prey now mixes small spiders (65%) and flies. Other skins
  keep fly spawning. The existing interval, enable/disable switch, distance range
  and one-prey limit apply to both types.
- Portia's feeding action becomes "Release a small spider". The saved species
  activates its food preference on startup and updates live when changed.
- Original vector prey with eight articulated legs, brief crawling steps and
  pauses. Portia uses the full existing stalk/pounce sequence, then holds the
  folded spider at its mouth while eating. No blood or additional effects.
- Live and captured prey identities remain separate: switching skins or feeding
  during eating cannot transform an existing animal or held meal.
- Added spawn/selection, complete hunts, size/DPI movement and artwork, timeout,
  display removal, single-slot and native tray/persistence checks.

## 0.7.0

- Twenty skin options: opposite-sex counterparts for the eight remaining original
  species, Portia labiata female and a stylized green Hyllus diardi female. Original
  persistent IDs are retained; the original nine species now have both sexes.
- Species profiles control abdomen contour and size, head proportions, walking
  limb lengths and thickness, stance and chelicera size. Portia has slim fringed
  legs; Hyllus has a broad head and longer full abdomen. Male/female proportions
  and markings differ, including the male zebra's long chelicerae.
- Separate articulated palps with slender female tips and fuller male bulbs.
  Alternating sideways eye wipes and chelicera strokes replace the former jitter;
  walking forelegs stay planted during grooming. Preview demonstrates the cycle.
- Finger boarding completes a smooth airborne turn toward the grip, avoiding an
  initial backwards catch with the front feet out of reach.
- Added all-profile joint/bounds, folded-retreat fit, finger-grip, eight-direction
  catch, grooming contact/continuity, legacy-ID and save/reload checks. Native
  render checks cover four poses per profile and distinct silhouette geometry.

## 0.6.4

- The spider routes around the cocoon, lines up at an end opening, pauses and
  crawls head first inside before folding its legs to sleep.
- Silk only covers a spider that has entered the retreat. Its head, body and
  abdomen pass beneath the existing cover in sequence, without an opacity fade.
- Waking unfolds the legs before crawling out through the opposite end. Feeding
  an inside spider also sends it out through an end before it starts hunting.
- The cocoon keeps its shape and size. Its corner position leaves enough room
  for the full entrance and exit poses; tiny work areas retain bounded behavior.
- Added four-corner entrance/exit, path, occlusion, limb-clearance, size/DPI,
  frame-step and interrupted-entry tests, plus actual WPF sequence previews.

## 0.6.3

- Clicking during a finger ride now plans a landing above the finger, turns
  toward it, briefly braces and makes an upward jump with a silk trail.
- Clicking during the catch/climb first brings the spider onto the finger.
  The offered finger stays still until takeoff, while the real pointer returns
  immediately. Later mouse movement cannot drag the launch point or landing.
- Near the top edge, the spider chooses a safe sideways landing at the highest
  available position. The upward arc keeps its legs inside the desktop boundary.
- Fast-motion escapes remain immediate. Added departure tests covering catches,
  climbs, full rides, repeated clicks, screen edges, mixed DPI and cleanup.

## 0.6.2

- Added a Finger settings tab with a live skin-tone slider, from deep brown to
  very pale, and a large preview on light or dark backgrounds.
- The 101 shades adjust skin, shading, creases and nail colors together while
  preserving the finger silhouette, pointer alignment and transparent hand fade.
- The choice persists between sessions. Old saves retain the original shade;
  resetting preferences restores it without changing nest or meal progress.
- Finger rendering replaces one cached image as the slider moves rather than
  retaining an image for every shade. Added save, migration and renderer checks.

## 0.6.1

- Replaced the regular hammock lines with an irregular, translucent silk pocket,
  fine crossing fibers, loose entrance rims and attachments to both corner edges.
- The spider turns diagonally across its selected corner, then lowers and folds
  its feet into the retreat. Waking unfolds the same pose smoothly.
- Silk is drawn behind and over the spider so it rests inside the pocket. Nest
  progress still controls silk density and is preserved in existing saves.
- Added four-corner settling, wake, return and display-reflow tests, including
  fixed leg lengths at 20-180% sizes and simulated mixed DPI.
- Added renderer-generated corner and settling previews. Silk retains only the
  current pair of drawing layers and 4x image caches as the nest grows.

## 0.6.0

- Consolidated tray preferences into Appearance, Behavior and Desktop settings.
- Added animated skin preview, light/dark backgrounds, rotation and detail mode.
- Size slider now supports 20-180%.
- Added sliders for fly timing, finger delay, approach and dismissal distances,
  and fast-movement tolerance; settings persist and migrate from earlier saves.
- Added seven skins: Regal male, Tan, Red-backed male, Emerald, Gray wall female,
  Pantropical male and Apache. Revisited the original Regal, Bold and Zebra coats,
  with separate palette regions, markings and body/leg proportions.
- Refined the finger's nail profile, soft pad shading and creases, retaining its
  centered position, transparent hand fade and cursor restoration.
- Extended cursor-history retention for long configurable invitations.
- Prepared MIT licensing, source/portable packages, checksums, Windows CI and
  contribution/release documentation.

## 0.5.2

- Patrols and trips home respond to nearby cursors independently of greeting
  animation cooldowns; an interrupted route resumes when the cursor leaves.
- Moved the mouse observer onto its own message-loop thread with explicit cleanup.

## Prototype 0.1 and early Windows iterations (before 0.5.2)

These milestones summarize the original README, preserved project work and
development feedback. Intermediate version numbers are not assigned retroactively.

- Started with a C# WPF transparent overlay, procedural spider, autonomous
  hunting, cursor rides, a growing hammock, tray controls and local saves.
  The original Linux-authored prototype explicitly lacked Windows compilation
  and runtime verification; those were follow-up work.
- Preserved the prototype, built it on Windows and ran its behavior checks.
  Added regression coverage and native diagnostics for overlay transparency,
  click-through behavior, focus, settings, cursor restoration and cleanup.
- Expanded exploration across monitors, preferred the rightmost home screen,
  and worked on display scaling, reflow, resume handling and persistence.
  Replaced the generic tray symbol with a spider icon and provided a desktop
  shortcut to the runnable build. Startup remained opt-in.
- Added the original Regal, Bold and Zebra appearances, then repeatedly revised
  rounded bodies, fuzz, proportions, eye placement, palps and front/back readability.
- Reworked walking into short bursts, deliberate pauses, inquisitive turns and
  corner perching. Revised long forelegs, paired forward/upward reaches and the
  projected joint angles after feedback on sideways-looking bends.
- Made hunting more readable through notice, turn, stalk, pause, crouch, jump,
  capture and eating. Allowed prey farther away while retaining occasional
  spawning and one prey animal at a time.
- Developed cursor play from an attached ride into a voluntary finger offer:
  tolerate small pointer jitter, wait nearby, reach, jump, hang, climb and ride.
  Added gentle carrying, facing/waving, click departures and startled escapes
  with a silk trail. Later releases refined boarding and departure further.
- Reshaped the finger using a supplied pose reference, faded the hand end into
  transparency, centered it over the pointer and restored the pointer on exit.
  Personal reference photographs were kept out of the distributed artwork.
- Improved silk visibility and cursor attention during long edge patrols.
  Kept the Windows desktop overlay and local procedural artwork throughout.
