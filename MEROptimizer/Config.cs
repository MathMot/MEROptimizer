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

    [Description("\n-------------Global Options-------------\n" +
      "If the primitives that will be optimized are only non collidable")]
    public bool OptimizeOnlyNonCollidable { get; set; } = false;

    [Description("Prvents group of primitives to be optimized (aka keeped server sided)\n" +
      "Simply name one of its empty parents with one of the entered name here and it will be excluded.")]
    public List<string> excludeObjects { get; set; } = new List<string>();

    [Description("\n-------------Schematic cluster splitting options-------------\n" +
      "Could be quite hard to understand, more info in the plugin readme\n" +
      "If enabled, splits schematics into clusters of primitives to then spawn them independently per players based on their distance to the cluster")]
    public bool ClusterizeSchematic { get; set; } = true;

    [Description("Prevents group of primitives to be used by the clusters. Useful for skyboxs, outer walls of buildings and giant primitives that requires to be seen from far away" +
      "Simply name one of its empty parents with one of the entered names here and it will be excluded.")]
    public List<string> excludeUnspawningDistantObjects { get; set; } = new List<string>();

    [Description("In units, the distance required for a cluster to spawn/unspawn its primitives to the corresponding player")]
    public float SpawnDistance { get; set; } = 50;

    [Description("Should spectating players be also affected by the cluster system" +
    "If enabled, when a player spectates another, it will spawn all of the primitives that the spectated player currently sees, otherwise spectators will see all of the schematics at all time")]
    public bool ShouldSpectatorBeAffectedByDistanceSpawning { get; set; } = false;

    [Description("For each cluster, number of primitives that'll spawn per server frame (higher count means quicker spawn but potential freezes for clients)" +
      "If set to zero (0), each cluster will spawn its primitives instantly")]
    public int numberOfPrimitivePerSpawn { get; set; } = 1;

    [Description("\n-----Clusters Options-----\n" +
      "In units, the maximum distance between a primitive and a specific cluster to be included in it, the more distance the less cluster will spawn")]
    public int MaxDistanceForPrimitiveCluster { get; set; } = 5;

    [Description("Maximum amount of primitive per cluster, if reached, a new cluster will spawn and be used. The less primitives per cluster the more clusters will spawn")]
    public int MaxPrimitivesPerCluster { get; set; } = 200;


  }
}
