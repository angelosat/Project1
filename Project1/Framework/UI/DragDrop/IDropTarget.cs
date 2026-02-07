using System;

namespace Project1.Core
{
    interface IDropTarget
    {
        event EventHandler<DragEventArgs> DragDrop;
        Func<DragEventArgs, DragDropEffects> DragDropAction { get; set; }
        DragDropEffects Drop(DragEventArgs args);
    }
}
