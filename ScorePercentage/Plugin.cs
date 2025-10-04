using System.Linq;
using System.Reflection;
using HarmonyLib;
using IPA;
using IPA.Config;
using IPA.Loader;
using IPA.Logging;
using SiraUtil.Zenject;

namespace ScorePercentage;

[Plugin(RuntimeOptions.SingleStartInit)]
internal class Plugin
{
    private const string HarmonyId = "com.github.qe201020335.ScorePercentage";
    private const string MultiplayerInfoId = "MultiplayerInfo";
    private const string MappingExtensionsId = "MappingExtensions";
    private const string SongPlayHistoryId = "SongPlayHistory";

    internal static Logger Logger { get; private set; } = null!;

    private readonly Harmony _harmony;

    internal bool MultiplayerInfoInstalled { get; }
    internal bool MappingExtensionsInstalled { get; }
    internal PluginMetadata? SPHMetadata { get; }

    // Methods with [Init] are called when the plugin is first loaded by IPA.
    // All the parameters are provided by IPA and are optional.
    // The constructor is called before any method with [Init]. Only use [Init] with one constructor.
    [Init]
    public Plugin(Logger logger, Config config, Zenjector zenjector)
    {
        Logger = logger;
        _harmony = new Harmony(HarmonyId);
        PluginConfig.Initialize(config);

        MultiplayerInfoInstalled = PluginManager.EnabledPlugins.Any(metadata => metadata.Id == MultiplayerInfoId);
        MappingExtensionsInstalled = PluginManager.EnabledPlugins.Any(metadata => metadata.Id == MappingExtensionsId);
        SPHMetadata = PluginManager.GetPluginFromId(SongPlayHistoryId);

        zenjector.UseLogger(Logger);
        zenjector.UseAutoBinder();
        zenjector.Install(Location.App, container =>
        {
            container.BindInstance(this);
            container.BindInstance(PluginConfig.Instance);
        });

        Logger.Info("Plugin initialized");
    }

    [OnStart]
    public void OnStart()
    {
        _harmony.PatchAll(Assembly.GetExecutingAssembly());
    }

    [OnExit]
    public void OnExit()
    {
        _harmony.UnpatchSelf();
    }
}