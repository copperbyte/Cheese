using System;
using System.Collections.Generic;

using Cheese;


namespace Cheese.Machine
{




	internal class VmStack {
		//List<LuaValue> Storage;
		int FramePointer;


		internal VmStack() {
			//Storage = new List<LuaValue>();
			FramePointer = 0;
		}
	}

	public class Machine
	{
		// Stack?
		// Globals table, from Envrionment?
		// Upval table?

		public Machine()
		{

		}


		internal void ExecuteChunk(Chunk Chunk) {
			;//
		}

	}


}

