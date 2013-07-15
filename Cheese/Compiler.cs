
using System;
//using System.IO;
//using System.Text;
using System.Collections.Generic;

namespace Cheese
{
	using VList = List<Value>;

	internal static class Tools {
		public static void Add(this List<Instruction> InstList, Instruction.OP code) {
			InstList.Add(new Instruction(code));	}
		public static void Add(this List<Instruction> InstList, Instruction.OP code, int a) {
			InstList.Add(new Instruction(code, a));	}
		public static void Add(this List<Instruction> InstList, Instruction.OP code, int a, int b) {
			InstList.Add(new Instruction(code, a, b));	}
		public static void Add(this List<Instruction> InstList, Instruction.OP code, int a, int b, int c) {
			InstList.Add(new Instruction(code, a, b, c));	}
		public static void Add(this List<Instruction> InstList, Instruction.OP code, int a, int b, bool bK, int c) {
			InstList.Add(new Instruction(code, a, b, bK, c));	}
		public static void Add(this List<Instruction> InstList, Instruction.OP code, int a, int b, int c, bool cK) {
			InstList.Add(new Instruction(code, a, b, c, cK));	}
		public static void Add(this List<Instruction> InstList, Instruction.OP code, int a, int b, bool bK, int c, bool cK) {
			InstList.Add(new Instruction(code, a, b, bK, c, cK));	}
	}

	class Instruction {
		public enum OP {
			ERROR,
			MOVE,
			LOADK,
			LOADBOOL,
			LOADNIL,
			GETUPVAL,
			GETGLOBAL,
			GETTABLE,
			SETGLOBAL,
			SETUPVAL,
			SETTABLE,
			NEWTABLE,
			SELF,
			ADD,
			SUB,
			MUL,
			DIV,
			MOD,
			POW,
			UNM,
			NOT,
			LEN,
			CONCAT,
			JMP,
			EQ,
			LT,
			LE,
			TEST,
			TESTSET,
			CALL,
			TAILCALL,
			RETURN,
			FORLOOP,
			FORPREP,
			TFORLOOP,
			SETLIST,
			CLOSE,
			CLOSURE,
			VARARG
		}

		internal OP Code;
		internal int A, B, C;
		internal bool isA = false, isB=false, isC=false;
		internal bool rkB = false, rkC = false;

		public Instruction() { ; }
		public Instruction(OP code) { Code = code; }
		public Instruction(OP code, int a) { Code = code; A = a; isA = true; }
		public Instruction(OP code, int a, int b) { Code = code; A = a; B = b; isA = isB = true;}
		public Instruction(OP code, int a, int b, int c) { Code = code; A = a; B = b; C = c; isA = isB = isC = true; }
		public Instruction(OP code, int a, int b, bool bK, int c) { Code = code; A = a; B = b; C = c; isA = isB = isC = true; rkB = bK;}
		public Instruction(OP code, int a, int b, int c, bool cK) { Code = code; A = a; B = b; C = c; isA = isB = isC = true; rkC = cK;}
		public Instruction(OP code, int a, int b, bool bK, int c, bool cK) { Code = code; A = a; B = b; C = c; isA = isB = isC = true; rkB = bK; rkC = cK;}


	}

	class ConstEntry {
		internal Value Value = new Value();
		internal double NumberVal;
		internal string StringVal;
	}

	class LocalEntry {
		internal Value Value = new Value();
		internal string Name;
	}

	class Value {
		public enum ESide {
			LEFT,
			RIGHT
		}
		public enum ELoc {
			LOCAL,  // locals are a register, but don't release it 
			GLOBAL,
			UPVAL,
			TABLE,
			CONSTANT,
			REGISTER
		}
		internal ESide Side;
		internal ELoc Loc;
		internal int Index;
		internal bool Consecutive;
		internal Value Key;

		internal bool IsTable { 
			get { return (Key != null); }
		}
		internal bool IsRegister { 
			get { return (Loc == ELoc.LOCAL || Loc == ELoc.REGISTER); }
		}
		internal bool IsGlobal { 
			get { return Loc == ELoc.GLOBAL; }
		}
		internal bool IsConstant { 
			get { return Loc == ELoc.CONSTANT; }
		}

		public Value(int Index=0, ELoc Loc=ELoc.REGISTER, ESide Side=ESide.RIGHT, Value Key=null) {
			this.Index = Index; 
			this.Loc = Loc;
			this.Side = Side;
			this.Consecutive = false;
			this.Key = null;
		}

		public Value(Value Other) {
			this.Index = Other.Index;
			this.Loc = Other.Loc;
			this.Side = Other.Side;
			this.Consecutive = Other.Consecutive;
			this.Key = Other.Key;
		}

		public override string ToString()
		{
			string Ret = /*Side + ":" +*/ Loc + ":" + (Consecutive?"C":"") + Index.ToString();
			if(Key != null)
				Ret += "[" + Key.ToString() + "]";
			return Ret;
		}
	}


	class Function {


		internal int NumParams;
		internal bool IsVarArg;
		internal int MaxStackSize;

		internal SortedSet<int> UsedRegs;
		internal List<ConstEntry> ConstantTable;
		internal List<LocalEntry> LocalTable;
		internal List<Instruction> Instructions;
	
		public Function() {
			NumParams = 0;
			IsVarArg = false;
			MaxStackSize = 0;

			UsedRegs = new SortedSet<int>();
			ConstantTable = new List<ConstEntry>();
			LocalTable = new List<LocalEntry>();
			Instructions = new List<Instruction>();
		}

		internal void Print() {
			PrintHeader();
			PrintConstants();
			PrintLocals();
			PrintInstructions();
		}

		internal void PrintHeader() {
			Console.WriteLine("NumParams: {0}", NumParams);
			Console.WriteLine("IsVarArg: {0}", IsVarArg);
			Console.WriteLine("MaxStackSize: {0}", MaxStackSize);
		}

