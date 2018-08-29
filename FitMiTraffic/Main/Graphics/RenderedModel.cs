using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitMiTraffic.Main.Graphics
{
	public class RenderedModel
	{
		protected Model Model;

		public Vector3 Position;
		public Vector3 Rotation;

		public RenderedModel(ContentManager content, string modelName)
		{
			Model = content.Load<Model>(modelName);

			//This stuff re-calculates surface normals to give a low-poly flat shading effect
			foreach (ModelMesh mesh in Model.Meshes)
			{
				foreach (ModelMeshPart part in mesh.MeshParts)
				{
					VertexPositionNormalTexture[] vertices = new VertexPositionNormalTexture[part.VertexBuffer.VertexCount];
					part.VertexBuffer.GetData<VertexPositionNormalTexture>(vertices);

					short[] indices = new short[part.IndexBuffer.IndexCount];
					part.IndexBuffer.GetData<short>(indices);

					for (int i = 0; i < indices.Length; i += 3)
					{
						Vector3 p1 = vertices[indices[i]].Position;
						Vector3 p2 = vertices[indices[i + 1]].Position;
						Vector3 p3 = vertices[indices[i + 2]].Position;

						Vector3 v1 = p2 - p1;
						Vector3 v2 = p3 - p1;
						Vector3 normal = Vector3.Cross(v1, v2);

						normal.Normalize();

						vertices[indices[i]].Normal = normal;
						vertices[indices[i + 1]].Normal = normal;
						vertices[indices[i + 2]].Normal = normal;
					}

					part.VertexBuffer.SetData<VertexPositionNormalTexture>(vertices);
				}
			}
		}

		public void Render(GameTime gameTime, Matrix view, Matrix projection)
		{
			foreach (ModelMesh mesh in Model.Meshes)
			{
				foreach (BasicEffect effect in mesh.Effects)
				{
					//effect.EnableDefaultLighting();
					effect.LightingEnabled = true;
					effect.DirectionalLight0.Enabled = true;
					effect.DirectionalLight0.Direction = Vector3.Down;
					effect.DirectionalLight0.DiffuseColor = new Vector3(1, 1, 1);

					effect.TextureEnabled = true;
					effect.World = Matrix.CreateScale(2) * Matrix.CreateFromYawPitchRoll(Rotation.X, Rotation.Y, Rotation.Z) * Matrix.CreateTranslation(Position);
					effect.View = view;
					effect.Projection = projection;
					effect.Alpha = 1;
				}
				mesh.Draw();
			}
		}
	}
}
