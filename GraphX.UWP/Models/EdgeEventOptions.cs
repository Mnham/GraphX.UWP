using GraphX.Common.Enums;

namespace GraphX.Controls.Models
{
    public sealed class EdgeEventOptions
    {
        private EdgeControl _ec;

        private bool _mouseclick = true;

        private bool _mousedblclick = true;

        private bool _mouseenter = true;

        private bool _mouseleave = true;

        private bool _mousemove = true;

        /// <summary>
        /// Gets or sets if MouseDown event should be enabled
        /// </summary>
        public bool MouseClickEnabled
        {
            get => _mouseclick;
            set
            {
                if (_mouseclick != value)
                {
                    _mouseclick = value;
                    _ec.UpdateEventhandling(EventType.MouseClick);
                }
            }
        }

        /// <summary>
        /// Gets or sets if MouseDoubleClick event should be enabled
        /// </summary>
        public bool MouseDoubleClickEnabled
        {
            get => _mousedblclick;
            set
            {
                if (_mousedblclick != value)
                {
                    _mousedblclick = value;
                    _ec.UpdateEventhandling(EventType.MouseDoubleClick);
                }
            }
        }

        /// <summary>
        /// Gets or sets if MouseEnter event should be enabled
        /// </summary>
        public bool MouseEnterEnabled
        {
            get => _mouseenter;
            set
            {
                if (_mouseenter != value)
                {
                    _mouseenter = value;
                    _ec.UpdateEventhandling(EventType.MouseEnter);
                }
            }
        }

        /// <summary>
        /// Gets or sets if MouseLeave event should be enabled
        /// </summary>
        public bool MouseLeaveEnabled
        {
            get => _mouseleave;
            set
            {
                if (_mouseleave != value)
                {
                    _mouseleave = value;
                    _ec.UpdateEventhandling(EventType.MouseLeave);
                }
            }
        }

        /// <summary>
        /// Gets or sets if MouseMove event should be enabled
        /// </summary>
        public bool MouseMoveEnabled
        {
            get => _mousemove;
            set
            {
                if (_mousemove != value)
                {
                    _mousemove = value;
                    _ec.UpdateEventhandling(EventType.MouseMove);
                }
            }
        }

        public EdgeEventOptions(EdgeControl ec)
        {
            _ec = ec;
        }

        public void Clean()
        {
            _ec = null;
        }
    }
}