using NUnit.Framework;
using System;
using System.IO;

using Cheese;
using Machine = Cheese.Machine;

namespace NUnitTests
{
	[TestFixture()]
	public class CheeseTests
	{

		public void ConsoleCompareTest(string Code, string Expected)
		{
			Machine.LuaEnvironment LuaEnv = new Machine.LuaEnvironment();
			StringWriter LocalOut = new StringWriter();
			LuaEnv.SetOutput(LocalOut);

			LuaEnv.Execute(Code);

			Assert.AreEqual(LocalOut.ToString(), Expected);
		}


		[Test()]
		public void GlobalTest()
		{
			string Code = @"
				x = 1;
				y = 2;
				z = x + y;
				w = y - x;
				print(x, y, z, w);";

			string Expected = "1\t2\t3\t1\r\n";

			ConsoleCompareTest(Code, Expected);
		}


		[Test()]
		public void LocalTest()
		{
			string Code = @"
				function test_func(x) 
				   local y = 2;
				   y = y * x;
				   return y;
				end

				x = 1;
				y = 2;
				local z = x + y;
				local w = test_func(x + z);

				print(x, y, z, w); ";

			string Expected = "1\t2\t3\t8\r\n";

			ConsoleCompareTest(Code, Expected);
		}

		[Test()]
		public void TableTest()
		{
			string Code = @"
				tab = {};
				tab.x = 1;
				tab['y'] = 2;
				zp = 'z';
				tab[zp] = tab.x + tab['y'];

				print(tab.x,tab['y'],(tab.x+tab['y'])); ";

			string Expected = "1\t2\t3\r\n";

			ConsoleCompareTest(Code, Expected);
		}

	}
}

