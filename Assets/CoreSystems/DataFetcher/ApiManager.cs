using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace CoreSystems.DataFetcher
{
	[CreateAssetMenu(fileName = "ApiManager", menuName = "CoreSystems/Api/ApiManager")]
	public class ApiManager : ScriptableObject
	{
		[Header("Configuration")]
		[SerializeField] private bool useMockData;
		[SerializeField] private ApiClient apiClient;
		[SerializeField] private MockApiClient mockApiClient;

		[Header("Services (Optional)")]
		[SerializeField] private List<ScriptableObject> services = new();

		[Header("Debug")]
		[SerializeField] private bool enableDebugLogs = true;

		public IApiClient Client => useMockData ? mockApiClient : apiClient;

		public T GetService<T>() where T : ScriptableObject
		{
			var service = services.OfType<T>().FirstOrDefault();

			if (service == null)
				throw new System.Exception($"[ApiManager] Service of type {typeof(T).Name} not found.");

			return service;
		}

		private void OnValidate()
		{
			var duplicates = services
				.GroupBy(s => s.GetType())
				.Where(g => g.Count() > 1)
				.SelectMany(g => g.Skip(1));

			foreach (var duplicate in duplicates)
				Debug.LogWarning($"[ApiManager] Duplicate service found: {duplicate.name} ({duplicate.GetType().Name})");
		}
		
		public async Task<Result> Get(string endpoint, System.Action<string> onSuccess = null, System.Action<string> onError = null)
		{
			return await Client.Get(endpoint, onSuccess, onError);
		}
	}
}