//Object Pool ( http://unitypatterns.com/resource/objectpool/ ) is licensed under:
/* The MIT License (MIT)
Copyright (c) 2013 UnityPatterns
Permission is hereby granted, free of charge, to any person obtaining a copy of
this software and associated documentation files (the “Software”), to deal in the
Software without restriction, including without limitation the rights to use, copy,
modify, merge, publish, distribute, sublicense, and/or sell copies of the Software,
and to permit persons to whom the Software is furnished to do so, subject to the
following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED “AS IS”, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A
PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF
CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE
OR THE USE OR OTHER DEALINGS IN THE SOFTWARE. */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public sealed class ObjectPool : MonoBehaviour
{
	static ObjectPool _instance;

	Dictionary<Component, List<Component>> objectLookup = new Dictionary<Component, List<Component>>();
	Dictionary<Component, Component> prefabLookup = new Dictionary<Component, Component>();
	
	public static void Clear()
	{
		instance.objectLookup.Clear();
		instance.prefabLookup.Clear();
	}

	public static void CreatePool<T>(T prefab) where T : Component
	{
		if(prefab==null)
			return;
		if (!instance.objectLookup.ContainsKey(prefab))
			instance.objectLookup.Add(prefab, new List<Component>());
	}
	
	public static T Spawn<T>(T prefab, Vector3 position, Quaternion rotation) where T : Component
	{
		if(prefab==null)
			return null;
		if (instance.objectLookup.ContainsKey(prefab))
		{
			T obj = null;
			var list = instance.objectLookup[prefab];
			if (list.Count > 0)
			{
				while (obj == null && list.Count > 0)
				{
					obj = list[0] as T;
					list.RemoveAt(0);
				}
				if (obj != null)
				{
					obj.transform.parent = null;
					obj.transform.localPosition = position;
					obj.transform.localRotation = rotation;
					obj.gameObject.SetActive(true);
					instance.prefabLookup.Add(obj, prefab);
					obj.SendMessage("OnSpawned", null, SendMessageOptions.DontRequireReceiver);
					return (T)obj;
				}
			}
			obj = (T)Object.Instantiate(prefab, position, rotation);
			obj.name = prefab.name;
			instance.prefabLookup.Add(obj, prefab);
			obj.SendMessage("OnSpawned", null, SendMessageOptions.DontRequireReceiver);
			return (T)obj;
		}
		else {
			T obj = null;
			obj = (T)Object.Instantiate(prefab, position, rotation);
			obj.name=prefab.name;
			return (T)obj;
		}
	}
	public static T Spawn<T>(T prefab, Vector3 position) where T : Component
	{
		return Spawn(prefab, position, Quaternion.identity);
	}
	public static T Spawn<T>(T prefab) where T : Component
	{
		return Spawn(prefab, Vector3.zero, Quaternion.identity);
	}

	public static void Recycle<T>(T obj) where T : Component
	{
		if (instance.prefabLookup.ContainsKey(obj))
		{
			instance.objectLookup[instance.prefabLookup[obj]].Add(obj);
			instance.prefabLookup.Remove(obj);
			obj.transform.parent = instance.transform;
			obj.gameObject.SetActive(false);
		}
		else
			Object.Destroy(obj.gameObject);
	}

	public static int Count<T>(T prefab) where T : Component
	{
		if (instance.objectLookup.ContainsKey(prefab))
			return instance.objectLookup[prefab].Count;
		else
			return 0;
	}

	public static ObjectPool instance
	{
		get
		{
			if (_instance != null)
				return _instance;
			var obj = new GameObject("_ObjectPool");
			obj.transform.localPosition = Vector3.zero;
			_instance = obj.AddComponent<ObjectPool>();
			return _instance;
		}
	}
}

public static class ObjectPoolExtensions
{
	public static void CreatePool<T>(this T prefab) where T : Component
	{
		ObjectPool.CreatePool(prefab);
	}
	
	public static T Spawn<T>(this T prefab, Vector3 position, Quaternion rotation) where T : Component
	{
		return ObjectPool.Spawn(prefab, position, rotation);
	}
	public static T Spawn<T>(this T prefab, Vector3 position) where T : Component
	{
		return ObjectPool.Spawn(prefab, position, Quaternion.identity);
	}
	public static T Spawn<T>(this T prefab) where T : Component
	{
		return ObjectPool.Spawn(prefab, Vector3.zero, Quaternion.identity);
	}
	
	public static void Recycle<T>(this T obj) where T : Component
	{
		ObjectPool.Recycle(obj);
	}

	public static int Count<T>(T prefab) where T : Component
	{
		return ObjectPool.Count(prefab);
	}
}
