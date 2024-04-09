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
        private IDropItemView[,] _dropItems;
        private Action<Vector2> _onDragStarted;
        private Action<Vector2> _onDragEnded;
        private Action<int, int> _onDropItemPlaced;
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

        public void SetTile()
        {
            
        }
        
        public void SetDropItemViews(CellModel[,] cellModelList)
        {
            int columnCount = cellModelList.GetLength(0);
            int rowCount = cellModelList.GetLength(1);
            dropItemPool.CreateDropItemPool(columnCount, rowCount);
            _dropItems = new IDropItemView[columnCount, rowCount];
            for (int i = 0; i < columnCount; i++)
            {
                for (int j = 0; j < rowCount; j++)
                {
                    CellModel cellModel = cellModelList[i, j];
                    Transform tileTransform = Instantiate(tilePrefab, cellModel.position, Quaternion.identity);
                    tileTransform.SetParent(transform);
                    tileTransform.transform.localScale = cellModel.localScale;
                    _dropItems[i, j] = SpawnDropItem(cellModel.position, cellModel);
                }
            }
        }

        public void FallNewDropItem(CellModel cellModel, float initialVerticalPosition)
        {
            Vector2 initialPosition = new Vector2(cellModel.position.x, initialVerticalPosition);
            IDropItemView dropItem = SpawnDropItem(initialPosition, cellModel);
            if (_dropItems[cellModel.columnIndex, cellModel.rowIndex] != null)
            {
                Debug.LogError("Target cell has drop item view.");
            }
            _dropItems[cellModel.columnIndex, cellModel.rowIndex] = dropItem;
            dropItem.UpdateTargetVerticalPosition(cellModel.position);
            dropItem.SetIndex(cellModel.columnIndex, cellModel.rowIndex);
        }

        public Vector2 GetDropItemPosition(int columnIndex, int rowIndex)
        {
            return _dropItems[columnIndex, rowIndex].GetPosition();
        }

        private IDropItemView SpawnDropItem(Vector2 initialPosition, CellModel cellModel)
        {
            IDropItemView dropItem = dropItemPool.GetDropItemFromPool();
            dropItem.SetParent(transform);
            dropItem.SetPosition(initialPosition);
            dropItem.SetDropItemSprite(_dropItemTypeToSpriteDict[cellModel.dropItemType]);
            dropItem.SetLocalScale(cellModel.localScale);
            dropItem.SetOnMoveCompletedAction(_onDropItemPlaced);
            dropItem.SetIndex(cellModel.columnIndex, cellModel.rowIndex);
            return dropItem;
        }

        public void SwapDropItems(CellModel firstCellModel, CellModel secondCellModel, bool canSwap)
        {
            IDropItemView firstDropItem = _dropItems[firstCellModel.columnIndex, firstCellModel.rowIndex];
            IDropItemView secondDropItem = _dropItems[secondCellModel.columnIndex, secondCellModel.rowIndex];
            if (canSwap)
            {
                _dropItems[secondCellModel.columnIndex, secondCellModel.rowIndex] = firstDropItem;
                _dropItems[firstCellModel.columnIndex, firstCellModel.rowIndex] = secondDropItem;
                firstDropItem.SetIndex(secondCellModel.columnIndex, secondCellModel.rowIndex);
                firstDropItem.SwapDropItem(secondCellModel.position, true);
                secondDropItem.SetIndex(firstCellModel.columnIndex, firstCellModel.rowIndex);
                secondDropItem.SwapDropItem(firstCellModel.position, false);
            }
            else
            {
                firstDropItem.SwapAndBack(secondCellModel.position, true);
                secondDropItem.SwapAndBack(firstCellModel.position, false);
            }
        }

        public void FillDropItem(CellModel previousCellModel, CellModel newCellModel)
        {
            IDropItemView dropItem = _dropItems[previousCellModel.columnIndex, previousCellModel.rowIndex];
            if (_dropItems[newCellModel.columnIndex, newCellModel.rowIndex] != null)
            {
                Debug.LogError("The cell to be filled is not empty.");
            }
            _dropItems[newCellModel.columnIndex, newCellModel.rowIndex] = dropItem;
            _dropItems[previousCellModel.columnIndex, previousCellModel.rowIndex] = null;
            dropItem.UpdateTargetVerticalPosition(newCellModel.position);
            dropItem.SetIndex(newCellModel.columnIndex, newCellModel.rowIndex);
        }

        public void ExplodeDropItem(int columnIndex, int rowIndex)
        {
            dropItemPool.ReturnDropItemToPool(_dropItems[columnIndex, rowIndex]);
            _dropItems[columnIndex, rowIndex] = null;
        }
        
        public Vector2 GetOriginalSizeOfSprites()
        {
            return tilePrefab.GetComponent<SpriteRenderer>().sprite.bounds.size;
        }

        public void SetOnDropItemPlaced(Action<int, int> onDropItemPlaced)
        {
            _onDropItemPlaced = onDropItemPlaced;
        }
    }

    public interface IBoardView
    {
        Camera GetCamera();
        void SetCameraPosition(Vector2 cameraPosition);
        void SetDropItemViews(CellModel[,] cellModelList);
        Vector2 GetOriginalSizeOfSprites();
        void SwapDropItems(CellModel firstCellModel, CellModel secondCellModel, bool canSwap);
        void FillDropItem(CellModel previousCellModel, CellModel newCellModel);
        void FallNewDropItem(CellModel cellModel, float initialVerticalPosition);
        void SetOnDragStarted(Action<Vector2> onDragStarted);
        void SetOnDragEnded(Action<Vector2> onDragEnded);
        void ExplodeDropItem(int columnIndex, int rowIndex);
        void SetOnDropItemPlaced(Action<int, int> onDropItemPlaced);
        Vector2 GetDropItemPosition(int columnIndex, int rowIndex);
    }

}