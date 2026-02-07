using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project1.Core.UI;
using Project1.Core.Net;
using Project1.Core.Base;
using Project1.Core.UI;

namespace Project1.Core.Screens
{
    class MainScreen : GameScreen
    {
        static MainScreen _Instance;
        public static MainScreen Instance => _Instance ??= new MainScreen();
        MainMenuWindow MainMenuWindow;
        static Texture2D Background;

        public static void LoadContent()
        {
            Background = Game1.Instance.Content.Load<Texture2D>("Graphics/bg");
        }

        public override GameScreen Initialize(NetEndpoint net)
        {
            base.Initialize(net);
            WindowManager.Initialize();
            this.MainMenuWindow = new MainMenuWindow();
            this.MainMenuWindow.Show();
            new Label($"{GlobalVars.Version}").Show();
            return this;
        }
        MainScreen()
        {
            WindowManager = new UIManager();
            KeyHandlers.Push(WindowManager);
        }

        public override void Update(Game1 game, GameTime gt)
        {
            WindowManager.Update(game, gt);
            base.Update(game, gt);
        }

        public override void Draw(SpriteBatch sb)
        {
            sb.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
            sb.Draw(Background, new Rectangle(0, 0, Game1.Instance.graphics.GraphicsDevice.Viewport.Width, Game1.Instance.graphics.GraphicsDevice.Viewport.Height), Color.White);
            sb.End();
            WindowManager.Draw(sb, null);
        }
    }
}
