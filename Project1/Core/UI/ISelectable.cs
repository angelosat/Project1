using Microsoft.Xna.Framework;
using Project1.Core.Simulation;
using Project1.Framework.UI;
using System;
using System.Collections.Generic;

namespace Project1.Core.UI
{
    public interface ISelectable
    {
        string Name { get; }
        IEnumerable<Control> GetSelectionInfo();
        IEnumerable<(string label, Type type)> GetSelectionTabs();
        IEnumerable<Control> GetSelectionDetails();
        bool Exists { get; }
        Vector3 Global { get; }
        MapBase Map { get; }
        IEnumerable<(string Label, Type GuiType)> GetTabs() { yield break; }
    }
}
