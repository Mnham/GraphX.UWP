using GraphX.Common.Interfaces;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace GraphX.Logic.Algorithms.LayoutAlgorithms
{
    public abstract class LayoutParametersBase : ILayoutParameters
    {
        protected LayoutParametersBase()
        {
            Seed = Guid.NewGuid().GetHashCode();
        }

        #region ICloneable Members

        public object Clone()
        {
            return MemberwiseClone();
        }

        public int Seed { get; set; }

        #endregion ICloneable Members

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T field, T newValue, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, newValue))
            {
                return false;
            }

            field = newValue;
            OnPropertyChanged(propertyName);

            return true;
        }

        #endregion INotifyPropertyChanged Members
    }
}