using AdminToys;
using Logger = LabApi.Features.Console.Logger;
using PrimitiveObjectToy = AdminToys.PrimitiveObjectToy;
using LabApi.Events.Arguments.PlayerEvents;
using MEC;
using MEROptimizer.Application.Components;
using Mirror;
using PlayerRoles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.Assertions.Must;
using LabApi.Features.Wrappers;
using ProjectMER.Events.Arguments;
using ProjectMER.Features.Objects;
using ProjectMER.Events.Handlers;

namespace MEROptimizer.Application
{
  public class MEROptimizer
  {

    private bool hasGenerated = false;

    private Mesh cubeMesh;

    private Mesh sphereMesh;

    private Mesh capsuleMesh;

    private Mesh cylinderMesh;

    public static uint PrimitiveAssetId;

    /*
    private Mesh quadMesh;
   
    private Mesh planeMesh;
    */

    private bool excludeCollidables;

    private List<string> excludedNames;

    private bool hideDistantPrimitives;

    public static bool shouldSpectatorsBeAffectedByPDS;

    public static bool ShouldTutorialsBeAffectedByDistanceSpawning;

    private float distanceRequiredForUnspawning;

    private Dictionary<string, float> CustomSchematicSpawnDistance = new Dictionary<string, float>();

    private float maxDistanceForPrimitiveCluster;

    private int maxPrimitivesPerCluster;

    private List<string> excludedNamesForUnspawningDistantObjects;

    public static float numberOfPrimitivePerSpawn;

    public static float MinimumSizeBeforeBeingBigPrimitive;

    public static bool isDynamiclyDisabled = false;

    public static bool IsDebug = false;

    public List<OptimizedSchematic> optimizedSchematics = new List<OptimizedSchematic>();
    public void Load(Config config)
    {
      //Config
      MEROptimizer.IsDebug = config.Debug;
      excludeCollidables = config.OptimizeOnlyNonCollidable;
      excludedNames = config.excludeObjects;

      hideDistantPrimitives = config.ClusterizeSchematic;
      distanceRequiredForUnspawning = config.SpawnDistance;
      excludedNamesForUnspawningDistantObjects = config.excludeUnspawningDistantObjects;
      maxDistanceForPrimitiveCluster = config.MaxDistanceForPrimitiveCluster;
      maxPrimitivesPerCluster = config.MaxPrimitivesPerCluster;
      shouldSpectatorsBeAffectedByPDS = config.ShouldSpectatorBeAffectedByDistanceSpawning;
      numberOfPrimitivePerSpawn = config.numberOfPrimitivePerSpawn;
      MinimumSizeBeforeBeingBigPrimitive = config.MinimumSizeBeforeBeingBigPrimitive;
      ShouldTutorialsBeAffectedByDistanceSpawning = config.ShouldTutorialsBeAffectedByDistanceSpawning;
      CustomSchematicSpawnDistance = config.CustomSchematicSpawnDistance;

      // LabAPI Events
      LabApi.Events.Handlers.PlayerEvents.Joined += OnJoined;
      LabApi.Events.Handlers.PlayerEvents.Spawned += OnSpawned;
      LabApi.Events.Handlers.PlayerEvents.ChangedSpectator += OnChangedSpectator;
      LabApi.Events.Handlers.ServerEvents.WaitingForPlayers += OnWaitingForPlayers;


      // MER Events
      ProjectMER.Events.Handlers.Schematic.SchematicSpawned += OnSchematicSpawned;
      ProjectMER.Events.Handlers.Schematic.SchematicDestroyed += OnSchematicDestroyed;

    }

    public void Unload()
    {
      // LabAPI Events
      LabApi.Events.Handlers.PlayerEvents.Joined -= OnJoined;
      LabApi.Events.Handlers.PlayerEvents.Spawned -= OnSpawned;
      LabApi.Events.Handlers.PlayerEvents.ChangedSpectator -= OnChangedSpectator;
      LabApi.Events.Handlers.ServerEvents.WaitingForPlayers -= OnWaitingForPlayers;

      // MER Events

      ProjectMER.Events.Handlers.Schematic.SchematicSpawned -= OnSchematicSpawned;
      ProjectMER.Events.Handlers.Schematic.SchematicDestroyed -= OnSchematicDestroyed;

      Clear();
    }


