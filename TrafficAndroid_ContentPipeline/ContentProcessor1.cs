// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.
//Modified by Jacob Brunson to calculate bounding box and adjust mesh normals to give a flat shading effect

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;

namespace Microsoft.Xna.Framework.Content.Pipeline.Processors
{
    [ContentProcessor(DisplayName = "Model - Jacob's Flat Effect")]
    class ContentProcessor1 : ModelProcessor
    {
        public override ModelContent Process(NodeContent input, ContentProcessorContext context)
        {

            double minX = double.MaxValue;
            double minY = double.MaxValue;
            double minZ = double.MaxValue;
            double maxX = double.MinValue;
            double maxY = double.MinValue;
            double maxZ = double.MinValue;

            foreach (NodeContent nc in input.Children)
            {
                if (nc is MeshContent)
                {
                    MeshContent mc = (MeshContent)nc;
                    foreach (Vector3 v in mc.Positions)
                    {
                        if (v.X < minX)
                            minX = v.X;
                        if (v.Y < minY)
                            minY = v.Y;
                        if (v.Z < minZ)
                            minZ = v.Z;
                        if (v.X > maxX)
                            maxX = v.X;
                        if (v.Y > maxY)
                            maxY = v.Y;
                        if (v.Z > maxZ)
                            maxZ = v.Z;
                    }
                }
            }
            double lenX = maxX - minX;
            double lenZ = maxZ - minZ;
            double lenY = maxY - minY;

            ModelContent model = base.Process(input, context);

            FlattenNormals(model);
            BoundingBox bb = GetBoundingBox(model);
            model.Tag = bb;

            Console.WriteLine(bb.Min);

            return model;
        }

        private Vector3 GetVertexPosition(VertexBufferContent buffer, int i)
        {
            float[] pos = new float[3];

            for (int j = 0; j < 3; j++)
            {
                int offset = i * (int)buffer.VertexDeclaration.VertexStride + j * 4; //4 bytes per float
                pos[j] = BitConverter.ToSingle(buffer.VertexData, offset);
            }

            return new Vector3(pos[0], pos[1], pos[2]);
        }

        private void SetVertexNormalCoord(VertexBufferContent buffer, int i, Vector3 normal, int c)
        {
            int offset = i * (int)buffer.VertexDeclaration.VertexStride + 12;
            float f;

            if (c == 0)
            {
                f = normal.X;
            }
            else if (c == 1)
            {
                f = normal.Y;
            }
            else
            {
                f = normal.Z;
            }

            byte[] bytes = BitConverter.GetBytes(f);
            for (int j = 0; j < 4; j++)
            {
                buffer.VertexData[i + c * 4 + j] = bytes[j];
            }
        }

        private BoundingBox GetBoundingBox(ModelContent model)
        {
            Vector3 min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            Vector3 max = new Vector3(float.MinValue, float.MinValue, float.MinValue);

            foreach (ModelMeshContent mesh in model.Meshes)
            {

                foreach (ModelMeshPartContent part in mesh.MeshParts)
                {
                    for (int i = 0; i < part.NumVertices; i++)
                    {
                        Vector3 pos = GetVertexPosition(part.VertexBuffer, i);
                        min = Vector3.Min(min, pos);
                        max = Vector3.Max(max, pos);
                    }
                }
            }

            return new BoundingBox(min, max);
        }

        private void FlattenNormals(ModelContent model)
        {
            //This stuff re-calculates surface normals to give a low-poly flat shading effect
            foreach (ModelMeshContent mesh in model.Meshes)
            {

                foreach (ModelMeshPartContent part in mesh.MeshParts)
                {
                    IndexCollection indices = part.IndexBuffer;
                    for (int i = 0; i < indices.Count; i += 3)
                    {
						Console.WriteLine("TEST TEST 123");
                        Vector3 p1 = GetVertexPosition(part.VertexBuffer, indices[i + 0]);
                        Vector3 p2 = GetVertexPosition(part.VertexBuffer, indices[i + 1]);
                        Vector3 p3 = GetVertexPosition(part.VertexBuffer, indices[i + 2]);

                        Vector3 v1 = p2 - p1;
                        Vector3 v2 = p3 - p1;
                        Vector3 normal = Vector3.Cross(v1, v2);

                        normal.Normalize();

                        for (int j = 0; j < 3; j++)
                        {
                            for (int k = 0; k < 3; k++)
                            {
                                SetVertexNormalCoord(part.VertexBuffer, i + j, normal, k);
                            }
                        }
                    }

                    /*
					byte[] vertices = part.VertexBuffer.VertexData;
					//We only have the packed vertex data available so we need to parse the individual bytes
					for (int i = 0; i < part.NumVertices; i++)
					{
						float[] pos = new float[3];
						float[] normal = new float[3];

						for (int j = 0; j < 3; j++)
						{
							int posOffset = i * (int)part.VertexBuffer.VertexDeclaration.VertexStride + j * 4; //4 bytes per float
							int normalOffset = posOffset + 12; //Normal is after the 3 position words

							pos[j] = System.BitConverter.ToSingle(vertices, posOffset);
							normal[j] = System.BitConverter.ToSingle(vertices, normalOffset);
						}

						Console.WriteLine("=====");
					}*/


                    //byte[] newArray = new[] { vertices[3], vertices[2], vertices[1], vertices[0] };


                    /*short[] indices = new short[part.IndexBuffer.IndexCount];
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

					part.VertexBuffer.SetData<VertexPositionNormalTexture>(vertices);*/
                }
            }
        }
    }
}