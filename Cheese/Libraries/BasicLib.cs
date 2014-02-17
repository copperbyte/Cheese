using System;

using System.IO;



namespace Cheese.Machine
{

	internal static class BasicLib {

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
			Env.SystemOut.Flush();
			// Ignoring Rets

			return;
		}

		internal static void next(LuaEnvironment Env, VmStack Stack, int ArgC, int RetC) {
			LuaTable TableArg = Stack[0] as LuaTable;
			LuaValue KeyArg = LuaNil.Nil;

			if(Stack.Top >= 1) {
				KeyArg = Stack[1];
			}

			bool UseNext = false;
			foreach(var Curr in TableArg) {
				if(KeyArg == LuaNil.Nil) {
					Stack[-1] = Curr.Key;
					Stack[0] = Curr.Value;
					return;
				}



				if(Curr.Key.Equals(KeyArg)) {
					UseNext = true;
					continue;
				}

				if(UseNext) {
					Stack[-1] = Curr.Key;
					Stack[0] = Curr.Value;
					return;
				}	
			}

			/*

			var Enumerable = TableArg.EnumerableTable;

			bool TableKeyFound = false;
			if(Enumerable != null) {
				var Enumerator = Enumerable.GetEnumerator();
				bool More = Enumerator.MoveNext();

				if(KeyArg != LuaNil.Nil) {
					while(Enumerator.Current.Key != KeyArg) {
						More = Enumerator.MoveNext();
						if(!More)
							break;
					}
					if(Enumerator.Current.Key == KeyArg)
						TableKeyFound = true;
					More = Enumerator.MoveNext(); // advance
				}

				if(More) { // on Match
					Stack[-1] = Enumerator.Current.Key;
					Stack[0] = Enumerator.Current.Value;
					return;
				}
			}

			if(KeyArg == LuaNil.Nil || KeyArg is LuaInteger) {
				var ListEnumerable = TableArg.EnumerableArray;

				if(ListEnumerable != null) {
					var ListEnumerator = ListEnumerable.GetEnumerator();
					bool More = ListEnumerator.MoveNext();					

					LuaInteger ListIndex = new LuaInteger(1);

					if(KeyArg != LuaNil.Nil) {
						while(ListIndex.Integer < (KeyArg as LuaInteger).Integer) {
							More = ListEnumerator.MoveNext();
							ListIndex.Integer = ListIndex.Integer + 1;
							if(!More)
								break;
						}
						More = ListEnumerator.MoveNext(); // advance
						ListIndex.Integer = ListIndex.Integer + 1;
					}

					if(More) { // on Match
						Stack[-1] = ListIndex;
						Stack[0] = ListEnumerator.Current;
						return;
					} 
				} 
			}
			*/

			Stack[-1] = LuaNil.Nil;
			return;
		}

		internal static void pairs(LuaEnvironment Env, VmStack Stack, int ArgC, int RetC) {
			Stack[-1] = Env.m_Globals[new LuaString("next")];
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
			Stack[-1] = Env.m_Globals[new LuaString("ipairsaux")];
			Stack[0] = Stack[0];
			Stack[1] = new LuaInteger(0);
		}


		internal static void LoadInto(LuaEnvironment Env, LuaTable Dest) {
			Dest[new LuaString("print")] = new LuaSysDelegate(BasicLib.print);
			Dest[new LuaString("next")] = new LuaSysDelegate(BasicLib.next);
			Dest[new LuaString("pairs")] = new LuaSysDelegate(BasicLib.pairs);
			Dest[new LuaString("ipairsaux")] = new LuaSysDelegate(BasicLib.ipairsaux);
			Dest[new LuaString("ipairs")] = new LuaSysDelegate(BasicLib.ipairs);
		}
	}


}

