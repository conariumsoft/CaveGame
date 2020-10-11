using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace CaveGame.Core.Generic
{
	public class QuadTree<T>
	{
		const int QUADTREE_NODE_CAPACITY = 4;

		Rectangle boundary;

		QuadTree<T> northWest;
		QuadTree<T> southWest;
		QuadTree<T> southEast;
		QuadTree<T> northEast;

		public QuadTree()
		{

		}

	}
}
