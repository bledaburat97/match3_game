
using System.Collections.Generic;
using UnityEngine;

namespace Board
{
    public class DropItemPool : MonoBehaviour, IDropItemPool
    {
        [SerializeField] private Transform dropPrefab;
        private Queue<Transform> _pooledObjects= new Queue<Transform>();

        public void CreatePooledObjects(int columnCount, int rowCount)
        {
            for (int i = 0; i < columnCount * rowCount; i++)
            {
                Transform dropTransform = Instantiate(dropPrefab, transform);
                dropTransform.gameObject.SetActive(false);
                _pooledObjects.Enqueue(dropTransform);
            }
        }
        
        public Transform GetObjectFromPool()
        {
            if (_pooledObjects.Count > 0)
            {
                Transform dropTransform = _pooledObjects.Dequeue();
                dropTransform.gameObject.SetActive(true);
                return dropTransform;
            }
            else
            {
                Debug.LogWarning("No objects left in pool.");
                return null;
            }
        }

        public void ReturnObjectToPool(Transform dropTransform)
        {
            dropTransform.gameObject.SetActive(false);
            _pooledObjects.Enqueue(dropTransform);
        }
    }

    public interface IDropItemPool
    {
        void CreatePooledObjects(int columnCount, int rowCount);
        Transform GetObjectFromPool();
        void ReturnObjectToPool(Transform dropTransform);
    }
}


