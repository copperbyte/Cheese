using System;
using System.Collections.Generic;

using Cheese;


namespace Cheese.Machine
{

	class ConstEntry {
		internal int Index;
		internal double NumberVal;
		internal string StringVal;
	}

	class LocalEntry {
		internal int Index;
		internal string Name;
		internal int StartPC, EndPC;
	}

	class Function {


		internal int NumParams;
		internal bool IsVarArg;
		internal int MaxStackSize;

		internal SortedSet<int> UsedRegs;  // Move back to Compiler???
		internal List<ConstEntry> ConstantTable;

		//internal List<LocalEntry> LocalTable;
		internal List< List<LocalEntry> > LocalScopes;
		internal List<LocalEntry> FullLocalTable;

		internal List<Instruction> Instructions;

		public Function() {
			NumParams = 0;
			IsVarArg = false;
			MaxStackSize = 0;

			UsedRegs = new SortedSet<int>();
			ConstantTable = new List<ConstEntry>();
			//LocalTable = new List<LocalEntry>();
			LocalScopes = new List< List<LocalEntry> >();
			FullLocalTable = new List<LocalEntry>();
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
					Console.WriteLine("CS  {0:00} := {1}", Const.Index, Const.StringVal);
				else
					Console.WriteLine("CN  {0:00} := {1}", Const.Index, Const.NumberVal);
			}
		}

		internal void PrintLocals() {
			foreach(LocalEntry Local in FullLocalTable) {
				Console.WriteLine("LV  {0:00} := {1}  ({2}..{3})", 
				                  Local.Index, Local.Name,
				                  Local.StartPC, Local.EndPC);
			}
		}

		internal void PrintInstructions() {
			int Counter = 0;
			foreach(Instruction Inst in Instructions) {
				Console.WriteLine("{0:000} IC  {1}\t{2}  {3}  {4}", 
				                  Counter,
				                  Inst.Code, 
				                  (Inst.isA?Inst.A.ToString():" "), 
				                  (Inst.isB?(Inst.rkB?(Inst.B+256):Inst.B).ToString():" "), 
				                  (Inst.isC?(Inst.rkC?(Inst.C+256):Inst.C).ToString():" "));
				Counter++;
			}
		}
	}

}

