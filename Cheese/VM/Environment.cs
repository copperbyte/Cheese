using System;


namespace Cheese.Machine
{

	internal static class SystemFunctions {

		internal static LuaValue print(LuaEnvironment Env, LuaTable Arguments) {

			bool First = true;
			foreach(LuaValue Curr in Arguments.EnumerableArray) {
				if(!First)
					Console.Write("\t");
				Console.Write(Curr.ToString());
				First = false;
			}

			Console.WriteLine();

			return LuaNil.Nil;
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

	}

	public class LuaEnvironment
	{
		internal Machine Machine;

		// Globals?
		// Has to be a LuaTable so it can be used in Lua code by "_G"
		internal LuaTable Globals;

		// Upvals?


		public LuaEnvironment()
		{
			Globals = new LuaTable();

			InitSystemFunctions();
		}


		public void ExecuteChunk(Chunk Chunk) {

			if(Machine == null)
				Machine = new Machine(this);		

			Machine.ExecuteChunk(Chunk);
		}


		////
		private void InitSystemFunctions() {

			Globals[new LuaString("print")] = new LuaDelegate(SystemFunctions.print);

			Globals[new LuaString("next")] = new LuaDelegate(SystemFunctions.next);
			Globals[new LuaString("pairs")] = new LuaDelegate(SystemFunctions.pairs);

			Globals[new LuaString("ipairsaux")] = new LuaDelegate(SystemFunctions.ipairsaux);
			Globals[new LuaString("ipairs")] = new LuaDelegate(SystemFunctions.ipairs);
		}
	}
}

