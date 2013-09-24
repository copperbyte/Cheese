using System;

using System.IO;


namespace Cheese.Machine
{

	internal static class SystemFunctions {

		internal static LuaValue print(LuaEnvironment Env, LuaTable Arguments) {

			bool First = true;
			foreach(LuaValue Curr in Arguments.EnumerableArray) {
				if(!First)
					Env.SystemOut.Write("\t");
				Env.SystemOut.Write(Curr.ToString());
				First = false;
			}

			Env.SystemOut.WriteLine();

			return LuaNil.Nil;
		}

		internal static void printS(LuaEnvironment Env, VmStack Stack, int ArgC, int RetC) {

			bool First = true;
			for(int Loop = 0; Loop < (ArgC-1); Loop++) {
			//foreach(LuaValue Curr in Arguments.EnumerableArray) {
				if(!First)
					Env.SystemOut.Write("\t");
				LuaValue Curr = Stack[Loop];
				Env.SystemOut.Write(Curr.ToString());
				First = false;
			}

			Env.SystemOut.WriteLine();

			// Ignoring Rets

			return;
		}


		internal static LuaValue next(LuaEnvironment Env, LuaTable Arguments) {
			LuaTable TableArg = Arguments[1] as LuaTable;
			LuaValue KeyArg = LuaNil.Nil;

			if(Arguments.Length == 2) {
				KeyArg = Arguments[2];
			}

			var Enumerable = TableArg.EnumerableTable;
			if(Enumerable == null)
				return LuaNil.Nil;

			var Enumerator = Enumerable.GetEnumerator();
			bool More = Enumerator.MoveNext();
		
			if(KeyArg != LuaNil.Nil) {
				while(Enumerator.Current.Key != KeyArg) {
					More = Enumerator.MoveNext();
					if(!More)
						break;
				}
				More = Enumerator.MoveNext(); // advance
			}

			if(!More) // end of table, no match
				return LuaNil.Nil;
			else { // on Match
				LuaTable Result = new LuaTable();
				Result.Add(Enumerator.Current.Key);
				Result.Add(Enumerator.Current.Value);
				return Result;
			}
		}


		internal static LuaValue pairs(LuaEnvironment Env, LuaTable Arguments) {
			LuaTable Result = new LuaTable();
			Result.Add(Env.Globals[new LuaString("next")]);
			Result.Add(Arguments[1]);
			Result.Add(LuaNil.Nil);
			return Result;
		}


		internal static void nextS(LuaEnvironment Env, VmStack Stack, int ArgC, int RetC) {
			LuaTable TableArg = Stack[0] as LuaTable;
			LuaValue KeyArg = LuaNil.Nil;

			if(Stack.Top >= 1) {
				KeyArg = Stack[1];
			}

			var Enumerable = TableArg.EnumerableTable;
			if(Enumerable == null) {
				Stack[-1] = LuaNil.Nil;
			}

			var Enumerator = Enumerable.GetEnumerator();
			bool More = Enumerator.MoveNext();

			if(KeyArg != LuaNil.Nil) {
				while(Enumerator.Current.Key != KeyArg) {
					More = Enumerator.MoveNext();
					if(!More)
						break;
				}
				More = Enumerator.MoveNext(); // advance
			}

			if(!More) { // end of table, no match
				//return LuaNil.Nil;
				Stack[-1] = LuaNil.Nil;
			} else { // on Match
				Stack[-1] = Enumerator.Current.Key;
				Stack[0] = Enumerator.Current.Value;
			}
		}


		internal static void pairsS(LuaEnvironment Env, VmStack Stack, int ArgC, int RetC) {
			Stack[-1] = Env.Globals[new LuaString("next")];
			Stack[0] = Stack[0];
			Stack[1] = LuaNil.Nil;
		}


