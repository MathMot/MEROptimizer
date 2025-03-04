using Exiled.API.Features;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MEROptimizer.MEROptimizer.Application;
using HarmonyLib;

namespace MEROptimizer.MEROptimizer
{
  public class Plugin : Plugin<Config>
  {
    public override string Name => "MEROptimizer";
    public override string Author => "Math";
    public override string Prefix => "mero";

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
