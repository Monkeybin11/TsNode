﻿using System.Windows;
using System.Windows.Input;
using TsNode.Interface;

namespace TsNode.Controls.Drag
{
    public class ViewportDrag : IDragController
    {
        public void Cancel()
        {

        }

        public bool CanDragStart(object sender, MouseEventArgs args)
        {
            return args.MiddleButton == MouseButtonState.Pressed;
        }

        public void DragEnd(object sender, MouseEventArgs args)
        {

        }

        public void OnDrag(object sender, MouseEventArgs args)
        {
            if (args.MiddleButton != MouseButtonState.Pressed)
                return;

            var offset = args.GetPosition(NetworkView) - MouseStart;

            NetworkView.Translate(offset.X, offset.Y);
            NetworkView.GridUpdate();

            MouseStart = args.GetPosition(NetworkView);
        }

        private Point MouseStart { get; set; }
        private NetworkView NetworkView { get; }
        private FrameworkElement _inputElement;
        public ViewportDrag(NetworkView networkView , MouseEventArgs mouseEventArgs)
        {
            MouseStart = mouseEventArgs.GetPosition(networkView);
            NetworkView = networkView;
            _inputElement = networkView;
        }
    }
}
