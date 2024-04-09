using System.Collections.Generic;
using UnityEngine;

namespace Board
{
    public class BoardController : IBoardController
    {
        private IDropItemDeterminer _dropItemDeterminer;
        private int _columnCount;
        private int _rowCount;
        private float _cellSize;
        private CellModel[,] _cellModelList;
        private IBoardView _view;
        private Vector2 _originPosition;
        private int _dragStartedColumnIndex;
        private int _dragStartedRowIndex;
        private Vector2 _dragStartedPosition;
        private IMatchManager _matchManager;
        private FillingDropItemDeterminer _fillingDropItemDeterminer;
        private IActiveCellModelsManager _activeCellModelsManager;
        public BoardController(IBoardView view)
        {
            _view = view;
            _originPosition = Vector2.zero;
            _dragStartedColumnIndex = -1;
            _dragStartedRowIndex = -1;
            _activeCellModelsManager = new ActiveCellModelsManager();
        }
        
        public void InitializeBoard(int columnCount, int rowCount)
        {
            _dropItemDeterminer = new DropItemDeterminer();
            _columnCount = columnCount;
            _rowCount = rowCount;
            _cellModelList = new CellModel[_columnCount, _rowCount];
            Vector2 boardSize = new BoardSizeCalculator().GetBoardSize(_columnCount, _rowCount, _view.GetCamera());
            _cellSize = boardSize.y / _rowCount;
            SetCellModelList();
            SetCamera();
            SetActions();
            _view.SetDropItemViews(_cellModelList);
            _matchManager = new MatchManager(_columnCount, _rowCount, GetCellModel);
            _fillingDropItemDeterminer = new FillingDropItemDeterminer(_columnCount, _rowCount, GetCellModel);
        }

        private void SetCamera()
        {
            Vector2 cameraPosition = new Vector2((_columnCount - 1) * _cellSize / 2, (_rowCount - 1) * _cellSize / 2);
            _view.SetCameraPosition(cameraPosition);
        }

        private void SetActions()
        {
            _view.SetOnDragStarted(OnDragStarted);
            _view.SetOnDragEnded(OnDragEnded);
            _view.SetOnDropItemPlaced(OnMoveCompleted);
        }

        private void SetCellModelList()
        {
            DropItemType[,] initialDropItemTypeList = _dropItemDeterminer.GetInitialDropItemTypes(_columnCount, _rowCount);
            Vector2 sizeOfSprite = _view.GetOriginalSizeOfSprites();
            Vector2 localScaleOfDropItem = new Vector2(1 / sizeOfSprite.x, 1 / sizeOfSprite.y) * _cellSize;
            for (int i = 0; i < _columnCount; i++)
            {
                for (int j = 0; j < _rowCount; j++)
                {
                    CellModel cellModel = new CellModel()
                    {
                        dropItemType = initialDropItemTypeList[i, j],
                        columnIndex = i,
                        rowIndex = j,
                        localScale = localScaleOfDropItem,
                        position = new Vector2(i, j) * _cellSize + _originPosition,
                        hasAssignedDropItem = true,
                        hasPlacedDropItem = true,
                    };
                    _cellModelList[i, j] = cellModel;
                }
            }
        }

        private void OnDragStarted(Vector2 worldPosition)
        {
            _dragStartedColumnIndex = Mathf.FloorToInt(((worldPosition - _originPosition).x + _cellSize / 2) / _cellSize);
            _dragStartedRowIndex = Mathf.FloorToInt(((worldPosition - _originPosition).y + _cellSize / 2) / _cellSize);
            _dragStartedPosition = worldPosition;
        }
        
        private void OnDragEnded(Vector2 worldPosition)
        {
            if (!IsValidPosition(_dragStartedColumnIndex, _dragStartedRowIndex)) return;
            
            int columnIndex = Mathf.FloorToInt(((worldPosition - _originPosition).x + _cellSize / 2) / _cellSize);
            int rowIndex = Mathf.FloorToInt(((worldPosition - _originPosition).y + _cellSize / 2) / _cellSize);

            if (columnIndex == _dragStartedColumnIndex && rowIndex == _dragStartedRowIndex) return;
            
            float swipeAngle = Mathf.Atan2(worldPosition.y - _dragStartedPosition.y, worldPosition.x - _dragStartedPosition.x) *
                    180 / Mathf.PI;
            if (swipeAngle < 0) swipeAngle += 360;
            
            if (swipeAngle >= 315 || swipeAngle < 45)
            {
                columnIndex = _dragStartedColumnIndex + 1;
                rowIndex = _dragStartedRowIndex;
            }
            else if (swipeAngle >= 45 && swipeAngle < 135)
            {
                columnIndex = _dragStartedColumnIndex;
                rowIndex = _dragStartedRowIndex + 1;
            }
            else if (swipeAngle >= 135 && swipeAngle < 225)
            {
                columnIndex = _dragStartedColumnIndex - 1;
                rowIndex = _dragStartedRowIndex;
            }
            else 
            {
                columnIndex = _dragStartedColumnIndex;
                rowIndex = _dragStartedRowIndex - 1;
            }

            if (!IsValidPosition(columnIndex, rowIndex)) return;

            CellModel firstCellModel = GetCellModel(_dragStartedColumnIndex, _dragStartedRowIndex);
            CellModel secondCellModel = GetCellModel(columnIndex, rowIndex);
            
            firstCellModel.hasPlacedDropItem = false;
            secondCellModel.hasPlacedDropItem = false;
            
            if (CanSwap(firstCellModel, secondCellModel))
            {
                SwapDropItems(firstCellModel, secondCellModel);
                _activeCellModelsManager.AddSimultaneousCellModelsToList(new List<CellModel>() { firstCellModel, secondCellModel });
                _view.SwapDropItems(firstCellModel, secondCellModel, true);
            }

            else
            {
                _view.SwapDropItems(firstCellModel, secondCellModel, false);
            }
        }
        

