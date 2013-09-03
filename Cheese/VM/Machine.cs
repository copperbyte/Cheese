using System;
using System.Collections.Generic;

using Cheese;


namespace Cheese.Machine
{




	internal class VmStack {
		//List<LuaValue> Storage;
		List<LuaValue> Registers;
		struct Frame {
			internal int PC;
			internal int Base;
			internal int Top;
		}
		Frame CurrFrame;
		List<Frame> Frames;

		internal VmStack() {
			Registers = new List<LuaValue>();
			Frames = new List<Frame>();
			CurrFrame.Base = 0;
			CurrFrame.Top = 0;
		}

		public LuaValue this[int i]
		{
			get { return Registers[CurrFrame.Base+i]; }
			set { Registers[CurrFrame.Base+i] = value; 
				CurrFrame.Top = (CurrFrame.Base+i > CurrFrame.Top) ? (CurrFrame.Base+i):CurrFrame.Top;
			}
		}

		public void Clear() {
			;// Storage.Clear();
			CurrFrame.Base = 0;
			CurrFrame.Top = 0;
			//Storage.Add(new int(0));
			//FramePointer = 1;
		}

		public void Reserve(int Space) {
			int Needed = CurrFrame.Top + Space;
			if(Registers.Count >= Needed) {
				return;
			} else {
				Registers.Capacity = Needed;
				while(Registers.Count < Needed) {
					Registers.Add(/*null*/LuaNil.Nil);
				}
			}
		}

		public void PushFrame(int PC, int Size) {
			Reserve(Size + 2);

			CurrFrame.PC = PC;
			Frames.Add(CurrFrame);

			//Storage[TopPointer] = PC;
			//Storage[TopPointer+1] = FramePointer;

			CurrFrame.Base = CurrFrame.Top;

			//FramePointer += 2;
			//TopPointer = FramePointer + Size;
			// Track Top Pointer more exactly?
		}

		public int PopFrame() {
			CurrFrame = Frames[Frames.Count - 1];
			Frames.RemoveAt(Frames.Count - 1);

			//int RestoredPC = (int)Storage[FramePointer - 2];
			//int RestoredFramePointer = (int)Storage[FramePointer - 1];

			//TopPointer = FramePointer - 1;
			//FramePointer = RestoredFramePointer;
			// Not actually freeing items

			return CurrFrame.PC;
		}
	
	
		
	}

	public class Machine
	{
		internal VmStack Stack;
		int ProgramCounter;
		// Globals table, from Envrionment?
		private LuaTable Globals;
		// Upval table?

		public Machine(LuaTable Globals) 
		{
			this.Globals = Globals;
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

			while(true) {
				Instruction CurrOp = Function.Instructions[ProgramCounter];
				ProgramCounter++;

				// Execute Op
				Console.WriteLine("Executing: {0}", CurrOp.ToString());

				switch(CurrOp.Code) { 

				case Instruction.OP.MOVE:
					Stack[CurrOp.A] = Stack[CurrOp.B];
					break;

				case Instruction.OP.LOADK: 
					Stack[CurrOp.A] = Function.ConstantTable[CurrOp.B].Value;
					break;
				
				case Instruction.OP.LOADBOOL:
					Stack[CurrOp.A] = (CurrOp.B != 0 ? LuaBool.True : LuaBool.False);
					if(CurrOp.C != 0)
						ProgramCounter++;
					break;

				case Instruction.OP.LOADNIL:
					for(int i = CurrOp.A; i <= CurrOp.B; i++) {
						Stack[i] = LuaNil.Nil;
					}
					break;
				// ADD
				// SUB
				// GETGLOBAL  // R(A) := Gbl[Kst(Bx)]
				case Instruction.OP.GETGLOBAL:
					Stack[CurrOp.A] = Globals[Function.ConstantTable[CurrOp.B].Value];
					break;                  
				// SETGLOBAL  // Gbl[Kst(Bx)] := R(A)
				case Instruction.OP.SETGLOBAL:
					Globals[Function.ConstantTable[CurrOp.B].Value] = Stack[CurrOp.A];
					break;
				// CALL (print only)
				
				case Instruction.OP.RETURN:
					break;
				}

				if(CurrOp.Code == Instruction.OP.RETURN)
					break;
			}

			ProgramCounter = Stack.PopFrame();
		}

	}


}

