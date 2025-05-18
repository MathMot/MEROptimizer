using AdminToys;
using Exiled.API.Extensions;
using Exiled.API.Features;
using Exiled.API.Features.Toys;
using Exiled.Events.EventArgs.Player;
using MapEditorReborn.API.Features.Objects;
using MapEditorReborn.Events.EventArgs;
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

namespace MEROptimizer.Application
{
  public class MEROptimizer
  {

    private bool hasGenerated = false;

    private Mesh cubeMesh;

    private Mesh sphereMesh;

    private Mesh capsuleMesh;

    private Mesh cylinderMesh;

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


    public List<OptimizedSchematic> optimizedSchematics = new List<OptimizedSchematic>();
    public void Load(Config config)
    {
      //Config
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
      // Exiled Events

      Exiled.Events.Handlers.Player.Verified += OnVerified;
      Exiled.Events.Handlers.Player.Spawned += OnSpawned;
      Exiled.Events.Handlers.Player.ChangingSpectatedPlayer += OnChangingSpectatedPlayer;
      Exiled.Events.Handlers.Server.WaitingForPlayers += OnWaitingForPlayers;


      // MER Events

      MapEditorReborn.Events.Handlers.Schematic.SchematicSpawned += OnSchematicSpawned;
      MapEditorReborn.Events.Handlers.Schematic.SchematicDestroyed += OnSchematicDestroyed;

    }

    public void Unload()
    {
      // Exiled Events

      Exiled.Events.Handlers.Player.Verified -= OnVerified;
      Exiled.Events.Handlers.Player.Spawned -= OnSpawned;
      Exiled.Events.Handlers.Player.ChangingSpectatedPlayer -= OnChangingSpectatedPlayer;
      Exiled.Events.Handlers.Server.WaitingForPlayers -= OnWaitingForPlayers;

      // MER Events

      MapEditorReborn.Events.Handlers.Schematic.SchematicSpawned -= OnSchematicSpawned;
      MapEditorReborn.Events.Handlers.Schematic.SchematicDestroyed -= OnSchematicDestroyed;

      Clear();
    }


    // ---------------------- Private methods

    private void Clear()
    {
      optimizedSchematics.Clear();
    }

