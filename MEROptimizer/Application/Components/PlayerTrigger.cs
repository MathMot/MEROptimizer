using Exiled.API.Features;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MEROptimizer.MEROptimizer.Application.Components
{
  public class PlayerTrigger : MonoBehaviour
  {

    public Player player { get; set; }

    private Vector3 offset { get; set; }

    void Start()
    {
      offset = new Vector3(0, 2000, 0);
    }


    void Update()
    {
      if (player == null || player.ReferenceHub == null || player.ReferenceHub.transform == null)
      {
        UnityEngine.GameObject.Destroy(this.gameObject);
        return;
      }

      this.transform.position = player.ReferenceHub.transform.position + offset;
    }


  }
}
