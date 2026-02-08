using Project1.Core.Base;
using Project1.Core.Helpers;
using Project1.Core.Rendering;
using Project1.Framework.UI;

namespace Project1.Core.UI.Settings
{
    class CameraSettings : GameSettings
    {
        GroupBox _Gui;
        internal override GroupBox Gui => this._Gui ??= this.CreateGui();
        bool _tmpFog, _tmpSmooth;
        internal override string Name => "Camera";

        GroupBox CreateGui()
        {
            var box = new GroupBox();
            box.Name = "Camera";
            _tmpFog = Camera.Fog;
            _tmpSmooth = Camera.SmoothCentering;
            var fog = new CheckBoxNew("Fog", () => _tmpFog = !_tmpFog, () => _tmpFog);
            var smooth = new CheckBoxNew("Smooth Centering", () => _tmpSmooth = !_tmpSmooth, () => _tmpSmooth);

            box.AddControlsVertically(fog, smooth);
            return box;
        }

        internal override void Apply()
        {
            Camera.Fog = _tmpFog;
            Camera.SmoothCentering = _tmpSmooth;
            Engine.Config.GetOrCreateElement("Settings").GetOrCreateElement("Camera").GetOrCreateElement(nameof(Camera.Fog)).Value = _tmpFog.ToString();
            Engine.Config.GetOrCreateElement("Settings").GetOrCreateElement("Camera").GetOrCreateElement(nameof(Camera.SmoothCentering)).Value = _tmpSmooth.ToString();
        }
        internal override void Cancel()
        {
        }
    }
}
