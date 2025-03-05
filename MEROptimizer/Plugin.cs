using Exiled.API.Features;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MEROptimizer.MEROptimizer.Application;
using HarmonyLib;
using Exiled.API.Enums;

namespace MEROptimizer.MEROptimizer
{
  public class Plugin : Plugin<Config>
  {
    public override string Name => "MEROptimizer";
    public override string Author => "Math";
    public override string Prefix => "mero";
    public override PluginPriority Priority { get; } = PluginPriority.Low;

    private static int patchCount = 1;

    public static Application.MEROptimizer merOptimizer;
    private Harmony harmony;
    public override void OnEnabled()
    {
      merOptimizer = new Application.MEROptimizer();
      merOptimizer.Load(Config);

      // Harmony
      harmony = new Harmony($"{Author.ToLower()}.{Name.ToLower()}.{patchCount++}");
      harmony.PatchAll();

      Log.Info($"{Name} v{Version.Major}.{Version.Minor}.{Version.Build} by {Author} has been enabled!");
    }

    public override void OnDisabled()
    {
      merOptimizer?.Unload();
      merOptimizer = null;
      harmony?.UnpatchAll();
      harmony = null;
    }
  }
}
