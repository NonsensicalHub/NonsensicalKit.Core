using NonsensicalKit.Core.Service;

namespace NonsensicalKit.Core.DagLogicNode
{
    public class DagReturnPreviousLevel : NonsensicalMono
    {
        private DagLogicManager _manager;

        public void Switch()
        {
            if (_manager == null)
            {
                _manager = ServiceCore.Get<DagLogicManager>();
            }

            _manager?.ReturnPreviousLevel();
        }
    }
}
