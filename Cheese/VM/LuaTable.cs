using System;
using System.Collections;
using System.Collections.Generic;

using AlgoLib;

namespace Cheese
{
	using Cheese.Machine;

	public class LuaTable : LuaValue, IEnumerable<KeyValuePair<LuaValue,LuaValue>> {

		//public static readonly LuaTable Default = new LuaTable();

		private List<LuaValue> Array; // ordered LuaValue array
		private Trie<LuaValue> Trie; // String to LuaValue
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
			if(HashSize > 0) {
				Trie = new Trie<LuaValue>();
				HashMap = new Dictionary<LuaValue, LuaValue>(HashSize);
			}
		}

		public override string ToString() {
			return "lua table tostring";
		}

		#region indexers
		// Lua is 1-based, and so is this.
		public LuaValue this[long Index]
		{
			get { 
				return Get(Index);
			}
			set { Array[(int)Index-1] = value; }
		}

		public LuaValue this[LuaValue Key]
		{
			get { 
				return Get(Key);
			}
			set { 
				Add(Key, value);
			}
		}

		public LuaValue this[string Key]
		{
			get { 
				return Get(Key);
			}
			set { 
				Add(Key, value);
			}
		}
		#endregion // indexers

		public int Length {
			get {
				if(Array == null)
					return 0;
				return Array.Count;
			}
		}

		public int Count {
			get {
				int R = 0;
				if(HashMap == null)
					;
				else
					R += HashMap.Count;
				if(Trie == null)
					;
				else
					R += Trie.Count;
				return R;
			}
		}


		public bool Empty {
			get {
				if(Array == null && Trie == null && HashMap == null)
					return true;
				if(Array != null && Array.Count > 0)
					return false;
				if(Trie != null && Trie.Count > 0)
					return false;
				if(HashMap != null && HashMap.Count > 0)
					return false;
				return true;
			}
		}

		/*
		public IEnumerable<LuaValue> EnumerableArray
		{
			get { return this.Array; }
		}

		public IEnumerable<KeyValuePair<LuaValue,LuaValue>> EnumerableTable
		{
			get { return this.HashMap; }
		}
		*/

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

		#region Add
		public void Add(LuaValue Item) {
			if(Array == null)
				Array = new List<LuaValue>();
			Array.Add(Item);
		}

		public void Add(LuaValue Key, LuaValue Value) {
			if(Key is LuaString) {
				Add((Key as LuaString).Text, Value);
				return;
			}
			if(HashMap == null)
				HashMap = new Dictionary<LuaValue, LuaValue>();
			if(Value != LuaNil.Nil && Value != null)
				HashMap[Key]= Value;
			else 
				HashMap.Remove(Key);
		}

		public void Add(LuaString Key, LuaValue Value) {
			if(Trie == null)
				Trie = new Trie<LuaValue>();
			if(Value != LuaNil.Nil && Value != null)
				Trie[Key.Text] = Value;//Trie.Add(Key.Text, Value);
			else
				Trie.Remove(Key.Text);
		}

		public void Add(string Key, LuaValue Value) {
			if(Trie == null)
				Trie = new Trie<LuaValue>();
			if(Value != LuaNil.Nil && Value != null)
				Trie[Key] = Value;//Trie.Add(Key, Value);
			else
				Trie.Remove(Key);
		}
		public void Add(long Key, LuaValue Value) {
			if(Array == null)
				Array = new List<LuaValue>();

			if(Array.Count >= Key + 1) {
				Array[(int)Key - 1] = Value;
				return;
			} else if(Array.Count * 2 >= Key + 1) {
				while(Array.Count <= (Key+1)) {
					Array.Add(LuaNil.Nil);
				}
				Array[(int)Key - 1] = Value;
				return;
			}
			else {
				LuaInteger LInt = new LuaInteger(Key);
				Add(LInt, Value);
			}

		}
		#endregion

		#region Insert
		public void Insert(long Key, LuaValue Value) {
			if(Array == null)
				Array = new List<LuaValue>();

			Array.Insert((int)Key-1, Value);
		}
		#endregion