		internal void PrintConstants() {
			foreach(ConstEntry Const in ConstantTable) {
				if(Const.StringVal != null)
					Console.WriteLine("CS  {0} := {1}", Const.Value.Index, Const.StringVal);
				else
					Console.WriteLine("CN  {0} := {1}", Const.Value.Index, Const.NumberVal);
			}
		}

		internal void PrintLocals() {
			foreach(LocalEntry Local in LocalTable) {
				Console.WriteLine("LV  {0} := {1}", Local.Value.Index, Local.Name);
			}
		}

		internal void PrintInstructions() {
			foreach(Instruction Inst in Instructions) {
				Console.WriteLine("IC  {0}\t{1}  {2}  {3}", Inst.Code, 
				                  (Inst.isA?Inst.A.ToString():""), 
				                  (Inst.isB?(Inst.rkB?(Inst.B+256):Inst.B).ToString():""), 
				                  (Inst.isC?(Inst.rkC?(Inst.C+256):Inst.C).ToString():""));
			}
		}
	}


	class Compiler
	{ 

		Function CurrFunc;
		List<Function> Functions;
		Stack<Function> FunctionStack;

		List<Value> Globals;


		public Compiler()
		{
			Functions = new List<Function>();
			FunctionStack = new Stack<Function>();
			Globals = new List<Value>();
		}


		public void Compile(ParseNode RootChunk) 
		{
			if(RootChunk.Children == null)
				return;

			Function RootFunc = new Function();

			// Curr and Stack.Top should be synonymous
			FunctionStack.Push(RootFunc);
			CurrFunc = RootFunc;

			foreach(ParseNode Child in RootChunk.Children) {
				if(Child.Type == ParseNode.EType.BLOCK)
					CompileBlock(Child);
			}

			// all functions, even chunk root, end with a return
			RootFunc.Instructions.Add(Instruction.OP.RETURN, 0, 1);

			Functions.Insert(0, RootFunc);

			FunctionStack.Pop();

			foreach(Function Func in Functions) {
				Console.WriteLine("Function:  ");
				Func.Print();
			}
		}




		int GetFreeRegister() {
			int Result = 0;
			while(CurrFunc.UsedRegs.Contains(Result)) {
				Result++;
			}
			CurrFunc.UsedRegs.Add(Result);
			CurrFunc.MaxStackSize = Math.Max(CurrFunc.MaxStackSize, Result+1);
			return Result;
		}


		void FreeRegister(Value Reg) {
			if(Reg.Loc == Value.ELoc.REGISTER)
				CurrFunc.UsedRegs.Remove(Reg.Index);
		}

		void FreeRegister(int Reg) {
			CurrFunc.UsedRegs.Remove(Reg);
		}

		bool IsConsecutiveRegisterFree(int First, int Count) {
			for(int I = 0; I < Count; I++) {
				int CR = First + I;
				if(CurrFunc.UsedRegs.Contains(CR))
					return false;
			}
			return true;
		}

		// First is first tried, not promised first
		// Actually register them as used? Or do like 'local'?
		VList GetConsecutiveRegisters(int First, int Count) {
			while(true) {
				if(IsConsecutiveRegisterFree(First, Count)) {
					break;
				}
				First++;
			}
			VList Result = new VList();
			for(int I = 0; I < Count; I++) {
				int CR = First+I;
				CurrFunc.UsedRegs.Add(CR);
				CurrFunc.MaxStackSize = Math.Max(CurrFunc.MaxStackSize, CR+1);
				Result.Add(new Value(CR, Value.ELoc.REGISTER, Value.ESide.RIGHT));
				Result[Result.Count - 1].Consecutive = true;
			}
			return Result;
		}

		// MOdified GetConsecutiveRegisters 
		// Only the function reg is reserved, the others are identified,
		// but not claimed.
		// The call point must claim them once the args are evaluated,
		VList GetStackFrame(int First, int Count) {
			while(true) {
				if(IsConsecutiveRegisterFree(First, Count)) {
					break;
				}
				First++;
			}
			VList Result = new VList();
			for(int I = 0; I < Count; I++) {
				int CR = First+I;
				if(I == 0) {
					CurrFunc.UsedRegs.Add(CR);
					CurrFunc.MaxStackSize = Math.Max(CurrFunc.MaxStackSize, CR+1);
				}
				Result.Add(new Value(CR, Value.ELoc.REGISTER, Value.ESide.RIGHT));
				Result[Result.Count - 1].Consecutive = true;
			}
			return Result;
		}

		void ClaimStackFrame(VList Frame) {
			foreach(Value Reg in Frame) {
				CurrFunc.UsedRegs.Add(Reg.Index);
				CurrFunc.MaxStackSize = Math.Max(CurrFunc.MaxStackSize, Reg.Index+1);
			}
		}

		Value GetConstIndex(string ConstValue) {
			foreach(ConstEntry Entry in CurrFunc.ConstantTable) {
				if(Entry.StringVal != null && Entry.StringVal == ConstValue) {
					return Entry.Value;
				}
			}
			ConstEntry NewEntry = new ConstEntry();
			NewEntry.Value.Index = CurrFunc.ConstantTable.Count;
			NewEntry.Value.Loc = Cheese.Value.ELoc.CONSTANT;
			NewEntry.StringVal = ConstValue;
			CurrFunc.ConstantTable.Add(NewEntry);
			return NewEntry.Value;
		}

		Value GetConstIndex(double ConstValue) {
			foreach(ConstEntry Entry in CurrFunc.ConstantTable) {
				if(Entry.StringVal == null && Entry.NumberVal == ConstValue) 
					return Entry.Value;
			}
			ConstEntry NewEntry = new ConstEntry();
			NewEntry.Value.Index = CurrFunc.ConstantTable.Count;
			NewEntry.Value.Loc = Cheese.Value.ELoc.CONSTANT;
			NewEntry.NumberVal = ConstValue;
			CurrFunc.ConstantTable.Add(NewEntry);
			return NewEntry.Value;
		}

