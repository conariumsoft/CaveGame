#define UI_DEBUG
using CaveGame.Core;
using Microsoft.Xna.Framework;
using NLua;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Input;

namespace CaveGame.Client.UI
{
	public class UIRect : UINode
	{
		public virtual bool MouseOver => IsMouseInside(Mouse.GetState());
        public UINode FindFirstChildWithName(string name) => Children.First(t => t.Name == name);
        public List<UINode> FindChildrenWithName(string name) => Children.FindAll(t => t.Name == name);



        public bool Debugging { get; set; }
		public bool ParentOverride { get; set; }

        public UIRect()
        {
			Debugging = true;
            Visible = true;
            Active = true;
            AnchorPoint = Vector2.Zero;
            Children = new List<UINode>();
            BorderSize = 1f;
        }

		public UIRect(Lua state, LuaTable table) : this() => this.InitFromLuaPropertyTable(state, table);


        public virtual bool IsMouseInside(MouseState mouse)=>
			(mouse.X > AbsolutePosition.X && mouse.Y > AbsolutePosition.Y 
            && mouse.X < (AbsolutePosition.X + AbsoluteSize.X)
            && mouse.Y < (AbsolutePosition.Y + AbsoluteSize.Y));
        

		public bool ClipsDescendants { get; set; }

		public bool Visible { get; set; }
		public bool Active { get; set; }

		private UINode _parent;
		public UINode Parent
		{
			get { return _parent; }
			set
			{
				if (_parent != null)
					_parent.Children.Remove(this);
				_parent = value;
				_parent?.Children.Add(this);
			}
		}
		public List<UINode> Children { get; set; }
		public Vector2 AnchorPoint { get; set; }

		public virtual Vector2 AbsoluteSize => new Vector2(
			Size.Pixels.X + (Parent.AbsoluteSize.X * Size.Scale.X),
			Size.Pixels.Y + (Parent.AbsoluteSize.Y * Size.Scale.Y)
		);
		
		public virtual Vector2 AbsolutePosition => new Vector2(
			Parent.AbsolutePosition.X + Position.Pixels.X + (Parent.AbsoluteSize.X * Position.Scale.X) - (AnchorPoint.X * AbsoluteSize.X),
			Parent.AbsolutePosition.Y + Position.Pixels.Y + (Parent.AbsoluteSize.Y * Position.Scale.Y) - (AnchorPoint.Y * AbsoluteSize.Y)
		);

		public UICoords Position { get; set; }
		public UICoords Size { get; set; }
		public Color BGColor { get; set; }
		public bool BorderEnabled { get; set; }
		public float BorderSize { get; set; }
		public Color BorderColor { get; set; }
		public string Name { get; set; }

		

		public virtual void Update(GameTime gt)
		{
            foreach (UINode child in Children)
			{
				child.Update(gt);
			}
		}

		[Conditional("UI_DEBUG")]
		protected void DrawAnchorPoint(GraphicsEngine gfx)
		{

			gfx.Circle(new Color(0, 0, 1.0f), AbsolutePosition , 2);
			gfx.Circle(new Color(1, 1, 0.0f), AbsolutePosition+AbsoluteSize, 2);
			gfx.Circle(new Color(0, 1, 0.0f), AbsolutePosition + (AnchorPoint * AbsoluteSize), 2);
			//gfx.Text($"abs pos{this.AbsolutePosition} size{this.AbsoluteSize}", AbsolutePosition);
			//gfx.Text($"{this.Children.Count} children", AbsolutePosition + new Vector2(0, 12));
		}

		public virtual void Draw(GraphicsEngine gfx)
		{


			gfx.Rect(BGColor, AbsolutePosition, AbsoluteSize);
			gfx.OutlineRect(BorderColor, AbsolutePosition, AbsoluteSize, BorderSize);


			Rectangle current = gfx.GraphicsDevice.ScissorRectangle;
			if (ClipsDescendants)
				gfx.GraphicsDevice.ScissorRectangle = new Rectangle(AbsolutePosition.ToPoint(), AbsoluteSize.ToPoint());

            if (Debugging)
				DrawAnchorPoint(gfx);

			foreach (UINode child in Children)
			{
				child.Draw(gfx);
			}

			if (ClipsDescendants)
				gfx.GraphicsDevice.ScissorRectangle = current;
		}

	}
}
