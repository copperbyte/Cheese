using System;


namespace Cheese.Machine
{
	public class Environment
	{
		internal Machine Machine;

		// Globals?
		// Upvals?


		public Environment()
		{
			Machine = new Machine();
		}


		public void ExecuteChunk(Chunk Chunk) {

			Machine.ExecuteChunk(Chunk);
		}

	}
}

