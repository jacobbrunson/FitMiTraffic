﻿using Microsoft.Xna.Framework;
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
