#if IS_FAUXCORE
namespace StardewMods.FauxCore.Common.Services.Integrations.ContentPatcher;
#else
namespace StardewMods.Common.Services.Integrations.ContentPatcher;
#endif

/// <summary>Raised when the Content Patcher Conditions Api is ready.</summary>
internal sealed class ConditionsApiReadyEventArgs : EventArgs { }