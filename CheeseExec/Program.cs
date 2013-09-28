using System;
using System.IO;
using System.Text;
using System.Collections.Generic;

using Cheese;
using Machine = Cheese.Machine;

namespace CheeseExec
{


	class MainClass
	{
		public static void Main (string[] args)
		{
			//string Input = "Hello World [[longstr]] [==[longerstr]==] 0101 >= 234.56 0xFF and \"QUOTED\" --[[ COMMENT 1 ]] -- COMMENT 2 \n __index . .. ... ; : :: ;;  _9999_ ( 1 + 2 ) = 3 break  \"QUOTE ESCAPE \n YEAH\" !";
			//StringReader Reader = new StringReader (Input);
			//Scanner Scans = new Scanner(Reader);
			//FileStream TestFile = new FileStream("test.lua",FileMode.Open);
			FileStream TestFile = null;

			if(args.Length >= 1) {
				TestFile = new FileStream(args[0], FileMode.Open);
			}
			else {
			//FileStream TestFile = new FileStream("..\\..\\TestFiles\\test_one.slua",FileMode.Open);
			//FileStream TestFile = new FileStream("..\\..\\TestFiles\\test_two.slua",FileMode.Open);
			//FileStream TestFile = new FileStream("..\\..\\TestFiles\\test_three.slua",FileMode.Open);
			//FileStream TestFile = new FileStream("..\\..\\TestFiles\\test_four.slua",FileMode.Open);
			//FileStream TestFile = new FileStream("..\\..\\TestFiles\\test_five.slua",FileMode.Open);
			//FileStream TestFile = new FileStream("..\\..\\TestFiles\\test_six.slua",FileMode.Open);
			//FileStream TestFile = new FileStream("..\\..\\TestFiles\\timetest.lua",FileMode.Open);
			//FileStream TestFile = new FileStream("..\\..\\TestFiles\\self_test.slua",FileMode.Open);
				TestFile = new FileStream("..\\..\\..\\Cheese\\TestFiles\\concat_test.slua",FileMode.Open);
			}


			// TODO:  	UPVAL, CLOSE , everything
			//          and, or, compiler
			//			VARARG , everything
			// 			TAILCALL , everything
			//			System Functions
			//   		Less allocations? Cache simple values? 
			//          String-to-number auto-converts?


			StreamReader TestFileReader = new StreamReader(TestFile);

			/*
			Parser Parsy = new Parser(TestFileReader);
			ParseNode Root = Parsy.Parse();
			Parsy.PrintTree(Root);

			Compiler Comper = new Compiler();
			Chunk CompiledChunk = Comper.Compile(Root);
			*/



			Machine.LuaEnvironment LuaEnv = new Machine.LuaEnvironment();
			StringWriter LocalOut = new StringWriter();
			LuaEnv.SetOutput(LocalOut);

			//LuaEnv.ExecuteChunk(CompiledChunk);
			LuaEnv.Execute(TestFileReader);

			Console.WriteLine("**{0}**", LocalOut.ToString());

			Console.ReadKey();
		}
	}
}