		bool CheckGlobal(string Name) {
			foreach(Value Iter in Globals) {
				if (Iter.Index < CurrFunc.ConstantTable.Count &&
					Name == CurrFunc.ConstantTable[Iter.Index].StringVal) {
					return true;
				}
			}
			return false;
		}

		Value GetGlobalIndex(string GlobalName) {
			foreach(Value Iter in Globals) {
				if (Iter.Index < CurrFunc.ConstantTable.Count &&
				    GlobalName == CurrFunc.ConstantTable[Iter.Index].StringVal) {
					return Iter;
				}
			}
			Value ConstV = GetConstIndex(GlobalName);
			Value GlobalV = new Value();
			GlobalV.Index = ConstV.Index;
			GlobalV.Loc = Value.ELoc.GLOBAL;
			Globals.Add(GlobalV);
			return GlobalV;
		}


		Value GetLocalIndex(string Name, bool Create=true) {
			foreach(LocalEntry Entry in CurrFunc.LocalTable) {
				if(Entry.Name != null && Entry.Name == Name) {
					return Entry.Value;
				}
			}
			if(Create) {
				//int FreeReg = GetFreeRegister();
				LocalEntry NewEntry = new LocalEntry();
				NewEntry.Value.Index = CurrFunc.LocalTable.Count;
				NewEntry.Value.Loc = Value.ELoc.LOCAL;
				NewEntry.Name = Name;
				CurrFunc.LocalTable.Add(NewEntry);
				return NewEntry.Value;
			} else {
				return null;
			}
		}

		void FinalizeLocal(Value Local) {
			CurrFunc.UsedRegs.Add(Local.Index);
			CurrFunc.MaxStackSize = Math.Max(CurrFunc.MaxStackSize, Local.Index+1);
		}


		Value GetLReg(VList LVals = null, VList RVals = null) {
			Value Result = null;
			if(LVals != null && RVals != null && LVals.Count > RVals.Count) {
				if (LVals[RVals.Count].IsRegister && !LVals[RVals.Count].IsTable) {
					Result = LVals[RVals.Count];
				}
			}
			return Result;
		}

		Value GetTReg() {
			return new Value(GetFreeRegister(), Value.ELoc.REGISTER, Value.ESide.RIGHT);
		}

		Value GetLRegorTReg(VList LVals = null, VList RVals = null) {
			Value Result = null;
			Result = GetLReg(LVals, RVals);
			if(Result == null)
				Result = GetTReg();
			return Result;
		}


		// make table resolution recursive? 
		// end point, table must be Register, Key must be REG/CONST
		// Takes Value, returns Value with directly usable Table parts?
		Value ResolveTableValue(Value Original) {

			if(!Original.IsTable)
				return null;

			Value UseVal = null;

			if(Original.IsRegister) {
				UseVal = new Value(Original);
			} else {
				Value TReg = GetTReg();
				Value TKey = Original.Key;
				Original.Key = null;
				EmitToRegisterOp(TReg, Original);
				Original.Key = TKey;
				TReg.Key = TKey;
				UseVal = TReg;
			}

			// Get Key into a REG or CONST
			Value UseKey = null;
			//bool KeyConst = false;
			if(Original.Key.IsRegister) {
				UseKey = new Value(Original.Key);
			} else if(Original.Key.IsConstant) {
				UseKey = new Value(Original.Key);
				//KeyConst = true;
			} else {
				Value TReg = GetTReg();
				Value TK = Original.Key.Key;
				Original.Key.Key = null;
				EmitToRegisterOp(TReg, Original.Key);
				Original.Key.Key = TK;
				UseKey = TReg;
				UseKey.Key = TK;
			}


			UseVal.Key = UseKey;

			if(UseVal.Key.IsTable) {
				// GETTABLE
				// Chain UseKey's Key to its result register
				Value ChainKey = UseKey.Key;
				UseKey.Key = null;
				Value ChainTable = GetAsRegister(UseVal);
				ChainTable.Key = ChainKey;
				FreeRegister(UseKey);
				FreeRegister(UseVal);
				return ResolveTableValue(ChainTable);
			}

			return UseVal;
		}

		// Move Whatever to Register Function
		// Mostly for moving whatever RVal to a Temp Reg		
		void EmitToRegisterOp(Value LVal, Value RVal) {
			Console.WriteLine("E2:  {0}  <=  {1}", LVal, RVal);
			if(!LVal.IsRegister)
				return;

			if(LVal.IsRegister && RVal.IsRegister && LVal.Index == RVal.Index)
				return;

			if(RVal.IsTable) {
				// Magic RVal to Good-Table-RVal Function Here
				Value UseRVal = ResolveTableValue(RVal);
				FreeRegister(UseRVal.Key);
				FreeRegister(UseRVal);
				CurrFunc.Instructions.Add(Instruction.OP.GETTABLE, LVal.Index, UseRVal.Index, UseRVal.Key.Index, UseRVal.Key.IsConstant);
			} else if(RVal.IsRegister) {
				FreeRegister(RVal);
				CurrFunc.Instructions.Add(Instruction.OP.MOVE, LVal.Index, RVal.Index);
			} else if(RVal.IsGlobal) {
				CurrFunc.Instructions.Add(Instruction.OP.GETGLOBAL, LVal.Index, RVal.Index);
			} else if(RVal.IsConstant) {
				CurrFunc.Instructions.Add(Instruction.OP.LOADK, LVal.Index, RVal.Index);
			}
			// else GETUPVAL
		}

