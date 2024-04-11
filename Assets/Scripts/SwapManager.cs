using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using UnityEngine;

namespace Board
{
    public class SwapManager : ISwapManager
    {
        private int _dragStartedColumnIndex;
        private int _dragStartedRowIndex;
        private Vector2 _originPosition;
        private float _cellSize;
        private Vector2 _dragStartedPosition;
        private Func<int, int, CellModel> _getCellModel;
        private IActiveCellModelsManager _activeCellModelsManager;
        private IMatchManager _matchManager;
        private IBoardView _boardView;
        
        public SwapManager(IBoardView boardView, IActiveCellModelsManager activeCellModelsManager,  Vector2 originPosition)
        {
            _dragStartedColumnIndex = -1;
            _dragStartedRowIndex = -1;
            _boardView = boardView;
            _activeCellModelsManager = activeCellModelsManager;
            _originPosition = originPosition;

        }

        public void Init(IMatchManager matchManager, float cellSize, Func<int, int, CellModel> getCellModel)
        {
            _matchManager = matchManager;
            _cellSize = cellSize;
            _getCellModel = getCellModel;
            SubscribeEvents();
        }

        private void SubscribeEvents()
        {
            _boardView.OnDragStartedEvent += OnDragStarted;
            _boardView.OnDragEndedEvent += OnDragEnded;
        }
        
        private void OnDragStarted(object sender, Vector2 worldPosition)
        {
            CalculateIndices(worldPosition, out _dragStartedColumnIndex, out _dragStartedRowIndex);
            _dragStartedPosition = worldPosition;
        }
        
        private void OnDragEnded(object sender, Vector2 worldPosition)
        {
            CellModel firstCellModel = _getCellModel(_dragStartedColumnIndex, _dragStartedRowIndex);
            if (firstCellModel == null || !firstCellModel.HasPlacedDropItem) return;
            if (!IsWorldPositionOutsideTheInitialCell(worldPosition)) return;
            GetTargetCellModel(worldPosition, out int columnIndex, out int rowIndex);
            CellModel secondCellModel = _getCellModel(columnIndex, rowIndex);
            firstCellModel.HasPlacedDropItem = false;
            List<CellModel> activeCellModels = new List<CellModel>();
            if (secondCellModel == null || !secondCellModel.HasPlacedDropItem)
            {
                activeCellModels.Add(firstCellModel);
                AddActiveCellModels(activeCellModels, new List<CellModel>());
                firstCellModel.GetDropItem().AnimateFlick(new Vector2(columnIndex - _dragStartedColumnIndex, rowIndex - _dragStartedRowIndex));
                return;
            }
            
            secondCellModel.HasPlacedDropItem = false;
            activeCellModels.Add(firstCellModel);
            activeCellModels.Add(secondCellModel);
            if (CanSwap(firstCellModel, secondCellModel, out List<CellModel> cellModelsToBeMatched))
            {
                SwapDropItemTypes(firstCellModel, secondCellModel);
                AddActiveCellModels(activeCellModels, cellModelsToBeMatched);
                SwapDropItems(firstCellModel, secondCellModel);
            }

            else
            {
                AddActiveCellModels(activeCellModels, new List<CellModel>());
                firstCellModel.GetDropItem().AnimateSwapAndBack(secondCellModel.Position, true);
                secondCellModel.GetDropItem().AnimateSwapAndBack(firstCellModel.Position, false);
            }
        }
        
        public bool CheckIfColumnIndicesIntersectWithAnyActiveCellModelList(List<CellModel> activeCellModels, List<CellModel> cellModelsToBeMatched, out int intersectedActiveCellModelsListIndex)
        {
            List<int> columnIndices = GetActiveColumnIndices(activeCellModels, cellModelsToBeMatched);
            intersectedActiveCellModelsListIndex = -1;
            for (int i = 0; i < _activeCellModelsManager.GetSimultaneouslyActiveCellModelsList().Count; i++)
            {
                List<CellModel> cellModelsToBeMatchedOfActiveCellModels = _matchManager.GetMatchedCellModels(_activeCellModelsManager.GetSimultaneouslyActiveCellModelsList()[i]);
                foreach (CellModel cellModelToBeMatched in cellModelsToBeMatchedOfActiveCellModels)
                {
                    if (columnIndices.Contains(cellModelToBeMatched.ColumnIndex))
                    {
                        intersectedActiveCellModelsListIndex = i;
                        return true;
                    }
                }
            }

            return false;
        }

