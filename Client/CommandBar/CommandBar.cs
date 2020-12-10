using NLua;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CaveGame.Client;
using CaveGame.Client.UI;
using CaveGame.Core.LuaInterop;

namespace CaveGame.Core
{
	public class Script
	{
		public static void print(params string[] strs)
		{
			Console.WriteLine(strs);
		}
	}

	public delegate void ArgsCallback(params string[] args);

	
	public class Command
	{
		public delegate void CommandHandler(CommandBar sender, Command command, params string[] args);
		public string Keyword;
		public string Description;
		public List<string> Args;
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
			Args = new List<string> { };
			OnCommand += callback;
		}

		public void InvokeCommand(CommandBar sender, params string[] args)
		{
			OnCommand?.Invoke(sender, this, args);
		}
	}

	public class CommandBar : GameComponent, IMessageOutlet
	{

		public void BindCommandInformation(Command command)
		{
			Commands.Add(command);
		}

		public delegate void CommandHandler(CommandBar sender, Command command, params string[] args);

		public event CommandHandler Handler;

		#region Command Methods
		private void QuitGame(params string[] args)
		{
			_game.Exit();
		}

		private void ClearConsole(params string[] args)
		{
			MessageHistory.Clear();
		}

		private void LuaExec(params string[] args)
		{
			string expression = String.Join(" ", args);
			consoleLua.DoString(expression);
		}

		private void CommandList(params string[] args)
		{
			foreach(Command cmd in Commands)
			{
				MessageHistory.Add(new Message(cmd.Keyword + " " + GetArgsInfo(cmd) + " (" + cmd.Description + ")", Color.GreenYellow));
			}
		}

		#endregion


		#region Command Definitions
		public static Command C_Quit = new Command("quit", "Closes the game session", new List<string> { });
		public static Command C_Clear = new Command("clear", "Resets the console", new List<string> { });
		public static Command C_LuaExec = new Command("lua", "", new List<string> { "expression" });
		public static Command C_List = new Command("list", "", new List<string> { });

		public List<Command> Commands = new List<Command> { 
			C_Quit, C_Clear, C_LuaExec, C_List
		};

		#endregion

		private void ProcessCommand(Command command, params string[] args)
		{
			if (command.Equals(C_Quit))
			{
				QuitGame(args);
				return;
			}
			if (command.Equals(C_Clear))
			{
				ClearConsole(args);
				return;
			}
			if (command.Equals(C_LuaExec))
			{
				LuaExec(args);
				return;
			}
				
			if (command.Equals(C_List))
			{
				CommandList(args);
				return;
			}
			command.InvokeCommand(this, args);
			// the primary handler
			Handler?.Invoke(this, command, args);

		}


		List<Message> MessageHistory;
		List<string> CommandHistory { get; set; }

		int inputCurrent { get; set; }

		Lua consoleLua;

		bool autocompletemenuOpen;

		KeyboardState previousKBState;

		TextInput inputBox;

		Microsoft.Xna.Framework.Game _game;

		public void LuaPrint(string data)
		{
			MessageHistory.Add(new Message("lua: "+data, new Color(0.5f, 0.5f, 1.0f)));
		}

		public void Out(Message message)
		{
			MessageHistory.Add(message);
		}
		public void Out(string message, Color color)
		{
			Out(new Message(message, color));
		}

		public void Out(string message)
		{
			Out(new Message(message, Color.White));
		}



		public bool Open { get; set; }

		public CommandBar(Microsoft.Xna.Framework.Game game): base(game)
		{
			inputBox = new TextInput();
			inputBox.ClearOnReturn = true;
			inputBox.Handler += ProcessInput;

			_game = game;
			CommandHistory = new List<string>();
			Open = false;

			TextInputManager.ListenTextInput += OnTextInput;

			autocompletemenuOpen = false;
			MessageHistory = new List<Message>();
			inputCurrent = 0;
			CommandHistory.Add("");
			consoleLua = new Lua();
			consoleLua.LoadCLRPackage();
			consoleLua["console"] = this;
			consoleLua["game"] = game;
			consoleLua.DoString(LuaSnippets.UtilityFunctions);
			consoleLua.DoString("function print(data) console:LuaPrint(tostring(data)) end");
		}


		private void ProcessInput(object sender, string command)
		{
			MessageHistory.Add(new Message(">" + command, new Color(0.75f, 0.75f, 0.75f)));

			
			CommandHistory.Add(command);
			inputCurrent = CommandHistory.Count-1;

			string cleaned = command.Trim();

			string[] keywords = cleaned.Split(' ');

			foreach (Command cmdDef in Commands)
			{
				if (keywords[0] == cmdDef.Keyword) {
					ProcessCommand(cmdDef, keywords.Skip(1).ToArray());
					return;
				}
			}
			MessageHistory.Add(new Message("No command " + keywords[0] + " found!", new Color(1.0f, 0, 0)));

		}
		
		public void OnTextInput(object sender, TextInputEventArgs args)
		{

			if (args.Key == Keys.Escape || args.Key == Keys.Tab || args.Key == Keys.OemTilde)
				return;

			if (!Open)
				return;

			inputBox.OnTextInput(sender, args);

		}

		public string InputBufferProcess(string s)
		{
			if (Math.Floor(inputBox.CursorBlinkTimer * 4) % 2 == 0)
			{
				return s.Insert(inputBox.CursorPosition, "|");
			}
			return s;
		}

		private string GetArgsInfo(Command cmd)
		{
			string additional = "";
			if (cmd.Args != null && cmd.Args.Count > 0)
			{
				additional = String.Join(" ", cmd.Args);
			}
			return additional;
		}

		RasterizerState rastering = new RasterizerState() { ScissorTestEnable = true };
		const float line_height = 16;
		public void Draw(GraphicsEngine GFX)
		{
			if (!Open)
				return;
			#region Draw box
			Color backgroundColor = new Color(0.01f, 0.01f, 0.01f, 0.75f);
			Vector2 screenSize = new Vector2(Game.Window.ClientBounds.Width, Game.Window.ClientBounds.Height);
			Vector2 consoleSize = screenSize - new Vector2(300, 200);
			Vector2 consolePosition = new Vector2(150, 100);

			Color inputBoxColor = new Color(0.15f, 0.15f, 0.25f);
			Vector2 inputBoxPosition = consolePosition + new Vector2(0, consoleSize.Y - 20);
			Vector2 inputBoxSize = new Vector2(consoleSize.X, 20);

			GFX.Begin(SpriteSortMode.Immediate, null, SamplerState.PointClamp, null, rastering);
			GFX.OutlineRect(new Color(0.5f, 0.5f, 0.5f), consolePosition - new Vector2(2, 2), consoleSize + new Vector2(4, 4), 2);
			Rectangle current = GFX.GraphicsDevice.ScissorRectangle;
			GFX.GraphicsDevice.ScissorRectangle = new Rectangle(consolePosition.ToPoint(), consoleSize.ToPoint());
			
			GFX.Rect(backgroundColor, consolePosition, consoleSize);
			GFX.Rect(inputBoxColor, inputBoxPosition, inputBoxSize);
			
			#endregion


			var font = GFX.Fonts.Arial10;

			//Draw message history

			lock (MessageHistory)
			{
				int iter = MessageHistory.Count;
				int visIter = 1;
				var history = MessageHistory.ToArray();
				//history.Reverse();
			//	foreach (Message message in history)
				for (int i = MessageHistory.Count-1; i >= 0; i--)
				{
					var message = history[i];
					string text = message.Text.WrapText(GFX.Fonts.Arial10, consoleSize.X);
					visIter++;
					visIter += text.Count(c => c == '\n');
					GFX.Text(font, text, consolePosition + new Vector2(0, (consoleSize.Y - 20) - (visIter * line_height)), message.TextColor);
					
				}
			}
				
			// Autocomplete suggestion
			if (inputBox.SpecialSelection == false && (inputBox.InputBuffer != "" || autocompletemenuOpen == true)) {
					string cleaned = inputBox.InputBuffer;

					foreach (Command cmd in Commands)
					{
						if (cmd.Keyword.StartsWith(cleaned))
						{

							GFX.Text(font, InputBufferProcess(cmd.Keyword) + " " + GetArgsInfo(cmd), inputBoxPosition , new Color(0.5f, 0.5f, 0.0f));
							break;
						}
					}

					// Command options
					int autocompleteOptionIndex = 0;
					foreach (Command cmd in Commands)
						if (cmd.Keyword.StartsWith(cleaned))
							autocompleteOptionIndex++;

					if (autocompleteOptionIndex > 0)
						GFX.Rect(new Color(0.1f, 0.1f, 0.1f), inputBoxPosition - new Vector2(0, (autocompleteOptionIndex + 1) * line_height), new Vector2(400, autocompleteOptionIndex * line_height));

					var iterator2 = 0;
					foreach (Command cmd in Commands)
					{
						if (cmd.Keyword.StartsWith(cleaned))
						{
							GFX.Text(font, cmd.Keyword + " " + GetArgsInfo(cmd), inputBoxPosition - new Vector2(-5, (iterator2 + 2) * line_height), new Color(1.0f, 1.0f, 0));
							iterator2++;
						}
				}
			}
			

			if (inputBox.SpecialSelection)
			{
				
				var beforeText = inputBox.GetScissorTextBefore();
				var middleText = inputBox.GetScissorTextDuring();
				var afterText = inputBox.GetScissorTextAfter();
				var start = font.MeasureString(beforeText);
				var end = font.MeasureString(middleText);

				GFX.Rect(Color.Blue, inputBoxPosition + new Vector2(start.X, 0), end);

				// first section
				GFX.Text(font, beforeText,  inputBoxPosition, Color.White);
				GFX.Text(font, middleText, inputBoxPosition + new Vector2(start.X, 0), Color.Black);
				GFX.Text(font, afterText, inputBoxPosition + new Vector2(start.X + end.X, 0), Color.White);
			} else
			{
				// Input buffer
				GFX.Text(font, inputBox.DisplayText, inputBoxPosition, new Color(1.0f, 1.0f, 1.0f));
			}

			GFX.GraphicsDevice.ScissorRectangle = current;
			
			GFX.End();
		}

		public override void Update(GameTime gameTime)
		{
			KeyboardState keyboard = Keyboard.GetState();
			if (keyboard.IsKeyDown(Keys.OemTilde) && !previousKBState.IsKeyDown(Keys.OemTilde))
				Open = !Open;

			inputBox.Focused = Open;

			if (Open)
			{

				inputBox.Update(gameTime);


				if (keyboard.IsKeyDown(Keys.Tab) && !previousKBState.IsKeyDown(Keys.Tab))
				{
					if (inputBox.InputBuffer == "")
					{
						autocompletemenuOpen = !autocompletemenuOpen;
					}
					else
					{
						foreach (Command cmd in Commands)
						{
							if (cmd.Keyword.StartsWith(inputBox.InputBuffer))
							{
								inputBox.InputBuffer = cmd.Keyword;
								inputBox.CursorPosition = inputBox.InputBuffer.Length;
							}
						}
					}	
				}
				


				if (keyboard.IsKeyDown(Keys.Up) && !previousKBState.IsKeyDown(Keys.Up))
				{
					
					inputBox.InputBuffer = CommandHistory[inputCurrent];
					inputCurrent = Math.Max(inputCurrent - 1, 0);
				}


				if (keyboard.IsKeyDown(Keys.Down) && !previousKBState.IsKeyDown(Keys.Down))
				{
					inputCurrent = Math.Min(inputCurrent + 1, CommandHistory.Count);

					if (inputCurrent == CommandHistory.Count)
						inputBox.InputBuffer = "";
					else
						inputBox.InputBuffer = CommandHistory[inputCurrent];

				}
			}

			
			previousKBState = keyboard;

			base.Update(gameTime);
		}
	}
}
