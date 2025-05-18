using CommandSystem;
using Exiled.API.Features;
using Exiled.API.Features.Pickups;
using MEROptimizer.Application.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MEROptimizer.Application.Commands
{
  [CommandHandler(typeof(RemoteAdminCommandHandler))]
  public class RealTimePrimitiveCountCmd : ICommand
  {
    public string Command { get; } = "mero.realtimedisplay";

    public string[] Aliases { get; } = new string[] { "mero.rtdp" };

    public string Description { get; } = "Displays (or remove) the total count of primitives currently loaded to you every seconds, doesn't work for whitelisted roles";

    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
    {
      if (!Player.TryGet(sender, out Player player))
      {
        response = $"You must be an active player to execute this command !";
        return false;
      }

      bool hasComponent = false;

      PlayerDisplayHint playerDisplayHint = player.GameObject.GetComponent<PlayerDisplayHint>();

      hasComponent = playerDisplayHint != null;

      if (!hasComponent)
      {
        player.GameObject.AddComponent<PlayerDisplayHint>().player = player;
      }
      else
      {
        playerDisplayHint.RemoveComponent();
      }

      response = $"{(hasComponent ? "Removed your constant primitive count display !" : "Adding a constant primitive count display !")}";

      return true;
    }
  }
}
