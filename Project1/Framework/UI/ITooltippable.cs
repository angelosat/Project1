using System.Collections.Generic;

namespace Project1.Framework.UI;

public interface ITooltippable
{
    void GetTooltipInfo(Control tooltip);
    IEnumerable<Control> GetTooltipControls();
    string LabelReadable { get; }
}
