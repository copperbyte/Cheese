
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

		internal SortedSet<int> UsedRegs;
		internal List<ConstEntry> ConstantTable;
		internal List<LocalEntry> LocalTable;
		internal List<Instruction> Instructions;
	
		public Function() {
			UsedRegs = new SortedSet<int>();
			ConstantTable = new List<ConstEntry>();
			LocalTable = new List<LocalEntry>();
			Instructions = new List<Instruction>();
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

		SortedSet<string> Globals;


		public Compiler()
		{
			Functions = new List<Function>();
			Globals = new SortedSet<string>();
		}


		public void Compile(ParseNode RootChunk) 
		{
			if(RootChunk.Children == null)
				return;

			Function RootFunc = new Function();
			CurrFunc = RootFunc;

			foreach(ParseNode Child in RootChunk.Children) {
				if(Child.Type == ParseNode.EType.BLOCK)
					CompileBlock(Child);
			}

			// all functions, even chunk root, end with a return
			RootFunc.Instructions.Add(Instruction.OP.RETURN, 0, 1);

			Functions.Insert(0, RootFunc);

			foreach(Function Func in Functions) {
				Func.PrintConstants();
				Func.PrintLocals();
				Func.PrintInstructions();
			}
		}




		int GetFreeRegister() {
			int Result = 0;
			while(CurrFunc.UsedRegs.Contains(Result)) {
				Result++;
			}
			CurrFunc.UsedRegs.Add(Result);
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
				else if(Statement.Type == ParseNode.EType.LOCAL_ASSIGN_STAT) 
					CompileLocalAssignStmt(Statement);

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
			// this is essentially CompileNameList right here
			VList LVs = new VList((Left.Children.Count / 2) + 1);
			foreach(ParseNode Name in Left.Children) {
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

		VList CompileExpList(ParseNode ExpList) {
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
				Temp.Children.Add(Exp);
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
			VList Result = new VList();

			ParseNode VarOrExp = PrefixExp.Children[0];
			if(VarOrExp.Children[0].Type == ParseNode.EType.VAR) {

				string Name = VarOrExp.Children[0].GetTerminal().Value; // FIXME complicated names
				Value CVal = GetConstIndex(Name);

				int DestReg = GetFreeRegister();

				Result.Add(new Value(DestReg, Value.ELoc.REGISTER, Value.ESide.RIGHT));

				CurrFunc.Instructions.Add(Instruction.OP.GETGLOBAL, DestReg, CVal.Index);
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