		// Mostly for moving a Temp Reg to a whatever LVal	
		void EmitFromRegisterOp(Value LVal, Value RVal) {
			Console.WriteLine("E3:  {0}  <=  {1}", LVal, RVal);
			if(!RVal.IsRegister)
				return;

			if(LVal.IsRegister && RVal.IsRegister && LVal.Index == RVal.Index)
				return;


			if(LVal.IsTable) {
				;//CurrFunc.Instructions.Add(Instruction.OP.SETTABLE, UseLVal.Index, UseKey.Index, RVal.Index);
			} else if(LVal.IsRegister) {
				FreeRegister(RVal);
				CurrFunc.Instructions.Add(Instruction.OP.MOVE, LVal.Index, RVal.Index);
			} else if(LVal.IsGlobal) {
				FreeRegister(RVal);
				CurrFunc.Instructions.Add(Instruction.OP.SETGLOBAL, RVal.Index, LVal.Index);
			} else if(LVal.IsConstant) {
				throw new Parser.ParseException("assignment to constant", new Token());
			} 
			// else if upval SETUPVAL

		}

		// Make a Move X to Y general function
		void EmitAssignOp(Value LVal, Value RVal) {
			if(LVal == null || RVal == null)
				return;

			Console.WriteLine("E1:  {0}  <=  {1}", LVal, RVal);


			if(LVal.Loc == RVal.Loc && LVal.Index == RVal.Index)
				return;

			if(LVal.IsRegister && RVal.IsRegister && LVal.Index == RVal.Index)
				return;


			if(LVal.IsTable) {
				Console.WriteLine("T1:  {0}  <=  {1}", LVal, RVal);
				// make table resolution recursive? 
				// end point, table must be Register, Key must be REG/CONST
				// Takes Value, returns Value with directly usable Table parts?

				// Get L into a register
				Value UseLVal = null;
				if(LVal.IsRegister) {
					UseLVal = LVal;
				} else {
					Value TReg = GetTReg();
					Value TKey = LVal.Key;
					LVal.Key = null;
					EmitToRegisterOp(TReg, LVal);
					LVal.Key = TKey;
					TReg.Key = TKey;
					UseLVal = TReg;
				}

				// Get Key into a REG or CONST
				Value UseKey = null;
				bool KeyConst = false;
				if(UseLVal.Key.IsRegister) {
					UseKey = UseLVal.Key;
				} else if(UseLVal.Key.IsConstant) {
					UseKey = UseLVal.Key;
					KeyConst = true;
				} else {
					Value TReg = GetTReg();
					EmitToRegisterOp(TReg, UseLVal.Key);
					UseKey = TReg;
				}

				Console.WriteLine("T2:  {0}  <=  {1}", UseLVal, RVal);

				// figure out the R (is an RK) , generate a SETTABLE ;
				if(RVal.IsTable) {
					Value TReg = GetTReg();
					EmitToRegisterOp(TReg, RVal);
					CurrFunc.Instructions.Add(Instruction.OP.SETTABLE, UseLVal.Index, UseKey.Index, KeyConst, RVal.Index);
					FreeRegister(TReg);
					// generate a GETTABLE, then assign? 
				} else if(RVal.IsRegister) {
					CurrFunc.Instructions.Add(Instruction.OP.SETTABLE, UseLVal.Index, UseKey.Index, KeyConst, RVal.Index);
				} else if(RVal.IsConstant) {
					CurrFunc.Instructions.Add(Instruction.OP.SETTABLE, UseLVal.Index, UseKey.Index, KeyConst, RVal.Index, true);
				} else if(RVal.IsGlobal) {
					Value TReg = GetTReg();
					EmitToRegisterOp(TReg, RVal);
					FreeRegister(TReg);
					CurrFunc.Instructions.Add(Instruction.OP.SETTABLE, UseLVal.Index, UseKey.Index, KeyConst, TReg.Index);
				} 
				// else if upval

				FreeRegister(UseLVal);
				FreeRegister(UseKey);
				FreeRegister(RVal);
			} else if(LVal.IsRegister) {
				EmitToRegisterOp(LVal, RVal);
				FreeRegister(RVal);
			} else if(LVal.IsGlobal) {

				if(RVal.IsRegister) {
					EmitFromRegisterOp(LVal, RVal);
					FreeRegister(RVal);
				} else {
					Value TReg = GetTReg();
					EmitToRegisterOp(TReg, RVal);
					EmitFromRegisterOp(LVal, TReg);
					FreeRegister(TReg);
				}
			} else if(LVal.IsConstant) {
				// Maybe assume it means global of constant?
				throw new Parser.ParseException("assignment to constant", new Token());
			}
			// else if upval
		}

		// Return self, or make a TReg, assign it.
		// Caller should Free this, always
		Value GetAsRegister(Value SrcVal) {
			if(SrcVal.IsRegister && !SrcVal.IsTable)
				return SrcVal;

			Value TReg = GetTReg();
			EmitToRegisterOp(TReg, SrcVal);
			return TReg;
		}

		void CompileBlock(ParseNode Block) {
			if(Block.Children == null)
				return;

			foreach(ParseNode Statement in Block.Children) {
				if(Statement.Type == ParseNode.EType.ASSIGN_STAT)
					CompileAssignmentStmt(Statement);
				else if(Statement.Type == ParseNode.EType.FUNC_CALL)
					CompileFunctionCallStmt(Statement);
				else if(Statement.Type == ParseNode.EType.FUNC_STAT)
					CompileFunctionStmt(Statement);
				else if(Statement.Type == ParseNode.EType.LOCAL_ASSIGN_STAT) 
					CompileLocalAssignStmt(Statement);
				else if(Statement.Type == ParseNode.EType.RETURN_STAT) 
					CompileReturnStmt(Statement);
				//CurrFunc.UsedRegs.Clear();
			}

		}

		// STATEMENTS

