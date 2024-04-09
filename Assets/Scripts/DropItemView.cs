using System;
using DG.Tweening;
using UnityEngine;

namespace Board
{
    public class DropItemView : MonoBehaviour, IDropItemView
    {
        [SerializeField] private Transform transform;
        [SerializeField] private SpriteRenderer spriteRenderer;
        private Vector2 _targetPosition;
        private bool _isMoving = false;
        private const float _gravity = 5f;
        private Action _onMoveCompleted;
        private float _startTime;
        
        void Start()
        {
            _targetPosition = transform.position;
        }

        void Update()
        {
            if (_isMoving)
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
            DOTween.Sequence().Append(Swap(targetPosition, isDragged))
                .OnComplete(() => _onMoveCompleted?.Invoke());
        }

        public void AnimateSwapAndBack(Vector2 targetPosition, bool isDragged)
        {
            Vector2 position = transform.position;
            DOTween.Sequence().Append(Swap(targetPosition, isDragged)).Append(Swap(position, !isDragged))
                .OnComplete(() => _onMoveCompleted?.Invoke());
        }
        
        private Sequence Swap(Vector2 targetPosition, bool isDragged)
        {
            Vector2 localScale = transform.localScale;
            Vector2 maxLocalScale = isDragged ? localScale * 2 : localScale;
            
            return DOTween.Sequence().AppendCallback(() => spriteRenderer.sortingOrder = isDragged ? 2 : 1)
                .Append(transform.DOMove(targetPosition, 0.4f))
                .Join(DOTween.Sequence().Append(transform.DOScale(maxLocalScale, 0.2f))
                    .Append(transform.DOScale(localScale, 0.2f)))
                .OnComplete(() => spriteRenderer.sortingOrder = 1);
        }
        
        public void SetActive(bool status)
        {
            gameObject.SetActive(status);
        }

        public void SetParent(Transform parentTransform)
        {
            transform.SetParent(parentTransform);
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
    }
}