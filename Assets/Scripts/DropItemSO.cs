using UnityEngine;

namespace Board
{
    [CreateAssetMenu()]
    public class DropItemSO : ScriptableObject
    {
        public DropItemType dropItemType;
        public Sprite sprite;
    }
}