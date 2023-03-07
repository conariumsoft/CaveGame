#define UI_DEBUG
//#define AUTOCASTING_DEBUG

using CaveGame.Common;
using CaveGame.Common.LuaInterop;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NLua;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Microsoft.Xna.Framework.Input;


namespace CaveGame.Client.UI
{
	public static class TypeCastingHax
	{
		public static void InitFromLuaPropertyTable(this object thing, Lua environment, LuaTable table)
        {
			foreach (KeyValuePair<object, object> kvp in environment.GetTableDict(table))
            {
                if (kvp.Key is string keyString)
                {
                    var prop = thing.GetType().GetProperty(keyString);
                    if (prop != null)
                    {
#if AUTOCASTING_DEBUG
						Debug.WriteLine("PropertySet {0} to {1} on {2}", keyString, kvp.Value.ToString(), thing.ToString());
#endif
						prop.SetValue(thing, Cast(prop.PropertyType, kvp.Value));
                    }
                }
            }
		}

		public static object Cast(this Type Type, object data)
		{
			var DataParam = Expression.Parameter(typeof(object), "data");
			var Body = Expression.Block(Expression.Convert(Expression.Convert(DataParam, data.GetType()), Type));

			var Run = Expression.Lambda(Body, DataParam).Compile();
			var ret = Run.DynamicInvoke(data);
			return ret;
		}
	}

	


	public class UIRoot : UINode
	{
		public bool MouseOver => false;


		public bool IsMouseInside(MouseState state) => false; // keep note this is intentionally off...

        public UINode FindFirstChildWithName(string name) => Children.First(t => t.Name == name);
        public List<UINode> FindChildrenWithName(string name) => Children.FindAll(t => t.Name == name);

		public TNode FindNode<TNode>(string name) => (TNode)Children.Where(t => (t is TNode)).First(t => t.Name == name);



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
