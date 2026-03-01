using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

#nullable enable

namespace Project1.Core.Input.Hotkeys
{
    public sealed record ActionGroup
    {
        readonly string Name;
        readonly ActionGroup? Parent;
        public ActionGroup(string name, ActionGroup? parent = null, int sortOrder = 0)
        {
            // Guard: prevent more than two levels
            if (parent != null && parent.Parent != null)
                throw new InvalidOperationException(
                    $"{nameof(ActionGroup)} '{name}' cannot have a parent with a parent — max 2 levels allowed."
                );

            this.Name = name;
            this.Parent = parent;
            //SortOrder = sortOrder;
        }
        public string Prefix
        {
            get
            {
                if (this.Parent is not null)
                    return $"{this.Parent.Prefix}.{this.Name.ToLowerInvariant()}";
                return this.Name.ToLowerInvariant();
            }
        }
    }
    public record struct ActionId(string Label, string Prefix, ActionGroup Group) { }
    public static class DefaultGroups
    {
        public static readonly ActionGroup General = new("General");
        public static readonly ActionGroup Tool = new("Tool");                     // high-level group
        public static readonly ActionGroup Designation = new("Designation", Tool); // nested under Tool
        public static readonly ActionGroup Movement = new("Movement", Tool);
    }
    internal class HotkeyManagerNew
    {
    }
}
