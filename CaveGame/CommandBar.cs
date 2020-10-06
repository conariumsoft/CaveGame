using NLua;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CaveGame.Client;

namespace CaveGame.Core
{


	public class Script
	{
		public static void print(params string[] strs)
		{
			Console.WriteLine(strs);
		}
	}
	

	public struct Message
	{
		public string Text;
		public Color TextColor;

		public Message(string text, Color color)
		{
			Text = text;
			TextColor = color;
		}
	}

	public delegate void ArgsCallback(params string[] args);

	public struct Command
	{

		public string Keyword;
		public string Description;
		public List<string> Args;

		public Command(string cmd, string desc = "", List<string> args = default)
		{
			Keyword = cmd;
			Description = desc;
			Args = args;
		}
	}

	public abstract class JNgineGame: Game
	{
		public Color SkyColor { get; set; }
	}

	public class CommandBar : GameComponent
	{

		#region Command Methods
		private void QuitGame(params string[] _)
		{
			_game.Exit();
		}

		private void ClearConsole(params string[] _)
		{
			MessageHistory.Clear();
		}
		private void SetFOV(params string[] args)
		{

			bool success = Int32.TryParse(args[0], out int fov);
			if (!success)
			{
				MessageHistory.Add(new Message("Invalid argument: " + args[0], new Color(1.0f, 0, 0)));
				return;
			}
			_game.Camera.FieldOfView = fov;
		}

		private void SetSkyColor(params string[] args)
		{
			Int32.TryParse(args[0], out int red);
			Int32.TryParse(args[1], out int green);
			Int32.TryParse(args[2], out int blue);
			_game.SkyColor = new Color(red, green, blue); 
		}

		private void LuaExec(params string[] args)
		{
			string expression = String.Join(" ", args);
			consoleLua.DoString(expression);
		}

		private void CommandList(params string[] _)
		{
			foreach(Command cmd in Commands)
			{
				MessageHistory.Add(new Message(cmd.Keyword + " " + GetArgsInfo(cmd) + " (" + cmd.Description + ")", Color.GreenYellow));
			}
		}

		#endregion


		#region Command Definitions
		public static Command C_Quit = new Command("quit", "Closes the game session", null);
		public static Command C_Clear = new Command("clear", "Resets the console", null);
		public static Command C_SetSky = new Command("setSky", "Sets Sky color", new List<string>{"rInt", "gInt", "bInt"});
		public static Command C_SetFOV = new Command("setFOV", "Sets camera field of view", new List<string> {"fovInt"});
		public static Command C_LuaExec = new Command("lua", "", new List<string> { "expression" });
		public static Command C_List = new Command("list", "", null);

		public static  List<Command> Commands = new List<Command> { 
			C_Quit, C_Clear, C_SetSky, C_SetFOV, C_LuaExec, C_List
		};

		#endregion

		private void ProcessCommand(Command command, params string[] args)
		{
			if (command.Equals(C_Quit))
				QuitGame(args);
			if (command.Equals(C_Clear))
				ClearConsole(args);
			if (command.Equals(C_SetSky))
				SetSkyColor(args);
			if (command.Equals(C_SetFOV))
				SetFOV(args);
			if (command.Equals(C_LuaExec))
				LuaExec(args);
			if (command.Equals(C_List))
				CommandList(args);
		}


		List<Message> MessageHistory;
		List<string> CommandHistory;

		int inputCurrent;
		string inputBuffer;
		float cursorTimer;
		int cursorPosition;

		Lua consoleLua;

		bool autocompletemenuOpen;

		KeyboardState previousKBState;

		JNgineGame _game;

		public void LuaPrint(string data)
		{
			MessageHistory.Add(new Message("lua: "+data, new Color(0.5f, 0.5f, 1.0f)));
		}

		public bool Open { get; set; }

		public CommandBar(JNgineGame game): base(game)
		{
			_game = game;
			CommandHistory = new List<string>();
			Open = false;

			autocompletemenuOpen = false;
			MessageHistory = new List<Message>();
			cursorTimer = 0;
			cursorPosition = 0;
			inputCurrent = 0;
			inputBuffer = "";
			CommandHistory.Add("");
			consoleLua = new Lua();
			consoleLua.LoadCLRPackage();
			consoleLua["console"] = this;
			consoleLua.DoString("function print(data) console:LuaPrint(tostring(data)) end");
		}


		private void ProcessInput(string command)
		{
			MessageHistory.Add(new Message(">" + command, new Color(0.75f, 0.75f, 0.75f)));

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
			if (!Open)
				return;

			if (args.Key == Keys.Escape || args.Key == Keys.Tab || args.Key == Keys.OemTilde)
				return;


			if (args.Key == Keys.Enter)
			{
				ProcessInput(inputBuffer);
				CommandHistory.Add(inputBuffer);
				inputBuffer = "";
				inputCurrent = CommandHistory.Count;
				cursorPosition = 0;
				return;
			}

			if (args.Key == Keys.Back)
			{
				if (cursorPosition > 0)
				{
					inputBuffer = inputBuffer.Remove(cursorPosition - 1);
					cursorPosition--;
				}
				return;
			}

			inputBuffer = inputBuffer.Insert(cursorPosition, args.Character.ToString());
			cursorPosition += 1;
		}

