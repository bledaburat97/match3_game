using System.Collections.Generic;
using System.Linq;

namespace Board
{
    public class ActiveCellModelsManager : IActiveCellModelsManager
    {
        private List<List<CellModel>> _simultaneouslyActiveCellModelsList;

        public ActiveCellModelsManager()
        {
            _simultaneouslyActiveCellModelsList = new List<List<CellModel>>();
        }
        
        //Create and add new active moving cell model list.
        public void CreateNewActiveCellModelList(List<CellModel> newCellModels)
        {
            TryToRemoveFromActiveCellModelsList(newCellModels);
            _simultaneouslyActiveCellModelsList.Add(newCellModels);
        }

        //Add new active moving cell model list to the list which has common cell model. 
        public void AddActiveCellModelsToAlreadyActiveList(List<CellModel> newCellModels, int activeCellModelsListIndex)
        {
            TryToRemoveFromActiveCellModelsList(newCellModels);
            _simultaneouslyActiveCellModelsList[activeCellModelsListIndex].AddRange(newCellModels);
        }

        private void TryToRemoveFromActiveCellModelsList(List<CellModel> newCellModels)
        {
            foreach (CellModel newCellModel in newCellModels)
            {
                if (IsCellModelActive(newCellModel.ColumnIndex, newCellModel.RowIndex,
                        out int simultaneousCellModelListIndex))
                {
                    CellModel cellModel = _simultaneouslyActiveCellModelsList[simultaneousCellModelListIndex].FirstOrDefault(cellModel =>
                        cellModel.ColumnIndex == newCellModel.ColumnIndex
                        && cellModel.RowIndex == newCellModel.RowIndex);
                    _simultaneouslyActiveCellModelsList[simultaneousCellModelListIndex].Remove(cellModel);
                }
            }
        }
        
        //When the item's move completed, its cell model's hasPlacedDropItem parameter becomes true,
        //thus we can check that parameter to understand that is there an active movement or not.
        public bool CheckAllActiveCellModelsCompleted(CellModel checkingCellModel, out int activeCellModelsListIndex)
        {
            if (IsCellModelActive(checkingCellModel.ColumnIndex, checkingCellModel.RowIndex,
                    out activeCellModelsListIndex))
            {
                foreach (CellModel cellModel in _simultaneouslyActiveCellModelsList[activeCellModelsListIndex])
                {
                    if (cellModel.HasPlacedDropItem == false) return false;
                }

                return true;
            }

            return false;
        }

        public List<List<CellModel>> GetSimultaneouslyActiveCellModelsList()
        {
            return _simultaneouslyActiveCellModelsList;
        }

        public void RemoveActiveCellModelsAtIndex(int activeCellModelsListIndex)
        {
            _simultaneouslyActiveCellModelsList.RemoveAt(activeCellModelsListIndex);
            TryRemoveEmptyLists();
        }

        private void TryRemoveEmptyLists()
        {
            for (int i = _simultaneouslyActiveCellModelsList.Count - 1; i >= 0; i--)
            {
                List<CellModel> cellModelList = _simultaneouslyActiveCellModelsList[i];
                if (cellModelList.Count == 0)
                {
                    _simultaneouslyActiveCellModelsList.RemoveAt(i);
                }
            }
        }

        private bool IsCellModelActive(int columnIndex, int rowIndex, out int activeCellModelsListIndex)
        {
            activeCellModelsListIndex = -1;
            for (int j = 0; j < _simultaneouslyActiveCellModelsList.Count; j++)
            {
                if (_simultaneouslyActiveCellModelsList[j].Any(cellModel =>
                        cellModel.ColumnIndex == columnIndex
                        && cellModel.RowIndex == rowIndex))
                {
                    activeCellModelsListIndex = j;
                    return true;
                }
            }

            return false;
        }
    }

    public interface IActiveCellModelsManager
    {
        void CreateNewActiveCellModelList(List<CellModel> newCellModels);
        void AddActiveCellModelsToAlreadyActiveList(List<CellModel> newCellModels, int activeCellModelsListIndex);
        bool CheckAllActiveCellModelsCompleted(CellModel checkingCellModel, out int simultaneousCellModelListIndex);
        List<List<CellModel>> GetSimultaneouslyActiveCellModelsList();
        void RemoveActiveCellModelsAtIndex(int activeCellModelsListIndex);
    }
}