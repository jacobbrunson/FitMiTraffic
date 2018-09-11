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
		protected Texture2D Texture;

		private Effect effect;


		private Effect shadowEffect;

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
			Vector3 min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
			Vector3 max = new Vector3(float.MinValue, float.MinValue, float.MinValue);

			foreach (ModelMesh mesh in this.Model.Meshes)
			{
				foreach (ModelMeshPart meshPart in mesh.MeshParts)
				{
					int vertexStride = meshPart.VertexBuffer.VertexDeclaration.VertexStride;
					int vertexBufferSize = meshPart.NumVertices * vertexStride;

					int vertexDataSize = vertexBufferSize / sizeof(float);
					float[] vertexData = new float[vertexDataSize];
					meshPart.VertexBuffer.GetData<float>(vertexData);

					for (int i = 0; i < vertexDataSize; i += vertexStride / sizeof(float))
					{
						Vector3 vertex = new Vector3(vertexData[i], vertexData[i + 1], vertexData[i + 2]);
						min = Vector3.Min(min, vertex);
						max = Vector3.Max(max, vertex);
					}
				}
			}

			return new BoundingBox(min, max);
		}
		private void FlattenNormals()
		{
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

		public RenderedModel(ContentManager content, string modelName, string textureName=null)
		{
			Model = content.Load<Model>(modelName);
			effect = content.Load<Effect>("effect");
			//shadowEffect = content.Load<Effect>("shadowmap");

			if (textureName != null)
			{
				Texture = content.Load<Texture2D>(textureName);
			} else
			{
				if (names.ContainsKey(modelName))
				{
					Texture = content.Load<Texture2D>(names[modelName]);
				} else
				{
					Texture = ((BasicEffect)Model.Meshes[0].Effects[0]).Texture;
					names.Add(modelName, Texture.Name);
				}
			}

			//if (new Random().Next(0, 2) == 0)
			FlattenNormals();

			var bounds = GetBounds();
			meshSize = bounds.Max - bounds.Min;
		}

		public void RenderShadowMap(GraphicsDevice graphics, Matrix lightViewProjection)
		{
			Matrix world = Matrix.CreateScale(Size / meshSize) * Rotation * Matrix.CreateTranslation(Position + Offset);
			for (int index = 0; index < Model.Meshes.Count; index++)
			{
				ModelMesh mesh = Model.Meshes[index];
				for (int i = 0; i < mesh.MeshParts.Count; i++)
				{
					
					ModelMeshPart meshpart = mesh.MeshParts[i];
					shadowEffect.Parameters["World"].SetValue(world);
					shadowEffect.Parameters["LightViewProj"].SetValue(world * lightViewProjection);

					shadowEffect.CurrentTechnique.Passes[0].Apply();
					graphics.SetVertexBuffer(meshpart.VertexBuffer);
					graphics.Indices = meshpart.IndexBuffer;
					//shadowEffect.SetVertexBuffer(meshpart.VertexBuffer);
					//shadowEffect.Indices = (meshpart.IndexBuffer);
					int primitiveCount = meshpart.PrimitiveCount;
					int vertexOffset = meshpart.VertexOffset;
					int vCount = meshpart.NumVertices;
					int startIndex = meshpart.StartIndex;
					
					graphics.DrawIndexedPrimitives(PrimitiveType.TriangleList, vertexOffset, startIndex,
						primitiveCount);
				}
			}
		}

		public void Render(GameTime gameTime, BaseEffect effect)
		{
			Matrix world = Matrix.CreateScale(Size / meshSize * Scale) * Rotation * Matrix.CreateTranslation(Position + Offset);
			if (effect.Technique.Name.Equals("ShadowMap"))
			{
				//Console.WriteLine(effect.LightViewProjection);
			}
			foreach (ModelMesh mesh in Model.Meshes)
			{
				foreach (ModelMeshPart part in mesh.MeshParts)
				{
					part.Effect = effect.Effect;

					//effect.CurrentTechnique = effect.Techniques[technique];
					effect.Parameters["World"].SetValue(world); //world * mesh.ParentBone.Transform
					//effect.Parameters["View"].SetValue(view);
					//effect.Parameters["Projection"].SetValue(projection);
					//effect.Parameters["LightViewProj"].SetValue(world * lightViewProjection);
					//effect.Parameters["ShadowMap"].SetValue(shadowMap);
					effect.Parameters["xLightsWorldViewProjection"].SetValue(world * effect.LightViewProjection);

					Matrix worldInverseTransposeMatrix = Matrix.Transpose(Matrix.Invert(world)); // mesh.ParentBone.Transform * world
					effect.Parameters["WorldInverseTranspose"].SetValue(worldInverseTransposeMatrix);

					effect.Parameters["ModelTexture"].SetValue(Texture);

					//effect.Parameters["xLightPos"].SetValue(TrafficGame.lightPosition);
					//effect.Parameters["DiffuseLightDirection"].SetValue(TrafficGame.lightDirection);

					effect.Parameters["resolution"].SetValue(new Vector2(600, 800));

					effect.Parameters["CarColor"].SetValue(new Vector4(Color.R/255f, Color.G/255f, Color.B/255f, 1));
				}
				mesh.Draw();
			}
		}
	}
}
