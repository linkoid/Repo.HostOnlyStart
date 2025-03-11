using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;

namespace Linkoid.Repo.HostOnlyStart;

[BepInPlugin("Linkoid.Repo.HostOnlyStart", "Host Only Start", "1.0")]
public class HostOnlyStart : BaseUnityPlugin
{
    internal static HostOnlyStart Instance { get; private set; } = null!;
    internal new static ManualLogSource Logger => Instance._logger;
    private ManualLogSource _logger => base.Logger;
    internal Harmony? Harmony { get; set; }

    private void Awake()
    {
        Instance = this;

        // Prevent the plugin from being deleted
        this.gameObject.transform.parent = null;
        this.gameObject.hideFlags = HideFlags.HideAndDontSave;

        Patch();

        Logger.LogInfo($"{Info.Metadata.GUID} v{Info.Metadata.Version} has loaded!");
    }

    internal void Patch()
    {
        Harmony ??= new Harmony(Info.Metadata.GUID);
        Harmony.PatchAll();
    }

    internal void Unpatch()
    {
        Harmony?.UnpatchSelf();
    }
}