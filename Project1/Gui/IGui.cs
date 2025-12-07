using System;
using System.Collections.Generic;

namespace Start_a_Town_.UI
{
    public interface IGui
    {
        void NewGui(GroupBox box);

        public Control NewGui()
        {
            var box = new GroupBox();
            this.NewGui(box);
            return box;
        }
        //Control Refresh(T copm);
    }
    public interface IGuiBuilder<TBuilder>
    where TBuilder : IGuiBuilder<TBuilder>
    {
        static abstract Control Build(Actor actor);
        private static readonly Dictionary<Type, Window> _cachedWindows = [];

        // Default refresh method for singleton inspector
        static Window Refresh(Actor actor)
        {
            var type = typeof(TBuilder);
            if (!_cachedWindows.TryGetValue(type, out var window))
            {
                window = new Window();
                _cachedWindows[type] = window;
            }

            window.Client.ClearControls();
            window.Client.AddControls(TBuilder.Build(actor));
            return window;
        }
    }
    public abstract class GuiBuilder : GroupBox
    {
        private static readonly Dictionary<Type, Window> _singletonWindows = new();
        protected Entity Entity;
        protected abstract void Build();
        public GuiBuilder() { }
        protected GuiBuilder(Entity entity)
        {
            this.Entity = entity;
            this.Build();
        }
        public Window RefreshSingleton()
        {
            var type = GetType();
            if (!_singletonWindows.TryGetValue(type, out var win))
            {
                win = new Window { Movable = true };
                _singletonWindows[type] = win;
            }
            win.Client.ClearControls();
            win.Client.AddControls(this);// BuildFor(entity));
            return win;
        }
       
        protected abstract GuiBuilder BuildFor(Entity entity);
        GuiBuilder SetActor(Entity entity)
        {
            this.Entity = entity;
            this.Build();
            return this;
        }
        internal override void OnSelectedTargetChanged(TargetArgs target)
        {
            var actor = target.Object as Actor;
            if (actor is null)
                return;
            var newgui = this.BuildFor(actor);// new NeedsMoodsUI(actor);
            var win = newgui.RefreshSingleton();
            win.SetTitle(actor.Name);
            win.Validate(true);
        }

        public static T BuildFloating<T>(Actor actor) where T : GuiBuilder, new()
        {
            var builder = new T();
            builder.SetActor(actor);
            return builder;
        }
        public static Window RefreshSingleton<T>(Actor actor) where T : GuiBuilder, new()
        {
            var builder = new T();
            builder.SetActor(actor);
            return builder.RefreshSingleton();
        }
    }
    public static class GuiExtensions
    {
        public static Control NewGui(this IGui gui)
        {
            var box = new GroupBox();
            gui.NewGui(box);
            return box;
        }
    }
}
