using UnityEngine;

namespace Board
{
    public class CellModel
    {
        public DropItemType dropItemType { get; set; }
        public int columnIndex { get; set; }
        public int rowIndex { get; set; }
        public Vector2 position { get; set; }
        public Vector2 localScale { get; set; }
        public bool hasDropItem { get; set; }
    }
}