    // ---------------------- Private methods

    public static void Debug(string message)
    {
      if (!MEROptimizer.IsDebug) return;
      Logger.Debug(message);
    }

    private void Clear()
    {
      optimizedSchematics.Clear();
    }

    private GameObject tempPrimitivePrefab;
    private void GenerateMeshFilters()
    {
      if (tempPrimitivePrefab == null)
      {
        if (!NetworkClient.prefabs.ContainsKey(1321952889u))
        {
          Logger.Error("Can't generate proper colliders for primitives, primitive gameobject is not found.");
          return;
        }

        tempPrimitivePrefab = NetworkClient.prefabs.Where(k => k.Key == 1321952889u).FirstOrDefault().Value;
        PrimitiveAssetId = tempPrimitivePrefab.GetComponent<NetworkIdentity>().assetId;
      }

      GameObject toy = UnityEngine.GameObject.Instantiate(tempPrimitivePrefab);
      PrimitiveObjectToy primitive = toy.GetComponent<PrimitiveObjectToy>();

      primitive.NetworkPosition = new Vector3(0f, 0f, 0f);
      primitive.NetworkScale = Vector3.one;

      primitive.NetworkPrimitiveType = PrimitiveType.Cube;
      this.cubeMesh = primitive.GetComponent<MeshFilter>().mesh;

      primitive.NetworkPrimitiveType = PrimitiveType.Sphere;
      this.sphereMesh = primitive.GetComponent<MeshFilter>().mesh;

      primitive.NetworkPrimitiveType = PrimitiveType.Capsule;
      this.capsuleMesh = primitive.GetComponent<MeshFilter>().mesh;

      primitive.NetworkPrimitiveType = PrimitiveType.Cylinder;
      this.cylinderMesh = primitive.GetComponent<MeshFilter>().mesh;

      /*
      primitive.NetworkPrimitiveType = PrimitiveType.Quad;
      this.quadMesh = primitive.GetComponent<MeshFilter>().mesh;

      primitive.NetworkPrimitiveType = PrimitiveType.Plane;
      this.planeMesh = primitive.GetComponent<MeshFilter>().mesh;
      */

      UnityEngine.Object.Destroy(primitive.gameObject);

      hasGenerated = true;
    }

    Dictionary<PrimitiveObjectToy, bool> GetPrimitivesToOptimize(Transform parent, List<Transform> parentToExclude,
      Dictionary<PrimitiveObjectToy, bool> primitives = null, bool clusterChilds = true)
    {

      if (primitives == null) primitives = new Dictionary<PrimitiveObjectToy, bool>();

      for (int i = 0; i < parent.childCount; i++)
      {
        Transform child = parent.GetChild(i);
        if (child == null || parentToExclude.Contains(child)) continue;

        if (clusterChilds)
        {
          foreach (string name in excludedNamesForUnspawningDistantObjects)
          {
            if (child.name.Contains(name))
            {
              clusterChilds = false;
              //break;
            }
          }
        }

        if (child.TryGetComponent(out PrimitiveObjectToy primitive))
        {

          if (excludedNames.Any(n => primitive.name.ToLower().Contains(n.ToLower())))
          {
            continue;
          }

          // Keep the quads/planes, colliders are buggy, + removing primitives working as empty for MER
          if ((primitive.PrimitiveType == PrimitiveType.Quad || primitive.PrimitiveType == PrimitiveType.Plane)
            && primitive.PrimitiveFlags.HasFlag(PrimitiveFlags.Collidable))
          {
            continue;
          }

          if (this.excludeCollidables)
          {
            if (primitive.PrimitiveFlags.HasFlag(PrimitiveFlags.Collidable)) continue;
          }

          if (primitive.PrimitiveFlags != PrimitiveFlags.None)
          {
            primitives.Add(primitive, clusterChilds);
          }

          //continue;
        }

        if (!parentToExclude.Contains(child))
        {
          if (!excludedNames.Any(n => child.name.ToLower().Contains(n.ToLower())))
          {
            GetPrimitivesToOptimize(child, parentToExclude, primitives, clusterChilds: clusterChilds);
          }
        }
      }

      //return GetPrimitivesToOptimize(null, parentToExclude, primitives);
      return primitives;
    }

