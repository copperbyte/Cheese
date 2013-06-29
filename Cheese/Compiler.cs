
using System;
//using System.IO;
//using System.Text;
using System.Collections.Generic;

namespace Cheese
{
	using VList = List<Value>;

	class Instruction {
		public enum OP {
			ERROR,
			LOADK,
			SETGLOBAL,
			GETGLOBAL,
			ADD,
			SUB,
			CALL,
			RETURN
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
	}




	class Compiler
	{ 



		SortedSet<int> UsedRegs;
		List<ConstEntry> ConstantTable;

		List<Instruction> Instructions;
		public Compiler()
		{

			UsedRegs = new SortedSet<int>();
			ConstantTable = new List<ConstEntry>();
		
			Instructions = new List<Instruction>();
		}


		public void Compile(ParseNode RootChunk) 
		{
			if(RootChunk.Children == null)
				return;

			foreach(ParseNode Child in RootChunk.Children) {
				if(Child.Type == ParseNode.EType.BLOCK)
					CompileBlock(Child);
			}

			PrintConstants();
			PrintInstructions();
		}

		void PrintConstants() {
			foreach(ConstEntry Const in ConstantTable) {
				if(Const.StringVal != null)
					Console.WriteLine("CS {0} := {1}", Const.Index, Const.StringVal);
				else
					Console.WriteLine("CN {0} := {1}", Const.Index, Const.NumberVal);
			}
		}

		void PrintInstructions() {
			foreach(Instruction Inst in Instructions) {
				Console.WriteLine("I  {0}  {1}  {2}  {3}", Inst.Code, Inst.A, Inst.B, Inst.C);
			}
		}


		int GetFreeRegister() {
			int Result = 0;
			while(UsedRegs.Contains(Result)) {
				Result++;
			}
			UsedRegs.Add(Result);
			return Result;
		}

		void FreeRegister(int Reg) {
			UsedRegs.Remove(Reg);
		}

		int GetConstIndex(string Value) {
			foreach(ConstEntry Entry in ConstantTable) {
				if(Entry.StringVal != null && Entry.StringVal == Value) {
					return Entry.Index;
				}
			}
			ConstEntry NewEntry = new ConstEntry();
			NewEntry.Index = ConstantTable.Count;
			NewEntry.StringVal = Value;
			ConstantTable.Add(NewEntry);
			return NewEntry.Index;
		}

		int GetConstIndex(double Value) {
			foreach(ConstEntry Entry in ConstantTable) {
				if(Entry.StringVal == null && Entry.NumberVal == Value) 
					return Entry.Index;
			}
			ConstEntry NewEntry = new ConstEntry();
			NewEntry.Index = ConstantTable.Count;
			NewEntry.NumberVal = Value;
			ConstantTable.Add(NewEntry);
			return NewEntry.Index;
		}

		void CompileBlock(ParseNode Block) {
			if(Block.Children == null)
				return;

			foreach(ParseNode Statement in Block.Children) {
				if(Statement.Type == ParseNode.EType.ASSIGN_STAT)
					CompileAssignmentStmt(Statement);

				UsedRegs.Clear();
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
				Instructions.Add(Inst);
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

					} else if(Child.Token.Type == Token.EType.NUMBER) {
						double NumValue = 0.0;
						Double.TryParse(Child.Token.Value, out NumValue);

						int CIndex = GetConstIndex(NumValue);
						int DReg = GetFreeRegister();

						Instruction LoadK = new Instruction(Instruction.OP.LOADK, DReg, CIndex);
						Instructions.Add(LoadK);

						Value CValue = new Value();
						CValue.Loc = Value.ELoc.REGISTER;
						CValue.Index = DReg;
						CValue.Side = Value.ESide.RIGHT;

						Result.Add(CValue);
					} else if(Child.Token.Type == Token.EType.STRING) {
						int CIndex = GetConstIndex(Child.Token.Value);
						int DReg = GetFreeRegister();

						Instruction LoadK = new Instruction(Instruction.OP.LOADK, DReg, CIndex);
						Instructions.Add(LoadK);

						Value CValue = new Value();
						CValue.Loc = Value.ELoc.REGISTER;
						CValue.Index = DReg;
						CValue.Side = Value.ESide.RIGHT;

						Result.Add(CValue);
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


				string Name = VarOrExp.Children[0].GetTerminal().Value;
				int NameIndex = GetConstIndex(Name);

				int DestReg = GetFreeRegister();
				Value RVal = new Value();
				RVal.Index = DestReg;
				RVal.Loc = Value.ELoc.REGISTER;
				RVal.Side = Value.ESide.RIGHT;
				Result.Add(RVal);

				Instruction Inst = new Instruction(Instruction.OP.GETGLOBAL, DestReg, NameIndex);
				Instructions.Add(Inst);
			}

			return Result;
		}

		Value CompileBinOp(ParseNode BinOp) {

			ParseNode Left = BinOp.Children[0];
			ParseNode Op = BinOp.Children[1];
			ParseNode Right = BinOp.Children[2];


			VList LV = CompileExp(Left);
			VList RV = CompileExp(Right);

			//Console.WriteLine("t{0} := {1} {2} {3}", T, LV[0], Op.Token.Value, RV[0]);


			Instruction.OP InstOp = Instruction.OP.ERROR;
			if(Op.Token.IsOperator("+"))
				InstOp = Instruction.OP.ADD;
			else if(Op.Token.IsOperator("-"))
				InstOp = Instruction.OP.SUB;

			Value Result = new Value();
			Result.Index = GetFreeRegister();
			Result.Loc = Value.ELoc.REGISTER;
			Result.Side = Value.ESide.RIGHT;

			Instruction Inst = new Instruction(InstOp, Result.Index, LV[0].Index, RV[0].Index); 
			Instructions.Add(Inst);

			return Result;
		}

	}
}

