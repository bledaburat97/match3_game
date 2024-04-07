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
                    CellModel cellModel = cellModelList[i, j];
                    Transform tileTransform = Instantiate(tilePrefab, cellModel.position, Quaternion.identity);
                    tileTransform.SetParent(transform);
                    tileTransform.transform.localScale = cellModel.localScale;
                    _dropItems[i, j] = SpawnDropItemTransform(cellModel.position, cellModel.localScale, cellModel.dropItemType);
                }
            }
        }

        public Sequence FallNewDropItemView(CellModel cellModel, float initialVerticalPosition)
        {
            Vector2 initialPosition = new Vector2(cellModel.position.x, initialVerticalPosition);
            Transform dropItemTransform = SpawnDropItemTransform(initialPosition, cellModel.localScale, cellModel.dropItemType);
            if (_dropItems[cellModel.columnIndex, cellModel.rowIndex] != null)
            {
                Debug.LogError("Target cell has drop item view.");
                return DOTween.Sequence();
            }
            _dropItems[cellModel.columnIndex, cellModel.rowIndex] = dropItemTransform;
            return DOTween.Sequence().Append(dropItemTransform.DOMove(cellModel.position,
                (initialVerticalPosition - cellModel.position.y) * 1f)).Pause();
        }

        private Transform SpawnDropItemTransform(Vector2 initialPosition, Vector2 localScale, DropItemType dropItemType)
        {
            Transform dropItemTransform = dropItemPool.GetObjectFromPool();
            dropItemTransform.SetParent(transform);
            dropItemTransform.position = initialPosition;
            dropItemTransform.GetComponent<SpriteRenderer>().sprite =
                _dropItemTypeToSpriteDict[dropItemType];
            dropItemTransform.transform.localScale = localScale;
            return dropItemTransform;
        }

        public void SwapDropItems(CellModel firstCellModel, CellModel secondCellModel, Action onComplete)
        {
            Transform firstDropItem = _dropItems[firstCellModel.columnIndex, firstCellModel.rowIndex];
            Transform secondDropItem = _dropItems[secondCellModel.columnIndex, secondCellModel.rowIndex];
            _dropItems[secondCellModel.columnIndex, secondCellModel.rowIndex] = firstDropItem;
            _dropItems[firstCellModel.columnIndex, firstCellModel.rowIndex] = secondDropItem;
            DOTween.Sequence().Append(firstDropItem.DOMove(secondCellModel.position, 0.5f))
                .Join(secondDropItem.DOMove(firstCellModel.position, 0.5f))
                .OnComplete(onComplete.Invoke);
        }

        public void FillDropItem(CellModel previousCellModel, CellModel newCellModel)
        {
            Transform dropItem = _dropItems[previousCellModel.columnIndex, previousCellModel.rowIndex];
            if (_dropItems[newCellModel.columnIndex, newCellModel.rowIndex] != null)
            {
                Debug.LogError("The cell to be filled is not empty.");
            }
            _dropItems[newCellModel.columnIndex, newCellModel.rowIndex] = dropItem;
            _dropItems[previousCellModel.columnIndex, previousCellModel.rowIndex] = null;
            DOTween.Sequence().Append(dropItem.DOMove(newCellModel.position,
                (previousCellModel.position.y - newCellModel.position.y) * 1f));
        }

        public void ExplodeDropItem(int columnIndex, int rowIndex)
        {
            dropItemPool.ReturnObjectToPool(_dropItems[columnIndex, rowIndex]);
            _dropItems[columnIndex, rowIndex] = null;
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
        void SwapDropItems(CellModel firstCellModel, CellModel secondCellModel, Action onComplete);
        void FillDropItem(CellModel previousCellModel, CellModel newCellModel);
    }

}