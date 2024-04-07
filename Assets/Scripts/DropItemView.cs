using System;
using System.Collections;
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
        private int _columnIndex;
        private int _rowIndex;
        private Action<int, int> _onMoveCompleted;
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
                    _isMoving = true;
                    _onMoveCompleted?.Invoke(_columnIndex, _rowIndex);
                }
            }
        }

        public void UpdateTargetVerticalPosition(Vector2 targetPosition)
        {
            _targetPosition = targetPosition;
            _isMoving = true;
            _startTime = Time.time;
        }

        public void SetIndex(int columnIndex, int rowIndex)
        {
            _columnIndex = columnIndex;
            _rowIndex = rowIndex;
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

        public void SetOnMoveCompletedAction(Action<int,int> onMoveCompleted)
        {
            _onMoveCompleted = onMoveCompleted;
        }
    }

    public interface IDropItemView
    {
        void UpdateTargetVerticalPosition(Vector2 targetPosition);
        void SetActive(bool status);
        void SetParent(Transform parentTransform);
        void SetPosition(Vector2 position);
        void SetDropItemSprite(Sprite sprite);
        void SetLocalScale(Vector2 localScale);
        void SetIndex(int columnIndex, int rowIndex);
        void SetOnMoveCompletedAction(Action<int, int> onMoveCompleted);
    }
}