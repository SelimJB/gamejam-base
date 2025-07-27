using UnityEngine;

namespace Staging
{
	public class SimplePlayerController : MonoBehaviour
	{
		[SerializeField] private float moveSpeed = 5f;

		void Update()
		{
			float moveX = Input.GetAxis("Horizontal"); // A/D ou flèches gauche/droite
			float moveZ = Input.GetAxis("Vertical"); // W/S ou flèches haut/bas

			Vector3 movement = new Vector3(moveX, 0f, moveZ);
			transform.Translate(movement * moveSpeed * Time.deltaTime, Space.World);
		}
	}
}