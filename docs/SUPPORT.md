# Optional Cash App support

The creator supplied the original QR card and requested its recognizable Cash App
appearance. SupportQr.png is a lossless PNG crop of that supplied JPEG's decoded
pixels. The surrounding phone screenshot is not distributed. The QR decodes to:

`https://cash.app/$tedparkes?qr=1`

The settings link uses that same recipient. The QR is bundled and embedded, with
no remote image request. Clicking the code displays a larger copy. Clicking the
recipient opens the default browser; nothing opens automatically. There is no
payment SDK, account connection, login form, stored financial data or automatic
payment. Donations are optional and do not unlock features.

[Cash App's official instructions](https://cash.app/learn/getting-started/how-to-send-and-receive-money)
describe sharing a cashtag or payment link to receive money. This exposes the
public payment profile and may reveal the profile name; it does not itself grant
account access. Never put a login code or PIN in the app or repository.
[Cash App security guidance](https://cash.app/security) covers account protections.
Recipients and senders should verify the profile before transferring money.
No transfer or account-login test was performed for this release.

Cash App branding belongs to its respective owner and is used here to identify
the optional payment destination. The MIT license covers the project's code and
original procedural spider art; it does not grant rights to third-party trademarks
or imply endorsement. A fork should deliberately review the recipient. If changing
it, replace the QR, PaymentUrl, CashTag and corresponding validation/tests together.

Validation: the supplied original and crop decode identically. Local checks decoded
the crop at 112/128/144/168/320/420/545 pixels, and decoded the actual normal and
compact WPF settings captures to the same address. Phone scanning and a real
donation remain manual checks. Diagnostic modes never open payment pages.
