using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;

namespace Mothenticate.IdentityProvider.Sso;

// Singleton; call NotifyChanged() after saving provider settings so IOptionsMonitor re-reads from DB.
public sealed class SsoOptionsChangeSource : IOptionsChangeTokenSource<OAuthOptions>, ISsoSettingsChangeNotifier
{
    private volatile CancellationTokenSource _cts = new();

    string IOptionsChangeTokenSource<OAuthOptions>.Name => Microsoft.Extensions.Options.Options.DefaultName;

    IChangeToken IOptionsChangeTokenSource<OAuthOptions>.GetChangeToken() => new CancellationChangeToken(_cts.Token);

    public void NotifyChanged()
    {
        var old = Interlocked.Exchange(ref _cts, new CancellationTokenSource());
        old.Cancel();
        old.Dispose();
    }
}
