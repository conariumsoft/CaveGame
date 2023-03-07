using CaveGame.Common.Game.Entities;
using CaveGame.Common.Game.Inventory;
using CaveGame.Common.Network;
using CaveGame.Common.Extensions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Text;

namespace CaveGame.Common.Game.Items
{
	public class Nimdoc : Item
	{
		public override int MaxStack => 1;

		public override void Draw(GraphicsEngine GFX, Vector2 position, float scale)
		{

			int frame = ((int)(GFX.GlobalAnimationTimer * 8.0f)).Mod(10);
			var quad = new Rectangle(0, frame * 16, 16, 16);
			GFX.Sprite(GFX.Nimdoc, position, quad, Color.White, Rotation.Zero, Vector2.Zero, scale, SpriteEffects.None, 0);

		}


		public Vector2 RaySurfaceNormal { get; set; }
		public Vector2 RayEndpoint { get; set; }
		public Vector2 RayStartpoint { get; set; }


		public override void OnClientLMBHeld(Player player, IGameClient client, ItemStack stack, GameTime gt)
		{

			RayStartpoint = player.Position;

			MouseState mouse = Mouse.GetState();

			var mp = client.Camera.ScreenToWorldCoordinates(mouse.Position.ToVector2());


			var unitVec = (mp - player.Position);
			unitVec.Normalize();

			var result = client.World.TileRaycast(player.Position, Rotation.FromUnitVector(unitVec), player.Position.Distance(mp)+1);

			if (result.Hit)
			{
				// set goalpoints for electric arc to align to
				RayEndpoint = result.Intersection;
				RaySurfaceNormal = result.SurfaceNormal;

				// damage tile
				// NOTE: this sends a shit ton of packets?, find a way to condense
				client.Send(new DamageTilePacket(result.TileCoordinates, 1));
			}

			base.OnClientLMBHeld(player, client, stack, gt);
		}
		public static Random RNG = new Random();
		public override void OnClientDraw(GraphicsEngine GFX)
		{
			if (MouseDown)
			{
				// debug lines
				GFX.Line(Color.Green, RayStartpoint, RayEndpoint, 1.0f);
				GFX.Line(Color.Yellow, RayEndpoint, RayEndpoint + (RaySurfaceNormal * 10), 1.0f);

				// holy fuck
				Vector2 prev = RayStartpoint;
				int distanceSlices = (int)RayStartpoint.Distance(RayEndpoint)/4;
				for (int i = 0; i < distanceSlices; i++)
				{
					float dist = (i / (float)distanceSlices);
					float range = Math.Clamp(dist*2, 0, 1);
					if (dist > 0.5f)
						range = 1 - (Math.Clamp(dist, 0, 1));

					var stepPoint = RayStartpoint.Lerp(RayEndpoint, i / (float)distanceSlices);
					var wave = Rotation.FromUnitVector((RayStartpoint - RayEndpoint).Unit()).RotateDeg(70 + (RNG.NextFloat()*20));
					float leanTo = (float) Math.Floor(Math.Sin(GFX.GlobalAnimationTimer*2) * 10)/3.0f;
					float waveForm = (float)Math.Sin(i + Math.Round(GFX.GlobalAnimationTimer * 6, 1) * 10);
					var final = stepPoint + (wave.ToUnitVector() * (leanTo + waveForm+ RNG.NextFloat()) * (range*20));
					GFX.Line(Color.Cyan, prev, final, 1.2f);
					prev = final;
				}
			}


			base.OnClientDraw(GFX);
		}
	}
}
