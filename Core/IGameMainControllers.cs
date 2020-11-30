using CaveGame.Core.Game.Entities;
using CaveGame.Core.Generic;
using CaveGame.Core.Network;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace CaveGame.Core
{

	public enum TextXAlignment
	{
		Left,
		Center,
		Right
	}
	public enum TextYAlignment
	{
		Top,
		Center,
		Bottom,

	}
	public enum GameSteamAchievement : byte
	{
		HELLO_WORLD = 0,
		MORNING_WOOD = 1,

	}

	public interface IEntityManager
    {
		int GetNextEntityNetworkID();
    }
	public interface ISteamManager
    {
		bool HasAchievement(GameSteamAchievement ach);
		void AwardAchievement(GameSteamAchievement ach);
    }
	public interface ICaveGame
    {

    }

	public interface ICommonGameDriver
    {
		IMessageOutlet Output { get; }
	}

	public interface IGameClient
	{
		Camera2D Camera { get; }
		void Send(Packet p);
		IClientWorld World { get; }
		
	}

	public interface IGameServer
	{
		void SendTo(Packet p, User user);
		void SendToAll(Packet p);
		void SendToAllExcept(Packet p, User exclusion);
		User GetConnectedUser(IPEndPoint ep);
		void OutputAndChat(string text);
		void Chat(string text);
		void Chat(string text, Color color);
		IServerWorld World { get; }
		void SpawnEntity(IEntity entity);
		void Update(GameTime gt);
		int TickRate { get; }
		int MaxPlayers { get; }
		IEntityManager EntityManager { get; }

	}

	// TODO: Make comprehensive render contract
	public interface IGraphicsEngine
    {
		Vector2 WindowSize { get; set; }
		SpriteBatch SpriteBatch { get; set; }

		SpriteSortMode SpriteSortMode { get; set; }
		BlendState BlendState { get; set; }
		SamplerState SamplerState { get; set; }
		DepthStencilState DepthStencilState { get; set; }
		RasterizerState	RasterizerState { get; set; }
		Effect Shader { get; set; }
		Matrix Matrix { get; set; }

		void Begin(SpriteSortMode sorting = SpriteSortMode.Deferred, BlendState blending = null, SamplerState sampling = null, 
			DepthStencilState depthStencil = null, RasterizerState rasterizing = null, Effect effect = null, Matrix? transform = null);
		void Begin();
		void End();

    }

	// todo: sound
	public interface ISoundEngine
    {

    }




}