        private bool CanSwap(CellModel firstCellModel, CellModel secondCellModel)
        {
            SwapDropItems(firstCellModel, secondCellModel);
            bool isMatched = _matchManager.GetMatchedCellModels(new List<CellModel> { firstCellModel, secondCellModel }).Count > 0;;
            SwapDropItems(firstCellModel, secondCellModel);
            return isMatched;
        }

        private void SwapDropItems(CellModel firstCellModel, CellModel secondCellModel)
        {
            DropItemType firstDropItemType = firstCellModel.dropItemType;
            DropItemType secondDropItemType = secondCellModel.dropItemType;
            firstCellModel.dropItemType = secondDropItemType;
            secondCellModel.dropItemType = firstDropItemType;
        }
        
        private void OnMoveCompleted(int columnIndex, int rowIndex)
        {
            CellModel cellModel = GetCellModel(columnIndex, rowIndex);
            cellModel.hasPlacedDropItem = true;
            if (_activeCellModelsManager.CheckAllActiveCellModelsCompleted(cellModel, out int listIndex))
            {
                List<CellModel> matchedCellModelList = _matchManager.GetMatchedCellModels(_activeCellModelsManager.GetSimultaneousCellModels(listIndex));
                List<CellModel> simultaneouslyMovingCellModels = new List<CellModel>();
                foreach (CellModel matchedCellModel in matchedCellModelList)
                {
                    matchedCellModel.hasAssignedDropItem = false;
                    matchedCellModel.hasPlacedDropItem = false;
                }

                Dictionary<CellModel, int> targetRowIndexOfFillingDropItems =
                    _fillingDropItemDeterminer.GetTargetRowIndexOfFillingDropItems(
                        out int[] emptyCellCountInEachColumn);
                

                foreach (CellModel explodingCellModel in matchedCellModelList)
                {
                    _view.ExplodeDropItem(explodingCellModel.columnIndex, explodingCellModel.rowIndex);
                }

                foreach (KeyValuePair<CellModel, int> pair in targetRowIndexOfFillingDropItems)
                {
                    CellModel cellModelToBeFilled = Fill(pair.Key, pair.Value);
                    _view.FillDropItem(pair.Key, cellModelToBeFilled);
                    simultaneouslyMovingCellModels.Add(cellModelToBeFilled);
                }

                int forbiddenColumnIndex = 5;
                for (int i = 0; i < _columnCount; i++)
                {
                    if(i == forbiddenColumnIndex) continue;
                    for (int j = 0; j < emptyCellCountInEachColumn[i]; j++)
                    {
                        int columnIndexOfSpawned = i;
                        int rowIndexOfSpawned = _rowCount - emptyCellCountInEachColumn[i] + j;
                        float initialVerticalPosition = _rowCount * _cellSize;
                        if (rowIndexOfSpawned > 0)
                        {
                            float belowDropItemVerticalPosition = _view.GetDropItemPosition(columnIndexOfSpawned, rowIndexOfSpawned - 1).y;
                            if (belowDropItemVerticalPosition > (_rowCount - 1) * _cellSize)
                            {
                                initialVerticalPosition = belowDropItemVerticalPosition + _cellSize;
                            }
                        }
                        CellModel cellModelToBeFell = SpawnNewDropItem(i, rowIndexOfSpawned,
                            initialVerticalPosition);
                        simultaneouslyMovingCellModels.Add(cellModelToBeFell);
                    }
                }

                _activeCellModelsManager.AddSimultaneousCellModelsToList(simultaneouslyMovingCellModels);
                _activeCellModelsManager.RemoveSimultaneousCellModelsAtIndex(listIndex);
            }
        }

        private CellModel SpawnNewDropItem(int columnIndex, int rowIndex, float initialVerticalPosition)
        {
            CellModel cellModel = GetCellModel(columnIndex, rowIndex);
            Fall(cellModel, _dropItemDeterminer.GenerateRandomDropItemType());
            _view.FallNewDropItem(cellModel, initialVerticalPosition);
            return cellModel;
        }

        private CellModel Fill(CellModel cellModel, int targetRowIndex)
        {
            CellModel newCellModel = GetCellModel(cellModel.columnIndex, targetRowIndex);
            if (!cellModel.hasAssignedDropItem)
            {
                Debug.LogError("There is not filling object in the cell");
            }
            
            if (newCellModel.hasAssignedDropItem)
            {
                Debug.LogError("Target cell is not empty.");
            }
            
            DropItemType dropItemType = cellModel.dropItemType;
            newCellModel.dropItemType = dropItemType;
            cellModel.hasAssignedDropItem = false;
            cellModel.hasPlacedDropItem = false;
            newCellModel.hasAssignedDropItem = true;
            return newCellModel;
        }

        private void Fall(CellModel cellModel, DropItemType dropItemType)
        {
            if (cellModel.hasAssignedDropItem)
            {
                Debug.LogError("Target cell is not empty.");
                return;
            }

            cellModel.dropItemType = dropItemType;
            cellModel.hasAssignedDropItem = true;
        }
        
        private CellModel GetCellModel(int columnIndex, int rowIndex)
        {
            return _cellModelList[columnIndex, rowIndex];
        }
        
        private bool IsValidPosition(int columnIndex, int rowIndex) {
            if (columnIndex < 0 || rowIndex < 0 || columnIndex >= _columnCount || rowIndex >= _rowCount) {
                return false;
            }

            return GetCellModel(columnIndex, rowIndex).hasPlacedDropItem;
        }
    }
    
    public interface IBoardController
    {
        void InitializeBoard(int columnCount, int rowCount);
    }
}