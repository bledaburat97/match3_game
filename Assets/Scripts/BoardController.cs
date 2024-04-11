using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace Board
{
    public class BoardController : IBoardController
    {
        private int _columnCount;
        private int _rowCount;
        private float _cellSize;
        private Vector2 _originPosition;
        private List<int> _nonSpawnableColumnIndices;
        private IBoardView _view;
        private IRandomDropItemDeterminer _randomDropItemDeterminer;
        private IMatchManager _matchManager;
        private IFillingDropItemDeterminer _fillingDropItemDeterminer;
        private IActiveCellModelsManager _activeCellModelsManager;
        private ISwapManager _swapManager;
        private CellModel[,] _cellModelList;

        public BoardController(IBoardView view, IActiveCellModelsManager activeCellModelsManager, Vector2 originPosition, ISwapManager swapManager, List<int> nonSpawnableColumnIndices)
        {
            _view = view;
            _originPosition = originPosition;
            _activeCellModelsManager = activeCellModelsManager;
            _swapManager = swapManager;
            _nonSpawnableColumnIndices = nonSpawnableColumnIndices;
        }
        
        public void InitializeBoard(int columnCount, int rowCount)
        {
            _randomDropItemDeterminer = new RandomDropItemDeterminer();
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

        //Create cells and their initial images.
        private void SetCellModelList()
        {
            _view.CreateDropItemPool(_columnCount * _rowCount);
            DropItemType[,] initialDropItemTypeList = _randomDropItemDeterminer.GetInitialDropItemTypes(_columnCount, _rowCount);
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
        
        //When an item completes its move
        private void OnMoveCompleted(int columnIndex, int rowIndex)
        {
            CellModel cellModel = GetCellModel(columnIndex, rowIndex);
            cellModel.HasPlacedDropItem = true;
            //check other items which are moving simultaneously with the item completes their moves.
            if (_activeCellModelsManager.CheckAllActiveCellModelsCompleted(cellModel, out int activeCellModelsListIndex))
            {
                ExplodeBoard(activeCellModelsListIndex);
            }
        }

        //search a match for each item which completed its move
        private void ExplodeBoard(int activeCellModelsListIndex)
        {
            List<CellModel> matchedCellModelList = _matchManager.GetMatchedCellModels(_activeCellModelsManager.GetSimultaneouslyActiveCellModelsList()[activeCellModelsListIndex]);
            Sequence explosionSequence = DOTween.Sequence();
            //if there is a match explode the items. 
            foreach (CellModel matchedCellModel in matchedCellModelList)
            {
                explosionSequence.Join(ExplodeDropItem(matchedCellModel));
            }

            //After the explosion fill the empty cells on the board.
            explosionSequence.OnComplete(() => FillBoard(activeCellModelsListIndex)).Play();
        }

        private void FillBoard(int activeCellModelsListIndex)
        {
            //newActiveCellModels is a list of new active moving items after the explosion.
            List<CellModel> newActiveCellModels = new List<CellModel>();

            //get target row index of cell models which were on the board during the explosion and are filling the empty cells
            Dictionary<CellModel, int> targetRowIndexOfFillingDropItems =
                _fillingDropItemDeterminer.GetTargetRowIndexOfFillingDropItems(
                    out int[] emptyCellCountInEachColumn);

            foreach (KeyValuePair<CellModel, int> pair in targetRowIndexOfFillingDropItems)
            {
                CellModel cellModelToBeFilled = SetNewDropItemOfEmptyCell(pair.Key, pair.Value);
                newActiveCellModels.Add(cellModelToBeFilled); //the cell models are added to list of active moving items.
            }

            //the cell models whose drop items are spawned are added to list of active moving items.
            newActiveCellModels.AddRange(GetCellModelsOfDropItemsToBeFell(emptyCellCountInEachColumn)); 
            
            //cellModelsToBeMatched refers to cell models which will explode when the new active moving item list completed their moves.
            List<CellModel> cellModelsToBeMatched = _matchManager.GetMatchedCellModels(newActiveCellModels);
            
            //check new active moving item list has same cell model with an already declared active moving item list.
            if (_swapManager.CheckIfColumnIndicesIntersectWithAnyActiveCellModelList(newActiveCellModels, cellModelsToBeMatched, out int intersectedActiveCellModelsListIndex))
            {
                //if intersected active cell model list is the list the one whose items completed their moves. 
                if (intersectedActiveCellModelsListIndex == activeCellModelsListIndex)
                {
                    _activeCellModelsManager.CreateNewActiveCellModelList(newActiveCellModels);
                }
                else
                {
                    _activeCellModelsManager.AddActiveCellModelsToAlreadyActiveList(newActiveCellModels, intersectedActiveCellModelsListIndex);
                }
            }
            else
            {
                _activeCellModelsManager.CreateNewActiveCellModelList(newActiveCellModels);
            }
            
            //Remove the list whose items completed their moves. 
            _activeCellModelsManager.RemoveActiveCellModelsAtIndex(activeCellModelsListIndex);
        }
        
        //Explode drop item
        private Sequence ExplodeDropItem(CellModel cellModel)
        {
            IDropItemView dropItem = cellModel.GetDropItem();
            cellModel.HasPlacedDropItem = false;
            return dropItem.AnimateExplosion().OnComplete(() =>
            {
                cellModel.RemoveDropItem();
                _view.ReturnDropItemToPool(dropItem);
            }).Pause();
        }

        private List<CellModel> GetCellModelsOfDropItemsToBeFell(int[] emptyCellCountInEachColumn)
        {
            List<CellModel> cellModelsToBeFell = new List<CellModel>();
            for (int i = 0; i < _columnCount; i++)
            {
                if (_nonSpawnableColumnIndices.Contains(i)) continue;
                
                for (int j = 0; j < emptyCellCountInEachColumn[i]; j++)
                {
                    int columnIndexOfSpawned = i;
                    int rowIndexOfSpawned = _rowCount - emptyCellCountInEachColumn[i] + j;
                    float initialVerticalPosition = _rowCount * _cellSize;
                    if (rowIndexOfSpawned > 0)
                    {
                        float belowDropItemVerticalPosition = GetCellModel(columnIndexOfSpawned, rowIndexOfSpawned - 1)
                            .GetDropItem().GetPosition().y;
                        if (belowDropItemVerticalPosition > (_rowCount - 1) * _cellSize)
                        {
                            initialVerticalPosition = belowDropItemVerticalPosition + _cellSize;
                        }
                    }

                    CellModel cellModelToBeFell = SpawnNewDropItemForEmptyCell(i, rowIndexOfSpawned,
                        initialVerticalPosition);
                    cellModelsToBeFell.Add(cellModelToBeFell);
                }
            }

            return cellModelsToBeFell;
        }
        
        private CellModel SetNewDropItemOfEmptyCell(CellModel cellModel, int targetRowIndex)
        {
            CellModel newCellModel = GetCellModel(cellModel.ColumnIndex, targetRowIndex);
            if (!cellModel.HasAssignedDropItem()) Debug.LogError("There is not filling object in the cell");
            if (newCellModel.HasAssignedDropItem()) Debug.LogError("Target cell is not empty.");
            
            DropItemType dropItemType = cellModel.DropItemType;
            newCellModel.DropItemType = dropItemType;
            IDropItemView dropItem = cellModel.GetDropItem();
            newCellModel.SetDropItem(dropItem);
            dropItem.UpdateTargetVerticalPosition(newCellModel.Position);
            cellModel.RemoveDropItem();
            cellModel.HasPlacedDropItem = false;
            return newCellModel;
        }

        private CellModel SpawnNewDropItemForEmptyCell(int columnIndex, int rowIndex, float initialVerticalPosition)
        {
            CellModel cellModel = GetCellModel(columnIndex, rowIndex);
            if (cellModel.HasAssignedDropItem()) Debug.LogError("Target cell has drop item view.");
            cellModel.DropItemType = _randomDropItemDeterminer.GenerateRandomDropItemType();
            Vector2 initialPosition = new Vector2(cellModel.Position.x, initialVerticalPosition);
            IDropItemView dropItem = _view.GetDropItemFromPool();
            dropItem.SetPosition(initialPosition);
            dropItem.SetDropItemSprite(_view.GetDropItemSprite(cellModel.DropItemType));
            cellModel.SetDropItem(dropItem);
            dropItem.UpdateTargetVerticalPosition(cellModel.Position);
            return cellModel;
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