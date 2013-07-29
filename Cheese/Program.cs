using System;
using System.IO;
using System.Text;
using System.Collections.Generic;

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
			FileStream TestFile = new FileStream("..\\..\\TestFiles\\test_five.slua",FileMode.Open);


			StreamReader TestFileReader = new StreamReader(TestFile);

			Parser Parsy = new Parser(TestFileReader);
			ParseNode Root = Parsy.Parse();
			Parsy.PrintTree(Root);

			Compiler Comper = new Compiler();
			Comper.Compile(Root);


			Console.ReadKey();
		}
	}
}
