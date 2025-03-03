using Exiled.API.Features;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MEROptimizer.MEROptimizer.Application;

namespace MEROptimizer.MEROptimizer
{
  public class Plugin : Plugin<Config>
  {
    public override string Name => "MEROptimizer";
    public override string Author => "Math";
    public override string Prefix => "mero";

    public static Application.MEROptimizer merOptimizer;
    public override void OnEnabled()
    {
      merOptimizer = new Application.MEROptimizer();
      merOptimizer.Load(Config);
    }

    public override void OnDisabled()
    {
      merOptimizer?.Unload();
      merOptimizer = null;
    }
  }
}
