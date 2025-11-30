using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace CoreSystems.DataFetcher
{
	/// <summary>
	/// Utility class for loading images from URLs
	/// </summary>
	public static class ImageLoader
	{
		public static async Task<bool> LoadImageAsync(string imageUrl, Image targetImage, Vector2 pivot = default)
		{
			if (string.IsNullOrEmpty(imageUrl) || targetImage == null)
			{
				Debug.LogWarning("[ImageLoader] Invalid image URL or target image");
				return false;
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

					if (pivot == default)
						pivot = new Vector2(0.5f, 0.5f);

					var sprite = Sprite.Create(texture,
						new Rect(0, 0, texture.width, texture.height),
						pivot);

					targetImage.sprite = sprite;

					Debug.Log($"[ImageLoader] ✅ Image loaded from: {imageUrl}");
					return true;
				}

				Debug.LogError($"[ImageLoader] ❌ Failed to load image: {imageUrl} - {request.error}");
				return false;
			}
			catch (Exception ex)
			{
				Debug.LogError($"[ImageLoader] Exception loading image: {ex.Message}");
				return false;
			}
		}

		/// <summary>
		/// Load image from URL and return the Texture2D
		/// </summary>
		/// <param name="imageUrl">URL of the image to load</param>
		/// <returns>Texture2D if successful, null otherwise</returns>
		public static async Task<Texture2D> LoadTextureAsync(string imageUrl)
		{
			if (string.IsNullOrEmpty(imageUrl))
			{
				Debug.LogWarning("[ImageLoader] Invalid image URL");
				return null;
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
					Debug.Log($"[ImageLoader] ✅ Texture loaded from: {imageUrl}");
					return texture;
				}

				Debug.LogError($"[ImageLoader] ❌ Failed to load texture: {imageUrl} - {request.error}");
				return null;
			}
			catch (Exception ex)
			{
				Debug.LogError($"[ImageLoader] Exception loading texture: {ex.Message}");
				return null;
			}
		}

		/// <summary>
		/// Load image from URL and return a Sprite
		/// </summary>
		/// <param name="imageUrl">URL of the image to load</param>
		/// <param name="pivot">Pivot point for the sprite (default: center)</param>
		/// <returns>Sprite if successful, null otherwise</returns>
		public static async Task<Sprite> LoadSpriteAsync(string imageUrl, Vector2 pivot = default)
		{
			var texture = await LoadTextureAsync(imageUrl);
			if (texture == null) return null;

			if (pivot == default)
				pivot = new Vector2(0.5f, 0.5f);

			var sprite = Sprite.Create(texture,
				new Rect(0, 0, texture.width, texture.height),
				pivot);

			return sprite;
		}
	}
}