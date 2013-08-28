using System;
using System.Collections.Generic;

using Cheese;


namespace Cheese.Machine
{




	internal class VmStack {
		//List<LuaValue> Storage;
		List<Object> Storage;
		int FramePointer, TopPointer;


		internal VmStack() {
			//Storage = new List<LuaValue>();
			FramePointer = 0;
			TopPointer = 0;
		}

		public void Clear() {
			;// Storage.Clear();
			FramePointer = 0;
			//Storage.Add(new int(0));
			//FramePointer = 1;
		}

		public void Reserve(int Space) {
			int Needed = TopPointer + Space;
			if(Storage.Count >= Needed) {
				return;
			} else {
				Storage.Capacity = Needed;
				while(Storage.Count < Needed) {
					Storage.Add(null);
				}
			}
		}

		public void PushFrame(int PC, int Size) {
			Reserve(Size + 2);

			Storage[TopPointer] = PC;
			Storage[TopPointer+1] = FramePointer;

			FramePointer += 2;
			TopPointer = FramePointer + Size;
		}

		public int PopFrame() {
			int RestoredPC = (int)Storage[FramePointer - 2];
			int RestoredFramePointer = (int)Storage[FramePointer - 1];

			TopPointer = FramePointer - 1;
			FramePointer = RestoredFramePointer;
			// Not actually freeing items

			return RestoredPC;
		}
	}

	public class Machine
	{
		internal VmStack Stack;
		int ProgramCounter;
		// Globals table, from Envrionment?
		// Upval table?

		public Machine()
		{
			Stack = new VmStack();
			ProgramCounter = 0;
		}


		internal void ExecuteChunk(Chunk Chunk) {
			;//

			Stack.Clear();
			ProgramCounter = 0;

			ExecuteFunction(Chunk.RootFunc);
		}


		internal void ExecuteFunction(Function Function) {

			Stack.PushFrame(ProgramCounter, Function.MaxStackSize);
			ProgramCounter = 0;


			ProgramCounter = Stack.PopFrame();
		}

	}


}

