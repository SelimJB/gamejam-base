using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace GameJam.UI
{
	[RequireComponent(typeof(Image))]
	public class UIImageGrayscale : MonoBehaviour
	{
		[SerializeField] private Material defaultGrayscaleMaterial;

		[Header("Grayscale Settings")]
		[Range(0f, 1f), SerializeField] private float grayscaleAmount;

		[Header("Animation")]
		[SerializeField] private float transitionDuration = 0.3f;
		[SerializeField] private Ease transitionEase = Ease.OutQuad;

		private Image targetImage;
		private Material grayscaleMaterial;
		private Material originalMaterial;

		public float GrayscaleAmount { get => grayscaleAmount; set => SetGrayscaleAmount(value); }
		private static readonly int GrayscaleAmountProperty = Shader.PropertyToID("_GrayscaleAmount");
		private float lastGrayscaleValue = -1f;

		private void Awake()
		{
			SetupGrayscaleEffect();
		}

		private void Start()
		{
			SetGrayscaleAmount(grayscaleAmount);
		}

		// Only if the material is not set up yet
		private void SetupGrayscaleEffect()
		{
			targetImage = GetComponent<Image>();
			originalMaterial = targetImage.material;

			if (defaultGrayscaleMaterial == null)
			{
				var shader = Shader.Find("UI/ImageGrayscale");
				if (shader != null)
				{
					defaultGrayscaleMaterial = new Material(shader);
				}
				else
				{
					Debug.LogError("UIImageGrayscale: Could not find 'UI/ImageGrayscale' shader!");
					return;
				}
			}

			grayscaleMaterial = new Material(defaultGrayscaleMaterial);
			targetImage.material = grayscaleMaterial;
		}

		private void SetGrayscaleAmount(float amount)
		{
			var newAmount = Mathf.Clamp01(amount);

			if (Mathf.Abs(grayscaleAmount - newAmount) > 0.001f)
			{
				grayscaleAmount = newAmount;

				if (grayscaleMaterial == null) return;

				grayscaleMaterial.SetFloat(GrayscaleAmountProperty, grayscaleAmount);
				AutoRefreshMaterial();
			}
		}

		private void ForceSliderRefresh()
		{
			if (targetImage != null && grayscaleMaterial != null)
			{
				var tempMaterial = new Material(grayscaleMaterial.shader);
				tempMaterial.SetFloat(GrayscaleAmountProperty, grayscaleAmount);

				tempMaterial.mainTexture = grayscaleMaterial.mainTexture;
				tempMaterial.color = grayscaleMaterial.color;
				tempMaterial.name = $"GrayscaleSlider_{grayscaleAmount:F3}";

				var oldMaterial = grayscaleMaterial;
				grayscaleMaterial = tempMaterial;
				targetImage.material = grayscaleMaterial;

				targetImage.SetMaterialDirty();
				targetImage.SetVerticesDirty();

				if (Application.isPlaying)
					Destroy(oldMaterial);
			}
			else if (grayscaleMaterial == null)
			{
				SetupGrayscaleEffect();
			}
		}

		private void AutoRefreshMaterial()
		{
			if (targetImage != null && grayscaleMaterial != null)
			{
				var tempMaterial = new Material(grayscaleMaterial.shader);
				tempMaterial.SetFloat(GrayscaleAmountProperty, grayscaleAmount);

				tempMaterial.mainTexture = grayscaleMaterial.mainTexture;
				tempMaterial.color = grayscaleMaterial.color;

				var oldMaterial = grayscaleMaterial;
				grayscaleMaterial = tempMaterial;
				targetImage.material = grayscaleMaterial;

				if (Application.isPlaying)
					Destroy(oldMaterial);
				else
					DestroyImmediate(oldMaterial);
			}
		}

		public Tween ToGrayscaleAmount(float targetAmount, float duration = -1f)
		{
			var actualDuration = duration < 0 ? transitionDuration : duration;
			return DOTween.To(() => grayscaleAmount, SetGrayscaleAmount, targetAmount, actualDuration)
				.SetEase(transitionEase);
		}

		public void SetGrayscaleEnabled(bool enabled)
		{
			if (targetImage != null)
			{
				targetImage.material = enabled ? grayscaleMaterial : originalMaterial;
			}
		}

		public bool IsGrayscaleEnabled()
		{
			return targetImage != null && targetImage.material == grayscaleMaterial;
		}

		private void OnValidate()
		{
			if (Application.isPlaying)
			{
				ForceSliderRefresh();
			}
		}

		private void OnDestroy()
		{
			if (grayscaleMaterial == null) return;

			if (Application.isPlaying)
				Destroy(grayscaleMaterial);
		}

		[ContextMenu("Force Refresh UI"), EditorButton]
		public void ForceRefreshUI()
		{
			if (grayscaleMaterial == null) return;

			var oldValue = grayscaleAmount;
			var tempMaterial = new Material(grayscaleMaterial);
			tempMaterial.SetFloat(GrayscaleAmountProperty, oldValue);

			if (targetImage != null)
			{
				targetImage.material = tempMaterial;
				targetImage.SetMaterialDirty();
				targetImage.SetVerticesDirty();
			}

			if (Application.isPlaying)
				Destroy(grayscaleMaterial);
			else
				DestroyImmediate(grayscaleMaterial);

			grayscaleMaterial = tempMaterial;

			Debug.Log($"UI forcé à se rafraîchir avec grayscale: {oldValue}");
		}
	}
}