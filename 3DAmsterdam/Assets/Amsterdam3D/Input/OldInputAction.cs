using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amsterdam3D.InputHandler
{
    /// <summary>
    /// Action class for old input system by unity. Currently not implemented as it is not needed
    /// </summary>
    public class OldInputAction : IAction
    {
        public bool Used { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public string name => throw new NotImplementedException();

        public bool Started => throw new NotImplementedException();

        public bool Performed => throw new NotImplementedException();

        public bool Cancelled => throw new NotImplementedException();

        public void Disable()
        {
            throw new NotImplementedException();
        }

        public T ReadValue<T>() where T : struct
        {
            throw new NotImplementedException();
        }

        public void SetValue(dynamic value)
        {
            throw new NotImplementedException();
        }
        public void SubscribeStarted(UnityInputSystemAction.ActionDelegate del, int priority)
        {
            throw new NotImplementedException();
        }
        public void SubscribeCancelled(UnityInputSystemAction.ActionDelegate del, int priority)
        {
            throw new NotImplementedException();
        }
        public void SubscribePerformed(UnityInputSystemAction.ActionDelegate del, int priority)
        {
            throw new NotImplementedException();
        }
	}
}
