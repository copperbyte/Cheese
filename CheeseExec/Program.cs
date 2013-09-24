using System;
using System.IO;
using System.Text;
using System.Collections.Generic;

using Machine = Cheese.Machine;
namespace Cheese
{


	class MainClass
	{
		public static void Main (string[] args)
		{
			//string Input = "Hello World [[longstr]] [==[longerstr]==] 0101 >= 234.56 0xFF and \"QUOTED\" --[[ COMMENT 1 ]] -- COMMENT 2 \n __index . .. ... ; : :: ;;  _9999_ ( 1 + 2 ) = 3 break  \"QUOTE ESCAPE \n YEAH\" !";
			//StringReader Reader = new StringReader (Input);
			//Scanner Scans = new Scanner(Reader);
			//FileStream TestFile = new FileStream("my_rules.lua",FileMode.Open);
			//FileStream TestFile = new FileStream("test.lua",FileMode.Open);

			//FileStream TestFile = new FileStream("..\\..\\TestFiles\\test_one.slua",FileMode.Open);
			//FileStream TestFile = new FileStream("..\\..\\TestFiles\\test_two.slua",FileMode.Open);
			//FileStream TestFile = new FileStream("..\\..\\TestFiles\\test_three.slua",FileMode.Open);
			//FileStream TestFile = new FileStream("..\\..\\TestFiles\\test_four.slua",FileMode.Open);
			//FileStream TestFile = new FileStream("..\\..\\TestFiles\\test_five.slua",FileMode.Open);
			//FileStream TestFile = new FileStream("..\\..\\TestFiles\\test_six.slua",FileMode.Open);
			//FileStream TestFile = new FileStream("..\\..\\TestFiles\\timetest.lua",FileMode.Open);
			FileStream TestFile = new FileStream("..\\..\\TestFiles\\self_test.slua",FileMode.Open);

			// TODO:  	UPVAL, CLOSE , everything
			// 			UNOP ,  machine
			//			VARARG , everything
			// 			TAILCALL , everything
			//			System Functions

			StreamReader TestFileReader = new StreamReader(TestFile);

			Parser Parsy = new Parser(TestFileReader);
			ParseNode Root = Parsy.Parse();
			Parsy.PrintTree(Root);

			Compiler Comper = new Compiler();
			Chunk CompiledChunk = Comper.Compile(Root);

			// Create Environment (which contains VM)
			// Environment.ExecuteChunk(CompiledChunk)
			//  executes Chunk RootFunc, causes others funcs and such 
			//  to be loaded into the environment

			Machine.LuaEnvironment LuaEnv = new Machine.LuaEnvironment();
			StringWriter LocalOut = new StringWriter();
			LuaEnv.SetOutput(LocalOut);

			LuaEnv.ExecuteChunk(CompiledChunk);

			Console.WriteLine("**{0}**", LocalOut.ToString());

			Console.ReadKey();
		}
	}
}
