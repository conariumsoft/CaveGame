using CaveGame.Core;
using DataManagement;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using NLua;
using System;
using System.Collections.Generic;
using System.Text;

namespace CaveGame.Client.UI
{
    public class ScrollRect : UIRect
    {

        public float CanvasSize { get; set; }

		float canvasPos;
		float goalCanvasPos;
		public float CanvasPosition
		{
			get => canvasPos;
			set => goalCanvasPos = value;
		}

		public Vector2 RealAbsoluteSize
		{
			get
			{
				var xSize = Size.Pixels.X + (Parent.AbsoluteSize.X * Size.Scale.X);
				var ySize = (Size.Pixels.Y + (Parent.AbsoluteSize.Y * Size.Scale.Y));

				return new Vector2(xSize, ySize);
			}
		}
		public new Vector2 AbsoluteSize
		{
			get
			{
				var xSize = (Size.Pixels.X + (Parent.AbsoluteSize.X * Size.Scale.X)) - ScrollbarWidth;
				var ySize = (Size.Pixels.Y + (Parent.AbsoluteSize.Y * Size.Scale.Y)) * this.CanvasSize;

				return new Vector2(xSize, ySize);
			}
		}

		public Vector2 RealAbsolutePosition
		{
			get
			{
				var xPos = Parent.AbsolutePosition.X + Position.Pixels.X + (Parent.AbsoluteSize.X * Position.Scale.X) - (AnchorPoint.X * AbsoluteSize.X);
				var yPos = Parent.AbsolutePosition.Y + Position.Pixels.Y + (Parent.AbsoluteSize.Y * Position.Scale.Y) - (AnchorPoint.Y * AbsoluteSize.Y);

				return new Vector2(xPos, yPos);
			}
		}
		public override Vector2 AbsolutePosition
		{
			get
			{
				var xPos = Parent.AbsolutePosition.X + Position.Pixels.X + (Parent.AbsoluteSize.X * Position.Scale.X) - (AnchorPoint.X * AbsoluteSize.X);
				var yPos = (Parent.AbsolutePosition.Y + Position.Pixels.Y + (Parent.AbsoluteSize.Y * Position.Scale.Y) - (AnchorPoint.Y * AbsoluteSize.Y)) - ( (CanvasPosition/ CanvasSize) * AbsoluteSize.Y);

				return new Vector2(xPos, yPos);
			}
		}

		public Color ScrollbarColor { get; set; }

		public int ScrollbarWidth { get; set; }

		public UIRect Scrollbar { get; private set; }
		public TextButton Scrubber { get; private set; }
		public UIRect Content { get; private set; }

		private int initialScrollValue;

        public ScrollRect(Lua state, LuaTable table) : this() {

            this.InitFromLuaPropertyTable(state, table);
		}

        public ScrollRect() : base() {
			CreateSubcomponents();
		}

		
		private void CreateSubcomponents()
        {
	

			MouseState mouse = Mouse.GetState();
			initialScrollValue = mouse.ScrollWheelValue;

			Scrollbar = new UIRect()
			{
				Size = new UICoords(ScrollbarWidth, 0, 0, 1),
				AnchorPoint = new Vector2(0, 0),
				Position = new UICoords(-16, 0, 1, 0),
				BGColor = new Color(0.5f, 0.5f, 0.5f),
				Parent = this,
			};

			Scrubber = new TextButton()
			{
				Size = new UICoords(0, 0, 1, 1),
				Position = new UICoords(0,0,0,0),
				Parent = Scrollbar,
				Text = "",
				BGColor = new Color(0.25f, 25f, 1.0f),
				UnselectedBGColor = new Color(0.75f, 0.75f, 0.75f),
				SelectedBGColor = new Color(1f, 1f, 1f),
				Font = GraphicsEngine.Instance.Fonts.Arial10,
			};

			Content = new UIRect()
			{
				Size = new UICoords(0, 0, 1, 1),
			};
        }
		protected bool IsMouseInside(MouseState mouse)
		{
			return (mouse.X > RealAbsolutePosition.X && mouse.Y > RealAbsolutePosition.Y
				&& mouse.X < (RealAbsolutePosition.X + RealAbsoluteSize.X)
				&& mouse.Y < (RealAbsolutePosition.Y + RealAbsoluteSize.Y));
		}

		float lastFrameScrollValue;

		public override void Update(GameTime gt)
        {

			canvasPos = canvasPos.Lerp(goalCanvasPos, gt.GetDelta()*20.0f);

			MouseState mouse = Mouse.GetState();
			initialScrollValue = mouse.ScrollWheelValue;

			float scrubberFrac = 1 / this.CanvasSize;
			float posFrac = this.CanvasPosition/ this.CanvasSize;
			Scrollbar.Position = new UICoords(-ScrollbarWidth, ( this.RealAbsolutePosition.Y -this.AbsolutePosition.Y ), 1, 0);
			Scrubber.Size = new UICoords(0, 100, 1, 0);
			Scrubber.Position = new UICoords(0, 0, 0, posFrac);


			if (IsMouseInside(mouse))
            {
				if (mouse.ScrollWheelValue != lastFrameScrollValue)
                {
					float diff = (mouse.ScrollWheelValue - lastFrameScrollValue) / 500.0f;
					//CanvasPosition += diff;
					var dd = CanvasPosition - diff;
					CanvasPosition = Math.Clamp(dd, 0, CanvasSize);

					lastFrameScrollValue = mouse.ScrollWheelValue;
                }
            }

			
			
			

			foreach (UINode child in Children)
			{
				child.Update(gt);
			}
			Scrollbar.Update(gt);
			Scrubber.Update(gt);
		}

        public override void Draw(GraphicsEngine gfx)
		{
			Rectangle current = gfx.GraphicsDevice.ScissorRectangle;
			gfx.GraphicsDevice.ScissorRectangle = new Rectangle(RealAbsolutePosition.ToPoint(), RealAbsoluteSize.ToPoint());
			gfx.Rect(BGColor, RealAbsolutePosition, RealAbsoluteSize);
			gfx.OutlineRect(BorderColor, RealAbsolutePosition, RealAbsoluteSize, BorderSize);
			DrawAnchorPoint(gfx);
			gfx.Text($"canvas pos {CanvasPosition} size {CanvasSize}", RealAbsolutePosition);
            foreach (UINode child in Children)
            {
                child.Draw(gfx);
            }
            gfx.GraphicsDevice.ScissorRectangle = current;
			Scrollbar.Draw(gfx);
			Scrubber.Draw(gfx);

			
			
		}

	}
}
