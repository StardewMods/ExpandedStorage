#if IS_FAUXCORE

namespace StardewMods.FauxCore.Common.Enums;
#else

namespace StardewMods.Common.Enums;
#endif

using NetEscapades.EnumGenerators;

/// <summary>Specifies the type of patch to be applied.</summary>
[EnumExtensions]
public enum PatchType
{
    /// <summary>Prefix patch type.</summary>
    Prefix,

    /// <summary>Postfix patch type.</summary>
    Postfix,

    /// <summary>Transpiler patch type.</summary>
    Transpiler,

    /// <summary>Finalizer patch type.</summary>
    Finalizer,
}