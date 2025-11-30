using System;
using System.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace CoreSystems.DataFetcher.Demo
{
	public class DataFetcherSceneManager : MonoBehaviour
	{
		[SerializeField] private ApiManager apiManager;
		[SerializeField] private Button fetchRandomAsiaButton;
		[SerializeField] private Button fetchJapanButton;
		[SerializeField] private Button getFactButton;
		[SerializeField] private TextMeshProUGUI countryNameText;
		[SerializeField] private TextMeshProUGUI factText;
		[SerializeField] private Image flagImage;
		[SerializeField] private string randomApiUrl = "https://official-joke-api.appspot.com/jokes/random";
		[SerializeField] private GameObject loader;

		private RestCountriesService restCountriesService;
		private Tween loaderRotationTween;
		private CanvasGroup loaderCanvasGroup;

		private void Start()
		{
			InitializeLoader();
			restCountriesService = apiManager.GetService<RestCountriesService>();
			fetchJapanButton.onClick.AddListener(OnFetchJapanButtonClicked);
			fetchRandomAsiaButton.onClick.AddListener(OnFetchRandomCountryButtonClicked);
			getFactButton.onClick.AddListener(OnGetFactButtonClicked);
		}

		private void InitializeLoader()
		{
			if (loader == null) return;

			loader.SetActive(true);

			loaderCanvasGroup = loader.GetComponent<CanvasGroup>();
			if (loaderCanvasGroup == null)
			{
				loaderCanvasGroup = loader.AddComponent<CanvasGroup>();
			}

			loaderCanvasGroup.alpha = 0f;
			loaderCanvasGroup.blocksRaycasts = false;
			loaderCanvasGroup.interactable = false;

			StartLoaderRotation();
		}

		private void OnDestroy()
		{
			fetchJapanButton.onClick.RemoveAllListeners();
			fetchRandomAsiaButton.onClick.RemoveAllListeners();
			getFactButton.onClick.RemoveAllListeners();

			StopLoaderRotation();
		}

		private void ShowLoader()
		{
			if (loaderCanvasGroup != null)
			{
				loaderCanvasGroup.alpha = 1f;
				loaderCanvasGroup.blocksRaycasts = true;
				loaderCanvasGroup.interactable = true;
			}
		}

		private void HideLoader()
		{
			if (loaderCanvasGroup != null)
			{
				loaderCanvasGroup.alpha = 0f;
				loaderCanvasGroup.blocksRaycasts = false;
				loaderCanvasGroup.interactable = false;
			}
		}

		private void StartLoaderRotation()
		{
			if (loader == null) return;

			if (loaderRotationTween != null && loaderRotationTween.IsActive())
				return;

			var rectTransform = loader.GetComponent<RectTransform>();
			var targetTransform = rectTransform != null ? rectTransform : loader.transform;

			loaderRotationTween = targetTransform
				.DORotate(new Vector3(0, 0, -360f), 1f, RotateMode.FastBeyond360)
				.SetEase(Ease.Linear)
				.SetUpdate(UpdateType.Normal, true)
				.SetRecyclable(true)
				.SetLoops(-1, LoopType.Restart);
		}

		private void StopLoaderRotation()
		{
			if (loaderRotationTween != null && loaderRotationTween.IsActive())
			{
				loaderRotationTween.Kill();
				loaderRotationTween = null;
			}
		}

		private async Task LoadImageFromUrl(string imageUrl, Image targetImage)
		{
			if (string.IsNullOrEmpty(imageUrl) || targetImage == null)
			{
				Debug.LogWarning("[DataFetcherSceneManager] Invalid image URL or target image");
				return;
			}

			try
			{
				using var request = UnityWebRequestTexture.GetTexture(imageUrl);
				var operation = request.SendWebRequest();

				while (!operation.isDone)
				{
					await Task.Yield();
				}

				if (request.result == UnityWebRequest.Result.Success)
				{
					var texture = DownloadHandlerTexture.GetContent(request);

					var sprite = Sprite.Create(texture,
						new Rect(0, 0, texture.width, texture.height),
						new Vector2(0.5f, 0.5f));

					targetImage.sprite = sprite;

					Debug.Log($"[DataFetcherSceneManager] ✅ Image loaded from: {imageUrl}");
				}
				else
				{
					Debug.LogError($"[DataFetcherSceneManager] ❌ Failed to load image: {imageUrl} - {request.error}");
				}
			}
			catch (Exception ex)
			{
				Debug.LogError($"[DataFetcherSceneManager] Exception loading image: {ex.Message}");
			}
		}

		private async void OnGetFactButtonClicked()
		{
			ShowLoader();
			try
			{
				var fact = await apiManager.Get(randomApiUrl);
				factText.text = fact.Data;
			}
			catch (Exception ex)
			{
				Debug.LogError($"Error fetching fact: {ex.Message}");
			}
			finally
			{
				HideLoader();
			}
		}

		private async void OnFetchJapanButtonClicked()
		{
			ShowLoader();
			try
			{
				var country = await restCountriesService.GetCountryByCodeAsync("JP");
				countryNameText.text = country.name.common;

				if (country.flags != null && !string.IsNullOrEmpty(country.flags.png))
				{
					await LoadImageFromUrl(country.flags.png, flagImage);
				}
			}
			catch (Exception ex)
			{
				Debug.LogError($"Error fetching Japan: {ex.Message}");
			}
			finally
			{
				HideLoader();
			}
		}

		private async void OnFetchRandomCountryButtonClicked()
		{
			ShowLoader();
			try
			{
				var countries = await restCountriesService.GetCountriesByRegionAsync("Asia");
				var randomIndex = UnityEngine.Random.Range(0, countries.Length);
				var country = countries[randomIndex];
				countryNameText.text = country.name.common;

				if (country.flags != null && !string.IsNullOrEmpty(country.flags.png))
				{
					await LoadImageFromUrl(country.flags.png, flagImage);
				}
			}
			catch (Exception ex)
			{
				Debug.LogError($"Error fetching random country: {ex.Message}");
			}
			finally
			{
				HideLoader();
			}
		}
	}
}