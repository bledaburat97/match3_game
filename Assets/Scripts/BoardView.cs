using System;
using System.Collections.Generic;
using UnityEngine;

namespace Board
{
    public class BoardView : MonoBehaviour, IBoardView
    {
        [SerializeField] private Camera mainCamera;
        [SerializeField] private List<DropItemSO> dropItemSOList;
        [SerializeField] private Transform tilePrefab;
        [SerializeField] private DropItemView dropItemPrefab;
        
        private Queue<IDropItemView> _pooledDropItems = new Queue<IDropItemView>();
        private Dictionary<DropItemType, Sprite> _dropItemTypeToSpriteDict;
        public event EventHandler<Vector2> OnDragStartedEvent;
        public event EventHandler<Vector2> OnDragEndedEvent;
        
        private void Start()
        {
            _dropItemTypeToSpriteDict = new Dictionary<DropItemType, Sprite>();
            foreach (DropItemSO dropItemSO in dropItemSOList)
            {
                _dropItemTypeToSpriteDict.Add(dropItemSO.dropItemType, dropItemSO.sprite);
            }
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                Vector3 worldPosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
                Debug.Log("x: " + worldPosition.x + " y: " + worldPosition.y);
                OnDragStartedEvent?.Invoke(this, worldPosition);
            }

            if (Input.GetMouseButtonUp(0))
            {
                Vector3 worldPosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
                OnDragEndedEvent?.Invoke(this, worldPosition);
            }
        }

        public void CreateDropItemPool(int count)
        {
            for (int i = 0; i < count; i++)
            {
                IDropItemView dropItem = Instantiate(dropItemPrefab, transform);
                dropItem.SetActive(false);
                _pooledDropItems.Enqueue(dropItem);
            }
        }
        
        public IDropItemView GetDropItemFromPool()
        {
            if (_pooledDropItems.Count > 0)
            {
                IDropItemView dropItem = _pooledDropItems.Dequeue();
                dropItem.SetActive(true);
                return dropItem;
            }
            else
            {
                Debug.LogWarning("No objects left in pool.");
                return null;
            }
        }

        public void ReturnDropItemToPool(IDropItemView dropItem)
        {
            dropItem.SetActive(false);
            _pooledDropItems.Enqueue(dropItem);
        }

        public void CreateTile(Vector2 position, Vector2 localScale)
        {
            Transform tileTransform = Instantiate(tilePrefab, position, Quaternion.identity);
            tileTransform.SetParent(transform);
            tileTransform.transform.localScale = localScale;
        }

        public Sprite GetDropItemSprite(DropItemType dropItemType)
        {
            return _dropItemTypeToSpriteDict[dropItemType];
        }
        
        public Camera GetCamera()
        {
            return mainCamera;
        }

        public void SetCameraPosition(Vector2 cameraPosition)
        {
            mainCamera.transform.position = new Vector3(cameraPosition.x, cameraPosition.y, mainCamera.transform.position.z);
        }
        
        public Vector2 GetOriginalSizeOfSprites()
        {
            return tilePrefab.GetComponent<SpriteRenderer>().sprite.bounds.size;
        }
    }

    public interface IBoardView
    {
        event EventHandler<Vector2> OnDragStartedEvent;
        event EventHandler<Vector2> OnDragEndedEvent;
        Camera GetCamera();
        void SetCameraPosition(Vector2 cameraPosition);
        Vector2 GetOriginalSizeOfSprites();
        void CreateDropItemPool(int count);
        IDropItemView GetDropItemFromPool();
        void CreateTile(Vector2 position, Vector2 localScale);
        Sprite GetDropItemSprite(DropItemType dropItemType);
        void ReturnDropItemToPool(IDropItemView dropItem);
    }

}