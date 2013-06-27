using System;

namespace Cheese
{
	class Compiler
	{

		public Compiler()
		{

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


		void CompileBlock(ParseNode Block) {
			if(Block.Children == null)
				return;

			foreach(ParseNode Statement in Block.Children) {
				if(Statement.Type == ParseNode.EType.ASSIGN_STAT)
					CompileAssignmentStmt(Statement);
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


			for(int I = 0; I < Math.Max(Left.Children.Count, Right.Children.Count); I++) {
				ParseNode CL, CR;
				CL = Left.Children[I];
				CR = Right.Children[I];

				string LV = CL.GetTerminal().Value;
				string RV = CR.GetTerminal().Value;

				Console.WriteLine("{0} <= {1}", LV, RV);
			}


		}
	}
}

