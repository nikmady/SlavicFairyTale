using Game.Runtime.Camera;

namespace Game.View.Camera
{
    public class CameraViewBinder
    {
        private readonly CameraFollowRuntime _runtime;
        private readonly CameraView _view;

        public CameraViewBinder(CameraFollowRuntime runtime, CameraView view)
        {
            _runtime = runtime;
            _view = view;
        }

        public void Tick()
        {
            if (_runtime != null && _view != null)
                _view.SetPosition(_runtime.CurrentPosition);
        }
    }
}
