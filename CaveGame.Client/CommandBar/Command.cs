using System.Collections.Generic;

namespace CaveGame.Common;

public class Command
{
    public delegate void CommandHandler(CommandBar sender, Command command, params string[] args);

    public string Keyword { get; init; }
    public string Description { get; init; }
    public List<string> Args { get; init; }
    public event CommandHandler OnCommand;

    public Command(string cmd, string desc, List<string> args)
    {
        Keyword = cmd;
        Description = desc;
        Args = args;
    }
    public Command(string cmd, string desc, List<string> args, CommandHandler callback)
    {
        Keyword = cmd;
        Description = desc;
        Args = args;
        OnCommand += callback;
    }
    public Command(string cmd, string desc, CommandHandler callback)
    {
        Keyword = cmd;
        Description = desc;
        Args = new List<string>();
        OnCommand += callback;
    }

    public void InvokeCommand(CommandBar sender, params string[] args)
    {
        OnCommand?.Invoke(sender, this, args);
    }
}