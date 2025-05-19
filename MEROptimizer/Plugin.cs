// #DivaDevs (ﾉ>ω<)ﾉ*✲ﾟ*｡✲ﾟ 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LabApi.Features;
using MEROptimizer.Application;

namespace MEROptimizer
{
  public class Plugin : LabApi.Loader.Features.Plugins.Plugin<Config>
  {
    public override string Name => "MEROptimizer";
    public override string Author { get; } = "Math";

    public override string Description { get; } = "Meant to optimize MapEditorReborn primitives by making them client sided + Providing an API to spawn & handle client side primitives.";
    public override Version Version { get; } = new Version(2, 0, 1, 0);

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
}
