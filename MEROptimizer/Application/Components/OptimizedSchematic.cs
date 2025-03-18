using Exiled.API.Features;
using Exiled.API.Features.Toys;
using MapEditorReborn.API.Features.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static PlayerList;

namespace MEROptimizer.MEROptimizer.Application.Components
{
  public class OptimizedSchematic
  {
    public SchematicObject schematic { get; set; }

    private string schematicName;

    public List<Collider> colliders { get; set; }

    public List<ClientSidePrimitive> nonClusteredPrimitives { get; set; }

    public List<PrimitiveCluster> primitiveClusters { get; set; }

    public DateTime spawnTime { get; set; }

    public int schematicsTotalPrimitives { get; set; }

    public int serverSpawnedPrimitives => schematicsTotalPrimitives - nonClusteredPrimitives.Count;

    public OptimizedSchematic(SchematicObject schematic, List<Collider> colliders, Dictionary<ClientSidePrimitive, bool> primitives,
      bool doClusters = false, float distance = 50, List<string> excludedUnspawnObjects = null, int maxDistanceForPrimitiveCluster = 5,
      int maxPrimitivesPerCluster = 200)
    {
      this.schematic = schematic;
      this.colliders = colliders;
      spawnTime = DateTime.Now;

      schematicName = schematic.name;

      nonClusteredPrimitives = new List<ClientSidePrimitive>();
      primitiveClusters = new List<PrimitiveCluster>();

      GenerateClustersAndSpawn(doClusters, primitives, distance, excludedUnspawnObjects, maxDistanceForPrimitiveCluster, maxPrimitivesPerCluster);

    }

    private void GenerateClustersAndSpawn(bool doClusters, Dictionary<ClientSidePrimitive, bool> primitives,
      float distance, List<string> excludedUnspawnObjects, int maxDistanceForPrimitiveCluster, int maxPrimitivesPerCluster)
    {
      if (!doClusters)
      {
        foreach (ClientSidePrimitive primitive in primitives.Keys)
        {
          nonClusteredPrimitives.Add(primitive);
        }
      }
      else
      {

        // Remove non clustered primitives
        foreach (ClientSidePrimitive primitive in primitives.Keys.ToList())
        {
          if (!primitives[primitive])
          {
            nonClusteredPrimitives.Add(primitive);
            primitives.Remove(primitive);
          }
        }

        if (!primitives.IsEmpty())
        {

          // Calculate the center of the schematic, where the first cluster will spawn
          Vector3 center3D = Vector3.zero;
          foreach (ClientSidePrimitive p in primitives.Keys)
          {
            center3D += p.position;
          }

          center3D /= primitives.Count;

          // Sort the primitives by their distance with the center
          List<ClientSidePrimitive> sortedPrimitives = primitives.Keys.ToList();
          sortedPrimitives = sortedPrimitives.OrderBy(s => Vector3.Distance(s.position, center3D)).ToList();

          Dictionary<int, List<ClientSidePrimitive>> clusters = new Dictionary<int, List<ClientSidePrimitive>>();

          int clusterNumber = 1;

          // Creates clusters, add the primitives to the clusters until all clusters are generated
          while (sortedPrimitives.Count > 0)
          {

            ClientSidePrimitive closestFromCenterPrimitive = sortedPrimitives.First();

            List<ClientSidePrimitive> clusterPrimitives = new List<ClientSidePrimitive>() { closestFromCenterPrimitive };

            List<ClientSidePrimitive> sortedPrimitiveByCluster = sortedPrimitives.ToList();

            Vector3 centerPos = closestFromCenterPrimitive.position;

            // Keep all of the primitives where their distance correspond
            sortedPrimitiveByCluster.RemoveAll(p =>
            Vector3.Distance(p.position, centerPos) > maxDistanceForPrimitiveCluster);

            // Remove excess primitives based on config
            if (sortedPrimitiveByCluster.Count > maxPrimitivesPerCluster)
            {
              sortedPrimitiveByCluster = sortedPrimitiveByCluster.OrderBy(s => Vector3.Distance(s.position, centerPos)).ToList();
              sortedPrimitiveByCluster.RemoveRange(maxPrimitivesPerCluster, sortedPrimitiveByCluster.Count - maxPrimitivesPerCluster);
            }


            clusterPrimitives.AddRange(sortedPrimitiveByCluster);

            sortedPrimitives.RemoveAll(p => clusterPrimitives.Contains(p));

            // sort the primitives on their y value, so that the first to spawn will be the bottom ones

            clusterPrimitives = clusterPrimitives.OrderBy(p => p.position.y).ToList();

            clusters.Add(clusterNumber++, clusterPrimitives);
          }

          //Creates the Gameobjects for the clusters
          foreach (KeyValuePair<int, List<ClientSidePrimitive>> cluster in clusters)
          {


            // Get the center of the cluster

            Vector3 center = Vector3.zero;
            foreach (ClientSidePrimitive primitive in cluster.Value)
            {
              center += primitive.position;
            }

            center /= cluster.Value.Count;

            // Creates the GameObject

            GameObject gameObject = new GameObject($"[MERO] PrimitiveCluster_{schematic.name}_{cluster.Key}");

            gameObject.transform.position = center + new Vector3(0, 2000, 0);
            gameObject.transform.rotation = Quaternion.identity;
            gameObject.transform.localScale = Vector3.one;

            SphereCollider collider = gameObject.AddComponent<SphereCollider>();
            collider.radius = distance;
            collider.isTrigger = true;

            PrimitiveCluster primitiveCluster = gameObject.AddComponent<PrimitiveCluster>();
            primitiveCluster.id = cluster.Key;
            primitiveCluster.primitives = cluster.Value;

            primitiveClusters.Add(primitiveCluster);
          }
        }
      }

      foreach (ClientSidePrimitive primitive in nonClusteredPrimitives)
      {
        primitive.SpawnForEveryone();
      }
    }

