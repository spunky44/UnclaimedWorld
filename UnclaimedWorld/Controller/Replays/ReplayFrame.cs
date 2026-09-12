using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using System.Windows.Forms;
using Microsoft.Xna.Framework.Input;
using System.Runtime.Serialization;

namespace UWGame.Control.Replays
{
    //[Serializable]
    public class ReplayFrame //: ISerializable
    {
        public GameTime GameTime;

        public KeyboardState KeyboardState;
        public MouseState MouseState;

        public ReplayVerificationData RecordedVerificationData = new ReplayVerificationData();

        int replayFormatVersion = 0;

        private static Dictionary<Microsoft.Xna.Framework.Input.Keys, bool> keyStates;

        private List<Microsoft.Xna.Framework.Input.Keys> changedKeys = new List<Microsoft.Xna.Framework.Input.Keys>();

        static ReplayFrame()
        {
            keyStates = new Dictionary<Microsoft.Xna.Framework.Input.Keys, bool>();
            foreach (Microsoft.Xna.Framework.Input.Keys key in Enum.GetValues(typeof(Microsoft.Xna.Framework.Input.Keys)))
            {
                keyStates.Add(key, false);
            }
        }

        public ReplayFrame()
        {

        }

        public ReplayFrame(GameTime gameTime, int mouseX, int mouseY, bool leftButtonDown, bool rightButtonDown, List<Microsoft.Xna.Framework.Input.Keys> changedKeys)
        {
            this.GameTime = gameTime;
            InitMouseState(mouseX, mouseY, leftButtonDown, rightButtonDown);
            this.changedKeys = changedKeys;
        }

        #region Old serialization test code
        /*protected ReplayFrame(SerializationInfo info, StreamingContext context)
        {
            LoadTime(info,context);

            LoadMouseState(info,context);

            LoadKeyboardState(info,context);
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            SaveTime(info,context);
            
            SaveMouseState(info,context);

            SaveKeyboardState(info,context);
          
        }
        private void SaveTime(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("totalTicks", gameTime.TotalGameTime.Ticks);
            info.AddValue("elapsedTicks", gameTime.ElapsedGameTime.Ticks);
        }
        private void LoadTime(SerializationInfo info, StreamingContext context)
        {
            TimeSpan totalTime = new TimeSpan(info.GetInt64("totalTicks"));
            TimeSpan elapsedTime = new TimeSpan(info.GetInt64("elapsedTicks"));

            gameTime = new GameTime(totalTime, elapsedTime);
        }
        private void SaveMouseState(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("mouseX",(short)mouseState.X);
            info.AddValue("mouseY", (short)mouseState.Y);

            info.AddValue("leftButtonDown",mouseState.LeftButton == Microsoft.Xna.Framework.Input.ButtonState.Pressed);
            info.AddValue("rightButtonDown", mouseState.RightButton == Microsoft.Xna.Framework.Input.ButtonState.Pressed);
        }
        private void LoadMouseState(SerializationInfo info, StreamingContext context)
        { 
            short mouseX = info.GetInt16("mouseX");
            short mouseY = info.GetInt16("mouseY");

            bool leftButtonDown = info.GetBoolean("leftButtonDown");
            bool rightButtonDown = info.GetBoolean("rightButtonDown");
        }
        private void SaveKeyboardState(SerializationInfo info, StreamingContext context)
        {
            short numberOfKeysWithChangedState = (short)changedKeys.Count;

            info.AddValue("changedKeyCount", numberOfKeysWithChangedState);
            short index = 0;
            foreach(var key in changedKeys)
            {
                index++;
                info.AddValue("changedKey" + index, (short)key);
            }
        }
        private void LoadKeyboardState(SerializationInfo info, StreamingContext context)
        {
            short numberOfKeysWithChangedState = info.GetInt16("changedKeyCount");
            for (short index = 0; index < numberOfKeysWithChangedState; index++ )
            {
                Microsoft.Xna.Framework.Input.Keys key = (Microsoft.Xna.Framework.Input.Keys)info.GetInt16("changedKey" + index);

                keyStates[key] = !keyStates[key];//If the key has been added to the list that means that the state has changed from the previous frame      
            }

            List<Microsoft.Xna.Framework.Input.Keys> pressedKeys = new List<Microsoft.Xna.Framework.Input.Keys>();
            foreach(var keyState in keyStates)
            {
                if (keyState.Value == true)
                {
                    pressedKeys.Add(keyState.Key);
                }
            }
            
            keyBoardState = new KeyboardState(pressedKeys.ToArray());
        }*/
        #endregion


