namespace YandexMusic.Models.Landing;

/// <summary>The small payload returned by the feed wizard status endpoint.</summary>
public sealed class WizardStatus
{
    /// <summary>Whether the onboarding wizard has been completed.</summary>
    public bool IsWizardPassed { get; init; }
}
