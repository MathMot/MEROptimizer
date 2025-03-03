using Exiled.API.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MEROptimizer
{

  public sealed class Config : IConfig
  {
    [Description("If the plugin is enabled or not.")]
    public bool IsEnabled { get; set; } = true;

    [Description("Displays plugin debug logs.")]
    public bool Debug { get; set; }

    [Description("If the primitives that will be optimized are only non collidable")]
    public bool OptimizeOnlyNonCollidable { get; set; } = false;

    [Description("If your primitive has any parent with a name corresponding to one of them, it will not be optimized.")]
    public List<string> excludeObjects { get; set; } = new List<string>();

  }
}
