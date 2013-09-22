using System;
using System.Collections.Generic;

namespace Cheese
{
	using Cheese.Machine;

	public abstract class LuaValue
	{

	}


	public class LuaNil : LuaValue {
		public static readonly LuaNil Nil = new LuaNil();

		private LuaNil() { }

		public override string ToString() {
			return "nil";
		}
	}

	public class LuaBool : LuaValue {
		public static readonly LuaBool True = new LuaBool(true);
		public static readonly LuaBool False = new LuaBool(false);

		public bool Value {
			get; 
			private set;
		}

		public LuaBool(bool Value) {
			this.Value = Value;
		}

		public override string ToString() {
			//return Value.ToString();
			return (this.Value ? "true" : "false");
		}
	}

	public class LuaString : LuaValue  {

		public static readonly LuaString Empty = new LuaString(string.Empty);

		public String Text {
			get;
			private set;
		}

		public LuaString(string Value) {
			Text = Value;
		}

		public override string ToString() {
			return Text;
		}

		public override bool Equals(Object Other) {
			if(Other == null)
				return false;
			else if(Other is string) 
				return this.Text.Equals(Other);
			else if(Other is LuaString)
				return this.Text == (Other as LuaString).Text;
			else
				return false;
		}
		public override int GetHashCode() {
			return Text.GetHashCode();
		}
	}

	public class LuaNumber : LuaValue {

		public static readonly LuaNumber Default = new LuaNumber(default(double));

		public double Number {
			get;
			internal set;
		}

		public LuaNumber(double Value) {
			Number = Value;
		}

		public override string ToString() {
			return Number.ToString();
		}

		public override bool Equals(Object Other) {
			if(Other == null)
				return false;
			else if(Other is double || Other is float) 
				return this.Number.Equals(Other);
			else if(Other is LuaNumber)
				return this.Number == (Other as LuaNumber).Number;
			else
				return false;
		}
		public override int GetHashCode() {
			return Number.GetHashCode();
		}

	}

	public class LuaInteger : LuaValue {

		public static readonly LuaInteger Default = new LuaInteger(default(long));

		public long Integer {
			get;
			internal set;
		}

		public LuaInteger(long Value) {
			Integer = Value;
		}

		public override string ToString() {
			return Integer.ToString();
		}

		public override bool Equals(Object Other) {
			if(Other == null)
				return false;
			else if(Other is double || Other is float) 
				return this.Integer.Equals(Other);
			else if(Other is LuaInteger)
				return this.Integer == (Other as LuaInteger).Integer;
			else
				return false;
		}
		public override int GetHashCode() {
			return Integer.GetHashCode();
		}
	}

	public class LuaTable : LuaValue {

		//public static readonly LuaTable Default = new LuaTable();

		private List<LuaValue> Array;
		// That the index is a LuaValue makes me unhappy, it means wrapping
		// everything. Inside the machine everything will be wrapped anyway,
		// but the interface out won't all be wrapped.
		// Make Key an Int, and do the calls to GetHashCode myself? 
		private Dictionary<LuaValue, LuaValue> HashMap;

		public LuaTable() {
			;
		}

		internal LuaTable(int ArraySize=0, int HashSize=0) {
			if(ArraySize > 0) {
				Array = new List<LuaValue>(ArraySize); 
				for(int i = 0; i < ArraySize; i++) {
					Array.Add(LuaNil.Nil);
				}
			}
			if(HashSize > 0)
				HashMap = new Dictionary<LuaValue, LuaValue>(HashSize);
		}

		public override string ToString() {
			return "lua table tostring";
		}

		// Lua is 1-based, and so is this.
		public LuaValue this[long Index]
		{
			get { 
				if(Array == null) 
					return LuaNil.Nil;
				else if(Array.Count <= Index-1)
					return LuaNil.Nil;
				else
					return Array[(int)Index-1]; }
			set { Array[(int)Index-1] = value; }
		}

		public LuaValue this[LuaValue Key]
		{
			get { 

				if(Key is LuaNumber) {
					double NumVal = (Key as LuaNumber).Number;
					if(NumVal % 1 == 0) {
						long IntVal = (long)NumVal;
						return this[IntVal];
					}
				}

				if(Key is LuaInteger) {
					return this[(Key as LuaInteger).Integer];
				}

				if(HashMap == null)
					return LuaNil.Nil;
				if(HashMap.ContainsKey(Key))
					return HashMap[Key];
				else
					return LuaNil.Nil;
			}
			set { 

				if(Key is LuaNumber) {
					double NumVal = (Key as LuaNumber).Number;
					if(NumVal % 1 == 0) {
						long IntVal = (long)NumVal;
						this[IntVal] = value;
						return;
					}
				}

				if(HashMap == null)
					HashMap = new Dictionary<LuaValue, LuaValue>();
				if(Key == LuaNil.Nil || value == LuaNil.Nil)
					HashMap.Remove(Key);
				else
					HashMap[Key] = value; 
			}
		}


		public int Length {
			get {
				if(Array == null)
					return 0;
				return Array.Count;
			}
		}

		public int Count {
			get {
				if(HashMap == null)
					return 0;
				return HashMap.Count;
			}
		}

		public IEnumerable<LuaValue> EnumerableArray
		{
			get { return this.Array; }
		}

		public IEnumerable<KeyValuePair<LuaValue,LuaValue>> EnumerableTable
		{
			get { return this.HashMap; }
		}


		public void Add(LuaValue Item) {
			if(Array == null)
				Array = new List<LuaValue>();
			Array.Add(Item);
		}

	}


	// LuaClosure , a Function and an UpVal storage wrapped
	public class LuaClosure : LuaValue {

		internal Cheese.Machine.Function Function;
		// UpVal storage?

		internal LuaClosure(Cheese.Machine.Function Func) {
			this.Function = Func;
		}

	}


	// LuaDelegate , a wrapper for a C# function
	public class LuaDelegate : LuaValue {

		public delegate LuaValue Delegate(LuaEnvironment Env, LuaTable Arguments);

		internal Delegate NativeDelegate;

		public LuaDelegate(Delegate Value) {
			NativeDelegate = Value;
		}


		public LuaValue Call(LuaEnvironment Env, LuaTable Arguments) {
			return NativeDelegate(Env, Arguments);
		}

	}


	// LuaSysDelegate , a wrapper for a C# function, but handles the stack directly
	internal class LuaSysDelegate : LuaValue {

		public delegate void SysDelegate(LuaEnvironment Env, VmStack Stack, 
		                                 int ArgCount, int RetCount);

		internal SysDelegate NativeSysDelegate;

		public LuaSysDelegate(SysDelegate Value) {
			NativeSysDelegate = Value;
		}


		public LuaValue Call(LuaEnvironment Env, VmStack Stack, 
		                     int ArgCount, int RetCount) {
			NativeSysDelegate(Env, Stack, ArgCount, RetCount);
			return LuaNil.Nil;
		}
	}

}

