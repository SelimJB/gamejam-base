using System;
using System.Threading.Tasks;

namespace CoreSystems.DataFetcher
{
	public interface IApiClient
	{
		Task<Result> Get(string endpoint, Action<string> onSuccess = null, Action<string> onError = null);
		Task<Result> Post(string endpoint, string jsonData, Action<string> onSuccess = null, Action<string> onError = null);

		void SetAuthToken(string token);

		void SetBaseUrl(string baseUrl);
	}


	[Serializable]
	public class Result
	{
		public bool Success { get; set; }
		public string Data { get; set; }
		public string ErrorMessage { get; set; }
		public int StatusCode { get; set; }

		public static Result CreateSuccess(string data, int statusCode = 200)
		{
			return new Result
			{
				Success = true,
				Data = data,
				StatusCode = statusCode
			};
		}

		public static Result CreateError(string errorMessage, int statusCode = 0)
		{
			return new Result
			{
				Success = false,
				ErrorMessage = errorMessage,
				StatusCode = statusCode
			};
		}
	}
}