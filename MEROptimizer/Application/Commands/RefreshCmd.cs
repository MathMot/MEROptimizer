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
  public class RefreshCmd : ICommand
  {
    public string Command { get; } = "mero.refresh";

    public string[] Aliases { get; }

    public string Description { get; } = "Refresh all of the client side primitive of schematics for you only.";

    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
    {
      if (!Player.TryGet(sender, out Player player))
      {
        response = $"You must be an active player to execute this command !";
        return false;
      }

      foreach (OptimizedSchematic optimizedSchematic in Plugin.merOptimizer.optimizedSchematics)
      {
        optimizedSchematic.RefreshFor(player);
      }
      response = $"Succesfully refreshed all of the optimized schematics !";

      return true;
    }
  }
}
