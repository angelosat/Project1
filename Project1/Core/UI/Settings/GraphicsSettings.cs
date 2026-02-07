using Project1.Core.Base;
using Project1.Core.Graphics.Particles;
using Project1.Core.Helpers;
using Project1.Core.UI;
using Project1.Core.UI;

namespace Project1.Core.UI.Settings
{
    class GraphicsSettings : GameSettings
    {
        ComboBoxNewNew<ParticleDensityLevel> Combo_Particles;
        bool Changed;
        ParticleDensityLevel TempParticles;
        GroupBox _Gui;
        internal override GroupBox Gui => this._Gui ??= this.CreateGui();
        internal override string Name => "Graphics";

        public GraphicsSettings()
        {
            
        }
        GroupBox CreateGui()
        {
            var box = new GroupBox();
            box.Name = "Graphics";
            this.TempParticles = ParticleDensityLevel.Current;
            this.Combo_Particles = new(ParticleDensityLevel.All, 200, "Particle Density", i => i.Name, () => this.TempParticles, i => { this.TempParticles = i; });
            box.Controls.Add(this.Combo_Particles);
            return box;
        }
        internal override void Apply()
        {
            if (!Changed)
                return;
            this.Changed = false;

            ParticleDensityLevel.Current = this.TempParticles;

            Engine.Config.GetOrCreateElement("Settings").GetOrCreateElement("Graphics").GetOrCreateElement("Particles").Value = ParticleDensityLevel.Current.Name;
        }
        internal override void Cancel()
        {
        }
    }
}
