using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Board
{
    public class ActiveCellModelsManager : IActiveCellModelsManager
    {
        private List<List<CellModel>> _simultaneousCellModelsList;

        public ActiveCellModelsManager()
        {
            _simultaneousCellModelsList = new List<List<CellModel>>();
        }
        
        public void AddSimultaneousCellModelsToList(List<CellModel> newCellModelList)
        {
            foreach (CellModel newCellModel in newCellModelList)
            {
                if (IsCellModelActive(newCellModel.columnIndex, newCellModel.rowIndex,
                        out int simultaneousCellModelListIndex))
                {
                    CellModel cellModel = _simultaneousCellModelsList[simultaneousCellModelListIndex].FirstOrDefault(cellModel =>
                        cellModel.columnIndex == newCellModel.columnIndex
                        && cellModel.rowIndex == newCellModel.rowIndex);
                    _simultaneousCellModelsList[simultaneousCellModelListIndex].Remove(cellModel);
                }
            }

            _simultaneousCellModelsList.Add(newCellModelList);
        }
        
        public bool CheckAllActiveCellModelsCompleted(CellModel checkingCellModel, out int simultaneousCellModelListIndex)
        {
            if (IsCellModelActive(checkingCellModel.columnIndex, checkingCellModel.rowIndex,
                    out simultaneousCellModelListIndex))
            {
                foreach (CellModel cellModel in _simultaneousCellModelsList[simultaneousCellModelListIndex])
                {
                    if (cellModel.hasPlacedDropItem == false) return false;
                }

                return true;
            }

            Debug.LogError("Cell model is not in the _cellModelsMovingSimultaneouslyList");
            return false;
        }

        public List<CellModel> GetSimultaneousCellModels(int simultaneousCellModelListIndex)
        {
            return _simultaneousCellModelsList[simultaneousCellModelListIndex];
        }

        public void RemoveSimultaneousCellModelsAtIndex(int simultaneousCellModelListIndex)
        {
            _simultaneousCellModelsList.RemoveAt(simultaneousCellModelListIndex);
        }

        private bool IsCellModelActive(int columnIndex, int rowIndex, out int simultaneousCellModelListIndex)
        {
            simultaneousCellModelListIndex = -1;
            for (int j = 0; j < _simultaneousCellModelsList.Count; j++)
            {
                if (_simultaneousCellModelsList[j].Any(cellModel =>
                        cellModel.columnIndex == columnIndex
                        && cellModel.rowIndex == rowIndex))
                {
                    simultaneousCellModelListIndex = j;
                    return true;
                }
            }

            return false;
        }
    }

    public interface IActiveCellModelsManager
    {
        void AddSimultaneousCellModelsToList(List<CellModel> newCellModelList);
        bool CheckAllActiveCellModelsCompleted(CellModel checkingCellModel, out int simultaneousCellModelListIndex);
        List<CellModel> GetSimultaneousCellModels(int simultaneousCellModelListIndex);
        void RemoveSimultaneousCellModelsAtIndex(int simultaneousCellModelListIndex);
    }
}