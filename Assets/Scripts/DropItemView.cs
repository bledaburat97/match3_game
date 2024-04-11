using System;
using DG.Tweening;
using UnityEngine;

namespace Board
{
    public class DropItemView : MonoBehaviour, IDropItemView
    {
        [SerializeField] private SpriteRenderer spriteRenderer;
        private Vector2 _targetPosition;
        private bool _isMoving = false;
        private bool _hasActiveAnimation = false;
        private const float _gravity = 100f;
        private Action _onMoveCompleted;
        private float _startTime;
        private const float _explosionDuration = 0.1f;
        private const float _swappingDuration = 0.3f;
        private const float _maxScaleRatioDuringSwapping = 1.5f;
        private const float _flickDuration = 0.3f;
        private const float _flickMovementRatio = 0.3f;
        
        void Start()
        {
            _targetPosition = transform.position;
        }

        void Update()
        {
            if (!_hasActiveAnimation && _isMoving)
            {
                Vector2 moveDirection = _targetPosition - (Vector2)transform.position;
                float totalTimePassed = Time.time - _startTime;
                float acceleration = _gravity * totalTimePassed;
                
                if (Mathf.Abs(moveDirection.magnitude) > .01f)
                {
                    transform.position += new Vector3(moveDirection.x, moveDirection.y, 0) * (acceleration * Time.deltaTime);
                }
                else
                {
                    transform.position = _targetPosition;
                    _isMoving = false;
                    _onMoveCompleted?.Invoke();
                }
            }
        }

        public void UpdateTargetVerticalPosition(Vector2 targetPosition)
        {
            _targetPosition = targetPosition;
            _isMoving = true;
            _startTime = Time.time;
        }

        public void AnimateSwap(Vector2 targetPosition, bool isDragged)
        {
            DOTween.Sequence().OnStart(() => SetAnimationStatus(true)).Append(Swap(targetPosition, isDragged))
                .OnComplete(CompleteAnimation);
        }

        public void AnimateSwapAndBack(Vector2 targetPosition, bool isDragged)
        {
            Vector2 position = transform.position;
            DOTween.Sequence().OnStart(() => SetAnimationStatus(true)).Append(Swap(targetPosition, isDragged)).Append(Swap(position, !isDragged))
                .OnComplete(CompleteAnimation);
        }
        
        private Sequence Swap(Vector2 targetPosition, bool isDragged)
        {
            Vector2 localScale = transform.localScale;
            Vector2 maxLocalScale = isDragged ? localScale * _maxScaleRatioDuringSwapping : localScale;
            
            return DOTween.Sequence().AppendCallback(() => spriteRenderer.sortingOrder = isDragged ? 2 : 1)
                .Append(transform.DOMove(targetPosition, _swappingDuration))
                .Join(DOTween.Sequence().Append(transform.DOScale(maxLocalScale, _swappingDuration / 2))
                    .Append(transform.DOScale(localScale, _swappingDuration / 2)))
                .OnComplete(() => spriteRenderer.sortingOrder = 1);
        }
        
        public void SetActive(bool status)
        {
            gameObject.SetActive(status);
        }
        
        public void SetPosition(Vector2 position)
        {
            transform.position = position;
            _targetPosition = position;
        }

        public void SetDropItemSprite(Sprite sprite)
        {
            spriteRenderer.sprite = sprite;
        }

        public void SetLocalScale(Vector2 localScale)
        {
            transform.localScale = localScale;
        }

        public void SetOnMoveCompletedAction(Action onMoveCompleted)
        {
            _onMoveCompleted = onMoveCompleted;
        }

        public Vector2 GetPosition()
        {
            return transform.position;
        }

        public Sequence AnimateExplosion()
        {
            return DOTween.Sequence().Append(transform.DOScale(0f, _explosionDuration));
        }

        public void AnimateFlick(Vector2 direction)
        {
            Vector2 position = transform.position;
            Vector2 targetPosition = position + (direction * transform.localScale.x * _flickMovementRatio);
            DOTween.Sequence().OnStart(() => SetAnimationStatus(true)).Append(transform.DOMove(targetPosition, _flickDuration / 2)).Append(transform.DOMove(position, _flickDuration / 2))
                .OnComplete(CompleteAnimation);
        }
        
        private void SetAnimationStatus(bool status)
        {
            _hasActiveAnimation = status;
            if (!status) _startTime = Time.time;
        }

        private void CompleteAnimation()
        {
            SetAnimationStatus(false);
            _onMoveCompleted?.Invoke();
        }
    }

    public interface IDropItemView
    {
        void UpdateTargetVerticalPosition(Vector2 targetPosition);
        void SetActive(bool status);
        void SetPosition(Vector2 position);
        void SetDropItemSprite(Sprite sprite);
        void SetLocalScale(Vector2 localScale);
        void SetOnMoveCompletedAction(Action onMoveCompleted);
        void AnimateSwap(Vector2 targetPosition, bool isDragged);
        void AnimateSwapAndBack(Vector2 targetPosition, bool isDragged);
        Vector2 GetPosition();
        Sequence AnimateExplosion();
        void AnimateFlick(Vector2 direction);
    }
}