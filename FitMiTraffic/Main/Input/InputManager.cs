using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using FitMi_Research_Puck;
using Microsoft.Xna.Framework.Input;
using FitMiTraffic.Main.Utility;
using Microsoft.Xna.Framework;

namespace FitMiTraffic.Main.Input
{

	static class InputManager
	{

		private static KeyboardState PreviousKeyState;
		private static KeyboardState CurrentKeyState;

		private static MouseState PreviousMouseState;
		private static MouseState CurrentMouseState;

		//private static HIDPuckDongle PuckManager;
		/*private static PuckPacket Puck
		{
			get
			{
				return PuckManager.PuckPack0;
			}
		}*/

		public static float LateralMovement
		{
			get
			{
				var gyro = 0;// Puck.Gyrometer[0];

				if (Math.Abs(gyro) > 50)
				{
					return gyro.Map(-500, 500, -1, 1);
				}
				else
				{
					if (CurrentKeyState.IsKeyDown(Keys.A))
					{
						return -0.8f;
					}
					else if (CurrentKeyState.IsKeyDown(Keys.D))
					{
						return 0.8f;
					}
				}

				return 0;
			}
		}

		public static bool ZoomOut
		{
			get
			{
				return PreviousKeyState.IsKeyUp(Keys.OemOpenBrackets) && CurrentKeyState.IsKeyDown(Keys.OemOpenBrackets);
			}
		}

		public static bool ZoomIn
		{
			get
			{
				return PreviousKeyState.IsKeyUp(Keys.OemCloseBrackets) && CurrentKeyState.IsKeyDown(Keys.OemCloseBrackets);
			}
		}

		public static Vector2 MoveCameraAmount
		{
			get
			{
				if (Mouse.GetState().MiddleButton == ButtonState.Released)
				{
					return Vector2.Zero;
				}
				Vector2 dir = new Vector2(PreviousMouseState.X - CurrentMouseState.X, PreviousMouseState.Y - CurrentMouseState.Y);

				return dir;
			}
		}

		public static bool ToggleDebug
		{
			get
			{
				return PreviousKeyState.IsKeyUp(Keys.Z) && CurrentKeyState.IsKeyDown(Keys.Z);
			}
		}

		public static bool Escape
		{
			get
			{
				return CurrentKeyState.IsKeyDown(Keys.Escape);
			}
		}

		public static bool Initialize()
		{
			/*PuckManager = new HIDPuckDongle();
			PuckManager.Open();
			PuckManager.SendCommand(0, HidPuckCommands.SENDVEL, 0x00, 0x01);
			PuckManager.SendCommand(1, HidPuckCommands.SENDVEL, 0x00, 0x01);*/

			return true;
		}

		public static void Update()
		{
			PreviousKeyState = CurrentKeyState;
			CurrentKeyState = Keyboard.GetState();

			PreviousMouseState = CurrentMouseState;
			CurrentMouseState = Mouse.GetState();

			//PuckManager.CheckForNewPuckData();
		}
	}
}
