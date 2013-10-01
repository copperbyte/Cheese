using System;

using System.IO;


namespace Cheese.Machine
{

	internal static class BaseLib {

		internal static void print(LuaEnvironment Env, VmStack Stack, int ArgC, int RetC) {

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

		internal static void next(LuaEnvironment Env, VmStack Stack, int ArgC, int RetC) {
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

		internal static void pairs(LuaEnvironment Env, VmStack Stack, int ArgC, int RetC) {
			Stack[-1] = Env.Globals[new LuaString("next")];
			Stack[0] = Stack[0];
			Stack[1] = LuaNil.Nil;
		}

		internal static void ipairsaux(LuaEnvironment Env, VmStack Stack, int ArgC, int RetC) {
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

		internal static void ipairs(LuaEnvironment Env, VmStack Stack, int ArgC, int RetC) {
			Stack[-1] = Env.Globals[new LuaString("ipairsaux")];
			Stack[0] = Stack[0];
			Stack[1] = new LuaInteger(0);
		}


		internal static void LoadInto(LuaEnvironment Env, LuaTable Dest) {
			Dest[new LuaString("print")] = new LuaSysDelegate(BaseLib.print);
			Dest[new LuaString("next")] = new LuaSysDelegate(BaseLib.next);
			Dest[new LuaString("pairs")] = new LuaSysDelegate(BaseLib.pairs);
			Dest[new LuaString("ipairsaux")] = new LuaSysDelegate(BaseLib.ipairsaux);
			Dest[new LuaString("ipairs")] = new LuaSysDelegate(BaseLib.ipairs);
		}
	}

	internal static class MathLib {

		internal static void Abs(LuaEnvironment Env, VmStack Stack, int ArgC, int RetC) {

			LuaValue Param = Stack[0];
			LuaValue Result = LuaNil.Nil;

			if(Param is LuaInteger) {
				Result = new LuaInteger(Math.Abs( (Param as LuaInteger).Integer));
			} else if(Param is LuaNumber) {
				Result = new LuaNumber(Math.Abs( (Param as LuaNumber).Number) );
			}

			Stack[-1] = Result;
			return;
		}


		internal static void LoadInto(LuaEnvironment Env, LuaTable Dest) {
			LuaTable LMath = new LuaTable();

			LMath[new LuaString("abs")] = new LuaSysDelegate(MathLib.Abs);

			Dest[new LuaString("math")] = LMath;
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
			BaseLib.LoadInto(this, Globals);
			MathLib.LoadInto(this, Globals);
		}
	}
}

