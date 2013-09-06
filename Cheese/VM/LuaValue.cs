using System;
using System.Collections.Generic;

namespace Cheese
{
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
			return Value.ToString();
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

		public bool Equals(LuaNumber Other) {
			return this.Number == Other.Number;
		}
		public override int GetHashCode() {
			return Number.GetHashCode();
		}

	}

	public class LuaInteger : LuaValue {

		public static readonly LuaInteger Default = new LuaInteger(default(int));

		public int Integer {
			get;
			private set;
		}

		public LuaInteger(int Value) {
			Integer = Value;
		}

		public override string ToString() {
			return Integer.ToString();
		}

		public bool Equals(LuaInteger Other) {
			return this.Integer == Other.Integer;
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

		public override string ToString() {
			return "lua table tostring";
		}

		public LuaValue this[int Index]
		{
			get { 
				if(Array == null) 
					return LuaNil.Nil;
				else if(Array.Count >= Index)
					return LuaNil.Nil;
				else
					return Array[Index]; }
			set { Array[Index] = value; }
		}

		public LuaValue this[LuaValue Key]
		{
			get { 
				if(HashMap == null)
					return LuaNil.Nil;
				if(HashMap.ContainsKey(Key))
					return HashMap[Key];
				else
					return LuaNil.Nil;
			}
			set { 
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


		public void Add(LuaValue Item) {
			if(Array == null)
				Array = new List<LuaValue>();
			Array.Add(Item);
		}

	}


	// LuaClosure , a Function and an UpVal storage wrapped

	// LuaDelegate , a wrapper for a C# function
	public class LuaDelegate : LuaValue {

		public delegate LuaValue Delegate(LuaTable Arguments);

		internal Delegate NativeDelegate;

		public LuaDelegate(Delegate Value) {
			NativeDelegate = Value;
		}


		public LuaValue Call(LuaTable Arguments) {
			return NativeDelegate(Arguments);
		}

	}

}

