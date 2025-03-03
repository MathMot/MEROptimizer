using Exiled.API.Features;
using MapEditorReborn.API.Features.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MEROptimizer.MEROptimizer.Application.Components
{
  public class OptimizedSchematic
  {
    public SchematicObject schematic { get; set; }

    public List<Collider> colliders { get; set; }

    public List<ClientSidePrimitive> primitives { get; set; }

    public DateTime spawnTime { get; set; }

    public int schematicsTotalPrimitives { get; set; }

    public int serverSpawnedPrimitives => schematicsTotalPrimitives - primitives.Count;

    public OptimizedSchematic(SchematicObject schematic, List<Collider> colliders, List<ClientSidePrimitive> primitives)
    {
      this.schematic = schematic;
      this.colliders = colliders;
      this.primitives = primitives;
      spawnTime = DateTime.Now;
    }

    public void ShowFor(Player player)
    {
      HideFor(player);

      foreach (ClientSidePrimitive primitive in primitives)
      {
        primitive.SpawnClientPrimitive(player);
      }

    }

    public void HideFor(Player player)
    {
      foreach (ClientSidePrimitive primitive in primitives)
      {
        primitive.DestroyClientPrimitive(player);
      }
    }



    public void SpawnClientPrimitivesToAll()
    {
      Log.Debug($"Displaying {this.schematic.name}'s client side primitives !");
      foreach (Player player in Player.List.Where(p => p != null && p.IsVerified))
      {
        SpawnClientPrimitives(player);
      }
    }

    public void SpawnClientPrimitives(Player player)
    {
      foreach (ClientSidePrimitive primitive in primitives)
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

      foreach (ClientSidePrimitive primitive in primitives)
      {
        primitive.DestroyForEveryone();
      }
    }
  }
}
