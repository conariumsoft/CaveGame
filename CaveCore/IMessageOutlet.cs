using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace CaveGame.Core
{
	public interface IMessageOutlet
	{
		public void Out(string message, Color color);

		public void Out(string message);
	}
}
