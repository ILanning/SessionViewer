using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using SessionViewer.Content;
using CommonCode.Modifiers;
using CommonCode.Drawing;
using CommonCode;

namespace SessionViewer
{
    class Skaia : Location, ICopyable<Skaia>
    {
        public TexturedPlane Battlefield;
        TexturedPlane godTierInnerSpiro;
        TexturedPlane godTierInnerFill;
        TexturedPlane godTierOuterSpiro;
        TexturedPlane godTierOuterFill;
        TexturedPlane godTierSymbol;
        TexturedPlane glowEffect;
        Spirograph[] reckoningGates;
        TexturedPlane[] reckoningMeteors;
        /// <summary>
        /// Spirograph under Skaia.
        /// </summary>
        Spirograph lowerSpirograph;
        public int PrototypeLevel = 0;

        public static Skaia BuildSkaia(string path, DynamicContentManager content)
        {
            return new Skaia(path, content);
        }

        public Skaia() { }

        public Skaia(string filePath, DynamicContentManager Content)
        {
            SkaiaBuilder builder = SkaiaBuilder.BuilderRead(filePath);
            plane = new TexturedPlane(builder.Image);
            Battlefield = new TexturedPlane(builder.Battlefield);
            Battlefield.DepthBias = -0.01f;
            plane.billboard = true;
            Battlefield.billboard = true;
            CursorDetectRadius = builder.CursorDetectRadius;
            PrimaryColor = new Color((byte)builder.PrimaryColor.X, (byte)builder.PrimaryColor.Y, (byte)builder.PrimaryColor.Z, (byte)builder.PrimaryColor.W);
            Name = builder.Name;
            Description = builder.Description;
            isVisible = builder.IsVisible;

            constructSkaia();
            //for (int i = 0; i < 50; i++)
            //{ 
            //    reckoningGates[i] = new 
            //}
        }

        private void constructSkaia() 
        {
            godTierInnerSpiro = new TexturedPlane(new Vector3(0, 8, 0), Quaternion.Identity, new Vector2(45), new Color(182, 251, 255, 100),
                                true, ScreenManager.Content.Load<Texture2D>(".//Miscellaneous//God Tier 1.png"));
            godTierInnerSpiro.depthBias = -0.02f;
            godTierInnerSpiro.AddModifier(new BillboardRotateModifier3D(0.0005f, true, false, -1));
            godTierInnerFill = new TexturedPlane(new Vector3(0, 8, 0), Quaternion.Identity, new Vector2(45), new Color(0, 197, 255, 66),
                                true, ScreenManager.Content.Load<Texture2D>(".//Miscellaneous//God Tier 2.png"));
            godTierInnerFill.depthBias = -0.01f;
            godTierInnerFill.AddModifier(new BillboardRotateModifier3D(0.0005f, true, false, -1));
            godTierOuterSpiro = new TexturedPlane(new Vector3(0, 8, 0), Quaternion.Identity, new Vector2(67), new Color(182, 251, 255, 66),
                                true, ScreenManager.Content.Load<Texture2D>(".//Miscellaneous//God Tier 1.png"));
            godTierOuterSpiro.depthBias = -0.04f;
            godTierOuterSpiro.AddModifier(new BillboardRotateModifier3D(0.0005f, true, false, -1));
            godTierOuterFill = new TexturedPlane(new Vector3(0, 8, 0), Quaternion.Identity, new Vector2(67), new Color(0, 197, 255, 66),
                                true, ScreenManager.Content.Load<Texture2D>(".//Miscellaneous//God Tier 2.png"));
            godTierOuterFill.depthBias = -0.03f;
            godTierOuterFill.AddModifier(new BillboardRotateModifier3D(0.0005f, true, false, -1));
            glowEffect = new TexturedPlane(new Vector3(0, 8, 0), Quaternion.Identity, new Vector2(40), new Color(0, 197, 255, 128),
                                true, ScreenManager.Content.Load<Texture2D>(".//Miscellaneous//Glow.png"));
            glowEffect.depthBias = -0.05f;
            godTierSymbol = new TexturedPlane(new Vector3(0, 8, 0), Quaternion.Identity, new Vector2(7), new Color(0, 0, 0, 255),
                                true, ScreenManager.Content.Load<Texture2D>(".//Miscellaneous//of Breath.png"));
            godTierSymbol.depthBias = -0.06f;

            lowerSpirograph = new Spirograph(1f, 10f / 7f, 7, 1, 12, 0, Color.White);
            lowerSpirograph.Rotation = Quaternion.CreateFromYawPitchRoll(0, 0, MathHelper.PiOver2);
            lowerSpirograph.WorldPosition = new Vector3(0, 2, 0);
            lowerSpirograph.AddModifier(new RotateModifier3D(Quaternion.CreateFromYawPitchRoll(-0.01f, 0, 0), false, -1));
            lowerSpirograph.Generate(600);

            reckoningGates = new Spirograph[50];
        }

