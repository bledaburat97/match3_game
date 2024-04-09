using System;
using System.Collections.Generic;
using System.Linq;

namespace Board
{
    public class MatchManager : IMatchManager
    {
        private int _columnCount = 8;
        private int _rowCount = 8;
        private Func<int, int, CellModel> _getCellModel;
        private Dictionary<CellModel, int> _matchedCellModelAndMatchIndexDict;
        private int _matchCount;
        
        public MatchManager(int columnCount, int rowCount, Func<int, int, CellModel> getCellModel)
        {
            _columnCount = columnCount;
            _rowCount = rowCount;
            _getCellModel = getCellModel;
            ResetMatchedCellModels();
        }
        
        private void ResetMatchedCellModels()
        {
            _matchedCellModelAndMatchIndexDict = new Dictionary<CellModel, int>();
            _matchCount = 0;
        }

        public List<CellModel> GetMatchedCellModels(List<CellModel> cellModelsToBeChecked)
        {
            ResetMatchedCellModels();
            foreach (CellModel cellModel in cellModelsToBeChecked)
            {
                SetVerticalMatch(cellModel);
            }

            foreach (CellModel cellModel in cellModelsToBeChecked)
            {
                SetHorizontalMatch(cellModel);
            }

            return _matchedCellModelAndMatchIndexDict.Keys.ToList();
        }
        
        private void SetVerticalMatch(CellModel cellModel)
        {
            if (_matchedCellModelAndMatchIndexDict.ContainsKey(cellModel)) return;

            int downLinkAmount = GetLinkAmount(cellModel.ColumnIndex, cellModel.RowIndex, 0, -1, cellModel.DropItemType);
            int upLinkAmount = GetLinkAmount(cellModel.ColumnIndex, cellModel.RowIndex, 0, 1, cellModel.DropItemType);

            int verticalLinkAmount = 1 + downLinkAmount + upLinkAmount;
            if (verticalLinkAmount >= 3)
            {
                int downMostRowIndex = cellModel.RowIndex - downLinkAmount;
                for (int i = 0; i < verticalLinkAmount; i++)
                {
                    _matchedCellModelAndMatchIndexDict.Add(_getCellModel(cellModel.ColumnIndex, downMostRowIndex + i), _matchCount);
                }
            }

            _matchCount += 1;
        }
        
        private void SetHorizontalMatch(CellModel cellModel)
        {
            int leftLinkAmount = GetLinkAmount(cellModel.ColumnIndex, cellModel.RowIndex, -1, 0, cellModel.DropItemType);
            int rightLinkAmount = GetLinkAmount(cellModel.ColumnIndex, cellModel.RowIndex, 1, 0, cellModel.DropItemType);

            int horizontalLinkAmount = 1 + leftLinkAmount + rightLinkAmount;
            if (horizontalLinkAmount >= 3)
            {
                int leftMostColumnIndex = cellModel.ColumnIndex - leftLinkAmount;
                List<int> intersectedMatchIndexes = new List<int>();
                for (int i = 0; i < horizontalLinkAmount; i++)
                {
                    if (_matchedCellModelAndMatchIndexDict.TryGetValue(
                            _getCellModel(leftMostColumnIndex + i, cellModel.RowIndex), out int intersectedMatchIndex))
                    {
                        intersectedMatchIndexes.Add(intersectedMatchIndex);
                    }
                }

                int matchIndex = _matchCount;
                if (intersectedMatchIndexes.Count > 0)
                {
                    matchIndex = intersectedMatchIndexes[0];
                    for (int i = 1; i < intersectedMatchIndexes.Count; i++)
                    {
                        UpdateMatchIndexesOfCellModels(intersectedMatchIndexes[i], matchIndex);
                    }
                }
                
                for (int i = 0; i < horizontalLinkAmount; i++)
                {
                    if (!_matchedCellModelAndMatchIndexDict.ContainsKey(_getCellModel(leftMostColumnIndex + i, cellModel.RowIndex)))
                    {
                        _matchedCellModelAndMatchIndexDict.Add(_getCellModel(leftMostColumnIndex + i, cellModel.RowIndex), matchIndex);
                    }
                }

                if (intersectedMatchIndexes.Count == 0) _matchCount += 1;
            }

        }

        private void UpdateMatchIndexesOfCellModels(int previousMatchIndex, int newMatchIndex)
        {
            var keysToUpdate = new List<CellModel>();

            foreach (var pair in _matchedCellModelAndMatchIndexDict)
            {
                if (pair.Value == previousMatchIndex)
                {
                    keysToUpdate.Add(pair.Key);
                }
            }

            foreach (var key in keysToUpdate)
            {
                _matchedCellModelAndMatchIndexDict[key] = newMatchIndex;
            }
        }
        
        private int GetLinkAmount(int columnIndex, int rowIndex, int columnStep, int rowStep, DropItemType dropItemType)
        {
            int linkAmount = 0;
            int currentColumn = columnIndex + columnStep;
            int currentRow = rowIndex + rowStep;

            while (IsValidPosition(currentColumn, currentRow) &&
                   _getCellModel(currentColumn, currentRow).HasPlacedDropItem &&
                   _getCellModel(currentColumn, currentRow).DropItemType == dropItemType)
            {
                linkAmount++;
                currentColumn += columnStep;
                currentRow += rowStep;
            }

            return linkAmount;
        }
        
        private bool IsValidPosition(int columnIndex, int rowIndex) {
            if (columnIndex < 0 || rowIndex < 0 || columnIndex >= _columnCount || rowIndex >= _rowCount) {
                return false;
            } 
            return true;
        }
    }

    public interface IMatchManager
    {
        List<CellModel> GetMatchedCellModels(List<CellModel> cellModelsToBeChecked);
    }
}