		internal static LuaValue ipairsaux(LuaEnvironment Env, LuaTable Arguments) {
			LuaTable TableArg = Arguments[1] as LuaTable;
			LuaInteger KeyArg = Arguments[2] as LuaInteger;

			KeyArg = new LuaInteger(KeyArg.Integer+1);

			if(TableArg.Length >= KeyArg.Integer) {
				LuaTable Result = new LuaTable();
				Result.Add(KeyArg);
				Result.Add(TableArg[KeyArg.Integer]);
				return Result;
			} else {
				return LuaNil.Nil;
			}
		}

		internal static LuaValue ipairs(LuaEnvironment Env, LuaTable Arguments) {
			LuaTable Result = new LuaTable(3);
			Result[1] = Env.Globals[new LuaString("ipairsaux")];
			Result[2] = Arguments[1];
			Result[3] = new LuaInteger(0);
			return Result;
		}

		internal static void ipairsauxS(LuaEnvironment Env, VmStack Stack, int ArgC, int RetC) {
			LuaTable TableArg = Stack[0] as LuaTable;
			LuaInteger KeyArg = Stack[1] as LuaInteger;

			KeyArg = new LuaInteger(KeyArg.Integer+1);

			if(TableArg.Length >= KeyArg.Integer) {
				Stack[-1] = KeyArg;
				Stack[0] = TableArg[KeyArg.Integer];
			} else {
				Stack[-1] = LuaNil.Nil;
			}
		}

		internal static void ipairsS(LuaEnvironment Env, VmStack Stack, int ArgC, int RetC) {
			Stack[-1] = Env.Globals[new LuaString("ipairsaux")];
			Stack[0] = Stack[0];
			Stack[1] = new LuaInteger(0);
		}

	}

	public class LuaEnvironment
	{
		internal Machine Machine;

		internal TextWriter SystemOut;

		// Globals?
		// Has to be a LuaTable so it can be used in Lua code by "_G"
		internal LuaTable Globals;

		// Upvals?


		public LuaEnvironment()
		{
			Globals = new LuaTable();

			SystemOut = Console.Out;

			InitSystemFunctions();
		}

		public void SetOutput(TextWriter Out) {
			SystemOut = Out;
		}

		public Chunk Compile(TextReader Input) {
			Parser Parsy = new Parser(Input);
			ParseNode Root = Parsy.Parse();
			Parsy.PrintTree(Root);

			Compiler Comper = new Compiler();
			Chunk CompiledChunk = Comper.Compile(Root);

			return CompiledChunk;
		}

		public Chunk Compile(string Code) {
			return Compile(new StringReader(Code));
		}


		public void Execute(TextReader Input) {
			Chunk Compiled = Compile(Input);
			ExecuteChunk(Compiled);
		}
		public void Execute(string Code) {
			Chunk Compiled = Compile(Code);
			ExecuteChunk(Compiled);
		}


		public void ExecuteChunk(Chunk Chunk) {

			if(Machine == null)
				Machine = new Machine(this);		

			Machine.ExecuteChunk(Chunk);
		}


		////
		private void InitSystemFunctions() {

			//Globals[new LuaString("print")] = new LuaDelegate(SystemFunctions.print);
			Globals[new LuaString("print")] = new LuaSysDelegate(SystemFunctions.printS);

			//Globals[new LuaString("next")] = new LuaDelegate(SystemFunctions.next);
			//Globals[new LuaString("pairs")] = new LuaDelegate(SystemFunctions.pairs);
			Globals[new LuaString("next")] = new LuaSysDelegate(SystemFunctions.nextS);
			Globals[new LuaString("pairs")] = new LuaSysDelegate(SystemFunctions.pairsS);


			//Globals[new LuaString("ipairsaux")] = new LuaDelegate(SystemFunctions.ipairsaux);
			//Globals[new LuaString("ipairs")] = new LuaDelegate(SystemFunctions.ipairs);
			Globals[new LuaString("ipairsaux")] = new LuaSysDelegate(SystemFunctions.ipairsauxS);
			Globals[new LuaString("ipairs")] = new LuaSysDelegate(SystemFunctions.ipairsS);
		}
	}
}

