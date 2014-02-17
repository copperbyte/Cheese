using System;
using System.Collections.Generic;

using Cheese;


namespace Cheese.Machine
{
	class LocalEntry {
		internal int Index;
		internal string Name;
		internal int StartPC, EndPC;
		public override string ToString ()
		{
			return string.Format ("{0} : {1} ({2}-{3})", Name, Index, StartPC, EndPC);
		}
	}

	class UpvalEntry {
		internal int Index;
		internal string Name;
	}

	class Function {


		internal int NumParams;
		internal bool IsVarArg;
		internal int MaxStackSize;

		internal List<LuaValue> ConstantTable;

		//internal List< List<LocalEntry> > LocalScopes;
		internal List<LocalEntry> FullLocalTable;
		internal List<Function> SubFunctions;

		internal List<Instruction> Instructions;

		public Function() {
			NumParams = 0;
			IsVarArg = false;
			MaxStackSize = 0;

			ConstantTable = new List<LuaValue>();
			//LocalScopes = new List< List<LocalEntry> >();
			FullLocalTable = new List<LocalEntry>();
			SubFunctions = new List<Function>();

			Instructions = new List<Instruction>();
		}

		internal void Print() {
			PrintHeader();
			PrintConstants();
			PrintLocals();
			PrintInstructions();

			foreach(Function SubFunc in SubFunctions) {
				Console.WriteLine();
				SubFunc.Print();
			}
		}

		internal void PrintHeader() {
			Console.WriteLine("NumParams: {0}", NumParams);
			Console.WriteLine("IsVarArg: {0}", IsVarArg);
			Console.WriteLine("MaxStackSize: {0}", MaxStackSize);
		}

		internal void PrintConstants() {
			int Index = 0;
			foreach(LuaValue Const in ConstantTable) {
				Console.WriteLine("CV  {0:00} := {1}", Index, Const.ToString());
				Index++;
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
				//Console.WriteLine("{0:000} IC  {1}\t{2}  {3}  {4}", 
				//                  Counter,
				//                  Inst.Code, 
				//                  (Inst.isA?Inst.A.ToString():" "), 
				//                  (Inst.isB?(Inst.rkB?(Inst.B+256):Inst.B).ToString():" "), 
				//                  (Inst.isC?(Inst.rkC?(Inst.C+256):Inst.C).ToString():" "));
				Console.WriteLine("{0:000} IC  {1}", 
				                  Counter, Inst.ToString());

				Counter++;
			}
		}
	}

}

