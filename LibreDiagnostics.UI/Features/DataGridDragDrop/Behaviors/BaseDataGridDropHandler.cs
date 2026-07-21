//Author: https://github.com/AvaloniaUI/Avalonia.Xaml.Behaviors/
//Adjustments were made.

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using Avalonia.Xaml.Interactions.DragAndDrop;
using BlackSharp.MVVM.ComponentModel;
using System.Collections;

namespace LibreDiagnostics.UI.Features.DataGridDragDrop.Behaviors;

public abstract class BaseDataGridDropHandler<T> : DropHandlerBase
    where T : ViewModelBase
{
    #region Fields

    const string _RowDraggingUpStyleClass   = "DraggingUp";
    const string _RowDraggingDownStyleClass = "DraggingDown";

    const string _Up   = "up";
    const string _Down = "down";

    readonly DataFormat<string> _DirectionFormat = DataFormat.CreateStringApplicationFormat("direction");

    #endregion

    #region Public

    public override bool Validate(object sender, DragEventArgs e, object sourceContext, object targetContext, object state)
    {
        if (e.Source is Control c && sender is DataGrid dg)
        {
            bool valid = Validate(dg, e, sourceContext, targetContext, false);
            if (valid)
            {
                var row = FindDataGridRowFromChildView(c);
                string direction;

                if (e.DataTransfer.Contains(_DirectionFormat))
                {
                    direction = e.DataTransfer.TryGetValue(_DirectionFormat);
                }
                else
                {
                    direction = _Down;
                }

                ApplyDraggingStyleToRow(row!, direction);
                ClearDraggingStyleFromAllRows(sender, exceptThis: row);
            }

            return valid;
        }

        ClearDraggingStyleFromAllRows(sender);

        return false;
    }

    public override bool Execute(object sender, DragEventArgs e, object sourceContext, object targetContext, object state)
    {
        ClearDraggingStyleFromAllRows(sender);

        if (e.Source is Control && sender is DataGrid dg)
        {
            return Validate(dg, e, sourceContext, targetContext, true);
        }

        return false;
    }

    public override void Cancel(object sender, RoutedEventArgs e)
    {
        base.Cancel(sender, e);

        //This is necessary to clear adorner borders when mouse leaves DataGrid
        //they would remain even after changing screens
        ClearDraggingStyleFromAllRows(sender);
    }

    #endregion

    #region Protected

    protected abstract T MakeCopy(IList parentCollection, T item);
    protected abstract T MakeCopy(IList<T> parentCollection, T item);

    protected abstract bool Validate(DataGrid dg, DragEventArgs e, object sourceContext, object targetContext, bool bExecute);

    protected bool RunDropAction(DataGrid dg, DragEventArgs e, bool bExecute, T sourceItem, T targetItem, IList items)
    {
        int sourceIndex = items.IndexOf(sourceItem);
        int targetIndex = items.IndexOf(targetItem);

        if (sourceIndex < 0 || targetIndex < 0)
        {
            return false;
        }

        switch (e.DragEffects)
        {
            case DragDropEffects.Copy:
                if (bExecute)
                {
                    var clone = MakeCopy(items, sourceItem);
                    InsertItem(items, clone, targetIndex + 1);
                    dg.SelectedIndex = targetIndex + 1;
                }

                return true;
            case DragDropEffects.Move:
                if (bExecute)
                {
                    MoveItem(items, sourceIndex, targetIndex);
                    dg.SelectedIndex = targetIndex;
                }

                return true;
            case DragDropEffects.Link:
                if (bExecute)
                {
                    SwapItem(items, sourceIndex, targetIndex);
                    dg.SelectedIndex = targetIndex;
                }
                return true;
            default:
                return false;
        }
    }

    protected void InsertItem(IList items, T item, int index)
    {
        items.Insert(index, item);
    }

    protected void MoveItem(IList items, int sourceIndex, int targetIndex)
    {
        if (sourceIndex < targetIndex)
        {
            var item = items[sourceIndex];
            items.RemoveAt(sourceIndex);
            items.Insert(targetIndex, item);
        }
        else
        {
            var removeIndex = sourceIndex + 1;
            if (items.Count + 1 > removeIndex)
            {
                var item = items[sourceIndex];
                items.RemoveAt(removeIndex - 1);
                items.Insert(targetIndex, item);
            }
        }
    }

    protected void SwapItem(IList items, int sourceIndex, int targetIndex)
    {
        var item1 = items[sourceIndex];
        var item2 = items[targetIndex];
        items[targetIndex] = item1;
        items[sourceIndex] = item2;
    }

    #endregion

    #region Private

    static DataGridRow FindDataGridRowFromChildView(StyledElement sourceChild)
    {
        int maxDepth = 16;

        DataGridRow row = null;
        StyledElement current = sourceChild;

        while (maxDepth-- > 0 || row is null)
        {
            if (current is DataGridRow dgr)
            {
                row = dgr;
            }

            current = current?.Parent;
        }

        return row;
    }

    static DataGridRowsPresenter GetRowsPresenter(Visual v)
    {
        foreach (var cv in v.GetVisualChildren())
        {
            if (cv is DataGridRowsPresenter dgrp)
            {
                return dgrp;
            }
            else if (GetRowsPresenter(cv) is DataGridRowsPresenter dgrp2)
            {
                return dgrp2;
            }
        }

        return null;
    }

    static void ClearDraggingStyleFromAllRows(object sender, DataGridRow exceptThis = null)
    {
        if (sender is DataGrid dg)
        {
            var presenter = GetRowsPresenter(dg);
            if (presenter is null)
            {
                return;
            }

            foreach (var r in presenter.Children)
            {
                if (r == exceptThis)
                {
                    continue;
                }

                if (r!.Classes.Contains(_RowDraggingUpStyleClass))
                {
                    r?.Classes?.Remove(_RowDraggingUpStyleClass);
                }

                if (r!.Classes.Contains(_RowDraggingDownStyleClass))
                {
                    r?.Classes?.Remove(_RowDraggingDownStyleClass);
                }
            }
        }
    }

    static void ApplyDraggingStyleToRow(DataGridRow row, string direction)
    {
        if (direction == _Up)
        {
            if (row.Classes.Contains(_RowDraggingDownStyleClass) == true)
            {
                row.Classes.Remove(_RowDraggingDownStyleClass);
            }
            if (row.Classes.Contains(_RowDraggingUpStyleClass) == false)
            {
                row.Classes.Add(_RowDraggingUpStyleClass);
            }
        }
        else if (direction == _Down)
        {
            if (row.Classes.Contains(_RowDraggingUpStyleClass) == true)
            {
                row.Classes.Remove(_RowDraggingUpStyleClass);
            }
            if (row.Classes.Contains(_RowDraggingDownStyleClass) == false)
            {
                row.Classes.Add(_RowDraggingDownStyleClass);
            }
        }
    }

    #endregion
}

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
