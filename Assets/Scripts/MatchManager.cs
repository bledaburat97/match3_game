using System;
using System.Collections.Generic;
using System.Linq;

namespace Board
{
    public class MatchManager : IMatchManager
    {
        private Func<int, int, CellModel> _getCellModel;
        private Dictionary<CellModel, int> _matchedCellModelAndMatchIndexDict;
        private int _matchCount;
        
        public MatchManager(int columnCount, int rowCount, Func<int, int, CellModel> getCellModel)
        {
            _getCellModel = getCellModel;
            ResetMatchedCellModels();
        }
        
        private void ResetMatchedCellModels()
        {
            _matchedCellModelAndMatchIndexDict = new Dictionary<CellModel, int>();
            _matchCount = 0;
        }

        //Sets all different matches in a dictionary, and returns them in a merged list.
        //All different matches are stored separately because in future, there can be need for element count of each match.
        public List<CellModel> GetMatchedCellModels(List<CellModel> cellModelsToBeChecked)
        {
            ResetMatchedCellModels();
            //At first check vertical matches for all cell models to be checked.
            foreach (CellModel cellModel in cellModelsToBeChecked)
            {
                SetVerticalMatch(cellModel);
            }
            //Then check horizontal matches for all cell models and remove intersections.
            foreach (CellModel cellModel in cellModelsToBeChecked)
            {
                SetHorizontalMatch(cellModel);
            }

            return _matchedCellModelAndMatchIndexDict.Keys.ToList();
        }
        
        //check a match for a cell model in vertical line and if there is a match, add the cell model.
        private void SetVerticalMatch(CellModel cellModel)
        {
            //if it is already on the list, its probable match must have been added to the list.
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
                
                //if this link has common cell models with any stored matches, hold these match indexes.
                for (int i = 0; i < horizontalLinkAmount; i++)
                {
                    if (_matchedCellModelAndMatchIndexDict.TryGetValue(
                            _getCellModel(leftMostColumnIndex + i, cellModel.RowIndex), out int intersectedMatchIndex))
                    {
                        intersectedMatchIndexes.Add(intersectedMatchIndex);
                    }
                }

                int matchIndex = _matchCount;
                
                //if there are matches which have common cell models with this link,
                //set match indexes of all of the cell models in those matches as the first intersected match index.
                //so, these matches becomes merged.
                if (intersectedMatchIndexes.Count > 0)
                {
                    matchIndex = intersectedMatchIndexes[0];
                    for (int i = 1; i < intersectedMatchIndexes.Count; i++)
                    {
                        UpdateMatchIndexesOfCellModels(intersectedMatchIndexes[i], matchIndex);
                    }
                }
                
                //Add the non intersected cell models of the link to the match list.
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

            while (_getCellModel(currentColumn, currentRow) != null &&
                   _getCellModel(currentColumn, currentRow).HasPlacedDropItem &&
                   _getCellModel(currentColumn, currentRow).DropItemType == dropItemType)
            {
                linkAmount++;
                currentColumn += columnStep;
                currentRow += rowStep;
            }

            return linkAmount;
        }
    }

    public interface IMatchManager
    {
        List<CellModel> GetMatchedCellModels(List<CellModel> cellModelsToBeChecked);
    }
}