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

			Assert.AreEqual(Expected, LocalOut.ToString());
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

		[Test()]
		public void IterListTest()
		{
			string Code = @"
				array_tab = { ""apple"", ""grape"", ""orange"" };
				function print_table(tbl)
				  for key, value in pairs(tbl) do
                    print(key, value);
				  end
				end

				function print_i_table(tbl)
				  for key, value in ipairs(tbl) do
                    print(key, value);
				  end
				end

				print_table(array_tab);
                print_i_table(array_tab); ";

			string Expected = "1\tapple\r\n2\tgrape\r\n3\torange\r\n1\tapple\r\n2\tgrape\r\n3\torange\r\n";

			// FIXME: 'pairs', 'next' doesn't work on List-based LuaTable

			ConsoleCompareTest(Code, Expected);
		}

		[Test()]
		public void IterHashTest()
		{
			string Code = @"
				hash_tab = { STR=1, AGI=2, INT=3 };
				function print_table(tbl)
				  for key, value in pairs(tbl) do
                    print(key, value);
				  end
				end

				function print_i_table(tbl)
				  for key, value in ipairs(tbl) do
                    print(key, value);
				  end
				end

				print_table(hash_tab);
                print_i_table(hash_tab); ";

			string Expected = "STR\t1\r\nAGI\t2\r\nINT\t3\r\n";

			ConsoleCompareTest(Code, Expected);
		}

		[Test()]
		public void AssignCompTest()
		{
			string Code = @"
				function assign_comp()
				  q = 4;
				  u = 7;
				  b = q <= u;
				  print(""b: "",b);
				end

				assign_comp(); ";

			string Expected = "b: \ttrue\r\n";

			ConsoleCompareTest(Code, Expected);
		}

		[Test()]
		public void NonCompIfTest()
		{
			string Code = @"
				function non_comp_if()
				  n = 13
				  if n then
				    print(""n 1 is true"");
				  end
				  
				  n = nil
				  if n then
				    print(""n 2 is true"");
				  end

				end

				non_comp_if();
			 ";

			string Expected = "n 1 is true\r\n";

			ConsoleCompareTest(Code, Expected);
		}

		[Test()]
		public void IfTest()
		{
			string Code = @"
				
				function if_else_if() 
				  n = 12
				  if n > 15 then
				    print(""the number is > 15"")
				  elseif n > 10 then
				    print(""the number is > 10"") 
				  elseif n > 5 then
				    print(""the number is > 5"")
				  else
				    print(""the number is <= 5"")
				  end
				end

				---

				function eq_ne()
				  n = 12
				  if n == 15 then
				    print(""the number is == 15"")
				  elseif n ~= 15 then
				    print(""the number is ~= 15"")
				  end
				end

				
				if_else_if();
				eq_ne();
				 ";

			string Expected = "the number is > 10\r\n"
							+ "the number is ~= 15\r\n"  ;

			ConsoleCompareTest(Code, Expected);
		}


		[Test()]
		public void WhileLoopTest()
		{
			string Code = @"
				i = 1
			  	while i <= 10 do
			    	print(i)
			    	i = i + 1
			  	end	 ";

			string Expected = "";
			for(int i = 1; i <= 10; i++) {
				Expected += i + "\r\n";
			}

			ConsoleCompareTest(Code, Expected);
		}

		[Test()]
		public void RepeatLoopTest()
		{
			string Code = @"
				i = 10
				repeat
					print(i)
					i = i - 1
				until i == 0 ";

			string Expected = "";
			for(int i = 10; i > 0; i--) {
				Expected += i + "\r\n";
			}

			ConsoleCompareTest(Code, Expected);
		}

		[Test()]
		public void ForNumLoopTest()
		{
			string Code = @"
				for i = 1, 100, 8 do
    				print(i)
  				end   ";

			string Expected = "";
			for(int i = 1; i <= 100; i+=8) {
				Expected += i + "\r\n";
			}

			ConsoleCompareTest(Code, Expected);
		}

		[Test()]
		public void ForNumNestLoopTest()
		{
			string Code = @"
                i = 0;
				local j = 5;
				for i = 1, 3 do
			    	for j = 1, i do
			    		print(j);
			  		end
				end
				print(i,j); ";

			string Expected = "";
			int i=0, j=5;
			{
				for(int ix = 1; ix <= 3; ix++) {
					for(int jx = 1; jx <= ix; jx++) {
						Expected += jx + "\r\n";
					}
				}
			}
			Expected += i + "\t" + j + "\r\n";

			ConsoleCompareTest(Code, Expected);
		}

		[Test()]
		public void ForIterLoopTest()
		{
			string Code = @"
				tbl = {""a"", ""b"", ""c""}
				for key, value in ipairs(tbl) do
					print(key, value)
				end  ";

			string Expected = "1\ta\r\n2\tb\r\n3\tc\r\n";

			ConsoleCompareTest(Code, Expected);
		}

	}
}