		void CompileAssignmentStmt(ParseNode Assignment) {
			if(Assignment.Children.Count != 3)
				return;
			/*ASSIGN_STAT : 3
				......VAR_LIST : 1
				        VAR : 1
				..........TERMINAL : NAME:x :
				......TERMINAL : OPERATOR:= :
				......EXP_LIST : 1
				        EXP : 1
				..........TERMINAL : NUMBER:1 :
		    */
			ParseNode Left, Right;
			Left = Assignment.Children[0];
			Right = Assignment.Children[2];

			VList LVs = CompileLeftVarList(Left);
			VList RVs = CompileExpList(Right, LVs);

			int MaxCount = Math.Max(LVs.Count, RVs.Count);
			for(int I = 0; I < MaxCount; I++) {
				Value LVal=null, RVal=null;

				if(LVs.Count > I)
					LVal = LVs[I];
				if(RVs.Count > I)
					RVal = RVs[I];

				//if(RVal != null)
				//	FreeRegister(RVal);

				if(LVal == null || RVal == null)
					continue;


				EmitAssignOp(LVal, RVal);
			}

		}


		void CompileLocalAssignStmt(ParseNode LocalAssignment) {
			if(LocalAssignment.Children.Count != 4)
				return;

			ParseNode Left, Right;
			// 'local' = 0
			Left = LocalAssignment.Children[1];
			// '=' = 2
			Right = LocalAssignment.Children[3];

			// Left is a NAME_LIST for local, not a VAR_LIST
			VList LVs = CompileNameList(Left);

			VList RVs = CompileExpList(Right, LVs);

			int MaxCount = Math.Max(LVs.Count, RVs.Count);
			for(int I = 0; I < MaxCount; I++) {
				Value LVal=null, RVal=null;

				if(LVs.Count > I)
					LVal = LVs[I];
				if(RVs.Count > I)
					RVal = RVs[I];

				//if(RVal != null)
				//	FreeRegister(RVal);

				if(LVal != null)
					FinalizeLocal(LVal);

				EmitAssignOp(LVal, RVal);
			}
		}


		void CompileFunctionStmt(ParseNode FuncStatement) {
			// 'function' funcname funcbody | 
			// funcname : NAME ('.' NAME)* (':' NAME)? ;
			// funcbody : '(' (parlist1)? ')' block 'end';';
		    
			ParseNode FuncName = FuncStatement.Children[1];
			ParseNode FuncBody = FuncStatement.Children[2];

			ParseNode ParList = null, FuncBlock = null;
			if(FuncBody.Children[1].Type == ParseNode.EType.PAR_LIST) {
				ParList = FuncBody.Children[1];
				FuncBlock = FuncBody.Children[3];
			} else {
				ParList = null;
				FuncBlock = FuncBody.Children[2];
			}

			if(FuncBlock.IsTerminal() && FuncBlock.Token.IsKeyword("end"))
				FuncBlock = null;


			// Resolve Funcname, 
			// get it into globals
			string FuncNameStr;
			FuncNameStr = FuncName.Children[0].Token.Value;

			// push function..
			Function NewFunc = new Function();
			FunctionStack.Push(NewFunc);
			CurrFunc = NewFunc;

			// Parse ParList, List of Args/Locals?
			// Do it after CurrFunc shift
			// FIXME: '...'
			// VList ParValues = CompileParList(ParList);
			VList ParValues;
			if(ParList != null) {
				ParValues = CompileNameList(ParList.Children[0]);
				CurrFunc.NumParams = ParValues.Count;
			}

			if(FuncBlock != null)
				CompileBlock(FuncBlock); 

			CurrFunc.Instructions.Add(Instruction.OP.RETURN, 0, 1); // default return

			Functions.Add(NewFunc);
			FunctionStack.Pop();  // we are done
			CurrFunc = FunctionStack.Peek();

			Value GVal = GetGlobalIndex(FuncNameStr);
			Value ClosureReg = new Value(GetFreeRegister());
			int UpValCount = 0;
			CurrFunc.Instructions.Add(Instruction.OP.CLOSURE, ClosureReg.Index, UpValCount);
			EmitAssignOp(GVal, ClosureReg);
			//CurrFunc.Instructions.Add(Instruction.OP.SETGLOBAL, ClosureReg.Index, GVal.Index);
			FreeRegister(GVal);
			FreeRegister(ClosureReg);
		}


		void CompileFunctionCallStmt(ParseNode CallStmt) {
			// functioncall: varOrExp nameAndArgs+;
			CompilePrefixExp(CallStmt);
		}


		void CompileReturnStmt(ParseNode ReturnStatement) {
			// 'return' (explist1)?
			ParseNode ExpList = ReturnStatement.Children[1];
			VList RetVs = CompileExpList(ExpList);

			int A = 0, B = 1;
			if(RetVs != null) {
				if(RetVs.Count > 0) {
					A = RetVs[0].Index;
					B = RetVs.Count + 1;
				}
			}

			CurrFunc.Instructions.Add(Instruction.OP.RETURN, A, B);
		}



		// AST PARTS
		VList CompileLeftVarList(ParseNode VarList) {
			// Can Var-list be an R-Value? Maybe. Maybe it would be wrapped in an ExpList if it was?
			// Anyway, this is meant to be an L-Value. 
			//  It should tell the assigner which of local, global, table, upval this is, 
			//   so the statement can pick the right op
			//  and which index to use.
			// If discoverying those requires an expression (tablelookups), 
			//  this will already generate that code
		
			VList Result = new VList();

			foreach(ParseNode Child in VarList.Children) {
				if(Child.Type != ParseNode.EType.VAR)
					continue;

				Value CurrV = CompileLeftVar(Child);
				Result.Add(CurrV);
			}

			return Result;
		}

