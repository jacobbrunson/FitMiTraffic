using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FitMiAndroid;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using NewTrafficRacer.Utility;
using Microsoft.Xna.Framework.Input.Touch;

namespace NewTrafficRacer.Input
{

	static class InputManager
	{

		private static KeyboardState PreviousKeyState;
		private static KeyboardState CurrentKeyState;

		private static MouseState PreviousMouseState;
		private static MouseState CurrentMouseState;

        private static bool PuckPack1ForceDebounce = false;

		private static HIDPuckDongle PuckManager;
		private static PuckPacket Puck
		{
			get
			{
				return PuckManager.PuckPack0;
			}
		}

		public static float LateralMovement
		{
			get
			{
				var gyro = Puck.Gyrometer[0];

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

		public static bool Restart
		{
			get
			{
                TouchCollection touchLocations = TouchPanel.GetState();
                return PreviousKeyState.IsKeyUp(Keys.R) && CurrentKeyState.IsKeyDown(Keys.R) || touchLocations.Count > 0 && touchLocations[0].State == TouchLocationState.Pressed;
            }
		}

		public static bool ToggleDebug
		{
			get
			{
                if (PuckManager.PuckPack1.Loadcell > 500)
                {
                    if (PuckPack1ForceDebounce == false)
                    {
                        return true;
                    }
                    PuckPack1ForceDebounce = true;
                } else
                {
                    PuckPack1ForceDebounce = false;
                }
				return PreviousKeyState.IsKeyUp(Keys.Z) && CurrentKeyState.IsKeyDown(Keys.Z);
			}
		}

		public static bool Quit
		{
			get
			{
				return CurrentKeyState.IsKeyDown(Keys.Escape) || GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed;
			}
		}

        public static bool Enter
        {
            get
            {
                return CurrentKeyState.IsKeyDown(Keys.Enter);
            }
        }

        public static bool Initialize()
		{
			try
			{
				PuckManager = new HIDPuckDongle(Game.Activity);
				PuckManager.Open();
				PuckManager.SendCommand(0, HidPuckCommands.SENDVEL, 0x00, 0x01);
				PuckManager.SendCommand(1, HidPuckCommands.SENDVEL, 0x00, 0x01);

                if (!PuckManager.IsOpen)
                {
                    throw new Exception();
                }
			} catch (Exception e)
			{
				Console.WriteLine("Failed to initialize puck");
				return false;
			}

			return true;
		}

		public static void Update()
		{
			PreviousKeyState = CurrentKeyState;
			CurrentKeyState = Keyboard.GetState();

			PreviousMouseState = CurrentMouseState;
			CurrentMouseState = Mouse.GetState();

			PuckManager.CheckForNewPuckData();
		}
	}
}
