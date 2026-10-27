# Optional Cash App support

The small "Buy me a coffee / Monster" button opens a flyout containing the
creator's original Cash App QR at 272 pixels, a scanning instruction and Close.
There is no username label, clickable payment link, recipient tooltip or browser
launch. Close, Escape, an outside click or closing settings dismisses the flyout.
The QR is bundled locally; opening the flyout makes no network request.

SupportQr.png is a lossless PNG crop of the supplied JPEG's decoded pixels.
The surrounding phone screenshot is not distributed. Its original payment
recipient is unchanged. Scanning still reveals the same public Cash App profile;
removing the text label does not make the payment destination anonymous.

There is no payment SDK, account connection, login form, stored financial data
or automatic payment. Support is optional and does not unlock features.
Check the recipient in Cash App before sending a payment.

Cash App branding belongs to its respective owner and is used to identify the
optional payment destination. The MIT license covers the project's code and
original procedural spider art; it does not grant rights to third-party trademarks
or imply endorsement. A fork should deliberately review its payment recipient
and verify any replacement QR. Never include login codes or account credentials.

Validation: the QR asset remains byte-for-byte identical to RC13. Earlier checks
decoded the supplied original, crop and rendered WPF flyout to the same payment
profile. RC14 checks the QR-only controls, original image, compact layout and
open/close behavior. No payment or account-login test was performed.
