using System;
using System.Collections.Generic;
using UnityEngine;

namespace CoreSystems.VFX
{
	public class ParticleVFXManager : MonoBehaviour
	{
		[SerializeField] private List<VFXEntry> vfxPrefabs;

		private readonly Dictionary<string, GameObject> vfxDictionary = new();
		private readonly Dictionary<GameObject, Queue<GameObject>> pool = new();

		[Serializable]
		public class VFXEntry
		{
			public string id;
			public GameObject prefab;
		}

		private static ParticleVFXManager instance;
		public static ParticleVFXManager Instance
		{
			get
			{
				if (instance == null)
				{
					instance = FindObjectOfType<ParticleVFXManager>();
				}

				return instance;
			}
		}

		private void Awake()
		{
			if (instance == null)
			{
				instance = this;
				DontDestroyOnLoad(gameObject);
			}
			else if (instance != this)
			{
				Destroy(gameObject);
			}

			foreach (var entry in vfxPrefabs)
			{
				pool[entry.prefab] = new Queue<GameObject>();
				vfxDictionary[entry.id] = entry.prefab;
			}
		}

		public void Spawn(GameObject prefabVfx, Vector3 position, bool verbose = false)
		{
			if (!pool.ContainsKey(prefabVfx))
			{
				pool[prefabVfx] = new Queue<GameObject>();
				Debug.Log($"Added {prefabVfx.name} to the VFX pool.");
			}

			GameObject vfx;

			if (pool[prefabVfx].Count > 0)
			{
				vfx = pool[prefabVfx].Dequeue();
				vfx.SetActive(true);
			}
			else
			{
				vfx = Instantiate(prefabVfx);
			}

			vfx.transform.position = position;
			vfx.transform.parent = transform;
			var particleSystem = vfx.GetComponent<ParticleSystem>();
			particleSystem.Play();

			if (verbose)
				Debug.Log(particleSystem.name + " VFX spawned at " + position + "duration: " + particleSystem.main.duration);

			StartCoroutine(ReturnToPoolAfter(vfx, particleSystem.main.duration + 0.1f, prefabVfx));
		}

		public void Spawn(string id, Vector3 position, bool verbose = false)
		{
			var vfxPrefab = vfxDictionary.GetValueOrDefault(id);

			if (vfxPrefab == null)
			{
				Debug.LogError($"VFX with ID '{id}' not found.");
				return;
			}

			Spawn(vfxPrefab, position, verbose);
		}

		private System.Collections.IEnumerator ReturnToPoolAfter(GameObject go, float delay, GameObject prefabVfx)
		{
			yield return new WaitForSeconds(delay);
			go.SetActive(false);
			pool[prefabVfx].Enqueue(go);
		}
	}
}