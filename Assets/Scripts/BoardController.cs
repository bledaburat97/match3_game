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
        private IMatchManager _matchManager;
        private FillingDropItemDeterminer _fillingDropItemDeterminer;
        private IActiveCellModelsManager _activeCellModelsManager;
        private ISwapManager _swapManager;
        public BoardController(IBoardView view, IActiveCellModelsManager activeCellModelsManager, Vector2 originPosition, ISwapManager swapManager)
        {
            _view = view;
            _originPosition = originPosition;
            _activeCellModelsManager = activeCellModelsManager;
            _swapManager = swapManager;
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
            _matchManager = new MatchManager(_columnCount, _rowCount, GetCellModel);
            _fillingDropItemDeterminer = new FillingDropItemDeterminer(_columnCount, _rowCount, GetCellModel);
            _swapManager.Init(_matchManager, _cellSize, GetCellModel);
        }

        private void SetCamera()
        {
            Vector2 cameraPosition = new Vector2((_columnCount - 1) * _cellSize / 2, (_rowCount - 1) * _cellSize / 2);
            _view.SetCameraPosition(cameraPosition);
        }

        private void SetCellModelList()
        {
            _view.CreateDropItemPool(_columnCount * _rowCount);
            DropItemType[,] initialDropItemTypeList = _dropItemDeterminer.GetInitialDropItemTypes(_columnCount, _rowCount);
            Vector2 sizeOfSprite = _view.GetOriginalSizeOfSprites();
            Vector2 localScaleOfDropItem = new Vector2(1 / sizeOfSprite.x, 1 / sizeOfSprite.y) * _cellSize;
            for (int i = 0; i < _columnCount; i++)
            {
                for (int j = 0; j < _rowCount; j++)
                {
                    Vector2 position = new Vector2(i, j) * _cellSize + _originPosition;
                    CellModel cellModel = new CellModel(i, j, position, localScaleOfDropItem, OnMoveCompleted);
                    cellModel.DropItemType = initialDropItemTypeList[i, j];
                    _view.CreateTile(position, localScaleOfDropItem);
                    IDropItemView dropItem = _view.GetDropItemFromPool();
                    dropItem.SetPosition(position);
                    dropItem.SetDropItemSprite(_view.GetDropItemSprite(cellModel.DropItemType));
                    cellModel.SetDropItem(dropItem);
                    cellModel.HasPlacedDropItem = true;
                    _cellModelList[i, j] = cellModel;
                }
            }
        }
        
        
        private void ExplodeDropItem(CellModel cellModel)
        {
            _view.ReturnDropItemToPool(cellModel.GetDropItem());
            cellModel.RemoveDropItem();
        }
        
        private void OnMoveCompleted(int columnIndex, int rowIndex)
        {
            CellModel cellModel = GetCellModel(columnIndex, rowIndex);
            cellModel.HasPlacedDropItem = true;
            if (_activeCellModelsManager.CheckAllActiveCellModelsCompleted(cellModel, out int listIndex))
            {
                List<CellModel> matchedCellModelList = _matchManager.GetMatchedCellModels(_activeCellModelsManager.GetSimultaneousCellModels(listIndex));
                List<CellModel> simultaneouslyMovingCellModels = new List<CellModel>();
                foreach (CellModel matchedCellModel in matchedCellModelList)
                {
                    ExplodeDropItem(matchedCellModel);
                    matchedCellModel.HasPlacedDropItem = false;
                }

                Dictionary<CellModel, int> targetRowIndexOfFillingDropItems =
                    _fillingDropItemDeterminer.GetTargetRowIndexOfFillingDropItems(
                        out int[] emptyCellCountInEachColumn);
                
                foreach (KeyValuePair<CellModel, int> pair in targetRowIndexOfFillingDropItems)
                {
                    CellModel cellModelToBeFilled = Fill(pair.Key, pair.Value);
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
                            float belowDropItemVerticalPosition = GetCellModel(columnIndexOfSpawned, rowIndexOfSpawned - 1).GetDropItem().GetPosition().y;
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
            if (cellModel.HasAssignedDropItem())
            {
                Debug.LogError("Target cell has drop item view.");
            }
            cellModel.DropItemType = _dropItemDeterminer.GenerateRandomDropItemType();
            Vector2 initialPosition = new Vector2(cellModel.Position.x, initialVerticalPosition);
            IDropItemView dropItem = _view.GetDropItemFromPool();
            dropItem.SetPosition(initialPosition);
            dropItem.SetDropItemSprite(_view.GetDropItemSprite(cellModel.DropItemType));
            cellModel.SetDropItem(dropItem);
            dropItem.UpdateTargetVerticalPosition(cellModel.Position);
            return cellModel;
        }

        private CellModel Fill(CellModel cellModel, int targetRowIndex)
        {
            CellModel newCellModel = GetCellModel(cellModel.ColumnIndex, targetRowIndex);
            if (!cellModel.HasAssignedDropItem())
            {
                Debug.LogError("There is not filling object in the cell");
            }
            
            if (newCellModel.HasAssignedDropItem())
            {
                Debug.LogError("Target cell is not empty.");
            }
            
            DropItemType dropItemType = cellModel.DropItemType;
            newCellModel.DropItemType = dropItemType;
            IDropItemView dropItem = cellModel.GetDropItem();
            newCellModel.SetDropItem(dropItem);
            dropItem.UpdateTargetVerticalPosition(newCellModel.Position);
            cellModel.RemoveDropItem();
            cellModel.HasPlacedDropItem = false;
            return newCellModel;
        }
        
        private CellModel GetCellModel(int columnIndex, int rowIndex)
        {
            if (columnIndex < 0 || rowIndex < 0 || columnIndex >= _columnCount || rowIndex >= _rowCount) {
                return null;
            }
            return _cellModelList[columnIndex, rowIndex];
        }
    }
    
    public interface IBoardController
    {
        void InitializeBoard(int columnCount, int rowCount);
    }
}