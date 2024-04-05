using UnityEngine;

namespace Board
{
    public class DropItemView : IDropItemView
    {
        private Transform _transform;
        private DropItemModel _dropItemModel;

        public DropItemView(Transform transform, DropItemModel dropItemModel)
        {
            _transform = transform;
            _dropItemModel = dropItemModel;
        }
    }

    public interface IDropItemView
    {
        
    }
}