using AdminToys;
using Exiled.API.Features;
using Exiled.API.Features.Toys;
using Mirror;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MEROptimizer.MEROptimizer.Application.Components
{
  public class ClientSidePrimitive
  {
    public Vector3 position { get; set; }
    public Quaternion rotation { get; set; }
    public Vector3 scale { get; set; }
    public PrimitiveType primitiveType { get; set; }
    public Color color { get; set; }
    public PrimitiveFlags primitiveFlags { get; set; }

    public SpawnMessage spawnMessage { get; set; }

    public ObjectDestroyMessage destroyMessage { get; set; }

    public uint netId { get; set; }


    public ClientSidePrimitive(Vector3 position, Quaternion rotation, Vector3 scale, PrimitiveType primitiveType, Color color, PrimitiveFlags primitiveFlags)
    {
      this.position = position;
      this.rotation = rotation;
      this.scale = scale;
      this.primitiveType = primitiveType;
      this.color = color;
      this.primitiveFlags = primitiveFlags;
      this.netId = NetworkIdentity.GetNextNetworkId();
      GenerateNetworkMessages();
    }

    private void GenerateNetworkMessages()
    {
      NetworkWriterPooled writer = NetworkWriterPool.Get();
      writer.Write<byte>(1);
      writer.Write<byte>(67);
      writer.Write<Vector3>(position);
      writer.Write<Quaternion>(rotation);
      writer.Write<Vector3>(scale);
      writer.Write<byte>(0);
      writer.Write<bool>(false);
      writer.Write<int>((int)primitiveType);
      writer.Write<Color>(color);
      writer.Write<byte>((byte)(primitiveFlags));
      writer.Write<uint>(0);

      spawnMessage = new SpawnMessage()
      {
        netId = netId,
        isLocalPlayer = false,
        isOwner = false,
        sceneId = 0,
        assetId = Primitive.Prefab.netIdentity.assetId,
        position = position,
        rotation = rotation,
        scale = scale,
        payload = writer.ToArraySegment()
      };

      destroyMessage = new ObjectDestroyMessage()
      {
        netId = netId,
      };

    }

    public void DestroyForEveryone()
    {
      foreach (Player player in Player.List.Where(p => p != null && p.IsVerified))
      {
        DestroyClientPrimitive(player);
      }
    }

    public void DestroyClientPrimitive(Player target)
    {
      target?.Connection?.Send(destroyMessage);
    }

    public void SpawnForEveryone()
    {
      foreach (Player player in Player.List.Where(p => p != null && p.IsVerified))
      {
        SpawnClientPrimitive(player);
      }
    }

    public void SpawnClientPrimitive(Player target)
    {
      //DestroyClientPrimitive(target);
      target?.Connection?.Send(spawnMessage);
    }
  }
}
