using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.DevConsole;
using MegaCrit.Sts2.Core.DevConsole.ConsoleCommands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using Skz.Ocean.Commands;
using Skz.Ocean.Entities.Players;
using Skz.Ocean.Helper;
using Skz.Ocean.Model;

namespace Skz.Ocean.Console;

public class ThoughtConsoleCmd : AbstractConsoleCmd
{
	public override string CmdName => "thought";

	public override string Args => "[add|remove] <thought-id:string>";

	public override string Description => "Adds/Removes thought from player (add by default)";

	public override bool IsNetworked => true;

	public override CmdResult Process(Player? issuingPlayer, string[] args)
	{
		if (args.Length < 1)
		{
			return new CmdResult(success: false, CmdName + " requires a thought name");
		}
		if (issuingPlayer == null)
		{
			return new CmdResult(success: false, "A run is currently not in progress!");
		}
		string text;
		if (args[0].ToLowerInvariant().Equals("add") || args[0].ToLowerInvariant().Equals("remove"))
		{
			if (args.Length < 2)
			{
				return new CmdResult(success: false, "You need to specify a thought!");
			}
			text = args[1];
		}
		else
		{
			text = args[0];
		}
		ThoughtModel thoughtById = GetThoughtById(text);
		if (thoughtById == null)
		{
			return new CmdResult(success: false, "Unable to create thought '" + text + "'.");
		}
		if (args[0].ToLowerInvariant().Equals("remove"))
		{
			ThoughtModel thoughtById2 = issuingPlayer.GetThoughtById(thoughtById.Id);
			if (thoughtById2 != null)
			{
				TaskHelper.RunSafely(ThoughtCmd.Remove(thoughtById2));
				return new CmdResult(success: true, "Thought removed!");
			}
			return new CmdResult(success: false, "Unable to find thought in player!");
		}
		TaskHelper.RunSafely(ThoughtCmd.Obtain(thoughtById.ToMutable(), issuingPlayer));
		return new CmdResult(success: true, "Added thought '" + text + "'");
	}

	private static ThoughtModel? GetThoughtById(string id)
	{
		id = id.ToUpperInvariant();
		List<ThoughtModel> source = SarkazHelper.GetAllThoughts().Where((ThoughtModel r) => r.Id.Entry.Contains(id)).ToList();
		ThoughtModel thoughtModel = source.FirstOrDefault((ThoughtModel r) => r.Id.Entry == id);
		if (thoughtModel != null)
		{
			return thoughtModel;
		}
		ThoughtModel thoughtModel2 = source.FirstOrDefault((ThoughtModel r) => r.Id.Entry.StartsWith(id));
		if (thoughtModel2 != null)
		{
			return thoughtModel2;
		}
		return source.FirstOrDefault();
	}

	public override CompletionResult GetArgumentCompletions(Player? player, string[] args)
	{
		if (args.Length == 0 || (args.Length == 1 && string.IsNullOrWhiteSpace(args[0])))
		{
			CompletionResult completionResult = new CompletionResult();
			int num = 2;
			List<string> list = new List<string>(num);
			CollectionsMarshal.SetCount(list, num);
			Span<string> span = CollectionsMarshal.AsSpan(list);
			int num2 = 0;
			span[num2] = "add";
			num2++;
			span[num2] = "remove";
			completionResult.Candidates = list;
			completionResult.Type = CompletionType.Subcommand;
			completionResult.ArgumentContext = CmdName;
			return completionResult;
		}
		if (args.Length == 1)
		{
			List<string> list2 = new List<string>();
			string text = args[0].ToLowerInvariant();
			if ("add".StartsWith(text))
			{
				list2.Add("add");
			}
			if ("remove".StartsWith(text))
			{
				list2.Add("remove");
			}
			if (list2.Count > 0)
			{
				return CompleteArgument(list2, Array.Empty<string>(), text, CompletionType.Subcommand);
			}
			List<string> candidates = SarkazHelper.GetAllThoughts().Select((ThoughtModel r) => r.Id.Entry).ToList();
			return CompleteArgument(candidates, Array.Empty<string>(), args[0], CompletionType.Argument, (string candidate, string partialArg) => candidate.Contains(partialArg, StringComparison.OrdinalIgnoreCase));
		}
		if (args.Length == 2)
		{
			List<string> candidates2 = SarkazHelper.GetAllThoughts().Select((ThoughtModel r) => r.Id.Entry).ToList();
			return CompleteArgument(candidates2, new string[1] { args[0] }, args[1], CompletionType.Argument, (string candidate, string partial) => candidate.Contains(partial, StringComparison.OrdinalIgnoreCase));
		}
		return new CompletionResult
		{
			Type = CompletionType.Argument,
			ArgumentContext = CmdName
		};
	}
}
