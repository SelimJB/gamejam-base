using CoreSystems.Audio;
using UnityEngine;

namespace Staging
{
	public class Coin : MonoBehaviour
	{
		[SerializeField] private float rotationSpeed = 50f;
		[SerializeField] private ParticleSystem collectEffect;
		[SerializeField] private MeshRenderer meshRenderer;
		[SerializeField] private Collider coinCollider;
		[SerializeField] private AudioClip collectSound;

		private AudioManager audioManager;

		private void Awake()
		{
			audioManager = AudioManager.Instance;
		}

		private void Update()
		{
			transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
		}

		private void OnTriggerEnter(Collider other)
		{
			collectEffect.Play();
			meshRenderer.enabled = false;
			coinCollider.enabled = false;
			audioManager.Play(collectSound);

			Destroy(gameObject, 0.5f);
		}
	}
}