		Value CompileLeftVar(ParseNode Var) {

			Value Result = new Value();

			Value First = null;

			int ChildIndex = 0;
			if(Var.Children[0].Type == ParseNode.EType.TERMINAL &&
				Var.Children[0].Token.Type == Token.EType.NAME) {
				string Name = Var.Children[0].Token.Value; ChildIndex++;
				Value LocalV = GetLocalIndex(Name, false);
				if(LocalV != null) {
					First = LocalV;
				} else {
					Value GlobalV = GetGlobalIndex(Name);
					if(GlobalV != null)
						First = GlobalV;
				} 
			}

			else if(Var.Children[0].Type == ParseNode.EType.TERMINAL &&
			   Var.Children[0].Token.IsBracket("(")) {
				VList ExpVs = CompileExp(Var.Children[1]);
				First = ExpVs[0];
				ParseNode VarSuffix = Var.Children[3];
				// Suffix stuff
				ChildIndex += 3;
			}

			Result = First;

			VList Keys = new VList();
			while(ChildIndex < Var.Children.Count) {
				if(Var.Children[ChildIndex].Type != ParseNode.EType.VAR_SUFFIX) {
					// throw?
					ChildIndex++;
					continue;
				}

				// Value CompileVarSuffix(ParseNode VarSuffix)
				//varSuffix: nameAndArgs* ('[' exp ']' | '.' NAME);
				ParseNode VarSuffix = Var.Children[ChildIndex]; ChildIndex++;

				// handle this somehow?
				while(VarSuffix.Children[0].Type == ParseNode.EType.NAME_AND_ARGS) {
					break;
				}
				Value Key = null;
				if(VarSuffix.Children[0].Type == ParseNode.EType.TERMINAL &&
					VarSuffix.Children[0].Token.IsBracket("[")) {
					VList SubRs = CompileExp(VarSuffix.Children[1]);
					Key = SubRs[0];
				}
				else if(VarSuffix.Children[0].Type == ParseNode.EType.TERMINAL &&
				        VarSuffix.Children[0].Token.IsOperator(".")) {
					string Name = VarSuffix.Children[1].Token.Value;
					Key = GetConstIndex(Name);
				} 
				if(Key != null)
					Keys.Add(Key);
			}

			if(Keys.Count > 0) {
				Value Curr = new Value(First);
				// Take first, wrap it in a new Table value
				for(int i = 0; i < Keys.Count; i++) {
					Value Key = Keys[i];
					Curr.Key = Key;

					if( (i + 1) < Keys.Count) {
						Value TReg = GetAsRegister(Curr);
						FreeRegister(Curr.Key);
						FreeRegister(Curr);
						Curr = TReg;
					}
					//Console.WriteLine("LV {0}", Curr.ToString());
				}
				Result = Curr;
			} 

			return Result;
		}

		VList CompileNameList(ParseNode NameList) {
			VList LVs = new VList();
			foreach(ParseNode Name in NameList.Children) {
				if(Name.Type == ParseNode.EType.TERMINAL && 
				   Name.Token.IsOperator(",")) {
					continue;
				}
				else if(Name.Type == ParseNode.EType.TERMINAL && 
				        Name.Token.Type == Token.EType.NAME) {
					string NameVal = Name.Token.Value;
					Value LVal = GetLocalIndex(NameVal);
					//Value LVal = new Value(LReg, Value.ELoc.LOCAL, Value.ESide.LEFT);
					LVs.Add(LVal);
				}
			}
			return LVs;
		}

		VList CompileExpList(ParseNode ExpList, VList LVals = null) {
			// explist1 : (exp ',')* exp;
			if(ExpList.Children == null)
				return null;

			VList Result = new VList();

			foreach(ParseNode Child in ExpList.Children) {
				if(Child.Type != ParseNode.EType.EXP)
					continue;

				VList CurrVs = CompileExp(Child, LVals, Result);
				Result.AddRange(CurrVs);
			}

			return Result;
		}

		VList CompileExp(ParseNode Exp, VList LVals = null, VList RVals = null) {

			// sometimes CompileExp will be called, but Exp will actually 
			//  be a node right under an exp, 
			// So rather than every calling point being complicated, this will fix it up
			// FIXME (wrap and call on fake-parent?)
			if(Exp.Type != ParseNode.EType.EXP) {
				ParseNode Temp = new ParseNode(ParseNode.EType.EXP);
				Temp.Add(Exp);
				return CompileExp(Temp, LVals, RVals);
			}

			VList Result = new VList();

			foreach(ParseNode Child in Exp.Children) {
				if(Child.Type == ParseNode.EType.TERMINAL) {
					if(Child.Token.Type == Token.EType.KEYWORD) {
						if(Child.Token.IsKeyword("nil")) {
							Value Dest = GetLRegorTReg(LVals, RVals);
							CurrFunc.Instructions.Add(Instruction.OP.LOADNIL, Dest.Index, Dest.Index);
							Result.Add(Dest);
						} else if(Child.Token.IsKeyword("true")) {
							Value Dest = GetLRegorTReg(LVals, RVals);
							CurrFunc.Instructions.Add(Instruction.OP.LOADBOOL, Dest.Index, 1, 0);
							Result.Add(Dest);
						} else if(Child.Token.IsKeyword("false")) {
							Value Dest = GetLRegorTReg(LVals, RVals);
							CurrFunc.Instructions.Add(Instruction.OP.LOADBOOL, Dest.Index, 0, 0);
							Result.Add(Dest);
						}


					} else if(Child.Token.Type == Token.EType.NUMBER) {
						double NumValue = 0.0;
						Double.TryParse(Child.Token.Value, out NumValue);
						Value CVal = GetConstIndex(NumValue);
						Result.Add(CVal);
						//Value Dest = GetLRegorTReg(LVals, RVals);
						//EmitAssignOp(Dest, CVal);
						//Result.Add(Dest);
					} else if(Child.Token.Type == Token.EType.STRING) {
						Value CVal = GetConstIndex(Child.Token.Value);
						Result.Add(CVal);
						//Value Dest = GetLRegorTReg(LVals, RVals);
						//EmitAssignOp(Dest, CVal);
						//Result.Add(Dest);
					}
					// '...' ???
				} else if(Child.Type == ParseNode.EType.UN_OP_WRAP) {
					Result.Add(CompileUnOp(Child, LVals, RVals));
				} else if(Child.Type == ParseNode.EType.BIN_OP_WRAP) {
					Result.Add(CompileBinOp(Child, LVals, RVals));
				} else if(Child.Type == ParseNode.EType.PREFIX_EXP) {
					// might be a var, might be a function call
					Result.AddRange(CompilePrefixExp(Child, LVals, RVals));
				} else if(Child.Type == ParseNode.EType.TABLE_CONS) {
					Result.Add(CompileTableConstructor(Child, LVals, RVals));
				}
			}

			return Result;
		}

