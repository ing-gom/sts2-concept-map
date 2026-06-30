using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.DevConsole;
using MegaCrit.Sts2.Core.DevConsole.ConsoleCommands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Runs;

namespace Sts2ConceptMap;

/// <summary>
/// Dev-console command <c>conceptmap</c> for testing every map concept in-game without re-rolling runs.
/// The game auto-discovers AbstractConsoleCmd subtypes in mod assemblies
/// (DevConsole ctor → ReflectionHelper.GetSubtypesInMods&lt;AbstractConsoleCmd&gt;()), so just defining
/// this class registers it. Console debug commands are enabled whenever the game runs modded.
///
/// Usage:
///   conceptmap                 list every concept (key + tier)
///   conceptmap list            same as above
///   conceptmap &lt;key&gt;      force that concept and regenerate the current act's map
///   conceptmap next            cycle to the next concept and regenerate (rapid "test all")
///   conceptmap surprise        back to auto (a concept per map)
/// </summary>
public sealed class ConceptConsoleCmd : AbstractConsoleCmd
{
    public override string CmdName => "conceptmap";
    public override string Args => "[list | next | stats | surprise | <concept-key>]";
    public override string Description =>
        "Concept Maps: force a map concept and regenerate the current act to preview it. " +
        "'list' shows all concepts, 'next' cycles through them, 'surprise' restores auto mode.";
    public override bool IsNetworked => false;

    public override CmdResult Process(Player? issuingPlayer, string[] args)
    {
        if (args.Length == 0 || string.Equals(args[0], "list", StringComparison.OrdinalIgnoreCase))
            return new CmdResult(success: true, BuildList());

        if (string.Equals(args[0], "stats", StringComparison.OrdinalIgnoreCase))
            return new CmdResult(success: true,
                "Patches: " + ConceptMapService.InitStatus + "\nLast map: " + ConceptMapService.LastStats);

        string arg = args[0].ToLowerInvariant();
        string targetKey;

        if (arg == "next")
        {
            targetKey = ConceptMapService.CycleNext();
        }
        else if (arg == ConceptMapService.SurpriseKey)
        {
            targetKey = ConceptMapService.SurpriseKey;
        }
        else
        {
            var c = ConceptMapService.Concepts.FirstOrDefault(
                x => string.Equals(x.Key, arg, StringComparison.OrdinalIgnoreCase));
            if (c == null)
                return new CmdResult(success: false, $"Unknown concept '{arg}'. Try: conceptmap list");
            targetKey = c.Key;
        }

        ConceptMapService.SetModeByKey(targetKey);

        var rm = RunManager.Instance;
        if (rm?.State == null || issuingPlayer?.RunState == null)
            return new CmdResult(success: true,
                $"Concept set to '{targetKey}'. Start a run — maps regenerate on the next act.");

        // Re-enter the current act so the map regenerates with the forced concept (same path 'act' uses).
        int act = rm.State.CurrentActIndex;
        Task task = rm.EnterAct(act);
        return new CmdResult(task, success: true, $"Map concept -> '{targetKey}'. Regenerating act {act + 1}...");
    }

    public override CompletionResult GetArgumentCompletions(Player? player, string[] args)
    {
        if (args.Length <= 1)
        {
            var opts = new List<string> { "list", "next", "stats", ConceptMapService.SurpriseKey };
            opts.AddRange(ConceptMapService.Concepts.Select(c => c.Key));
            string partial = args.Length == 1 ? args[0] : string.Empty;
            return CompleteArgument(opts, Array.Empty<string>(), partial);
        }
        return base.GetArgumentCompletions(player, args);
    }

    private static string BuildList()
    {
        var sb = new System.Text.StringBuilder();
        sb.Append("Concept Maps — '").Append(ConceptMapService.CurrentModeKey).Append("' active.\n");
        sb.Append(ConceptMapService.SurpriseKey).Append("  (auto: a concept per map, Act 1 easy -> Act 3 hard)\n");
        foreach (var tier in new[] { 1, 2, 3 })
        {
            sb.Append("-- Tier ").Append(tier).Append(" --\n");
            foreach (var c in ConceptMapService.Concepts.Where(c => c.Tier == tier))
                sb.Append("  ").Append(c.Key.PadRight(11)).Append(ConceptMapService.LabelOf(c)).Append('\n');
        }
        sb.Append("Use: conceptmap <key> | conceptmap next | conceptmap surprise");
        return sb.ToString();
    }
}
