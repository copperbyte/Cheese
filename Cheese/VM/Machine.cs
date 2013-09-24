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
			int Needed = CurrFrame.Top + Space+1;
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
			if(Func != null)
				Reserve(Func.MaxStackSize - ArgBase + 1);

			CurrFrame.PC = PC;
			Frames.Add(CurrFrame);

			int NewBase = CurrFrame.Base + ArgBase + 1;
			//Console.WriteLine("Stack.PushFrame  Base {0} => {1} ", CurrFrame.Base, NewBase);
			CurrFrame.Base = NewBase; //CurrFrame.Top;
			CurrFrame.Func = Func;
		}

		public int PopFrame() {
			//Console.Write("Stack.PopFrame  Base {0}", CurrFrame.Base);
			if(Frames.Count == 0) {
				CurrFrame.Base = 0;
				CurrFrame.Top = 0;
				CurrFrame.Func = null;
			} else {
				CurrFrame = Frames[Frames.Count - 1];
				Frames.RemoveAt(Frames.Count - 1);
			}
			//Console.WriteLine(" => {0} ", CurrFrame.Base);

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
		private LuaEnvironment Environment;
		private LuaTable Globals;
		// Upval table?

		public Machine(LuaEnvironment Env) 
		{
			this.Environment = Env;
			this.Globals = Env.Globals;
			Stack = new VmStack();
			ProgramCounter = 0;
		}


		internal void ExecuteChunk(Chunk Chunk) {
			;//

			Stack.Clear();

			Stack.Reserve(1);
			Stack[0] = new LuaClosure(Chunk.RootFunc);
			ProgramCounter = 0;

			ExecuteFunction(Chunk.RootFunc);
		}


		internal void ExecuteFunction(Function Function, int CallDepth=1) {

			Stack.PushFrame(ProgramCounter, Function, 0);
			ProgramCounter = 0;

			ExecuteMachine(CallDepth);

			ProgramCounter = Stack.PopFrame();
		}

		private void ExecuteMachine(int CallDepth=1) {

			while(true) {
				if(Stack == null || Stack.Func == null || 
				   ProgramCounter >= Stack.Func.Instructions.Count)
					break;

				Instruction CurrOp = Stack.Func.Instructions[ProgramCounter];
				ProgramCounter++;

				// Execute Op
				//Console.WriteLine("Executing: {0}", CurrOp.ToString());

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

				// NEWTABLE
				case Instruction.OP.NEWTABLE: {
					Stack[CurrOp.A] = new LuaTable(CurrOp.B, CurrOp.C);
					continue; 
				}
				
				// SETLIST   // R(A)[(C-1)*FPF+i] := R(A+i), 1 <= i <= B
				case Instruction.OP.SETLIST: {
					LuaTable TableValue = Stack[CurrOp.A] as LuaTable;
					for(int si = CurrOp.A+1, di = CurrOp.C; si <= CurrOp.B; si++, di++) {
						TableValue[di] = Stack[si];
					}
					continue;
				}
				
				// GETTABLE   //  R(A) := R(B)[RK(C)]
				case Instruction.OP.GETTABLE: {
					LuaTable TableValue = Stack[CurrOp.B] as LuaTable;
					LuaValue KeyValue = GetRK(CurrOp.C, CurrOp.rkC);
					LuaValue ResultValue = TableValue[KeyValue];
					Stack[CurrOp.A] = ResultValue;
					continue;
				}
				// SETTABLE   //  R(A)[RK(B)] := RK(C)
				case Instruction.OP.SETTABLE: {
					LuaValue SrcValue = GetRK(CurrOp.C, CurrOp.rkC);
					LuaTable TableValue = Stack[CurrOp.A] as LuaTable;
					LuaValue KeyValue = GetRK(CurrOp.B, CurrOp.rkB);
					TableValue[KeyValue] = SrcValue;
					continue;
				}

				// ADD   // R(A) := RK(B) + RK(C)
				case Instruction.OP.ADD:
				case Instruction.OP.SUB:
				case Instruction.OP.MUL:
				case Instruction.OP.DIV:
				case Instruction.OP.MOD:
				case Instruction.OP.POW: {
					LuaValue FV, SV;
					FV = GetRK(CurrOp.B, CurrOp.rkB);
					SV = GetRK(CurrOp.C, CurrOp.rkC);

					if(FV is LuaInteger && SV is LuaInteger) {
						LuaInteger FI = FV as LuaInteger, SI = SV as LuaInteger;

						long lR = default(long);
						switch(CurrOp.Code) { 
						case Instruction.OP.ADD:
							lR = (FI.Integer + SI.Integer);
							break;
						case Instruction.OP.SUB:
							lR = (FI.Integer - SI.Integer);
							break;
						case Instruction.OP.MUL:
							lR = (FI.Integer * SI.Integer);
							break;
						case Instruction.OP.DIV:
							lR = (FI.Integer / SI.Integer);
							break;
						case Instruction.OP.MOD:
							lR = (FI.Integer % SI.Integer);
							break;
						case Instruction.OP.POW:
							lR = (long)(Math.Pow(FI.Integer, SI.Integer));
							break;
						}
						Stack[CurrOp.A] = new LuaInteger(lR);
						continue;
					} 
					else {
						LuaNumber F, S;
						F = FV as LuaNumber;
						S = FV as LuaNumber;
				
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
							dR = (Math.Pow(F.Number, S.Number));
							break;
						}
						Stack[CurrOp.A] = new LuaNumber(dR);
						continue;
					}
				}

				// UNM   // R(A) := -R(B)
				case Instruction.OP.UNM: {
					LuaValue SV;
					SV = Stack[CurrOp.B];

					if(SV is LuaInteger) {
						LuaInteger SI = SV as LuaInteger;
						Stack[CurrOp.A] = new LuaInteger(-SI.Integer);
						continue;
					} 
					else {
						LuaNumber SN = SV as LuaNumber;
						Stack[CurrOp.A] = new LuaNumber(-SN.Number);
						continue;
					}
				}
				
				// NOT   // R(A) := ~R(B)
				case Instruction.OP.UNM: {
					LuaValue SV;
					SV = Stack[CurrOp.B];

					if(SV is LuaBool) {
						LuaBool SB = SV as LuaBool;
						if(SB.Value)
							Stack[CurrOp.A] = LuaBool.False;
						else
							Stack[CurrOp.A] = LuaBool.True;
						continue;
					} else if(SV is LuaNil) {
						Stack[CurrOp.A] = LuaBool.True;
					}
					else if(SV is LuaInteger) {
						LuaInteger SI = SV as LuaInteger;
						Stack[CurrOp.A] = (SI.Integer == 0 ? LuaBool.True : LuaBool.False);
						continue;
					}
					else if(SV is LuaNumber) {
						LuaNumber SN = SV as LuaNumber;
						Stack[CurrOp.A] = (SN.Number == 0.0 ? LuaBool.True : LuaBool.False);
						continue;
					}
					continue;
				}
				
				// EQ   // if ((RK(B) == RK(C)) ~= A) then PC++
				case Instruction.OP.EQ: {
					LuaValue LVal = GetRK(CurrOp.B, CurrOp.rkB);
					LuaValue RVal = GetRK(CurrOp.C, CurrOp.rkC);

					int CompInt = (LVal.Equals(RVal) ? 1 : 0);

					if(CompInt == CurrOp.A) {
						Instruction NextJmp = Stack.Func.Instructions[ProgramCounter];
						ProgramCounter += NextJmp.B; 
					}
					ProgramCounter++;
					continue;
				}

				// LT   // if ((RK(B) < RK(C)) ~= A) then PC++
				case Instruction.OP.LT:	{
					LuaValue LVal = GetRK(CurrOp.B, CurrOp.rkB);
					LuaValue RVal = GetRK(CurrOp.C, CurrOp.rkC);

					int CompInt = 0;
					if(LVal.GetType() == typeof(LuaNumber)) {
						CompInt = ((LVal as LuaNumber).Number < (RVal as LuaNumber).Number) ? 1 : 0;
					} else if(LVal.GetType() == typeof(LuaInteger)) {
						CompInt = ((LVal as LuaInteger).Integer < (RVal as LuaInteger).Integer) ? 1 : 0;
					} else if(LVal.GetType() == typeof(LuaString)) {
						CompInt = (String.Compare((LVal as LuaString).Text, (RVal as LuaString).Text) == -1) ? 1 : 0;
					}

					if(CompInt == CurrOp.A) {
						Instruction NextJmp = Stack.Func.Instructions[ProgramCounter];
						ProgramCounter += NextJmp.B; 
					}
					ProgramCounter++;
					continue;
				}

				// LE   // if ((RK(B) <= RK(C)) ~= A) then PC++
				case Instruction.OP.LE: {
					LuaValue LVal = GetRK(CurrOp.B, CurrOp.rkB);
					LuaValue RVal = GetRK(CurrOp.C, CurrOp.rkC);

					int CompInt = 0;
					if(LVal.GetType() == typeof(LuaNumber)) {
						CompInt = ((LVal as LuaNumber).Number <= (RVal as LuaNumber).Number) ? 1 : 0;
					} else if(LVal.GetType() == typeof(LuaInteger)) {
						CompInt = ((LVal as LuaInteger).Integer < (RVal as LuaInteger).Integer) ? 1 : 0;
					} else if(LVal.GetType() == typeof(LuaString)) {
						CompInt = (String.Compare((LVal as LuaString).Text, (RVal as LuaString).Text) != 1) ? 1 : 0;
					}

					if(CompInt == CurrOp.A) {
						Instruction NextJmp = Stack.Func.Instructions[ProgramCounter];
						ProgramCounter += NextJmp.B; 
					}
					ProgramCounter++;
					continue;
				}

				// TEST  // if not (R(A) <=> C) then PC++
				case Instruction.OP.TEST: {
					LuaValue TestValue = Stack[CurrOp.A];
					byte TestBool = 1;
					if(TestValue is LuaNil || 
						(TestValue is LuaBool && ((TestValue as LuaBool) == LuaBool.False))) {
						TestBool = 0;
					}

					if(TestBool == CurrOp.C) {
						Instruction NextJmp = Stack.Func.Instructions[ProgramCounter];
						ProgramCounter += NextJmp.B; 
					}
					ProgramCounter++;
					continue;
				}

				// TESTSET  // if (R(B) <=> C) then R(A) := R(B) else PC++
				case Instruction.OP.TESTSET: {
					LuaValue TestValue = Stack[CurrOp.B];
					byte TestBool = 1;
					if(TestValue is LuaNil || 
						(TestValue is LuaBool && (TestValue as LuaBool).Value == false)) {
						TestBool = 0;
					}
					if(TestBool == CurrOp.C) {
						Stack[CurrOp.A] = TestValue;
						Instruction NextJmp = Stack.Func.Instructions[ProgramCounter];
						ProgramCounter += NextJmp.B; 
					}
					ProgramCounter++;
					continue;
				}


				// JMP  // PC += sBx 
				case Instruction.OP.JMP: {
					ProgramCounter += CurrOp.B;
					continue;
				}

				// FORLOOP   //  R(A) += R(A+2); if R(A) <?= R(A+1) then { PC += sBx; R(A+3) = R(A) }
				case Instruction.OP.FORLOOP: {
					if(Stack[CurrOp.A] is LuaNumber) {
						LuaNumber InitVal = Stack[CurrOp.A] as LuaNumber;
						LuaNumber LimitVal = Stack[CurrOp.A + 1] as LuaNumber;
						LuaNumber StepVal = Stack[CurrOp.A + 2] as LuaNumber;
						LuaNumber IndexVal = Stack[CurrOp.A + 3] as LuaNumber;
						double IndexNum = InitVal.Number + StepVal.Number;

						if(((0 < StepVal.Number) ? (IndexNum <= LimitVal.Number) 
				   						: (LimitVal.Number <= IndexNum))) {
							ProgramCounter += CurrOp.B;
							InitVal.Number = IndexNum;
							IndexVal.Number = IndexNum;
						}
					}
					else if(Stack[CurrOp.A] is LuaInteger) {
						LuaInteger InitVal = Stack[CurrOp.A] as LuaInteger;
						LuaInteger LimitVal = Stack[CurrOp.A + 1] as LuaInteger;
						LuaInteger StepVal = Stack[CurrOp.A + 2] as LuaInteger;
						LuaInteger IndexVal = Stack[CurrOp.A + 3] as LuaInteger;
						long IndexNum = InitVal.Integer + StepVal.Integer;

						if(((0 < StepVal.Integer) ? (IndexNum <= LimitVal.Integer) 
						    : (LimitVal.Integer <= IndexNum))) {
							ProgramCounter += CurrOp.B;
							InitVal.Integer = IndexNum;
							IndexVal.Integer = IndexNum;
						}
					}
					continue;
				}
				// FORPREP   //  R(A) -= R(A+2); PC += sBx
				case Instruction.OP.FORPREP: {
					if(Stack[CurrOp.A] is LuaNumber) {
						LuaNumber InitVal = Stack[CurrOp.A] as LuaNumber;
						LuaNumber LimitVal = Stack[CurrOp.A + 1] as LuaNumber;
						LuaNumber StepVal = Stack[CurrOp.A + 2] as LuaNumber;
						InitVal = new LuaNumber(InitVal.Number);
						Stack[CurrOp.A] = InitVal;
						Stack[CurrOp.A + 3] = new LuaNumber(0.0);
						InitVal.Number = InitVal.Number - StepVal.Number;
					}
					else if(Stack[CurrOp.A] is LuaInteger) {
						LuaInteger InitVal = Stack[CurrOp.A] as LuaInteger;
						LuaInteger LimitVal = Stack[CurrOp.A + 1] as LuaInteger;
						LuaInteger StepVal = Stack[CurrOp.A + 2] as LuaInteger;
						InitVal = new LuaInteger(InitVal.Integer);
						Stack[CurrOp.A] = InitVal;
						Stack[CurrOp.A + 3] = new LuaInteger(0);
						InitVal.Integer = InitVal.Integer - StepVal.Integer;
					}
					ProgramCounter += CurrOp.B;
					continue;
				}
				
				case Instruction.OP.TFORLOOP: {
					LuaValue GeneratorVal = Stack[CurrOp.A];
					LuaValue StateVal     = Stack[CurrOp.A + 1];
					LuaValue ControlVal   = Stack[CurrOp.A + 2];

					int CB = CurrOp.A + 3;
					Stack[CB] = GeneratorVal;
					Stack[CB + 1] = StateVal;						
					Stack[CB + 2] = ControlVal;

					int CapPC = ProgramCounter;

					// Make Call (diff for func or delegate)
					if(GeneratorVal is LuaDelegate) {
						LuaDelegate FuncDelegate = GeneratorVal as LuaDelegate;
						LuaTable Arguments = new LuaTable();

						Arguments.Add(StateVal);
						Arguments.Add(ControlVal);

						LuaValue ReturnValue = null;
						ReturnValue = FuncDelegate.Call(Environment, Arguments);
						// Decode ReturnValue, assign return parts

						if(ReturnValue is LuaTable) {
							LuaTable ReturnTable = ReturnValue as LuaTable;
							int ri = 1;
							for(int i = CurrOp.A+3; i <= CurrOp.A+CurrOp.C+2; i++) {
								Stack[i] = ReturnTable[ri]; 
								ri++;
							}
						} else if(ReturnValue == null || ReturnValue is LuaNil) {
							Stack[CurrOp.A + 3] = LuaNil.Nil;
						} else {
							; // FIXME hilarious bug
						}

					} 
					else if(GeneratorVal is LuaSysDelegate) {
						LuaSysDelegate FuncDelegate = GeneratorVal as LuaSysDelegate;

						// Stack is prepped for 2 Args already;

						Stack.PushFrame(ProgramCounter, null, CB);

						FuncDelegate.Call(Environment, Stack, 3, CurrOp.C+2);

						ProgramCounter = Stack.PopFrame();
						// Decode ReturnValue, assign return parts

						if(CurrOp.C == 1) {
							; // Do nothing
						} else if(CurrOp.C == 0) {
							; // Handle Var Returns
						} else if(CurrOp.C == 2) {
							; //Already on stack
						} else {
							; //Already on stack
						}
					}
					else if(GeneratorVal is LuaClosure) {
						LuaClosure ClosureValue = GeneratorVal as LuaClosure;
						Stack.PushFrame(ProgramCounter, ClosureValue.Function, CB);
						ProgramCounter = 0;
						ExecuteMachine(1);
						ProgramCounter = Stack.PopFrame();
					}

					ProgramCounter = CapPC;

					LuaValue NewControl = Stack[CB];

					if(!(NewControl is LuaNil)) {
						Stack[CurrOp.A + 2] = NewControl;
						Instruction NextJmp = Stack.Func.Instructions[ProgramCounter];
						ProgramCounter += NextJmp.B; 
					}
					ProgramCounter++;
					continue;
				}

				// CALL
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
						ReturnValue = FuncDelegate.Call(Environment, Arguments);
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
							int ri = 1;
							for(int i = CurrOp.A; i < CurrOp.A+CurrOp.C-1; i++) {
								Stack[i] = ReturnTable[ri]; 
								ri++;
							}
						}
					} 
					else if(FuncValue is LuaSysDelegate) {
						LuaSysDelegate FuncDelegate = FuncValue as LuaSysDelegate;


						if(CurrOp.B == 0) {
							; // Handle Var Arg case
						} else {
							; // args are on stack already
						}

						Stack.PushFrame(ProgramCounter, null, CurrOp.A);

						LuaValue ReturnValue = null;
						ReturnValue = FuncDelegate.Call(Environment, Stack, CurrOp.B, CurrOp.C);

						ProgramCounter = Stack.PopFrame();
						// Decode ReturnValue, assign return parts

						if(CurrOp.C == 1) {
							; // Do nothing
						} else if(CurrOp.C == 0) {
							; // Handle Var Returns
						} else if(CurrOp.C == 2) {
							; //Already on stack
						} else {
							; //Already on stack
						}
					}
					else if(FuncValue is LuaClosure) {
						LuaClosure ClosureValue = FuncValue as LuaClosure;

						// Capture Args

						Stack.PushFrame(ProgramCounter, ClosureValue.Function, CurrOp.A);
						ProgramCounter = 0;
						CallDepth++;
						// UpVal storage setup?

						// Trust that RETURN will handle the returns
						continue;
					}

					break;
				}
				
				// SELF //  R(A+1) := R(B); R(A) := R(B)[RK(C)]
				case Instruction.OP.SELF: {
					LuaTable SelfTable = Stack[CurrOp.B] as LuaTable;
					LuaValue Key = GetRK(CurrOp.C, CurrOp.rkC);
					LuaValue FuncVal = SelfTable[Key];
					Stack[CurrOp.A] = FuncVal;
					Stack[CurrOp.A + 1] = SelfTable;
					continue;
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
						for(int ri = CurrOp.A; ri <= CurrOp.A+CurrOp.B-2; ri++) {
							Stack[si] = Stack[ri]; 
							si++;
						}
					}

					CallDepth--;
					if(CallDepth == 0) // bottomed out, stop executing, not just adjust stack
						return;

					ProgramCounter = Stack.PopFrame();		
					continue; // not just break, pop, etc
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

