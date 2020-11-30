#define UI_DEBUG


using CaveGame.Core;
using CaveGame.Core.LuaInterop;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NLua;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Text;


namespace CaveGame.Client.UI
{


	public interface UIRootNode
	{
		void Update(GameTime gt);
		void Draw(GraphicsEngine gfx);
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
		string Name { get; }


	}


	public static class Poo
    {
		public static object Cast(this Type Type, object data)
		{
			var DataParam = Expression.Parameter(typeof(object), "data");
			var Body = Expression.Block(Expression.Convert(Expression.Convert(DataParam, data.GetType()), Type));

			var Run = Expression.Lambda(Body, DataParam).Compile();
			var ret = Run.DynamicInvoke(data);
			return ret;
		}
	}

	public class UIRect : UINode
	{
	
		public UIRect(Lua state, LuaTable table) : base()
		{
			Visible = true;
			Active = true;
			AnchorPoint = Vector2.Zero;
			Children = new List<UINode>();
			BorderSize = 1f;

			foreach (KeyValuePair<object, object> kvp in state.GetTableDict(table))
			{
				if (kvp.Key is string keyString)
				{
					var prop = this.GetType().GetProperty(keyString);
					if (prop != null)
                    {
						Debug.WriteLine("PropertySet {0} to {1} on {2}", keyString, kvp.Value.ToString(), this.ToString());
						prop.SetValue(this, Poo.Cast(prop.PropertyType, kvp.Value));
                    }
	
				}
			}
		}

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
				_parent.Children.Add(this);
			}
		}
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
        public string Name { get; set; }

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
		private void DrawAnchorPoint(GraphicsEngine gfx)
		{

			gfx.Circle(new Color(0, 1, 0.0f), AbsolutePosition + (AnchorPoint*AbsoluteSize), 2);
		}

		public virtual void Draw(GraphicsEngine gfx)
		{

			gfx.Rect(BGColor, AbsolutePosition, AbsoluteSize);
			gfx.OutlineRect(BorderColor, AbsolutePosition, AbsoluteSize, BorderSize);
			DrawAnchorPoint(gfx);
			foreach (UINode child in Children)
			{
				child.Draw(gfx);
			}
		}

	}


	public class UIRoot : UINode
	{
		public LuaEvent<LuaEventArgs> OnUnload = new LuaEvent<LuaEventArgs>();
		public LuaEvent<LuaEventArgs> OnLoad = new LuaEvent<LuaEventArgs>();

		public bool Visible { get; }
		public bool Active { get; }

		public List<UINode> Children { get; }
		public Vector2 AnchorPoint { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

		public Vector2 AbsoluteSize { get; private set; }

		public Vector2 AbsolutePosition=> Vector2.Zero;

		public UICoords Position { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

		public UICoords Size { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

		private UINode _parent;
		public UINode Parent
		{
			get { return _parent; }
			set
			{
				if (_parent != null)
					_parent.Children.Remove(this);

				_parent = value;
				_parent.Children.Add(this);
			}
		}

        public string Name { get; }

        public UIRoot()
		{
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

		public virtual void Draw(GraphicsEngine gfx)
		{
			AbsoluteSize = gfx.WindowSize;
			foreach (UINode child in Children)
			{
				child.Draw(gfx);
			}
		}

	}
}
