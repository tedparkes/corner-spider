# Preparing a GitHub release

The public repository is https://github.com/tedparkes/corner-spider.
Nothing is published automatically by the local packaging script or build workflow.

Changes after a preserved tag belong to the next release. Choose its version,
update the version sources below and tag the reviewed commit containing the new
documentation. Replace `vNEXT_VERSION` below with that tag. Keep the existing release
tags and ZIPs unchanged.

1. Use the existing corner-spider repository, or create an empty repository for a
   deliberate fork. Review LICENSE and the public README before uploading.
2. Use the prepared local repository on branch `main`, or extract the source
   bundle into a new repository. The source includes .github, .gitignore, LICENSE
   and docs. Connect your chosen GitHub repository and push the reviewed commit:

   ```text
   git remote add origin YOUR_REPOSITORY_URL
   git push -u origin main
   git push origin vNEXT_VERSION
   ```

   Commit the release contents and create the matching tag before pushing it.
   Do not upload the surrounding workspace, personal saves, backups or reference
   photographs. Keep release ZIPs as release attachments, outside source history.
3. Run Build.cmd on Windows and the native --self-test command. Complete the
   manual checks in docs/VERIFICATION.md. The hosted workflow runs behavior checks
   and packaging; it does not substitute for an interactive Windows session.
4. Run: `powershell -NoProfile -File .\Package.ps1`
   If local script policy blocks execution, use a PowerShell session permitted by
   your own policy; no permanent policy change is part of this project.
5. Create a draft release using that version's tag and reviewed commit. Mark
   release candidates as prereleases. Use `dist/RELEASE-NOTES.md` as its body.
   It contains the complete changelog, organized by version with changes and fixes.
   Keep the full history in the first public release, including the early builds.
6. Attach the Windows ZIP, source ZIP, SHA256SUMS.txt and CHANGELOG.md from dist.
   The generic GitHub source archives do not contain the prebuilt executable.
   Both ZIPs contain CHANGELOG.md at the root.
7. Check the draft's changelog, files and links, then publish when ready.

## Preserve the published wording

Before editing an existing release or preparing its successor, fetch the current
GitHub release body and README. The maintained public text is the editorial
baseline; generated notes are a draft. Preserve manual wording changes and
deletions, and merge only the intended additions. Do not replace a live post with
an older local copy or restore removed text while adding screenshots or fixes.

Keep the screenshot introduction brief: **All 20 skin options across 11 species**
(update the counts when the app changes), followed by the combined gallery.
Keep the collapsible silk-retreat comparison. Omit the removed explanation about
plain backgrounds, enlarged views and personal windows, and the extra gallery
download/dark-background link row. The images can remain available as assets.
Apply these choices when reviewing generated release notes for later versions.

The portable executable is unsigned. Do not claim it is signed, security-certified
or tested on systems that were not exercised. Code signing is a future improvement.

Before each release, update the version and verification notes. Retest the exact
release contents and record which manual checks remain unverified. The optional
support QR intentionally identifies the creator; review docs/SUPPORT.md before
redistributing a fork. Keep this recipient or deliberately replace and verify the QR.

When bumping the final version, move completed Unreleased entries into its
changelog section, refresh docs/VERIFICATION.md and regenerate the packages and
release body together. Keep earlier versions in the changelog.

Version sources: AssemblyVersion/FileVersion and AssemblyInformationalVersion in JumpingSpider.cs, displayed app/about text,
README files and CHANGELOG. Package.ps1 reads the version from the compiled app.
Prior releases and hashes should be retained rather than silently replaced.
