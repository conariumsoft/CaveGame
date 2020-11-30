using CaveGame.Core;
using CaveGame.Core.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace CaveGame.Core
{
    


    using Circle = List<Vector2>;
    using Arc = List<Vector2>;

    public class MissingGameDataException : ApplicationException { 
        public string MissingFilename { get; set; }
        public string MissingFilepath { get; set; }
        public string FilePurpose { get; set; }
        public string WhatToDo { get; set; }


    }
    public class MissingContentFolderException : MissingGameDataException { }
    public class MissingScriptException : MissingGameDataException { }
    public class MissingSoundEffectException : MissingGameDataException { }
    public class MissingTextureException : MissingGameDataException { }

    public static class ShapeCache
    {
        private static readonly Dictionary<String, Circle> circleCache = new Dictionary<string, Circle>();

        public static Arc GetArc(float radius, int sides, float startingAngle, float radians)
        {
            Arc points = new Arc();

            points.AddRange(GetCircle(radius, sides));
            points.RemoveAt(points.Count - 1);

            double curAngle = 0.0;
            double anglePerSide = MathHelper.TwoPi / sides;

            while ((curAngle + (anglePerSide / 2.0)) < startingAngle)
            {
                curAngle += anglePerSide;

                points.Add(points[0]);
                points.RemoveAt(0);
            }

            points.Add(points[0]);
            int sidesInArc = (int)((radians / anglePerSide) + 0.5);

            points.RemoveRange(sidesInArc + 1, points.Count - sidesInArc - 1);

            return points;
        }

        public static Circle GetCircle(double radius, int sides)
        {
            String circleKey = radius + "x" + sides;

            if (circleCache.ContainsKey(circleKey))
                return circleCache[circleKey];

            Circle circleDef = new Circle();

            const double max = 2.0 * Math.PI;

            double step = max / sides;

            for (double theta = 0.0; theta < max; theta += step)
            {
                circleDef.Add(new Vector2((float)(radius * Math.Cos(theta)), (float)(radius * Math.Sin(theta))));
            }

            circleDef.Add(new Vector2((float)(radius * Math.Cos(0)), (float)(radius * Math.Sin(0))));

            circleCache.Add(circleKey, circleDef);

            return circleDef;
        }
    }
    public class FontManager
    {
        public SpriteFont Arial8 { get; private set; }
        public SpriteFont Arial10 { get; private set; }
        public SpriteFont Arial12 { get; private set; }
        public SpriteFont Arial14 { get; private set; }
        public SpriteFont Arial16 { get; private set; }
        public SpriteFont Arial20 { get; private set; }
        public SpriteFont Arial30 { get; private set; }
        public SpriteFont Arial10Italic { get; private set; }
        public SpriteFont Consolas10 { get; private set; }
        public SpriteFont Consolas12 { get; private set; }
        public SpriteFont ComicSans10 { get; private set; }
        public void LoadAssets(ContentManager Content)
        {
            Arial8 = Content.Load<SpriteFont>("Fonts/Arial8");
            Arial10 = Content.Load<SpriteFont>("Fonts/Arial10");
            Arial12 = Content.Load<SpriteFont>("Fonts/Arial12");
            Arial14 = Content.Load<SpriteFont>("Fonts/Arial14");
            Arial16 = Content.Load<SpriteFont>("Fonts/Arial16");
            Arial20 = Content.Load<SpriteFont>("Fonts/Arial20");
            Arial30 = Content.Load<SpriteFont>("Fonts/Arial30");
            Arial10Italic = Content.Load<SpriteFont>("Fonts/Arial10Italic");
            Consolas10 = Content.Load<SpriteFont>("Fonts/Consolas10");
            Consolas12 = Content.Load<SpriteFont>("Fonts/Consolas12");
            ComicSans10 = Content.Load<SpriteFont>("Fonts/ComicSans10");
        }
    }

    public class GraphicsEngine : IGraphicsEngine
    {


        public bool LoadFonts { get; set; }

        public static GraphicsEngine Instance { get; private set; }

        #region Texture Shortcuts
        public Texture2D Player => Textures["Entities/player.png"];
        public Texture2D TitleScreen => Textures["TitleScreen.png"];
        public Texture2D EyeOfHorus => Textures["csoft.png"];
        public Texture2D ParticleSet => Textures["particles.png"];
        public Texture2D TileSheet => Textures["tilesheet.png"];
        public Texture2D BG => Textures["menu_bg.png"];
        public Texture2D Border => Textures["border.png"];
        public Texture2D Slot => Textures["slot.png"];

        public Texture2D BombSprite => Textures["bomb.png"];
		public Texture2D Bong => Textures["bong.png"];
		public Texture2D Arrow => Textures["arrow.png"];
		public Texture2D Bucket => Textures["bucket.png"];
		public Texture2D BigPickaxe => Textures["bigpickaxe.png"];
		public Texture2D Helmet => Textures["helmet.png"];
		public Texture2D Chestplate => Textures["chestplate.png"];
		public Texture2D Sword => Textures["sword.png"];
		public Texture2D WallScraper => Textures["wallscraper.png"];
		public Texture2D PickaxeNew => Textures["pickaxenew.png"];
		public Texture2D Scroll => Textures["scroll.png"];
		public Texture2D Dynamite => Textures["dynamite.png"];
		public Texture2D Workbench => Textures["workbench.png"];
		public Texture2D Potion => Textures["potion.png"];
		public Texture2D Jetpack => Textures["jetpack.png"];
		public Texture2D Door => Textures["door.png"];
		public Texture2D ForestPainting => Textures["forestpainting.png"];
		public Texture2D Ingot => Textures["ingot.png"];
		public Texture2D Leggings => Textures["leggings.png"];
		public Texture2D Furnace => Textures["furnace.png"];
		public Texture2D Campfire => Textures["campfire.png"];
        public Texture2D VoidMonster => Textures["Entities/tortured.png"];

        //public static Texture2D Campfire	=> Textures["campfire.png"];
        #endregion

        public Dictionary<string, Texture2D> Textures = new Dictionary<string, Texture2D>();

        struct TextureDef
        {
            public string Path { get; set; }
            public string Name { get; set; }
            public TextureDef(string name, string path)
            {
                Name = name;
                Path = path;
            }
        }

        private Queue<TextureDef> LoadingQueue = new Queue<TextureDef>();

        public AssetLoader AssetLoader = new AssetLoader();
        private Texture2D pixel;

        private void LoadNextAsset(GraphicsDevice gdev)
        {
            TextureDef nextTex = LoadingQueue.Dequeue();
            Texture2D loaded = AssetLoader.LoadTexture(gdev, nextTex.Path);
            Textures.Add(nextTex.Name, loaded);
            //Debug.WriteLine("{0} => {1}", nextTex.Name, nextTex.Path);
            LoadedTextures++;
        }

        public int TotalTextures { get; private set; }
        public int LoadedTextures { get; private set; }

        public bool FontsLoaded { get; set; }

        public void LoadAssets(GraphicsDevice graphicsDevice)
        {
            var texturesPath = Path.Combine("Assets", "Textures");
            if (!Directory.Exists(texturesPath))
                throw new MissingContentFolderException { MissingFilename = "texturesPath" };

            foreach (var tex in Directory.GetFiles("Assets/Textures/", "*.png"))
            {
                LoadingQueue.Enqueue(new TextureDef(
                    tex.Replace("Assets/Textures/", ""),
                    tex
                ));
                TotalTextures++;
            }


            var entityTexturesPath = Path.Combine("Assets", "Textures", "Entities");
            foreach (var tex in Directory.GetFiles("Assets/Textures/Entities/", "*.png"))
            {
                // Texture2D loaded = AssetLoader.LoadTexture(graphicsDevice, tex);
                LoadingQueue.Enqueue(new TextureDef(
                    tex.Replace("Assets/Textures/", ""),
                    tex
                ));
                TotalTextures++;
            }

            foreach (var tex in Directory.GetFiles("Assets/Textures/Items/", "*.png"))
            {
                //Texture2D loaded = AssetLoader.LoadTexture(graphicsDevice, tex);
                LoadingQueue.Enqueue(new TextureDef(
                    tex.Replace("Assets/Textures/Items/", ""),
                    tex
                ));
                TotalTextures++;
            }
        }

        public void Initialize()
        {
            SpriteBatch = new SpriteBatch(GraphicsDevice);
            if (LoadFonts)
                Fonts.LoadAssets(ContentManager);

            // create pixel
            pixel = new Texture2D(GraphicsDevice, 1, 1);
            pixel.SetData<Color>(new Color[] { Color.White });

            FontsLoaded = true;
        }

        public FontManager Fonts = new FontManager();

        public Vector2 WindowSize { get; set; }
        public SpriteSortMode SpriteSortMode { get; set; }
        public BlendState BlendState { get; set; }
        public SamplerState SamplerState { get; set; }
        public DepthStencilState DepthStencilState { get; set; }
        public RasterizerState RasterizerState { get; set; }
        public Effect Shader { get; set; }
        public Matrix Matrix { get; set; }

        public ContentManager ContentManager { get; set; }
        public SpriteBatch SpriteBatch { get; set; }
        public GraphicsDevice GraphicsDevice { get; set; }
        public GraphicsDeviceManager GraphicsDeviceManager { get; set; }

        public bool ContentLoaded { get; private set; }

        public float GraphicsTimer { get; set; }

        public float LoadingDelay { get; set; }

        public GraphicsEngine()
        {
            Instance = this;
            LoadingDelay = 0.03f;
            FontsLoaded = false;
            LoadFonts = true;
        }

        float ardeusIsADipshit;

        public void Update(GameTime gt)
        {
            //ardeusIsADipshit += gt.GetDelta();


           // if (ardeusIsADipshit > LoadingDelay)
           // {
               // ardeusIsADipshit = 0;
                if (LoadingQueue.Count > 0)
                    LoadNextAsset(GraphicsDevice);

                if (LoadingQueue.Count == 0)
                    ContentLoaded = true;
           // }
        }

        public void Clear(Color color) => GraphicsDevice.Clear(color);
        public void End() => SpriteBatch.End();
        public void Begin() => SpriteBatch.Begin();
        public void Begin(SpriteSortMode sorting = SpriteSortMode.Deferred, BlendState blending = null, SamplerState sampling = null, DepthStencilState depthStencil = null, RasterizerState rasterizing = null, Effect effect = null, Matrix? transform = null)
        {
            SpriteBatch.Begin(sorting, blending, sampling, depthStencil, rasterizing, effect, transform);
        }
        public void Arc(Color color, Vector2 center, float radius, int sides, Rotation startingAngle, Rotation radians, float thickness = 1)
        {
            List<Vector2> arc = ShapeCache.GetArc(radius, sides, startingAngle.Radians, radians.Radians);
            Polygon(color, center, arc, thickness);
        }
        public void Circle(Color color, Vector2 position, double radius, int sides = 12, float thickness = 1)
        {
            List<Vector2> c = ShapeCache.GetCircle(radius, sides);
            Polygon(color, position, c, thickness);
        }
        public void Line(Color color, Vector2 point, float length, Rotation angle, float thickness = 1)
        {
            Vector2 origin = new Vector2(0f, 0.5f);
            Vector2 scale = new Vector2(length, thickness);
            SpriteBatch.Draw(pixel, point, null, color, angle.Radians, origin, scale, SpriteEffects.None, 0);
        }
        public void Line(Color color, Vector2 point1, Vector2 point2, float thickness = 1)
        {
            float distance = Vector2.Distance(point1, point2);
            float angle = (float)Math.Atan2(point2.Y - point1.Y, point2.X - point1.X);

            float expanded = (float)Math.Floor(angle * Math.PI);
            float backDown = expanded / (float)Math.PI;

            Line(color, point1, distance, Rotation.FromRad(angle), thickness);
        }
        public void OutlineRect(Color color, Vector2 position, Vector2 size, float thickness = 2.0f)
        {
            Line(color, position, position + new Vector2(0, size.Y), thickness);
            Line(color, position, position + new Vector2(size.X, 0), thickness);
            Line(color, position + new Vector2(size.X, 0), position + size, thickness);
            Line(color, position + new Vector2(0, size.Y), position + new Vector2(size.X, size.Y), thickness);
        }
        public void Polygon(Color color, Vector2 position, List<Vector2> points, float thickness = 1)
        {
            if (points.Count < 2)
                return;

            for (int i = 1; i < points.Count; i++)
                Line(color, points[i - 1] + position, points[i] + position, thickness);
        }
        public void Polygon(Color color, List<Vector2> points, float thickness = 1)
        {
            if (points.Count < 2)
                return;

            for (int i = 1; i < points.Count; i++)
                Line(color, points[i - 1], points[i], thickness);
        }
        public void Rect(Color color, Vector2 position, Vector2 size) => Rect(color, position, size, Rotation.Zero);
        public void Rect(Color color, Vector2 position, Vector2 size, Rotation rotation)
        {
            SpriteBatch.Draw(
                pixel,
                new Rectangle(position.ToPoint(), size.ToPoint()),
                null,
                color, rotation.Degrees, new Vector2(0, 0), SpriteEffects.None, 0
            );
        }
        public void Sprite(Texture2D texture, Vector2 position) => Sprite(texture, position, Color.White);
        public void Sprite(Texture2D texture, Vector2 position, Color color) => SpriteBatch.Draw(texture, position, color);
        public void Sprite(Texture2D texture, Vector2 position, Rectangle? quad, Color color) => SpriteBatch.Draw(texture, position, quad, color);
        public void Sprite(Texture2D texture, Vector2 position, Rectangle? quad, Color color, Rotation rotation, Vector2 origin, Vector2 scale, SpriteEffects efx, float layer) => SpriteBatch.Draw(texture,position,quad,color,rotation.Degrees,origin,scale,efx,layer);
        public void Sprite(Texture2D texture, Vector2 position, Rectangle? quad, Color color, Rotation rotation, Vector2 origin, float scale, SpriteEffects efx, float layer) => SpriteBatch.Draw(texture, position, quad, color, rotation.Degrees, origin, scale, efx, layer);
        public void Text(string text, Vector2 position) => Text(Fonts.Arial10, text, position);
        public void Text(string text, Vector2 position, Color color) => SpriteBatch.DrawString(Fonts.Arial10, text, position, color);
        public void Text(SpriteFont font, string text, Vector2 position) => Text(font, text, position, Color.White, TextXAlignment.Left, TextYAlignment.Top);
        public void Text(SpriteFont font, string text, Vector2 position, Color color, TextXAlignment textX = TextXAlignment.Left, TextYAlignment textY = TextYAlignment.Top)
        {
            float xoffset = 0;
            float yoffset = 0;

            Vector2 bounds = font.MeasureString(text);

            if (textX == TextXAlignment.Center)
                xoffset = bounds.X / 2;
            if (textX == TextXAlignment.Right)
                xoffset = bounds.X;

            if (textY == TextYAlignment.Center)
                yoffset = bounds.Y / 2;
            if (textY == TextYAlignment.Bottom)
                yoffset = bounds.Y;

            SpriteBatch.DrawString(font, text, position-new Vector2(xoffset, yoffset), color);
        }
        /*public void Line3D(Camera camera, Vector3 pointA, Vector3 pointB, Color color)
		{
			Line3D(camera, pointA, pointB, color, color);
		}
		public void Line3D(Camera camera, Vector3 pointA, Vector3 pointB, Color colorA, Color colorB)
		{
			Line3D(camera, pointA, pointB, colorA, colorB);
		}
		public void Line3D(Camera camera, Vector3 pointA, Vector3 pointB, Color color)
		{
			Line3D(camera, pointA, pointB, color, color);
		}
		public void Line3D(Camera camera, Vector3 pointA, Vector3 pointB, Color colorA, Color colorB)
		{
			_effect3D.View = camera.View;
			_effect3D.Projection = camera.Projection;

			_effect3D.CurrentTechnique.Passes[0].Apply();

			var vertices = new[] { new VertexPositionColor(pointA, colorA), new VertexPositionColor(pointB, colorB)};
			GraphicsDevice.DrawUserPrimitives(PrimitiveType.LineList, vertices, 0, 1);
		}*/

    }
}