		VList CompilePrefixExp(ParseNode PrefixExp, VList LVals=null, VList RVals=null) {
			// prefixexp: varOrExp nameAndArgs*;
			VList Result = new VList();


			ParseNode VarOrExp = PrefixExp.Children[0];
			if(PrefixExp.Children.Count == 1) // not a func call
				return CompileVarOrExp(VarOrExp, LVals, RVals);
			else {
				Result = CompileVarOrExp(VarOrExp);
			}



			// nameAndArgs: (':' NAME)? args;
			foreach(ParseNode NameAndArgs in PrefixExp.Children) {
				if(NameAndArgs.Type != ParseNode.EType.NAME_AND_ARGS)
					continue;

				// Turns out the first thing was a function to call. Once params
				// are resolved, call it the func in Result[0], and then pop it
			
				ParseNode ClassFuncName = null;
				ParseNode Args = null;

				if(NameAndArgs.Children[0].Type == ParseNode.EType.TERMINAL &&
					NameAndArgs.Children[0].Token.IsOperator(":")) {
					ClassFuncName = NameAndArgs.Children[1];
					// OP.SELF 
					Args = NameAndArgs.Children[2];
				} else {
					Args = NameAndArgs.Children[0];
				}

				int RetCount = 0; 
				if(LVals != null && RVals != null)
					RetCount = LVals.Count - RVals.Count;
				int ArgCount = CountArgs(Args);

				int StackSpaceNeeded = Math.Max(RetCount, ArgCount+1);

				Console.WriteLine(" R:{0}  A:{1}  SN:{2}", RetCount, ArgCount, StackSpaceNeeded);
				Console.Write(" UR: ");
				foreach(var R in CurrFunc.UsedRegs) {
					Console.Write(" {0} ", R); 
				}
				Console.WriteLine();

				Value FuncVal = Result[Result.Count - 1];
				Result.RemoveAt(Result.Count - 1);

				FreeRegister(FuncVal);


				VList StackSpace;
				StackSpace = GetStackFrame(FuncVal.Index, StackSpaceNeeded);
				EmitAssignOp(StackSpace[0], FuncVal);
				FuncVal = StackSpace[0];

				Console.WriteLine(" FV:{0} ", FuncVal.Index);

				//  Allocate LVals for args? Pass into CompileArgs?
				VList ArgRegs = new VList(ArgCount);
				for(int i =0; i < ArgCount; i++) 
					ArgRegs.Add(StackSpace[i+1]);
				VList ArgVs = CompileArgs(Args, ArgRegs);
				// Claim ArgRegs
				ClaimStackFrame(ArgRegs);

				CurrFunc.Instructions.Add(Instruction.OP.CALL, FuncVal.Index, ArgVs.Count+1, RetCount+1);
				// Add return regs to Result?
				for(int i = 0; i < RetCount; i++)
					Result.Add(StackSpace[i]);
				// Free the excess
				for(int i = RetCount; i < StackSpace.Count; i++)
					FreeRegister(StackSpace[i]);
			}

			return Result;
		}

		VList CompileVarOrExp(ParseNode VarOrExp, VList LVals=null, VList RVals=null) {
			// varOrExp: var | '(' exp ')';

			VList Result = new VList();

			if(VarOrExp.Children[0].Type == ParseNode.EType.VAR) {
				ParseNode Var = VarOrExp.Children[0];
				string Name = Var.Children[0].Token.Value; // FIXME complicated names
				Value VarVal = null;
				{{
					Value LocalV = GetLocalIndex(Name, false);
					if(LocalV != null) {
						VarVal = LocalV;
					} else {
						//bool IsGlobal = CheckGlobal(Name);
						Value GlobalV = GetGlobalIndex(Name);
						VarVal = GlobalV;
					}
				}}

				{{
					VList Keys = new VList();
					int ChildIndex = 1;
					while(ChildIndex < Var.Children.Count) {
						if(Var.Children[ChildIndex].Type != ParseNode.EType.VAR_SUFFIX) {
							// throw?
							ChildIndex++;
							continue;
						}

						// Value CompileVarSuffix(ParseNode VarSuffix)
						//varSuffix: nameAndArgs* ('[' exp ']' | '.' NAME);
						ParseNode VarSuffix = Var.Children[ChildIndex]; ChildIndex++;

						// handle this somehow?
						while(VarSuffix.Children[0].Type == ParseNode.EType.NAME_AND_ARGS) {
							break;
						}
						Value Key = null;
						if(VarSuffix.Children[0].Type == ParseNode.EType.TERMINAL &&
						   VarSuffix.Children[0].Token.IsBracket("[")) {
							VList SubRs = CompileExp(VarSuffix.Children[1]);
							Key = SubRs[0];
						}
						else if(VarSuffix.Children[0].Type == ParseNode.EType.TERMINAL &&
						        VarSuffix.Children[0].Token.IsOperator(".")) {
							string SuffixName = VarSuffix.Children[1].Token.Value;
							Key = GetConstIndex(SuffixName);
						} 
						if(Key != null)
							Keys.Add(Key);
					}

					if(Keys.Count > 0) {
						Value Curr = new Value(VarVal);
						// Take first, wrap it in a new Table value
						for(int i = 0; i < Keys.Count; i++) {
							Value Key = Keys[i];
							Curr.Key = Key;

							if( (i + 1) < Keys.Count) {
								Value TReg = GetAsRegister(Curr);
								FreeRegister(Curr.Key);
								FreeRegister(Curr);
								Curr = TReg;
							}
						}
						VarVal = Curr;
					} 
				}}

				Result.Add(VarVal);

				/*
				Value LReg = GetLReg(LVals, RVals);

				if(VarVal.IsRegister) {
					if( (LReg != null && LReg.Consecutive) ) {
						Result.Add(LReg);
						//FreeRegister(VarVal);
						EmitAssignOp(LReg, VarVal);
					} else {
						Result.Add(VarVal);
					}
				} else {
					Value Dest = GetLRegorTReg(LVals, RVals);
					Result.Add(Dest);
					//FreeRegister(VarVal);
					EmitAssignOp(Dest, VarVal);
					//CurrFunc.Instructions.Add(Instruction.OP.GETGLOBAL, Dest.Index, VarVal.Index);
				}
				*/
			} else { // assume ( exp ) 
				ParseNode Exp = VarOrExp.Children[1];
				Result = CompileExp(Exp);
			}

			return Result;
		}

