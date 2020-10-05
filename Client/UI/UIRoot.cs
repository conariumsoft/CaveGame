#define UI_DEBUG


using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;


namespace CaveGame.Client.UI
{


	public interface UIRootNode
	{
		void Update(GameTime gt);
		void Draw(SpriteBatch sb);
		List<UINode> Children { get; }
		bool Visible { get; }
		bool Active { get; }

		Vector2 AnchorPoint { get; set; }
		Vector2 AbsoluteSize { get;  }
		Vector2 AbsolutePosition { get; }
		
		UICoords Position { get; set; }
		UICoords Size { get; set; }
	}
	public interface UINode : UIRootNode
	{
		
		UINode Parent { get; }

	}

	public class UIRect : UINode
	{
		public bool Visible { get; set; }
		public bool Active { get; set; }
		public UINode Parent { get; set; }
		public List<UINode> Children { get; set; }
		public Vector2 AnchorPoint { get; set; }
		public Vector2 AbsoluteSize
		{
			get
			{
				var xSize = Size.Pixels.X + (Parent.AbsoluteSize.X * Size.Scale.X);
				var ySize = Size.Pixels.Y + (Parent.AbsoluteSize.Y * Size.Scale.Y);

				return new Vector2(xSize, ySize);
			}
		}
		public Vector2 AbsolutePosition
		{
			get
			{
				var xPos = Parent.AbsolutePosition.X + Position.Pixels.X + (Parent.AbsoluteSize.X * Position.Scale.X) - (AnchorPoint.X * AbsoluteSize.X);
				var yPos = Parent.AbsolutePosition.Y + Position.Pixels.Y + (Parent.AbsoluteSize.Y * Position.Scale.Y) - (AnchorPoint.Y * AbsoluteSize.Y);

				return new Vector2(xPos, yPos);
			}
		}
		public UICoords Position { get; set; }
		public UICoords Size { get; set; }
		public Color BGColor { get; set; }
		public bool BorderEnabled { get; set; }
		public float BorderSize { get; set; }
		public Color BorderColor { get; set; }


		public UIRect()
		{
			Visible = true;
			Active = true;
			AnchorPoint = Vector2.Zero;
			Children = new List<UINode>();
			BorderSize = 1f;
		}

		public virtual void Update(GameTime gt)
		{

			foreach (UINode child in Children)
			{
				child.Update(gt);
			}
		}

		//[Conditional("UI_DEBUG")]
		private void DrawAnchorPoint(SpriteBatch sb)
		{

			sb.Circle(new Color(0, 1, 0.0f), AbsolutePosition + (AnchorPoint*AbsoluteSize), 2);
		}

		public virtual void Draw(SpriteBatch sb)
		{

			sb.Rect(BGColor, AbsolutePosition, AbsoluteSize);
			sb.OutlineRect(BorderColor, AbsolutePosition, AbsoluteSize, BorderSize);
			DrawAnchorPoint(sb);
			foreach (UINode child in Children)
			{
				child.Draw(sb);
			}
		}

	}


	public class UIRoot : UINode
	{

		GraphicsDevice device;

		public bool Visible { get; }
		public bool Active { get; }

		public List<UINode> Children { get; }
		public Vector2 AnchorPoint { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

		public Vector2 AbsoluteSize { 
			get {
				return new Vector2(device.Viewport.Width, device.Viewport.Height);
			} 
		}

		public Vector2 AbsolutePosition
		{
			get
			{
				return Vector2.Zero;
			}
		}

		public UICoords Position { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

		public UICoords Size { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

		public UINode Parent => throw new NotImplementedException();

		public UIRoot(GraphicsDevice device)
		{
			this.device = device;

			Children = new List<UINode>();
			Visible = true;
			Active = true;
		}


		public virtual void Update(GameTime gt)
		{
			


			foreach (UINode child in Children)
			{
				child.Update(gt);
			}
		}

		public virtual void Draw(SpriteBatch sb)
		{
			foreach (UINode child in Children)
			{
				child.Draw(sb);
			}
		}

	}
}
