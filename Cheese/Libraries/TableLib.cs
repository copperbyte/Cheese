using System;

using System.IO;



namespace Cheese.Machine
{

	internal static class TableLib {

		internal static void Insert(LuaEnvironment Env, VmStack Stack, int ArgC, int RetC) {
			LuaTable TableArg = Stack[0] as LuaTable;
			LuaValue InsertValue = LuaNil.Nil;

			int Pos = 0;

			if(ArgC == 3 ) { // 2 args, table, value
				Pos = TableArg.Length + 1;
				InsertValue = Stack[1];
			} else if(ArgC == 4) { // 3 args, table, pos, value
				LuaValue PosVal = Stack[1];
				if(PosVal is LuaInteger)
					Pos = (int)(PosVal as LuaInteger).Integer;
				else if(PosVal is LuaNumber)
					Pos = (int)(PosVal as LuaNumber).Number;
				InsertValue = Stack[2];
			}

			TableArg.Insert(Pos, InsertValue);

			return;
		}


		internal static void Remove(LuaEnvironment Env, VmStack Stack, int ArgC, int RetC) {
			LuaTable TableArg = Stack[0] as LuaTable;

			int Pos = 0;

			if(ArgC == 2 ) { // 1 args, table 
				Pos = TableArg.Length;
			} else if(ArgC == 3) { // 3 args, table, pos
				LuaValue PosVal = Stack[1];
				if(PosVal is LuaInteger)
					Pos = (int)(PosVal as LuaInteger).Integer;
				else if(PosVal is LuaNumber)
					Pos = (int)(PosVal as LuaNumber).Number;
			}

			TableArg.Remove(Pos);

			return;
		}


		internal static void LoadInto(LuaEnvironment Env, LuaTable Dest) {

			LuaTable LTable = new LuaTable();

			LTable["insert"] = new LuaSysDelegate(TableLib.Insert);
			LTable["remove"] = new LuaSysDelegate(TableLib.Remove);

		
			Dest["table"] = LTable;
		}
	}


}

