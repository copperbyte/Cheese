using System;


namespace Cheese.Machine
{
	public class Environment
	{
		internal Machine Machine;

		// Globals?
		// Has to be a LuaTable so it can be used in Lua code by "_G"
		internal LuaTable Globals;

		// Upvals?


		public Environment()
		{
			Globals = new LuaTable();
			Machine = new Machine(Globals);
		}


		public void ExecuteChunk(Chunk Chunk) {

			Machine.ExecuteChunk(Chunk);
		}

	}
}

