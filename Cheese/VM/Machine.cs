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
			internal Function Func;
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

		/*
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
		}*/

		public void PushFrame(int PC, Function Func, int ArgBase) {
			Reserve(Func.MaxStackSize - ArgBase);

			CurrFrame.PC = PC;
			Frames.Add(CurrFrame);

			CurrFrame.Base = CurrFrame.Base + ArgBase + 1; //CurrFrame.Top;
			CurrFrame.Func = Func;
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
	
		internal Function Func {
			get {
				return CurrFrame.Func;
			}
		}

		internal int Top {
			get {
				return (CurrFrame.Top - CurrFrame.Base);
			}
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

			Stack.PushFrame(ProgramCounter, Function, 0);
			ProgramCounter = 0;

			ExecuteMachine();

			ProgramCounter = Stack.PopFrame();
		}

		private void ExecuteMachine() {

			while(true) {
				if(ProgramCounter >= Stack.Func.Instructions.Count)
					break;

				Instruction CurrOp = Stack.Func.Instructions[ProgramCounter];
				ProgramCounter++;

				// Execute Op
				Console.WriteLine("Executing: {0}", CurrOp.ToString());

				switch(CurrOp.Code) { 

				case Instruction.OP.MOVE:
					Stack[CurrOp.A] = Stack[CurrOp.B];
					break;

				case Instruction.OP.LOADK: 
					Stack[CurrOp.A] = Stack.Func.ConstantTable[CurrOp.B].Value;
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

				// GETGLOBAL  // R(A) := Gbl[Kst(Bx)]
				case Instruction.OP.GETGLOBAL:
					Stack[CurrOp.A] = Globals[Stack.Func.ConstantTable[CurrOp.B].Value];
					break;                  
				// SETGLOBAL  // Gbl[Kst(Bx)] := R(A)
				case Instruction.OP.SETGLOBAL:
					Globals[Stack.Func.ConstantTable[CurrOp.B].Value] = Stack[CurrOp.A];
					break;


				// ADD   // R(A) := RK(B) + RK(C)
				case Instruction.OP.ADD:
				case Instruction.OP.SUB:
				case Instruction.OP.MUL:
				case Instruction.OP.DIV:
				case Instruction.OP.MOD:
				case Instruction.OP.POW: {
					LuaNumber F, S;
					F = GetRK(CurrOp.B, CurrOp.rkB) as LuaNumber;
					S = GetRK(CurrOp.C, CurrOp.rkC) as LuaNumber;
						
					double dR = default(double);
					switch(CurrOp.Code) { 
					case Instruction.OP.ADD:
						dR = (F.Number + S.Number);
						break;
					case Instruction.OP.SUB:
						dR = (F.Number - S.Number);
						break;
					case Instruction.OP.MUL:
						dR = (F.Number * S.Number);
						break;
					case Instruction.OP.DIV:
						dR = (F.Number / S.Number);
						break;
					case Instruction.OP.MOD:
						dR = (F.Number % S.Number);
						break;
					case Instruction.OP.POW:
						dR = (Math.Pow(F.Number , S.Number));
						break;
					}
					Stack[CurrOp.A] = new LuaNumber(dR);
					break;
				}

								
				// CALL (print only)
				case Instruction.OP.CALL: {
					LuaValue FuncValue = Stack[CurrOp.A];

					if(FuncValue is LuaDelegate) {
						LuaDelegate FuncDelegate = FuncValue as LuaDelegate;

						LuaTable Arguments = new LuaTable();
						
						if(CurrOp.B == 0) {
							; // Handle Var Arg case
						} else {
							for(int i = CurrOp.A+1; i < CurrOp.A+CurrOp.B; i++) {
								Arguments.Add(Stack[i]);
							}
						}

						LuaValue ReturnValue = null;
						ReturnValue = FuncDelegate.Call(Arguments);
						// Decode ReturnValue, assign return parts

						if(CurrOp.C == 1) {
							; // Do nothing
						} else if(CurrOp.C == 0) {
							; // Handle Var Returns
						} else if(CurrOp.C == 2) {
							if(ReturnValue is LuaTable) {
								LuaTable ReturnTable = ReturnValue as LuaTable;
								if(ReturnTable.Length == 1)
									ReturnValue = ReturnTable[0];
							}
							Stack[CurrOp.A] = ReturnValue;
						} else {
							LuaTable ReturnTable = ReturnValue as LuaTable;
							int ri = 0;
							for(int i = CurrOp.A; i < CurrOp.A+CurrOp.C-1; i++) {
								Stack[i] = ReturnTable[ri]; 
								ri++;
							}
						}
					} 
					else if(FuncValue is LuaClosure) {
						LuaClosure ClosureValue = FuncValue as LuaClosure;

						// Capture Args

						Stack.PushFrame(ProgramCounter, ClosureValue.Function, CurrOp.A);
						ProgramCounter = 0;
						// UpVal storage setup?

						// Trust that RETURN will handle the returns
						continue;
					}

					break;
				}
				
				// CLOSURE    R(A) := closure(KPROTO[Bx], R(A), ... ,R(A+n))
				case Instruction.OP.CLOSURE: {
					Function ClosureFunc = Stack.Func.SubFunctions[CurrOp.B];
					LuaClosure Closure = new LuaClosure(ClosureFunc);
					Stack[CurrOp.A] = Closure;

					continue;
				}

				case Instruction.OP.RETURN: {

					if(CurrOp.B == 0) {
						; // var arg
					} else if(CurrOp.B == 1) {
						; // no return
					} else if(CurrOp.B >= 2) {
						int si = -1;
						for(int ri = CurrOp.A; ri < CurrOp.A+CurrOp.B-2; ri++) {
							Stack[si] = Stack[ri]; 
							si++;
						}
					}

					ProgramCounter = Stack.PopFrame();

					break; // not just break, pop, etc
				}

				} // end switch

			} // Instruction While Loop


		}


		private LuaValue GetRK(int Index, bool RK) {
			if(RK)
				return Stack.Func.ConstantTable[Index].Value;
			else 
				return Stack[Index];
		}
	}


}

