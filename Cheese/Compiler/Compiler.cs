
using System;
//using System.IO;
//using System.Text;
using System.Collections.Generic;

using Cheese.Machine;

namespace Cheese
{


	//using Value = CompilerValue;
	using VList = List<CompilerValue>;


	class CompilerValue {

		public enum ELoc {
			LOCAL,  // locals are a register, but don't release it 
			GLOBAL,
			UPVAL,
			TABLE,
			CONSTANT,
			REGISTER
		}

		internal ELoc Loc;
		internal int Index;
		internal bool Consecutive;
		internal CompilerValue Key;

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
		internal bool IsRegisterOrConstant {
			get { return (IsRegister || IsConstant); }
		}

		public CompilerValue(int Index=0, ELoc Loc=ELoc.REGISTER, CompilerValue Key=null) {
			this.Index = Index; 
			this.Loc = Loc;
			this.Consecutive = false;
			this.Key = null;
		}

		public CompilerValue(CompilerValue Other) {
			this.Index = Other.Index;
			this.Loc = Other.Loc;
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


	// Essentially a function, but for the 'root' of the function tree, 
	public class Chunk {
		internal Function RootFunc;

		public Chunk() {
			;
		}
	} 

	class Compiler
	{ 

		Machine.Function CurrFunc;
		List<Machine.Function> Functions;
		Stack<Machine.Function> FunctionStack;

		List<CompilerValue> Globals;


		public Compiler()
		{
			Functions = new List<Function>();
			FunctionStack = new Stack<Function>();
			Globals = new List<CompilerValue>();
		}


		public Chunk Compile(ParseNode RootChunk) 
		{
			if(RootChunk.Children == null)
				return null;

			Chunk Result = new Chunk();
			Result.RootFunc = new Function();

			// Curr and Stack.Top should be synonymous
			FunctionStack.Push(Result.RootFunc);
			CurrFunc = Result.RootFunc;

			PushLocalScope();

			foreach(ParseNode Child in RootChunk.Children) {
				if(Child.Type == ParseNode.EType.BLOCK)
					CompileBlock(Child);
			}

			// all functions, even chunk root, end with a return
			Result.RootFunc.Instructions.Add(Instruction.OP.RETURN, 0, 1);
			PopLocalScope();

			Functions.Insert(0, Result.RootFunc);

			FunctionStack.Pop();

			foreach(Function Func in Functions) {
				Console.WriteLine("Function:  ");
				Func.Print();
			}


			return Result;
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

		// Does not reserve the register, just gets the next free one. 
		// Use it to set up something, while leaving it available as a temp
		// until its final, then Claim it.
		int GetUnclaimedRegister() {
			int Result = 0;
			while(CurrFunc.UsedRegs.Contains(Result)) {
				Result++;
			}
			return Result;
		}

		void ClaimRegister(CompilerValue Reg) {
			CurrFunc.UsedRegs.Add(Reg.Index);
			CurrFunc.MaxStackSize = Math.Max(CurrFunc.MaxStackSize, Reg.Index+1);
		}


		void FreeRegister(CompilerValue Reg) {
			if(Reg.Loc == CompilerValue.ELoc.REGISTER)
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
		// Remember to Claim later
		VList UnclaimedConsecutiveRegisters(int First, int Count) {
			while(true) {
				if(IsConsecutiveRegisterFree(First, Count)) {
					break;
				}
				First++;
			}
			VList Result = new VList();
			for(int I = 0; I < Count; I++) {
				int CR = First+I;
				//CurrFunc.UsedRegs.Add(CR);
				//CurrFunc.MaxStackSize = Math.Max(CurrFunc.MaxStackSize, CR+1);
				Result.Add(new CompilerValue(CR, CompilerValue.ELoc.REGISTER));
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
				Result.Add(new CompilerValue(CR, CompilerValue.ELoc.REGISTER));
				Result[Result.Count - 1].Consecutive = true;
			}
			return Result;
		}

		void ClaimStackFrame(VList Frame) {
			foreach(CompilerValue Reg in Frame) {
				CurrFunc.UsedRegs.Add(Reg.Index);
				CurrFunc.MaxStackSize = Math.Max(CurrFunc.MaxStackSize, Reg.Index+1);
			}
		}


		// Constants and Globals

		CompilerValue GetConstIndex(string ConstValue) {
			foreach(ConstEntry Entry in CurrFunc.ConstantTable) {
				if(Entry.StringVal != null && Entry.StringVal == ConstValue) {
					return new CompilerValue(Entry.Index, CompilerValue.ELoc.CONSTANT);
					//return Entry.Value;
				}
			}
			ConstEntry NewEntry = new ConstEntry();
			NewEntry.Index = CurrFunc.ConstantTable.Count;
			//NewEntry.Loc = Cheese.CompilerValue.ELoc.CONSTANT;
			NewEntry.StringVal = ConstValue;
			CurrFunc.ConstantTable.Add(NewEntry);
			return new CompilerValue(NewEntry.Index, CompilerValue.ELoc.CONSTANT);
			//return NewEntry.Value;
		}

		CompilerValue GetConstIndex(double ConstValue) {
			foreach(ConstEntry Entry in CurrFunc.ConstantTable) {
				if(Entry.StringVal == null && Entry.NumberVal == ConstValue) {
					return new CompilerValue(Entry.Index, CompilerValue.ELoc.CONSTANT);
					//return Entry.Value;
				}
			}
			ConstEntry NewEntry = new ConstEntry();
			NewEntry.Index = CurrFunc.ConstantTable.Count;
			//NewEntry.Value.Loc = Cheese.CompilerValue.ELoc.CONSTANT;
			NewEntry.NumberVal = ConstValue;
			CurrFunc.ConstantTable.Add(NewEntry);
			return new CompilerValue(NewEntry.Index, CompilerValue.ELoc.CONSTANT);
			//return NewEntry.Value;
		}

		bool CheckGlobal(string Name) {
			foreach(CompilerValue Iter in Globals) {
				if (Iter.Index < CurrFunc.ConstantTable.Count &&
					Name == CurrFunc.ConstantTable[Iter.Index].StringVal) {
					return true;
				}
			}
			return false;
		}

		CompilerValue GetGlobalIndex(string GlobalName) {
			foreach(CompilerValue Iter in Globals) {
				if (Iter.Index < CurrFunc.ConstantTable.Count &&
				    GlobalName == CurrFunc.ConstantTable[Iter.Index].StringVal) {
					return Iter;
				}
			}
			CompilerValue ConstV = GetConstIndex(GlobalName);
			CompilerValue GlobalV = new CompilerValue();
			GlobalV.Index = ConstV.Index;
			GlobalV.Loc = CompilerValue.ELoc.GLOBAL;
			Globals.Add(GlobalV);
			return GlobalV;
		}


		// Local Tools 
		CompilerValue GetLocalIndex(string Name, bool Create=true) {

			for(int RI = CurrFunc.LocalScopes.Count-1; RI >= 0; RI--) {
				List<LocalEntry> CurrScope = CurrFunc.LocalScopes[RI];
				foreach(LocalEntry Entry in CurrScope) {
					if(Entry.Name != null && Entry.Name == Name) {
						return new CompilerValue(Entry.Index, CompilerValue.ELoc.LOCAL);
						//return Entry.Value;
					}
				}
			}

			//foreach(LocalEntry Entry in CurrFunc.LocalTable) {
			//	if(Entry.Name != null && Entry.Name == Name) {
			//		return Entry.Value;
			//	}
			//}
			if(Create) {
				//if(CurrFunc.LocalScopes.Count == 0) {
				//	PushLocalScope();
				//}
				return CreateLocal(Name);
			} else {
				return null;
			}
		}

		CompilerValue CreateLocal(string Name) {
			//int FreeReg = GetFreeRegister();
			int LocalCount = 0;
			foreach(List<LocalEntry> Scope in CurrFunc.LocalScopes) {
				foreach(LocalEntry Entry in Scope) {
					LocalCount++;
				}
			}
			LocalEntry NewEntry = new LocalEntry();
			//NewEntry.Value.Index = CurrFunc.LocalTable.Count;
			NewEntry.Index = LocalCount;
			//NewEntry.Value.Loc = CompilerValue.ELoc.LOCAL;
			NewEntry.Name = Name;
			NewEntry.StartPC = CurrFunc.Instructions.Count - 1;
			List<LocalEntry> TopScope = CurrFunc.LocalScopes[CurrFunc.LocalScopes.Count - 1];
			TopScope.Add(NewEntry);
			//CurrFunc.LocalTable.Add(NewEntry);
			return new CompilerValue(NewEntry.Index, CompilerValue.ELoc.LOCAL);
			//return NewEntry.Value;
		}

		void FinalizeLocal(CompilerValue Local) {
			CurrFunc.UsedRegs.Add(Local.Index);
			CurrFunc.MaxStackSize = Math.Max(CurrFunc.MaxStackSize, Local.Index+1);
		}

		void PushLocalScope() {
			List<LocalEntry> NewScope = new List<LocalEntry>();
			CurrFunc.LocalScopes.Add(NewScope);
		}

		void PopLocalScope() {
			if(CurrFunc.LocalScopes.Count > 0) {
				List<LocalEntry> OutScope = null;
				OutScope = CurrFunc.LocalScopes[CurrFunc.LocalScopes.Count - 1];
				CurrFunc.LocalScopes.RemoveAt(CurrFunc.LocalScopes.Count - 1);
				foreach(LocalEntry Entry in OutScope) {
					Entry.EndPC = CurrFunc.Instructions.Count - 1;
					FreeRegister(Entry.Index);
				}
				CurrFunc.FullLocalTable.AddRange(OutScope);
			}
		}


		// Register Shuffles

		CompilerValue GetLReg(VList LVals = null, VList RVals = null) {
			CompilerValue Result = null;
			if(LVals != null && RVals != null && LVals.Count > RVals.Count) {
				if (LVals[RVals.Count].IsRegister && !LVals[RVals.Count].IsTable) {
					Result = LVals[RVals.Count];
				}
			}
			return Result;
		}

		CompilerValue GetTReg() {
			return new CompilerValue(GetFreeRegister(), CompilerValue.ELoc.REGISTER);
		}
		CompilerValue GetUnclaimedReg() {
			return new CompilerValue(GetUnclaimedRegister(), CompilerValue.ELoc.REGISTER);
		}

		CompilerValue GetLRegorTReg(VList LVals = null, VList RVals = null) {
			CompilerValue Result = null;
			Result = GetLReg(LVals, RVals);
			if(Result == null)
				Result = GetTReg();
			return Result;
		}


		// make table resolution recursive? 
		// end point, table must be Register, Key must be REG/CONST
		// Takes Value, returns Value with directly usable Table parts?
		CompilerValue ResolveTableValue(CompilerValue Original) {

			if(!Original.IsTable)
				return null;

			Console.WriteLine("R:  {0} ", Original);

			CompilerValue UseVal = null;

			if(Original.IsRegister) {
				UseVal = new CompilerValue(Original);
			} else {
				CompilerValue TReg = GetTReg();
				CompilerValue TKey = Original.Key;
				Original.Key = null;
				EmitToRegisterOp(TReg, Original);
				Original.Key = TKey;
				TReg.Key = TKey;
				UseVal = TReg;
			}

			// Get Key into a REG or CONST
			CompilerValue UseKey = null;
			//bool KeyConst = false;


			if(Original.Key.IsRegister) {
				UseKey = new CompilerValue(Original.Key);
			} else if(Original.Key.IsConstant) {
				UseKey = new CompilerValue(Original.Key);
				//KeyConst = true;
			} else {
				CompilerValue TReg = GetTReg();
				CompilerValue TK = Original.Key.Key;
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
				CompilerValue ChainKey = UseKey.Key;
				UseKey.Key = null;
				CompilerValue ChainTable = GetAsRegister(UseVal);
				ChainTable.Key = ChainKey;
				FreeRegister(UseKey);
				FreeRegister(UseVal);
				return ResolveTableValue(ChainTable);
			}

			return UseVal;
		}

		// Move Whatever to Register Function
		// Mostly for moving whatever RVal to a Temp Reg		
		void EmitToRegisterOp(CompilerValue LVal, CompilerValue RVal) {
			Console.WriteLine("E2:  {0}  <=  {1}", LVal, RVal);
			if(!LVal.IsRegister)
				return;

			if(LVal.IsRegister && RVal.IsRegister && LVal.Index == RVal.Index)
				return;

			if(RVal.IsTable) {
				// Magic RVal to Good-Table-RVal Function Here
				CompilerValue UseRVal = ResolveTableValue(RVal);
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
		void EmitFromRegisterOp(CompilerValue LVal, CompilerValue RVal) {
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
		void EmitAssignOp(CompilerValue LVal, CompilerValue RVal) {
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
				CompilerValue UseLVal = null;
				if(LVal.IsRegister) {
					UseLVal = LVal;
				} else {
					CompilerValue TReg = GetTReg();
					CompilerValue TKey = LVal.Key;
					LVal.Key = null;
					EmitToRegisterOp(TReg, LVal);
					LVal.Key = TKey;
					TReg.Key = TKey;
					UseLVal = TReg;
				}

				// Get Key into a REG or CONST
				CompilerValue UseKey = null;
				bool KeyConst = false;
				if(UseLVal.Key.IsRegister) {
					UseKey = UseLVal.Key;
				} else if(UseLVal.Key.IsConstant) {
					UseKey = UseLVal.Key;
					KeyConst = true;
				} else {
					CompilerValue TReg = GetTReg();
					EmitToRegisterOp(TReg, UseLVal.Key);
					UseKey = TReg;
				}

				Console.WriteLine("T2:  {0}  <=  {1}", UseLVal, RVal);

				// figure out the R (is an RK) , generate a SETTABLE ;
				if(RVal.IsTable) {
					CompilerValue TReg = GetTReg();
					EmitToRegisterOp(TReg, RVal);
					CurrFunc.Instructions.Add(Instruction.OP.SETTABLE, UseLVal.Index, UseKey.Index, KeyConst, RVal.Index);
					FreeRegister(TReg);
					// generate a GETTABLE, then assign? 
				} else if(RVal.IsRegister) {
					CurrFunc.Instructions.Add(Instruction.OP.SETTABLE, UseLVal.Index, UseKey.Index, KeyConst, RVal.Index);
				} else if(RVal.IsConstant) {
					CurrFunc.Instructions.Add(Instruction.OP.SETTABLE, UseLVal.Index, UseKey.Index, KeyConst, RVal.Index, true);
				} else if(RVal.IsGlobal) {
					CompilerValue TReg = GetTReg();
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
					CompilerValue TReg = GetTReg();
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
		CompilerValue GetAsRegister(CompilerValue SrcVal) {
			if(SrcVal.IsRegister && !SrcVal.IsTable)
				return SrcVal;

			CompilerValue TReg = GetUnclaimedReg();
			EmitToRegisterOp(TReg, SrcVal);
			ClaimRegister(TReg);
			return TReg;
		}


		// Compiling

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
				else if(Statement.Type == ParseNode.EType.IF_STAT) 
					CompileIfStmt(Statement);
				else if(Statement.Type == ParseNode.EType.WHILE_STAT) 
					CompileWhileStmt(Statement);
				else if(Statement.Type == ParseNode.EType.REPEAT_STAT)
					CompileRepeatStmt(Statement);
				else if(Statement.Type == ParseNode.EType.FOR_NUM_STAT)
					CompileForNumericalStmt(Statement);
				else if(Statement.Type == ParseNode.EType.FOR_ITER_STAT)
					CompileForIterStmt(Statement);
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
				CompilerValue LVal=null, RVal=null;

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
				CompilerValue LVal=null, RVal=null;

				if(LVs.Count > I)
					LVal = LVs[I];
				if(RVs.Count > I)
					RVal = RVs[I];

				//if(RVal != null)
				//	FreeRegister(RVal);



				EmitAssignOp(LVal, RVal);

				FinalizeLocal(LVal);	
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

			PushLocalScope();

			// Parse ParList, List of Args/Locals?
			// Do it after CurrFunc shift
			// FIXME: '...'
			// VList ParValues = CompileParList(ParList);
			VList ParValues;
			if(ParList != null) {
				ParValues = CompileNameList(ParList.Children[0]);
				foreach(CompilerValue Param in ParValues) {
					FinalizeLocal(Param);
				}
				CurrFunc.NumParams = ParValues.Count;
			}

			if(FuncBlock != null)
				CompileBlock(FuncBlock); 

			CurrFunc.Instructions.Add(Instruction.OP.RETURN, 0, 1); // default return

			PopLocalScope();

			Functions.Add(NewFunc);
			FunctionStack.Pop();  // we are done
			CurrFunc = FunctionStack.Peek();

			CompilerValue GVal = GetGlobalIndex(FuncNameStr);
			CompilerValue ClosureReg = new CompilerValue(GetFreeRegister());
			//int UpValCount = 0;
			int FunctionNumber = Functions.Count - 1;
			CurrFunc.Instructions.Add(Instruction.OP.CLOSURE, ClosureReg.Index, FunctionNumber);
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


		void CompileIfStmt(ParseNode IfStatement) {
			// 'if' exp 'then' block ('elseif' exp 'then' block)* ('else' block)? 'end' |

			List<int> JumpStartOps = new List<int>();
			List<int> JumpNextOps = new List<int>();
			List<int> JumpEndOps = new List<int>();

			int ChildIndex = 0;
			int EndOpPos = 0;

			// Generator Ops
			while(ChildIndex < IfStatement.Children.Count) {
				ParseNode Exp=null, Block=null;

				if(IfStatement.Children[ChildIndex].Token.IsKeyword("if") ||
					IfStatement.Children[ChildIndex].Token.IsKeyword("elseif")) {
					Exp = IfStatement.Children[ChildIndex + 1];
					Block = IfStatement.Children[ChildIndex + 3];
				} else if(IfStatement.Children[ChildIndex].Token.IsKeyword("else")) {
					Exp = null;
					Block = IfStatement.Children[ChildIndex + 1];
				} else if(IfStatement.Children[ChildIndex].Token.IsKeyword("end")) {
					EndOpPos = CurrFunc.Instructions.Count;
					break;
				}

				JumpStartOps.Add(CurrFunc.Instructions.Count);
				Console.WriteLine("START  : {0} ", CurrFunc.Instructions.Count);
				PushLocalScope();

				VList ExpVs = null;
				if(Exp != null) 
					ExpVs = CompileExp(Exp);


				if(ExpVs != null) {
					CompilerValue ExpV = ExpVs[0];
					if(ExpV == null) {
						; // do nothing
					} else {
						// else, check ExpVs for true/false
						if(!(ExpV.IsRegister && !ExpV.IsTable)) {
							CompilerValue UR = GetUnclaimedReg();
							EmitToRegisterOp(UR, ExpV);
							ClaimRegister(UR);
							ExpV = UR;
						}
						FreeRegister(ExpV);
						CurrFunc.Instructions.Add(Instruction.OP.TEST, ExpV.Index, -1, 0);
						CurrFunc.Instructions.Add(Instruction.OP.JMP, 0);
					}
				}
				JumpNextOps.Add(CurrFunc.Instructions.Count-1);
				Console.WriteLine("NEXT   : {0} ", CurrFunc.Instructions.Count-1);				

				CompileBlock(Block);

				// if there are else's, add a jump here
				if(ChildIndex + 4 + 3 /*else block end*/  < IfStatement.Children.Count) {
					CurrFunc.Instructions.Add(Instruction.OP.JMP, 0);
					JumpEndOps.Add(CurrFunc.Instructions.Count-1);
				} else {
					EndOpPos = CurrFunc.Instructions.Count;
				}
				Console.WriteLine("JEND   : {0} ", CurrFunc.Instructions.Count-1);
				PopLocalScope();

				ChildIndex += 4;
			}

			// Fix the JMPs
			for(int i = 0; i < JumpNextOps.Count; i++) {
				if(i == JumpNextOps.Count - 1) {
					CurrFunc.Instructions[JumpNextOps[i]].A = (EndOpPos - JumpNextOps[i])-1;
					Console.WriteLine("JNEXT IS JUMP TO END {0} - {1} = {2}",
					                  EndOpPos, JumpNextOps[i], (EndOpPos - JumpNextOps[i])-1);
				} else {
					int SkipJumpDist = (JumpStartOps[i+1] - JumpNextOps[i])-1;
					CurrFunc.Instructions[JumpNextOps[i]].A = SkipJumpDist;
					Console.WriteLine("JNEXT  {0} - {1} = {2}",
					                  JumpStartOps[i+1], JumpNextOps[i], SkipJumpDist);
				}
			}
			for(int i = 0; i < JumpEndOps.Count; i++) {
				int SkipJumpDist = (EndOpPos - JumpEndOps[i])-1;
				CurrFunc.Instructions[JumpEndOps[i]].A = SkipJumpDist;
				Console.WriteLine("JEND  {0} - {1} = {2}",
				                  EndOpPos, JumpEndOps[i], SkipJumpDist);
			}

		}


		void CompileWhileStmt(ParseNode WhileStatement) {
			// 'while' exp 'do' block 'end' | 


			int StartOp = CurrFunc.Instructions.Count;
			int ExpFailOp = 0;
			int EndOp = 0;

			ParseNode Exp=null, Block=null;

			Exp = WhileStatement.Children[1];
			Block = WhileStatement.Children[3];


			Console.WriteLine("W-START  : {0} ", CurrFunc.Instructions.Count);
			PushLocalScope();

			{{
				VList ExpVs = null;
				if(Exp != null) 
					ExpVs = CompileExp(Exp);


				if(ExpVs != null) {
					CompilerValue ExpV = ExpVs[0];
					if(ExpV == null) {
						; // do nothing
					} else {
						// else, check ExpVs for true/false
						if(!(ExpV.IsRegister && !ExpV.IsTable)) {
							CompilerValue UR = GetUnclaimedReg();
							EmitToRegisterOp(UR, ExpV);
							ClaimRegister(UR);
							ExpV = UR;
						}
						FreeRegister(ExpV);
						CurrFunc.Instructions.Add(Instruction.OP.TEST, ExpV.Index, -1, 0);
						CurrFunc.Instructions.Add(Instruction.OP.JMP, 0);
					}
				}
			}}

			ExpFailOp = CurrFunc.Instructions.Count-1;
			Console.WriteLine("W-FAIL   : {0} ", ExpFailOp);				

			CompileBlock(Block);

			CurrFunc.Instructions.Add(Instruction.OP.JMP, 0);
			EndOp = CurrFunc.Instructions.Count-1;
				
			Console.WriteLine("W-END   : {0} ", CurrFunc.Instructions.Count-1);
			PopLocalScope();

			// Fix the JMPs
			CurrFunc.Instructions[ExpFailOp].A = (EndOp - ExpFailOp);
			Console.WriteLine("W-OUT IS JUMP TO END {0} - {1} = {2}",
			                  EndOp, ExpFailOp, (EndOp - ExpFailOp));

							
			CurrFunc.Instructions[EndOp].A = (StartOp - EndOp)-1;
			Console.WriteLine("W-LOOP  {0} - {1} = {2}",
			                  StartOp, EndOp, (StartOp - EndOp)-1);

		}


		void CompileRepeatStmt(ParseNode RepeatStatement) {
			// 'repeat' block 'until' exp | 

			int StartOp = CurrFunc.Instructions.Count;
			int EndOp = 0;

			ParseNode Exp=null, Block=null;

			Exp = RepeatStatement.Children[3];
			Block = RepeatStatement.Children[1];


			Console.WriteLine("R-START  : {0} ", StartOp);
			PushLocalScope();

			CompileBlock(Block);

			{{
					VList ExpVs = null;
					if(Exp != null) 
						ExpVs = CompileExp(Exp);


					if(ExpVs != null) {
						CompilerValue ExpV = ExpVs[0];
						if(ExpV == null) {
							; // do nothing
						} else {
							// else, check ExpVs for true/false
							if(!(ExpV.IsRegister && !ExpV.IsTable)) {
								CompilerValue UR = GetUnclaimedReg();
								EmitToRegisterOp(UR, ExpV);
								ClaimRegister(UR);
								ExpV = UR;
							}
							FreeRegister(ExpV);
							CurrFunc.Instructions.Add(Instruction.OP.TEST, ExpV.Index, -1, 0);
							CurrFunc.Instructions.Add(Instruction.OP.JMP, 0);
						}
					}
			}}

			EndOp = CurrFunc.Instructions.Count-1;

			Console.WriteLine("R-END   : {0} ", EndOp);


			// Fix the JMPs
			CurrFunc.Instructions[EndOp].A = (StartOp - EndOp)-1;
			Console.WriteLine("R-LOOP  {0} - {1} = {2}",
			                  StartOp, EndOp, (StartOp - EndOp)-1);

			PopLocalScope();
		}


		void CompileForNumericalStmt(ParseNode ForStatement) {
			// 'for' NAME '=' exp ',' exp (',' exp)? 'do' block 'end' |  

			ParseNode VarName = null, 
			          InitExp = null, EndExp = null, StepExp = null, 
			          Block = null; 

			VarName = ForStatement.Children[1];
			InitExp = ForStatement.Children[3];
			EndExp  = ForStatement.Children[5];
			if(ForStatement.Children[6].Token.IsOperator(",")) {
				StepExp = ForStatement.Children[7];
				Block = ForStatement.Children[9];
			} else {
				StepExp = null;
				Block = ForStatement.Children[7];
			}

			PushLocalScope();

			// Alloocate 4 Locals in a row.
			CompilerValue IndexVal, LimitVal, StepVal, VisableValue;
			//IndexVal = GetLocalIndex("(for index)");
			//LimitVal = GetLocalIndex("(for limit)");
			//StepVal = GetLocalIndex("(for step)");
			//VisableValue = GetLocalIndex(VarName.Token.Value);

			IndexVal = CreateLocal("(for index)");
			LimitVal = CreateLocal("(for limit)");
			StepVal = CreateLocal("(for step)");
			VisableValue = CreateLocal(VarName.Token.Value);

			// Assign values to the loop vals

			EmitAssignOp(IndexVal, CompileSingularExp(InitExp, IndexVal));	
            FinalizeLocal(IndexVal);

			EmitAssignOp(LimitVal, CompileSingularExp(EndExp, LimitVal));
			FinalizeLocal(LimitVal);

			if(StepExp == null) {
				CompilerValue DefaultOne = GetConstIndex(1.0);
				EmitAssignOp(StepVal, DefaultOne);
			} else {
				EmitAssignOp(StepVal, CompileSingularExp(StepExp, StepVal));
			}
			FinalizeLocal(StepVal);

			FinalizeLocal(VisableValue);

			int PrepOp = CurrFunc.Instructions.Count;
			int LoopOp = 0;


			Console.WriteLine("FN-PREP  : {0} ", PrepOp);
			CurrFunc.Instructions.Add(Instruction.OP.FORPREP, IndexVal.Index, 0);

			CompileBlock(Block);

			LoopOp = CurrFunc.Instructions.Count;
			CurrFunc.Instructions.Add(Instruction.OP.FORLOOP, IndexVal.Index, 0);
			Console.WriteLine("FN-END   : {0} ", LoopOp);

			// Fix the JMPs
			CurrFunc.Instructions[PrepOp].B = (LoopOp - PrepOp)-1;
			Console.WriteLine("FN-OUT IS JUMP TO END {0} - {1} = {2}",
			                  LoopOp, PrepOp, (LoopOp - PrepOp)-1);

			CurrFunc.Instructions[LoopOp].B = (PrepOp - LoopOp);
			Console.WriteLine("FN-LOOP  {0} - {1} = {2}",
			                  PrepOp, LoopOp, (PrepOp - LoopOp));

			PopLocalScope();

		}


		void CompileForIterStmt(ParseNode ForStatement) {
			// 'for' namelist 'in' explist1 'do' block 'end' | 

			ParseNode NameList, ExpList, Block;
			NameList = ForStatement.Children[1];
			ExpList = ForStatement.Children[3];
			Block = ForStatement.Children[5];


			PushLocalScope();

			// Alloocate 4 Locals in a row.
			CompilerValue GeneratorVal, StateVal, ControlVal;
			GeneratorVal = CreateLocal("(for generator)");
			StateVal = CreateLocal("(for state)");
			ControlVal = CreateLocal("(for control)");
			VList VisableVals = CompileNameList(NameList);

			VList InternalVals = new VList() { GeneratorVal, StateVal, ControlVal };
			VList CalledInternalVals = CompileExpList(ExpList, InternalVals);
			// Assign vals?
			FinalizeLocal(GeneratorVal);
			FinalizeLocal(StateVal);
			FinalizeLocal(ControlVal);

			foreach(CompilerValue LoopLocal in VisableVals) {
				FinalizeLocal(LoopLocal);
			}

			int SkipOp = CurrFunc.Instructions.Count;
			CurrFunc.Instructions.Add(Instruction.OP.JMP, 0);
			Console.WriteLine("FI-SKIP  : {0} ", SkipOp);


			int StartOp = CurrFunc.Instructions.Count;
			int LoopOp = 0;

			Console.WriteLine("FI-START  : {0} ", StartOp);

			CompileBlock(Block);

			LoopOp = CurrFunc.Instructions.Count;
			CurrFunc.Instructions.Add(Instruction.OP.TFORLOOP, GeneratorVal.Index, 0, VisableVals.Count);
			Console.WriteLine("FI-TFORLOOP   : {0} ", LoopOp);
			CurrFunc.Instructions[LoopOp].isB = false;

			int JumpBack = -((LoopOp - SkipOp)+1);
			CurrFunc.Instructions.Add(Instruction.OP.JMP, JumpBack);
			Console.WriteLine("FI-JMP   : {0} ", JumpBack);


			PopLocalScope();

			// Fix Skip
			CurrFunc.Instructions[SkipOp].A = (-JumpBack) - 2;
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

				CompilerValue CurrV = CompileLeftVar(Child);
				Result.Add(CurrV);
			}

			return Result;
		}

		CompilerValue CompileLeftVar(ParseNode Var) {

			CompilerValue Result = new CompilerValue();

			CompilerValue First = null;

			int ChildIndex = 0;
			if(Var.Children[0].Type == ParseNode.EType.TERMINAL &&
				Var.Children[0].Token.Type == Token.EType.NAME) {
				string Name = Var.Children[0].Token.Value; ChildIndex++;
				CompilerValue LocalV = GetLocalIndex(Name, false);
				if(LocalV != null) {
					First = LocalV;
				} else {
					CompilerValue GlobalV = GetGlobalIndex(Name);
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
				CompilerValue Key = null;
				if(VarSuffix.Children[0].Type == ParseNode.EType.TERMINAL &&
					VarSuffix.Children[0].Token.IsBracket("[")) {
					VList SubRs = CompileExp(VarSuffix.Children[1]);
					Key = SubRs[0];
					if(Key.IsTable) {
						CompilerValue TReg = GetUnclaimedReg();
						EmitToRegisterOp(TReg, Key);
						ClaimRegister(TReg);
						Key = TReg;
					}
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
				CompilerValue Curr = new CompilerValue(First);
				// Take first, wrap it in a new Table value
				for(int i = 0; i < Keys.Count; i++) {
					CompilerValue Key = Keys[i];
					Curr.Key = Key;

					if( (i + 1) < Keys.Count) {
						CompilerValue TReg = GetAsRegister(Curr);
						FreeRegister(Curr.Key);
						FreeRegister(Curr);
						Curr = TReg;
					}
					//Console.WriteLine("LV {0}", Curr.ToString());
				}
				Result = Curr;
				Result = ResolveTableValue(Result);
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
					//Value LVal = GetLocalIndex(NameVal);
					CompilerValue LVal = CreateLocal(NameVal);
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
							CompilerValue Dest = GetLRegorTReg(LVals, RVals);
							CurrFunc.Instructions.Add(Instruction.OP.LOADNIL, Dest.Index, Dest.Index);
							Result.Add(Dest);
						} else if(Child.Token.IsKeyword("true")) {
							CompilerValue Dest = GetLRegorTReg(LVals, RVals);
							CurrFunc.Instructions.Add(Instruction.OP.LOADBOOL, Dest.Index, 1, 0);
							Result.Add(Dest);
						} else if(Child.Token.IsKeyword("false")) {
							CompilerValue Dest = GetLRegorTReg(LVals, RVals);
							CurrFunc.Instructions.Add(Instruction.OP.LOADBOOL, Dest.Index, 0, 0);
							Result.Add(Dest);
						}


					} else if(Child.Token.Type == Token.EType.NUMBER) {
						double NumValue = 0.0;
						Double.TryParse(Child.Token.Value, out NumValue);
						CompilerValue CVal = GetConstIndex(NumValue);
						Result.Add(CVal);
						//Value Dest = GetLRegorTReg(LVals, RVals);
						//EmitAssignOp(Dest, CVal);
						//Result.Add(Dest);
					} else if(Child.Token.Type == Token.EType.STRING) {
						CompilerValue CVal = GetConstIndex(Child.Token.Value);
						Result.Add(CVal);
						//Value Dest = GetLRegorTReg(LVals, RVals);
						//EmitAssignOp(Dest, CVal);
						//Result.Add(Dest);
					}
					// '...' ???
				} else if(Child.Type == ParseNode.EType.UN_OP_WRAP) {
					Result.Add(CompileUnOp(Child, LVals, RVals));
				} else if(Child.Type == ParseNode.EType.MATH_BIN_OP_WRAP) {
					Result.Add(CompileMathBinOp(Child, LVals, RVals));
				} else if(Child.Type == ParseNode.EType.COMP_BIN_OP_WRAP) {
					Result.Add(CompileCompBinOp(Child, LVals, RVals));
				} else if(Child.Type == ParseNode.EType.PREFIX_EXP) {
					// might be a var, might be a function call
					Result.AddRange(CompilePrefixExp(Child, LVals, RVals));
				} else if(Child.Type == ParseNode.EType.TABLE_CONS) {
					Result.Add(CompileTableConstructor(Child, LVals, RVals));
				}
			}

			return Result;
		}

		CompilerValue CompileSingularExp(ParseNode Exp, CompilerValue LVal) {
			VList LVals = null;
			if(LVal != null) {
				LVals = new VList();
				LVals.Add(LVal);
			}

			VList Results = null;
			Results = CompileExp(Exp, LVals);

			if(Results == null)
				return null;
			else if(Results.Count == 0) 
				return null;
			else
				return Results[0];
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

				CompilerValue FuncVal = Result[Result.Count - 1];
				Result.RemoveAt(Result.Count - 1);

				FreeRegister(FuncVal);


				VList StackSpace;
				if(FuncVal.IsRegister)
					StackSpace = GetStackFrame(FuncVal.Index, StackSpaceNeeded);
				else 
					StackSpace = GetStackFrame(0, StackSpaceNeeded);
				EmitAssignOp(StackSpace[0], FuncVal);
				FuncVal = StackSpace[0];

				Console.WriteLine(" FV:{0} ", FuncVal.ToString());

				//  Allocate LVals for args? Pass into CompileArgs?
				VList ArgRegs = new VList(ArgCount);
				for(int i =0; i < ArgCount; i++) 
					ArgRegs.Add(StackSpace[i+1]);
				VList ArgVs = CompileArgs(Args, ArgRegs);
				for(int i =0; i < ArgCount; i++)  {
					EmitAssignOp(ArgRegs[i], ArgVs[i]);
					ClaimRegister(ArgRegs[i]);
				}
				// Claim ArgRegs
				ClaimStackFrame(ArgRegs);

				CurrFunc.Instructions.Add(Instruction.OP.CALL, FuncVal.Index, ArgCount+1, RetCount+1);
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
				CompilerValue VarVal = null;
				{{
					CompilerValue LocalV = GetLocalIndex(Name, false);
					if(LocalV != null) {
						VarVal = LocalV;
					} else {
						//bool IsGlobal = CheckGlobal(Name);
						CompilerValue GlobalV = GetGlobalIndex(Name);
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
						CompilerValue Key = null;
						if(VarSuffix.Children[0].Type == ParseNode.EType.TERMINAL &&
						   VarSuffix.Children[0].Token.IsBracket("[")) {
							VList SubRs = CompileExp(VarSuffix.Children[1]);
							Key = SubRs[0];
							if(Key.IsTable){
								CompilerValue TReg = GetUnclaimedReg();
								EmitToRegisterOp(TReg, Key);
								ClaimRegister(TReg);
								Key = TReg;
							}
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
						CompilerValue Curr = new CompilerValue(VarVal);
						// Take first, wrap it in a new Table value
						for(int i = 0; i < Keys.Count; i++) {
							CompilerValue Key = Keys[i];
							Curr.Key = Key;

							if( (i + 1) < Keys.Count) {
								CompilerValue TReg = GetAsRegister(Curr);
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

		CompilerValue CompileTableConstructor(ParseNode TableCons, VList LVals=null, VList RVals=null) {
			// tableconstructor : '{' (fieldlist)? '}';
			//fieldlist : field (fieldsep field)* (fieldsep)?;
			//field : '[' exp ']' '=' exp | NAME '=' exp | exp;
			//fieldsep : ',' | ';';| ';';

			// Detect Child count.
			// Detect Array or Hash type
			ParseNode FieldList = null;

			Console.WriteLine(" TC.C: {0} ", TableCons.Children.Count);
			if(TableCons.Children.Count > 2) {
				FieldList = TableCons.Children[1];
				Console.WriteLine(" FL.C: {0} ", FieldList.Children.Count);
			}

			int ArrayCount = 0, HashCount = 0;
			if(FieldList != null) {
				foreach(ParseNode Field in FieldList.Children) {
					if(Field.Type != ParseNode.EType.FIELD) // skip the seperators
						continue; 
					if(Field.Children.Count == 1) 
						ArrayCount++;
					else if(Field.Children.Count == 3) // blah = exp
						HashCount++;
				}
			}


			// always create to a fresh register
			CompilerValue ConsReg = GetLRegorTReg();		

			CurrFunc.Instructions.Add(Instruction.OP.NEWTABLE, ConsReg.Index, ArrayCount, HashCount); // FIXME

			// Build Fields
			// Get Registers for Arrays
			if(FieldList != null) {
				VList ArrayVals = null;
				int ArrayIndex = 0; // next Array index to use
				if(ArrayCount > 0)
					ArrayVals = UnclaimedConsecutiveRegisters(ConsReg.Index+1, ArrayCount);

				foreach(ParseNode Field in FieldList.Children) {
					if(Field.Type != ParseNode.EType.FIELD) // skip the seperators
						continue; 
					if(Field.Children.Count == 1) { 
						// Process exp into Treg, track for setlist
						VList T = CompileExp(Field.Children[0], ArrayVals, null);
						if(T.Count > 1) {
							; // FIXME
						}
						EmitAssignOp(ArrayVals[ArrayIndex], T[0]);
						ClaimRegister(ArrayVals[ArrayIndex]);
						ArrayIndex++;
					}
					else if(Field.Children.Count == 3)  {// blah = exp
						// Process, SETTABLE.
						// Process Left make key, Process Right
						ParseNode KeyNode = Field.Children[0];
						ParseNode ValNode = Field.Children[2];

						CompilerValue KeyVal = null;
						if(KeyNode.Type == ParseNode.EType.TERMINAL && 
							KeyNode.Token.Type == Token.EType.NAME) {
							KeyVal = GetConstIndex(KeyNode.Token.Value);
						} else if(KeyNode.Type == ParseNode.EType.TERMINAL &&
							KeyNode.Token.IsBracket("[")) {
							VList T = CompileExp(KeyNode.Children[1]);
							KeyVal = T[0];
						}
						ConsReg.Key = KeyVal;

						VList Values = CompileExp(ValNode);

						EmitAssignOp(ConsReg, Values[0]);
						FreeRegister(ConsReg.Key);
						FreeRegister(Values[0]);
					}
				}

				if(ArrayCount > 0) {
					CurrFunc.Instructions.Add(Instruction.OP.SETLIST, ConsReg.Index, ArrayCount, 1);
					// FIXME: SETLIST pages, 50+? Move into Loop
					foreach(CompilerValue Val in ArrayVals) {
						FreeRegister(Val);
					}
				}
			}

			// Setlist

			return ConsReg;
		}

		CompilerValue CompileMathBinOp(ParseNode BinOp, VList LVals = null, VList RVals = null) {

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


			// FIXME: Handle input values as Reg-or-Consts

			CompilerValue LV = LVs[0];
			CompilerValue RV = RVs[0];
			
			if(!LV.IsRegisterOrConstant || LV.IsTable)
				LV = GetAsRegister(LV);
			if(!RV.IsRegisterOrConstant || RV.IsTable)
				RV = GetAsRegister(RV);


			FreeRegister(LV);
			FreeRegister(RV);

			//Value Result = new Value(GetFreeRegister(), Value.ELoc.REGISTER, Value.ESide.RIGHT);
			CompilerValue Result = GetLRegorTReg(LVals, RVals);

			CurrFunc.Instructions.Add(InstOp, Result.Index, LV.Index, LV.IsConstant, RV.Index, RV.IsConstant);

			return Result;
		}

		CompilerValue CompileCompBinOp(ParseNode BinOp, VList LVals = null, VList RVals = null) {
			//  EQ A B C if ((RK(B) == RK(C)) ~= A) then PC++
			//  LT A B C if ((RK(B) <  RK(C)) ~= A) then PC++
			//  LE A B C if ((RK(B) <= RK(C)) ~= A) then PC++

			ParseNode Left = BinOp.Children[0];
			ParseNode Op = BinOp.Children[1];
			ParseNode Right = BinOp.Children[2];


			VList LVs = CompileExp(Left);
			VList RVs = CompileExp(Right);


			Instruction.OP InstOp = Instruction.OP.ERROR;
			if(Op.Token.IsOperator("<"))
				InstOp = Instruction.OP.LT;
			else if(Op.Token.IsOperator("<="))
				InstOp = Instruction.OP.LE;
			else if(Op.Token.IsOperator(">"))
				InstOp = Instruction.OP.LT;
			else if(Op.Token.IsOperator(">="))
				InstOp = Instruction.OP.LE;
			else if(Op.Token.IsOperator("=="))
				InstOp = Instruction.OP.EQ;
			else if(Op.Token.IsOperator("~="))
				InstOp = Instruction.OP.EQ;


			CompilerValue LV = LVs[0], RV = RVs[0];

			if(!LV.IsRegisterOrConstant || LV.IsTable)
				LV = GetAsRegister(LV);
			if(!RV.IsRegisterOrConstant || RV.IsTable)
				RV = GetAsRegister(RV);

			int CompValue = 0;

			if(Op.Token.IsOperator(">") || Op.Token.IsOperator(">=")) {
				CompilerValue T = LV;
				LV = RV;
				RV = T;
			}
			if(Op.Token.IsOperator("~=")) {
				CompValue = 1;
			}

			FreeRegister(LV);
			FreeRegister(RV);

			//Value Result = new Value(GetFreeRegister(), Value.ELoc.REGISTER, Value.ESide.RIGHT);
			//Value Result = GetLRegorTReg(LVals, RVals);

			CompilerValue DestVal = null;
			if(LVals != null && RVals != null && LVals.Count > RVals.Count) {
				DestVal = LVals[RVals.Count];
				CompValue = 1;
			}



			//int JumpDist = 0; // this is going to get over-written in the If/Loop work
			CurrFunc.Instructions.Add(InstOp, CompValue, LV.Index, LV.IsConstant, RV.Index, RV.IsConstant);
				


			if(DestVal == null) {
				CurrFunc.Instructions.Add(Instruction.OP.JMP, 0);
			} else if(DestVal != null) {
				CurrFunc.Instructions.Add(Instruction.OP.JMP, 1);
				Console.WriteLine(" CBO: LReg: {0}", DestVal.ToString());
				CompilerValue TReg = GetUnclaimedReg();
				CurrFunc.Instructions.Add(Instruction.OP.LOADBOOL, TReg.Index, 0, 1); // skip next 
				CurrFunc.Instructions.Add(Instruction.OP.LOADBOOL, TReg.Index, 1, 0);
				EmitFromRegisterOp(DestVal, TReg);
			}

			return DestVal;
		}

		CompilerValue CompileUnOp(ParseNode UnOp, VList LVals = null, VList RVals = null) {

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

			CompilerValue RV = GetAsRegister(RVs[0]);
			FreeRegister(RV);

			CompilerValue Result = GetLRegorTReg(LVals, RVals);

			CurrFunc.Instructions.Add(InstOp, Result.Index, RV.Index);

			return Result;
		}

	}
}
