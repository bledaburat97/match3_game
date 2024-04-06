using System;
using System.Collections.Generic;
using DG.Tweening;
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
        private Transform[,] _dropItems;
        private IBoardView.OnClickAction _onClick;
        private Action<Vector2> _onDragStarted;
        private Action<Vector2> _onDragEnded;

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
                _onDragStarted.Invoke(worldPosition);
            }

            if (Input.GetMouseButtonUp(0))
            {
                Vector3 worldPosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
                _onDragEnded.Invoke(worldPosition);
            }
        }

        public void SetOnClick(IBoardView.OnClickAction onClick)
        {
            _onClick = onClick;
        }

        public void SetOnDragStarted(Action<Vector2> onDragStarted)
        {
            _onDragStarted = onDragStarted;
        }

        public void SetOnDragEnded(Action<Vector2> onDragEnded)
        {
            _onDragEnded = onDragEnded;
        }

        public Camera GetCamera()
        {
            return mainCamera;
        }

        public void SetCameraPosition(Vector2 cameraPosition)
        {
            mainCamera.transform.position = new Vector3(cameraPosition.x, cameraPosition.y, mainCamera.transform.position.z);
        }
        
        public void SetDropItemViews(CellModel[,] cellModelList)
        {
            int columnCount = cellModelList.GetLength(0);
            int rowCount = cellModelList.GetLength(1);
            dropItemPool.CreatePooledObjects(columnCount, rowCount);
            _dropItems = new Transform[columnCount, rowCount];
            for (int i = 0; i < columnCount; i++)
            {
                for (int j = 0; j < rowCount; j++)
                {
                    Transform tileTransform = Instantiate(tilePrefab, cellModelList[i, j].position, Quaternion.identity);
                    tileTransform.SetParent(transform);
                    tileTransform.transform.localScale = cellModelList[i, j].localScale;
                    Transform dropItemTransform = dropItemPool.GetObjectFromPool();
                    dropItemTransform.SetParent(transform);
                    dropItemTransform.position = cellModelList[i, j].position;
                    dropItemTransform.GetComponent<SpriteRenderer>().sprite =
                        _dropItemTypeToSpriteDict[cellModelList[i, j].dropItemType];
                    dropItemTransform.transform.localScale = cellModelList[i, j].localScale;
                    _dropItems[i, j] = dropItemTransform;
                }
            }
        }

        public void SwapDropItems(CellModel firstCellModel, CellModel secondCellModel)
        {
            Transform firstDropItem = _dropItems[firstCellModel.columnIndex, firstCellModel.rowIndex];
            Transform secondDropItem = _dropItems[secondCellModel.columnIndex, secondCellModel.rowIndex];
            DOTween.Sequence().Append(firstDropItem.DOMove(secondCellModel.position, 0.5f));
            DOTween.Sequence().Append(secondDropItem.DOMove(firstCellModel.position, 0.5f));

            _dropItems[secondCellModel.columnIndex, secondCellModel.rowIndex] = firstDropItem;
            _dropItems[firstCellModel.columnIndex, firstCellModel.rowIndex] = secondDropItem;
        }

        public void ExplodeDropItem(int columnIndex, int rowIndex)
        {
            dropItemPool.ReturnObjectToPool(_dropItems[columnIndex, rowIndex]);
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
        void SetDropItemViews(CellModel[,] cellModelList);
        Vector2 GetOriginalSizeOfSprites();
    }

}