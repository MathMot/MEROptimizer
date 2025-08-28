// #DivaDevs (ﾉ>ω<)ﾉ*✲ﾟ*｡✲ﾟ 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#if EXILED
using Exiled.API.Enums;
#endif
using LabApi.Features;
using MEROptimizer.Application;

namespace MEROptimizer
{
#if EXILED
  public class Plugin : Exiled.API.Features.Plugin<Config>
  {
    public override string Name => "MEROptimizer";
    public override string Author => "Math";
    public override string Prefix => "mero";
    public override PluginPriority Priority { get; } = PluginPriority.Low;

    public static Application.MEROptimizer merOptimizer;

    public override void OnEnabled()
    {
      merOptimizer = new Application.MEROptimizer();
      merOptimizer.Load(Config);

      base.OnEnabled();
    }

    public override void OnDisabled()
    {
      merOptimizer?.Unload();
      merOptimizer = null;

      base.OnDisabled();
    }
  }

#else
  public class Plugin : LabApi.Loader.Features.Plugins.Plugin<Config>
  {
    public override string Name => "MEROptimizer";
    public override string Author { get; } = "Math";

    public override string Description { get; } = "Meant to optimize MapEditorReborn primitives by making them client sided + Providing an API to spawn & handle client side primitives.";
    public override Version Version { get; } = new Version(2, 0, 7, 0);

    public override Version RequiredApiVersion { get; } = new Version(LabApiProperties.CompiledVersion);

    public static Application.MEROptimizer merOptimizer;
    public override void Enable()
    {
      merOptimizer = new Application.MEROptimizer();
      merOptimizer.Load(Config);

    }

    public override void Disable()
    {
      merOptimizer?.Unload();
      merOptimizer = null;
    }
  }

#endif
}

