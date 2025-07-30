using UnityEngine;

namespace Staging
{
	public class SimplePlayerController : MonoBehaviour
	{
		[SerializeField] private Rigidbody rb;
		[SerializeField] private float moveSpeed = 5f;

		private void FixedUpdate()
		{
			var moveX = Input.GetAxis("Horizontal");
			var moveZ = Input.GetAxis("Vertical");

			var movement = new Vector3(moveX, 0f, moveZ) * moveSpeed * Time.fixedDeltaTime;
			rb.AddForce(movement, ForceMode.VelocityChange);
		}
	}
}