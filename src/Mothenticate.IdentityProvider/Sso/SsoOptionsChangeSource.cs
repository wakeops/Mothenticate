using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;

namespace Mothenticate.IdentityProvider.Sso;

// Singleton; call Trigger() after saving SSO settings so IOptionsMonitor re-reads from DB.
public sealed class SsoOptionsChangeSource :
    IOptionsChangeTokenSource<GoogleOptions>,
    IOptionsChangeTokenSource<OAuthOptions>
{
    private volatile CancellationTokenSource _cts = new();

    string IOptionsChangeTokenSource<GoogleOptions>.Name => Microsoft.Extensions.Options.Options.DefaultName;
    string IOptionsChangeTokenSource<OAuthOptions>.Name => SsoDefaults.GitHubScheme;

    IChangeToken IOptionsChangeTokenSource<GoogleOptions>.GetChangeToken() => BuildToken();
    IChangeToken IOptionsChangeTokenSource<OAuthOptions>.GetChangeToken() => BuildToken();

    private IChangeToken BuildToken() => new CancellationChangeToken(_cts.Token);

    public void Trigger()
    {
        var old = Interlocked.Exchange(ref _cts, new CancellationTokenSource());
        old.Cancel();
        old.Dispose();
    }
}
