using System;
using System.Collections.Generic;

using TaskTracker.ExceptionUtils;

namespace TaskTracker.Presentation.WPF.Utils
{
    internal static class EnumUtils
    {
        public static IEnumerable<T> GetValues<T>()
        {
            foreach (var item in Enum.GetValues(typeof(T)))
            {
                yield return (T)item;
            }
        }       
    }

    internal static class ConversionUtils
    {
        public static Double? SafeParseDouble(string str)
        {
            double tmp;
            return Double.TryParse(str, out tmp) ? tmp : (double?)null;
        }
    }
	
	internal delegate void UpdateEventHandler();

    internal interface IUpdateManager
    {
        void BeginUpdate();
        void EndUpdate();
        void CancelUpdate();
        void RequestUpdate();
        void ForceUpdate();

        event UpdateEventHandler OnUpdate;
    }

    internal class UpdateManager : IUpdateManager
    {
        private int counter;

        private bool isUpdateRequested;

        private void DoUpdate()
        {
            isUpdateRequested = false;
            InternalUpdate();
            OnUpdate?.Invoke();
        }

        protected virtual void InternalUpdate()
        {
            // empty
        }

        public UpdateManager(UpdateEventHandler onUpdate = null)
        {
            counter = 0;
            isUpdateRequested = false;

            if (onUpdate != null)
                OnUpdate += onUpdate;
        }

        public void BeginUpdate()
        {
            counter++;
        }

        public void EndUpdate()
        {
            counter--;
            if (counter == 0)
            {
                DoUpdate();
            }
        }

        public void CancelUpdate()
        {
            counter--;
            if (isUpdateRequested)
            {
                DoUpdate();
            }
        }

        public void RequestUpdate()
        {
            isUpdateRequested = counter > 0;
            if (!isUpdateRequested)
            {
                DoUpdate();
            }
        }

        public void ForceUpdate()
        {
            DoUpdate();
        }        

        public event UpdateEventHandler OnUpdate;
    }
}
