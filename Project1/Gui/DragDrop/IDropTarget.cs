using System;

namespace Start_a_Town_
{
    interface IDropTarget
    {
        event EventHandler<DragEventArgs> DragDrop;
        Func<DragEventArgs, DragDropEffects> DragDropAction { get; set; }
        DragDropEffects Drop(DragEventArgs args);
    }
}
