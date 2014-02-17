using System;

using System.IO;



namespace Cheese.Machine
{


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
			BasicLib.LoadInto(this, m_Globals);
			MathLib.LoadInto(this, m_Globals);
		}
	}
}

