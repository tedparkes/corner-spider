# Preparing a GitHub release

The prepared source bundle is ready for a public repository. Nothing is published
by the local packaging script or build workflow.

1. Create an empty public repository named corner-spider under your chosen
   GitHub account. Review LICENSE and the public README before uploading.
2. Use the prepared local repository on branch `main`, or extract the source
   bundle into a new repository. The source includes .github, .gitignore, LICENSE
   and docs. Connect your chosen GitHub repository and push the reviewed commit:

   ```text
   git remote add origin YOUR_REPOSITORY_URL
   git push -u origin main
   git push origin v1.0.0-rc.8
   ```

   The prepared local repository already has the release tag. For a newly
   initialized source extraction, commit the files and create that tag first.
   Do not upload the surrounding workspace, personal saves, backups or reference
   photographs. Keep release ZIPs as release attachments, outside source history.
3. Run Build.cmd on Windows and the native --self-test command. Complete the
   manual checks in docs/VERIFICATION.md. The hosted workflow runs behavior checks
   and packaging; it does not substitute for an interactive Windows session.
4. Run: `powershell -NoProfile -File .\Package.ps1`
   If local script policy blocks execution, use a PowerShell session permitted by
   your own policy; no permanent policy change is part of this project.
5. Create a draft release tagged v1.0.0-rc.8 (mark it as a prerelease), targeting the reviewed commit. Use the
   notes in CHANGELOG.md. Attach the Windows ZIP, source ZIP and SHA256SUMS.txt from
   dist. The generic GitHub source archives do not contain the prebuilt executable.
6. Check the draft's files and links, then publish when ready.

The portable executable is unsigned. Do not claim it is signed, security-certified
or tested on systems that were not exercised. Code signing is a future improvement.

Before final 1.0, complete the manual checklist and update the informational
version from 1.0.0-rc.8 to 1.0.0. Retest the exact release contents. The optional
support QR intentionally identifies the creator; review docs/SUPPORT.md before
redistributing a fork. Keep this recipient or replace both QR and link deliberately.

Version sources: AssemblyVersion/FileVersion and AssemblyInformationalVersion in JumpingSpider.cs, displayed app/about text,
README files and CHANGELOG. Package.ps1 reads the version from the compiled app.
Prior releases and hashes should be retained rather than silently replaced.
