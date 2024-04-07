using System;
using System.Collections.Generic;

namespace Board
{
    public class FillingDropItemDeterminer
    {
        private int _columnCount;
        private int _rowCount;
        private Func<int, int, CellModel> _getCellModel;

        public FillingDropItemDeterminer(int columnCount, int rowCount, Func<int, int, CellModel> getCellModel)
        {
            _columnCount = columnCount;
            _rowCount = rowCount;
            _getCellModel = getCellModel;
        }

        public Dictionary<CellModel, int> GetTargetRowIndexOfFillingDropItems(out int[] emptyCellCountInEachColumn)
        {
            Dictionary<CellModel, int> targetRowIndexOfFillingDropItemsDict = new Dictionary<CellModel, int>();
            emptyCellCountInEachColumn = new int[_columnCount];
            for (int i = 0; i < _columnCount; i++)
            {
                for (int j = 0; j < _rowCount; j++)
                {
                    CellModel cellModel = _getCellModel(i,j);
                    if (!cellModel.hasAssignedDropItem) emptyCellCountInEachColumn[i]++;
                    else
                    {
                        int count = 0;
                        for (int k = j - 1; k >= 0; k--)
                        {
                            if (!_getCellModel(i, k).hasAssignedDropItem) 
                            {
                                count++;
                            }
                        }

                        if (count > 0)
                        {
                            targetRowIndexOfFillingDropItemsDict.Add(cellModel, cellModel.rowIndex - count);
                        }
                    }
                }
            }

            return targetRowIndexOfFillingDropItemsDict;
        }
    }
}