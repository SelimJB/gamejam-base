using UnityEngine;

namespace Utils
{
	public sealed class Singleton<T> : MonoBehaviour where T : MonoBehaviour
	{
		public static T Instance
		{
			get
			{
				if (instance == null)
				{
					instance = FindObjectOfType<T>();
				}

				return instance;
			}
		}

		private static T instance;

		private void Awake()
		{
			if (instance == null)
				instance = gameObject.GetComponent<T>();

			else if (instance.GetInstanceID() != GetInstanceID())
				Destroy(gameObject);
		}
	}
}