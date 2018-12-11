﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using NewTrafficRacer.Modes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewTrafficRacer.Graphics
{
	public class RenderedModel
	{
		protected Model Model;
		protected Texture2D Texture;

		public Vector3 Position;
		public Vector3 Offset;
		public Matrix Rotation = Matrix.Identity;
		public Vector3 Size = Vector3.One;
		public Vector3 Scale = Vector3.One;
		public Color Color = Color.White;

		private Vector3 meshSize;

		private static Dictionary<string, string> names = new Dictionary<string, string>();


		private BoundingBox GetBounds()
		{
			if (Model.Tag == null)
			{
				throw new Exception("Model must be processed using Jacob's Flat Processor");
			}
			return (BoundingBox)Model.Tag;
		}

		public RenderedModel(ContentManager content, string modelName, string textureName = null)
		{
            Console.WriteLine("bout to load model: " + modelName);
			Model = content.Load<Model>(modelName);

			if (textureName != null)
			{
                Console.WriteLine("bout to load tex: " + textureName);
				Texture = content.Load<Texture2D>(textureName);
			}
			else
			{
				if (names.ContainsKey(modelName))
				{
                    Console.WriteLine("tryna load " + names[modelName]);
                    Texture = content.Load<Texture2D>(names[modelName]);
				}
				else
				{
					Texture = ((BasicEffect)Model.Meshes[0].Effects[0]).Texture;
					names.Add(modelName, Texture.Name);
				}
			}

			var bounds = GetBounds();
			meshSize = bounds.Max - bounds.Min;
		}

		public void Render(GameTime gameTime, Effect effect)
		{
			Matrix world =  Matrix.CreateScale(Size / meshSize * Scale) * Rotation * Matrix.CreateTranslation(Position + Offset);
			effect.Parameters["World"].SetValue(world);
			effect.Parameters["NormalMatrix"].SetValue(Matrix.Transpose(Matrix.Invert(world)));
            effect.Parameters["LightMatrix"].SetValue(world * GameMode.lighting.View * GameMode.lighting.Projection);
			effect.Parameters["ModelTexture"].SetValue(Texture);

            Vector4 ChromaKeyReplace = effect.Parameters["ChromaKeyReplace"].GetValueVector4();
            GameMode.RENDER_FIX(effect);
            effect.Parameters["ChromaKeyReplace"].SetValue(ChromaKeyReplace);
            foreach (ModelMesh mesh in Model.Meshes)
			{
				foreach (ModelMeshPart part in mesh.MeshParts)
				{
					part.Effect = effect;

				}
				mesh.Draw();
			}
		}
	}
}