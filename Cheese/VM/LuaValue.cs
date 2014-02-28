using System;
using System.Collections;
using System.Collections.Generic;

namespace Cheese
{
	using Cheese.Machine;

	public abstract class LuaValue
	{

		public virtual long ToInteger() {
			if(this is LuaInteger)
				return (this as LuaInteger).Integer;
			else if(this is LuaNumber)
				return (long)(this as LuaNumber).Number;
			else if(this is LuaString) {
				if((this as LuaString).Text.Contains("."))
					return (long)Convert.ToDouble((this as LuaString).Text);
				else
					return Convert.ToInt64((this as LuaString).Text); 
			}
			return 0;
		}

		public virtual double ToNumber() {
			if(this is LuaInteger)
				return (double)(this as LuaInteger).Integer;
			else if(this is LuaNumber)
				return (this as LuaNumber).Number;
			else if(this is LuaString) {
				return Convert.ToDouble((this as LuaString).Text); 
			}
			return 0.0;
		}

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

		int Hash;
		public String Text {
			get;
			private set;
		}

		public LuaString(string Value) {
			Text = Value;
			Hash = Value.GetHashCode();
		}

		public override string ToString() {
			return Text;
		}

		public override bool Equals(Object Other) {
			if(Other == null)
				return false;
			else if(Other is string) 
				return (this.Text == (Other as string));
			else if(Other is LuaString)
				return this.Text == (Other as LuaString).Text;
			else
				return false;
		}
		public bool Equals(string Other) {
			if(Other == null)
				return false;
			else 
				return (this.Text == Other);
		}
		public bool Equals(LuaString Other) {
			if(Other == null)
				return false;
			else
				return this.Text == (Other as LuaString).Text;
		}
		public override int GetHashCode() {
			//return Text.GetHashCode();
			return Hash;
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

	// Special type that isn't Table to store Argument lists?
	// Is that just List<LuaValue> ?

	public class LuaUserData : LuaValue {

		public Object UserData {
			get;
			private set;
		}

		public LuaUserData(Object Value) {
			UserData = Value;
		}

		public override string ToString() {
			return UserData.ToString();
		}

		public override bool Equals(Object Other) {
			return UserData.Equals(Other);
		}
		public override int GetHashCode() {
			return UserData.GetHashCode();
		}
	}

	// LuaClosure , a Function and an UpVal storage wrapped
	public class LuaClosure : LuaValue {

		internal Cheese.Machine.Function Function;
		// UpVal storage?

		internal LuaClosure(Cheese.Machine.Function Func) {
			this.Function = Func;
		}

		// Call? LuaEnvironment?  see LuaEnvironment
		// Env.Call(Closure, Args) or Closure.Call(Env, Args);
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

