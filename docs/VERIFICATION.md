# Verification for 1.0.0-rc.14

Local Windows verification, October 27, 2026. RC13 packages, source and Git tag
are preserved. This release simplifies the optional support flyout only.

The Windows .NET Framework build and complete deterministic behavior suite pass.
See [behavior output](behavior-checks.txt). No spider behavior or artwork changed.

The existing native settings checks were updated for QR-only support. They
exercise the original QR image at 272 logical pixels, compact settings layout,
open/close behavior, closing the owner window, and verify that Close is the only
button inside the flyout. Obsolete URL-validation checks were removed with the
URL and browser-launch implementation. Settings persistence, web-visit toggles
and guest-silk rendering checks in the same native routine also pass.
See [native results](support-checks.txt) and the [flyout](support-flyout.png).
The resulting flyout and settings screenshots were visually inspected.

SupportQr.png remains byte-for-byte identical to RC13. It retains the same public
payment destination. There is no visible username label, recipient tooltip or
clickable payment link. No payment or phone-scanning test was performed here.
Earlier releases decoded the original QR and actual rendered flyout successfully.

Both ZIPs are checked entry by entry. The extracted source rebuilds with the
complete suite. The installed executable matches the extracted Windows package,
and the Desktop shortcut points to it. Package and launch evidence is recorded
beside the archives. Source bytes match the local Git repository.

The full native overlay, cursor and tray suite was not repeated for this small
UI change; see [RC10 verification](VERIFICATION-RC10.md). The retreat rendering
checks passed on RC13; see [RC13 verification](VERIFICATION-RC13.md). Physical
mixed-DPI changes, sleep/resume and a long resource soak remain manual checks.

Manual check: open Settings, click Buy me a coffee / Monster, scan the QR on your
phone and verify the recipient before deciding whether to send any payment.
Close the flyout with Close, Escape or an outside click.

The hosted packages remain private to the user's ChatGPT account. No public
GitHub release has been published.
