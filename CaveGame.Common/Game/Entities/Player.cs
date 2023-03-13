// What the fuck??
// FIXME: Conditional Using Declarations???
#if CLIENT
using CaveGame.Client;
using CaveGame.Common.Game.Inventory;
#endif
using CaveGame.Common.Game.Inventory;
using CaveGame.Common.Game.Inventory;
using CaveGame.Common.Network;
using CaveGame.Common.Game.Tiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using CaveGame.Common.Extensions;
using CaveGame.Common.Game.Items;

namespace CaveGame.Common.Game.Entities
{

	public interface IExplosionDamagable
	{
		void Damage(DamageType type, IDamageSource source, int amount);
		void Damage(DamageType type, IDamageSource source, int amount, Vector2 direction);
	}
	
	public class Humanoid : PhysicsEntity, IExplosionDamagable
	{
		public virtual int BaseDefense { get; }
		public virtual int DefenseModifier { get; set; }
		public virtual int ArmorDefense
		{
			get
			{
				int sum = 0;
				if (ArmorSlots[0, 0].Item is IArmorItem defensive1)
					sum += defensive1.Defense;
				if (ArmorSlots[0, 1].Item is IArmorItem defensive2)
					sum += defensive2.Defense;
				if (ArmorSlots[0, 2].Item is IArmorItem defensive3)
					sum += defensive3.Defense;
				return sum;
			} 
		}
		public override int Defense => BaseDefense + DefenseModifier + ArmorDefense;

		public Humanoid() : base()
		{
			ArmorSlots = new Container(1, 3);
			ArmorSlots.ItemWhitelistEnabled = true;
			ArmorSlots.ItemTagWhitelist = new List<ItemTag> { ItemTag.Armor };
		}

		public Direction Facing { get; set; }
		public bool Walking { get; set; }
		public override float Mass { get; }

		public Color Color { get; set; }

		public Container ArmorSlots { get; set; }


		const float walk_anim_time = 1.25f;
		const float push_anim_time = 1.0f;

		protected virtual Texture2D Sprite { get; }


		public virtual Rectangle GetAnimationFrame() { throw new NotImplementedException(); }



		public override void Draw(GraphicsEngine gfx)
		{

			
			int flipSprite = 0;
			if (Facing == Direction.Left)
				flipSprite = 0;
			if (Facing == Direction.Right)
				flipSprite = 1;

			DrawHealth(gfx);
			//DrawName(gfx);

			gfx.Sprite(Sprite, Position, GetAnimationFrame(), Illumination.MultiplyAgainst(Color), Rotation.Zero, new Vector2(8, 12), 1, (SpriteEffects)flipSprite, 0);
		}
	}

	public class Player : Humanoid, ICanBleed, IServerPhysicsObserver, IClientPhysicsObserver
	{
		public static Rectangle SP_IDLE = new Rectangle(0, 0, 16, 24);
		public static Rectangle SP_WALK0 = new Rectangle(16, 0, 16, 24);
		public static Rectangle SP_WALK1 = new Rectangle(32, 0, 16, 24);
		public static Rectangle SP_WALK2 = new Rectangle(48, 0, 16, 24);
		public static Rectangle SP_DUCK = new Rectangle(64, 0, 16, 24);
		public static Rectangle SP_FALL = new Rectangle(80, 0, 16, 24);
		public static Rectangle SP_JUMP = new Rectangle(96, 0, 16, 24);
		public static Rectangle SP_PUSH0 = new Rectangle(144, 0, 16, 24);
		public static Rectangle SP_PUSH1 = new Rectangle(160, 0, 16, 24);

		public static Rectangle[] WALK_CYCLE =
		{
			SP_WALK0,
			SP_WALK1,
			SP_WALK2,
			SP_WALK1,
		};
		public static Rectangle[] PUSH_CYCLE =
		{
			SP_PUSH0,
			SP_PUSH1,
		};


        public override Vector2 BoundingBox => new Vector2(8, 12);
        public override float Mass => 1.5f;
		//public override float Buoyancy => 4.0f; 
		public override Vector2 Friction => new Vector2(5.0f, 1f);
		public override int MaxHealth => 100;

		public bool God { get; set; }
		public bool OnRope { get; set; }
		public User User { get; set; }
		public string DisplayName { get; set; }
		public bool Pushing { get; set; }

		public Container Inventory { get; set; }

		protected float animationTimer;
		protected float landingImpactAnim;

		public Player()
		{

			Inventory = new Container(10, 4);
			ArmorSlots = new Container(1, 3);
			ArmorSlots.ItemWhitelistEnabled = true;
			DisplayName = "Player";
			ArmorSlots.ItemTagWhitelist = new List<ItemTag> { ItemTag.Armor };
			Health = MaxHealth;
			God = false;
			Position = new Vector2(0, -200);
			Velocity = Vector2.Zero;
			OnGround = false;
			Pushing = false;
			Color = Color.White;
		}


		public override void ClientUpdate(IGameClient client, GameTime gt)
		{
			animationTimer += (float)gt.ElapsedGameTime.TotalSeconds * 5;
			base.ClientUpdate(client, gt);
		}

		public void ServerPhysicsStep(IServerWorld world, float step)=> PhysicsStep(world, step);
		public void ServerPhysicsTick(IServerWorld world, float step) => ServerPhysicsStep(world, step);

		public void DrawName(GraphicsEngine GFX)
		{
			Vector2 namebounds = GFX.Fonts.Arial8.MeasureString(DisplayName);
			GFX.Text(GFX.Fonts.Arial8, DisplayName, Position - new Vector2(0, namebounds.Y + BoundingBox.Y), Color.White, TextXAlignment.Center, TextYAlignment.Top);
		}


		const float walk_anim_time = 1.25f;
		const float push_anim_time = 1.0f;
		public override void Draw(GraphicsEngine gfx)
		{

			Rectangle spriteFrame = SP_IDLE;

			int flipSprite = 0;
			if (Facing == Direction.Left)
				flipSprite = 0;
			if (Facing == Direction.Right)
				flipSprite = 1;


			if (Walking) 
			{
				spriteFrame = WALK_CYCLE.GetSpriteFrame(animationTimer * walk_anim_time);
				if (Pushing)
					spriteFrame = SP_PUSH0;
			}
			if (!OnGround)
			{
				if (Velocity.Y > 0)
					spriteFrame = SP_JUMP;
				else
					spriteFrame = SP_FALL;
			}

			//DrawHealth(gfx);
			//DrawName(gfx);
			
			gfx.Sprite(gfx.Player, Position, spriteFrame, Illumination.MultiplyAgainst(Color), Rotation.Zero, new Vector2(8, 12), 1, (SpriteEffects)flipSprite, 0);
		}

		private Vector2 GetTopLeft()
        {
			if (Facing == Direction.Right)
				return base.TopLeft - new Vector2(4, 0);

			return base.TopLeft;

		}


        const float threshold = 0.1f;
		protected override void SolidCollisionReaction(IGameWorld world, Tile t, Vector2 separation, Vector2 normal, Point tilePos)
		{
			Pushing = false;

			var isActuallyMoving = (RecentVelocity.X > threshold) || (RecentVelocity.X < -threshold);
			var velocityPredictsMoving = (Velocity.X > threshold) || (Velocity.X < -threshold);
			if (isActuallyMoving == false && velocityPredictsMoving == true) // pushing but not moving
				Pushing = true;
			base.SolidCollisionReaction(world, t, separation, normal, tilePos);
		}

		public virtual void ClientPhysicsTick(IClientWorld world, float step) => PhysicsStep(world, step);
    }
}
