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
  public class InfoCmd : ICommand
  {
    public string Command { get; } = "mero.info";

    public string[] Aliases { get; }

    public string Description { get; } = "Displays information about all of the optimized schematics.";

    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
    {
      if (!Player.TryGet(sender, out Player player))
      {
        response = $"You must be an active player to execute this command !";
        return false;
      }

      string message = "";

      int serverSidePrimitives = 0;
      int clientSidePrimitives = 0;

      foreach (OptimizedSchematic os in Plugin.merOptimizer.optimizedSchematics)
      {
        serverSidePrimitives += os.schematicServerSidePrimitiveCount;
        clientSidePrimitives += os.GetTotalPrimitiveCount();
      }

      message = $"Total of server sided primitives : {serverSidePrimitives}\n" +
        $"Total of client side primitives : {clientSidePrimitives}\n" +
        $"Total of primitives {serverSidePrimitives + clientSidePrimitives}" +
        $"\n----------------\n";

      foreach (OptimizedSchematic os in Plugin.merOptimizer.optimizedSchematics)
      {
        message +=
          $"Schematic : {os.schematic.name}\n" +
          $"Spawned at {os.spawnTime.ToLongTimeString()}\n" +
          $"Total primitive count : {os.GetTotalPrimitiveCount() + os.schematicServerSidePrimitiveCount}\n" +
          $"Client side primitive count: {os.GetTotalPrimitiveCount()}\n" +
          $"Server side primitive count: {os.schematicServerSidePrimitiveCount}\n" +
          $"Number of server side colliders : {os.colliders.Count}\n" +
          $"Number of clusters : {os.primitiveClusters.Count}\n----------------\n";
      }

      response = message != "" ? message : "No information to display";

      return true;
    }
  }
}
