using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using FitMi_Research_Puck;
using Microsoft.Xna.Framework.Input;
using FitMiTraffic.Main.Utility;

namespace FitMiTraffic.Main.Input
{

	static class InputManager
	{

		private static KeyboardState PreviousState;
		private static KeyboardState CurrentState;

		/*private static HIDPuckDongle PuckManager;
		private static PuckPacket Puck
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
					if (CurrentState.IsKeyDown(Keys.A))
					{
						return -0.5f;
					}
					else if (CurrentState.IsKeyDown(Keys.D))
					{
						return 0.5f;
					}
				}

				return 0;
			}
		}

		public static bool ZoomOut
		{
			get
			{
				return PreviousState.IsKeyUp(Keys.OemOpenBrackets) && CurrentState.IsKeyDown(Keys.OemOpenBrackets);
			}
		}

		public static bool ZoomIn
		{
			get
			{
				return PreviousState.IsKeyUp(Keys.OemCloseBrackets) && CurrentState.IsKeyDown(Keys.OemCloseBrackets);
			}
		}

		public static bool ToggleDebug
		{
			get
			{
				return PreviousState.IsKeyUp(Keys.Z) && CurrentState.IsKeyDown(Keys.Z);
			}
		}

		public static bool Escape
		{
			get
			{
				return CurrentState.IsKeyDown(Keys.Escape);
			}
		}

		public static bool Initialize()
		{
			//PuckManager = new HIDPuckDongle();
			//PuckManager.Open();
			//PuckManager.SendCommand(0, HidPuckCommands.SENDVEL, 0x00, 0x01);
			//PuckManager.SendCommand(1, HidPuckCommands.SENDVEL, 0x00, 0x01);

			return true;
		}

		public static void Update()
		{
			PreviousState = CurrentState;
			CurrentState = Keyboard.GetState();
			//PuckManager.CheckForNewPuckData();
		}
	}
}
