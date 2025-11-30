using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace CoreSystems.DataFetcher
{
	[CreateAssetMenu(fileName = "MockApiClient", menuName = "CoreSystems/Api/MockApiClient")]
	public class MockApiClient : ScriptableObject, IApiClient
	{
		[Header("Mock Responses by Base URL")]
		[Tooltip("Group mock responses by API base URL")]
		[SerializeField] private List<MockResponse> mockResponses = new();

		[Header("Network Simulation")]
		[SerializeField] private bool simulateNetworkDelay = true;
		[SerializeField] private float defaultDelaySeconds = 0.5f;

		[Header("Debug")]
		[SerializeField] private bool enableLogging = true;

		private string currentBaseUrl;
		private string authToken;

		public void SetAuthToken(string token)
		{
			authToken = token;
			if (enableLogging)
				Debug.Log($"[MockDataFetcher] Auth token set: {(!string.IsNullOrEmpty(token) ? "***" : "null")}");
		}

		public void SetBaseUrl(string url)
		{
			currentBaseUrl = NormalizeUrl(url);

			if (enableLogging)
				Debug.Log($"[MockDataFetcher] Base URL set to: {currentBaseUrl}");
		}

		private string NormalizeUrl(string url)
		{
			return string.IsNullOrEmpty(url) ? "" : url.TrimEnd('/').ToLower();
		}


		public async Task<Result> Get(string endpoint, Action<string> onSuccess = null, Action<string> onError = null)
		{
			if (enableLogging)
				Debug.Log($"[MockDataFetcher] 🎭 GET: {endpoint}");

			if (simulateNetworkDelay)
				await Task.Delay((int)(defaultDelaySeconds * 1000));

			var mapping = FindEndpointMapping(endpoint);

			if (mapping == null)
			{
				var errorResult = Result.CreateError($"No mock response found for endpoint: {endpoint}", 404);

				if (enableLogging)
					Debug.LogError($"[MockDataFetcher] ❌ No mock found for: {endpoint}");
				onError?.Invoke(errorResult.ErrorMessage);
				return errorResult;
			}

			if (mapping.delaySeconds > 0)
				await Task.Delay((int)(mapping.delaySeconds * 1000));

			if (mapping.shouldFail)
			{
				var errorResult = Result.CreateError(mapping.errorMessage, mapping.statusCode);
				if (enableLogging)
					Debug.LogError($"[MockDataFetcher] ❌ Simulated error for {endpoint}: {mapping.errorMessage}");
				onError?.Invoke(errorResult.ErrorMessage);
				return errorResult;
			}

			var json = GetJsonFromMapping(mapping, endpoint);

			if (string.IsNullOrEmpty(json))
			{
				var errorResult = Result.CreateError($"No JSON data available for endpoint: {endpoint}", 500);
				if (enableLogging)
					Debug.LogError($"[MockDataFetcher] ❌ No JSON data for: {endpoint}");
				onError?.Invoke(errorResult.ErrorMessage);
				return errorResult;
			}

			var successResult = Result.CreateSuccess(json, mapping.statusCode);

			if (enableLogging)
				Debug.Log($"[MockDataFetcher] ✅ Mock response for {endpoint} (Status: {mapping.statusCode})");
			onSuccess?.Invoke(json);
			return successResult;
		}

		public async Task<Result> Post(string endpoint, string jsonData, Action<string> onSuccess = null, Action<string> onError = null)
		{
			if (enableLogging)
				Debug.Log($"[MockDataFetcher] 🎭 POST: {endpoint}\nData: {jsonData}");

			if (simulateNetworkDelay)
				await Task.Delay((int)(defaultDelaySeconds * 1000));

			var mapping = FindEndpointMapping(endpoint);

			if (mapping == null)
			{
				var errorResult = Result.CreateError($"No mock response found for endpoint: {endpoint}", 404);
				if (enableLogging)
					Debug.LogError($"[MockDataFetcher] ❌ No mock found for POST: {endpoint}");
				onError?.Invoke(errorResult.ErrorMessage);
				return errorResult;
			}

			if (mapping.delaySeconds > 0)
				await Task.Delay((int)(mapping.delaySeconds * 1000));

			if (mapping.shouldFail)
			{
				var errorResult = Result.CreateError(mapping.errorMessage, mapping.statusCode);
				if (enableLogging)
					Debug.LogError($"[MockDataFetcher] ❌ Simulated POST error for {endpoint}: {mapping.errorMessage}");
				onError?.Invoke(errorResult.ErrorMessage);
				return errorResult;
			}

			var json = GetJsonFromMapping(mapping, endpoint);

			if (string.IsNullOrEmpty(json))
			{
				var errorResult = Result.CreateError($"No JSON data available for endpoint: {endpoint}", 500);
				if (enableLogging)
					Debug.LogError($"[MockDataFetcher] ❌ No JSON data for POST: {endpoint}");
				onError?.Invoke(errorResult.ErrorMessage);
				return errorResult;
			}

			var successResult = Result.CreateSuccess(json, mapping.statusCode);

			if (enableLogging)
				Debug.Log($"[MockDataFetcher] ✅ Mock POST response for {endpoint} (Status: {mapping.statusCode})");

			onSuccess?.Invoke(json);

			return successResult;
		}

		private EndpointMapping FindEndpointMapping(string endpoint)
		{
			var mockResponse = mockResponses.FirstOrDefault(m =>
				NormalizeUrl(m.baseUrl) == currentBaseUrl);

			if (mockResponse == null)
			{
				if (enableLogging)
					Debug.LogWarning($"[MockDataFetcher] No MockResponse found for baseUrl: {currentBaseUrl}");
				return null;
			}

			var mapping = mockResponse.endpointMappings.FirstOrDefault(m =>
				m.endpoint == endpoint);

			if (mapping != null)
				return mapping;

			mapping = mockResponse.endpointMappings.FirstOrDefault(m =>
				endpoint.Contains(m.endpoint) || m.endpoint.Contains(endpoint));

			if (mapping != null)
				return mapping;

			if (mockResponse.defaultJsonAsset == null) return null;

			if (enableLogging)
				Debug.Log($"[MockDataFetcher] Using defaultJsonAsset for endpoint: {endpoint}");

			return new EndpointMapping
			{
				endpoint = endpoint,
				jsonAsset = mockResponse.defaultJsonAsset,
				statusCode = 200
			};
		}

		private string GetJsonFromMapping(EndpointMapping mapping, string endpoint)
		{
			if (mapping.jsonAsset != null)
			{
				return mapping.jsonAsset.text;
			}

			var mockResponse = mockResponses.FirstOrDefault(m =>
				NormalizeUrl(m.baseUrl) == currentBaseUrl);

			if (mockResponse?.defaultJsonAsset == null) return null;

			if (enableLogging)
				Debug.Log($"[MockDataFetcher] Using defaultJsonAsset for endpoint: {endpoint}");

			return mockResponse.defaultJsonAsset.text;
		}
	}

	[Serializable]
	public class EndpointMapping
	{
		[Header("Endpoint")]
		public string endpoint;
		[Header("Response")]
		public TextAsset jsonAsset;
		[Header("Error Simulation (Optional)")]
		public bool shouldFail;
		[TextArea(2, 5)] public string errorMessage;
		public int statusCode = 200;

		[Header("Network Simulation")]
		[Range(0f, 5f)] public float delaySeconds = 0.1f;
	}

	[Serializable]
	public class MockResponse
	{
		[Header("Base URL")]
		[Tooltip("Base URL for this API (e.g., https://restcountries.com)")]
		public string baseUrl;
		[Header("Endpoint Mappings")]
		[Tooltip("Specific endpoint → JSON mappings")]
		public List<EndpointMapping> endpointMappings = new();
		[Header("Default Response")]
		[Tooltip("Used when endpoint is not found or has no jsonAsset")]
		public TextAsset defaultJsonAsset;
	}
}