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

		public IApiClient Client => useMockData ? mockApiClient : apiClient;
	}
}