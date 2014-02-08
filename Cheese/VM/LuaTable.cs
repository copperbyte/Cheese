using System;
using System.Collections;
using System.Collections.Generic;

using AlgoLib;

namespace Cheese
{
	using Cheese.Machine;

	public class LuaTable : LuaValue, IEnumerable<KeyValuePair<LuaValue,LuaValue>> {

		//public static readonly LuaTable Default = new LuaTable();

		private List<LuaValue> Array;
		// That the index is a LuaValue makes me unhappy, it means wrapping
		// everything. Inside the machine everything will be wrapped anyway,
		// but the interface out won't all be wrapped.
		// Make Key an Int, and do the calls to GetHashCode myself? 
		private Dictionary<LuaValue, LuaValue> HashMap;

		// Make a side-table for
		// Dictionary<string, LuaValue> StringMap ?

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
						if(Array != null && Array.Count >= IntVal)
							return this[IntVal];
					}
				}

				if(Key is LuaInteger) {
					if(Array != null && Array.Count >= (Key as LuaInteger).Integer)
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

		public LuaValue this[string Key]
		{
			get { 
				if(HashMap == null)
					return LuaNil.Nil;

				LuaString LKey = new LuaString(Key);
				if(HashMap.ContainsKey(LKey)) {
					return HashMap[LKey];
				}
				else
					return LuaNil.Nil;
			}
			set { 
				if(HashMap == null)
					HashMap = new Dictionary<LuaValue, LuaValue>();
				if(Key == null)
					return;
				LuaString LKey = new LuaString(Key);
				if(value == LuaNil.Nil)
					HashMap.Remove(LKey);
				else
					HashMap[LKey] = value; 
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


		public bool Empty {
			get {
				if(Array == null && HashMap == null)
					return true;
				if(Array != null && Array.Count > 0)
					return false;
				if(HashMap != null && HashMap.Count > 0)
					return false;
				return true;
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


		#region IEnumerable implementation
		IEnumerator<KeyValuePair<LuaValue, LuaValue>> 
		IEnumerable<KeyValuePair<LuaValue, LuaValue>>.GetEnumerator()
		{
			return new Enumerator(this);
		}
		#endregion
		#region IEnumerable implementation
		IEnumerator IEnumerable.GetEnumerator()
		{
			return new Enumerator(this);
		}
		#endregion


		public void Add(LuaValue Item) {
			if(Array == null)
				Array = new List<LuaValue>();
			Array.Add(Item);
		}

		public void Add(LuaValue Key, LuaValue Value) {
			if(HashMap == null)
				HashMap = new Dictionary<LuaValue, LuaValue>();
			HashMap.Add(Key, Value);
		}


		// ContainsKey 
		public bool ContainsKey(LuaValue Key) {
			if(Array != null) {
				if(Key is LuaNumber) {
					double NumVal = (Key as LuaNumber).Number;
					if(NumVal % 1 == 0) {
						long IntVal = (long)NumVal;
						if(Array.Count > IntVal-1)
							return true;
					}
				}

				if(Key is LuaInteger) {
					if(Array.Count > (Key as LuaInteger).Integer-1)
						return true;
				}
			}

			if(HashMap == null)
				return false;

			return HashMap.ContainsKey(Key);
		}

		public bool ContainsKey(string Key) {
			if(HashMap == null)
				return false;

			LuaString LKey = new LuaString(Key);
			return HashMap.ContainsKey(LKey);
		}


		// GetValue?

		public struct Enumerator : IEnumerator<KeyValuePair<LuaValue, LuaValue>>, 
		    IDisposable, IEnumerator  {

			private LuaTable Source;
			private IEnumerator<LuaValue> ArrayEnumer;
			private IEnumerator<KeyValuePair<LuaValue,LuaValue>> HashEnumer;
			private KeyValuePair<LuaValue, LuaValue> mCurrent;
			long ArrayIndex;
			private enum EState
			{ 
				eInit,
				eArray,
				eHash,
				eEnd
			};
			EState State;

			private bool disposed;

			public Enumerator(LuaTable Table) 
			{
				disposed = false;
				Source = Table;
				ArrayEnumer = null;
				HashEnumer = null;
				ArrayIndex = 0;
				State = EState.eInit;
				mCurrent = new KeyValuePair<LuaValue, LuaValue>();
				Reset();
			}

			public void Reset() {
				ArrayIndex = 0;
				State = EState.eInit;

				if(Source.Array != null)
					ArrayEnumer = Source.Array.GetEnumerator();
				if(Source.HashMap != null)
					HashEnumer = Source.HashMap.GetEnumerator();

				if(ArrayEnumer == null && HashEnumer == null)
					State = EState.eEnd;
			}

			public KeyValuePair<LuaValue, LuaValue> Current { 
				get { return mCurrent; }
			}

			Object IEnumerator.Current { 
				get { return mCurrent; }
			}

			public bool MoveNext() {
				// true if the enumerator was successfully advanced to the next element; 
				// false if the enumerator has passed the end of the collection.

				if(State == EState.eEnd) {
					return false;
				}

				if(State == EState.eInit) {
					if(ArrayEnumer != null)
						State = EState.eArray;
					else if(HashEnumer != null)
						State = EState.eHash;
					else
						State = EState.eEnd;
				}

				if(State == EState.eArray) {
					bool Advanced = ArrayEnumer.MoveNext();
					if(Advanced) {
						ArrayIndex++;
						mCurrent = new KeyValuePair<LuaValue, LuaValue>(
							new LuaInteger(ArrayIndex),
							ArrayEnumer.Current);
						return true;
					} else {
						if(HashEnumer != null)
							State = EState.eHash;
						else
							State = EState.eEnd;
					}
				}

				if(State == EState.eHash) {
					bool Advanced = HashEnumer.MoveNext();
					if(Advanced) {
						mCurrent = new KeyValuePair<LuaValue, LuaValue>(
							HashEnumer.Current.Key,
							HashEnumer.Current.Value);
						return true;
					} else {
						State = EState.eEnd;
					}
				}

				if(State == EState.eEnd) {
					return false;
				}

				return false;
			} 
		
			public void Dispose()
			{
				if(!this.disposed)
				{
					disposed = true;
					if(ArrayEnumer != null)
						ArrayEnumer.Dispose();
					if(HashEnumer != null)
						HashEnumer.Dispose();
				}
			}

		} // end Enumerator
	}

}