    // --------------- Events

    private void OnWaitingForPlayers()
    {
      if (!hasGenerated)
      {
        GenerateMeshFilters();
      }

      Clear();
    }

    //--------------- Events EXILED

    private void AddPlayerTrigger(Player player)
    {
      MEROptimizer.Debug($"Adding PlayerTrigger to {player.DisplayName}({player.PlayerId}) !");
      GameObject playerTrigger = new GameObject($"{player.PlayerId}_MERO_TRIGGER");
      playerTrigger.tag = "Player";

      Rigidbody rb = playerTrigger.AddComponent<Rigidbody>();
      rb.isKinematic = true;

      playerTrigger.AddComponent<BoxCollider>().size = new Vector3(1, 2, 1); // epic representation of a player's hitbox

      playerTrigger.AddComponent<PlayerTrigger>().player = player;

    }
    private void OnJoined(PlayerJoinedEventArgs ev)
    {
      if (ev.Player == null || ev.Player.IsNpc) return;

      AddPlayerTrigger(ev.Player);
      foreach (OptimizedSchematic schematic in optimizedSchematics.Where(s => s != null && s.schematic != null))
      {
        MEROptimizer.Debug($"Displaying static client sided primitives of {schematic.schematic.Name} to {ev.Player.DisplayName} because he just connected !");
        schematic.SpawnClientPrimitives(ev.Player);
      }
    }

    // one of the worst code i've ever written, i'm sorry about that
    private void OnSpawned(PlayerSpawnedEventArgs ev)
    {
      if (ev.Player == null) return;

      if (ev.Player.IsNpc)
      {
        bool hasFound = false;

        for (int i = 0; i < ev.Player.GameObject.transform.childCount; i++)
        {
          Transform child = ev.Player.GameObject.transform.GetChild(i);
          if (child != null && child.name == $"{ev.Player.PlayerId}_MERO_TRIGGER")
          {
            hasFound = true;
            break;
          }
        }

        if (!hasFound)
        {
          AddPlayerTrigger(ev.Player);
        }
      }
      else
      {

        // just spawned as a spectator, we spawn all clusters primitives for him
        if ((ev.Player.Role == RoleTypeId.Spectator || ev.Player.Role == RoleTypeId.Overwatch) && !shouldSpectatorsBeAffectedByPDS)
        {
          // Unspawning and then respawning primitives at the same frame causes the game to shit itself, so a delay is needed
          Timing.CallDelayed(.5f, () =>
          {
            if (ev.Player != null && (ev.Player.Role == RoleTypeId.Spectator || ev.Player.Role == RoleTypeId.Overwatch))
            {
              foreach (OptimizedSchematic schematic in optimizedSchematics.Where(s => s != null && s.schematic != null))
              {
                MEROptimizer.Debug($"Spawning all clusters (as a fade spawn) of {schematic.schematic.Name} to {ev.Player.DisplayName} because he spawned as a spectator (ssbadbs : {shouldSpectatorsBeAffectedByPDS})");

                foreach (PrimitiveCluster cluster in schematic.primitiveClusters)
                {
                  if (cluster.instantSpawn)
                  {
                    cluster.SpawnFor(ev.Player);
                  }
                  else
                  {
                    cluster.awaitingSpawn.Remove(ev.Player);
                    cluster.awaitingSpawn.Add(ev.Player, cluster.primitives.ToList());
                    cluster.spawning = true;
                  }
                }
              }
            }
          });

        }
        if (!ShouldTutorialsBeAffectedByDistanceSpawning && ev.Player.Role == RoleTypeId.Tutorial)
        {
          Timing.CallDelayed(.5f, () =>
          {

            if (ev.Player != null && ev.Player.Role == RoleTypeId.Tutorial)
            {
              foreach (OptimizedSchematic schematic in optimizedSchematics.Where(s => s != null && s.schematic != null))
              {
                MEROptimizer.Debug($"Spawning all clusters (as a fade spawn) of {schematic.schematic.Name} to {ev.Player.DisplayName} because he spawned as a tutorial and based on the specified config he should see all of the map (ssbadbs : {shouldSpectatorsBeAffectedByPDS})");

                foreach (PrimitiveCluster cluster in schematic.primitiveClusters)
                {
                  if (cluster.instantSpawn)
                  {
                    cluster.SpawnFor(ev.Player);
                  }
                  else
                  {
                    cluster.awaitingSpawn.Remove(ev.Player);
                    cluster.awaitingSpawn.Add(ev.Player, cluster.primitives.ToList());
                    cluster.spawning = true;
                  }

                }
              }
            }

          });
        }
        else
        {
          foreach (OptimizedSchematic schematic in optimizedSchematics)
          {
            MEROptimizer.Debug($"Unspawning all clusters of {schematic.schematic.Name} to {ev.Player.DisplayName} because he just changed role (ssbadbs : {shouldSpectatorsBeAffectedByPDS})");
            foreach (PrimitiveCluster cluster in schematic.primitiveClusters)
            {
              if (!cluster.insidePlayers.Contains(ev.Player))
              {
                cluster.UnspawnFor(ev.Player);
              }

            }
          }

          if (ev.Player.Role == RoleTypeId.Filmmaker || ev.Player.Role == RoleTypeId.Scp079)
          {
            Timing.CallDelayed(.5f, () =>
            {
              if (ev.Player != null && (ev.Player.Role == RoleTypeId.Filmmaker || ev.Player.Role == RoleTypeId.Scp079))
              {
                foreach (OptimizedSchematic schematic in optimizedSchematics.Where(s => s != null && s.schematic != null))
                {
                  MEROptimizer.Debug($"Spawning all clusters (as a fade spawn) of {schematic.schematic.Name} to {ev.Player.DisplayName} because he spawned as a filmaker ( why ) and based on the specified config he should see all of the map (ssbadbs : {shouldSpectatorsBeAffectedByPDS})");

                  foreach (PrimitiveCluster cluster in schematic.primitiveClusters)
                  {
                    if (cluster.instantSpawn)
                    {
                      cluster.SpawnFor(ev.Player);
                    }
                    else
                    {
                      cluster.awaitingSpawn.Remove(ev.Player);
                      cluster.awaitingSpawn.Add(ev.Player, cluster.primitives.ToList());
                      cluster.spawning = true;
                    }

                  }
                }
              }
            });
          }
        }

      }

    }

