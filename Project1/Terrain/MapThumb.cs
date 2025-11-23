using System;
using Start_a_Town_.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Start_a_Town_
{
    public class MapThumb : IContextable, ITooltippable, INameplateable, IDisposable
    {
        public MapBase Map { get; set; }
        public Sprite[] Sprites;
        bool Valid = true;
        static public int CurrentZoom = 0;

        public int Size { get { return (int)(this.Map.GetSizeInChunks() * Chunk.Size * Block.Width * (1 / (8f * Math.Pow(2, CurrentZoom)))); } }

        public Vector3 ObjectDimensions { get { return new Vector3(Size, Size / 2, Size / 2); } }

        public void Dispose()
        {
            foreach (var sprite in Sprites)
                if (sprite is not null)
                    sprite.Dispose();
        }

        #region INameplateable
        public string Name
        { get { return Map.GetName().Length > 0 ? Map.GetName() : Map.GetOffset().ToString(); } }
        public Vector3 Global
        { get { return new Vector3(Map.GetOffset(), 0); } }
        public Color GetNameplateColor()
        {
            return Color.White;
        }
        public Rectangle GetScreenBounds(Camera camera)
        {
            Vector2 screenPos = Coords.GetScreenCoords(Global, camera, new Vector3(Size, Size / 2, Size / 2));
            return new Rectangle((int)screenPos.X, (int)screenPos.Y, 1, 1);
        }
        public void OnNameplateCreated(Nameplate plate)
        {
            plate.Controls.Add(new Label()
            {
                Font = UIManager.FontBold,
                Text = this.Name,
                MouseThrough = true,
                LeftClickAction = () =>
                {
                }
            });
        }
        public void DrawNameplate(SpriteBatch sb, Rectangle viewport, Nameplate plate)
        {
            plate.Draw(sb, viewport);
        }
        #endregion

        public void Invalidate()
        {
            this.Valid = false;
        }

        public void Update()
        {
            if (Valid)
                return;

            Valid = true;
            Map.GenerateThumbnails(Map.GetFullPath());
            Map.LoadThumbnails();
        }

        public MapThumb(MapBase map)
        {
            this.Map = map;
            this.Sprites = new Sprite[3];
        }

        public void Draw(SpriteBatch sb, Camera camera)
        {
            Sprite thumb = Sprites[CurrentZoom];
            Vector2 screenLoc;
            Rectangle screenBounds;
            if (thumb is null)
                return;
            Texture2D tex = thumb.Texture;
            screenLoc = Coords.GetScreenCoords(new Vector3(Map.GetOffset(), 0), camera, ObjectDimensions) - new Vector2(tex.Width / 2, tex.Height / 2);
            screenBounds = new Rectangle((int)screenLoc.X, (int)screenLoc.Y, tex.Width, tex.Height);
            sb.Draw(tex, screenLoc, null, Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, new Vector3(Map.GetOffset(), 0).GetDrawDepth(this.Map, camera));
            if (thumb.HitTest(1, screenBounds, UIManager.MouseRect))
            {
                Controller.Instance.MouseoverNext.Object = this;
                sb.Draw(tex, screenLoc, new Color(1, 1, 1, 0.5f));
            }
        }

        public void GetContextActions(GameObject playerEntity, ContextArgs a)
        {
            a.Actions.Add(
                new ContextAction(
                    "Refresh thumbnail",
                    action: () =>
                    {
                        LoadingBox box = new LoadingBox()
                        {
                            LocationFunc = () => Coords.GetScreenCoords(new Vector3(Map.GetOffset(), 0), ScreenManager.CurrentScreen.Camera, ObjectDimensions),
                            ProgressFunc = () => Map.LoadProgress.ToString("##0%"),
                            TextFunc = () => "Loading map...",
                            TintFunc = () => Color.Lerp(Color.Red, Color.Lime, Map.LoadProgress),
                        };
                        box.Show();
                        return true;
                    }
            ));
        }

        public void GetTooltipInfo(Control tooltip)
        {
            this.Map.GetTooltipInfo(tooltip);
        }
    }
}
