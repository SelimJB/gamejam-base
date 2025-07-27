using UnityEngine;

namespace Staging
{
	public class CoinSpawner : MonoBehaviour
	{
		[SerializeField] private GameObject coinPrefab;
		[SerializeField] private int rows = 10;
		[SerializeField] private int columns = 10;
		[SerializeField] private Vector2 spacing = new(2f, 2f);
		[SerializeField] private Vector3 origin = Vector3.zero;

		private float timer;

		private void Start()
		{
			for (var x = 0; x < rows; x++)
			{
				for (var z = 0; z < columns; z++)
				{
					var position = origin + new Vector3(x * spacing.x, 0.5f, z * spacing.y);
					Instantiate(coinPrefab, position, Quaternion.identity, transform);
				}
			}
		}

		private void Update()
		{
			timer += Time.deltaTime;
			if (!(timer >= 0.2f)) return;

			timer = 0f;
			SpawnRandomCoin();
		}

		private void SpawnRandomCoin()
		{
			var randomPosition = new Vector3(Random.Range(origin.x, origin.x + rows * spacing.x),
				0.5f,
				Random.Range(origin.z, origin.z + columns * spacing.y));
			Instantiate(coinPrefab, randomPosition, Quaternion.identity, transform);
		}
	}
}