using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitMiTraffic.Main.Environment
{
	public class BigSign
	{

		public Vector2 Position;

		public BigSign() { }

		public void Render(SpriteBatch spriteBatch, GameTime gameTime, Matrix projection, Matrix view)
		{
			foreach (ModelMesh mesh in model.Meshes)
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

				foreach (BasicEffect effect in mesh.Effects)
				{
					effect.EnableDefaultLighting();
					effect.TextureEnabled = true;
					effect.World = Matrix.CreateScale(2f) * Matrix.CreateFromYawPitchRoll(0, 0, 0) * Matrix.CreateTranslation(Position.X, Position.Y, 0);
					effect.View = view;
					effect.Projection = projection;
					effect.Alpha = 1;
				}
				mesh.Draw();
			}
		}

		private static Model model;

		public static void LoadContent(ContentManager content)
		{
			model = content.Load<Model>("bigsign");
		}
	}
}
