using System.Collections.Generic;
using DG.Tweening;
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
        private BoardView _view;
        private Vector2 _originPosition;
        private int _dragStartedColumnIndex;
        private int _dragStartedRowIndex;
        private MatchManager _matchManager;
        private float spawnedDropItemVerticalPosition = 7f;
        private FillingDropItemDeterminer _fillingDropItemDeterminer;
        
        public BoardController(BoardView view)
        {
            _view = view;
            _originPosition = Vector2.zero;
            _dragStartedColumnIndex = -1;
            _dragStartedRowIndex = -1;
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
            Vector2 cameraPosition = new Vector2((columnCount - 1) * _cellSize / 2, (rowCount - 1) * _cellSize / 2);
            _view.SetCameraPosition(cameraPosition);
            _view.SetOnClick(GetIndexOfDropItem);
            _view.SetOnDragStarted(OnDragStarted);
            _view.SetOnDragEnded(OnDragEnded);
            _view.SetDropItemViews(_cellModelList);
            _matchManager = new MatchManager();
            _fillingDropItemDeterminer = new FillingDropItemDeterminer(_columnCount, _rowCount, GetCellModel);
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
                        hasDropItem = true
                    };
                    _cellModelList[i, j] = cellModel;
                }
            }
        }
        
        private void GetIndexOfDropItem(in Vector2 worldPosition, out int columnIndex, out int rowIndex) {
            columnIndex = Mathf.FloorToInt(((worldPosition - _originPosition).x + _cellSize / 2) / _cellSize);
            rowIndex = Mathf.FloorToInt(((worldPosition - _originPosition).y + _cellSize / 2) / _cellSize);

            if (columnIndex < 0 || columnIndex >= _columnCount || rowIndex < 0 || rowIndex >= _rowCount)
            {
                columnIndex = -1;
                rowIndex = -1;
            }
        }

        private void OnDragStarted(Vector2 worldPosition)
        {
            _dragStartedColumnIndex = Mathf.FloorToInt(((worldPosition - _originPosition).x + _cellSize / 2) / _cellSize);
            _dragStartedRowIndex = Mathf.FloorToInt(((worldPosition - _originPosition).y + _cellSize / 2) / _cellSize);

            if (_dragStartedColumnIndex < 0 || _dragStartedColumnIndex >= _columnCount || _dragStartedRowIndex < 0 || _dragStartedRowIndex >= _rowCount)
            {
                _dragStartedColumnIndex = -1;
                _dragStartedRowIndex = -1;
            }
        }
        
        private void OnDragEnded(Vector2 worldPosition)
        {
            if(_dragStartedColumnIndex == -1 || _dragStartedRowIndex == -1) return;
            
            int columnIndex = Mathf.FloorToInt(((worldPosition - _originPosition).x + _cellSize / 2) / _cellSize);
            int rowIndex = Mathf.FloorToInt(((worldPosition - _originPosition).y + _cellSize / 2) / _cellSize);

            if (columnIndex != _dragStartedColumnIndex) {
                rowIndex = _dragStartedRowIndex;
                if (columnIndex < _dragStartedColumnIndex) columnIndex = _dragStartedColumnIndex - 1;
                else columnIndex = _dragStartedColumnIndex + 1;
            }
            else
            {
                columnIndex = _dragStartedColumnIndex;
                if (rowIndex < _dragStartedRowIndex) rowIndex = _dragStartedRowIndex - 1;
                else rowIndex = _dragStartedRowIndex + 1;
            }

            if (CanSwap(_dragStartedColumnIndex, _dragStartedRowIndex, columnIndex, rowIndex))
            {
                Swap(_dragStartedColumnIndex, _dragStartedRowIndex, columnIndex, rowIndex);
                _matchManager.InitMatchedCellList();
                List<CellModel> explodingCellModelList = GetMatchedCellModelsAtIndex(_dragStartedColumnIndex, _dragStartedRowIndex);
                List<CellModel> secondExplodingCellModelList = GetMatchedCellModelsAtIndex(columnIndex, rowIndex);
                List<List<CellModel>> allMatchedCellModelLists = new List<List<CellModel>>();
                if (explodingCellModelList != null)
                {
                    foreach (CellModel cellModel in explodingCellModelList)
                    {
                        cellModel.hasDropItem = false;
                    }
                    allMatchedCellModelLists.Add(explodingCellModelList);
                }
                
                if (secondExplodingCellModelList != null)
                {
                    foreach (CellModel cellModel in secondExplodingCellModelList)
                    {
                        cellModel.hasDropItem = false;
                    }
                    allMatchedCellModelLists.Add(secondExplodingCellModelList);
                }
                

                Dictionary<CellModel, int> targetRowIndexOfFillingDropItems =
                    _fillingDropItemDeterminer.GetTargetRowIndexOfFillingDropItems(out int[] emptyCellCountInEachColumn);
                
                _view.SwapDropItems(_cellModelList[_dragStartedColumnIndex, _dragStartedRowIndex],
                    _cellModelList[columnIndex, rowIndex], () => OnAllMatchesDetermined(allMatchedCellModelLists, targetRowIndexOfFillingDropItems, emptyCellCountInEachColumn));
            }
        }

        private void OnAllMatchesDetermined(List<List<CellModel>> allMatchedCellModelLists, Dictionary<CellModel, int> targetRowIndexOfFillingDropItems, int[] emptyCellCountInEachColumn)
        {
            foreach (List<CellModel> cellModelList in allMatchedCellModelLists)
            {
                foreach (CellModel cellModel in cellModelList)
                {
                    _view.ExplodeDropItem(cellModel.columnIndex, cellModel.rowIndex);
                }
            }
            
            foreach (KeyValuePair<CellModel, int> pair in targetRowIndexOfFillingDropItems)
            {
                Fill(pair.Key, pair.Value);
                _view.FillDropItem(pair.Key, GetCellModel(pair.Key.columnIndex, pair.Value));
            }

            SpawnNewDropItems(emptyCellCountInEachColumn);
        }
        
        private void SpawnNewDropItems(int[] emptyCellCountInEachColumn)
        {
            int totalEmptyCellCount = 0;
            foreach (int matchedPieceCount in emptyCellCountInEachColumn)
            {
                totalEmptyCellCount += matchedPieceCount;
            }
            DropItemType[] randomDropItemTypeList = _dropItemDeterminer.GenerateRandomDropItemTypeList(totalEmptyCellCount);
            

            int dropItemIndex = 0;
            Sequence sequence = DOTween.Sequence();
            for (int i = 0; i < _columnCount; i++)
            {
                for (int j = 0; j < emptyCellCountInEachColumn[i]; j++)
                {
                    CellModel cellModel = GetCellModel(i, _rowCount - emptyCellCountInEachColumn[i] + j);
                    Fall(cellModel, randomDropItemTypeList[dropItemIndex]);
                    float initialVerticalPosition = spawnedDropItemVerticalPosition + _cellSize * j;
                    sequence.Join(_view.FallNewDropItemView(cellModel, initialVerticalPosition));
                    dropItemIndex++;
                }
            }

            sequence.OnComplete(OnAllDropItemsFell).Play();
        }
        
        private void OnAllDropItemsFell()
        {
            _matchManager.InitMatchedCellList();
            List<List<CellModel>> allMatchedCellModels = GetAllMatchedCellModelsOnBoard();
            if (allMatchedCellModels.Count == 0) return;
            foreach (List<CellModel> cellModelList in allMatchedCellModels)
            {
                foreach (CellModel cellModel in cellModelList)
                {
                    cellModel.hasDropItem = false;
                }
            }
            Dictionary<CellModel, int> targetRowIndexOfFillingDropItems = _fillingDropItemDeterminer.GetTargetRowIndexOfFillingDropItems(out int[] emptyCellCountInEachColumn);
            OnAllMatchesDetermined(allMatchedCellModels, targetRowIndexOfFillingDropItems, emptyCellCountInEachColumn);
        }

        private bool CanSwap(int firstColumnIndex, int firstRowIndex, int secondColumnIndex, int secondRowIndex)
        {
            if (secondColumnIndex < 0 || secondColumnIndex >= _columnCount || firstColumnIndex < 0 || firstRowIndex >= _rowCount)
            {
                return false;
            }

            if (firstColumnIndex == secondColumnIndex && firstRowIndex == secondRowIndex)
            {
                return false;
            }
            
            _matchManager.InitMatchedCellList();
            Swap(firstColumnIndex, firstRowIndex, secondColumnIndex, secondRowIndex);
            bool isMatched = IsMatched(firstColumnIndex, firstRowIndex) || IsMatched(secondColumnIndex, secondRowIndex);
            Swap(firstColumnIndex, firstRowIndex, secondColumnIndex, secondRowIndex);
            Debug.Log(isMatched);
            return isMatched;
        }

        private void Swap(int firstColumnIndex, int firstRowIndex, int secondColumnIndex, int secondRowIndex)
        {
            DropItemType firstDropItemType = _cellModelList[firstColumnIndex, firstRowIndex].dropItemType;
            DropItemType secondDropItemType = _cellModelList[secondColumnIndex, secondRowIndex].dropItemType;

            _cellModelList[firstColumnIndex, firstRowIndex].dropItemType = secondDropItemType;
            _cellModelList[secondColumnIndex, secondRowIndex].dropItemType = firstDropItemType;
        }

        private void Fill(CellModel cellModel, int targetRowIndex)
        {
            CellModel newCellModel = GetCellModel(cellModel.columnIndex, targetRowIndex);
            if (!cellModel.hasDropItem)
            {
                Debug.LogError("There is not filling object in the cell");
            }
            
            if (newCellModel.hasDropItem)
            {
                Debug.LogError("Target cell is not empty.");
                return;
            }
            
            DropItemType dropItemType = cellModel.dropItemType;
            newCellModel.dropItemType = dropItemType;
            cellModel.hasDropItem = false;
            newCellModel.hasDropItem = true;
        }

        private void Fall(CellModel cellModel, DropItemType dropItemType)
        {
            if (cellModel.hasDropItem)
            {
                Debug.LogError("Target cell is not empty.");
                return;
            }

            cellModel.dropItemType = dropItemType;
            cellModel.hasDropItem = true;
        }

        private bool IsMatched(int columnIndex, int rowIndex)
        {
            List<CellModel> verticallyMatchedCellModels = _matchManager.CheckVerticalMatch(columnIndex, rowIndex, _cellModelList);
            List<CellModel> horizontallyMatchedCellModels =
                _matchManager.CheckHorizontalMatch(columnIndex, rowIndex, _cellModelList, out List<CellModel> intersectedCellModels);
            return verticallyMatchedCellModels != null || horizontallyMatchedCellModels != null;
        }

        private List<CellModel> GetMatchedCellModelsAtIndex(int columnIndex, int rowIndex)
        {
            List<CellModel> verticallyMatchedCellModels = _matchManager.CheckVerticalMatch(columnIndex, rowIndex, _cellModelList);
            List<CellModel> horizontallyMatchedCellModels = _matchManager.CheckHorizontalMatch(columnIndex, rowIndex, _cellModelList, out List<CellModel> intersectedCellModels);
            if (verticallyMatchedCellModels != null)
            {
                if (horizontallyMatchedCellModels != null)
                {
                    verticallyMatchedCellModels.Remove(_cellModelList[columnIndex, rowIndex]);
                    verticallyMatchedCellModels.AddRange(horizontallyMatchedCellModels);
                    return verticallyMatchedCellModels;
                }
                else
                {
                    return verticallyMatchedCellModels;
                }
            }
            return horizontallyMatchedCellModels;
        }


        private List<List<CellModel>> GetAllMatchedCellModelsOnBoard()
        {
            List<List<CellModel>> allMatchedCellModels = new List<List<CellModel>>();
            for (int i = 0; i < _columnCount; i++)
            {
                for (int j = 0; j < _rowCount; j++)
                {
                    List<CellModel> verticallyMatchedCellModels = _matchManager.CheckVerticalMatch(i, j, _cellModelList);
                    if (verticallyMatchedCellModels != null)
                    {
                        allMatchedCellModels.Add(verticallyMatchedCellModels);
                    }
                }
            }

            for (int i = 0; i < _columnCount; i++)
            {
                for (int j = 0; j < _rowCount; j++)
                {
                    List<CellModel> horizontallyMatchedCellModels = _matchManager.CheckHorizontalMatch(i, j, _cellModelList, out List<CellModel> intersectedCellModels);
                    if (horizontallyMatchedCellModels != null)
                    {
                        if (intersectedCellModels.Count == 0)
                        {
                            allMatchedCellModels.Add(horizontallyMatchedCellModels);
                        }
                        
                        else if (intersectedCellModels.Count < horizontallyMatchedCellModels.Count)
                        {
                            MergeCellModelLists(intersectedCellModels, horizontallyMatchedCellModels);
                        }
                    }

                }
            }

            return allMatchedCellModels;

            void MergeCellModelLists(List<CellModel> intersectedCellModels, List<CellModel> horizontallyMatchedCellModels)
            {
                List<List<CellModel>> cellModelListsToBeMerged = new List<List<CellModel>>();
                foreach (CellModel cellModel in intersectedCellModels)
                {
                    for (int i = 0; i < allMatchedCellModels.Count; i++)
                    {
                        if (allMatchedCellModels[i].Contains(cellModel))
                        {
                            cellModelListsToBeMerged.Add(allMatchedCellModels[i]);
                        }
                    }
                }

                List<CellModel> mergedCellModelList = new List<CellModel>();
                foreach (List<CellModel> cellModelList in cellModelListsToBeMerged)
                {
                    mergedCellModelList.AddRange(cellModelList);
                    allMatchedCellModels.Remove(cellModelList);
                }

                foreach (CellModel cellModel in intersectedCellModels)
                {
                    mergedCellModelList.Remove(cellModel);
                }
                
                mergedCellModelList.AddRange(horizontallyMatchedCellModels);
                allMatchedCellModels.Add(mergedCellModelList);
            }
        }

        private CellModel GetCellModel(int columnIndex, int rowIndex)
        {
            return _cellModelList[columnIndex, rowIndex];
        }
    }

    public interface IBoardController
    {
        void InitializeBoard(int columnCount, int rowCount);
    }
}