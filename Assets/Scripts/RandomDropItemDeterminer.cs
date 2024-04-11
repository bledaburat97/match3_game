using System;
using System.Collections.Generic;
using System.Linq;

namespace Board
{
    public class RandomDropItemDeterminer : IRandomDropItemDeterminer
    {
        private DropItemType[,] _initialDropItemTypeList;
        private Random _rand = new Random();
        
        public DropItemType[,] GetInitialDropItemTypes(int columnCount, int rowCount)
        {
            _initialDropItemTypeList = new DropItemType[columnCount, rowCount];
            for (int i = 0; i < columnCount; i++)
            {
                for (int j = 0; j < rowCount; j++)
                {
                    SetNonMatchedRandomDropItemType(i, j);
                }
            }

            return _initialDropItemTypeList;
        }
        
        private void SetNonMatchedRandomDropItemType(int columnIndex, int rowIndex)
        {
            List<DropItemType> possibleDropItemTypes = Enum.GetValues(typeof(DropItemType))
                .Cast<DropItemType>()
                .ToList();
            
            if (columnIndex >= 2)
            {
                if (_initialDropItemTypeList[columnIndex - 1, rowIndex] == _initialDropItemTypeList[columnIndex - 2, rowIndex])
                {
                    possibleDropItemTypes.Remove(_initialDropItemTypeList[columnIndex - 1, rowIndex]);
                }
            }

            if (rowIndex >= 2)
            {
                if (_initialDropItemTypeList[columnIndex, rowIndex - 1] == _initialDropItemTypeList[columnIndex, rowIndex - 2])
                {
                    possibleDropItemTypes.Remove(_initialDropItemTypeList[columnIndex, rowIndex - 1]);
                }
            }

            int randomIndex = _rand.Next(0, possibleDropItemTypes.Count);
            _initialDropItemTypeList[columnIndex, rowIndex] = possibleDropItemTypes[randomIndex];
        }

        public DropItemType GenerateRandomDropItemType()
        {
            List<DropItemType> possibleDropItemTypes = Enum.GetValues(typeof(DropItemType))
                .Cast<DropItemType>()
                .ToList();
            return (DropItemType)_rand.Next(0, possibleDropItemTypes.Count);
        }
    }

    public interface IRandomDropItemDeterminer
    {
        DropItemType[,] GetInitialDropItemTypes(int columnCount, int rowCount);
        DropItemType GenerateRandomDropItemType();
    }

    public enum DropItemType
    {
        Red,
        Yellow,
        Blue,
        Green
    }

}