    private void OnChangedSpectator(PlayerChangedSpectatorEventArgs ev)
    {
      if (shouldSpectatorsBeAffectedByPDS)
      {
        if (ev.Player == null || ev.Player.IsNpc || ev.NewTarget == null) return;

        foreach (OptimizedSchematic schematic in optimizedSchematics)
        {
          foreach (PrimitiveCluster cluster in schematic.primitiveClusters)
          {
            if (ev.OldTarget != null && (cluster.insidePlayers.Contains(ev.OldTarget) && !cluster.insidePlayers.Contains(ev.NewTarget)))
            {
              cluster.UnspawnFor(ev.Player);
            }

            if (ev.NewTarget != null && cluster.insidePlayers.Contains(ev.NewTarget) && (ev.OldTarget == null || !cluster.insidePlayers.Contains(ev.OldTarget)))
            {
              cluster.SpawnFor(ev.Player);
            }

          }
        }
      }
    }

    // --------------- Events MER

    private void OnSchematicSpawned(SchematicSpawnedEventArgs ev)
    {

      if (isDynamiclyDisabled)
      {
        Logger.Warn($"Skipping the optimisation of {ev.Schematic.name} because the plugin is dynamicly disabled by command (mero.disable)");
        return;
      }
      if (!hasGenerated)
      {
        Logger.Error($"Unable to generate Optimized Schematic for {ev.Schematic.Name} because mesh filters are not generated yet !");
        return;
      }

      if (ev.Schematic == null) return;

      if (excludedNames.Any(n => ev.Schematic.Name.Contains(n)))
      {
        return;
      }

      List<Transform> parentsToExlude = new List<Transform>();

      foreach (Animator anim in ev.Schematic.GetComponentsInChildren<Animator>())
      {
        if (anim == null) continue;
        parentsToExlude.Add(anim.transform);
      }



      Dictionary<PrimitiveObjectToy, bool> primitivesToOptimize = GetPrimitivesToOptimize(ev.Schematic.transform, parentsToExlude);

      if (primitivesToOptimize == null || primitivesToOptimize.IsEmpty()) return;

      Dictionary<ClientSidePrimitive, bool> clientSidePrimitive = new Dictionary<ClientSidePrimitive, bool>();

      List<Collider> serverSideColliders = new List<Collider>();

      List<PrimitiveObjectToy> primitivesToDestroy = new List<PrimitiveObjectToy>();

      foreach (PrimitiveObjectToy primitive in primitivesToOptimize.Keys.ToList())
      {
        // Retrieve data
        Vector3 position = primitive.transform.position;
        Quaternion rotation = primitive.transform.rotation;
        Vector3 scale = primitive.transform.lossyScale;
        PrimitiveType primitiveType = primitive.PrimitiveType;
        Color color = primitive.NetworkMaterialColor;
        PrimitiveFlags primitiveFlags = primitive.PrimitiveFlags;


        // store the data about the primitive
        clientSidePrimitive.Add(new ClientSidePrimitive(position, rotation, scale, primitiveType, color, primitiveFlags), primitivesToOptimize[primitive]);

        // Add collider for the server if the primitive is collidable
        if (primitiveFlags.HasFlag(PrimitiveFlags.Collidable))
        {
          GameObject collider = new GameObject();
          collider.transform.localScale = new Vector3(Math.Abs(scale.x), Math.Abs(scale.y), Math.Abs(scale.z));
          collider.transform.position = position;
          collider.transform.rotation = rotation;
          collider.transform.name = $"[MEROCOLLIDER] {primitive.transform.name}";
          MeshCollider meshCollider = collider.AddComponent<MeshCollider>();
          meshCollider.convex = true;
          switch (primitiveType)
          {
            case PrimitiveType.Sphere:
              meshCollider.sharedMesh = this.sphereMesh;
              break;
            case PrimitiveType.Capsule:
              meshCollider.sharedMesh = this.capsuleMesh;
              break;
            case PrimitiveType.Cylinder:
              meshCollider.sharedMesh = this.cylinderMesh;
              break;
            case PrimitiveType.Cube:
              meshCollider.sharedMesh = this.cubeMesh;
              break;
            default:
              UnityEngine.GameObject.Destroy(collider);
              break;
          }

          if (meshCollider != null) serverSideColliders.Add(meshCollider);
        }

        primitivesToDestroy.Add(primitive);
      }

      // Store the client side primitive / server side colliders

      float distanceForClusterSpawn = distanceRequiredForUnspawning;

      if (CustomSchematicSpawnDistance.TryGetValue(ev.Schematic.Name, out float customDistance))
      {
        distanceForClusterSpawn = customDistance;
      }

      OptimizedSchematic schematic = new OptimizedSchematic(ev.Schematic, serverSideColliders, clientSidePrimitive,
        hideDistantPrimitives, distanceForClusterSpawn, excludedNamesForUnspawningDistantObjects,
        maxDistanceForPrimitiveCluster, maxPrimitivesPerCluster);

      optimizedSchematics.Add(schematic);



      if (ev.Schematic == null) return;

      foreach (PrimitiveObjectToy primitive in primitivesToDestroy)
      {
        if (primitive == null) continue;
        ev.Schematic._attachedBlocks.Remove(primitive.gameObject);
        GameObject.Destroy(primitive.gameObject);
      }
      Timing.CallDelayed(1f, () =>
      {

        if (ev.Schematic == null || schematic == null) return;
        schematic.schematicServerSidePrimitiveCount = ev.Schematic.GetComponentsInChildren<PrimitiveObjectToy>().Where(p => p != null).Count();
        schematic.schematicServerSidePrimitiveEmptiesCount = ev.Schematic.GetComponentsInChildren<PrimitiveObjectToy>().Where(p => p != null && p.PrimitiveFlags == PrimitiveFlags.None).Count();

      });

      //DestroyPrimitives(ev.Schematic, primitivesToDestroy);

    }


    private void OnSchematicDestroyed(SchematicDestroyedEventArgs ev)
    {
      foreach (OptimizedSchematic optimizedSchematic in optimizedSchematics.Where(s => s != null).ToList())
      {
        if (optimizedSchematic.schematic == null || optimizedSchematic.schematic == ev.Schematic)
        {
          optimizedSchematic.Destroy();
          optimizedSchematics.Remove(optimizedSchematic);
        }
      }
    }

  }
}
