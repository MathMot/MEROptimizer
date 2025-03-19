using CommandSystem;
using Exiled.API.Features;
using Exiled.API.Features.Pickups;
using MEROptimizer.MEROptimizer.Application.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MEROptimizer.MEROptimizer.Application.Commands
{
  [CommandHandler(typeof(RemoteAdminCommandHandler))]
  public class DisplayClustersCmd : ICommand, IUsageProvider
  {
    public string Command { get; } = "mero.displayClusters";

    public string[] Aliases { get; } = new string[] { "mero.dpc" };

    public string Description { get; } = "Display or not all clusters radius of schematics for you only (not accurate)";

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
        response = "You must specify if you want to display or hide the clusters radius ! example : mero.dpc true";
        return false;
      }

      if (!bool.TryParse(arguments.ElementAt(0).ToLower(), out bool display))
      {
        response = $"Unable to parse a correct bool from {arguments.ElementAt(0)}";
        return false;
      }

      foreach (OptimizedSchematic optimizedSchematic in Plugin.merOptimizer.optimizedSchematics)
      {
        if (display)
        {
          foreach (PrimitiveCluster cluster in optimizedSchematic.primitiveClusters)
          {
            cluster.HideRadius(player);
            cluster.DisplayRadius(player);
          }
        }
        else
        {
          foreach (PrimitiveCluster cluster in optimizedSchematic.primitiveClusters)
          {
            cluster.HideRadius(player);
          }
        }

      }
      response = $"Succesfully hidden all of the optimized schematics !";

      return true;
    }
  }
}
