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
  public class HideCmd : ICommand
  {
    public string Command { get; } = "mero.hide";

    public string[] Aliases { get; }

    public string Description { get; } = "Hide all client side primitives of schematics for you only. ( use mero.refresh to revert )";

    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
    {
      if (!Player.TryGet(sender, out Player player))
      {
        response = $"You must be an active player to execute this command !";
        return false;
      }

      foreach (OptimizedSchematic optimizedSchematic in Plugin.merOptimizer.optimizedSchematics)
      {
        optimizedSchematic.HideFor(player);
      }
      response = $"Succesfully hidden all of the optimized schematics !";

      return true;
    }
  }
}
