using UnityEngine;

namespace CoreSystems.DataFetcher
{
	[CreateAssetMenu(fileName = "ApiClientFactory", menuName = "CoreSystems/Api/ApiClientFactory")]
	public class ApiManager : ScriptableObject
	{
		[SerializeField] private bool useMockData;
		[SerializeField] private ApiClient apiClient;
		[SerializeField] private MockApiClient mockApiClient;

		[Header("Debug")]
		[SerializeField] private bool enableDebugLogs = true;

		private IApiClient GetClient()
		{
			IApiClient client = useMockData ? mockApiClient : apiClient;

			if (enableDebugLogs)
				Debug.Log($"[ApiClientFactory] Created {(useMockData ? "Mock" : "Real")} client: {client.GetType().Name}");

			return client;
		}

		public IApiClient Client => GetClient();
	}
}