		VList CompileArgs(ParseNode Args, VList LVals=null) {
			// args :  '(' (explist1)? ')' | tableconstructor | string ;
			VList Result = null;

			if (Args.Children[0].Type == ParseNode.EType.TERMINAL &&
				Args.Children[0].Token.IsBracket("(")) {
				if(Args.Children[1].Type == ParseNode.EType.EXP_LIST) {
					Result = CompileExpList(Args.Children[1], LVals);
				}
			} else if(Args.Children[0].Type == ParseNode.EType.TABLE_CONS) {
				; // FIXME
			} else if(Args.Children[0].Type == ParseNode.EType.TERMINAL &&
				Args.Children[0].Token.Type == Token.EType.STRING) {
				; // FIXME
			}

			return Result;
		}

		int CountArgs(ParseNode Args) {
			// args :  '(' (explist1)? ')' | tableconstructor | string ;
			int ArgCount = 0;

			if (Args.Children[0].Type == ParseNode.EType.TERMINAL &&
			    Args.Children[0].Token.IsBracket("(")) {
				if(Args.Children[1].Type == ParseNode.EType.EXP_LIST) {
					ParseNode ExpList = Args.Children[1];
					foreach(ParseNode Exp in ExpList.Children) {
						if(Exp.Type == ParseNode.EType.EXP)
							ArgCount++;
					}
				}
			} else if(Args.Children[0].Type == ParseNode.EType.TABLE_CONS) {
				; // FIXME
			} else if(Args.Children[0].Type == ParseNode.EType.TERMINAL &&
			          Args.Children[0].Token.Type == Token.EType.STRING) {
				; // FIXME
			}

			return ArgCount;
		}

		Value CompileTableConstructor(ParseNode TableCons, VList LVals=null, VList RVals=null) {

			// always create to a fresh register
			Value ConsReg = GetLRegorTReg();

			CurrFunc.Instructions.Add(Instruction.OP.NEWTABLE, ConsReg.Index, 0, 0); // FIXME

			return ConsReg;
		}

		Value CompileBinOp(ParseNode BinOp, VList LVals = null, VList RVals = null) {

			ParseNode Left = BinOp.Children[0];
			ParseNode Op = BinOp.Children[1];
			ParseNode Right = BinOp.Children[2];


			VList LVs = CompileExp(Left);
			VList RVs = CompileExp(Right);


			Instruction.OP InstOp = Instruction.OP.ERROR;
			if(Op.Token.IsOperator("+"))
				InstOp = Instruction.OP.ADD;
			else if(Op.Token.IsOperator("-"))
				InstOp = Instruction.OP.SUB;
			else if(Op.Token.IsOperator("*"))
				InstOp = Instruction.OP.MUL;
			else if(Op.Token.IsOperator("/"))
				InstOp = Instruction.OP.DIV;
			else if(Op.Token.IsOperator("^"))
				InstOp = Instruction.OP.POW;
			else if(Op.Token.IsOperator("%"))
				InstOp = Instruction.OP.MOD;

			Value LV = GetAsRegister(LVs[0]);
			Value RV = GetAsRegister(RVs[0]);

			FreeRegister(LV);
			FreeRegister(RV);

			//Value Result = new Value(GetFreeRegister(), Value.ELoc.REGISTER, Value.ESide.RIGHT);
			Value Result = GetLRegorTReg(LVals, RVals);

			CurrFunc.Instructions.Add(InstOp, Result.Index, LV.Index, RV.Index);

			return Result;
		}

		Value CompileUnOp(ParseNode UnOp, VList LVals = null, VList RVals = null) {

			ParseNode Op = UnOp.Children[0];
			ParseNode Right = UnOp.Children[1];

			VList RVs = CompileExp(Right);


			Instruction.OP InstOp = Instruction.OP.ERROR;
			if(Op.Token.IsOperator("-"))
				InstOp = Instruction.OP.UNM;
			else if(Op.Token.IsKeyword("not"))
				InstOp = Instruction.OP.NOT;
			else if(Op.Token.IsOperator("#"))
				InstOp = Instruction.OP.LEN;

			Value RV = GetAsRegister(RVs[0]);
			FreeRegister(RV);

			Value Result = GetLRegorTReg(LVals, RVals);

			CurrFunc.Instructions.Add(InstOp, Result.Index, RV.Index);

			return Result;
		}

	}
}

