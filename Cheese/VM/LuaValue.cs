using System;

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

	}

	public class LuaNumber : LuaValue {

		public static readonly LuaNumber Default = new LuaNumber(default(double));

		public double Number {
			get;
			private set;
		}

		public LuaNumber(double Value) {
			Number = Value;
		}

		public override string ToString() {
			return Number.ToString();
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
	}


}

