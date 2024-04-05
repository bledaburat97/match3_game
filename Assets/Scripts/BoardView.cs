using System;
using System.Collections.Generic;
using UnityEngine;

namespace Board
{
    public class BoardView : MonoBehaviour, IBoardView
    {
        [SerializeField] private DropItemPool dropItemPool;
        [SerializeField] private Camera mainCamera;
        [SerializeField] private List<DropItemSO> dropItemSOList;
        [SerializeField] private Transform tilePrefab;
        private Dictionary<DropItemType, Sprite> _dropItemTypeToSpriteDict;
        private Dictionary<DropItemModel, IDropItemView> _dropItemDictionary;
        private IBoardView.OnClickAction _onClick;

        private void Start()
        {
            _dropItemTypeToSpriteDict = new Dictionary<DropItemType, Sprite>();
            _dropItemDictionary = new Dictionary<DropItemModel, IDropItemView>();
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
                _onClick.Invoke(worldPosition, out int columnIndex, out int rowIndex);
                Debug.Log("columnIndex: " + columnIndex + "rowIndex: " + rowIndex);
            }
        }

        public void SetOnClick(IBoardView.OnClickAction onClick)
        {
            _onClick = onClick;
        }

        public Camera GetCamera()
        {
            return mainCamera;
        }

        public void SetCameraPosition(Vector2 cameraPosition)
        {
            mainCamera.transform.position = new Vector3(cameraPosition.x, cameraPosition.y, mainCamera.transform.position.z);
        }
        
        public void SetDropItemViews(DropItemModel[,] dropItemModelList)
        {
            int columnCount = dropItemModelList.GetLength(0);
            int rowCount = dropItemModelList.GetLength(1);
            dropItemPool.CreatePooledObjects(columnCount, rowCount);

            for (int i = 0; i < columnCount; i++)
            {
                for (int j = 0; j < rowCount; j++)
                {
                    Transform tileTransform = Instantiate(tilePrefab, dropItemModelList[i, j].position, Quaternion.identity);
                    tileTransform.SetParent(transform);
                    tileTransform.transform.localScale = dropItemModelList[i, j].localScale;
                    Transform dropItemTransform = dropItemPool.GetObjectFromPool();
                    dropItemTransform.SetParent(transform);
                    dropItemTransform.position = dropItemModelList[i, j].position;
                    dropItemTransform.GetComponent<SpriteRenderer>().sprite =
                        _dropItemTypeToSpriteDict[dropItemModelList[i, j].dropItemType];
                    dropItemTransform.transform.localScale = dropItemModelList[i, j].localScale;
                    IDropItemView dropItemView = new DropItemView(dropItemTransform, dropItemModelList[i, j]);
                    _dropItemDictionary.Add(dropItemModelList[i,j], dropItemView);
                }
            }
        }
        
        public Vector2 GetOriginalSizeOfSprites()
        {
            return tilePrefab.GetComponent<SpriteRenderer>().sprite.bounds.size;
        }
    }

    public interface IBoardView
    {
        delegate void OnClickAction(in Vector2 position, out int columnIndex, out int rowIndex);

        void SetOnClick(OnClickAction onClick);
        Camera GetCamera();
        void SetCameraPosition(Vector2 cameraPosition);
        void SetDropItemViews(DropItemModel[,] dropItemModelList);
        Vector2 GetOriginalSizeOfSprites();
    }

}