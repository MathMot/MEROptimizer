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

    [Description("If enabled, primitives that are in a distance higher as the one indicated below will unspawn for the corresponding player (Player Distance Spawning)")]
    public bool HideDistantPrimitivesToPlayers { get; set; } = true;

    [Description("In units, the distance required for the primitives to unspawn for each players(PDS)")]
    public float DistanceRequiredForUnspawningPrimitives { get; set; } = 50;

    [Description("If your primitive has any parent with a name corresponding to one of them, it will not be included in the PDS")]
    public List<string> excludeUnspawningDistantObjects { get; set; } = new List<string>();

    [Description("The maximum distance in unit for the distance required for a primitive to enter a cluster. The less it is the more clusters will spawn\n" +
      "(Clusters are points calculated in the schematic that contains all of the closest primitives around it)")]
    public int MaxDistanceForPrimitiveCluster { get; set; } = 5;

    [Description("Maximum amount of primitives per cluster the more there is the more clients will have to load primitives simultaneously when entering the radius of the cluster.(PDS)")]
    public int MaxPrimitivesPerCluster { get; set; } = 200;

    [Description("Should player's spectators be also affected by the PDS of their current spectated player")]
    public bool ShouldSpectatorsBeAffectByPDS { get; set; } = false;
  }
}
