using CaveGame.Common;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Input;

namespace CaveGame.Client.UI
{
	public interface ITextComponent
    {

    }


	public interface UIRootNode
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="gt"></param>
		void Update(GameTime gt);
		void Draw(GraphicsEngine GFX);
		List<UINode> Children { get; }

        UINode FindFirstChildWithName(string name);
        List<UINode> FindChildrenWithName(string name);

		bool IsMouseInside(MouseState ms);

		Vector2 AnchorPoint { get; set; }
		Vector2 AbsoluteSize { get; }
		Vector2 AbsolutePosition { get; }

		UICoords Position { get; set; }
		UICoords Size { get; set; }
	}
	public interface UINode : UIRootNode
	{ 
		bool MouseOver { get; }
		UINode Parent { get; }
		string Name { get; }
		bool Visible { get; }
		bool Active { get; }

	}
}
