using System;
using UnityEngine;

namespace Board
{
    public class CellModel
    {
        public DropItemType DropItemType { get; set; }
        public bool HasPlacedDropItem { get; set; }
        public int ColumnIndex => _columnIndex;
        public int RowIndex => _rowIndex;
        public Vector2 Position => _position;

        private int _columnIndex;
        private int _rowIndex;
        private Vector2 _position;
        private Vector2 _localScale;
        private Action<int, int> _onMoveCompleted;
        private IDropItemView _dropItem;

        //CellModel is the instance of one cell on the board.
        public CellModel(int columnIndex, int rowIndex, Vector2 position, Vector2 localScale, Action<int,int> onMoveCompleted)
        {
            _columnIndex = columnIndex;
            _rowIndex = rowIndex;
            _position = position;
            _localScale = localScale;
            _onMoveCompleted = onMoveCompleted;
        }
        
        public bool HasAssignedDropItem()
        {
            return _dropItem != null;
        }

        public void SetDropItem(IDropItemView dropItem)
        {
            _dropItem = dropItem;
            _dropItem.SetLocalScale(_localScale);
            _dropItem.SetOnMoveCompletedAction(() => _onMoveCompleted(_columnIndex, _rowIndex));
        }

        public void RemoveDropItem()
        {
            _dropItem = null;
        }

        public IDropItemView GetDropItem()
        {
            return _dropItem;
        }
    }
}