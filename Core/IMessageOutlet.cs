﻿using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace CaveGame.Core
{
	public interface IMessageOutlet
	{
		void Out(string message, Color color);
		void Out(string message);
	}
}
