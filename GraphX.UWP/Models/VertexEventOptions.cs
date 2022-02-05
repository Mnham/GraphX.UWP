using GraphX.Common.Enums;

namespace GraphX.Controls.Models
{
    public sealed class VertexEventOptions
    {
        private bool _mouseclick = true;

        private bool _mousedblclick = true;

        private bool _mouseenter = true;

        private bool _mouseleave = true;

        private bool _mousemove = true;

        private VertexControl _vc;

        /// <summary>
        /// Gets or sets if MouseDown event should be enabled
        /// </summary>
        public bool MouseClickEnabled
        {
            get => _mouseclick; set
            {
                if (_mouseclick == value)
                {
                    return;
                }
                _mouseclick = value;
                _vc.UpdateEventhandling(EventType.MouseClick);
            }
        }

        /// <summary>
        /// Gets or sets if MouseDoubleClick event should be enabled
        /// </summary>
        public bool MouseDoubleClickEnabled
        {
            get => _mousedblclick; set
            {
                if (_mousedblclick == value)
                {
                    return;
                }
                _mousedblclick = value;
                _vc.UpdateEventhandling(EventType.MouseDoubleClick);
            }
        }

        /// <summary>
        /// Gets or sets if MouseEnter event should be enabled
        /// </summary>
        public bool MouseEnterEnabled
        {
            get => _mouseenter; set
            {
                if (_mouseenter == value)
                {
                    return;
                }
                _mouseenter = value;
                _vc.UpdateEventhandling(EventType.MouseEnter);
            }
        }

        /// <summary>
        /// Gets or sets if MouseLeave event should be enabled
        /// </summary>
        public bool MouseLeaveEnabled
        {
            get => _mouseleave; set
            {
                if (_mouseleave == value)
                {
                    return;
                }
                _mouseleave = value;
                _vc.UpdateEventhandling(EventType.MouseLeave);
            }
        }

        /// <summary>
        /// Gets or sets if MouseMove event should be enabled
        /// </summary>
        public bool MouseMoveEnabled
        {
            get => _mousemove; set
            {
                if (_mousemove == value)
                {
                    return;
                }
                _mousemove = value;
                _vc.UpdateEventhandling(EventType.MouseMove);
            }
        }

        public VertexEventOptions(VertexControl vc)
        {
            _vc = vc;
        }

        public void Clean()
        {
            _vc = null;
        }
    }
}