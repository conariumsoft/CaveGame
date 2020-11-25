using CaveGame.Client;
using CaveGame.Core;
using CaveGame.Core.Game.Tiles;
using System;
using System.Diagnostics;
using System.IO;

namespace Cave
{
	public static class Program
	{

		[STAThread]
		static void Main()
		{
			Tile.AssertTileEnumeration();
			using (var game = new CaveGameGL())
			{
				try
                {
					game.Run();
					game.Exit();
				} catch(Exception e)
                {
					string crashReportHTML =
@"
<!DOCTYPE html>
<html>
<head>
<style>
body {
	background-color: #cccccc;
	text: black;
}
.data {
  border: 3px outset black;
  background-color: white;
  color: black;
  text-align: left;
  font-family: monospace;
}
</style>
" + @$"
<title>CaveGame Crash Report</title>
</head>
<body>
<h3>CaveGame Crash Report </h3>

<p>CaveGame has experienced a fatal crash! This file has been generated to document the error, and make reporting bugs to the devs easy.</p>

<p>Game Details:</p>

<p>Exception message: </p>
<div class='data'><p>{e.Message.Replace("at", "<br/>at")}</p></div>

<p>StackTrace:</p>
<div class='data'><p>{e.StackTrace.Replace("at", "<br/>at")}</p></div>

<p>Collected Data:</p>
<div class='data'><p>
GameVersion: {Globals.CurrentVersionFullString} <br/>
NetworkProtocolVersion: {Globals.ProtocolVersion} <br/>
TextureLoadSuccess: {GameTextures.ContentLoaded} <br/>
TextureCount: {GameTextures.Textures.Count} <br/>
Steam Enabled: {game.SteamManager.Enabled}<br/>
Average Recent Framerate: {game.FPSCounter.GetAverageFramerate()}<br/>
Exact Framerate: {game.FPSCounter.GetExactFramerate()}<br/>
ScrDeviceName: {game.Window.ScreenDeviceName} <br/>
GraphicsDeviceStatus: {game.GraphicsDevice.GraphicsDeviceStatus} <br/>
GraphicsProfile: {game.GraphicsDevice.GraphicsProfile} <br/>
GraphicsDebug: {game.GraphicsDevice.GraphicsDebug} <br/>
HardwareModeSwitch: {game.GraphicsDeviceManager.HardwareModeSwitch} <br/>
DesireVSync: {game.GraphicsDeviceManager.SynchronizeWithVerticalRetrace} <br/>
GraphicsProfile: {game.GraphicsDevice.GraphicsProfile} <br/>
GraphicsDebug: {game.GraphicsDevice.GraphicsDebug} <br/>
Window Dimensions: {game.Window.ClientBounds.Width}x{game.Window.ClientBounds.Height} <br/>
In World: {game.InWorldContext?.Active}<br/>
Connection Address: {game.InWorldContext?.ConnectAddress}<br/>
Username: {game.InWorldContext?.NetworkUsername}<br/>
Entity Count: {game.InWorldContext?.World.Entities.Count}<br/>
Chunks Loaded: {game.InWorldContext?.World.Chunks.Count}<br/>
Chunks Awaiting: {game.InWorldContext?.World.RequestedChunks.Count}<br/>
Lighting Thread Status: {game.InWorldContext?.World.Lighting.LightThread.ThreadState}<br/>
Window Dimensions: {game.Window.ClientBounds.Width}x{game.Window.ClientBounds.Height} <br/>
Screen Dimensions: {game.GraphicsDeviceManager.GraphicsDevice.Adapter.CurrentDisplayMode.Width}x{game.GraphicsDeviceManager.GraphicsDevice.Adapter.CurrentDisplayMode.Height}<br/>
Fullscreen: {game.GraphicsDeviceManager.IsFullScreen}<br/>
Settings.FPSLimit: {game.GameSettings.FPSLimit}<br/>
Settings.CameraShake: {game.GameSettings.CameraShake}<br/>
Settings.ParticlesEnabled: {game.GameSettings.Particles}<br/>
Game Context: {game.CurrentGameContext}<br/>
<p></div>
</body>
</html>
";
					string name = "crash.html";//$"crash_{DateTime.Now.ToString("MM-dd-yy-HH-mm-ss")}.html";
					File.WriteAllText(name, crashReportHTML);
					CaveGame.Core.OperatingSystem.OpenUrl(Path.GetFullPath(name));
                }
				
			}

			Process.GetCurrentProcess().CloseMainWindow();
			Environment.Exit(Environment.ExitCode);

		}
	}
}
