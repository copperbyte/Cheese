
using System;
//using System.IO;
//using System.Text;
using System.Collections.Generic;

namespace Cheese
{
	using VList = List<string>;

	class Instruction {
		public enum OP {
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

	class Compiler
	{


		SortedSet<int> UsedTemps;
		public Compiler()
		{
			UsedTemps = new SortedSet<int>();

		}


		public void Compile(ParseNode RootChunk) 
		{
			if(RootChunk.Children == null)
				return;

			foreach(ParseNode Child in RootChunk.Children) {
				if(Child.Type == ParseNode.EType.BLOCK)
					CompileBlock(Child);
			}
		}

		int GetTemp() {
			int Result = 0;
			while(UsedTemps.Contains(Result)) {
				Result++;
			}
			UsedTemps.Add(Result);
			return Result;
		}

		void CompileBlock(ParseNode Block) {
			if(Block.Children == null)
				return;

			foreach(ParseNode Statement in Block.Children) {
				if(Statement.Type == ParseNode.EType.ASSIGN_STAT)
					CompileAssignmentStmt(Statement);

				UsedTemps.Clear();
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


			VList RVs = CompileExpList(Right);

			for(int I = 0; I < Left.Children.Count; I++) {
				ParseNode CL;
				CL = Left.Children[I];

				string LV = CL.GetTerminal().Value;

				Console.WriteLine("{0} := {1}", LV, RVs[I]);

				// is LV a global, a local, an upvalue, or a tableval
				Instruction Ins = new Instruction(Instruction.OP.SETGLOBAL, LV, RVs[I]);
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
					if(Child.Token.Type == Token.EType.KEYWORD ||
						Child.Token.Type == Token.EType.NUMBER ||
						Child.Token.Type == Token.EType.STRING) {
						int T = GetTemp();
						Result.Add("t" + T);
						Console.WriteLine("t{0} := {1}", T, Child.Token.Value); 
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
				Result.Add(VarOrExp.Children[0].GetTerminal().Value);
			}

			return Result;
		}

		string CompileBinOp(ParseNode BinOp) {

			ParseNode Left = BinOp.Children[0];
			ParseNode Op = BinOp.Children[1];
			ParseNode Right = BinOp.Children[2];

			int T = GetTemp();

			VList LV = CompileExp(Left);
			VList RV = CompileExp(Right);

			Console.WriteLine("t{0} := {1} {2} {3}", T, LV[0], Op.Token.Value, RV[0]);

			return "t"+T;
		}

	}
}

