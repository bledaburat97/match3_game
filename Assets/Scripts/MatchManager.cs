using System.Collections.Generic;

namespace Board
{
    public class MatchManager
    {
        private int _columnCount = 8;
        private int _rowCount = 8;

        private bool[,] _alreadyMatchedCellList;

        public void InitMatchedCellList()
        {
            _alreadyMatchedCellList = new bool[_columnCount, _rowCount];
            for (int i = 0; i < _columnCount; i++)
            {
                for (int j = 0; j < _rowCount; j++)
                {
                    _alreadyMatchedCellList[i, j] = false;
                }
            }
        }

        public List<CellModel> CheckHorizontalMatch(int columnIndex, int rowIndex, CellModel[,] cellModels, out bool isSeparateMatch)
        {
            List<CellModel> matchedCellModels = new List<CellModel>();
            CellModel swappedCellModel = cellModels[columnIndex, rowIndex];
            DropItemType dropItemType = swappedCellModel.dropItemType;
            isSeparateMatch = true;
            int rightLinkAmount = 0;
            int leftLinkAmount = 0;

            for (int i = 1; i < _columnCount; i++)
            {
                if (IsValidPosition(columnIndex - i, rowIndex))
                {
                    if (cellModels[columnIndex - i, rowIndex].hasDropItem &&
                        cellModels[columnIndex - i, rowIndex].dropItemType == dropItemType)
                    {
                        leftLinkAmount++;
                    }
                    else
                    {
                        break;
                    }
                }
                else
                {
                    break;
                }
            }
            
            for (int i = 1; i < _columnCount; i++)
            {
                if (IsValidPosition(columnIndex + i, rowIndex))
                {
                    if (cellModels[columnIndex + i, rowIndex].hasDropItem &&
                        cellModels[columnIndex + i, rowIndex].dropItemType == dropItemType)
                    {
                        rightLinkAmount++;
                    }
                    else
                    {
                        break;
                    }
                }
                else
                {
                    break;
                }
            }
            
            int horizontalLinkAmount = 1 + leftLinkAmount + rightLinkAmount;
            if (horizontalLinkAmount >= 3) 
            {
                int leftMostColumnIndex = columnIndex - leftLinkAmount;
                int alreadyMatchedCellCount = 0;
                for (int i = 0; i < horizontalLinkAmount; i++) {
                    if (_alreadyMatchedCellList[leftMostColumnIndex + i, rowIndex])
                    {
                        alreadyMatchedCellCount++;
                    }
                    matchedCellModels.Add(cellModels[leftMostColumnIndex + i, rowIndex]);
                    _alreadyMatchedCellList[leftMostColumnIndex + i, rowIndex] = true;
                }

                if (alreadyMatchedCellCount == 0)
                {
                    isSeparateMatch = true;
                    return matchedCellModels;
                }
                    
                else if (alreadyMatchedCellCount < horizontalLinkAmount)
                {
                    isSeparateMatch = false;
                    return matchedCellModels;
                }
            }

            return null;
        }
        
        public List<CellModel> CheckVerticalMatch(int columnIndex, int rowIndex, CellModel[,] cellModels)
        {
            List<CellModel> matchedCellModels = new List<CellModel>();
            CellModel swappedCellModel = cellModels[columnIndex, rowIndex];
            DropItemType dropItemType = swappedCellModel.dropItemType;
            int upLinkAmount = 0;
            int downLinkAmount = 0;

            for (int i = 1; i < _rowCount; i++)
            {
                if (IsValidPosition(columnIndex, rowIndex - i))
                {
                    if (cellModels[columnIndex, rowIndex - i].hasDropItem &&
                        cellModels[columnIndex, rowIndex - i].dropItemType == dropItemType)
                    {
                        downLinkAmount++;
                    }
                    else
                    {
                        break;
                    }
                }
                else
                {
                    break;
                }
            }
            
            for (int i = 1; i < _rowCount; i++)
            {
                if (IsValidPosition(columnIndex, rowIndex + i))
                {
                    if (cellModels[columnIndex, rowIndex + i].hasDropItem &&
                        cellModels[columnIndex, rowIndex + i].dropItemType == dropItemType)
                    {
                        upLinkAmount++;
                    }
                    else
                    {
                        break;
                    }
                }
                else
                {
                    break;
                }
            }
            
            int verticalLinkAmount = 1 + downLinkAmount + upLinkAmount;
            if (verticalLinkAmount >= 3) 
            {
                int downMostRowIndex = rowIndex - downLinkAmount;
                int alreadyMatchedCellCount = 0;
                for (int i = 0; i < verticalLinkAmount; i++) {
                    if (_alreadyMatchedCellList[columnIndex, downMostRowIndex + i])
                    {
                        alreadyMatchedCellCount++;
                    }
                    matchedCellModels.Add(cellModels[columnIndex, downMostRowIndex + i]);
                    _alreadyMatchedCellList[columnIndex, downMostRowIndex + i] = true;
                }
                if (alreadyMatchedCellCount < verticalLinkAmount)
                {
                    return matchedCellModels;
                }
            }

            return null;
        }
        
        private bool IsValidPosition(int columnIndex, int rowIndex) {
            if (columnIndex < 0 || rowIndex < 0 || columnIndex >= _columnCount || rowIndex >= _rowCount) {
                return false;
            } 
            return true;
        }
    }
}