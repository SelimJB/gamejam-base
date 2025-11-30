using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace CoreSystems.DataFetcher
{
	public enum ApiType
	{
		Any,
		RestCountries,
		GameBackend,
		Analytics
	}

	[Serializable]
	public class MockResponse
	{
		[Header("API Context")]
		public ApiType apiType = ApiType.Any;

		[Header("Endpoint Configuration")]
		public string endpoint;
		[TextArea(3, 10)]
		public string jsonResponse;

		[Header("Error Simulation")]
		public bool shouldFail;
		[TextArea(2, 5)]
		public string errorMessage;
		public int statusCode = 200;

		[Header("Network Simulation")]
		[Range(0f, 5f)]
		public float delaySeconds = 0.1f;
	}

	[CreateAssetMenu(fileName = "MockApiClient", menuName = "ScriptableObjects/Api/MockApiClient")]
	public class MockApiClient : ScriptableObject, IApiClient
	{
		[Header("Mock Responses")]
		[SerializeField] private List<MockResponse> mockResponses = new List<MockResponse>();

		[Header("Network Simulation")]
		[SerializeField] private bool simulateNetworkDelay = true;
		[SerializeField] private float defaultDelaySeconds = 0.5f;

		[Header("Debug")]
		[SerializeField] private bool enableLogging = true;

		private string baseUrl;
		private string authToken;
		private ApiType currentApiType = ApiType.Any;

		public void SetAuthToken(string token)
		{
			authToken = token;
			if (enableLogging)
				Debug.Log($"[MockDataFetcher] Auth token set: {(!string.IsNullOrEmpty(token) ? "***" : "null")}");
		}

		public void SetBaseUrl(string url)
		{
			baseUrl = url;

			// Détecter le type d'API selon l'URL
			currentApiType = DetectApiType(url);

			if (enableLogging)
				Debug.Log($"[MockDataFetcher] Base URL set to: {baseUrl} (Detected API: {currentApiType})");
		}

		/// <summary>
		/// Set the API context manually (alternative to URL detection)
		/// </summary>
		public void SetApiType(ApiType apiType)
		{
			currentApiType = apiType;
			if (enableLogging)
				Debug.Log($"[MockDataFetcher] API type set to: {apiType}");
		}

		private ApiType DetectApiType(string url)
		{
			if (string.IsNullOrEmpty(url)) return ApiType.Any;

			url = url.ToLower();
			if (url.Contains("restcountries")) return ApiType.RestCountries;
			if (url.Contains("game") || url.Contains("backend")) return ApiType.GameBackend;
			if (url.Contains("analytics")) return ApiType.Analytics;

			return ApiType.Any;
		}

		/// <summary>
		/// Add a mock response for testing
		/// </summary>
		public void AddMockResponse(string endpoint, string jsonResponse, bool shouldFail = false, string errorMessage = "", int statusCode = 200)
		{
			mockResponses.Add(new MockResponse
			{
				endpoint = endpoint,
				jsonResponse = jsonResponse,
				shouldFail = shouldFail,
				errorMessage = errorMessage,
				statusCode = statusCode
			});
		}

		/// <summary>
		/// Add a mock response with object serialization
		/// </summary>
		public void AddMockResponse<T>(string endpoint, T responseObject, bool shouldFail = false, string errorMessage = "", int statusCode = 200)
		{
			string json = JsonUtility.ToJson(responseObject);
			AddMockResponse(endpoint, json, shouldFail, errorMessage, statusCode);
		}

		public async Task<Result> Get(string endpoint, Action<string> onSuccess = null, Action<string> onError = null)
		{
			if (enableLogging)
				Debug.Log($"[MockDataFetcher] 🎭 GET: {endpoint}");

			// Simuler la latence réseau
			if (simulateNetworkDelay)
			{
				await Task.Delay((int)(defaultDelaySeconds * 1000));
			}

			var mockResponse = FindMockResponse(endpoint);

			if (mockResponse == null)
			{
				var errorResult = Result.CreateError($"No mock response found for endpoint: {endpoint}", 404);
				if (enableLogging)
					Debug.LogError($"[MockDataFetcher] ❌ No mock found for: {endpoint}");
				onError?.Invoke(errorResult.ErrorMessage);
				return errorResult;
			}

			// Simuler délai spécifique à la réponse
			if (mockResponse.delaySeconds > 0)
			{
				await Task.Delay((int)(mockResponse.delaySeconds * 1000));
			}

			if (mockResponse.shouldFail)
			{
				var errorResult = Result.CreateError(mockResponse.errorMessage, mockResponse.statusCode);
				if (enableLogging)
					Debug.LogError($"[MockDataFetcher] ❌ Simulated error for {endpoint}: {mockResponse.errorMessage}");
				onError?.Invoke(errorResult.ErrorMessage);
				return errorResult;
			}

			var successResult = Result.CreateSuccess(mockResponse.jsonResponse, mockResponse.statusCode);
			if (enableLogging)
				Debug.Log($"[MockDataFetcher] ✅ Mock response for {endpoint} (Status: {mockResponse.statusCode})");
			onSuccess?.Invoke(mockResponse.jsonResponse);
			return successResult;
		}

		public async Task<Result> Post(string endpoint, string jsonData, Action<string> onSuccess = null, Action<string> onError = null)
		{
			if (enableLogging)
				Debug.Log($"[MockDataFetcher] 🎭 POST: {endpoint}\nData: {jsonData}");

			// Simuler la latence réseau
			if (simulateNetworkDelay)
			{
				await Task.Delay((int)(defaultDelaySeconds * 1000));
			}

			var mockResponse = FindMockResponse(endpoint);

			if (mockResponse == null)
			{
				var errorResult = Result.CreateError($"No mock response found for endpoint: {endpoint}", 404);
				if (enableLogging)
					Debug.LogError($"[MockDataFetcher] ❌ No mock found for POST: {endpoint}");
				onError?.Invoke(errorResult.ErrorMessage);
				return errorResult;
			}

			// Simuler délai spécifique à la réponse
			if (mockResponse.delaySeconds > 0)
			{
				await Task.Delay((int)(mockResponse.delaySeconds * 1000));
			}

			if (mockResponse.shouldFail)
			{
				var errorResult = Result.CreateError(mockResponse.errorMessage, mockResponse.statusCode);
				if (enableLogging)
					Debug.LogError($"[MockDataFetcher] ❌ Simulated POST error for {endpoint}: {mockResponse.errorMessage}");
				onError?.Invoke(errorResult.ErrorMessage);
				return errorResult;
			}

			var successResult = Result.CreateSuccess(mockResponse.jsonResponse, mockResponse.statusCode);
			if (enableLogging)
				Debug.Log($"[MockDataFetcher] ✅ Mock POST response for {endpoint} (Status: {mockResponse.statusCode})");
			onSuccess?.Invoke(mockResponse.jsonResponse);
			return successResult;
		}

		private MockResponse FindMockResponse(string endpoint)
		{
			// Filtrer d'abord par type d'API
			var candidateResponses = mockResponses.Where(r =>
				r.apiType == ApiType.Any || r.apiType == currentApiType).ToList();

			// Try exact match first
			foreach (var response in candidateResponses)
			{
				if (response.endpoint == endpoint)
					return response;
			}

			// Try partial match (useful for endpoints with parameters)
			foreach (var response in candidateResponses)
			{
				if (endpoint.Contains(response.endpoint) || response.endpoint.Contains(endpoint))
					return response;
			}

			return null;
		}

		/// <summary>
		/// Clear all mock responses
		/// </summary>
		public void ClearMockResponses()
		{
			mockResponses.Clear();
		}

		/// <summary>
		/// Setup common mock responses for testing
		/// </summary>
		[ContextMenu("Setup Test Data")]
		public void SetupTestData()
		{
			ClearMockResponses();

			// Example mock responses
			AddMockResponse("/api/user/profile", "{\"name\":\"Test User\",\"level\":5,\"score\":1250}");
			AddMockResponse("/api/leaderboard", "{\"players\":[{\"name\":\"Player1\",\"score\":2000},{\"name\":\"Player2\",\"score\":1500}]}");
			AddMockResponse("/api/error-test", "", true, "Simulated server error", 500);
		}
	}
}