using AdminToys;
using Exiled.API.Features;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MEROptimizer.MEROptimizer.Application.Components
{
  public class PrimitiveCluster : MonoBehaviour
  {
    public int id { get; set; }
    public List<ClientSidePrimitive> primitives { get; set; }

    public ClientSidePrimitive displayClusterPrimitive { get; set; }

    private Dictionary<Player, List<ClientSidePrimitive>> awaitingSpawn = new Dictionary<Player, List<ClientSidePrimitive>>();

    public List<Player> insidePlayers = new List<Player>();

    public void Start()
    {
      float radius = this.GetComponent<SphereCollider>().radius;
      displayClusterPrimitive = new ClientSidePrimitive(this.transform.position - new Vector3(0, 2000, 0),
        this.transform.rotation, Vector3.one * (radius / 2), PrimitiveType.Sphere, new Color(1, 0, 0, .5f), PrimitiveFlags.Visible);
    }

    public void OnDestroy()
    {
      foreach (ClientSidePrimitive primitive in primitives)
      {
        primitive.DestroyForEveryone();
      }
      displayClusterPrimitive?.DestroyForEveryone();
    }

    public void OnTriggerEnter(Collider collider)
    {
      // check player, collider.CompareTag("Player") blabla
      if (collider == null || collider.transform.parent == null) return;
      if (!collider.CompareTag("Player") || !Player.TryGet(collider.transform.parent.gameObject, out Player player)) return;

      if (!player.IsNPC)
      {
        awaitingSpawn.Remove(player);

        awaitingSpawn.Add(player, primitives.ToList());
      }



      if (!insidePlayers.Contains(player)) {
        insidePlayers.Add(player);
      }

    }

    public void Update()
    {
      foreach (Player player in awaitingSpawn.Keys.ToList())
      {
        List<ClientSidePrimitive> list = awaitingSpawn[player];

        if (list.IsEmpty())
        {
          awaitingSpawn.Remove(player);
          break;
        }

        List<Player> spectatingPlayers = player.CurrentSpectatingPlayers.ToList();

        // 1 = config, number of prim spawned per frame
        for (int i = 0; i < 1; i++)
        {
          ClientSidePrimitive prim = list.First();

          list.Remove(prim);

          prim.SpawnClientPrimitive(player);

          foreach(Player p in spectatingPlayers)
          {
            prim.SpawnClientPrimitive(p);
          }
        }



      }
    }
    public void OnTriggerExit(Collider collider)
    {
      // check player, collider.CompareTag("Player") blabla
      if (collider == null || collider.transform.parent == null) return;
      if (!collider.CompareTag("Player") || !Player.TryGet(collider.transform.parent.gameObject, out Player player)) return;

      awaitingSpawn.Remove(player);

      UnspawnFor(player);

      insidePlayers.Remove(player);

    }


    public void SpawnFor(Player player)
    {
      if (player == null || !player.IsVerified) return;
      foreach (ClientSidePrimitive primitive in primitives)
      {
        primitive.SpawnClientPrimitive(player);
      }
    }

    public void UnspawnFor(Player player)
    {
      if (player == null || !player.IsVerified) return;

      List<Player> spectatingPlayers = player.CurrentSpectatingPlayers.ToList();
      foreach (ClientSidePrimitive primitive in primitives)
      {
        primitive.DestroyClientPrimitive(player);

        foreach(Player p in spectatingPlayers)
        {
          primitive.DestroyClientPrimitive(p);
        }
      }


    }

    public void DisplayRadius(Player player)
    {
      displayClusterPrimitive?.SpawnClientPrimitive(player);
    }

    public void HideRadius(Player player)
    {
      displayClusterPrimitive?.DestroyClientPrimitive(player);
    }
  }
}