        public void LoadFrame(BinaryFileReader replayReader)
        {
            //Read all the data that is needed for one frame and advance the readposition so that we know if we have reached the end of the file
            LoadTime(replayReader);

            LoadMouseState(replayReader);

            LoadKeyboardState(replayReader);

            RecordedVerificationData.Load(replayReader);
        }

        private void LoadKeyboardState(BinaryFileReader replayReader)
        {
            List<Microsoft.Xna.Framework.Input.Keys> pressedKeys = new List<Microsoft.Xna.Framework.Input.Keys>();

            bool moreKeyInformation;

            while (true)
            {
                moreKeyInformation = replayReader.ReadBool();

                if (moreKeyInformation == true)
                {
                    Microsoft.Xna.Framework.Input.Keys key = (Microsoft.Xna.Framework.Input.Keys)replayReader.ReadInt();

                    keyStates[key] = !keyStates[key];//If the key has been added to the list that means that the state has changed from the previous frame                    
                }
                else
                {
                    break;
                }
            }

            foreach (var keyState in keyStates)
            {
                if (keyState.Value == true)
                {
                    pressedKeys.Add(keyState.Key);
                }
            }

            KeyboardState = new KeyboardState(pressedKeys.ToArray());
        }

        private void LoadMouseState(BinaryFileReader replayReader)
        {
            int mouseX;
            int mouseY;


            mouseX = replayReader.ReadInt();

            mouseY = replayReader.ReadInt();

            bool leftButtonDown = replayReader.ReadBool();


            bool rightButtonDown = replayReader.ReadBool();
            InitMouseState(mouseX, mouseY, leftButtonDown, rightButtonDown);
        }

        public void InitMouseState(int mouseX, int mouseY, bool leftButtonDown, bool rightButtonDown)
        {
            int scrollWheel = 0;
            Microsoft.Xna.Framework.Input.ButtonState leftButton;
            Microsoft.Xna.Framework.Input.ButtonState middleButton = Microsoft.Xna.Framework.Input.ButtonState.Released;
            Microsoft.Xna.Framework.Input.ButtonState rightButton;
            Microsoft.Xna.Framework.Input.ButtonState xButton1 = Microsoft.Xna.Framework.Input.ButtonState.Released;
            Microsoft.Xna.Framework.Input.ButtonState xButton2 = Microsoft.Xna.Framework.Input.ButtonState.Released;

            if (leftButtonDown)
            {
                leftButton = Microsoft.Xna.Framework.Input.ButtonState.Pressed;
            }
            else
            {
                leftButton = Microsoft.Xna.Framework.Input.ButtonState.Released;
            }

            if (rightButtonDown)
            {
                rightButton = Microsoft.Xna.Framework.Input.ButtonState.Pressed;
            }
            else
            {
                rightButton = Microsoft.Xna.Framework.Input.ButtonState.Released;
            }

            MouseState = new MouseState(mouseX, mouseY, scrollWheel, leftButton, middleButton, rightButton, xButton1, xButton2);
        }

        private void LoadTime(BinaryFileReader replayReader)
        {
            TimeSpan totalTime = new TimeSpan(replayReader.ReadLong());//read time

            TimeSpan elapsedTime = new TimeSpan(replayReader.ReadLong());

            GameTime = new GameTime(totalTime, elapsedTime);
        }


    }
}
