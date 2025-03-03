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

      foreach (OptimizedSchematic os in Plugin.merOptimizer.optimizedSchematics)
      {
        message +=
          $"Schematic : {os.schematic.name}\n" +
          $"Spawned at {os.spawnTime.ToShortTimeString()}\n" +
          $"Total primitive : {os.schematicsTotalPrimitives}\n" +
          $"Client side primitive count: {os.primitives.Count}\n" +
          $"Server side primitives count: {os.serverSpawnedPrimitives}\n" +
          $"Number of server side colliders : {os.colliders.Count}\n----------------\n";
      }

      response = message != "" ? message : "No information to display";

      return true;
    }
  }
}
