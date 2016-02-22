﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace VoxelEngine 
{
	
	[System.Serializable]
	public class MyChunkEvent : UnityEvent<ChunkPosition> {}

	[System.Serializable]
	public struct ChunkPosition
	{
		public int x, y, z;
		
		public ChunkPosition(int x, int y, int z)
		{
			this.x = x;
			this.y = y;
			this.z = z;
		}
		public ChunkPosition(float x, float y, float z)
		{
			this.x = Mathf.FloorToInt(x);
			this.y = Mathf.FloorToInt(y);
			this.z = Mathf.FloorToInt(z);
		}
		public Vector3 GetVector() {
			return new Vector3 (x, y, z);
		}
	}
	
	[System.Serializable]
	public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
	{
		[SerializeField]
		private List<TKey> keys = new List<TKey>();
		
		[SerializeField]
		public List<TValue> values = new List<TValue>();
		
		public void ClearLists() {
			keys.Clear ();
			values.Clear ();
		}
		// save the dictionary to lists
		public void OnBeforeSerialize()
		{
			SaveToLists ();
		}
		public void SaveToLists() {
			//Debug.LogError ("Trying to save: " + this.Count);
			ClearLists ();
				if (typeof(TKey).IsSubclassOf(typeof(UnityEngine.Object)) || typeof(TKey) == typeof(UnityEngine.Object)) 
			{
					foreach (var element in this) 
					{
						if (element.Key != null) 
						{
							keys.Add(element.Key);
							values.Add(element.Value);
						}
					}
				} else {
					//Debug.LogError("trying to read: " + this.Count);
				foreach (var element in this) {
					if (element.Key != null) 
					{
						keys.Add(element.Key);
						values.Add(element.Value);
					}
						/*try {
							TValue MyValue;
							if (this.TryGetValue(element.Key, out MyValue)) {
								keys.Add(element.Key);
								values.Add(MyValue);
							}
					} catch (System.IndexOutOfRangeException e) {
						//Debug.LogError ("Dictionary error.. Key and value dont match");
					}*/
				}
			}
		}
		// load dictionary from lists
		public void OnAfterDeserialize()
		{
			LoadFromLists();
		}
		public void LoadFromLists() {
			this.Clear ();
			
			if (keys.Count != values.Count)
				throw new System.Exception (string.Format ("there are {0} keys and {1} values after deserialization. Make sure that both key and value types are serializable."));
			
			for (int i = 0; i < keys.Count; i++)
				this.Add (keys [i], values [i]);
		}
	}
	[System.Serializable] public class DictionaryChunks : KVPList<ChunkPosition, Chunk> {}

	[System.Serializable]
	public class KVPList<TKey, TValue> : IDictionary<TKey, TValue>
	{
		[SerializeField]
		protected List<TKey> keys;
		[SerializeField]
		protected List<TValue> values;
		public List<TKey> Keys { get { return keys; } }
		public List<TValue> Values { get { return values; } }
		public int Count { get { return keys.Count; } }
		public KVPList() : this(new List<TKey>(), new List<TValue>())
		{
		}
		public KVPList(List<TKey> keys, List<TValue> values)
		{
			this.keys = keys; 
			this.values = values;
		}
		public TValue this[TKey key]
		{
			get
			{
				int index;
				if (!TryGetIndex(key, out index))
				{
					throw new KeyNotFoundException(key.ToString());
				}
				return values[index];
			}
			set
			{
				int index;
				if (!TryGetIndex(key, out index))
				{
					Add(key, value);
				}
				else values[index] = value;
			}
		}
		public void SetKeyAt(int i, TKey value)
		{
			AssertIndexInBounds(i);
			if (value != null && !value.Equals(keys[i]))
				AssertUniqueKey(value);
			keys[i] = value;
		}
		public TKey GetKeyAt(int i)
		{
			AssertIndexInBounds(i);
			return keys[i];
		}
		public void SetValueAt(int i, TValue value)
		{
			AssertIndexInBounds(i);
			values[i] = value;
		}
		public TValue GetValueAt(int i)
		{
			AssertIndexInBounds(i);
			return values[i];
		}
		public KeyValuePair<TKey, TValue> GetPairAt(int i)
		{
			AssertIndexInBounds(i);
			return new KeyValuePair<TKey, TValue>(keys[i], values[i]);
		}
		private void AssertIndexInBounds(int i)
		{
			//if (!keys.InBounds(i))
			//	throw new IndexOutOfRangeException("i");
		}
		public void Clear()
		{
			keys.Clear();
			values.Clear();
		}
		public void Insert(int i, TKey key, TValue value)
		{
			AssertUniqueKey(key);
			//Assert.ArgumentNotNull(key, "Dictionary key cannot be null");
			keys.Insert(i, key);
			values.Insert(i, value);
		}
		private void AssertUniqueKey(TKey key)
		{
			if (ContainsKey(key))
				throw new ArgumentException(string.Format("There's already a key `{0}` defined in the dictionary", key.ToString()));
		}
		public void Insert(TKey key, TValue value)
		{
			Insert(0, key, value);
		}
		public void Add(TKey key, TValue value)
		{
			Insert(Count, key, value);
		}
		public bool Remove(TKey key)
		{
			int index;
			if (TryGetIndex(key, out index))
			{
				keys.RemoveAt(index);
				values.RemoveAt(index);
				return true;
			}
			return false;
		}
		public void RemoveAt(int i)
		{
			AssertIndexInBounds(i);
			keys.RemoveAt(i);
			values.RemoveAt(i);
		}
		public void RemoveLast()
		{
			RemoveAt(Count - 1);
		}
		public void RemoveFirst()
		{
			RemoveAt(0);
		}
		public bool TryGetValue(TKey key, out TValue result)
		{
			int index;
			if (!TryGetIndex(key, out index))
			{
				result = default(TValue);
				return false;
			}
			result = values[index];
			return true;
		}
		public bool ContainsValue(TValue value)
		{
			return values.Contains(value);
		}
		public bool ContainsKey(TKey key)
		{
			return keys.Contains(key);
		}
		private bool TryGetIndex(TKey key, out int index)
		{
			return (index = keys.IndexOf(key)) != -1;
		}
		public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
		{
			for (int i = 0; i < Count; i++)
				yield return new KeyValuePair<TKey, TValue>(keys[i], values[i]);
		}
		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
		ICollection<TKey> IDictionary<TKey, TValue>.Keys
		{
			get { return keys; }
		}
		bool IDictionary<TKey, TValue>.Remove(TKey key)
		{
			return Remove(key);
		}
		ICollection<TValue> IDictionary<TKey, TValue>.Values
		{
			get { return values; }
		}
		public void Add(KeyValuePair<TKey, TValue> item)
		{
			keys.Add(item.Key);
			values.Add(item.Value);
		}
		public bool Contains(KeyValuePair<TKey, TValue> item)
		{
			return ContainsKey(item.Key);
		}
		public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
		{
			for (int i = arrayIndex; i < array.Length; i++)
			{
				array[i] = new KeyValuePair<TKey, TValue>(keys[i], values[i]);
			}
		}
		public bool IsReadOnly
		{
			get { return false; }
		}
		public bool Remove(KeyValuePair<TKey, TValue> item)
		{
			return Remove(item.Key);
		}
	}
	public static class KVPDictionaryExtensions
	{
		/*public static Dictionary<TKey, TValue> ToDictionary<TKey, TValue>(this KVPList<TKey, TValue> d)
		{
			return RTHelper.CreateDictionary(d.Keys, d.Values);
		}*/
		public static KVPList<TKey, TValue> ToKVPList<TKey, TValue>(this IDictionary<TKey, TValue> d)
		{
			return new KVPList<TKey, TValue>(d.Keys.ToList(), d.Values.ToList());
		}
	}
}
