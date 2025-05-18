using CommandSystem;
using LabApi.Features.Wrappers;
using MEC;
using MEROptimizer.Application.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MEROptimizer.Application.Commands
{
  [CommandHandler(typeof(RemoteAdminCommandHandler))]
  public class DisplayPrimitivesCmd : ICommand, IUsageProvider
  {
    public string Command { get; } = "mero.displayPrimitives";

    public string[] Aliases { get; } = new string[] { "mero.dp" };

    public string Description { get; } = "Display or not all client side primitives of schematics for you only";

    public string[] Usage { get; } = new string[] { "Display or hide (true/false)" };

    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
    {
      if (!Player.TryGet(sender, out Player player))
      {
        response = $"You must be an active player to execute this command !";
        return false;
      }

      if (arguments.Count < 1)
      {
        response = "You must specify if you want to display or hide the primitives ! example : mero.dp true";
        return false;
      }

      if (!bool.TryParse(arguments.ElementAt(0).ToLower(), out bool display))
      {
        response = $"Unable to parse a correct bool from {arguments.ElementAt(0)}";
        return false;
      }

      List<OptimizedSchematic> hiddenSchematics = new List<OptimizedSchematic>();

      foreach (OptimizedSchematic optimizedSchematic in Plugin.merOptimizer.optimizedSchematics)
      {
        if (display) hiddenSchematics.Add(optimizedSchematic);
        optimizedSchematic.HideFor(player);

        foreach (PrimitiveCluster cluster in optimizedSchematic.primitiveClusters)
        {
          cluster.UnspawnFor(player);
        }
      }

      if (display)
      {
        Timing.CallDelayed(.5f, () =>
        {
          foreach (OptimizedSchematic optimizedSchematic in hiddenSchematics)
          {
            optimizedSchematic.RefreshFor(player);

            foreach (PrimitiveCluster cluster in optimizedSchematic.primitiveClusters)
            {
              cluster.SpawnFor(player);
            }
          }
        });
      }


      response = $"Succesfully {(display ? "displayed(.5 seconds delay)" : "hidden")} all of the optimized schematics !";

      return true;
    }
  }
}