        private void AddActiveCellModels(List<CellModel> activeCellModels, List<CellModel> cellModelsToBeMatched)
        {
            if (CheckIfColumnIndicesIntersectWithAnyActiveCellModelList(activeCellModels, cellModelsToBeMatched, out int intersectedActiveCellModelsListIndex))
            {
                _activeCellModelsManager.AddActiveCellModelsToAlreadyActiveList(activeCellModels, intersectedActiveCellModelsListIndex);
            }
            else
            {
                _activeCellModelsManager.CreateNewActiveCellModelList(activeCellModels);
            }
        }
        
        private List<int> GetActiveColumnIndices(List<CellModel> activeCellModels, List<CellModel> cellModelsToBeMatched)
        {
            List<int> columnIndices = new List<int>();
            foreach (CellModel cellModel in activeCellModels)
            {
                if(!columnIndices.Contains(cellModel.ColumnIndex)) columnIndices.Add(cellModel.ColumnIndex);
            }
            foreach (CellModel cellModel in cellModelsToBeMatched)
            {
                if(!columnIndices.Contains(cellModel.ColumnIndex)) columnIndices.Add(cellModel.ColumnIndex);
            }

            return columnIndices;
        }
        
        private void CalculateIndices(Vector2 worldPosition, out int columnIndex, out int rowIndex)
        {
            columnIndex = Mathf.FloorToInt(((worldPosition - _originPosition).x + _cellSize / 2) / _cellSize);
            rowIndex = Mathf.FloorToInt(((worldPosition - _originPosition).y + _cellSize / 2) / _cellSize);
        }
        
        private bool IsWorldPositionOutsideTheInitialCell(Vector2 worldPosition)
        {
            CalculateIndices(worldPosition, out int columnIndex, out int rowIndex);

            if (columnIndex == _dragStartedColumnIndex && rowIndex == _dragStartedRowIndex) return false;
            return true;
        }

        private void GetTargetCellModel(Vector2 worldPosition, out int columnIndex, out int rowIndex)
        {
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
        }
        
        private bool CanSwap(CellModel firstCellModel, CellModel secondCellModel, out List<CellModel> cellModelsToBeMatched)
        {
            SwapDropItemTypes(firstCellModel, secondCellModel);
            cellModelsToBeMatched = _matchManager.GetMatchedCellModels(new List<CellModel> { firstCellModel, secondCellModel });
            bool isMatched = cellModelsToBeMatched.Count > 0;;
            SwapDropItemTypes(firstCellModel, secondCellModel);
            return isMatched;
        }

        private void SwapDropItemTypes(CellModel firstCellModel, CellModel secondCellModel)
        {
            DropItemType firstDropItemType = firstCellModel.DropItemType;
            DropItemType secondDropItemType = secondCellModel.DropItemType;
            firstCellModel.DropItemType = secondDropItemType;
            secondCellModel.DropItemType = firstDropItemType;
        }
        
        private void SwapDropItems(CellModel firstCellModel, CellModel secondCellModel)
        {
            IDropItemView firstDropItem = firstCellModel.GetDropItem();
            IDropItemView secondDropItem = secondCellModel.GetDropItem();

            secondCellModel.SetDropItem(firstDropItem);
            firstCellModel.SetDropItem(secondDropItem);
            firstDropItem.AnimateSwap(secondCellModel.Position, true);
            secondDropItem.AnimateSwap(firstCellModel.Position, false);
        }
    }

    public interface ISwapManager
    {
        void Init(IMatchManager matchManager, float cellSize, Func<int, int, CellModel> getCellModel);

        bool CheckIfColumnIndicesIntersectWithAnyActiveCellModelList(List<CellModel> activeCellModels,
            List<CellModel> cellModelsToBeMatched, out int intersectedActiveCellModelsListIndex);
    }
}