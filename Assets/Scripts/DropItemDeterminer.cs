using System;
using System.Collections.Generic;
using System.Linq;

namespace Board
{
    public class DropItemDeterminer : IDropItemDeterminer
    {
        private DropItemType[,] _dropItemTypeList;
        private Random _rand = new Random();
        
        public DropItemType[,] GetInitialDropItemTypes(int columnCount, int rowCount)
        {
            _dropItemTypeList = new DropItemType[columnCount, rowCount];
            for (int i = 0; i < columnCount; i++)
            {
                for (int j = 0; j < rowCount; j++)
                {
                    SetRandomRandomItemType(i, j);
                }
            }

            return _dropItemTypeList;
        }

        private void SetRandomRandomItemType(int columnIndex, int rowIndex)
        {
            List<DropItemType> dropItemTypes = Enum.GetValues(typeof(DropItemType))
                .Cast<DropItemType>()
                .ToList();
            
            if (columnIndex >= 2)
            {
                if (_dropItemTypeList[columnIndex - 1, rowIndex] == _dropItemTypeList[columnIndex - 2, rowIndex])
                {
                    dropItemTypes.Remove(_dropItemTypeList[columnIndex - 1, rowIndex]);
                }
            }

            if (rowIndex >= 2)
            {
                if (_dropItemTypeList[columnIndex, rowIndex - 1] == _dropItemTypeList[columnIndex, rowIndex - 2])
                {
                    dropItemTypes.Remove(_dropItemTypeList[columnIndex, rowIndex - 1]);
                }
            }

            int randomIndex = _rand.Next(0, dropItemTypes.Count);
            _dropItemTypeList[columnIndex, rowIndex] = dropItemTypes[randomIndex];
        }
    }

    public interface IDropItemDeterminer
    {
        DropItemType[,] GetInitialDropItemTypes(int columnCount, int rowCount);
    }

    public enum DropItemType
    {
        Red,
        Yellow,
        Blue,
        Green
    }

}