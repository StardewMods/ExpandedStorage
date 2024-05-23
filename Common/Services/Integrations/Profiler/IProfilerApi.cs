#if IS_FAUXCORE
namespace StardewMods.FauxCore.Common.Services.Integrations.Profiler;
#else
namespace StardewMods.Common.Services.Integrations.Profiler;
#endif

using System.Reflection;

// ReSharper disable All
#pragma warning disable

public interface IProfilerApi
{
    public IDisposable RecordSection(string ModId, string EventType, string Details);

    public MethodBase AddGenericDurationPatch(string type, string method, string detailsType = null!);
}