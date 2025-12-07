using Start_a_Town_.Components;
using Start_a_Town_.Towns.Constructions;
using Start_a_Town_.UI;
using System;

namespace Start_a_Town_
{
    class NeedsMoodsUI : GuiBuilder
    {
        GroupBox BoxNeeds, BoxMood;
        public NeedsMoodsUI()
        {
            
        }
        public NeedsMoodsUI(Entity entity) : base(entity)
        {
        }

        protected override void Build()
        {
            this.BoxNeeds = new GroupBox();
            this.BoxMood = new GroupBox() { Location = this.BoxNeeds.TopRight };
            this.Name = "Needs";
            var actor = this.Entity as Actor;
            var needs = actor.GetComponent<NeedsComponent>();
            var mood = actor.GetComponent<MoodComp>();

            this.BoxNeeds = new GroupBox();
            needs.GetUI(actor, this.BoxNeeds);

            this.BoxMood = new GroupBox() { Location = this.BoxNeeds.TopRight };
            mood.GetInterface(actor, this.BoxMood);

            this.Controls.Add(this.BoxNeeds, this.BoxMood);
        }
      
        public void Refresh(GameObject entity)
        {
            var needs = entity.GetComponent<NeedsComponent>();// as IGui<NeedsComponent>;
            var mood = entity.GetComponent<MoodComp>();

            this.Controls.Clear();

            this.BoxNeeds.ClearControls();
            this.BoxMood.ClearControls();

            //needs.GetUI(entity, this.BoxNeeds);
            //mood.GetInterface(entity, this.BoxMood);

            this.BoxNeeds.AddControls(needs.NewGui());
            this.BoxMood.AddControls(mood.NewGui());

            this.BoxMood.Location = this.BoxNeeds.TopRight;

            this.Controls.Add(this.BoxNeeds, this.BoxMood);

            this.Tag = entity;

            this.GetWindow()?.Title = $"{entity.Name} needs";

            this.Validate(true);
        }
        protected override GuiBuilder BuildFor(Entity entity) => new NeedsMoodsUI(entity);
        //internal override void OnSelectedTargetChanged(TargetArgs target)
        //{
        //    var actor = target.Object as Actor;
        //    if (actor is null)
        //        return;
        //    var newgui = new NeedsMoodsUI(actor);
        //    var win = newgui.RefreshSingleton();
        //    win.SetTitle(actor.Name);
        //    win.Validate(true);
        //    //return;
        //    //Refresh(target.Object as Actor);
        //}
        public Control Refresh(Actor actor)
        {
            this.Refresh(actor as GameObject);
            return this;
        }
    }
    //class NeedsMoodsUI : GroupBox
    //{
    //    GroupBox BoxNeeds, BoxMood;
    //    public NeedsMoodsUI()
    //    {
    //        this.BoxNeeds = new GroupBox();
    //        this.BoxMood = new GroupBox() { Location = this.BoxNeeds.TopRight };
    //        this.Name = "Needs";
    //    }
    //    public NeedsMoodsUI(Actor actor) : this()
    //    {
    //        this.BoxNeeds = new GroupBox();
    //        this.BoxMood = new GroupBox() { Location = this.BoxNeeds.TopRight };
    //        this.Name = "Needs";

    //        var needs = actor.GetComponent<NeedsComponent>();
    //        var mood = actor.GetComponent<MoodComp>();

    //        this.BoxNeeds = new GroupBox();
    //        needs.GetUI(actor, this.BoxNeeds);

    //        this.BoxMood = new GroupBox() { Location = this.BoxNeeds.TopRight };
    //        mood.GetInterface(actor, this.BoxMood);

    //        this.Controls.Add(this.BoxNeeds, this.BoxMood);
    //    }
    //    [Obsolete]
    //    public NeedsMoodsUI(GameObject entity)
    //    {
    //        this.BoxNeeds = new GroupBox();
    //        this.BoxMood = new GroupBox() { Location = this.BoxNeeds.TopRight };
    //        this.Name = "Needs";

    //        var needs = entity.GetComponent<NeedsComponent>();
    //        var mood = entity.GetComponent<MoodComp>();

    //        this.BoxNeeds = new GroupBox();
    //        needs.GetUI(entity, this.BoxNeeds);

    //        this.BoxMood = new GroupBox() { Location = this.BoxNeeds.TopRight };
    //        mood.GetInterface(entity, this.BoxMood);

    //        this.Controls.Add(this.BoxNeeds, this.BoxMood);

    //    }
    //    public void Refresh(GameObject entity)
    //    {
    //        var needs = entity.GetComponent<NeedsComponent>();// as IGui<NeedsComponent>;
    //        var mood = entity.GetComponent<MoodComp>();

    //        this.Controls.Clear();

    //        this.BoxNeeds.ClearControls();
    //        this.BoxMood.ClearControls();

    //        //needs.GetUI(entity, this.BoxNeeds);
    //        //mood.GetInterface(entity, this.BoxMood);

    //        this.BoxNeeds.AddControls(needs.NewGui());
    //        this.BoxMood.AddControls(mood.NewGui());

    //        this.BoxMood.Location = this.BoxNeeds.TopRight;

    //        this.Controls.Add(this.BoxNeeds, this.BoxMood);

    //        this.Tag = entity;

    //        this.GetWindow()?.Title = $"{entity.Name} needs";

    //        this.Validate(true);
    //    }
    //    static NeedsMoodsUI Instance;
    //    internal static Window GetGui(Actor actor)
    //    {
    //        Window window;

    //        if (Instance == null)
    //        {
    //            Instance = new NeedsMoodsUI();
    //            window = new Window(Instance) { Movable = true, Closable = true };
    //        }
    //        else
    //            window = Instance.GetWindow();
    //        Instance.Tag = actor;
    //        window.Title = $"{actor.Name} needs";
    //        Instance.Refresh(actor);
    //        return window;
    //    }
    //    internal override void OnSelectedTargetChanged(TargetArgs target)
    //    {
    //        Refresh(target.Object as Actor);
    //    }
    //    public Control Refresh(Actor actor)
    //    {
    //        this.Refresh(actor as GameObject);
    //        return this;
    //    }
    //}
}