        private Skaia(Skaia skaiaData)
        {
            plane = skaiaData.plane.DeepCopy();
            Battlefield = skaiaData.Battlefield.DeepCopy();
            PrimaryColor = skaiaData.PrimaryColor;
            Name = skaiaData.Name;
            Description = skaiaData.Description;
            isVisible = skaiaData.IsVisible;
            CursorDetectRadius = skaiaData.CursorDetectRadius;

            constructSkaia();
        }

        //public override void Initialize() { }

        public override void Update(GameTime gameTime)
        {
            Battlefield.Update(gameTime);
            godTierInnerSpiro.Update(gameTime);
            godTierInnerFill.Update(gameTime);
            godTierOuterSpiro.Update(gameTime);
            godTierOuterFill.Update(gameTime);
            lowerSpirograph.Update(gameTime);
            plane.Update(gameTime);
        }

        public override void HandleMessages(List<LocationMessage> messages)
        {
            //base.HandleMessages(messages);
        }

        /*public void DrawBattlefield(BasicEffect effect, GraphicsDevice graphics)
        {
            //RasterizerState offset = RasterizerState.CullNone;
            //offset.DepthBias -= 0.001f;
            //RasterizerState swap = graphics.RasterizerState;
            //graphics.RasterizerState = offset;
            graphics.RasterizerState.DepthBias -= 0.001f;
            //Vector3 offset = (ScreenManager.Globals.Camera.CameraPosition - WorldPlane.WorldPosition);
            //offset.Normalize();
            //Battlefield.WorldPosition = WorldPlane.WorldPosition + offset * 4;
            Battlefield.Draw(effect, graphics);
            graphics.RasterizerState.DepthBias += 0.001f;
            //graphics.RasterizerState = swap;
        }*/

        public override bool IsVisible
        {
            get { return isVisible; }
            set
            {
                if (isVisible == value)
                    return;
                if (!value)
                {
                    ColorModifier3D fadeModifier = new ColorModifier3D(Color.TransparentBlack, true, plane, 90);
                    fadeModifier.Complete += setisVisible;
                    plane.AddModifier(fadeModifier);
                }
                else
                {
                    isVisible = true;
                    plane.AddModifier(new ColorModifier3D(Color.White, true, plane, 90));
                }
            }
        }

        //TODO: Change how billboarding works 
        public override List<DrawListItem> GetDrawables(Camera camera)
        {
            var drawableList = new List<DrawListItem>(8);
            drawableList.Add(new DrawListItem(camera, plane));
            drawableList.Add(new DrawListItem(camera, lowerSpirograph));
            //Mess with depth buffer values to ensure the proper draw order
            drawableList.Add(new DrawListItem(Vector3.DistanceSquared(camera.CameraPosition, godTierInnerSpiro.WorldPosition) - 0.2f, godTierInnerSpiro));
            drawableList.Add(new DrawListItem(Vector3.DistanceSquared(camera.CameraPosition, godTierInnerFill.WorldPosition) - 0.1f, godTierInnerFill));
            drawableList.Add(new DrawListItem(Vector3.DistanceSquared(camera.CameraPosition, godTierOuterSpiro.WorldPosition) - 0.4f, godTierOuterSpiro));
            drawableList.Add(new DrawListItem(Vector3.DistanceSquared(camera.CameraPosition, godTierOuterFill.WorldPosition) - 0.3f, godTierOuterFill));
            drawableList.Add(new DrawListItem(Vector3.DistanceSquared(camera.CameraPosition, glowEffect.WorldPosition) - 0.5f, glowEffect));
            drawableList.Add(new DrawListItem(Vector3.DistanceSquared(camera.CameraPosition, godTierSymbol.WorldPosition) - 0.6f, godTierSymbol));
            return drawableList;
        }

        #region ICopyable<Skaia> Members

        public Skaia ShallowCopy()
        {
            Skaia clone = new Skaia();
            clone.Name = Name;
            clone.Description = Description;
            clone.plane = plane;
            clone.PrimaryColor = PrimaryColor;
            clone.CursorDetectRadius = CursorDetectRadius;
            return clone;
        }

        public Skaia ShallowCopy(LoadArgs l)
        {
            return ShallowCopy();
        }

        public Skaia DeepCopy()
        {
            Skaia clone = new Skaia(this);
            return clone;
        }

        public Skaia DeepCopy(LoadArgs l)
        {
            return DeepCopy();
        }

        #endregion
    }
}
