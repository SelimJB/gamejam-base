using UnityEngine;
using UnityEngine.SceneManagement;

namespace CoreSystems
{
	public class SceneResetter : MonoBehaviour
	{
		private void Update()
		{
			if (Input.GetKeyDown(KeyCode.Escape))
			{
				SceneManager.LoadScene(0);
			}
		}
	}
}