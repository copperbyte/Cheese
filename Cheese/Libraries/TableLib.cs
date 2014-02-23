using System;


using System.Text;


namespace Cheese.Machine
{

	internal static class TableLib {


		internal static void Concat(LuaEnvironment Env, VmStack Stack, int ArgC, int RetC) {
			LuaTable TableArg = Stack[0] as LuaTable;

			string Sep = "";
			int i = 1, j = TableArg.Length;

			StringBuilder Builder = new StringBuilder();

			if(ArgC >= 3 ) { // 2 args, table, sep
				Sep = (Stack[1] as LuaString).Text;
			} 
			if(ArgC >= 4) { // 3 args, table, sep, i
				LuaValue IVal = Stack[2];
				if(IVal is LuaInteger)
					i = (int)(IVal as LuaInteger).Integer;
				else if(IVal is LuaNumber)
					i = (int)(IVal as LuaNumber).Number;
			}
			if(ArgC >= 5) { // 4 args, table, sep, i, j
				LuaValue JVal = Stack[3];
				if(JVal is LuaInteger)
					j = (int)(JVal as LuaInteger).Integer;
				else if(JVal is LuaNumber)
					j = (int)(JVal as LuaNumber).Number;
			}

			for(int Loop = i; Loop <= j; Loop++) {
				if(Loop != i)
					Builder.Append(Sep);
				Builder.Append(TableArg[Loop].ToString());
			}

			Stack[-1] = new LuaString(Builder.ToString());
			return;
		}

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

		internal static void MaxN(LuaEnvironment Env, VmStack Stack, int ArgC, int RetC) {
			LuaTable TableArg = Stack[0] as LuaTable;

			long MaxLong = long.MinValue;
			double MaxDouble = double.MinValue;


			foreach(var Curr in TableArg) {
				if(Curr.Key is LuaNumber) {
					MaxDouble = Math.Max(MaxDouble, (Curr.Key as LuaNumber).Number);
				}

				else if(Curr.Key is LuaInteger) {
					MaxLong = Math.Max(MaxLong, (Curr.Key as LuaInteger).Integer);
				}
			}

			if(MaxLong >= MaxDouble) {
				Stack[-1] = new LuaInteger(MaxLong);
			} else {
				Stack[-1] = new LuaNumber(MaxDouble);
			}

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

		//FIXME: Sort

		internal static void LoadInto(LuaEnvironment Env, LuaTable Dest) {

			LuaTable LTable = new LuaTable();

			LTable["concat"] = new LuaSysDelegate(TableLib.Concat);
			LTable["insert"] = new LuaSysDelegate(TableLib.Insert);
			LTable["maxn"] = new LuaSysDelegate(TableLib.MaxN);
			LTable["remove"] = new LuaSysDelegate(TableLib.Remove);

		
			Dest["table"] = LTable;
		}
	}


}

