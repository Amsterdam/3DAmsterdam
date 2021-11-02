using System;
using UnityEngine;

namespace Netherlands3D.Events
{
    public static class ToggleActiveEvent
    {
        public class Args : EventArgs
        {

        }

        private static event EventHandler<Args> OnEvent = delegate { };

        public static void Raise(object sender)
        {
            OnEvent(sender, new Args());
        }

        public static void Subscribe(EventHandler<Args> f)
        {
            OnEvent += f;
        }

        public static void Unsubscribe(EventHandler<Args> f)
        {
            OnEvent -= f;
        }
    }
}
