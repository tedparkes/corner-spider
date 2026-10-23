# Changelog

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
