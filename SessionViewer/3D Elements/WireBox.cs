using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using SessionViewer;
using CommonCode;

namespace SessionViewer
{
    class WireBox : IModifiable3D
    {
        private Vector3 position;
        private Quaternion rotation;
        private Vector3 scale;
        private Color color;
        private VertexPositionColor[] lineVertexArray;
        private int[] vertexOrderArray = new int[24] { 0, 1, 1, 4, 4, 2, 2, 0, 0, 3, 1, 6, 4, 7, 2, 5, 3, 6, 6, 7, 7, 5, 5, 3 };

        public Vector3 WorldPosition { get { return position; } set { position = value; } }
        public Quaternion Rotation { get { return rotation; } set { rotation = value; } }
        public Vector3 Scale { get { return scale; } set { scale = value; } }
        public Color Color { get { return color; } set { color = value; } }

        public WireBox(Vector3 location, Quaternion direction, Vector3 size, Color color)
        {
            position = location;
            rotation = direction;
            size *= 0.5f;
            scale = size;
            this.color = color;
            lineVertexArray = new VertexPositionColor[] { 
                            new VertexPositionColor(new Vector3( size.X,  size.Y,  size.Z), color), 
                            new VertexPositionColor(new Vector3(-size.X,  size.Y,  size.Z), color), 
                            new VertexPositionColor(new Vector3( size.X, -size.Y,  size.Z), color), 
                            new VertexPositionColor(new Vector3( size.X,  size.Y, -size.Z), color), 
                            new VertexPositionColor(new Vector3(-size.X, -size.Y,  size.Z), color), 
                            new VertexPositionColor(new Vector3( size.X, -size.Y, -size.Z), color),
                            new VertexPositionColor(new Vector3(-size.X,  size.Y, -size.Z), color), 
                            new VertexPositionColor(new Vector3(-size.X, -size.Y, -size.Z), color)};
        }

        public void Draw(Effect effect, GraphicsDevice graphics)
        {
            if (!(effect is BasicEffect))
                throw new ArgumentException("effect", "Cameras only support the BasicEffect");
            ((BasicEffect)effect).TextureEnabled = false;
            ((BasicEffect)effect).VertexColorEnabled = true;
            ((BasicEffect)effect).World = Matrix.CreateFromQuaternion(rotation) * Matrix.CreateTranslation(position);
            graphics.DrawUserIndexedPrimitives<VertexPositionColor>(PrimitiveType.LineList,
                       lineVertexArray, 0, 8, vertexOrderArray, 0, 12);
        }

        #region IModifiable Members

        IModifier3D[] modifiers = new IModifier3D[4];

        public IModifier3D[] Modifiers
        {
            get { return modifiers; }
        }

        public void AddModifier(IModifier3D modifier)
        {
            modifier.Owner = this;
            for (int i = 0; i <= modifiers.Length; i++)
            {
                if (i == modifiers.Length)
                {
                    IModifier3D[] newModifiersArray = new IModifier3D[modifiers.Length + 4];
                    for (int h = 0; h < modifiers.Length; h++)
                    {
                        newModifiersArray[i] = modifiers[i];
                    }
                    newModifiersArray[modifiers.Length] = modifier;
                    modifiers = newModifiersArray;
                }
                if (modifiers[i] == null)
                {
                    modifiers[i] = modifier;
                    break;
                }
            }
        }

        public void ClearModifiers()
        {
            modifiers = new IModifier3D[4];
        }

        #endregion
    }
}
