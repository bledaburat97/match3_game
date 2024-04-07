using System.Collections.Generic;

namespace Board
{
    public class MatchManager
    {
        private int _columnCount = 8;
        private int _rowCount = 8;

        public List<CellModel> CheckHorizontalMatch(int columnIndex, int rowIndex, CellModel[,] cellModels, out List<CellModel> intersectedCellModels)
        {
            List<CellModel> matchedCellModels = new List<CellModel>();
            CellModel swappedCellModel = cellModels[columnIndex, rowIndex];
            DropItemType dropItemType = swappedCellModel.dropItemType;
            intersectedCellModels = new List<CellModel>();
            int rightLinkAmount = 0;
            int leftLinkAmount = 0;

            for (int i = 1; i < _columnCount; i++)
            {
                if (IsValidPosition(columnIndex - i, rowIndex))
                {
                    if (cellModels[columnIndex - i, rowIndex].hasPlacedDropItem &&
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
                    if (cellModels[columnIndex + i, rowIndex].hasPlacedDropItem &&
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
                for (int i = 0; i < horizontalLinkAmount; i++) {
                    matchedCellModels.Add(cellModels[leftMostColumnIndex + i, rowIndex]);
                }
                
                return matchedCellModels;
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
                    if (cellModels[columnIndex, rowIndex - i].hasPlacedDropItem &&
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
                    if (cellModels[columnIndex, rowIndex + i].hasPlacedDropItem &&
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
                for (int i = 0; i < verticalLinkAmount; i++) {
                    matchedCellModels.Add(cellModels[columnIndex, downMostRowIndex + i]);
                }

                return matchedCellModels;
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