		public string InputBufferProcess(string s)
		{
			if (Math.Floor(cursorTimer * 4) % 2 == 0)
			{
				return s.Insert(cursorPosition, "|");
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

		public void Draw(SpriteBatch spriteBatch)
		{
			if (!Open)
				return;
			#region Draw box
			Color backgroundColor = new Color(0, 0, 0, 0.75f);
			Vector2 screenSize = new Vector2(Game.Window.ClientBounds.Width, Game.Window.ClientBounds.Height);
			Vector2 consoleSize = screenSize - new Vector2(Game.Window.ClientBounds.Width/2, 0);
			Vector2 consolePosition = new Vector2(Game.Window.ClientBounds.Width/2, 0);

			Color inputBoxColor = new Color(0.15f, 0.15f, 0.25f);
			Vector2 inputBoxPosition = consolePosition + new Vector2(0, consoleSize.Y - 20);
			Vector2 inputBoxSize = new Vector2(consoleSize.X, 20);

			spriteBatch.Begin();
			spriteBatch.Rect(backgroundColor, consolePosition, consoleSize);
			spriteBatch.Rect(inputBoxColor, inputBoxPosition, inputBoxSize);
			spriteBatch.OutlineRect(new Color(0.0f, 0.0f, 0.0f), consolePosition, consoleSize);
			#endregion

			//Draw message history
			int iter = MessageHistory.Count;
			foreach (Message message in MessageHistory)
			{
				spriteBatch.Print(message.TextColor, consolePosition + new Vector2(0,  (consoleSize.Y-20)-(iter*14)), message.Text);
				iter--;
			}

			// Autocomplete suggestion
			if (inputBuffer != "" || autocompletemenuOpen == true) {
				string cleaned = inputBuffer;

				foreach (Command cmd in Commands)
				{
					if (cmd.Keyword.StartsWith(cleaned))
					{
						
						spriteBatch.Print(new Color(0.5f, 0.5f, 0.0f), inputBoxPosition, InputBufferProcess(cmd.Keyword) + " " + GetArgsInfo(cmd));
						break;
					}
				}

				// Command options
				int autocompleteOptionIndex = 0;
				foreach (Command cmd in Commands)
					if (cmd.Keyword.StartsWith(cleaned))
						autocompleteOptionIndex++;

				if (autocompleteOptionIndex > 0)
					spriteBatch.Rect(new Color(0.0f, 0.0f, 0.0f, 0.5f), inputBoxPosition - new Vector2(-5, (autocompleteOptionIndex+1) * 14), new Vector2(200, autocompleteOptionIndex * 14));
				
				var iterator2 = 0;
				foreach (Command cmd in Commands)
				{
					if (cmd.Keyword.StartsWith(cleaned))
					{
						spriteBatch.Print(new Color(1.0f, 1.0f, 0), inputBoxPosition - new Vector2(-5, (iterator2 + 2) * 14), cmd.Keyword + " " + GetArgsInfo(cmd));
						iterator2++;
					}
				}
			}

			// Input buffer
			spriteBatch.Print(new Color(1.0f, 1.0f, 1.0f), inputBoxPosition, InputBufferProcess(inputBuffer));

			spriteBatch.End();
		}

		public override void Update(GameTime gameTime)
		{
			KeyboardState keyboard = Keyboard.GetState();
			if (keyboard.IsKeyDown(Keys.OemTilde) && !previousKBState.IsKeyDown(Keys.OemTilde))
				Open = !Open;

			if (Open)
			{
				float delta = (float)gameTime.ElapsedGameTime.TotalSeconds;
				cursorTimer += delta;


				if (keyboard.IsKeyDown(Keys.Tab) && !previousKBState.IsKeyDown(Keys.Tab))
				{
					if (inputBuffer == "")
					{
						autocompletemenuOpen = !autocompletemenuOpen;
					}
					else
					{
						foreach (Command cmd in Commands)
						{
							if (cmd.Keyword.StartsWith(inputBuffer))
							{
								inputBuffer = cmd.Keyword;
								cursorPosition = inputBuffer.Length;
							}
						}
					}	
				}
				

				if (keyboard.IsKeyDown(Keys.Left) && !previousKBState.IsKeyDown(Keys.Left))
					cursorPosition--;

				if (keyboard.IsKeyDown(Keys.Right) && !previousKBState.IsKeyDown(Keys.Right))
					cursorPosition++;

				if (keyboard.IsKeyDown(Keys.Up) && !previousKBState.IsKeyDown(Keys.Up))
				{
					inputCurrent = Math.Max(inputCurrent - 1, 0);
					inputBuffer = CommandHistory[inputCurrent];
				}


				if (keyboard.IsKeyDown(Keys.Down) && !previousKBState.IsKeyDown(Keys.Down))
				{
					inputCurrent = Math.Min(inputCurrent + 1, CommandHistory.Count);

					if (inputCurrent == CommandHistory.Count)
						inputBuffer = "";
					else
						inputBuffer = CommandHistory[inputCurrent];

				}


				cursorPosition = Math.Max(cursorPosition, 0);
				cursorPosition = Math.Min(cursorPosition, inputBuffer.Length);
			}

			
			previousKBState = keyboard;

			base.Update(gameTime);
		}
	}
}
