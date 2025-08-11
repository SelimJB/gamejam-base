using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.EventSystems;

namespace CoreSystems.Achievements.UI
{
	public class AchievementNotification : MonoBehaviour, IPointerClickHandler
	{
		[SerializeField] private Image icon;
		[SerializeField] private Image iconBackground;
		[SerializeField] private TextMeshProUGUI title;
		[SerializeField] private TextMeshProUGUI description;

		[Header("Animation Settings")]
		[SerializeField] private float animationDuration = 0.5f;
		[SerializeField] private float disappearDelay = 3f;
		[SerializeField] private Ease appearEase = Ease.OutBack;
		[SerializeField] private Ease disappearEase = Ease.InBack;

		private RectTransform rectTransform;
		private Vector3 originalPosition;
		private Vector3 originalScale;

		public event Action onPointerClick;

		private enum AnimationState
		{
			Hidden,
			Appearing,
			Visible,
			Disappearing
		}

		private AnimationState currentState = AnimationState.Hidden;
		private Sequence currentSequence;

		private void Awake()
		{
			rectTransform = GetComponent<RectTransform>();
			originalPosition = rectTransform.anchoredPosition;
			originalScale = rectTransform.localScale;
		}

		public void Initialize(Achievement achievement)
		{
			icon.sprite = achievement.Icon;
			title.text = achievement.Title;
			description.text = achievement.Description;
			icon.color = achievement.Color;
			iconBackground.color = achievement.BackgroundColor;
		}

		public void ShowNotification()
		{
			StopCurrentSequence();

			rectTransform.localScale = Vector3.zero;
			rectTransform.anchoredPosition = originalPosition + Vector3.down * 100f;

			gameObject.SetActive(true);
			currentState = AnimationState.Appearing;
			currentSequence = DOTween.Sequence();
			currentSequence.Append(rectTransform.DOScale(originalScale, animationDuration).SetEase(appearEase));
			currentSequence.Join(rectTransform.DOAnchorPos(originalPosition, animationDuration).SetEase(appearEase));

			currentSequence.OnComplete(() =>
			{
				currentState = AnimationState.Visible;
				ScheduleAutoDisappear();
			});
		}

		private void ScheduleAutoDisappear()
		{
			if (currentState != AnimationState.Visible) return;

			currentSequence = DOTween.Sequence();
			currentSequence.AppendInterval(disappearDelay);
			currentSequence.AppendCallback(() =>
			{
				if (currentState == AnimationState.Visible)
				{
					HideNotification();
				}
			});
		}

		public void HideNotification()
		{
			if (currentState != AnimationState.Visible) return;

			StopCurrentSequence();

			currentState = AnimationState.Disappearing;

			currentSequence = DOTween.Sequence();
			currentSequence.Append(rectTransform.DOScale(Vector3.zero, animationDuration).SetEase(disappearEase));
			currentSequence.Join(rectTransform.DOAnchorPos(originalPosition + Vector3.down * 100f, animationDuration).SetEase(disappearEase));
			currentSequence.OnComplete(() =>
			{
				currentState = AnimationState.Hidden;
				gameObject.SetActive(false);
			});
		}

		private void StopCurrentSequence()
		{
			if (currentSequence != null && currentSequence.IsActive())
			{
				currentSequence.Kill();
				currentSequence = null;
			}
		}

		public void OnPointerClick(PointerEventData eventData)
		{
			if (currentState == AnimationState.Visible)
			{
				HideNotification();
				onPointerClick?.Invoke();
			}
		}

		private void OnDestroy()
		{
			StopCurrentSequence();
			rectTransform?.DOKill();
			onPointerClick = null;
		}
	}
}