using System;
using System.Collections.Generic;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Modding;

namespace Sts2ConceptMap;

/// <summary>
/// Entry point. Installs two Harmony patches:
///   1. <c>StandardActMap</c> constructor postfix — re-types every non-exempt map node to match the
///      act's rolled concept, producing a visible, themed map.
///   2. <c>NMapScreen.Open</c> postfix — shows a banner naming the current map's concept.
///
/// Concepts are gated by act (Act 1 easy → Act 3 hard). Pick "Surprise" (a concept per map) or force a
/// single concept in the in-game settings (ModConfig). Disable via env var STS2_CONCEPT_MAP_DISABLED=1.
/// Sister mod to Random Map / Blind Map — don't run Concept Map and Random Map together (both re-type
/// map nodes).
/// </summary>
[ModInitializer(nameof(Initialize))]
public partial class MainFile : Node
{
    public const string ModId = "Sts2ConceptMap";

    public static MegaCrit.Sts2.Core.Logging.Logger Logger { get; }
        = new(ModId, MegaCrit.Sts2.Core.Logging.LogType.Generic);

    public static void Initialize()
    {
        var harmony = new Harmony(ModId);
        var results = new List<string>();

        // Apply each patch independently so one failure can't abort the others (PatchAll aborts all on
        // any single failure). Records per-patch ok/FAIL into InitStatus for `conceptmap stats`.
        void TryPatch(string name, Type patchClass)
        {
            try
            {
                harmony.CreateClassProcessor(patchClass).Patch();
                results.Add(name + "=ok");
            }
            catch (Exception ex)
            {
                results.Add(name + "=FAIL(" + ex.Message + ")");
                Logger.Warn($"[{ModId}] patch '{name}' failed: {ex}");
            }
        }

        TryPatch("ctor", typeof(Patches.StandardActMap_Constructor_Patch));
        TryPatch("genmap", typeof(Patches.StandardActMap_GenerateMap_Patch));
        TryPatch("spoils", typeof(Patches.SpoilsActMap_Constructor_Patch));
        TryPatch("mapscreen", typeof(Patches.NMapScreenOpenPatch));

        ConceptMapService.InitStatus = string.Join(" | ", results);
        Logger.Info($"[{ModId}] patches: {ConceptMapService.InitStatus}");

        try
        {
            var envMode = System.Environment.GetEnvironmentVariable("STS2_CONCEPT_MAP_MODE");
            if (!string.IsNullOrEmpty(envMode)) ConceptMapService.SetModeByKey(envMode);
        }
        catch (Exception ex)
        {
            Logger.Warn($"[{ModId}] env mode failed: {ex.Message}");
        }

        Logger.Info($"[{ModId}] initialized (v0.1.1).");
    }
}
