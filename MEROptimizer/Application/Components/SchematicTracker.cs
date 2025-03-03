using Exiled.API.Features;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MEROptimizer.MEROptimizer.Application.Components
{
  public class SchematicTracker : MonoBehaviour
  {

    public OptimizedSchematic linkedSchematic;

    public MEROptimizer MEROptimizer;

    void OnDestroy()
    {
      if (linkedSchematic == null) return;
      
      MEROptimizer.optimizedSchematics.Remove(linkedSchematic);
      linkedSchematic.Destroy();
    }
  }
}
