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
				if(Statement == ParseNode.EType.ASSIGN_STAT)
					CompileAssignmentStmt(Statement);
			}

		}


		void CompileAssignmentStmt(ParseNode Assignment) {

		}
	}
}