    public void RefreshFor(Player player)
    {
      HideFor(player, false);

      foreach (ClientSidePrimitive primitive in nonClusteredPrimitives)
      {
        primitive.SpawnClientPrimitive(player);
      }
      Log.Debug($"Refresh the schematic {this.schematicName} for {player.DisplayNickname} !");
    }

    public void HideFor(Player player, bool showDebug = true)
    {
      if (player == null) return;
      if (showDebug)
      {
        Log.Debug($"Hiding client side primitives of {this.schematicName} to {player.DisplayNickname}");
      }

      foreach (ClientSidePrimitive primitive in nonClusteredPrimitives)
      {
        primitive.DestroyClientPrimitive(player);
      }
    }



    public void SpawnClientPrimitivesToAll()
    {
      Log.Debug($"Displaying {schematicName}'s client side primitives !");
      foreach (Player player in Player.List.Where(p => p != null && p.IsVerified))
      {
        SpawnClientPrimitives(player);
      }
    }

    public void SpawnClientPrimitives(Player player)
    {
      if (player == null) return;

      Log.Debug($"Displaying client side primitives of {this.schematicName} to {player.DisplayNickname}");
      foreach (ClientSidePrimitive primitive in nonClusteredPrimitives)
      {
        primitive.SpawnClientPrimitive(player);
      }
    }

    public void Destroy()
    {
      foreach (Collider collider in colliders.Where(c => c != null && c.gameObject != null))
      {
        UnityEngine.Object.Destroy(collider);
      }

      foreach (ClientSidePrimitive primitive in nonClusteredPrimitives)
      {
        primitive.DestroyForEveryone();
      }

      foreach (PrimitiveCluster cluster in primitiveClusters)
      {
        UnityEngine.Object.Destroy(cluster.gameObject);
      }

      Log.Debug($"Destroyed client side schematic {schematicName} !");
    }
  }
}