		#region Remove
		public void Remove(long Key) {
			if(Array == null)
				return;

			if(Array.Count > (Key - 1)) {
				Array.RemoveAt((int)Key - 1);
			}
		}
		#endregion

		#region Clear
		public void Clear() {
			if(Array != null) {
				Array.Clear();
				Array = null;
			}

			if (Trie != null) {
				Trie.Clear ();
				Trie = null;
			}

			if (HashMap != null) {
				HashMap.Clear ();
				HashMap = null;
			}
		}
		#endregion

		#region ContainsKey 
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

			if(Key is LuaString)
				return ContainsKey((Key as LuaString).Text);

			if(HashMap == null)
				return false;

			return HashMap.ContainsKey(Key);
		}

		public bool ContainsKey(string Key) {
			if(Trie == null)
				return false;
			return Trie.ContainsKey(Key);
		}
		#endregion

		#region Get
		public LuaValue Get(LuaValue Key) {
			if(Key is LuaString) {
				return Get((Key as LuaString).Text);
			}

			if(Key is LuaNumber) {
				double NumVal = (Key as LuaNumber).Number;
				if(NumVal % 1 == 0) {
					long IntVal = (long)NumVal;
					return Get(IntVal);
				}
			}

			if(Key is LuaInteger) {
				LuaValue R = Get((Key as LuaInteger).Integer);
				if(R != LuaNil.Nil && R != null)
					return R;
			}

			if(HashMap == null)
				return LuaNil.Nil;
			if(HashMap.ContainsKey(Key))
				return HashMap[Key];
			else
				return LuaNil.Nil;
		}
		public LuaValue Get(LuaString Key) {
			if(Trie == null)
				return LuaNil.Nil;
			if(Trie.ContainsKey(Key.Text)) {
				return Trie[Key.Text];
			}
			else
				return LuaNil.Nil;
		}
		public LuaValue Get(string Key) {
			if(Trie == null)
				return LuaNil.Nil;
			if(Trie.ContainsKey(Key)) {
				return Trie[Key];
			}
			else
				return LuaNil.Nil;
		}
		public LuaValue Get(long Key) {
			if(Array != null && Array.Count >= Key-1)
				return Array[(int)Key-1];
			return LuaNil.Nil;
		}
		#endregion


		public struct Enumerator : IEnumerator<KeyValuePair<LuaValue, LuaValue>>, 
		    IDisposable, IEnumerator  {

			private LuaTable Source;
			private IEnumerator<LuaValue> ArrayEnumer;
			private IEnumerator<KeyValuePair<string,LuaValue>> TrieEnumer;
			private IEnumerator<KeyValuePair<LuaValue,LuaValue>> HashEnumer;
			private KeyValuePair<LuaValue, LuaValue> mCurrent;
			long ArrayIndex;
			private enum EState
			{ 
				eInit,
				eArray,
				eTrie,
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
				TrieEnumer = null;
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
				if(Source.Trie != null)
					TrieEnumer = Source.Trie.GetEnumerator();
				if(Source.HashMap != null)
					HashEnumer = Source.HashMap.GetEnumerator();

				if(ArrayEnumer == null && TrieEnumer == null && HashEnumer == null)
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
					else if(TrieEnumer != null)
						State = EState.eTrie;
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
						if(TrieEnumer != null)
							State = EState.eTrie;
						else if(HashEnumer != null)
							State = EState.eHash;
						else
							State = EState.eEnd;
					}
				}

				if(State == EState.eTrie) {
					bool Advanced = TrieEnumer.MoveNext();
					if(Advanced) {
						mCurrent = new KeyValuePair<LuaValue, LuaValue>(
							new LuaString(TrieEnumer.Current.Key),
							TrieEnumer.Current.Value);
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
					if(TrieEnumer != null)
						TrieEnumer.Dispose();
					if(HashEnumer != null)
						HashEnumer.Dispose();
				}
			}

		} // end Enumerator
	}

}