    private void GenerateMeshFilters()
    {
      GameObject toy = UnityEngine.GameObject.Instantiate(Primitive.Prefab.gameObject);
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

    Dictionary<PrimitiveObject, bool> GetPrimitivesToOptimize(Transform parent, List<Transform> parentToExclude,
      Dictionary<PrimitiveObject, bool> primitives = null, bool clusterChilds = true)
    {

      if (primitives == null) primitives = new Dictionary<PrimitiveObject, bool>();

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

        if (child.TryGetComponent(out PrimitiveObject primitive))
        {
          if (excludedNames.Any(n => primitive.name.ToLower().Contains(n.ToLower())))
          {
            continue;
          }

          // Keep the quads/planes, colliders are buggy
          if ((primitive.Primitive.Type == PrimitiveType.Quad || primitive.Primitive.Type == PrimitiveType.Plane)
            && primitive.Primitive.Flags.HasFlag(PrimitiveFlags.Collidable))
          {
            continue;
          }

          if (this.excludeCollidables)
          {
            if (primitive.Primitive.Flags.HasFlag(PrimitiveFlags.Collidable)) continue;
          }

          primitives.Add(primitive, clusterChilds);
          continue;
        }

        if (!child.TryGetComponent(out MapEditorObject _) && !parentToExclude.Contains(child))
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
      Log.Debug($"Adding PlayerTrigger to {player.DisplayNickname}({player.Id}) !");
      GameObject playerTrigger = new GameObject($"{player.Id}_MERO_TRIGGER");
      playerTrigger.tag = "Player";

      Rigidbody rb = playerTrigger.AddComponent<Rigidbody>();
      rb.isKinematic = true;

      playerTrigger.AddComponent<BoxCollider>().size = new Vector3(1, 2, 1); // epic representation of a player's collision

      playerTrigger.AddComponent<PlayerTrigger>().player = player;

    }
    private void OnVerified(VerifiedEventArgs ev)
    {
      if (ev.Player == null || !ev.Player.IsVerified) return;

      AddPlayerTrigger(ev.Player);
      foreach (OptimizedSchematic schematic in optimizedSchematics.Where(s => s != null && s.schematic != null))
      {
        Log.Debug($"Displaying static client sided primitives of {schematic.schematic.Name} to {ev.Player.DisplayNickname} because he just connected !");
        schematic.SpawnClientPrimitives(ev.Player);
      }
    }

    // one of the worst code i've ever written, i'm sorry about that
    private void OnSpawned(SpawnedEventArgs ev)
    {
      if (ev.Player == null) return;

      if (!ev.Player.IsVerified)
      {
        if (ev.Player.IsNPC)
        {
          bool hasFound = false;

          for (int i = 0; i < ev.Player.GameObject.transform.childCount; i++)
          {
            Transform child = ev.Player.GameObject.transform.GetChild(i);
            if (child != null && child.name == $"{ev.Player.Id}_MERO_TRIGGER")
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

      }
      else
      {

        // just spawned as a spectator, we spawn all clusters primitives for him
        if ((ev.Player.Role.Type == RoleTypeId.Spectator || ev.Player.Role.Type == RoleTypeId.Overwatch) && !shouldSpectatorsBeAffectedByPDS)
        {
          // Unspawning and then respawning primitives at the same frame causes the game to shit itself, so a delay is needed
          Timing.CallDelayed(.5f, () =>
          {
            if (ev.Player != null && (ev.Player.Role.Type == RoleTypeId.Spectator || ev.Player.Role.Type == RoleTypeId.Overwatch))
            {
              foreach (OptimizedSchematic schematic in optimizedSchematics.Where(s => s != null && s.schematic != null))
              {
                Log.Debug($"Spawning all clusters (as a fade spawn) of {schematic.schematic.Name} to {ev.Player.DisplayNickname} because he spawned as a spectator (ssbadbs : {shouldSpectatorsBeAffectedByPDS})");

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
        if (!ShouldTutorialsBeAffectedByDistanceSpawning && ev.Player.Role.Type == RoleTypeId.Tutorial)
        {
          Timing.CallDelayed(.5f, () =>
          {

            if (ev.Player != null && ev.Player.Role.Type == RoleTypeId.Tutorial)
            {
              foreach (OptimizedSchematic schematic in optimizedSchematics.Where(s => s != null && s.schematic != null))
              {
                Log.Debug($"Spawning all clusters (as a fade spawn) of {schematic.schematic.Name} to {ev.Player.DisplayNickname} because he spawned as a tutorial and based on the specified config he should see all of the map (ssbadbs : {shouldSpectatorsBeAffectedByPDS})");

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
            Log.Debug($"Unspawning all clusters of {schematic.schematic.Name} to {ev.Player.DisplayNickname} because he just changed role (ssbadbs : {shouldSpectatorsBeAffectedByPDS})");
            foreach (PrimitiveCluster cluster in schematic.primitiveClusters)
            {
              if (!cluster.insidePlayers.Contains(ev.Player))
              {
                cluster.UnspawnFor(ev.Player);
              }

            }
          }

          if (ev.Player.Role.Type == RoleTypeId.Filmmaker || ev.Player.Role.Type == RoleTypeId.Scp079)
          {
            Timing.CallDelayed(.5f, () =>
            {
              if (ev.Player != null && (ev.Player.Role.Type == RoleTypeId.Filmmaker || ev.Player.Role.Type == RoleTypeId.Scp079))
              {
                foreach (OptimizedSchematic schematic in optimizedSchematics.Where(s => s != null && s.schematic != null))
                {
                  Log.Debug($"Spawning all clusters (as a fade spawn) of {schematic.schematic.Name} to {ev.Player.DisplayNickname} because he spawned as a filmaker ( why ) and based on the specified config he should see all of the map (ssbadbs : {shouldSpectatorsBeAffectedByPDS})");

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

    private void OnChangingSpectatedPlayer(ChangingSpectatedPlayerEventArgs ev)
    {
      if (shouldSpectatorsBeAffectedByPDS)
      {
        if (ev.Player == null || (!ev.Player.IsVerified && !ev.Player.IsNPC) || ev.NewTarget == null) return;

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
        Log.Warn($"Skipping the optimisation of {ev.Schematic.name} because the plugin is dynamicly disabled by command (mero.disable)");
        return;
      }
      if (!hasGenerated)
      {
        Log.Error($"Unable to generate Optimized Schematic for {ev.Schematic.Name} because mesh filters are not generated yet !");
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

      Dictionary<PrimitiveObject, bool> primitivesToOptimize = GetPrimitivesToOptimize(ev.Schematic.transform, parentsToExlude);

      if (primitivesToOptimize == null || primitivesToOptimize.IsEmpty()) return;

      Dictionary<ClientSidePrimitive, bool> clientSidePrimitive = new Dictionary<ClientSidePrimitive, bool>();

      List<Collider> serverSideColliders = new List<Collider>();

      List<PrimitiveObject> primitivesToDestroy = new List<PrimitiveObject>();

      foreach (PrimitiveObject primitive in primitivesToOptimize.Keys.ToList())
      {
        // Retrieve data

        Vector3 position = primitive.Position;
        Quaternion rotation = primitive.Rotation;

        Vector3 scale = primitive.transform.lossyScale;

        PrimitiveType primitiveType = primitive.Primitive.Type;

        Color color = primitive.Primitive.Base.NetworkMaterialColor;

        PrimitiveFlags primitiveFlags = primitive.Primitive.Flags;

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
      Log.Debug($"Destroying server-side primitives of {ev.Schematic.Name}");
      DestroyPrimitives(ev.Schematic, primitivesToDestroy);

      Timing.CallDelayed(1f, () =>
      {
        if (schematic == null || ev.Schematic == null) return;
        schematic.schematicServerSidePrimitiveCount = ev.Schematic.GetComponentsInChildren<PrimitiveObject>().Count();


      });

    }

    private void DestroyPrimitives(SchematicObject schematic, List<PrimitiveObject> primitives)
    {
      foreach (PrimitiveObject primitive in primitives.Where(p => p != null && p.gameObject != null))
      {
        schematic?.AttachedBlocks.Remove(primitive.gameObject);
        primitive.Destroy();
      }
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
