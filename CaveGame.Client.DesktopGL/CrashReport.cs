using CaveGame.Client;
using CaveGame.Common;
using System;
using System.IO;

namespace CaveGame.Client.DesktopGL
{
    public class CrashReport
    {

		CaveGameDesktopClient Game;
		Exception Exception;

		public bool ReuseSingleCrashFile { get; set; }
		public CrashReport(CaveGameDesktopClient game, Exception e)
        {
			Game = game;
			Exception = e;


#if DEBUG
			ReuseSingleCrashFile = true;
#else
			ReuseSingleCrashFile = false;
#endif
		}


		private string GetGameworldStatistics()
        {
			if (Game.GameClientContext != null && Game.GameClientContext.Active)
			{
				return @$"
<b>World Statistics</b> < br />
	In World: { Game.GameClientContext?.Active}< br />
	Connection Address: { Game.GameClientContext?.ConnectAddress}< br />
	Username: { Game.GameClientContext?.NetworkUsername}< br />
	Entity Count: { Game.GameClientContext?.World.Entities.Count}< br />
	Chunks Loaded: { Game.GameClientContext?.World.Chunks.Count}< br />
	Chunks Awaiting: { Game.GameClientContext?.World.RequestedChunks.Count}< br />
	Lighting Thread Status: { Game.GameClientContext?.World.Lighting.LightThread.ThreadState}< br />
				";

			}
			else
				return @$"
<b>World Statistics</b> < br />
	In World: { Game.GameClientContext?.Active}< br />";
			
		}


		public void GenerateHTMLReport()
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

<p>Please consider sending this report to us. It helps us tremendously when tracking down bugs.</p>
<p>You can send this to us via Discord, Steam, or Twitter</p>


<p>Game Details:</p>

<p>Exception message: </p>
<div class='data'><p>{Exception.Message.Replace(" at", "<br/>at")}</p></div>

<p>StackTrace:</p>
<div class='data'><p>{Exception.StackTrace.Replace(" at", "<br/>at")}</p></div>

<p>Collected Data:</p>
<div class='data'><p>
GameVersion: {Globals.CurrentVersionFullString} <br/>
NetworkProtocolVersion: {Globals.ProtocolVersion} <br/>
TextureLoadSuccess: {GraphicsEngine.Instance.ContentLoaded} <br/>
TextureCount: {GraphicsEngine.Instance.Textures.Count} <br/>
Steam Enabled: {Game.Steam.Enabled}<br/>
IsOS64Bit: {Environment.Is64BitOperatingSystem} <br/>
OperatingSystem: {Environment.OSVersion.Platform} platform. {Environment.OSVersion.VersionString} <br/>
Architecture: {Environment.GetEnvironmentVariable("PROCESSOR_ARCHITECTURE")} <br/>
ProcessorID: {Environment.GetEnvironmentVariable("PROCESSOR_IDENTIFIER")} <br/>
ProcessorLevel: {Environment.GetEnvironmentVariable("PROCESSOR_LEVEL")} <br/>
IsCaveGame64Bit: {Environment.Is64BitProcess} <br/>
CLRVersion: {Environment.Version} <br/>
WorkingSet: {Environment.WorkingSet} <br/>
ProcessorCount: {Environment.ProcessorCount} <br/>
SystemUptime: {Environment.TickCount64} <br/>
UserName: {Environment.UserName} <br/>
OSVersion: {Environment.OSVersion.VersionString} <br/>
{GetGameworldStatistics()} <br/>
ScrDeviceName: {Game.Window.ScreenDeviceName} <br/>
GraphicsDeviceStatus: {Game.GraphicsDevice.GraphicsDeviceStatus} <br/>
GraphicsProfile: {Game.GraphicsDevice.GraphicsProfile} <br/>
HardwareModeSwitch: {Game.GraphicsDeviceManager.HardwareModeSwitch} <br/>
DesireVSync: {Game.GraphicsDeviceManager.SynchronizeWithVerticalRetrace} <br/>
Window Dimensions: {Game.Window.ClientBounds.Width}x{Game.Window.ClientBounds.Height} <br/>
Window Dimensions: { Game.Window.ClientBounds.Width}x{ Game.Window.ClientBounds.Height} 
Screen Dimensions: {Game.GraphicsDeviceManager.GraphicsDevice.Adapter.CurrentDisplayMode.Width}x{Game.GraphicsDeviceManager.GraphicsDevice.Adapter.CurrentDisplayMode.Height}<br/>
Fullscreen: {Game.GraphicsDeviceManager.IsFullScreen}<br/>
Settings.FPSLimit: {Game.Settings.FramerateLimit}<br/>
Settings.CameraShake: {Game.Settings.CameraShake}<br/>
Settings.ParticlesEnabled: {Game.Settings.Particles}<br/>
Game Context: {Game.CurrentGameContext}<br/>
<p></div>
</body>
</html>
";
			Directory.CreateDirectory("Crashlogs");
			if (ReuseSingleCrashFile)
            {
				string name = Path.Combine("Crashlogs", "crash.html");//$"crash_{DateTime.Now.ToString("MM-dd-yy-HH-mm-ss")}.html";
				File.WriteAllText(name, crashReportHTML);
				CaveGame.Common.OperatingSystem.OpenUrl(Path.GetFullPath(name));
			} else {
				string name = Path.Combine("Crashlogs", $"crash_{DateTime.Now.ToString("MM-dd-yy-HH-mm-ss")}.html");
				File.WriteAllText(name, crashReportHTML);
				CaveGame.Common.OperatingSystem.OpenUrl(Path.GetFullPath(name));
			}
			
		}

    }
}
