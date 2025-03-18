using AdminToys;
using Exiled.API.Extensions;
using Exiled.API.Features;
using Exiled.API.Features.Toys;
using Exiled.Events.EventArgs.Player;
using MapEditorReborn.API.Features.Objects;
using MapEditorReborn.Events.EventArgs;
using MEC;
using MEROptimizer.MEROptimizer.Application.Components;
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

namespace MEROptimizer.MEROptimizer.Application
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

    private bool shouldSpectatorsBeAffectedByPDS;

    private float distanceRequiredForUnspawning;

    private int maxDistanceForPrimitiveCluster;

    private int maxPrimitivesPerCluster;

    private List<string> excludedNamesForUnspawningDistantObjects;

    public List<OptimizedSchematic> optimizedSchematics = new List<OptimizedSchematic>();
    public void Load(Config config)
    {
      //Config
      excludeCollidables = config.OptimizeOnlyNonCollidable;
      excludedNames = config.excludeObjects;

      hideDistantPrimitives = config.HideDistantPrimitivesToPlayers;
      distanceRequiredForUnspawning = config.DistanceRequiredForUnspawningPrimitives;
      excludedNamesForUnspawningDistantObjects = config.excludeUnspawningDistantObjects;
      maxDistanceForPrimitiveCluster = config.MaxDistanceForPrimitiveCluster;
      maxPrimitivesPerCluster = config.MaxPrimitivesPerCluster;
      shouldSpectatorsBeAffectedByPDS = config.ShouldSpectatorsBeAffectByPDS;

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
            GetPrimitivesToOptimize(child, parentToExclude, primitives);
          }
        }
      }

      return GetPrimitivesToOptimize(null, parentToExclude, primitives);
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
    private void OnVerified(VerifiedEventArgs ev)
    {
      if (ev.Player == null || !ev.Player.IsVerified) return;

      GameObject playerTrigger = new GameObject($"{ev.Player.Id}_MERO_TRIGGER");
      playerTrigger.transform.parent = ev.Player.GameObject.transform;
      playerTrigger.transform.localPosition = new Vector3(0, 2000, 0);
      playerTrigger.tag = "Player";
      Rigidbody rb = playerTrigger.AddComponent<Rigidbody>();
      rb.isKinematic = true;
      playerTrigger.AddComponent<BoxCollider>();

      foreach (OptimizedSchematic schematic in optimizedSchematics)
      {
        schematic.SpawnClientPrimitives(ev.Player);
      }
    }

    private void OnSpawned(SpawnedEventArgs ev)
    {
      if (ev.Player == null || !ev.Player.IsVerified) return;

      if (!shouldSpectatorsBeAffectedByPDS)
      {
        // just spawned as a spectator, we spawn all clusters primitives for him
        if (ev.Player.Role.Type == RoleTypeId.Spectator)
        {
          foreach (OptimizedSchematic schematic in optimizedSchematics)
          {
            foreach (PrimitiveCluster cluster in schematic.primitiveClusters)
            {
              cluster.SpawnFor(ev.Player);
            }
          }
        } // the player just spawned and was a spectator
        else if (ev.OldRole.Type == RoleTypeId.Spectator)
        {
          foreach (OptimizedSchematic schematic in optimizedSchematics)
          {
            foreach (PrimitiveCluster cluster in schematic.primitiveClusters)
            {
              cluster.UnspawnFor(ev.Player);
            }
          }
        }
      }
    }

    private void OnChangingSpectatedPlayer(ChangingSpectatedPlayerEventArgs ev)
    {
      if (shouldSpectatorsBeAffectedByPDS)
      {
        if (ev.Player == null || !ev.Player.IsVerified) return;

        foreach (OptimizedSchematic schematic in optimizedSchematics)
        {
          foreach (PrimitiveCluster cluster in schematic.primitiveClusters)
          {
            if (ev.OldTarget != null && cluster.insidePlayers.Contains(ev.OldTarget))
            {
              cluster.UnspawnFor(ev.Player);
            }

            if (ev.NewTarget != null && cluster.insidePlayers.Contains(ev.NewTarget))
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

      if (!hasGenerated)
      {
        Log.Error($"Unable to generate Optimized Schematic for {ev.Schematic.Name} because mesh filters are not generated yet !");
        return;
      }

      if (ev.Schematic == null) return;

      List<Transform> parentsToExlude = new List<Transform>();

      foreach (Animator anim in ev.Schematic.GetComponentsInChildren<Animator>())
      {
        if (anim == null) continue;
        parentsToExlude.Add(anim.transform);
      }

      Dictionary<PrimitiveObject, bool> primitivesToOptimize = GetPrimitivesToOptimize(ev.Schematic.transform, parentsToExlude);

      if (primitivesToOptimize == null || primitivesToOptimize.IsEmpty()) return;

      int totalPrimitiveCount = ev.Schematic.GetComponentsInChildren<PrimitiveObject>().Count();

      Dictionary<ClientSidePrimitive, bool> clientSidePrimitive = new Dictionary<ClientSidePrimitive, bool>();

      List<Collider> serverSideColliders = new List<Collider>();

      List<PrimitiveObject> primitivesToDestroy = new List<PrimitiveObject>();

      foreach (PrimitiveObject primitive in primitivesToOptimize.Keys.ToList())
      {

        // Retrieve data

        Vector3 position = primitive.Position;
        Quaternion rotation = primitive.Rotation;
        Vector3 scale = primitive.Scale;

        PrimitiveType primitiveType = primitive.Primitive.Type;

        Color color = primitive.Primitive.Base.NetworkMaterialColor;

        PrimitiveFlags primitiveFlags = primitive.Primitive.Flags;

        // store the data about the primitive

        clientSidePrimitive.Add(new ClientSidePrimitive(position, rotation, scale, primitiveType, color, primitiveFlags), primitivesToOptimize[primitive]);

        // Add collider for the server if the primitive is collidable
        if (primitiveFlags.HasFlag(PrimitiveFlags.Collidable))
        {
          GameObject collider = new GameObject();
          collider.transform.localScale = scale;
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

      OptimizedSchematic schematic = new OptimizedSchematic(ev.Schematic, serverSideColliders, clientSidePrimitive,
        hideDistantPrimitives, distanceRequiredForUnspawning, excludedNamesForUnspawningDistantObjects,
        maxDistanceForPrimitiveCluster, maxPrimitivesPerCluster)
      {
        schematicsTotalPrimitives = totalPrimitiveCount

      };

      optimizedSchematics.Add(schematic);
      Timing.CallDelayed(1f, () =>
      {
        if (ev.Schematic == null) return;
        Log.Debug($"Destroying server-side primitives of {ev.Schematic.Name}");
        DestroyPrimitives(ev.Schematic, primitivesToDestroy);
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
      foreach (OptimizedSchematic optimizedSchematic in optimizedSchematics.ToList())
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
