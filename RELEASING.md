# Preparing a GitHub release

The prepared source bundle is ready for a public repository. Nothing is published
by the local packaging script or build workflow.

Changes after a preserved tag belong to the next release. Choose its version,
update the version sources below and tag the reviewed commit containing the new
documentation. Replace `vNEXT_VERSION` below with that tag. Keep the existing RC8
tag and ZIPs unchanged; documentation previews built from RC8 are drafts.

1. Create an empty public repository named corner-spider under your chosen
   GitHub account. Review LICENSE and the public README before uploading.
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
   It contains the AI-assisted development introduction and the **complete**
   changelog, including the early prototype, inside an expandable section.
   For the first public release, keep that full history; GitHub's automatic
   commit summary covers only the later work because Git began at RC3.
6. Attach the Windows ZIP, source ZIP, SHA256SUMS.txt, CHANGELOG.md and
   DEVELOPMENT-HISTORY.md from dist. The generic GitHub source archives do not
   contain the prebuilt executable. Both ZIPs contain CHANGELOG.md at the root
   and DEVELOPMENT-HISTORY.md under docs.
7. Check the draft's expanded changelog, files and links, then publish when ready.
   Add a link to `docs/DEVELOPMENT-HISTORY.md` at the release tag in the chosen
   public repository so readers can view its images and source-relative links.
   Keep the story focused on public development milestones; no raw conversation,
   private reference photos, personal paths or guessed usage/cost figures.

The portable executable is unsigned. Do not claim it is signed, security-certified
or tested on systems that were not exercised. Code signing is a future improvement.

Before final 1.0, complete the manual checklist and update the informational
version from 1.0.0-rc.8 to 1.0.0. Retest the exact release contents. The optional
support QR intentionally identifies the creator; review docs/SUPPORT.md before
redistributing a fork. Keep this recipient or replace both QR and link deliberately.

When bumping the final version, move completed Unreleased entries into its
changelog section, refresh docs/VERIFICATION.md and regenerate the packages and
release body together. Maintain docs/DEVELOPMENT-HISTORY.md as further iterations
land. The existing numbered entries and prototype history stay in the changelog.

Version sources: AssemblyVersion/FileVersion and AssemblyInformationalVersion in JumpingSpider.cs, displayed app/about text,
README files and CHANGELOG. Package.ps1 reads the version from the compiled app.
Prior releases and hashes should be retained rather than silently replaced.
