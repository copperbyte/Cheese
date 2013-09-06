using System;


namespace Cheese.Machine
{

	internal static class SystemFunctions {

		internal static LuaValue print(LuaTable Arguments) {

			bool First = true;
			foreach(LuaValue Curr in Arguments.EnumerableArray) {
				if(!First)
					Console.Write("\t");
				Console.Write(Curr.ToString());
				First = false;
			}

			Console.WriteLine();

			return LuaNil.Nil;
		}

	}

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

			InitSystemFunctions();
		}


		public void ExecuteChunk(Chunk Chunk) {

			if(Machine == null)
				Machine = new Machine(Globals);		

			Machine.ExecuteChunk(Chunk);
		}


		////
		private void InitSystemFunctions() {

			Globals[new LuaString("print")] = new LuaDelegate(SystemFunctions.print);

		}
	}
}

