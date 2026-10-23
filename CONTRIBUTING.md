# Contributing

Keep changes local, small and compatible with Windows .NET Framework 4.8.
There are no package dependencies. The compiler in Build.cmd supports C# 5;
do not introduce newer syntax without an explicit toolchain migration.

Run Build.cmd. Behavior changes need deterministic checks that exercise outcomes.
Use --self-test on a Windows desktop for overlays, settings and resource cleanup.
Inspect generated previews after artwork changes. State what was actually tested.

Preserve click-through behavior, focus, one pet instance, opt-in startup, prior-save
migration and the one-insect limit. Never make the mouse observer block or consume
input. Keep a full cleanup path for hooks, icons, timers, overlays and cursor helpers.

Skin profiles use stable IDs. Keep Regal, Bold and Zebra IDs for old saves.
Place new colors and species metadata in SkinCatalog.cs and markings in PetArt.cs.
Reference images are research material, not assets to copy into the repository.

Issues should include the app version, Windows version, monitor layout/scaling,
steps to reproduce, expected result and actual result. Remove personal paths from
logs before attaching them. Do not submit private saves or desktop screenshots
without reviewing their content.
