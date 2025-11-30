using System;
using UnityEngine;
using UnityEngine.UI;

namespace CoreSystems.DataFetcher
{
	public class RestCountriesTest : MonoBehaviour
	{
		[SerializeField] private RestCountriesService restCountriesExample;
		[SerializeField] private Button fetchButton;

		private void Start()
		{
			fetchButton.onClick.AddListener(OnFetchButtonClicked);
		}

		private void OnDestroy()
		{
			fetchButton.onClick.RemoveAllListeners();
		}

		private async void OnFetchButtonClicked()
		{
			try
			{
				var country = await restCountriesExample.GetCountryByCodeAsync("JP");
				var country2 = await restCountriesExample.GetCountryByCodeAsync("FR");
				Debug.Log($"Country: {country.name.common}, Capital: {country.capital[0]}, Region: {country.region}");
				Debug.Log($"Country: {country2.name.common}, Capital: {country2.capital[0]}, Region: {country2.region}");
			}
			catch (Exception ex)
			{
				Debug.LogError($"Error fetching countries: {ex.Message}");
			}
		}
	}
}