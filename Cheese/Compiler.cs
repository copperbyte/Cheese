
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
		internal int Index;

		internal double NumberVal;
		internal string StringVal;
	}

	class Value {
		public enum ESide {
			LEFT,
			RIGHT
		}
		public enum ELoc {
			LOCAL,
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
		internal List<Instruction> Instructions;
	
		public Function() {
			UsedRegs = new SortedSet<int>();
			ConstantTable = new List<ConstEntry>();
			Instructions = new List<Instruction>();
		}

		internal void PrintConstants() {
			foreach(ConstEntry Const in ConstantTable) {
				if(Const.StringVal != null)
					Console.WriteLine("CS {0} := {1}", Const.Index, Const.StringVal);
				else
					Console.WriteLine("CN {0} := {1}", Const.Index, Const.NumberVal);
			}
		}

		internal void PrintInstructions() {
			foreach(Instruction Inst in Instructions) {
				Console.WriteLine("I  {0}  {1}  {2}  {3}", Inst.Code, Inst.A, Inst.B, Inst.C);
			}
		}
	}


	class Compiler
	{ 

		Function CurrFunc;
		List<Function> Functions;


		public Compiler()
		{
			Functions = new List<Function>();

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

			Functions.Insert(0, RootFunc);
			RootFunc.PrintConstants();
			RootFunc.PrintInstructions();

			foreach(Function Func in Functions) {
				Func.PrintConstants();
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

		void FreeRegister(int Reg) {
			CurrFunc.UsedRegs.Remove(Reg);
		}

		int GetConstIndex(string Value) {
			foreach(ConstEntry Entry in CurrFunc.ConstantTable) {
				if(Entry.StringVal != null && Entry.StringVal == Value) {
					return Entry.Index;
				}
			}
			ConstEntry NewEntry = new ConstEntry();
			NewEntry.Index = CurrFunc.ConstantTable.Count;
			NewEntry.StringVal = Value;
			CurrFunc.ConstantTable.Add(NewEntry);
			return NewEntry.Index;
		}

		int GetConstIndex(double Value) {
			foreach(ConstEntry Entry in CurrFunc.ConstantTable) {
				if(Entry.StringVal == null && Entry.NumberVal == Value) 
					return Entry.Index;
			}
			ConstEntry NewEntry = new ConstEntry();
			NewEntry.Index = CurrFunc.ConstantTable.Count;
			NewEntry.NumberVal = Value;
			CurrFunc.ConstantTable.Add(NewEntry);
			return NewEntry.Index;
		}

		void CompileBlock(ParseNode Block) {
			if(Block.Children == null)
				return;

			foreach(ParseNode Statement in Block.Children) {
				if(Statement.Type == ParseNode.EType.ASSIGN_STAT)
					CompileAssignmentStmt(Statement);

				CurrFunc.UsedRegs.Clear();
			}

		}


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

			// VList LVs = CompileLeftVarList(Left);
			VList RVs = CompileExpList(Right);

			for(int I = 0; I < Left.Children.Count; I++) {
				ParseNode CL;
				CL = Left.Children[I];

				string LV = CL.GetTerminal().Value;


				string Name = LV;
				int NameIndex = GetConstIndex(Name);


				// is LV a global, a local, an upvalue, or a tableval? 
				Instruction Inst = new Instruction(Instruction.OP.SETGLOBAL, RVs[I].Index, NameIndex);
				CurrFunc.Instructions.Add(Inst);
			}

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
			VList Result = new VList(2);

			if(Exp.Type == ParseNode.EType.PREFIX_EXP)
				return CompilePrefixExp(Exp);

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

						int CIndex = GetConstIndex(NumValue);
						int DReg = GetFreeRegister();

						CurrFunc.Instructions.Add(Instruction.OP.LOADK, DReg, CIndex);

						Result.Add(new Value(DReg, Value.ELoc.REGISTER, Value.ESide.RIGHT));
					} else if(Child.Token.Type == Token.EType.STRING) {
						int CIndex = GetConstIndex(Child.Token.Value);
						int DReg = GetFreeRegister();

						CurrFunc.Instructions.Add(Instruction.OP.LOADK, DReg, CIndex);

						Result.Add(new Value(DReg, Value.ELoc.REGISTER, Value.ESide.RIGHT));
					}
					// '...' ???
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
				int NameIndex = GetConstIndex(Name);

				int DestReg = GetFreeRegister();

				Result.Add(new Value(DestReg, Value.ELoc.REGISTER, Value.ESide.RIGHT));

				CurrFunc.Instructions.Add(Instruction.OP.GETGLOBAL, DestReg, NameIndex);
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

			FreeRegister(LV[0].Index);
			FreeRegister(RV[0].Index);

			Value Result = new Value(GetFreeRegister(), Value.ELoc.REGISTER, Value.ESide.RIGHT);

			CurrFunc.Instructions.Add(InstOp, Result.Index, LV[0].Index, RV[0].Index);

			return Result;
		}

	}
}

