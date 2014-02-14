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

		internal static void Acos(LuaEnvironment Env, VmStack Stack, int ArgC, int RetC) {
			LuaValue Param = Stack[0];
			LuaValue Result = LuaNil.Nil;
			if(Param is LuaInteger) {
				double Value = (double)(Param as LuaInteger).Integer;
				Result = new LuaNumber(Math.Acos(Value));
			} else if(Param is LuaNumber) {
				Result = new LuaNumber(Math.Acos((Param as LuaNumber).Number));
			}
			Stack[-1] = Result;
			return;
		}

		internal static void Asin(LuaEnvironment Env, VmStack Stack, int ArgC, int RetC) {
			LuaValue Param = Stack[0];
			LuaValue Result = LuaNil.Nil;
			if(Param is LuaInteger) {
				double Value = (double)(Param as LuaInteger).Integer;
				Result = new LuaNumber(Math.Asin(Value));
			} else if(Param is LuaNumber) {
				Result = new LuaNumber(Math.Asin((Param as LuaNumber).Number));
			}
			Stack[-1] = Result;
			return;
		}

		internal static void Atan(LuaEnvironment Env, VmStack Stack, int ArgC, int RetC) {
			LuaValue Param = Stack[0];
			LuaValue Result = LuaNil.Nil;
			if(Param is LuaInteger) {
				double Value = (double)(Param as LuaInteger).Integer;
				Result = new LuaNumber(Math.Atan(Value));
			} else if(Param is LuaNumber) {
				Result = new LuaNumber(Math.Atan((Param as LuaNumber).Number));
			}
			Stack[-1] = Result;
			return;
		}

		internal static void Atan2(LuaEnvironment Env, VmStack Stack, int ArgC, int RetC) {
			LuaValue YV = Stack[0], XV = Stack[1];

			double YD = 0.0, XD = 0.0;
			if(YV is LuaInteger)
				YD = (double)(YV as LuaInteger).Integer;
			else if(YV is LuaNumber)
				YD = (YV as LuaNumber).Number;
			else {
				Stack[-1] = LuaNil.Nil;
				return;
			}

			if(XV is LuaInteger)
				XD = (double)(XV as LuaInteger).Integer;
			else if(XV is LuaNumber)
				XD = (XV as LuaNumber).Number;
			else {
				Stack[-1] = LuaNil.Nil;
				return;
			}

			Stack[-1] = new LuaNumber(Math.Atan2(YD, XD));
			return;
		}

		internal static void Ceiling(LuaEnvironment Env, VmStack Stack, int ArgC, int RetC) {
			LuaValue Param = Stack[0];
			double VD = 0.0;
			if(Param is LuaInteger) {
				VD = (double)(Param as LuaInteger).Integer;
			} else if(Param is LuaNumber) {
				VD = (Param as LuaNumber).Number;
			} else {
				Stack[-1] = LuaNil.Nil;
				return;
			}
			Stack[-1] = new LuaNumber(Math.Ceiling(VD));
			return;
		}

		internal static void Cos(LuaEnvironment Env, VmStack Stack, int ArgC, int RetC) {
			LuaValue Param = Stack[0];
			double VD = 0.0;
			if(Param is LuaInteger) {
				VD = (double)(Param as LuaInteger).Integer;
			} else if(Param is LuaNumber) {
				VD = (Param as LuaNumber).Number;
			} else {
				Stack[-1] = LuaNil.Nil;
				return;
			}
			Stack[-1] = new LuaNumber(Math.Cos(VD));
			return;
		}

		internal static void Cosh(LuaEnvironment Env, VmStack Stack, int ArgC, int RetC) {
			LuaValue Param = Stack[0];
			double VD = 0.0;
			if(Param is LuaInteger) {
				VD = (double)(Param as LuaInteger).Integer;
			} else if(Param is LuaNumber) {
				VD = (Param as LuaNumber).Number;
			} else {
				Stack[-1] = LuaNil.Nil;
				return;
			}
			Stack[-1] = new LuaNumber(Math.Cosh(VD));
			return;
		}

		internal static void Deg(LuaEnvironment Env, VmStack Stack, int ArgC, int RetC) {
			LuaValue Param = Stack[0];
			double VD = 0.0;
			if(Param is LuaInteger) {
				VD = (double)(Param as LuaInteger).Integer;
			} else if(Param is LuaNumber) {
				VD = (Param as LuaNumber).Number;
			} else {
				Stack[-1] = LuaNil.Nil;
				return;
			}
			Stack[-1] = new LuaNumber( VD * (180.0 / Math.PI) );
			return;
		}

		internal static void Exp(LuaEnvironment Env, VmStack Stack, int ArgC, int RetC) {
			LuaValue Param = Stack[0];
			double VD = 0.0;
			if(Param is LuaInteger) {
				VD = (double)(Param as LuaInteger).Integer;
			} else if(Param is LuaNumber) {
				VD = (Param as LuaNumber).Number;
			} else {
				Stack[-1] = LuaNil.Nil;
				return;
			}
			Stack[-1] = new LuaNumber(Math.Exp(VD));
			return;
		}

		internal static void Floor(LuaEnvironment Env, VmStack Stack, int ArgC, int RetC) {
			LuaValue Param = Stack[0];
			double VD = 0.0;
			if(Param is LuaInteger) {
				VD = (double)(Param as LuaInteger).Integer;
			} else if(Param is LuaNumber) {
				VD = (Param as LuaNumber).Number;
			} else {
				Stack[-1] = LuaNil.Nil;
				return;
			}
			Stack[-1] = new LuaNumber(Math.Floor(VD));
			return;
		}

		internal static void Log(LuaEnvironment Env, VmStack Stack, int ArgC, int RetC) {
			LuaValue Param = Stack[0];
			double VD = 0.0;
			if(Param is LuaInteger) {
				VD = (double)(Param as LuaInteger).Integer;
			} else if(Param is LuaNumber) {
				VD = (Param as LuaNumber).Number;
			} else {
				Stack[-1] = LuaNil.Nil;
				return;
			}
			Stack[-1] = new LuaNumber(Math.Log(VD));
			return;
		}

		internal static void Log10(LuaEnvironment Env, VmStack Stack, int ArgC, int RetC) {
			LuaValue Param = Stack[0];
			double VD = 0.0;
			if(Param is LuaInteger) {
				VD = (double)(Param as LuaInteger).Integer;
			} else if(Param is LuaNumber) {
				VD = (Param as LuaNumber).Number;
			} else {
				Stack[-1] = LuaNil.Nil;
				return;
			}
			Stack[-1] = new LuaNumber(Math.Log10(VD));
			return;
		}

		internal static void Max(LuaEnvironment Env, VmStack Stack, int ArgC, int RetC) {
			LuaValue AV = Stack[0], BV = Stack[1];

			if(AV is LuaInteger && BV is LuaInteger) {
				Stack[-1] = new LuaInteger(Math.Max((AV as LuaInteger).Integer,
				                                    (BV as LuaInteger).Integer));
				return;
			}

			double AD = 0.0, BD = 0.0;
			if(AV is LuaInteger)
				AD = (double)(AV as LuaInteger).Integer;
			else if(AV is LuaNumber)
				AD = (AV as LuaNumber).Number;
			else {
				Stack[-1] = LuaNil.Nil;
				return;
			}

			if(BV is LuaInteger)
				BD = (double)(BV as LuaInteger).Integer;
			else if(BV is LuaNumber)
				BD = (BV as LuaNumber).Number;
			else {
				Stack[-1] = LuaNil.Nil;
				return;
			}

			Stack[-1] = new LuaNumber(Math.Max(AD, BD));
			return;
		}

		internal static void Min(LuaEnvironment Env, VmStack Stack, int ArgC, int RetC) {
			LuaValue AV = Stack[0], BV = Stack[1];

			if(AV is LuaInteger && BV is LuaInteger) {
				Stack[-1] = new LuaInteger(Math.Min((AV as LuaInteger).Integer,
				                                    (BV as LuaInteger).Integer));
				return;
			}

			double AD = 0.0, BD = 0.0;
			if(AV is LuaInteger)
				AD = (double)(AV as LuaInteger).Integer;
			else if(AV is LuaNumber)
				AD = (AV as LuaNumber).Number;
			else {
				Stack[-1] = LuaNil.Nil;
				return;
			}

			if(BV is LuaInteger)
				BD = (double)(BV as LuaInteger).Integer;
			else if(BV is LuaNumber)
				BD = (BV as LuaNumber).Number;
			else {
				Stack[-1] = LuaNil.Nil;
				return;
			}

			Stack[-1] = new LuaNumber(Math.Min(AD, BD));
			return;
		}

		internal static void Pow(LuaEnvironment Env, VmStack Stack, int ArgC, int RetC) {
			LuaValue AV = Stack[0], BV = Stack[1];

			double AD = 0.0, BD = 0.0;
			if(AV is LuaInteger)
				AD = (double)(AV as LuaInteger).Integer;
			else if(AV is LuaNumber)
				AD = (AV as LuaNumber).Number;
			else {
				Stack[-1] = LuaNil.Nil;
				return;
			}

			if(BV is LuaInteger)
				BD = (double)(BV as LuaInteger).Integer;
			else if(BV is LuaNumber)
				BD = (BV as LuaNumber).Number;
			else {
				Stack[-1] = LuaNil.Nil;
				return;
			}

			Stack[-1] = new LuaNumber(Math.Pow(AD, BD));
			return;
		}

		internal static void Rad(LuaEnvironment Env, VmStack Stack, int ArgC, int RetC) {
			LuaValue Param = Stack[0];
			double VD = 0.0;
			if(Param is LuaInteger) {
				VD = (double)(Param as LuaInteger).Integer;
			} else if(Param is LuaNumber) {
				VD = (Param as LuaNumber).Number;
			} else {
				Stack[-1] = LuaNil.Nil;
				return;
			}
			Stack[-1] = new LuaNumber( VD * (Math.PI / 180.0) );
			return;
		}

		internal static void Random(LuaEnvironment Env, VmStack Stack, int ArgC, int RetC) {
			if(ArgC == 1) {
				Stack[-1] = new LuaNumber(Env.RandomSource.NextDouble());
				return;
			} else {
				LuaValue Param = Stack[0];
				int VI1 = 0, VI2 = 0;
				if(Param is LuaInteger) {
					VI1 = (int)(Param as LuaInteger).Integer;
				} else if(Param is LuaNumber) {
					VI1 = (int)(Param as LuaNumber).Number;
				} else {
					Stack[-1] = LuaNil.Nil;
					return;
				}
				if(ArgC == 3) {
					LuaValue Param2 = Stack[1];
					if(Param2 is LuaInteger) {
						VI2 = (int)(Param2 as LuaInteger).Integer;
					} else if(Param2 is LuaNumber) {
						VI2 = (int)(Param2 as LuaNumber).Number;
					} else {
						Stack[-1] = LuaNil.Nil;
						return;
					}
				}
				if(ArgC == 2)
					Stack[-1] = new LuaInteger(Env.RandomSource.Next(VI1) + 1);
				else if(ArgC == 3)
					Stack[-1] = new LuaInteger(Env.RandomSource.Next(VI1, VI2+1));
				return;
			}
		}

		internal static void RandomSeed(LuaEnvironment Env, VmStack Stack, int ArgC, int RetC) {
			LuaValue Param = Stack[0];
			double VD = 0.0;
			if(Param is LuaInteger) {
				VD = (double)(Param as LuaInteger).Integer;
			} else if(Param is LuaNumber) {
				VD = (Param as LuaNumber).Number;
			} else {
				Stack[-1] = LuaNil.Nil;
				return;
			}
			Stack[-1] = LuaNil.Nil;
			Env.RandomSource = new Random((int)VD);
			return;
		}

		internal static void Sin(LuaEnvironment Env, VmStack Stack, int ArgC, int RetC) {
			LuaValue Param = Stack[0];
			double VD = 0.0;
			if(Param is LuaInteger) {
				VD = (double)(Param as LuaInteger).Integer;
			} else if(Param is LuaNumber) {
				VD = (Param as LuaNumber).Number;
			} else {
				Stack[-1] = LuaNil.Nil;
				return;
			}
			Stack[-1] = new LuaNumber(Math.Sin(VD));
			return;
		}

		internal static void Sinh(LuaEnvironment Env, VmStack Stack, int ArgC, int RetC) {
			LuaValue Param = Stack[0];
			double VD = 0.0;
			if(Param is LuaInteger) {
				VD = (double)(Param as LuaInteger).Integer;
			} else if(Param is LuaNumber) {
				VD = (Param as LuaNumber).Number;
			} else {
				Stack[-1] = LuaNil.Nil;
				return;
			}
			Stack[-1] = new LuaNumber(Math.Sinh(VD));
			return;
		}

		internal static void Sqrt(LuaEnvironment Env, VmStack Stack, int ArgC, int RetC) {
			LuaValue Param = Stack[0];
			double VD = 0.0;
			if(Param is LuaInteger) {
				VD = (double)(Param as LuaInteger).Integer;
			} else if(Param is LuaNumber) {
				VD = (Param as LuaNumber).Number;
			} else {
				Stack[-1] = LuaNil.Nil;
				return;
			}
			Stack[-1] = new LuaNumber(Math.Sqrt(VD));
			return;
		}

		internal static void Tan(LuaEnvironment Env, VmStack Stack, int ArgC, int RetC) {
			LuaValue Param = Stack[0];
			double VD = 0.0;
			if(Param is LuaInteger) {
				VD = (double)(Param as LuaInteger).Integer;
			} else if(Param is LuaNumber) {
				VD = (Param as LuaNumber).Number;
			} else {
				Stack[-1] = LuaNil.Nil;
				return;
			}
			Stack[-1] = new LuaNumber(Math.Tan(VD));
			return;
		}

		internal static void Tanh(LuaEnvironment Env, VmStack Stack, int ArgC, int RetC) {
			LuaValue Param = Stack[0];
			double VD = 0.0;
			if(Param is LuaInteger) {
				VD = (double)(Param as LuaInteger).Integer;
			} else if(Param is LuaNumber) {
				VD = (Param as LuaNumber).Number;
			} else {
				Stack[-1] = LuaNil.Nil;
				return;
			}
			Stack[-1] = new LuaNumber(Math.Tanh(VD));
			return;
		}

		internal static void LoadInto(LuaEnvironment Env, LuaTable Dest) {
			LuaTable LMath = new LuaTable();

			LMath[new LuaString("abs")] = new LuaSysDelegate(MathLib.Abs);
			LMath[new LuaString("acos")] = new LuaSysDelegate(MathLib.Acos);
			LMath[new LuaString("asin")] = new LuaSysDelegate(MathLib.Asin);
			LMath[new LuaString("atan")] = new LuaSysDelegate(MathLib.Atan);
			LMath[new LuaString("atan2")] = new LuaSysDelegate(MathLib.Atan2);
			LMath[new LuaString("ceil")] = new LuaSysDelegate(MathLib.Ceiling);
			LMath[new LuaString("cos")] = new LuaSysDelegate(MathLib.Cos);
			LMath[new LuaString("cosh")] = new LuaSysDelegate(MathLib.Cosh);
			LMath[new LuaString("deg")] = new LuaSysDelegate(MathLib.Deg);
			LMath[new LuaString("exp")] = new LuaSysDelegate(MathLib.Exp);
			LMath[new LuaString("floor")] = new LuaSysDelegate(MathLib.Floor);
			// fmod
			// frexp
			// huge
			// ldexp
			LMath[new LuaString("log")] = new LuaSysDelegate(MathLib.Log);
			LMath[new LuaString("log10")] = new LuaSysDelegate(MathLib.Log10);
			LMath[new LuaString("max")] = new LuaSysDelegate(MathLib.Max);
			LMath[new LuaString("min")] = new LuaSysDelegate(MathLib.Min);
			// modf
			LMath[new LuaString("pi")] = new LuaNumber(Math.PI);
			LMath[new LuaString("pow")] = new LuaSysDelegate(MathLib.Pow);
			LMath[new LuaString("rad")] = new LuaSysDelegate(MathLib.Rad);
			LMath[new LuaString("random")] = new LuaSysDelegate(MathLib.Random);
			LMath[new LuaString("randomseed")] = new LuaSysDelegate(MathLib.RandomSeed);
			LMath[new LuaString("sin")] = new LuaSysDelegate(MathLib.Sin);
			LMath[new LuaString("sinh")] = new LuaSysDelegate(MathLib.Sinh);
			LMath[new LuaString("sqrt")] = new LuaSysDelegate(MathLib.Sqrt);
			LMath[new LuaString("tan")] = new LuaSysDelegate(MathLib.Tan);
			LMath[new LuaString("tanh")] = new LuaSysDelegate(MathLib.Tanh);

			Dest[new LuaString("math")] = LMath;
		}
	}


	public class LuaEnvironment
	{
		internal Machine Machine;

		internal TextWriter SystemOut;

		internal Random RandomSource;

		// Globals?
		// Has to be a LuaTable so it can be used in Lua code by "_G"
		internal LuaTable m_Globals;

		public LuaTable Globals {
			get { return m_Globals; }

		}

		// Upvals?


		public LuaEnvironment()
		{
			m_Globals = new LuaTable();

			SystemOut = Console.Out;

			RandomSource = new Random();

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
			Execute(Compiled);
		}
		public void Execute(string Code) {
			Chunk Compiled = Compile(Code);
			Execute(Compiled);
		}


		public void Execute(Chunk Chunk) {

			if(Machine == null)
				Machine = new Machine(this);		

			Machine.ExecuteChunk(Chunk);
		}


		////
		public LuaTable Execute_VarArg(LuaClosure Closure, LuaTable Args) {

			return Machine.ExecuteFunction(Closure.Function, Args);
		}

		public LuaValue Execute(LuaClosure Closure) {
			return Machine.ExecuteFunction_Zero(Closure.Function);
		}

		public LuaValue Execute(LuaClosure Closure, LuaValue Arg){
			return Machine.ExecuteFunction_One(Closure.Function, Arg);
		}

		public LuaValue Execute(LuaClosure Closure, LuaValue A1=null, LuaValue A2=null){
			return Machine.ExecuteFunction_Two(Closure.Function, A1, A2);
		}

		public LuaValue Execute(LuaClosure Closure, LuaValue A1=null, LuaValue A2=null, LuaValue A3=null){
			return Machine.ExecuteFunction_Three(Closure.Function, A1, A2, A3);
		}

		////
		public LuaValue GetGlobalValue(string Name) {
			LuaString LuaKey = new LuaString(Name);
			return m_Globals[LuaKey];
		}

		////
		private void InitSystemFunctions() {
			BaseLib.LoadInto(this, m_Globals);
			MathLib.LoadInto(this, m_Globals);
		}
	}
}

