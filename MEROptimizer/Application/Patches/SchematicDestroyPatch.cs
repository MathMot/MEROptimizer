using Mirror;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MEROptimizer.Application.Patches
{/*
  [HarmonyPatch(typeof(SchematicObject), nameof(SchematicObject.OnDestroy))]
  public static class SchematicDestroyPatch
  {
    static bool Prefix(SchematicObject __instance)
    {
      AnimationController.Dictionary.Remove(__instance);

      foreach (GameObject gameobject in __instance.AttachedBlocks)
      {
        if (gameobject == null) continue;

        if (__instance._transformProperties.ContainsKey(gameobject.transform.GetInstanceID()))
        {
          NetworkServer.Destroy(gameobject);
        }
      }
      Schematic.OnSchematicDestroyed(new SchematicDestroyedEventArgs(__instance, __instance.Name));

      return false;
    }

  }  */
}
