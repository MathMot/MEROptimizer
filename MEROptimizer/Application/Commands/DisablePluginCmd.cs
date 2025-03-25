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
  public class DisablePluginCmd : ICommand
  {
    public string Command { get; } = "mero.disable";

    public string[] Aliases { get; } = new string[] { "mero.d" };

    public string Description { get; } = "Disable or enable the optimisation of newly created schematics";

    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
    {
      MEROptimizer.isDynamiclyDisabled = !MEROptimizer.isDynamiclyDisabled;

      response = $"New spawned schematics {(MEROptimizer.isDynamiclyDisabled ? "<color=red>will not" : "<color=green>will")}</color> be optimized !";

      return true;
    }
  }
}
