namespace Mothenticate.Data.Services;

public sealed class SetupState
{
    private volatile bool _configured;

    public bool IsConfigured => _configured;

    public void MarkConfigured() => _configured = true;
}
