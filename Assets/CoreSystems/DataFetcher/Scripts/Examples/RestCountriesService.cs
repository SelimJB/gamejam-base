using System;
using System.Threading.Tasks;
using UnityEngine;

namespace CoreSystems.DataFetcher
{
	[CreateAssetMenu(fileName = "RestCountriesService", menuName = "ScriptableObjects/Api/Services/RestCountriesService")]
	public class RestCountriesService : ScriptableObject
	{
		[Header("Configuration")]
		[SerializeField] private ApiManager apiManager;
		[SerializeField] private string baseUrl = "https://restcountries.com";

		private IApiClient GetClient()
		{
			var client = apiManager.Client;
			client.SetBaseUrl(baseUrl);
			return client;
		}

		public async Task<Country> GetCountryByCodeAsync(string countryCode = "JP")
		{
			var result = await GetClient().Get($"/v3.1/alpha/{countryCode}");

			if (!result.Success)
				throw new Exception($"API Error {result.StatusCode}: {result.ErrorMessage}");

			var data = result.Data;
			var countries = DeserializeCountryArray(data);

			if (countries.Length == 0)
				throw new Exception("No country found with the given code.");

			return countries[0];
		}

		public async Task<Country[]> GetCountriesByRegionAsync(string region = "Asia")
		{
			var result = await GetClient().Get($"/v3.1/region/{region}");

			if (!result.Success)
				throw new Exception($"API Error {result.StatusCode}: {result.ErrorMessage}");

			var data = result.Data;
			var countries = DeserializeCountryArray(data);
			return countries;
		}

		public async Task<Country> SearchCountriesByNameAsync(string name = "Japan")
		{
			var result = await GetClient().Get($"/v3.1/name/{name}");

			if (!result.Success)
				throw new Exception($"API Error {result.StatusCode}: {result.ErrorMessage}");

			var data = result.Data;
			var countries = DeserializeCountryArray(data);
			if (countries.Length == 0)
				throw new Exception("No country found with the given name.");
			return countries[0];
		}

		private Country[] DeserializeCountryArray(string json)
		{
			var wrappedJson = $"{{\"array\":{json}}}";
			var wrapper = JsonUtility.FromJson<CountryArrayWrapper>(wrappedJson);
			return wrapper?.array ?? Array.Empty<Country>();
		}

		[Serializable]
		private class CountryArrayWrapper
		{
			public Country[] array;
		}

		[Serializable]
		public class Country
		{
			public Name name;
			public string[] capital;
			public string region;
			public string subregion;
			public int population;
			public string[] borders;
			public Flags flags;
			public string cca2;
			public string cca3;
		}

		[Serializable]
		public class Name
		{
			public string common;
			public string official;
		}

		[Serializable]
		public class Flags
		{
			public string png;
			public string svg;
		}
	}
}