using System.Collections.Generic;
using UnityEngine;

namespace Board
{
    public class DropItemPool : MonoBehaviour, IDropItemPool
    {
        [SerializeField] private DropItemView dropItemPrefab;
        private Queue<IDropItemView> _pooledDropItems= new Queue<IDropItemView>();

        public void CreateDropItemPool(int columnCount, int rowCount)
        {
            for (int i = 0; i < columnCount * rowCount; i++)
            {
                IDropItemView dropItem = Instantiate(dropItemPrefab, transform);
                dropItem.SetActive(false);
                _pooledDropItems.Enqueue(dropItem);
            }
        }
        
        public IDropItemView GetDropItemFromPool()
        {
            if (_pooledDropItems.Count > 0)
            {
                IDropItemView dropItem = _pooledDropItems.Dequeue();
                dropItem.SetActive(true);
                return dropItem;
            }
            else
            {
                Debug.LogWarning("No objects left in pool.");
                return null;
            }
        }

        public void ReturnDropItemToPool(IDropItemView dropItem)
        {
            dropItem.SetActive(false);
            _pooledDropItems.Enqueue(dropItem);
        }
    }

    public interface IDropItemPool
    {
        void CreateDropItemPool(int columnCount, int rowCount);
        IDropItemView GetDropItemFromPool();
        void ReturnDropItemToPool(IDropItemView dropItem);
    }
}


