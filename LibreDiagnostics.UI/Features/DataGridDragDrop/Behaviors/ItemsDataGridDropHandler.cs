//Author: https://github.com/AvaloniaUI/Avalonia.Xaml.Behaviors/
//Adjustments were made.

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.VisualTree;
using BlackSharp.Core.Interfaces;
using BlackSharp.MVVM.ComponentModel;
using System.Collections;

namespace LibreDiagnostics.UI.Features.DataGridDragDrop.Behaviors
{
    public class ItemsDataGridDropHandlerGen<TItemViewModel> : BaseDataGridDropHandler<TItemViewModel>
        where TItemViewModel : ViewModelBase, ICloneable<TItemViewModel>
    {
        protected override TItemViewModel MakeCopy(IList parentCollection, TItemViewModel item) =>
            item.Clone();

        protected override TItemViewModel MakeCopy(IList<TItemViewModel> parentCollection, TItemViewModel item) =>
            item.Clone();

        protected override bool Validate(DataGrid dg, DragEventArgs e, object sourceContext, object targetContext, bool bExecute)
        {
            if (sourceContext is not TItemViewModel sourceItem
             || targetContext is not ViewModelBase
             || dg.GetVisualAt(e.GetPosition(dg)) is not Control targetControl
             || (dg.GetVisualAt(e.GetPosition(dg)) is DataGrid target && dg != target) //Only allow drag drop on itself
             || targetControl.DataContext is not TItemViewModel targetItem)
            {
                return false;
            }

            //return RunDropAction(dg, e, bExecute, sourceItem, targetItem, items);
            return RunDropAction(dg, e, bExecute, sourceItem, targetItem, dg.ItemsSource as IList);
        }
    }

    public sealed class ItemsDataGridDropHandler : ItemsDataGridDropHandlerGen<DragDropViewModelBase>
    {
    }
}

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
