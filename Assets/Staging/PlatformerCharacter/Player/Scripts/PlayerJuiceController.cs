using UnityEngine;
using Random = UnityEngine.Random;

namespace PlatformerCharacter.Player
{
	public class PlayerJuiceController : MonoBehaviour
	{
		[SerializeField] private Animator animator;
		[SerializeField] private AudioSource audioSource;
		[SerializeField] private LayerMask groundMask;
		[SerializeField] private ParticleSystem jumpParticles;
		[SerializeField] private ParticleSystem launchParticles;
		[SerializeField] private ParticleSystem moveParticles;
		[SerializeField] private ParticleSystem landParticles;
		[SerializeField] private AudioClip[] footsteps;
		[SerializeField] private float maxTilt = .1f;
		[SerializeField] private float tiltSpeed = 1;
		[SerializeField, Range(1f, 3f)] private float maxIdleSpeed = 2;
		[SerializeField] private float maxParticleFallSpeed = -40;

		private IPlayerController player;
		private bool playerGrounded;
		private ParticleSystem.MinMaxGradient currentGradient;
		private Vector2 movement;

		private void Awake() => player = GetComponentInParent<IPlayerController>();

		private void Update()
		{
			if (player == null) return;

			if (player.Input.X != 0) transform.localScale = new Vector3(player.Input.X > 0 ? 1 : -1, 1, 1);

			var targetRotVector = new Vector3(0, 0, Mathf.Lerp(-maxTilt, maxTilt, Mathf.InverseLerp(-1, 1, player.Input.X)));
			animator.transform.rotation =
				Quaternion.RotateTowards(animator.transform.rotation, Quaternion.Euler(targetRotVector), tiltSpeed * Time.deltaTime);

			animator.SetFloat(IdleSpeedKey, Mathf.Lerp(1, maxIdleSpeed, Mathf.Abs(player.Input.X)));

			// Splat
			if (player.LandingThisFrame)
			{
				animator.SetTrigger(GroundedKey);
				audioSource.PlayOneShot(footsteps[Random.Range(0, footsteps.Length)]);
			}

			// Jump effects
			if (player.JumpingThisFrame)
			{
				animator.SetTrigger(JumpKey);
				animator.ResetTrigger(GroundedKey);

				if (player.Grounded)
				{
					SetColor(jumpParticles);
					SetColor(launchParticles);
					jumpParticles.Play();
				}
			}

			if (!playerGrounded && player.Grounded)
			{
				playerGrounded = true;
				moveParticles.Play();
				landParticles.transform.localScale = Vector3.one * Mathf.InverseLerp(0, maxParticleFallSpeed, movement.y);
				SetColor(landParticles);
				landParticles.Play();
			}
			else if (playerGrounded && !player.Grounded)
			{
				playerGrounded = false;
				moveParticles.Stop();
			}

			var groundHit = Physics2D.Raycast(transform.position, Vector3.down, 2, groundMask);
			if (groundHit && groundHit.transform.TryGetComponent(out SpriteRenderer r))
			{
				currentGradient = new ParticleSystem.MinMaxGradient(r.color * 0.9f, r.color * 1.2f);
				SetColor(moveParticles);
			}

			movement = player.RawMovement;
		}

		private void OnDisable()
		{
			moveParticles.Stop();
		}

		private void OnEnable()
		{
			moveParticles.Play();
		}

		private void SetColor(ParticleSystem ps)
		{
			var main = ps.main;
			main.startColor = currentGradient;
		}

		private static readonly int GroundedKey = Animator.StringToHash("Grounded");
		private static readonly int IdleSpeedKey = Animator.StringToHash("IdleSpeed");
		private static readonly int JumpKey = Animator.StringToHash("Jump");
	}
}