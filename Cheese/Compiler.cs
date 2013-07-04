
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

		public Instruction() { ; }
		public Instruction(OP code) { Code = code; }
		public Instruction(OP code, int a) { Code = code; A = a; }
		public Instruction(OP code, int a, int b) { Code = code; A = a; B = b;}
		public Instruction(OP code, int a, int b, int c) { Code = code; A = a; B = b; C = c; }


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

		public Value(int Index=0, ELoc Loc=ELoc.REGISTER, ESide Side=ESide.RIGHT) {
			this.Index = Index; 
			this.Loc = Loc;
			this.Side = Side;
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
				Console.WriteLine("IC  {0}  {1}  {2}  {3}", Inst.Code, Inst.A, Inst.B, Inst.C);
			}
		}
	}


	class Compiler
	{ 

		Function CurrFunc;
		List<Function> Functions;
		Stack<Function> FunctionStack;

		SortedSet<string> Globals;


		public Compiler()
		{
			Functions = new List<Function>();
			FunctionStack = new Stack<Function>();
			Globals = new SortedSet<string>();
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
			return Globals.Contains(Name);
		}

		void AddGlobal(string Name) {
			Globals.Add(Name);
		}

		Value GetLocalIndex(string Name, bool Create=true) {
			foreach(LocalEntry Entry in CurrFunc.LocalTable) {
				if(Entry.Name != null && Entry.Name == Name) {
					return Entry.Value;
				}
			}
			if(Create) {
				int FreeReg = GetFreeRegister();
				LocalEntry NewEntry = new LocalEntry();
				NewEntry.Value.Index = FreeReg;//CurrFunc.LocalTable.Count;
				NewEntry.Value.Loc = Value.ELoc.LOCAL;
				NewEntry.Name = Name;
				CurrFunc.LocalTable.Add(NewEntry);
				return NewEntry.Value;
			} else {
				return null;
			}
		}


		void CompileBlock(ParseNode Block) {
			if(Block.Children == null)
				return;

			foreach(ParseNode Statement in Block.Children) {
				if(Statement.Type == ParseNode.EType.ASSIGN_STAT)
					CompileAssignmentStmt(Statement);
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
			VList RVs = CompileExpList(Right);

			int MaxCount = Math.Max(LVs.Count, RVs.Count);
			for(int I = 0; I < MaxCount; I++) {
				Value LVal=null, RVal=null;

				if(LVs.Count > I)
					LVal = LVs[I];
				if(RVs.Count > I)
					RVal = RVs[I];

				if(RVal != null)
					FreeRegister(RVal);

				if(LVal == null || RVal == null)
					continue;

				// is LV a global, a local, an upvalue, or a tableval? 
				if(LVal.Loc == Value.ELoc.GLOBAL || LVal.Loc == Value.ELoc.CONSTANT) {
					CurrFunc.Instructions.Add(Instruction.OP.SETGLOBAL, RVal.Index, LVal.Index);
				}
				else if(LVal.Loc == Value.ELoc.LOCAL || LVal.Loc == Value.ELoc.REGISTER) {
					if(LVal.Index != RVal.Index)
						CurrFunc.Instructions.Add(Instruction.OP.MOVE, LVal.Index, RVal.Index);
				}
				// set table
				// set upval
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

			VList RVs = CompileExpList(Right);

			int MaxCount = Math.Max(LVs.Count, RVs.Count);
			for(int I = 0; I < MaxCount; I++) {
				Value LVal=null, RVal=null;

				if(LVs.Count > I)
					LVal = LVs[I];
				if(RVs.Count > I)
					RVal = RVs[I];

				if(RVal != null)
					FreeRegister(RVal);

				// is LV a local 
				if(LVal != null && RVal != null) {
					// Skip self-move~
					if(LVal.Index != RVal.Index)
						CurrFunc.Instructions.Add(Instruction.OP.MOVE, LVal.Index, RVal.Index);
				}
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

			Value GVal = GetConstIndex(FuncNameStr);
			Value ClosureReg = new Value(GetFreeRegister());
			int UpValCount = 0;
			CurrFunc.Instructions.Add(Instruction.OP.CLOSURE, ClosureReg.Index, UpValCount);
			CurrFunc.Instructions.Add(Instruction.OP.SETGLOBAL, ClosureReg.Index, GVal.Index);
			FreeRegister(GVal);
			FreeRegister(ClosureReg);
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

			if(Var.Children[0].Type == ParseNode.EType.TERMINAL &&
				Var.Children[0].Token.Type == Token.EType.NAME) {
				string Name = Var.Children[0].Token.Value;
				Value LocalV = GetLocalIndex(Name, false);
				if(LocalV != null) {
					First = LocalV;
				} else {
					bool IsGlobal = CheckGlobal(Name);
					Value GlobalV = null;
					if(IsGlobal) {
						GlobalV = GetConstIndex(Name);
					} else {
						AddGlobal(Name);
						GlobalV = GetConstIndex(Name);
					}
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
			}


			// More Suffix Stuff
			Result = First;
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

		VList CompileExpList(ParseNode ExpList) {
			if(ExpList.Children == null)
				return null;

			VList Result = new VList(8);

			foreach(ParseNode Child in ExpList.Children) {
				if(Child.Type != ParseNode.EType.EXP)
					continue;

				VList CurrVs = CompileExp(Child);
				Result.AddRange(CurrVs);
			}

			return Result;
		}

		VList CompileExp(ParseNode Exp) {

			// sometimes CompileExp will be called, but Exp will actually 
			//  be a node right under an exp, 
			// FIXME (wrap and call on fake-parent?)
			if(Exp.Type != ParseNode.EType.EXP) {
				ParseNode Temp = new ParseNode(ParseNode.EType.EXP);
				Temp.Add(Exp);
				return CompileExp(Temp);
			}
			//if(Exp.Type == ParseNode.EType.PREFIX_EXP)
			//	return CompilePrefixExp(Exp);
			//else if(Exp.Type == ParseNode.EType.UN_OP_WRAP) 
			//	return new VList() { CompileUnOp(Exp) };

			VList Result = new VList(2);

			foreach(ParseNode Child in Exp.Children) {
				if(Child.Type == ParseNode.EType.TERMINAL) {
					if(Child.Token.Type == Token.EType.KEYWORD) {
						if(Child.Token.IsKeyword("nil")) {
							int DReg = GetFreeRegister();
							CurrFunc.Instructions.Add(Instruction.OP.LOADNIL, DReg, DReg);
							Result.Add(new Value(DReg, Value.ELoc.REGISTER, Value.ESide.RIGHT));
						} else if(Child.Token.IsKeyword("true")) {
							int DReg = GetFreeRegister();
							CurrFunc.Instructions.Add(Instruction.OP.LOADBOOL, DReg, 1, 0);
							Result.Add(new Value(DReg, Value.ELoc.REGISTER, Value.ESide.RIGHT));
						} else if(Child.Token.IsKeyword("false")) {
							int DReg = GetFreeRegister();
							CurrFunc.Instructions.Add(Instruction.OP.LOADBOOL, DReg, 0, 0);
							Result.Add(new Value(DReg, Value.ELoc.REGISTER, Value.ESide.RIGHT));
						}


					} else if(Child.Token.Type == Token.EType.NUMBER) {
						double NumValue = 0.0;
						Double.TryParse(Child.Token.Value, out NumValue);

						Value CVal = GetConstIndex(NumValue);
						int DReg = GetFreeRegister();

						CurrFunc.Instructions.Add(Instruction.OP.LOADK, DReg, CVal.Index);

						Result.Add(new Value(DReg, Value.ELoc.REGISTER, Value.ESide.RIGHT));
					} else if(Child.Token.Type == Token.EType.STRING) {
						Value CVal = GetConstIndex(Child.Token.Value);
						int DReg = GetFreeRegister();

						CurrFunc.Instructions.Add(Instruction.OP.LOADK, DReg, CVal.Index);

						Result.Add(new Value(DReg, Value.ELoc.REGISTER, Value.ESide.RIGHT));
					}
					// '...' ???
				} else if(Child.Type == ParseNode.EType.UN_OP_WRAP) {
					Result.Add(CompileUnOp(Child));
				} else if(Child.Type == ParseNode.EType.BIN_OP_WRAP) {
					Result.Add(CompileBinOp(Child));
				} else if(Child.Type == ParseNode.EType.PREFIX_EXP) {
					Result.AddRange(CompilePrefixExp(Child));
				}
			}

			return Result;
		}

		VList CompilePrefixExp(ParseNode PrefixExp) {
			// prefixexp: varOrExp nameAndArgs*;
			VList Result = new VList();

			// varOrExp: var | '(' exp ')';
			ParseNode VarOrExp = PrefixExp.Children[0];
			if(VarOrExp.Children[0].Type == ParseNode.EType.VAR) {

				string Name = VarOrExp.Children[0].GetTerminal().Value; // FIXME complicated names
				Value VarVal = null;
				{
					{
						Value LocalV = GetLocalIndex(Name, false);
						if(LocalV != null) {
							VarVal = LocalV;
						} else {
							bool IsGlobal = CheckGlobal(Name);
							Value GlobalV = GetConstIndex(Name);
							VarVal = GlobalV;
						}
					}}

				if(VarVal.Loc == Value.ELoc.REGISTER || VarVal.Loc == Value.ELoc.LOCAL) {
					Result.Add(VarVal);
				} else {
					int DestReg = GetFreeRegister();
					Result.Add(new Value(DestReg, Value.ELoc.REGISTER, Value.ESide.RIGHT));
					CurrFunc.Instructions.Add(Instruction.OP.GETGLOBAL, DestReg, VarVal.Index);
				}
			} else { // assume ( exp )
				ParseNode Exp = VarOrExp.Children[1];
				Result = CompileExp(Exp);
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
					Args = NameAndArgs.Children[2];
				} else {
					Args = NameAndArgs.Children[0];
				}

				VList ArgVs = CompileArgs(Args);

				// Set up the call?
				Value FuncVal = Result[Result.Count - 1];
				Result.RemoveAt(Result.Count - 1);

				int RetCount = 0; // FIXME How do I know this? Count LVals? That isn't a parameter!!
				CurrFunc.Instructions.Add(Instruction.OP.CALL, FuncVal.Index, ArgVs.Count+1, RetCount+1);
			}

			return Result;
		}

		VList CompileArgs(ParseNode Args) {
			// args :  '(' (explist1)? ')' | tableconstructor | string ;
			VList Result = null;

			if (Args.Children[0].Type == ParseNode.EType.TERMINAL &&
				Args.Children[0].Token.IsBracket("(")) {
				if(Args.Children[1].Type == ParseNode.EType.EXP_LIST) {
					Result = CompileExpList(Args.Children[1]);
				}
			} else if(Args.Children[0].Type == ParseNode.EType.TABLE_CONS) {
				; // FIXME
			} else if(Args.Children[0].Type == ParseNode.EType.TERMINAL &&
				Args.Children[0].Token.Type == Token.EType.STRING) {
				; // FIXME
			}

			return Result;
		}

		Value CompileBinOp(ParseNode BinOp) {

			ParseNode Left = BinOp.Children[0];
			ParseNode Op = BinOp.Children[1];
			ParseNode Right = BinOp.Children[2];


			VList LV = CompileExp(Left);
			VList RV = CompileExp(Right);


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

			FreeRegister(LV[0]);
			FreeRegister(RV[0]);

			Value Result = new Value(GetFreeRegister(), Value.ELoc.REGISTER, Value.ESide.RIGHT);

			CurrFunc.Instructions.Add(InstOp, Result.Index, LV[0].Index, RV[0].Index);

			return Result;
		}

		Value CompileUnOp(ParseNode UnOp) {

			ParseNode Op = UnOp.Children[0];
			ParseNode Right = UnOp.Children[1];

			VList RV = CompileExp(Right);


			Instruction.OP InstOp = Instruction.OP.ERROR;
			if(Op.Token.IsOperator("-"))
				InstOp = Instruction.OP.UNM;
			else if(Op.Token.IsKeyword("not"))
				InstOp = Instruction.OP.NOT;
			else if(Op.Token.IsOperator("#"))
				InstOp = Instruction.OP.LEN;

			FreeRegister(RV[0]);

			Value Result = new Value(GetFreeRegister(), Value.ELoc.REGISTER, Value.ESide.RIGHT);

			CurrFunc.Instructions.Add(InstOp, Result.Index, RV[0].Index);

			return Result;
		}

	}
}

