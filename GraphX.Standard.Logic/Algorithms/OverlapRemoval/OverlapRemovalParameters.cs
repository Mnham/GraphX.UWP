using GraphX.Common.Interfaces;

using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace GraphX.Logic.Algorithms.OverlapRemoval
{
    public class OverlapRemovalParameters : IOverlapRemovalParameters
    {
        private float _verticalGap = 10;
        private float _horizontalGap = 10;

        /// <summary>
        /// Gets or sets minimal vertical distance between vertices
        /// </summary>
		public float VerticalGap
        {
            get => _verticalGap;
            set => SetProperty(ref _verticalGap, value);
        }

        /// <summary>
        /// Gets or sets minimal horizontal distance between vertices
        /// </summary>
		public float HorizontalGap
        {
            get => _horizontalGap;
            set => SetProperty(ref _horizontalGap, value);
        }

        public object Clone()
        {
            return MemberwiseClone();
        }

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
    }
}