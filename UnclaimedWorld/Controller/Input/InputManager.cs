

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GameStateManagement;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

using InputEventSystem;
using UWGame.Control.Replays;
using System.Diagnostics;

namespace UWGame.Control.Input
{
    /// <summary>
    /// Each frame, reads input from either the devices or from a recorded file and updates the InputData class.
    /// </summary>    
    public class InputManager
    {
        public InputData InputData;

        Replayer replayer;

        bool isReplaying;


        Controller controller;

        public void Update(GameTime gameTime, bool gameHasFocus) //, Controller controller)
        {
            if(isReplaying == false || replayer.Mode == Replayer.ReplayingMode.CommandMode)
            {
                if (gameHasFocus == true)
                {
                    UpdateNewStateFromInputDevices();
                   // InputData.UpdateNewState(Keyboard.GetState(), GetZoomedMouseState(controller)); // Mouse.GetState());                 
                }
                else
                {
                    InputData.UpdateKeepOldState();// if we don´t have focus we don´t update input, the old and the new states should be the same
                }
            }   
            else
            {
                ReplayFrame currentFrame = replayer.CurrentReplay.GetCurrentFrame();

                InputData.UpdateNewState(currentFrame.KeyboardState, currentFrame.MouseState);                
            }

            InputData.UpdateEvents(gameTime);
        }

        public InputManager(Controller controller)
        {
            this.controller = controller;
        }

        public void UpdateNewStateFromInputDevices() //Controller controller)
        {
            InputData.UpdateNewState(Keyboard.GetState(), GetZoomedMouseState(controller)); // Mouse.GetState());                 
            
           // Debug.WriteLine(InputData.mouseX + ", " + InputData.mouseY);
        }


        public static MouseState GetZoomedMouseState(Controller controller)
        {
            MouseState state = Mouse.GetState();
            if (controller.ZoomIsActive())
            {
                float scaledX = state.Position.X / controller.ActiveZoomFactor;
                int roundedX = (int)Math.Round(scaledX);

                float scaledY = state.Position.Y / controller.ActiveZoomFactor;
                int roundedY = (int)Math.Round(scaledY);

                MouseState newState = new MouseState(                    
                    roundedX, //(int)(state.Position.X / controller.Options.ZoomFactor), 
                    roundedY, // (int)(state.Position.Y / controller.Options.ZoomFactor), 
                    state.ScrollWheelValue, state.LeftButton, state.MiddleButton, state.RightButton,
                    state.XButton1, state.XButton2);

                return newState;
            }

            return state;
        }

        public void OnMouseSetPos(int x, int y)
        {
            if (isReplaying == false)
            {
                if (controller.ZoomIsActive())
                {
                    float scaledX = x * controller.ActiveZoomFactor;
                    x = (int)Math.Round(scaledX);

                    float scaledY = y * controller.ActiveZoomFactor;
                    y = (int)Math.Round(scaledY);

                    /*
                    x = (int)(x * controller.Options.ZoomFactor);
                    y = (int)(y * controller.Options.ZoomFactor);*/
                }

                Mouse.SetPosition(x,y);
            }
            else
            { 
            
            }
        }

        public void Reset()
        {
            InputData.Reset();

        }

        public void Initialize(Replayer replayer)
        {   
            this.replayer = replayer;
            InputData = new InputData(new SetMousePositionHandler(OnMouseSetPos));

            SetToDefaultMode();
        }

        public void SetToReplayMode()
        {
            isReplaying = true;
        }

        public void SetToDefaultMode()
        {
            isReplaying = false;
        } 
    }
}
