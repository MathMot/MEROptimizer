using CommandSystem;
using LabApi.Features.Wrappers;
using MEROptimizer.Application.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MEROptimizer.Application.Commands
{
  [CommandHandler(typeof(RemoteAdminCommandHandler))]
  public class ClusterInfoCmd : ICommand
  {
    public string Command { get; } = "mero.clusters";

    public string[] Aliases { get; }

    public string Description { get; } = "Displays information about all of the clusters in schematics.";

    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
    {
      if (!Player.TryGet(sender, out Player player))
      {
        response = $"You must be an active player to execute this command !";
        return false;
      }

      string message = "";

      foreach (OptimizedSchematic os in Plugin.merOptimizer.optimizedSchematics)
      {
        message +=
          $"Schematic : {os.schematic.name}\n" +
          $"Number of clusters : {os.primitiveClusters.Count}";

        foreach (PrimitiveCluster cluster in os.primitiveClusters)
        {
          message += $"\nId : {cluster.id} | Pos : {cluster.transform.position} | Number of primitives : {cluster.primitives.Count}";
        }
      }

      message += $"\n----------------\n";

      response = message != "" ? message : "No information to display";

      return true;
    }
  }
}
