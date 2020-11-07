using System;
using System.Collections.Generic;
using System.Text;

namespace CaveGame.Core
{
	public interface ISingleton<TSingleton>
	{
		TSingleton Instance { get; }
	}
}
