using CommonCode;
using CommonCode.Drawing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SessionViewer.Content;

namespace SessionViewer
{
    class GenericMeteor : IModifiable3D, ICopyable<GenericMeteor>, IDrawable3D
    {
        protected TexturedPlane plane;
        public string name;

        public static GenericMeteor BuildGenericMeteor(string path, DynamicContentManager content)
        {
            return new GenericMeteor(path, content);
        }

        public GenericMeteor() { }

        public GenericMeteor(string filePath, DynamicContentManager Content)
        {
            GenericMeteorBuilder meteorData = GenericMeteorBuilder.BuilderRead(filePath);
            plane = new TexturedPlane(meteorData.Image);
            plane.billboard = true;
            name = meteorData.Name;
        }

        public GenericMeteor(TexturedPlane image, string Name)
        {
            plane = image;
            name = Name;
        }

        public virtual void Update(GameTime gameTime)
        {
            plane.Update(gameTime);
        }

        public virtual void Draw(Effect effect, GraphicsDevice graphics)
        {
            plane.Draw(effect, graphics);
        }

        public float DepthBias { get { return plane.DepthBias; } set { plane.DepthBias = value; } }

        #region IModifiable3D Members

        public Vector3 WorldPosition { get { return plane.WorldPosition; } set { plane.WorldPosition = value; } }
        public Quaternion Rotation { get { return plane.Rotation; } set { plane.Rotation = value; } }
        public Vector3 Scale { get { return plane.Scale; } set { plane.Scale = value; } }
        public Color Color { get { return plane.Color; } set { plane.Color = value; } }

        public IModifier3D[] Modifiers { get { return plane.Modifiers; } }

        public void AddModifier(IModifier3D modifier)
        {
            plane.AddModifier(modifier);
        }

        public void ClearModifiers()
        {
            plane.ClearModifiers();
        }
        #endregion

        #region ICopyable<Meteor> Members

        public GenericMeteor ShallowCopy()
        {
            GenericMeteor clone = new GenericMeteor();
            clone.name = name;
            clone.plane = plane;
            return clone;
        }

        public GenericMeteor DeepCopy()
        {
            GenericMeteor clone = new GenericMeteor(plane.DeepCopy(), name);
            return clone;
        }
        #endregion
    }
}
