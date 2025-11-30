using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace CoreSystems.DataFetcher
{
	[CreateAssetMenu(fileName = "ApiClient", menuName = "ScriptableObjects/Api/ApiClient")]
	public class ApiClient : ScriptableObject, IApiClient
	{
		[Header("Configuration")]
		[SerializeField] private float timeoutSeconds = 30f;
		[SerializeField] private int maxRetries = 3;
		[SerializeField] private float retryDelaySeconds = 1f;

		[Header("Debug")]
		[SerializeField] private bool enableLogging = true;

		private string baseUrl;
		private string authToken;

		public void SetAuthToken(string token)
		{
			authToken = token;

			if (enableLogging)
				Debug.Log($"[DataFetcher] Auth token set: {(!string.IsNullOrEmpty(token) ? "***" : "null")}");
		}

		public void SetBaseUrl(string url)
		{
			baseUrl = url?.TrimEnd('/') ?? "";
		}

		public async Task<Result> Get(string endpoint, Action<string> onSuccess = null, Action<string> onError = null)
		{
			var fullUrl = CombineUrl(baseUrl, endpoint);

			if (enableLogging)
				Debug.Log($"[DataFetcher] GET: {fullUrl}");

			var result = await SendRequestWithRetry(fullUrl, UnityWebRequest.kHttpVerbGET, null);

			if (result.Success)
			{
				if (enableLogging)
					Debug.Log($"[DataFetcher] ✅ GET Success: {fullUrl} (Status: {result.StatusCode})");
				onSuccess?.Invoke(result.Data);
			}
			else
			{
				if (enableLogging)
					Debug.LogError($"[DataFetcher] ❌ GET Failed: {fullUrl} - {result.ErrorMessage}");
				onError?.Invoke(result.ErrorMessage);
			}

			return result;
		}

		public async Task<Result> Post(string endpoint, string jsonData, Action<string> onSuccess = null, Action<string> onError = null)
		{
			var fullUrl = CombineUrl(baseUrl, endpoint);

			if (enableLogging)
				Debug.Log($"[DataFetcher] POST: {fullUrl}\nData: {jsonData}");

			var result = await SendRequestWithRetry(fullUrl, UnityWebRequest.kHttpVerbPOST, jsonData);

			if (result.Success)
			{
				if (enableLogging)
					Debug.Log($"[DataFetcher] ✅ POST Success: {fullUrl} (Status: {result.StatusCode})");
				onSuccess?.Invoke(result.Data);
			}
			else
			{
				if (enableLogging)
					Debug.LogError($"[DataFetcher] ❌ POST Failed: {fullUrl} - {result.ErrorMessage}");
				onError?.Invoke(result.ErrorMessage);
			}

			return result;
		}

		private void SetupRequest(UnityWebRequest request)
		{
			request.timeout = (int)timeoutSeconds;

			if (!string.IsNullOrEmpty(authToken))
			{
				request.SetRequestHeader("Authorization", $"Bearer {authToken}");
			}

			request.SetRequestHeader("Content-Type", "application/json");
			request.SetRequestHeader("Accept", "application/json");
		}

		private async Task<Result> SendRequestWithRetry(string fullUrl, string method, string postData)
		{
			for (var attempt = 0; attempt <= maxRetries; attempt++)
			{
				UnityWebRequest request = null;
				try
				{
					if (method == UnityWebRequest.kHttpVerbPOST)
					{
						request = UnityWebRequest.Post(fullUrl, postData, "application/json");
					}
					else
					{
						request = UnityWebRequest.Get(fullUrl);
					}

					SetupRequest(request);

					var result = await SendRequest(request);

					request.Dispose();
					request = null;

					if (result.Success || !IsRetriableError(result.StatusCode))
					{
						return result;
					}

					if (attempt == maxRetries)
					{
						return result;
					}

					if (enableLogging)
						Debug.LogWarning($"[DataFetcher] Retry {attempt + 1}/{maxRetries} in {retryDelaySeconds}s...");

					await Task.Delay((int)(retryDelaySeconds * 1000));
				}
				catch (Exception ex)
				{
					if (request != null)
					{
						request.Dispose();
						request = null;
					}

					if (attempt == maxRetries)
					{
						return Result.CreateError($"Request exception after {maxRetries} retries: {ex.Message}", 0);
					}

					if (enableLogging)
						Debug.LogWarning($"[DataFetcher] Exception on attempt {attempt + 1}: {ex.Message}");

					await Task.Delay((int)(retryDelaySeconds * 1000));
				}
			}

			return Result.CreateError("Max retries exceeded", 0);
		}

		private async Task<Result> SendRequest(UnityWebRequest request)
		{
			try
			{
				var operation = request.SendWebRequest();

				while (!operation.isDone)
				{
					await Task.Yield();
				}

				if (request.result == UnityWebRequest.Result.Success)
				{
					if (request.downloadHandler == null)
					{
						return Result.CreateError("Download handler is null", (int)request.responseCode);
					}

					var responseText = request.downloadHandler.text;

					if (string.IsNullOrEmpty(responseText))
					{
						return Result.CreateError("Response is empty", (int)request.responseCode);
					}

					return Result.CreateSuccess(responseText, (int)request.responseCode);
				}

				var errorMessage = $"Request failed: {request.error}";
				if (request.downloadHandler != null && !string.IsNullOrEmpty(request.downloadHandler.text))
				{
					errorMessage += $" - Response: {request.downloadHandler.text}";
				}

				return Result.CreateError(errorMessage, (int)request.responseCode);
			}
			catch (Exception ex)
			{
				return Result.CreateError($"Request exception: {ex.Message}", 0);
			}
		}

		private bool IsRetriableError(int statusCode)
		{
			return statusCode >= 500 || // Server errors
			       statusCode == 408 || // Request Timeout
			       statusCode == 429 || // Too Many Requests
			       statusCode == 0; // Network error
		}

		private string CombineUrl(string baseUrl, string endpoint)
		{
			if (string.IsNullOrEmpty(baseUrl))
				return endpoint;

			return endpoint.StartsWith("http") ? endpoint : $"{baseUrl.TrimEnd('/')}/{endpoint.TrimStart('/')}";
		}

		[ContextMenu("Test Connection")]
		private async void TestConnection()
		{
			if (string.IsNullOrEmpty(baseUrl))
			{
				Debug.LogWarning("[DataFetcher] No base URL set for testing");
				return;
			}

			Debug.Log($"[DataFetcher] Testing connection to: {baseUrl}");

			await Get("/",
				onSuccess: (data) => Debug.Log($"[DataFetcher] ✅ Connection test successful"),
				onError: (error) => Debug.LogError($"[DataFetcher] ❌ Connection test failed: {error}"));
		}
	}
}