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

			/////////////

			Code = @"
				st = { APPLE=1, ORANGE=2, PEAR=3 };
				local dt = {};
				dt.value = st.ORANGE;
				print(dt.value); ";

			Expected = "2\r\n";

			ConsoleCompareTest(Code, Expected);

			/////////////

			Code = @"
				st = { APPLE=1, ORANGE=2, PEAR=3 };
				local dt = { [st.APPLE]=""pie"", [st.ORANGE]=""juice"", [st.PEAR]=""can"" };
				print(dt[st.ORANGE]); ";

			Expected = "juice\r\n";

			ConsoleCompareTest(Code, Expected);

			/////////////

			Code = @"
				st = { APPLE=1, ORANGE=2, PEAR=3 };
				local dt = { [st.APPLE]=""pie"", [st.ORANGE]=""juice"", [st.PEAR]=""can"" };
				dt.parent = { ""APPLE"", ""ORANGE"", ""PEAR"" };
				print(dt.parent[st.ORANGE], dt[st.ORANGE]); ";

			Expected = "ORANGE\tjuice\r\n";

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
		public void UnmTest()
		{
			string Code = @"
				i = 10;
				j = -i;
				print(i,j); ";

			string Expected = "10\t-10\r\n";

			ConsoleCompareTest(Code, Expected);
		}

		[Test()]
		public void NotTest()
		{
			string Code = @"
				i = true;
				j = not i;
				print(i,j); ";

			string Expected = "true\tfalse\r\n";

			ConsoleCompareTest(Code, Expected);
		}

		[Test()]
		public void LenTest()
		{
			string Code = @"
				str = ""somestr"";
				lstr = #str;
				tbl = { ""a"", ""b"", ""c"", [""a""]=1, [""b""]=2, [""c""]=3 };
				ltbl = #tbl;
				print(lstr,ltbl); ";

			string Expected = "7\t6\r\n";

			ConsoleCompareTest(Code, Expected);
		}

		[Test()]
		public void ConcatTest()
		{
			string Code = @"
				object = { ""foo"", ""bar"", ""baz"" };

				t1 = object[1] .. object[2] .. object[3];

				t2 = '';
				for k,v in ipairs(object) do
				  t2 = t2 .. v;
				end

				print(t1, t2); ";

			string Expected = "foobarbaz\tfoobarbaz\r\n";

			ConsoleCompareTest(Code, Expected);
		}

		[Test()]
		public void PrecedenceTest()
		{
			string Code = @"
				x = 2 + 3 * 4 + 5;
                y = 2 + 3 ^ 2 * 3 + 4;
                z = -4^2;
                print(x, y, z); ";

			string Expected = "19\t33\t-16\r\n";

			ConsoleCompareTest(Code, Expected);
		}

		[Test()]
		public void TailCallTest()
		{
			string Code = @"
				function sum2(accu, n)
				  if n > 0 then
				    accu.value = accu.value + n;
				    return sum2(accu, n-1);
				  end
				end

				local accu = {value = 0}
				sum2(accu, 1000000)
				print(accu.value) ";

			string Expected = "500000500000\r\n";

			ConsoleCompareTest(Code, Expected);
		}


		[Test()]
		public void AssignCompTest()
		{
			string Code = @"
				function assign_comp()
				  q = 4;
				  u = 7;
                  a = q == u;
                  b = q ~= u;
				  c = q < u;
				  d = q > u;
				  e = q <= u;
				  f = q >= u;
				  print(a,b,c,d,e,f);
				end

				assign_comp(); ";

			string Expected = "false\ttrue\ttrue\tfalse\ttrue\tfalse\r\n";

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
		public void IfElseIfTest()
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
				
				if_else_if();";

			string Expected = "the number is > 10\r\n";

			ConsoleCompareTest(Code, Expected);
		}

		[Test()]
		public void CompOpTest()
		{
			string Code = @"
				one = 1;
				two = 2;
				three = 3;
				five = 5;

				other_three = one + two;

				if (one < two) then
					print(""one < two"");
				end

				if (two <= three) then
					print(""two <= three"");
				end

				if (three == other_three) then
					print(""three is three"");
				end				
				
				if (five > one) then
					print(""five > one"");
				end

				if ( (one+two) >= three) then
    				print(""math >= three"");
				end

				if (one ~= three) then
					print(""one is not three"");
				end
				 ";

			string Expected = "one < two\r\n"
							+ "two <= three\r\n"
							+ "three is three\r\n"
							+ "five > one\r\n"
							+ "math >= three\r\n"
							+ "one is not three\r\n";

			ConsoleCompareTest(Code, Expected);
		}


		[Test()]
		public void AssignLogiTest()
		{
			string Code = @"
				
				local A,B,C = 0,1,nil;				

				local D = (A and B);
				local E = (A or B);
				local F = (A and C);
				local G = (A or C);
				local H = (C and A);
				local I = (C or A);
				local J = (C or C or B or C or A);

				print(D,E,F,G,H,I,J);				
				";

			string Expected = "1\t0\tnil\t0\tnil\t0\t1\r\n";

			ConsoleCompareTest(Code, Expected);
		}

		[Test()]
		public void ComplexIfTest()
		{
			string Code = @"
				
				local A,B,C,D = 0,1,1,0;				

				if ( (A == B) and (C == D) ) then
					print(""1"");
				else 
					print(""2"");
				end

				if ( (A == B) or (C == D) ) then
					print(""3"");
				else 
					print(""4"");
				end		

				if ( (A == D) and (A ~= C) ) then
					print(""5"");
				else 
					print(""6"");
				end			

				";

			string Expected = "2\r\n4\r\n5\r\n";

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
		public void ForIterIPairsLoopTest()
		{
			string Code = @"
				tbl = {""a"", ""b"", ""c""}
				for key, value in ipairs(tbl) do
					print(key, value)
				end  ";

			string Expected = "1\ta\r\n2\tb\r\n3\tc\r\n";

			ConsoleCompareTest(Code, Expected);
		}

		[Test()]
		public void ForIterPairsLoopTest()
		{
			string Code = @"
				tbl = { ['a']=1, ['b']=2, ['c']=3}
				cpy = {}
				for key, value in pairs(tbl) do
					cpy[key] = (cpy[key] or 0) + value;
					print(key, value)
				end 	 
				for key, value in pairs(cpy) do
					print(key, value)
				end
				";

			string Expected = "a\t1\r\nb\t2\r\nc\t3\r\na\t1\r\nb\t2\r\nc\t3\r\n";

			ConsoleCompareTest(Code, Expected);
		}

		[Test()]
		public void AssignOrTest()
		{
			string Code = @"
				tbl = { ['a']=1, ['b']=2}
				tbl['b'] = (tbl['b'] or 0) + 2;
				tbl['c'] = (tbl['c'] or 0) + 3;
				print(tbl['b'],tbl['c']);
				";

			string Expected = "4\t3\r\n";

			ConsoleCompareTest(Code, Expected);
		}

		[Test()]
		public void RandomResultTest()
		{
			string Code = @"
                function dice(size)
					return math.random(size);
				end

				function testfunc()
					local first = dice(20);
					local second = dice(20);
					
					r = first + second;
					print(first, second, r);
				end

				testfunc();	";

			Machine.LuaEnvironment LuaEnv = new Machine.LuaEnvironment();
			StringWriter LocalOut = new StringWriter();
			LuaEnv.SetOutput(LocalOut);

			LuaEnv.Execute(Code);

			LuaValue R = LuaEnv.GetGlobalValue("r");


			Assert.True(R is LuaInteger);
			Assert.True((R as LuaInteger).Integer >= 0);
			Assert.True((R as LuaInteger).Integer <= 40);
		}


		[Test()]
		public void TableReturnTest()
		{
			string Code = @"
				function gen(parent)
                  local child = {};
                  child.base = parent;
                  child.generation = parent.generation + 1;
                  return child;
				end

                local start = {};
				start.generation = 1;

                local next = gen(start);
                print(next.generation); 

				local final = gen(next);
                print(final.generation);
                ";

			string Expected = "2\r\n3\r\n";

			ConsoleCompareTest(Code, Expected);
		}


		[Test()]
		public void TableFuncArgsTest()
		{
		// 	local res = math.max(character.stats[first], character.stats[second])			
			string Code = @"

                local first  = ""STR"";
				local second = ""AGI"";

				local character = {}
				character.stats = { STR=20, AGI=15, INT=12 };
				
				local res = math.max(character.stats[first], character.stats[second])			
				print(res);
				";

			string Expected = "20\r\n";

			ConsoleCompareTest(Code, Expected);
		}

		[Test()]
		public void HostCallClosureTest()
		{
			string Code = @"
				function print_repeat(msg, count)
                  accum = '';
                  for i = 1, count, 1 do
    				accum = accum .. msg;
  				  end  
				  print(accum);
                  return accum;
                end

				print_repeat(""a"", 3); ";

			string Expected = "aaa\r\nbbbbb\r\n";


			Machine.LuaEnvironment LuaEnv = new Machine.LuaEnvironment();
			StringWriter LocalOut = new StringWriter();
			LuaEnv.SetOutput(LocalOut);

			LuaEnv.Execute(Code);

			LuaClosure RptClosure = LuaEnv.GetGlobalValue("print_repeat") as LuaClosure;

			LuaTable Args = new LuaTable();
			Args.Add(new LuaString("b"));
			Args.Add(new LuaInteger(5));

			LuaTable Ret = LuaEnv.Execute_VarArg(RptClosure, Args);

			Assert.AreEqual(Expected, LocalOut.ToString());
		    
			Assert.AreEqual(1, Ret.Length);
			Assert.AreEqual("bbbbb", (Ret[1] as LuaString).Text);
		}

		[Test()]
		public void HostCallClosureOneTest()
		{
			string Code = @"
				function gen(parent)
                  local child = {};
                  child.base = parent;
                  child.generation = parent.generation + 1;
                  return child;
				end
                ";

			Machine.LuaEnvironment LuaEnv = new Machine.LuaEnvironment();
			StringWriter LocalOut = new StringWriter();
			LuaEnv.SetOutput(LocalOut);

			LuaEnv.Execute(Code);

			LuaClosure GenFunc = LuaEnv.GetGlobalValue("gen") as LuaClosure;

			LuaTable StartGen = new LuaTable();
			StartGen.Add(new LuaString("generation"), new LuaInteger(1));

			LuaValue Ret = LuaEnv.Execute(GenFunc, StartGen);
			LuaTable NextGen = Ret as LuaTable;

			Assert.AreEqual(2, (NextGen["generation"] as LuaInteger).Integer);
		}


		#region BaseLib
		[Test()]
		public void BaseToNumberTest()
		{
			string Code = @"
				local num = 0;
				num = tonumber(""123.45"");			
				print(num); 
				num = tonumber(""DEADBEEF"", 16);			
				print(num); 				
				";

			string Expected = "123.45\r\n3735928559\r\n";

			ConsoleCompareTest(Code, Expected);
		}

		[Test()]
		public void BaseToIntegerTest()
		{
			string Code = @"
				local int = 0;
				int = tointeger(""123.45"");			
				print(int); 
				int = tointeger(123.45);			
				print(int); 
				int = tointeger(""DEADBEEF"", 16);			
				print(int); 				
				";

			string Expected = "123\r\n123\r\n3735928559\r\n";

			ConsoleCompareTest(Code, Expected);
		}

		#endregion

		#region TableLib
		[Test()]
		public void TableConcatTest()
		{
			string Code = @"
				object = { ""foo"", ""bar"", ""baz"" };

				local ts = table.concat(object, ""!"");
				print(ts); 
				ts = table.concat(object, ""$"", 2);
				print(ts); 
				ts = table.concat(object, ""@"", 1, 2);
				print(ts); 
				";

			string Expected = "foo!bar!baz\r\nbar$baz\r\nfoo@bar\r\n";

			ConsoleCompareTest(Code, Expected);
		}

		[Test()]
		public void TableInsertTest()
		{
			string Code = @"
				object = { ""foo"", ""bar"", ""baz"" };

				table.insert(object, ""fizz"");
				table.insert(object, 2, ""buzz"");

				ts = '';
				for k,v in ipairs(object) do
				  ts = ts .. v;
				end

				print(ts); 

				";

			string Expected = "foobuzzbarbazfizz\r\n";

			ConsoleCompareTest(Code, Expected);
		}

		[Test()]
		public void TableMaxNTest()
		{
			string Code = @"
				object = { ""foo"", ""bar"", ""baz"", ""fizz"", ""buzz"" };				
				local maxV = table.maxn(object);
				print(maxV); 

				object[49.37] = ""what"";
				maxV = table.maxn(object);
				print(maxV); 
				";

			string Expected = "5\r\n49.37\r\n";

			ConsoleCompareTest(Code, Expected);
		}

		[Test()]
		public void TableRemoveTest()
		{
			string Code = @"
				object = { ""foo"", ""bar"", ""baz"", ""fizz"", ""buzz"" };

				table.remove(object);
				table.remove(object, 3);

				ts = '';
				for k,v in ipairs(object) do
				  ts = ts .. v;
				end

				print(ts); 

				";

			string Expected = "foobarfizz\r\n";

			ConsoleCompareTest(Code, Expected);
		}

		[Test()]
		public void TableRemoveInLoopTest()
		{

			string Code = @"
				local tt = { ""sauce"", ""juice"", ""can"" };
				
				local vic = ""juice"";
				for i,v in ipairs(tt) do
		    		if(v == vic) then
		    			table.remove(tt, i);
		    			break;
		    		end
				end
				
				for k,v in pairs(tt) do
		    		print(v);
				end

				";

			string Expected = "sauce\r\ncan\r\n";

			ConsoleCompareTest(Code, Expected);
		}

		#endregion

	}

	[TestFixture()]
	public class LuaTableTests
	{

		[Test()]
		public void ContainsKeyTest()
		{
			Machine.LuaEnvironment LuaEnv = new Machine.LuaEnvironment();
			StringWriter LocalOut = new StringWriter();
			LuaEnv.SetOutput(LocalOut);


			LuaTable Table = new LuaTable();

			Assert.True(Table.Empty);

			LuaString TestKey = new LuaString("tk");
			LuaString TestValue = new LuaString("tv");
			LuaInteger TestIndex = new LuaInteger(1);

			Assert.False(Table.ContainsKey(TestKey));
			Assert.False(Table.ContainsKey(TestKey.Text));

			Table.Add(TestKey, TestValue);
			Assert.False(Table.Empty);
			Assert.True(Table.ContainsKey(TestKey));
			Assert.True(Table.ContainsKey(TestKey.Text));

			Table.Add(TestValue);
			Assert.True(Table.ContainsKey(TestIndex));
		
		}

		[Test()]
		public void EnumeratorTest()
		{
			Machine.LuaEnvironment LuaEnv = new Machine.LuaEnvironment();
			StringWriter LocalOut = new StringWriter();
			LuaEnv.SetOutput(LocalOut);

			LuaTable Table = new LuaTable();
			LuaString TestKey = new LuaString("tk");
			LuaString TestValue = new LuaString("tv1");
			LuaString TestValue2 = new LuaString("tv2");
			LuaInteger TestIndex = new LuaInteger(1);

			Table.Add(TestKey, TestValue);
			Table.Add(TestValue);
			Table.Add(TestValue2, TestValue2);
			Table.Add(TestValue2);

			foreach(var Entry in Table) {
				LocalOut.Write("{0}:{1};", Entry.Key, Entry.Value);
			}

			Assert.AreEqual("1:tv1;2:tv2;tk:tv1;tv2:tv2;", LocalOut.ToString());

		}

		[Test()]
		public void TableMixUseTest()
		{
			string Code = @"
				card = {};
				card.test_table = { ""fighter"" }; 
			";

			string Expected = "aaa\r\nbbbbb\r\n";

			Machine.LuaEnvironment LuaEnv = new Machine.LuaEnvironment();
			StringWriter LocalOut = new StringWriter();
			LuaEnv.SetOutput(LocalOut);

			LuaEnv.Execute(Code);


			LuaTable CardTable = (LuaTable)LuaEnv.GetGlobalValue("card");
			LuaTable NestedTable = (LuaTable)CardTable["test_table"];

			foreach(var Entry in NestedTable) {
				LocalOut.Write("{0}:{1};", Entry.Key, Entry.Value);
			}

			Assert.AreEqual("1:fighter;", LocalOut.ToString());

		}
	
	

	}
}

