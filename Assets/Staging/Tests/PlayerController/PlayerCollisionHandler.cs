using CoreSystems;
using CoreSystems.Achievements;
using CoreSystems.Audio;
using CoreSystems.VFX;
using UnityEngine;

namespace Staging
{
	public class PlayerCollisionHandler : MonoBehaviour
	{
		[SerializeField] private GameObject impactVFXPrefab;
		[SerializeField, PreviewAudioClip] private AudioClip impactSFX;

		private ParticleVFXManager vfxManager;
		private AudioManager audioManager;

		private void Awake()
		{
			vfxManager = ParticleVFXManager.Instance;
			audioManager = AudioManager.Instance;
		}

		private void OnCollisionEnter(Collision other)
		{
			// Ideally, should be defined in a dedicated component on the collided object,
			// which would store the appropriate particle effect & SFX for that surface type.
			if (other.gameObject.layer == LayerMask.NameToLayer("Obstacle"))
			{
				GameEvents.ReportMilestone(MilestoneType.HitWall);
				vfxManager.Spawn(impactVFXPrefab, other.contacts[0].point);
				audioManager.Play(impactSFX);
			}
		}
		
		private void OnTriggerEnter(Collider other)
		{
			if (other.gameObject.TryGetComponent<Coin>(out var coin))
			{
				GameEvents.ReportMetricIncrease(MetricType.Coin);
			}
		}
	}
}