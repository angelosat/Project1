using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace Project1.Framework.UI;

public interface ITooltippable
{
    ITooltippable GetTooltipTarget() => this;
    IEnumerable<Control> GetTooltipControls();
    string LabelReadable { get; }
    Color? TooltipColor => null;
}
