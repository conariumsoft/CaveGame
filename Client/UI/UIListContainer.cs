using CaveGame.Client.UI;
using CaveGame.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace CaveGame.Client.UI
{
	public class UIListContainer : UINode
	{
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

		public bool Visible { get; set; }

		public bool Active { get; set; }

		public Vector2 AnchorPoint { get; set; }

		public Vector2 AbsoluteSize => Parent.AbsoluteSize;

		public Vector2 AbsolutePosition => Parent.AbsolutePosition;

		public UICoords Position { get => Parent.Position; set { } }

		public UICoords Size { get => Parent.Size; set { }}
		public int Padding { get; set; }

		public bool ExpandSelected { get; set; }
		public float ExpandedHeight { get; set; }
		public float CompressedHeight { get; set; }
        public string Name { get; set; }

        public UIListContainer()
		{

			Visible = true;
			Active = true;
			AnchorPoint = Vector2.Zero;
			Children = new List<UINode>();
			Padding = 1;
			ExpandSelected = false;
		}

		public void Draw(GraphicsEngine GFX)
		{
			foreach (UINode child in Children)
			{
				child.Draw(GFX);
			}
		}

		public void Update(GameTime gt)
		{

			double delta = gt.ElapsedGameTime.TotalSeconds;

			double alphaCapped = Math.Clamp(delta * 100.0, 1 / 1000.0, 0.5);

			int ypos = 0;
			foreach (UINode child in Children)
			{

				child.Update(gt);

				if (child is Label label)
				{
					child.Position = new UICoords(0, ypos, 0, 0);

					ypos += (int)child.AbsoluteSize.Y*(label.TextWrappingCount+1);
					ypos += Padding;
				} else
				{
					child.Position = new UICoords(0, ypos, 0, 0);

					ypos += (int)child.AbsoluteSize.Y;
					ypos += Padding;
				}

				/*
				 * if (child is TextButton text)
				{
					if (ExpandSelected == true)
					{
						
						if (text.Selected)
							child.Size = child.Size.Lerp(new UICoords(0, ExpandedHeight, 1.0f, 0), (float)alphaCapped);
						else
							child.Size = child.Size.Lerp(new UICoords(0, CompressedHeight, 1.0f, 0), (float)alphaCapped);
					}
				}
				*/


				
			}
		}
	}
}
