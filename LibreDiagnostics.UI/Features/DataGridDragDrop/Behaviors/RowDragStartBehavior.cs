//Author: https://github.com/AvaloniaUI/Avalonia.Xaml.Behaviors/
//Adjustments were made.

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia;
using Avalonia.Xaml.Interactivity;

namespace LibreDiagnostics.UI.Features.DataGridDragDrop.Behaviors
{
    /// <summary>
    /// Starts a drag operation from a DataGridRow by click-and-move.
    /// </summary>
    public class RowDragStartBehavior : Behavior<DataGridRow>
    {
        #region Fields

        private bool _Pressed;
        private Point _Start;
        private PointerPressedEventArgs _PressedEvent;

        const string _RowMove = "row-move";

        #endregion

        #region Protected

        protected override void OnAttached()
        {
            base.OnAttached();

            AssociatedObject.AddHandler(InputElement.PointerPressedEvent , OnPointerPressed , RoutingStrategies.Tunnel | RoutingStrategies.Bubble);
            AssociatedObject.AddHandler(InputElement.PointerMovedEvent   , OnPointerMoved   , RoutingStrategies.Tunnel | RoutingStrategies.Bubble);
            AssociatedObject.AddHandler(InputElement.PointerReleasedEvent, OnPointerReleased, RoutingStrategies.Tunnel | RoutingStrategies.Bubble);
        }

        protected override void OnDetaching()
        {
            AssociatedObject.RemoveHandler(InputElement.PointerPressedEvent , OnPointerPressed );
            AssociatedObject.RemoveHandler(InputElement.PointerMovedEvent   , OnPointerMoved   );
            AssociatedObject.RemoveHandler(InputElement.PointerReleasedEvent, OnPointerReleased);

            base.OnDetaching();
        }

        #endregion

        #region Private

        void OnPointerPressed(object sender, PointerPressedEventArgs e)
        {
            if (e.GetCurrentPoint(AssociatedObject).Properties.IsLeftButtonPressed)
            {
                _Pressed = true;
                _Start = e.GetPosition(AssociatedObject);
                _PressedEvent = e;
            }
            else
            {
                _Pressed = false;
                _PressedEvent = null;
            }
        }

        async void OnPointerMoved(object sender, PointerEventArgs e)
        {
            if (!_Pressed || _PressedEvent == null)
            {
                return;
            }

            var pos = e.GetPosition(AssociatedObject);
            if (Math.Abs(pos.X - _Start.X) + Math.Abs(pos.Y - _Start.Y) < 4)
            {
                return; //Small threshold
            }

            _Pressed = false;
            var pressedEvent = _PressedEvent;
            _PressedEvent = null;

            var dragData = new DataTransfer();
            dragData.Add(DataTransferItem.CreateText(_RowMove));

            await DragDrop.DoDragDropAsync(pressedEvent, dragData, DragDropEffects.Move);
        }

        void OnPointerReleased(object sender, PointerReleasedEventArgs e)
        {
            _Pressed = false;
            _PressedEvent = null;
        }

        #endregion
    }
}

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
