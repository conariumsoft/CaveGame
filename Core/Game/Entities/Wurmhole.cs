#if CLIENT
using CaveGame.Client;
#endif
using CaveGame.Core.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace CaveGame.Core.Game.Entities
{
    // Würmhole
    [Summonable]
    public class Wurmhole : PhysicsEntity, IServerPhysicsObserver, IClientPhysicsObserver, IThinker, ICanBleed, IProvokable
    {
        #region Sprite Quads
        public static Rectangle SP_IDLE0 = new Rectangle(0, 0, 16, 16);
        public static Rectangle SP_IDLE1 = new Rectangle(0, 16, 16, 16);
        public static Rectangle SP_IDLE2 = new Rectangle(0, 32, 16, 16);
        public static Rectangle SP_IDLE3 = new Rectangle(0, 48, 16, 16);

        public static Rectangle SP_DEATH0 = new Rectangle(16, 0, 16, 16);
        public static Rectangle SP_DEATH1 = new Rectangle(16, 16, 16, 16);
        public static Rectangle SP_DEATH2 = new Rectangle(16, 32, 16, 16);
        public static Rectangle SP_DEATH3 = new Rectangle(16, 48, 16, 16);
        public static Rectangle SP_DEATH4 = new Rectangle(16, 64, 16, 16);
        public static Rectangle SP_DEATH5 = new Rectangle(16, 80, 16, 16);
        public static Rectangle SP_DEATH6 = new Rectangle(16, 80, 16, 16);

        // final apoptosis
        public static Rectangle SP_DEATH7 = new Rectangle(0, 120, 8, 8);
        public static Rectangle SP_DEATH8 = new Rectangle(8, 120, 8, 8);
        public static Rectangle SP_DEATH9 = new Rectangle(16,120, 8, 8);

        #endregion

        public static Rectangle[] IDLE_FRAMES =
        {
           //SP_IDLE0,
            SP_IDLE1,
            SP_IDLE2,
            SP_IDLE3,
            SP_IDLE2,
        };
        public static Rectangle[] DEATH_FRAMES =
        {
            SP_DEATH0,
            SP_DEATH1,
            SP_DEATH1,
            SP_DEATH1,
            SP_DEATH1,
            SP_DEATH2,
            SP_DEATH2,
            SP_DEATH3,
            SP_DEATH4,
            SP_DEATH5,
            SP_DEATH6,
            SP_DEATH7,
            SP_DEATH7,
            SP_DEATH8,
            SP_DEATH9,
            SP_DEATH9,
        };

        public override float Mass => 0.1f;
        public override Vector2 BoundingBox => new Vector2(4, 4);

        public override Vector2 TopLeft => Position - new Vector2(8, 8);
        public override Vector2 Friction => new Vector2(0.25f, 0.25f);

        public bool Provoked { get; private set; }

        public bool TriggerNetworkHandled { get; set; }
        public override int MaxHealth => 20;

        public float Anger { get; }
        public float Fear { get; }
        public float ResponseTime { get; }
        public int IQ { get; }
		public float ReactionTime { get; }

		float animationTimer;
        float triggeredAnimationTimer;
        float autonomousMovement;

        public Wurmhole()
        {
            animationTimer = 0;
            triggeredAnimationTimer = 0;
            TriggerNetworkHandled = false;

        }


        public override void ClientUpdate(IGameClient client, GameTime gt)
        {
            animationTimer += gt.GetDelta()*5;


            if (Provoked)
            {
                triggeredAnimationTimer += gt.GetDelta()*8;
            }
            base.ClientUpdate(client, gt);
        }

        public override void ServerUpdate(IGameServer server, GameTime gt)
        {
            if (Provoked)
            {
                triggeredAnimationTimer += gt.GetDelta()*8; // LUL
                if (triggeredAnimationTimer > DEATH_FRAMES.Length+2)
                {
                    this.Dead = true;
                }
            }
            base.ServerUpdate(server, gt);
        }

        public void ClientPhysicsTick(IClientWorld world, float step) => PhysicsStep(world, step);



        public void ServerPhysicsTick(IServerWorld world, float step)
        {
            // vortex
            foreach(var entity in world.Entities)
            {
                if (entity!=this && entity is PhysicsEntity physEntity)
                {
                    var distanceFromSingularityVector = this.Position - physEntity.Position;
                    var distance = distanceFromSingularityVector.Length();
                    var directionVector = Vector2.Normalize(distanceFromSingularityVector);
                    var rotation = Rotation.FromUnitVector(directionVector);
                    var rotated90 = rotation.RotateDeg(15);
                    var rotatedUnitVec = rotated90.ToUnitVector();
                    if (distance < 300)
                    {
                        Velocity -= rotatedUnitVec * Math.Min((300.0f - distance) / 500.0f, 0.01f);
                    }

                    if (distance < 100)
                    {
                        

                        physEntity.Velocity += rotatedUnitVec * ((100.0f - distance)/230.0f);

                        if (entity is Bomb bomb && distance < 5)
                        {
                            Provoked = true;
                            bomb.Dead = true;
                        }
                        if (entity is ItemstackEntity itemstk && distance < 3)
                        {
                            itemstk.Dead = true;
                        }

                    }
                }
            }
            PhysicsStep(world, step);
        }


        public override void Draw(GraphicsEngine gfx)
        {

            Rectangle currentQuad = SP_IDLE0;


            if (Provoked)
            {
                currentQuad = DEATH_FRAMES[Math.Min((int)triggeredAnimationTimer, DEATH_FRAMES.Length-1)];
            } else
            {
                currentQuad = IDLE_FRAMES[(int)(animationTimer % 3)];
            }

            gfx.Sprite(
			   texture: gfx.VoidMonster, 
			   position: Position, 
			   quad: currentQuad, 
			   color: Color.White, 
			   rotation: Rotation.Zero, 
			   origin: new Vector2(currentQuad.Width/2, currentQuad.Height / 2),
			   scale: Vector2.One, 
			   efx: SpriteEffects.None, 
			   layer: 0
              );
#if CLIENT
           // sb.Print(Color.White, TopLeft-new Vector2(0, 16), "Triggered:"+Triggered.ToString());
#endif
        }

        public void Think(IGameServer server, GameTime gt)
        {
            throw new NotImplementedException();
        }

        public void Provoke() => Provoked = true;
    }
}
