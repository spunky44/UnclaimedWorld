using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using UWGame.SimSide.Entities;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Systems.Triggers;
using System.Diagnostics;
using System.Collections;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Systems.TimeSlicing;
using UWGame.SimSide.Scenarios;
using UWGame.ClientSide.Renderables;
using UWGame.Control;
using UWGame.SimSide.Allegiances.Statistics;
using UWGame.SimSide.XmlCollections;
using UWGame.SimSide.Processes;
using UWGame.SimSide.Overland;
using UWGame.SimSide.Overland.Locations;
using UWGame.SimSide.Entities.Containers;
using System.Runtime;

namespace UWGame.SimSide.Snapshots 
{
                      
    /// <summary>
    /// This is not the implementation of ISnapshot, but the main class to perform Save, Load and CRC
    /// </summary>              
    public class Snapshotter
    {
        CRC CRC = new CRC();
        BinaryWriter m_writer = null;
        BinaryReader m_reader = null;

        static bool snapshotting;

        /// <summary>
        /// DON'T test this outside DoSnapshot, for instance inside constructors. The value is meaningless.
        /// </summary>
        public Mode mode;

        #region Self verification

        /// <summary>
        /// to track field types in each ISnapshot class - we should omit inner Do calls...
        /// unfortunately we can only verify field types, not field names...
        /// </summary>
        Dictionary<Type, SnapshotClassVerification> snapshotClasses;

        SnapshotClassVerification snapshotClassBeingVerified;

        /// <summary>
        /// the stack will often contain null values, meaning that we do not need to verify that item
        /// </summary>
        Stack<SnapshotClassVerification> snapshotClassesBeingVerified = new Stack<SnapshotClassVerification>();

        #endregion


    //    Dictionary<Type, Type> loadPostProcessCallPending = new Dictionary<Type, Type>();

       // Dictionary<Type, HashSet<ISnapshot>> allSnapshottedISnapshots = new Dictionary<Type, HashSet<ISnapshot>>();
        Dictionary<Type, int> allSnapshottedISnapshots = new Dictionary<Type, int>();



        public enum LogPriority
        {
            low,
            med,
            high,
            infinity
        }

        static LogPriority m_logPriority = LogPriority.low; // output amount

        public enum Mode ///////make private
        {
            Save,
            Load,
            CRC 
        }

       
        /// <summary>
        /// Here is the workflow for changing the snapshot version of a class:
        /// 
        /// When programmer wants to add, remove members or change the type of a member
        /// he must keep in mind that older (previous version) save games will either
        /// be missing the new data, or will have serialized data that are of the wrong
        /// type. The solution is to increment the version (a constant declared at the top of DoSnapshot())
        /// see Sim.DoSnapshot as an example. 
        /// Then, add code that branches on version in the DoSnapshot method for the affected
        /// member. See the AllMonkeys example in Sim. Be sure to test the new code
        /// with an old save file, to make sure it survived a load... and also test by saving and
        /// loading with the new version.
        /// </summary>
        public enum Version : uint
        {
            Original = 1
        }

        /// <summary>
        /// todo: test this flag in all empty ctors... that ctor will be invoked during load
        /// </summary>
        static public bool IsSnapshotting
        {
            get
            {
                return snapshotting;
            }

            set
            {
                snapshotting = value;
            }
        }

        //CTOR
        public Snapshotter()
        {
            Log("Snapshot ctor", LogPriority.high);

            TypeAndDoMappings.Add(typeof(Point), 
                new Func<object, object>( o => DoPoint((Point)o, false)));

            TypeAndDoMappings.Add(typeof(string),
                new Func<object, object>(o => DoString((string)o, false)));

            TypeAndDoMappings.Add(typeof(Vector2),
                new Func<object, object>(o => DoVector2((Vector2)o, false)));

            TypeAndDoMappings.Add(typeof(Vector3),
                new Func<object, object>(o => DoVector3((Vector3)o, false)));

            TypeAndDoMappings.Add(typeof(WorldLocation),
               new Func<object, object>(o => DoWorldLocation((WorldLocation)o, false)));

            TypeAndDoMappings.Add(typeof(TimeSpan),
               new Func<object, object>(o => DoTimeSpan((TimeSpan)o, false)));

            TypeAndDoMappings.Add(typeof(ToolTypeCombinationID?),
               new Func<object, object>(o => DoEnumNullable((ToolTypeCombinationID?)o, false)));

            TypeAndDoMappings.Add(typeof(Color),
              new Func<object, object>(o => DoColor((Color)o, false)));

            TypeAndDoMappings.Add(typeof(EntityAndRoot),
              new Func<object, object>(o => DoEntityAndRoot((EntityAndRoot)o, false)));
            

        }


        bool verifyCompleteness;

        //  ____                    
        // / ___|  __ ___   _____   
        // \___ \ / _` \ \ / / _ \  
        //  ___) | (_| |\ V /  __/  
        // |____/ \__,_| \_/ \___| 
        /// <summary>
        /// Save should always be followed by a call to Load()!
        /// </summary>
        /// <param name="writer"></param>
        public void Save(BinaryWriter writer, SnapshotHeader header, bool verifyCompleteness = true, bool snapShotHeader = false, bool snapShotBody = false)  
        {
            this.verifyCompleteness = verifyCompleteness;

            if (!VerifyParameterlessConstructorsExist())
            {
                // bail out quickly:
                throw new Exception("Missing parameterless constructor(s). See the output log...");
            }


            mode = Mode.Save;
            m_writer = writer;
            m_reader = null;
          
            snapshotting = true;

            snapshotClassBeingVerified = null;
            snapshotClasses = new Dictionary<Type, SnapshotClassVerification>();

            allSnapshottedISnapshots.Clear();
           
            //first write the header info outside the sim data:
            //DoHeader(Mode.Save, null, m_writer, header);
            if (snapShotHeader)
            {
                DoISnapshot(header, verifyCompleteness);
            }
            
          //  The.Sim = (Sim)DoISnapshot(The.Sim, verifyCompleteness);
            if (snapShotBody)
            {
                DoISnapshot(The.Sim, verifyCompleteness); // don't replace the Sim instance...
            }
         
            snapshotting = false;
            m_writer = null;
        }


        public event Action PreLoadPostProcess;


        //  _                    _  
        // | |    ___   __ _  __| | 
        // | |   / _ \ / _` |/ _` | 
        // | |__| (_) | (_| | (_| | 
        // |_____\___/ \__,_|\__,_| 
        /// <summary>
        /// there are 4 load cases to test:
        /// 
        /// 1. Title screen Load
        /// 2. Ingame Load, same scenario
        /// 3. Ingame Load, different scenario
        /// 4. Ingame Save (ends with a Load)
        /// </summary>
        /// <param name="reader"></param>
        public void Load(BinaryReader reader) 
        {
            this.verifyCompleteness = false; // only works during Save

            mode = Mode.Load;
            m_reader = reader;
            m_writer = null;
           
            snapshotting = true;

            snapshotClassBeingVerified = null;
            snapshotClasses = new Dictionary<Type, SnapshotClassVerification>();
        
            SnapshotHeader header = (SnapshotHeader)DoISnapshot<SnapshotHeader>(null); // won't be used, but still needed to position the reader 

            //we replace the Sim instance... we still want to keep GameData and ScreenManager.
            Controller controller = The.Sim.Controller;

            //*** start of lock on Screens collection:

            int index = controller.GetIndexOfScreen(The.Sim);

            lock (controller.UpdateScreensLock)
            {
                controller.RemoveFromList(The.Sim); // NEW

                UWGame.SimSide.Sim.StartGameMode startGameMode = The.Sim.startGameMode;

                The.Sim.FreeMemoryBeforeLoad(); // also sets Sim to null! Call this on the old Sim to free up some memory before loading the new Sim! The old Sim is still in memory at this point.


                // compact large object heap   
                GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
                GC.Collect();
                GC.WaitForPendingFinalizers();


                The.Sim = (Sim)DoISnapshot(The.Sim, verifyCompleteness); // how long does this take... can we draw anything but a black screen here?

                // wait for signal:
                controller.AddScreen(The.Sim, index); // this sets ScreenManager too


                The.Client.Controller.RecreateClientAfterLoad(); 

           
                // Sim will place settings data in the new Client 
                // the Sim needs to load completely before the Client can start to Draw/Update
                header.LoadPostProcess(this); // for completeness...
                The.Sim.LoadPostProcess(this); // here, classes should recreate their client representations (Renderable etc.)
                The.Sim.StartGameParams = header.StartGameParams; // replace the old version... this resets IsSnapshotted...
                The.Sim.IsSnapshotted = false; // reset for next save

                The.Sim.beginRunProgress = Sim.ProgressAfterSaveLoad; // beginRunProgress; // when loading: transfer the progress state to the new Sim instance in The.Sim !
                The.Sim.startGameMode = startGameMode;

              //  The.Client.Controller.RecreateClientAfterLoad(); // some gui stuff needs the main thread...

            }//*** end of lock on Screens collection
            
            VerifyLoadPostProcessCallsComplete();

            allSnapshottedISnapshots.Clear();

            snapshotting = false;
            m_reader = null;

            Sim.LoadIsFinished = true; // NEW
        }

        public SnapshotHeader LoadHeader(BinaryReader reader)
        {
            this.verifyCompleteness = false; 

            mode = Mode.Load;
            m_reader = reader;
            m_writer = null;

            snapshotting = true;

            snapshotClassBeingVerified = null;
            snapshotClasses = new Dictionary<Type, SnapshotClassVerification>();

            SnapshotHeader header = (SnapshotHeader)DoISnapshot<SnapshotHeader>(null);

            header.LoadPostProcess(this);

            snapshotting = false;
            m_reader = null;

            return header;

        }


        //   ___  ____   ___       
        //  / __||  _ \ / __|      
        // | |   | |_) | |          
        // | |__ |  _ <| |__       
        //  \___||_| \_\\___| 
        public uint DoCRC()
        {
            mode = Mode.CRC;
            CRC = new CRC();//a fresh one
            //s_caller = null;//the first class to snapshot is always allowed
            snapshotting = true;
            The.Sim.DoSnapshot( The.Snapshotter );
            snapshotting = false;

            Snapshotter.Log("CRC value is " + CRC.GetCurrentCRCValue + " <------------- LOOK", Snapshotter.LogPriority.infinity);
            return CRC.GetCurrentCRCValue;
        }


       


        //  ____        _          
        // |  _ \  ___ ( )___       
        // | | | |/ _ \ // __|      
        // | |_| | (_) | \__ \      
        // |____/ \___/  |___/      

        //this will catch all the types not yet implemented
        //we do not want to catch these with reflection all the time
        //since that will slow down the crc snapshot every frame

        public System.Object DoUnknownObject(System.Object a, Type type = null, bool verifyField = true)
        {
            if (a is ISnapshot) // instance test - this will catch a ThreatMap in an IMap collection
                return DoISnapshot((ISnapshot)a, verifyField); 

           /* if (a is Enum)
                return DoEnum((Enum)a, verifyField);
            */

            if (mode == Mode.Save)
            {
                if (a != null)
                {
                    throw new Exception("Snapshot of type: " + a.GetType() + " not supported...");                    
                }
                else if (type != null)
                {
                    throw new Exception("Snapshot of type: " + type.Name + " not supported...");                    
                }                
            }

            /*
            if (a != null)
            {
                Log("Uh oh, the type, " + a.GetType() + " is not supported in a Do() method.");
            }
            else if (type != null)
            {
                Log("Uh oh, the type " + type.Name + " is not supported in a Do() method.");
            }
            else
            {
                Log("Uh oh, the type UNKNOWN is not supported in a Do() method.");
            }*/

            return a;
        }


        public Version DoVersion(Version version)
        {
            //verification code
          //  fieldsBeingVerified.Remove(typeof(Version).Name);

            if (mode == Mode.Load)
            {
                string typeName = m_reader.ReadString();
                Debug.Assert(typeName == "Version", "Expected a Version, but read a " + typeName);
                return (Version)m_reader.ReadUInt32();
            }
            else if (mode == Mode.Save)
            {
                m_writer.Write("Version");
                m_writer.Write(Convert.ToUInt32(version));
               // Log("No save Snapshot.Do impl for Version", LogPriority.low);
            }
            else if (mode == Mode.CRC)
            {
                CRC.AddData(BitConverter.GetBytes(Convert.ToUInt32(version))); //version has underlying type of uint32
            }

            return version;
        }

        public float DoFloat(float val, bool verifyField = true)
        {
            if (verifyField)
                VerifyField(typeof(float));
            

            if (mode == Mode.Load)
            {
                string typeName = m_reader.ReadString();
                Debug.Assert(typeName == "float","Expected a float, but read a " + typeName);
                return m_reader.ReadSingle();
            }
            else if (mode == Mode.Save)
            {
                m_writer.Write("float");
                m_writer.Write(val);
            }
            else if (mode == Mode.CRC)
            {
                CRC.AddData(BitConverter.GetBytes(val));
            }

            return val;
        }

        public T DoGameData<T>(T gameData) where T : IGameData
        {
            Type objectType = typeof(T);
            return (T)DoGameData(objectType, gameData);
        }

        public T DoGameData<T>(T gameData, bool verifyField = true) where T : IGameData
        {
            Type objectType = typeof(T);
            return (T)DoGameData(objectType, gameData, verifyField);
        }

       // public T DoGameData<T>(T gameData) where T : IGameData
        public object DoGameData(Type objectType, IGameData gameData, bool verifyField = true)
        {
            if (verifyField)
                VerifyField(objectType); // use derived or interface type??
            
         //   Type objectType = typeInfo typeof(T);
            string thisName = objectType.Name.ToLowerInvariant();

            if (mode == Mode.Load)
            {
                string readTypeName = m_reader.ReadString();
                if (readTypeName.Equals("null"))
                {
                   // return default(T);
                    return null;
                }               
              
                Debug.Assert(readTypeName == thisName, string.Format("Expected a {0}, but read a {1}", thisName, readTypeName));               

                string keyName = m_reader.ReadString(); 

                IGameData data = GameData.Instance.GetGameData(objectType, keyName);

                Debug.Assert(data != null, "Collection or key does not exist...");

               // return (T)data;
                return data;
            }
            else if (mode == Mode.Save)
            {
                if (gameData == null)
                {
                    m_writer.Write("null");
                    //m_writer.Write(0f);

                   // return default(T);
                    return null;
                }
                else
                {
                    m_writer.Write(thisName); 
                    m_writer.Write(gameData.KeyName);
                }
            }
            else if (mode == Mode.CRC)
                CRC.AddData(System.Text.Encoding.Unicode.GetBytes(gameData.KeyName)); // include class name???
            //   CRC.AddData(BitConverter.GetBytes(gameData));

            return gameData;

        }

        public float? DoFloatNullable(float? val, bool verifyField = true)
        {

            if (verifyField)
                VerifyField(typeof(float?));

            if (mode == Mode.Load)
            {
                string typeName = m_reader.ReadString();
                
                if (typeName == "null")
                    return null;

                Debug.Assert(typeName == "float?", "Expected a float?, but read a " + typeName);

                float value = m_reader.ReadSingle();

             
                return value;
            }
            else if (mode == Mode.Save)
            {
                if (val.HasValue)
                {
                    m_writer.Write("float?");
                    m_writer.Write(val.Value);
                }
                else
                {
                    m_writer.Write("null");
                  //  m_writer.Write(0f);
                }
            }
            else if (mode == Mode.CRC)
            {
                if (val.HasValue)
                    CRC.AddData(BitConverter.GetBytes(val.Value));
                else
                    CRC.AddData(BitConverter.GetBytes(0));
            }

            return val;
        }

        public double DoDouble(double val, bool verifyField = true)
        {           
            if (verifyField)
                VerifyField(typeof(double));


            if (mode == Mode.Load)
            {
                string typeName = m_reader.ReadString();
                Debug.Assert(typeName == "double", "Expected a double, but read a " + typeName);
                return m_reader.ReadDouble();
            }
            else if (mode == Mode.Save)
            {
                m_writer.Write("double");
                m_writer.Write(val);
            }
            else if (mode == Mode.CRC)
            {
                CRC.AddData(BitConverter.GetBytes(val));
            }

            return val;
        }

        public decimal DoDecimal(decimal val, bool verifyField = true)
        {
            if (verifyField)
                VerifyField(typeof(decimal));


            if (mode == Mode.Load)
            {
                string typeName = m_reader.ReadString();
                Debug.Assert(typeName == "decimal", "Expected a decimal, but read a " + typeName);
                return m_reader.ReadDecimal();
            }
            else if (mode == Mode.Save)
            {
                m_writer.Write("decimal");
                m_writer.Write(val);
            }
            else if (mode == Mode.CRC)
            {
                // not supported..?
                //CRC.AddData(BitConverter.GetBytes(val));
            }

            return val;
        }

        public decimal? DoDecimalNullable(decimal? val, bool verifyField = true)
        {
            if (verifyField)
                VerifyField(typeof(decimal?));

            if (mode == Mode.Load)
            {
                string typeName = m_reader.ReadString();

                if (typeName == "null")
                    return null;

                Debug.Assert(typeName == "decimal?", "Expected a decimal?, but read a " + typeName);

                decimal value = m_reader.ReadDecimal();

                return value;
            }
            else if (mode == Mode.Save)
            {
                if (val.HasValue)
                {
                    m_writer.Write("decimal?");
                    m_writer.Write(val.Value);
                }
                else
                {
                    m_writer.Write("null");
                    // m_writer.Write(false);
                }
            }
            else if (mode == Mode.CRC)
            {
                // not supported?
               /* if (val.HasValue)
                    CRC.AddData(BitConverter.GetBytes(val.Value));
                else
                    CRC.AddData(BitConverter.GetBytes(0.0));*/
            }

            return val;
        }


        public DateAndTime.TimeDateYear? DoTimeDateYearNullable(DateAndTime.TimeDateYear? val, bool verifyField = true)
        {
            if (verifyField)
                VerifyField(typeof(DateAndTime.TimeDateYear?));

            if (mode == Mode.Load)
            {
                string typeName = m_reader.ReadString();

                if (typeName == "null")
                    return null;

                Debug.Assert(typeName == "TimeDateYear?", "Expected a TimeDateYear?, but read a " + typeName);

                DateAndTime.TimeDateYear returnValue = ReadTimeDateYear();

                return returnValue;
            }
            else if (mode == Mode.Save)
            {
                if (val.HasValue)
                {
                    m_writer.Write("TimeDateYear?");

                    val = WriteTimeDateYear(val.Value);
                }
                else
                {
                    m_writer.Write("null");
                    // m_writer.Write(false);
                }
            }
            else if (mode == Mode.CRC)
            {
                // not supported?
               /* if (val.HasValue)
                    CRC.AddData(BitConverter.GetBytes(val.Value));
                else
                    CRC.AddData(BitConverter.GetBytes(0.0));*/
            }

            return val;
        }

        public DateAndTime.TimeDateYear DoTimeDateYear(DateAndTime.TimeDateYear value, bool verifyField = true)
        {
            if (verifyField)
                VerifyField(typeof(DateAndTime.TimeDateYear));

            if (mode == Mode.Load)
            {
                DateAndTime.TimeDateYear returnValue = ReadTimeDateYear();
                return returnValue;

            }
            else if (mode == Mode.Save)
            {
                value = WriteTimeDateYear(value);

            }
          /*  else if (mode == Mode.CRC) // TODO
            {
                CRC.AddData(BitConverter.GetBytes(val.Ticks));

            }*/

            return value;

        }

        private DateAndTime.TimeDateYear WriteTimeDateYear(DateAndTime.TimeDateYear value)
        {
            DoDouble(value.TimeOfDay, false);
            DoInt32(value.Day, false);
            DoInt32(value.Year, false);
            return value;
        }

        private DateAndTime.TimeDateYear ReadTimeDateYear()
        {
            double timeOfDay = DoDouble(0, false);
            int day = DoInt32(0, false);
            int year = DoInt32(0, false);

            DateAndTime.TimeDateYear returnValue = new DateAndTime.TimeDateYear(timeOfDay, day, year);

            return returnValue;
        }

        public DateTime DoDateTime(DateTime val, bool verifyField = true)
        {
            if (verifyField)
                VerifyField(typeof(DateTime));

            if (mode == Mode.Load)
            {
             
                long ticks = DoInt64(0);

                return new DateTime(ticks);

            }
            else if (mode == Mode.Save)
            {
                DoInt64(val.Ticks);

            }
            else if (mode == Mode.CRC)
            {
                CRC.AddData(BitConverter.GetBytes(val.Ticks));

            }

            return val;

        }

        public TimeSpan? DoTimeSpanNullable(TimeSpan? val, bool verifyField = true)
        {
            if (verifyField)
                VerifyField(typeof(byte?));

            if (mode == Mode.Load)
            {
                string typeName = m_reader.ReadString();

                if (typeName == "null")
                    return null;

                Debug.Assert(typeName == "TimeSpan?", "Expected a TimeSpan?, but read a " + typeName);

                long ticks = DoInt64(0);

                return new TimeSpan(ticks);
            }
            else if (mode == Mode.Save)
            {
                if (val.HasValue)
                {
                    m_writer.Write("TimeSpan?");

                    DoInt64(val.Value.Ticks);
                }
                else
                {
                    m_writer.Write("null");
                    // m_writer.Write(false);
                }
            }
            else if (mode == Mode.CRC)
            {
                if (val.HasValue)
                    CRC.AddData(BitConverter.GetBytes(val.Value.Ticks));
                else
                    CRC.AddData(BitConverter.GetBytes(false));
            }

            return val;


        }

        public TimeSpan DoTimeSpan(TimeSpan val, bool verifyField = true)
        {           
            if (verifyField)
                VerifyField(typeof(TimeSpan));

            if (mode == Mode.Load)
            {
               // string typeName = m_reader.ReadString();
               // Debug.Assert(typeName == "TimeSpan", "Expected a TimeSpan, but read a " + typeName);

                long ticks = DoInt64(0);

                return new TimeSpan(ticks);

            }
            else if (mode == Mode.Save)
            {
                DoInt64(val.Ticks);

            }
            else if (mode == Mode.CRC)
            {
                CRC.AddData(BitConverter.GetBytes(val.Ticks));

            }

            return val;
        }

        public bool DoBool(bool val, bool verifyField = true)
        {
            //verification code
            if (verifyField)
                VerifyField(typeof(bool));


            if (mode == Mode.Load)
            {
                string typeName = m_reader.ReadString();
                Debug.Assert(typeName == "bool", "Expected a bool, but read a " + typeName);
                return m_reader.ReadBoolean();
            }
            else if (mode == Mode.Save)
            {
                m_writer.Write("bool");
                m_writer.Write(val);
            }
            else if (mode == Mode.CRC)
                CRC.AddData(BitConverter.GetBytes(val));
            
            return val;
        }


        public byte? DoByteNullable(byte? val, bool verifyField = true)
        {
           
            if (verifyField)
                VerifyField(typeof(byte?));

            if (mode == Mode.Load)
            {
                string typeName = m_reader.ReadString();
             
                if (typeName == "null")
                    return null;

                Debug.Assert(typeName == "byte?", "Expected a byte?, but read a " + typeName);

                byte value = m_reader.ReadByte();

                return value;
            }
            else if (mode == Mode.Save)
            {
                if (val.HasValue)
                {
                    m_writer.Write("byte?");
                    m_writer.Write(val.Value);
                }
                else
                {
                    m_writer.Write("null");
                   // m_writer.Write(false);
                }
            }
            else if (mode == Mode.CRC)
            {
                if (val.HasValue)
                    CRC.AddData(BitConverter.GetBytes(val.Value));
                else
                    CRC.AddData(BitConverter.GetBytes(false));
            }

            return val;
        }

        public byte DoByte(byte val, bool verifyField = true)
        {
            //verification code
            if (verifyField)
                VerifyField(typeof(byte));
          
            // save space here by skipping type names?? safe???

            if (mode == Mode.Load)
            {
                //string typeName = m_reader.ReadString();
                //Debug.Assert(typeName == "bool", "Expected a bool, but read a " + typeName);
                return m_reader.ReadByte();
            }
            else if (mode == Mode.Save)
            {
                // m_writer.Write("bool");
                m_writer.Write(val);
            }
            else if (mode == Mode.CRC)            
                CRC.AddData(val);

            return val;
        }

        public bool? DoBoolNullable(bool? val, bool verifyField = true)
        {
           
            if (verifyField)
                VerifyField(typeof(bool?));

            if (mode == Mode.Load)
            {
                string typeName = m_reader.ReadString();
             
                if (typeName == "null")
                    return null;

                Debug.Assert(typeName == "bool?", "Expected a bool?, but read a " + typeName);

                bool value = m_reader.ReadBoolean();

                return value;
            }
            else if (mode == Mode.Save)
            {
                if (val.HasValue)
                {
                    m_writer.Write("bool?");
                    m_writer.Write(val.Value);
                }
                else
                {
                    m_writer.Write("null");
                   // m_writer.Write(false);
                }
            }
            else if (mode == Mode.CRC)
            {
                if (val.HasValue)
                    CRC.AddData(BitConverter.GetBytes(val.Value));
                else
                    CRC.AddData(BitConverter.GetBytes(false));
            }

            return val;
        }


        public double? DoDoubleNullable(double? val, bool verifyField = true)
        {
            if (verifyField)
                VerifyField(typeof(double?));
          
            if (mode == Mode.Load)
            {
                string typeName = m_reader.ReadString();

                if (typeName == "null")
                    return null;

                Debug.Assert(typeName == "double?", "Expected a double?, but read a " + typeName);

                double value = m_reader.ReadDouble();
               
                return value;
            }
            else if (mode == Mode.Save)
            {
                if (val.HasValue)
                {
                    m_writer.Write("double?");
                    m_writer.Write(val.Value);
                }
                else
                {
                    m_writer.Write("null");
                   // m_writer.Write(false);
                }
            }
            else if (mode == Mode.CRC)
            {
                if (val.HasValue)
                    CRC.AddData(BitConverter.GetBytes(val.Value));
                else
                    CRC.AddData(BitConverter.GetBytes(0.0));
            }

            return val;
        }

        public ushort? DoUInt16Nullable(ushort? val, bool verifyField = true)
        {
            if (verifyField)
                VerifyField(typeof(ushort?));

            if (mode == Mode.Load)
            {
                string typeName = m_reader.ReadString();

                if (typeName == "null")
                    return null;

                Debug.Assert(typeName == "ushort?", "Expected a ushort?, but read a " + typeName);

                ushort value = m_reader.ReadUInt16();

                return value;
            }
            else if (mode == Mode.Save)
            {
                if (val.HasValue)
                {
                    m_writer.Write("ushort?");
                    m_writer.Write(val.Value);
                }
                else
                {
                    m_writer.Write("null");
                    // m_writer.Write(false);
                }
            }
            else if (mode == Mode.CRC)
            {
                if (val.HasValue)
                    CRC.AddData(BitConverter.GetBytes(val.Value));
                else
                    CRC.AddData(BitConverter.GetBytes(0.0));
            }

            return val;
        }

        public ushort DoUInt16(ushort val, bool verifyField = true)
        {
            if (verifyField)
                VerifyField(typeof(ushort));
          
            if (mode == Mode.Load)
            {
                string typeName = m_reader.ReadString();
                Debug.Assert(typeName == "ushort", "Expected a ushort, but read a " + typeName);
                return m_reader.ReadUInt16();
            }
            else if (mode == Mode.Save)
            {
                m_writer.Write("ushort");
                m_writer.Write(val);
            }
            else if (mode == Mode.CRC)
                CRC.AddData(BitConverter.GetBytes(val));

            return val;
        }

        public Int32 DoInt32(Int32 val, bool verifyField = true)
        {
            if (verifyField)
                VerifyField(typeof(Int32));
          
            if (mode == Mode.Load)
            {
                string typeName = m_reader.ReadString();
                Debug.Assert(typeName == "int", "Expected a int, but read a " + typeName);
                return m_reader.ReadInt32();
            }
            else if (mode == Mode.Save)
            {
                m_writer.Write("int");
                m_writer.Write(val);
            }
            else if (mode == Mode.CRC)
                CRC.AddData(BitConverter.GetBytes(val));

            return val;
        }

        public Int32? DoInt32Nullable(Int32? val, bool verifyField = true)
        {
            if (verifyField)
                VerifyField(typeof(Int32?));

            if (mode == Mode.Load
                || mode == Mode.Save)
            {
                return DoInt32NullableStatic(mode, m_writer, m_reader, val);
            }
            else
            {
                if (val.HasValue)
                    CRC.AddData(BitConverter.GetBytes(val.Value));
                else
                    CRC.AddData(BitConverter.GetBytes(0));

                return val;
            }

           /* if (mode == Mode.Load)
            {
                string typeName = m_reader.ReadString();
              
                if (typeName == "null")
                    return null;

                Debug.Assert(typeName == "Int32?", "Expected a Int32?, but read a " + typeName);

                Int32 value = m_reader.ReadInt32();

                return value;
            }

            if (mode == Mode.Save)
            {
                if (val.HasValue)
                {
                    m_writer.Write("Int32?");
                    m_writer.Write(val.Value);
                }
                else
                {
                    m_writer.Write("null");
                   // m_writer.Write(0L);
                }
            }
            else if (mode == Mode.CRC)
            {
               
            }*/

          

        }

        public static Int32? DoInt32NullableStatic(Mode mode, BinaryWriter writer, BinaryReader reader, Int32? value)
        {
            if (mode == Mode.Load)
            {
                string typeName = reader.ReadString();

                if (typeName == "null")
                    return null;

                Debug.Assert(typeName == "Int32?", "Expected a Int32?, but read a " + typeName);

                Int32 readValue = reader.ReadInt32();

                return readValue;
            }
            else //if (mode == Mode.Save)
            {
                if (value.HasValue)
                {
                    writer.Write("Int32?");
                    writer.Write(value.Value);
                }
                else
                {
                    writer.Write("null");
                    // m_writer.Write(0L);
                }

                return value;
            }

        }

        public Int64 DoInt64(Int64 val, bool verifyField = true)
        {
            if (verifyField)
                VerifyField(typeof(Int64));
          
            if (mode == Mode.Load)
            {
                string typeName = m_reader.ReadString();
                Debug.Assert(typeName == "long", "Expected a long, but read a " + typeName);
                return m_reader.ReadInt64();
            }
            else if (mode == Mode.Save)
            {
                m_writer.Write("long");
                m_writer.Write(val);
            }
            else if (mode == Mode.CRC)
                CRC.AddData(BitConverter.GetBytes(val));

            return val;
        }

        public ulong DoUInt64(ulong val, bool verifyField = true)
        {
            if (verifyField)
                VerifyField(typeof(ulong));

            if (mode == Mode.Load)
            {
                string typeName = m_reader.ReadString();
                Debug.Assert(typeName == "ulong", "Expected a ulong, but read a " + typeName);
                return m_reader.ReadUInt64();
            }
            else if (mode == Mode.Save)
            {
                m_writer.Write("ulong");
                m_writer.Write(val);
            }
            else if (mode == Mode.CRC)
                CRC.AddData(BitConverter.GetBytes(val));

            return val;
        }

        public Vector3 DoVector3(Vector3 val, bool verifyField = true)
        {
            if (verifyField)
                VerifyField(typeof(Vector3));
         
            Vector3 vec = val;
            vec.X = DoFloat(vec.X);
            vec.Y = DoFloat(vec.Y);
            vec.Z = DoFloat(vec.Z);
            return vec;
        }

        public Matrix DoMatrix(Matrix val, bool verifyField = true)
        {
            if (verifyField)
                VerifyField(typeof(Matrix));

            Matrix matrix = val;
            matrix.M11 = DoFloat(matrix.M11);
            matrix.M11 = DoFloat(matrix.M12);
            matrix.M11 = DoFloat(matrix.M13);
            matrix.M11 = DoFloat(matrix.M14);

            matrix.M11 = DoFloat(matrix.M21);
            matrix.M11 = DoFloat(matrix.M22);
            matrix.M11 = DoFloat(matrix.M23);
            matrix.M11 = DoFloat(matrix.M24);

            matrix.M11 = DoFloat(matrix.M31);
            matrix.M11 = DoFloat(matrix.M32);
            matrix.M11 = DoFloat(matrix.M33);
            matrix.M11 = DoFloat(matrix.M34);

            matrix.M11 = DoFloat(matrix.M41);
            matrix.M11 = DoFloat(matrix.M42);
            matrix.M11 = DoFloat(matrix.M43);
            matrix.M11 = DoFloat(matrix.M44);

            return matrix;
        }

        public WorldLocation DoWorldLocation(WorldLocation val, bool verifyField = true)
        {
            if (verifyField)
                VerifyField(typeof(WorldLocation));
         

            WorldLocation vec = val;
            vec.X = DoFloat(vec.X);
            vec.Y = DoFloat(vec.Y);
            vec.Z = DoFloat(vec.Z);
            return vec;
        }

        public WorldLocation? DoWorldLocationNullable(WorldLocation? val, bool verifyField = true)
        {
            if (verifyField)
                VerifyField(typeof(WorldLocation?));
         
            if (mode == Mode.Load)
            {
                string typeName = m_reader.ReadString();
               
                if (typeName == "null")
                    return null;

                Debug.Assert(typeName == "WorldLocation?", "Expected a WorldLocation?, but read a " + typeName);

                float X = m_reader.ReadSingle();
                float Y = m_reader.ReadSingle();
                float Z = m_reader.ReadSingle();

                return new WorldLocation(X, Y, Z);
            }
            else if (mode == Mode.Save)
            {
                if (val.HasValue)
                {
                    m_writer.Write("WorldLocation?");
                    m_writer.Write(val.Value.X);
                    m_writer.Write(val.Value.Y);
                    m_writer.Write(val.Value.Z);
                }
                else
                {
                    m_writer.Write("null");                
                }
            }
            else if (mode == Mode.CRC)
            {
                if (val.HasValue)
                {
                    DoFloat(val.Value.X);
                    DoFloat(val.Value.Y);
                    DoFloat(val.Value.Z);
                }
                else
                    CRC.AddData(BitConverter.GetBytes(0));
            }

            return val;

        }


        public Vector4 DoVector4(Vector4 val, bool verifyField = true)
        {
            if (verifyField)
                VerifyField(typeof(Vector4));
         
            Vector4 vec = val;
            vec.X = DoFloat(vec.X);
            vec.Y = DoFloat(vec.Y);
            vec.Z = DoFloat(vec.Z);
            vec.W = DoFloat(vec.W);
            return vec;
        }

        public Vector3? DoVector3Nullable(Vector3? val, bool verifyField = true)
        {
            if (verifyField)
                VerifyField(typeof(Vector3?));
          
            if (mode == Mode.Load)
            {
                string typeName = m_reader.ReadString();
               
                if (typeName == "null")
                    return null;

                Debug.Assert(typeName == "Vector3?", "Expected a Vector3?, but read a " + typeName);

                float X = m_reader.ReadSingle();
                float Y = m_reader.ReadSingle();
                float Z = m_reader.ReadSingle();

                return new Vector3(X,Y,Z);
            }
            else if (mode == Mode.Save)
            {
                if (val.HasValue)
                {
                    m_writer.Write("Vector3?");
                    m_writer.Write(val.Value.X);
                    m_writer.Write(val.Value.Y);
                    m_writer.Write(val.Value.Z);
                }
                else
                {
                    m_writer.Write("null");                
                }
            }
            else if (mode == Mode.CRC)
            {
                if (val.HasValue)
                {
                    DoFloat(val.Value.X);
                    DoFloat(val.Value.Y);
                    DoFloat(val.Value.Z);
                }
                else
                    CRC.AddData(BitConverter.GetBytes(0));
            }

            return val;
        }

        public Vector2 DoVector2(Vector2 val, bool verifyField = true)
        {
            if (verifyField)
                VerifyField(typeof(Vector2));
            
            Vector2 vec = val;
            vec.X = DoFloat(vec.X);
            vec.Y = DoFloat(vec.Y);
            return vec;
        }

        public Vector2? DoVector2Nullable(Vector2? val, bool verifyField = true)
        {
            if (verifyField)
                VerifyField(typeof(Vector2?));
          
            if (mode == Mode.Load)
            {
                string typeName = m_reader.ReadString();
            
                if (typeName == "null")
                    return null;

                Debug.Assert(typeName == "Vector2?", "Expected a Vector2?, but read a " + typeName);

                float X = m_reader.ReadSingle();
                float Y = m_reader.ReadSingle();
                return new Vector2(X, Y);
            }
            else if (mode == Mode.Save)
            {
                if (val.HasValue)
                {
                    m_writer.Write("Vector2?");
                    m_writer.Write(val.Value.X);
                    m_writer.Write(val.Value.Y);
                }
                else
                {
                    m_writer.Write("null");
                 /*   m_writer.Write(0f);
                    m_writer.Write(0f);*/
                }
            }
            else if (mode == Mode.CRC)
            {
                if (val.HasValue)
                {
                    DoFloat(val.Value.X);
                    DoFloat(val.Value.Y);
                }
                else
                    CRC.AddData(BitConverter.GetBytes(0));
            }

            return val;
        }

        public System.Drawing.RectangleF DoRectangleF(System.Drawing.RectangleF val, bool verifyField = true)
        {
            if (verifyField)
                VerifyField(typeof(System.Drawing.RectangleF));
           
            System.Drawing.RectangleF rect = val;
            rect.Height = DoFloat(rect.Height);
            rect.Width = DoFloat(rect.Width);
            rect.X = DoFloat(rect.X);
            rect.Y = DoFloat(rect.Y);

            return rect;
        }


        public Rectangle? DoRectangleNullable(Rectangle? val, bool verifyField = true)
        {
            if (verifyField)
                VerifyField(typeof(Rectangle?));

            if (mode == Mode.Load)
            {
                string typeName = m_reader.ReadString();

                if (typeName == "null")
                    return null;

                Debug.Assert(typeName == "Rectangle?", "Expected a Rectangle?, but read a " + typeName);

                Rectangle rect = new Rectangle();
                rect.Height = DoInt32(rect.Height, false);
                rect.Width = DoInt32(rect.Width, false);
                rect.X = DoInt32(rect.X, false);
                rect.Y = DoInt32(rect.Y, false);

                return rect;

            }
            else if (mode == Mode.Save)
            {
                if (val.HasValue)
                {
                    m_writer.Write("Rectangle?");

                    DoInt32(val.Value.Height, false);
                    DoInt32(val.Value.Width, false);
                    DoInt32(val.Value.X, false);
                    DoInt32(val.Value.Y, false);
                    
                }
                else
                {
                    m_writer.Write("null");                   
                }
            }
            else if (mode == Mode.CRC)
            {
                if (val.HasValue)
                {
                    DoInt32(val.Value.Height, false);
                    DoInt32(val.Value.Width, false);
                    DoInt32(val.Value.X, false);
                    DoInt32(val.Value.Y, false);                 
                }
                else
                    CRC.AddData(BitConverter.GetBytes(0));
            }

            return val;

        }

        public Rectangle DoRectangle(Rectangle val, bool verifyField = true)
        {
            if (verifyField)
                VerifyField(typeof(Rectangle));

            Rectangle rect = val;
            rect.Height = DoInt32(rect.Height, false);
            rect.Width = DoInt32(rect.Width, false);
            rect.X = DoInt32(rect.X, false);
            rect.Y = DoInt32(rect.Y, false);

            return rect;
        }

        /// <summary>
        /// a special static Do function, designed do be used outside the normal Snapshot sequence - for listing save games together with extra info for example.
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="value"></param>
        /// <returns></returns>
       /* public static SnapshotHeader DoHeader(Mode mode, BinaryReader reader, BinaryWriter writer, SnapshotHeader value)
        {
           
            if (mode == Mode.Load)
            {
                SnapshotHeader header = new SnapshotHeader();            

                header.ScenarioName = DoStringStatic(mode, writer, reader, null);
                int? sourceAsInt = DoInt32NullableStatic(mode, writer, reader, null);
                if (sourceAsInt.HasValue)
                {
                    header.ScenarioSource = (Source)sourceAsInt.Value;
                }

                header.ProgramVersion = new System.Version(DoStringStatic(mode, writer, reader, null));
                header.Timestamp = new DateTime(reader.ReadInt64());


                return header;
            }
            else
            {
                DoStringStatic(mode, writer, reader, value.ScenarioName);
                DoInt32NullableStatic(mode, writer, reader, (int?)value.ScenarioSource);
                DoStringStatic(mode, writer, reader, value.ProgramVersion.ToString());
                writer.Write(value.Timestamp.Ticks);

                return value;
            }
        }*/

        public string DoString(string val, bool verifyField = true)
        {
            if (verifyField)
                VerifyField(typeof(string));

            if (mode == Mode.Load
                || mode == Mode.Save)
            {
                return DoStringStatic(mode, m_writer, m_reader, val);
            }
            else
            {
                CRC.AddData(System.Text.Encoding.Unicode.GetBytes(val));
                //.Net always uses Unicode encoding for System.string
            }

            return val;

           /* if (mode == Mode.Load)
            {
                string typeName = m_reader.ReadString();
                if (typeName == "null")
                    return null;

                Debug.Assert(typeName == "string", "Expected a string, but read a " + typeName);
                return m_reader.ReadString();
            }
            else if (mode == Mode.Save)
            {
                if (val == null)
                {
                    m_writer.Write("null");
                    return null;
                }
                else
                {
                    m_writer.Write("string");
                    m_writer.Write(val);
                }
            }
            else if (mode == Mode.CRC)
                CRC.AddData(System.Text.Encoding.Unicode.GetBytes(val));
                //.Net always uses Unicode encoding for System.string

            return val;*/
        }

        public static string DoStringStatic(Mode mode, BinaryWriter writer, BinaryReader reader, string val)
        {
            if (mode == Mode.Load)
            {
                string typeName = reader.ReadString();
                if (typeName == "null")
                    return null;

                Debug.Assert(typeName == "string", "Expected a string, but read a " + typeName);
                return reader.ReadString();
            }
            else if (mode == Mode.Save)
            {
                if (val == null)
                {
                    writer.Write("null");
                    return null;
                }
                else
                {
                    writer.Write("string");
                    writer.Write(val);
                }
            }
          /*  else if (mode == Mode.CRC)
                CRC.AddData(System.Text.Encoding.Unicode.GetBytes(val));*/
            //.Net always uses Unicode encoding for System.string

            return val;

        }

        /// <summary>
        /// For structs in collections!
        /// add type mappings for collection elements here.
        /// only value types??? then remove DoString
        /// </summary>
        private Dictionary<Type, Func<object, object>> TypeAndDoMappings = new Dictionary<Type, Func<object, object>>();


        public Point DoPoint(Point val, bool verifyField = true)
        {
            if (verifyField)
                VerifyField(typeof(Point));

            Point pt = val;
            pt.X = DoInt32(pt.X, false);
            pt.Y = DoInt32(pt.Y, false);
            return pt;
        }

        public TilePos DoTilePos(TilePos val, bool verifyField = true)
        {
            if (verifyField)
                VerifyField(typeof(TilePos));

            TilePos pt = val;
            pt.X = DoInt32(pt.X, false);
            pt.Y = DoInt32(pt.Y, false);
            return pt;
        }

        public TilePos? DoTilePosNullable(TilePos? val, bool verifyField = true)
        {
            if (verifyField)
                VerifyField(typeof(TilePos?));

            if (mode == Mode.Load)
            {
                string typeName = m_reader.ReadString();

                if (typeName == "null")
                    return null;

                Debug.Assert(typeName == "TilePos?", "Expected a TilePos?, but read a " + typeName);

                int X = m_reader.ReadInt32();
                int Y = m_reader.ReadInt32();

                return new TilePos(X, Y);

            }
            else if (mode == Mode.Save)
            {
                if (val.HasValue)
                {
                    m_writer.Write("TilePos?");
                    m_writer.Write(val.Value.X);
                    m_writer.Write(val.Value.Y);
                }
                else
                {
                    m_writer.Write("null");
                }
            }
            else if (mode == Mode.CRC)
            {
                if (val.HasValue)
                {
                    DoInt32(val.Value.X);
                    DoInt32(val.Value.Y);
                }
                else
                    CRC.AddData(BitConverter.GetBytes(0));
            }

            return val;

        }

        public Point? DoPointNullable(Point? val, bool verifyField = true)
        {
            if (verifyField)
                VerifyField(typeof(Point?));

            if (mode == Mode.Load)
            {
                string typeName = m_reader.ReadString();
               
                if (typeName == "null")
                    return null;

                Debug.Assert(typeName == "Point?", "Expected a Point?, but read a " + typeName);


                int X = m_reader.ReadInt32();
                int Y = m_reader.ReadInt32();

                return new Point(X, Y);
            }
            else if (mode == Mode.Save)
            {
                if (val.HasValue)
                {
                    m_writer.Write("Point?");
                    m_writer.Write(val.Value.X);
                    m_writer.Write(val.Value.Y);
                }
                else
                {
                    m_writer.Write("null");
                }
            }
            else if (mode == Mode.CRC)
            {
                if (val.HasValue)
                {
                    DoFloat(val.Value.X);
                    DoFloat(val.Value.Y);
                }
                else
                    CRC.AddData(BitConverter.GetBytes(0));
            }

            return val;
        }


        public Type DoType(Type t, bool verifyField = true)
        {
            if (verifyField)
                VerifyField(typeof(Type));


            string typeName = null;
            if (mode != Mode.Load)
            {
                typeName = t.FullName;
            }

            string readType = DoString(typeName, false);
            return Type.GetType(readType);

        }

        public EntityAndRoot DoEntityAndRoot(EntityAndRoot val, bool verifyField = true)
        {
            if (verifyField)
                VerifyField(typeof(EntityAndRoot));

            EntityAndRoot pt = val;
            pt.Entity = DoEnum(pt.Entity, false);
            pt.Root = DoEnum(pt.Root, false);

            return pt;
        }

        public EntityAndRoot? DoEntityAndRootNullable(EntityAndRoot? partAndRoot, bool verifyField = true)
        {
            if (verifyField)
                VerifyField(typeof(EntityAndRoot?));

            if (mode == Mode.Load)
            {
                string typeName = m_reader.ReadString();

                if (typeName == "null")
                    return null;

                Debug.Assert(typeName == "EntityAndRoot?", "Expected a EntityAndRoot?, but read a " + typeName);


                EntityID part = DoEnum(EntityID.Invalid, false);
                EntityID root = DoEnum(EntityID.Invalid, false);


                return new EntityAndRoot(part, root);

            }
            else if (mode == Mode.Save)
            {
                if (partAndRoot.HasValue)
                {
                    m_writer.Write("EntityAndRoot?");
                    DoEnum(partAndRoot.Value.Entity, false);
                    DoEnum(partAndRoot.Value.Root, false);
                }
                else
                {
                    m_writer.Write("null");
                }
            }
            else if (mode == Mode.CRC)
            {
                if (partAndRoot.HasValue)
                {
                    DoEnum(partAndRoot.Value.Entity, false);
                    DoEnum(partAndRoot.Value.Root, false);
                }
                else
                    CRC.AddData(BitConverter.GetBytes(0));
            }

            return partAndRoot;
        }


        public TravelLocation? DoTravelLocationNullable(TravelLocation? travelLocation, bool verifyField = true)
        {
            if (verifyField)
                VerifyField(typeof(TravelLocation?));

            if (mode == Mode.Load)
            {
                string typeName = m_reader.ReadString();

                if (typeName == "null")
                    return null;

                Debug.Assert(typeName == "TravelLocation?", "Expected a TravelLocation?, but read a " + typeName);

                long? AllegianceID = DoInt64Nullable(null, false);
                long SiteID = DoInt64(0, false);
                long? ExpeditionID = DoInt64Nullable(null, false);
                long? TerminalEntityID = DoInt64Nullable(null, false);
                
                return new TravelLocation(SiteID, AllegianceID, ExpeditionID, TerminalEntityID);

            }
            else if (mode == Mode.Save)
            {
                if (travelLocation.HasValue)
                {
                    m_writer.Write("TravelLocation?");
                    DoInt64Nullable(travelLocation.Value.AllegianceID, false);
                    DoInt64(travelLocation.Value.SiteID, false);
                    DoInt64Nullable(travelLocation.Value.ExpeditionID, false);
                    DoInt64Nullable(travelLocation.Value.TerminalEntityID, false);

                }
                else
                {
                    m_writer.Write("null");
                }
            }
            else if (mode == Mode.CRC)
            {
                if (travelLocation.HasValue)
                {
                    DoInt64Nullable(travelLocation.Value.AllegianceID, false);
                    DoInt64(travelLocation.Value.SiteID, false);
                    DoInt64Nullable(travelLocation.Value.ExpeditionID, false);
                    DoInt64Nullable(travelLocation.Value.TerminalEntityID, false);
                }
                else
                    CRC.AddData(BitConverter.GetBytes(0));
            }

            return travelLocation;
        }


        public StorageTarget? DoStorageTargetNullable(StorageTarget? storageTarget, bool verifyField = true)
        {
            if (verifyField)
                VerifyField(typeof(StorageTarget?));

            if (mode == Mode.Load)
            {
                string typeName = m_reader.ReadString();

                if (typeName == "null")
                    return null;

                Debug.Assert(typeName == "StorageTarget?", "Expected a StorageTarget?, but read a " + typeName);

                EntityID storedEntityID = DoEnum(EntityID.Invalid, false);
                StorageID storageID = DoEnum(StorageID.Invalid, false);
               // Compartment? compartment = DoEnumNullable((Compartment?)null, false);
              //  StorageCondition storageCondition = (StorageCondition)DoGameData(typeof(StorageCondition), null, false);

                return new StorageTarget(storedEntityID, storageID); //, storageCondition);

            }
            else if (mode == Mode.Save)
            {
                if (storageTarget.HasValue)
                {
                    m_writer.Write("StorageTarget?");
                    DoEnum(storageTarget.Value.StorageEntity, false);
                    DoEnum(storageTarget.Value.StorageID, false);
                  //  DoEnumNullable(storageTarget.Value.Compartment, false);
                  //  DoGameData(storageTarget.Value.StorageCondition, false);

                }
                else
                {
                    m_writer.Write("null");
                }
            }
            else if (mode == Mode.CRC)
            {
                if (storageTarget.HasValue)
                {
                    DoEnum(storageTarget.Value.StorageEntity, false);
                    DoEnum(storageTarget.Value.StorageID, false);
                 //   DoEnumNullable(storageTarget.Value.Compartment, false);
                 //   DoGameData(storageTarget.Value.StorageCondition, false);
                 }
                else
                    CRC.AddData(BitConverter.GetBytes(0));
            }

            return storageTarget;
        }


        public GeodeticCoordinate? DoGeodeticCoordinateNullable(GeodeticCoordinate? coords, bool verifyField = true)
        {
            if (verifyField)
                VerifyField(typeof(GeodeticCoordinate?));

            if (mode == Mode.Load)
            {
                string typeName = m_reader.ReadString();

                if (typeName == "null")
                    return null;

                Debug.Assert(typeName == "GeodeticCoordinate?", "Expected a GeodeticCoordinate?, but read a " + typeName);

                double latitude = DoDouble(0d, false);
                double longitude = DoDouble(0d, false);

                return new GeodeticCoordinate(longitude, latitude);

            }
            else if (mode == Mode.Save)
            {
                if (coords.HasValue)
                {
                    m_writer.Write("GeodeticCoordinate?");
                    DoDouble(coords.Value.Latitude, false);
                    DoDouble(coords.Value.Longitude, false);
                  
                }
                else
                {
                    m_writer.Write("null");
                }
            }
            else if (mode == Mode.CRC)
            {
                if (coords.HasValue)
                {
                    DoDouble(coords.Value.Latitude, false);
                    DoDouble(coords.Value.Longitude, false);                  
                }
                else
                    CRC.AddData(BitConverter.GetBytes(0));
            }

            return coords;
        }

        public GeodeticCoordinate DoGeodeticCoordinate(GeodeticCoordinate coords, bool verifyField = true)
        {
            if (verifyField)
                VerifyField(typeof(GeodeticCoordinate));
            
            double latitude = DoDouble(coords.Latitude, false);
            double longitude = DoDouble(coords.Longitude, false);

            GeodeticCoordinate newCoords = new GeodeticCoordinate(longitude, latitude);
            
            return newCoords;
        }

        public TravelLocation DoTravelLocation(TravelLocation travelLocation, bool verifyField = true)
        {
            if (verifyField)
                VerifyField(typeof(TravelLocation));

            
            long? allegianceID = DoInt64Nullable(travelLocation.AllegianceID, false);
            long siteID = DoInt64(travelLocation.SiteID, false);
            long? expeditionID = DoInt64Nullable(travelLocation.ExpeditionID, false);
            long? terminalEntityID = DoInt64Nullable(travelLocation.TerminalEntityID, false);

            TravelLocation newLocation = new TravelLocation(siteID, allegianceID, expeditionID, terminalEntityID);


            return newLocation;

        }

        public Color DoColor(Color color, bool verifyField = true)
        {
            if (verifyField)
                VerifyField(typeof(Color));

            Color newColor = new Color();             

            newColor.A = DoByte(color.A, false);
            newColor.R = DoByte(color.R, false);
            newColor.G = DoByte(color.G, false);
            newColor.B = DoByte(color.B, false);
            
            return newColor;
        }


        //public ulong DoEnum(System.Enum val, bool verifyField = true)

       
        public T DoEnum<T>(T val, bool verifyField = true)
            where T : struct,  IComparable, IFormattable, IConvertible // the closest constraint to enum we can get..
        {
            Type typeOfT = typeof(T);
            if (verifyField)
            {              
                VerifyField(typeOfT); 
            }


            // get the true underlying type to avoid cast exceptions:
            Type underlyingType = Enum.GetUnderlyingType(typeOfT);
            TypeCode typeCode = Type.GetTypeCode(underlyingType);

            switch(typeCode)
            {
                case TypeCode.Int32: // default for enums
                    {
                        int integerVal = Convert.ToInt32(val);

                        if (mode == Mode.CRC)
                        {
                            CRC.AddData(BitConverter.GetBytes(integerVal)); 
                            return val; // ignored in crc mode
                        }
                        else
                        {
                            //for save and/or load
                            int returnedInt = DoInt32(integerVal, false);

                            ValueType valueType = (ValueType)returnedInt;
                            return (T)valueType; // this will throw if we did not use the correct underlying type
                        }
                    }
                case TypeCode.Int64: // used by xml serialized classes
                    {
                        long integerVal = Convert.ToInt64(val);

                        if (mode == Mode.CRC)
                        {
                            CRC.AddData(BitConverter.GetBytes(integerVal));
                            return val; // ignored in crc mode
                        }
                        else
                        {
                            //for save and/or load
                            long returnedInt = DoInt64(integerVal, false);

                            ValueType valueType = (ValueType)returnedInt;
                            return (T)valueType; // this will throw if we did not use the correct underlying type
                        }
                    }
                case TypeCode.UInt64:
                    {
                        ulong ulongVal = Convert.ToUInt64(val);

                        if (mode == Mode.CRC)
                        {
                            CRC.AddData(BitConverter.GetBytes(ulongVal)); 
                            return val; //ignored in crc mode
                        }
                        else
                        {
                            //for save and/or load
                            ulong returnedULong = DoUInt64(ulongVal, false);

                            ValueType valueType = (ValueType)returnedULong;
                            return (T)valueType; // this will throw if we did not use the correct underlying type
                        }
                        
                    }
            }

            return val;
           
        }

        /// <summary>
        /// will verify the nullable enum type correctly, and does not require casting.
        /// works for ulong and int (default) basetype enums, more types can easily be added.
        /// </summary>
        /// <typeparam name="TEnum"></typeparam>
        /// <param name="val"></param>
        /// <param name="verifyField"></param>
        /// <returns></returns>
        public TEnum? DoEnumNullable<TEnum>(TEnum? val, bool verifyField = true)
              where TEnum : struct,  IComparable, IFormattable, IConvertible // the closest constraint to enum we can get..
        {
            Type typeOfNullableT = typeof(TEnum?);
            VerifyField(typeOfNullableT);

            // get the true underlying type to avoid cast exceptions:
            Type underlyingEnumType = Enum.GetUnderlyingType(typeof(TEnum)); //Enum.GetUnderlyingType(typeOfT.GetGenericArguments()[0]); // get the T in Nullable<T>
            TypeCode enumTypeCode = Type.GetTypeCode(underlyingEnumType);           

            // branch, based on value / null, and underlying type:
            if (val != null)
            {
                switch (enumTypeCode)
                {
                    case TypeCode.UInt64:
                        {
                            ulong? enumAsULong = (ulong)((ValueType)(val)); // cannot cast to ulong?, because we can only unbox to the same type (ulong)...

                            ulong? returnedULong = DoUInt64Nullable(enumAsULong, false); // DoEnumNullable(enumAsULong, false);

                            ValueType returnedValue = (ValueType)returnedULong;

                            return (TEnum)returnedValue; // cannot cast to Id?, probably for the same reason? Will always have a value
                        }
                    case TypeCode.Int32:
                        {
                            int? enumAsInt = (int)((ValueType)(val)); 

                            int? returnedInt = DoInt32Nullable(enumAsInt, false); 

                            ValueType returnedValue = (ValueType)returnedInt;

                            return (TEnum)returnedValue; // cannot cast to Id?, probably for the same reason? Will always have a value

                        }
                    case TypeCode.Int64:
                        {
                            long? enumAsLong = (long)((ValueType)(val)); 

                            long? returnedLong = DoInt64Nullable(enumAsLong, false); // DoEnumNullable(enumAsULong, false);

                            ValueType returnedValue = (ValueType)returnedLong;

                            return (TEnum)returnedValue; // cannot cast to Id?, probably for the same reason? Will always have a value
                        }
                }               

            }
            else
            {
                switch (enumTypeCode)
                {
                    case TypeCode.UInt64:
                        {
                            ulong? returnedULong = DoUInt64Nullable((ulong?)null, false); 

                            //return (Id?)((ValueType)returnedULong); this gives InvalidCastException, maybe to do with unboxing..
                            if (returnedULong.HasValue)
                            {
                                return (TEnum)((ValueType)returnedULong);
                            }
                            else
                            {
                                return null;
                            }
                        }
                    case TypeCode.Int32:
                        {
                            int? returnedInt = DoInt32Nullable((int?)null, false);

                            if (returnedInt.HasValue)
                            {
                                return (TEnum)((ValueType)returnedInt);
                            }
                            else
                            {
                                return null;
                            }

                        }
                    case TypeCode.Int64:
                        {
                            long? returnedLong = DoInt64Nullable((long?)null, false);

                            if (returnedLong.HasValue)
                            {
                                return (TEnum)((ValueType)returnedLong);
                            }
                            else
                            {
                                return null;
                            }
                        }
                }
            }

            return val;

        }

     

        /// <summary>
        /// convenience method - no longer needed with the improved DoEnumNullable.
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        public EntityID? DoEntityIDNullable(EntityID? val, bool verifyField = true)
        {
            if (verifyField)
                VerifyField(typeof(EntityID?));

            return DoEnumNullable(val);
        }

        /// <summary>
        /// convenience method - no longer needed with the improved DoEnum.
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        public EntityID DoEntityID(EntityID val, bool verifyField = true)
        {
            if (verifyField)
                VerifyField(typeof(EntityID));

            return DoEnum(val, false);
        }

        /// <summary>
        /// convenience method - no longer needed with the improved DoEnum.
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        public MethodID DoMethodID(MethodID val, bool verifyField = true)
        {
            if (verifyField)
                VerifyField(typeof(MethodID));

            return DoEnum(val, false);
        }

        /// <summary>
        /// convenience method - no longer needed with the improved DoEnumNullable.
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        public MethodID? DoMethodIDNullable(MethodID? val, bool verifyField = true)
        {
            return DoEnumNullable(val);
        }


        public Int64? DoInt64Nullable(Int64? val, bool verifyField = true)
        {
            if (verifyField)
                VerifyField(typeof(Int64?));

            if (mode == Mode.Load)
            {
                string typeName = m_reader.ReadString();
               
                if (typeName == "null")
                    return null;

                Debug.Assert(typeName == "Int64?", "Expected a Int64?, but read a " + typeName);

                Int64 value = m_reader.ReadInt64();                

                return value;
            }

            if (mode == Mode.Save)
            {
                if (val.HasValue)
                {
                    m_writer.Write("Int64?");
                    m_writer.Write(val.Value);
                }
                else
                {
                    m_writer.Write("null");
                  //  m_writer.Write(0L);
                }
            }
            else if (mode == Mode.CRC)
            {
                if (val.HasValue)
                    CRC.AddData(BitConverter.GetBytes(val.Value));
                else
                    CRC.AddData(BitConverter.GetBytes(0L));
            }
            return val;
        }

        public ulong? DoUInt64Nullable(ulong? val, bool verifyField = true)
        {
            if (verifyField)
                VerifyField(typeof(ulong?));


            if (mode == Mode.Load)
            {
                string typeName = m_reader.ReadString();
             
                if (typeName == "null")
                    return null;

                Debug.Assert(typeName == "UInt64?", "Expected a UInt64?, but read a " + typeName);

                ulong value = m_reader.ReadUInt64();

                return value;
            }

            if (mode == Mode.Save)
            {
                if (val.HasValue)
                {
                    m_writer.Write("UInt64?");
                    m_writer.Write(val.Value);
                }
                else
                {
                    m_writer.Write("null");
                   // m_writer.Write(0UL);
                }
            }
            else if (mode == Mode.CRC)
            {
                if (val.HasValue)
                    CRC.AddData(BitConverter.GetBytes(val.Value));
                else
                    CRC.AddData(BitConverter.GetBytes(0UL));
            }
            return val;
        }



        public T[][] DoJaggedArray<T>(T[][] collection, bool verifyField = true)
        {        

            Type collectionType = typeof(T[][]);
            string thisTypeName = collectionType.Name;

            if (verifyField)
                VerifyField(collectionType);


            if (mode == Mode.Load)
            {
                string readTypeName = m_reader.ReadString();

                if (readTypeName == "null")
                    return null;
                
                Debug.Assert(readTypeName == thisTypeName, string.Format("Expected a {0}, but read a {1}", thisTypeName, readTypeName));

                int count = DoInt32(0, false); //reads the number of elements

                T[][] temp = new T[count][];
             //   T[][] temp = (T[][])Activator.CreateInstance(collectionType);
              

                for (int c = 0; c < count; ++c)
                {
                    // now do the nested arrays

                    var value = DoArray<T>(null, false); // create the array

                    temp[c] = (T[])value; // assign - is cast ok?
                }

                return temp;
            }
            else
            {
                if (mode == Mode.Save)
                {
                    if (collection == null)
                    {
                        m_writer.Write("null");
                    }
                    else
                    {
                        m_writer.Write(thisTypeName); 
                    }
                }

                if (collection == null)
                {
                    return null; // bailout if instance is null
                }

                int count = collection.Length;
                count = DoInt32(count, false);//gets or sets the count
                
                foreach (var array in collection)
                {                  
                    
                    DoArray<T>(array, false);
                }
            }

            return collection;
        }

        public Queue<T> DoQueue<T>(Queue<T> collection, bool verifyField = true)
        {
         
            TypeInformation typeInfo;
            GetStaticTypeInfo<T>(out typeInfo); 

           
            Type collectionType = typeof(Queue<T>);
            string thisTypeName = collectionType.Name;

            if (verifyField)
                VerifyField(collectionType);
            

            if (mode == Mode.Load)
            {
                string readTypeName = m_reader.ReadString();
                if (readTypeName == "null")
                    return null;
              
                Debug.Assert(readTypeName == thisTypeName, string.Format("Expected a {0}, but read a {1}", thisTypeName, readTypeName));

                int count = DoInt32(0, false); //reads the number of elements

                Queue<T> temp = (Queue<T>)Activator.CreateInstance(collectionType);

                for (int i = 0; i < count; ++i)
                {                   
                    object value = DoElement(typeInfo, default(T)); // type, typeCode, isNullable, isSnapshot, isGameData, isType, tupleProperties, tupleTypeArguments, default(T));

                    temp.Enqueue((T)value);
                }

                return temp;
            }
            else
            {
                if (mode == Mode.Save)
                {
                    if (collection == null)
                    {                      
                        m_writer.Write("null");
                    }
                    else
                    {
                        m_writer.Write(thisTypeName); // ??
                    }
                }

                if (collection == null)
                {
                    return null; // bailout if instance is null
                }

                // save/crc the elements:

                int count = collection.Count;
                count = DoInt32(count, false);//gets or sets the count

                //for save or CRC...
                foreach (T element in collection)
                {
                    DoElement(typeInfo, element); // type, typeCode, isNullable, isSnapshot, isGameData, isType, tupleProperties, tupleTypeArguments, element);
                }

            }

            return collection;
        }

        public SerializableQueue<T> DoSerializableQueue<T>(SerializableQueue<T> collection, bool verifyField = true)
        {

            TypeInformation typeInfo;
            GetStaticTypeInfo<T>(out typeInfo);


            Type collectionType = typeof(SerializableQueue<T>);
            string thisTypeName = collectionType.Name;

            if (verifyField)
                VerifyField(collectionType);


            if (mode == Mode.Load)
            {
                string readTypeName = m_reader.ReadString();
                if (readTypeName == "null")
                    return null;

                Debug.Assert(readTypeName == thisTypeName, string.Format("Expected a {0}, but read a {1}", thisTypeName, readTypeName));

                int count = DoInt32(0, false); //reads the number of elements

                SerializableQueue<T> temp = (SerializableQueue<T>)Activator.CreateInstance(collectionType);

                for (int i = 0; i < count; ++i)
                {
                    object value = DoElement(typeInfo, default(T)); // type, typeCode, isNullable, isSnapshot, isGameData, isType, tupleProperties, tupleTypeArguments, default(T));

                    temp.Enqueue((T)value);
                }

                return temp;
            }
            else
            {
                if (mode == Mode.Save)
                {
                    if (collection == null)
                    {
                        m_writer.Write("null");
                    }
                    else
                    {
                        m_writer.Write(thisTypeName); // ??
                    }
                }

                if (collection == null)
                {
                    return null; // bailout if instance is null
                }

                // save/crc the elements:

                int count = collection.Count;
                count = DoInt32(count, false);//gets or sets the count

                //for save or CRC...
                foreach (T element in collection)
                {
                    DoElement(typeInfo, element); // type, typeCode, isNullable, isSnapshot, isGameData, isType, tupleProperties, tupleTypeArguments, element);
                }

            }

            return collection;
        }


        /// <summary>
        /// Generic Collections     
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection"></param>
        /// <returns></returns>
        public T[] DoArray<T>(T[] collection, bool verifyField = true)
        {
            TypeInformation typeInfo;
            GetStaticTypeInfo<T>(out typeInfo);

            
            Type collectionType = typeof(T[]);
            string thisTypeName = collectionType.Name;

            if (verifyField)
                VerifyField(collectionType);
            
            if (mode == Mode.Load)
            {
                string readTypeName = m_reader.ReadString();
                if (readTypeName == "null")
                    return null;

                Debug.Assert(readTypeName == thisTypeName, string.Format("Expected a {0}, but read a {1}", thisTypeName, readTypeName));

                int count = DoInt32(0, false); //reads the number of elements

                T[] result = (T[])Array.CreateInstance(typeInfo.Type, count); 

                for (int i = 0; i < count; ++i)
                {                   
                    object value = DoElement(typeInfo, default(T));

                    result[i] = (T)value;
                }

                return result;
            }
            else
            {
                if (mode == Mode.Save)
                {
                    if (collection == null)
                    {
                        m_writer.Write("null");
                    }
                    else
                    {
                        m_writer.Write(thisTypeName); 
                    }
                }

                if (collection == null)
                {
                    return null; // bailout if instance is null
                }

                // save/crc the elements:

                int count = collection.Length;
                count = DoInt32(count, false);//gets or sets the count

                //for save or CRC...
                foreach (T element in collection)
                {
                    DoElement(typeInfo, element);
                }

            }

            return collection;
        }

        /// <summary>
        /// mark the member type as accounted for in the snapshot type we are currently verifying.
        /// </summary>
        /// <param name="type"></param>
        private void VerifyField(Type type)
        {
#if DEBUG
            if (snapshotClassBeingVerified != null)
            {
                snapshotClassBeingVerified.SetTypeAsAccountedFor(type); //.Name);
            }
#endif
        }

       

        public List<T> DoList<T>(List<T> list, bool verifyField = true)      
        {
            TypeInformation typeInfo;
            GetStaticTypeInfo<T>(out typeInfo);

            //verification code
           
            Type collectionType = typeof(List<T>);
            string thisTypeName = collectionType.Name;

            if (verifyField)
                VerifyField(collectionType);
            

            if (mode == Mode.Load)
            {
                string readTypeName = m_reader.ReadString();

                if (readTypeName == "null")
                    return null;

                Debug.Assert(readTypeName == thisTypeName, string.Format("Expected a {0}, but read a {1}", thisTypeName, readTypeName));

#if !RELEASE
                if (readTypeName != thisTypeName)
                {
                    Debug.Assert(false);

                    throw new Exception();
                }
#endif



                int count = DoInt32(0, false); //reads the number of elements


                List<T> temp = (List<T>)Activator.CreateInstance(collectionType);
               
                for (int i = 0; i < count; ++i)
                {
                    object value = DoElement(typeInfo, default(T));

                    temp.Add((T)value);
                }

                return temp;
            }
            else
            {
                if (mode == Mode.Save)
                {
                    if (list == null)
                    {                     
                        m_writer.Write("null");
                        //m_writer.Write(0f);
                    }
                    else
                    {
                        m_writer.Write(thisTypeName); 
                    }
                }

                if (list == null)
                {                    
                    return null; // bailout if instance is null
                }

                // save/crc the elements:

                int count = list.Count;
                count = DoInt32(count, false);//gets or sets the count

                //for save or CRC...
                foreach (T element in list)
                {
                    DoElement(typeInfo, element);
                }

            }

            return list;
        }

        public HashSet<T> DoHashSet<T>(HashSet<T> set, bool verifyField = true)
        {
          /*  Type type;
            TypeCode typeCode;
            bool isNullable, isSnapshot, isType, isGameData;
            TypeInformation[] tupleTypeArguments;
            PropertyInfo[] tupleProperties;*/
            // get reflection/type info once for all elements:
            TypeInformation typeInfo;
            GetStaticTypeInfo<T>(out typeInfo); // out type, out typeCode, out isNullable, out isSnapshot, out isGameData, out isType, out tupleProperties, out tupleTypeArguments);

           
            Type collectionType = typeof(HashSet<T>);
            string thisTypeName = collectionType.Name;

            if (verifyField)
                VerifyField(collectionType);
            
            if (mode == Mode.Load)
            {
                string readTypeName = m_reader.ReadString();

                if (readTypeName == "null")
                    return null;

                Debug.Assert(readTypeName == thisTypeName, string.Format("Expected a {0}, but read a {1}", thisTypeName, readTypeName));

                int count = DoInt32(0, false); //reads the number of elements

                HashSet<T> temp = (HashSet<T>)Activator.CreateInstance(collectionType);

                for (int i = 0; i < count; ++i)
                {
                    object value = DoElement(typeInfo, default(T)); // type, typeCode, isNullable, isSnapshot, isGameData, isType, tupleProperties, tupleTypeArguments, default(T));

                    temp.Add((T)value);
                }

                return temp;
            }
            else
            {
                if (mode == Mode.Save)
                {
                    if (set == null)
                    {
                        m_writer.Write("null");
                        //m_writer.Write(0f);
                    }
                    else
                    {
                        m_writer.Write(thisTypeName);
                    }
                }

                if (set == null)
                {
                    return null; // bailout if instance is null
                }

                // save/crc the elements:

                int count = set.Count;
                count = DoInt32(count, false);//gets or sets the count

                //for save or CRC...
                foreach (T element in set)
                {
                    DoElement(typeInfo, element); //  type, typeCode, isNullable, isSnapshot, isGameData, isType, tupleProperties, tupleTypeArguments, element);
                }

            }

            return set;
        }

     /*   private static void GetStaticTypeInfo<T>(out Type type, out TypeCode typeCode, out bool isNullable,
            out bool isSnapshot, out bool isGameData, out bool isType, out PropertyInfo[] tupleProperties, out TypeInformation[] tupleTypeArguments)
        {
            type = typeof(T); // statically defined

            GetStaticTypeInfo(type, out typeCode, out isNullable, out isSnapshot, out isGameData, out isType, out tupleProperties, out tupleTypeArguments);
        }*/

        public static void GetStaticTypeInfo<T>(out TypeInformation typeInfo)
        {
            Type type = typeof(T); // statically defined

            GetStaticTypeInfo(type, out typeInfo);
        }


        /// <summary>
        /// Since all elements in a collection will be the same type, call this once for the whole collection. 
        /// 
        /// type argument can either be the static type or the instance type... static type is preferable since it is more efficient, but, it can give different results:
        /// typeof(IMap) gives IsSnapshot = false
        /// but ThreatMap.GetType() gives IsSnapshot = true
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="type"></param>
        /// <param name="typeCode"></param>
        /// <param name="isNullable"></param>
        /// <param name="isEnum"></param>
        /// <param name="isSnapshot"></param>
        /// <param name="isType"></param>
        private static void GetStaticTypeInfo(Type type, out TypeInformation typeInfo)
        {
            typeInfo = new TypeInformation();
            typeInfo.Type = type;
            typeInfo.TypeCode = Type.GetTypeCode(type);

            typeInfo.TupleTypeArguments = null;
            typeInfo.TupleProperties = null;
            typeInfo.IsNullable = false;
            typeInfo.IsSnapshot = false;
            typeInfo.IsType = false;
            typeInfo.IsGameData = false;

            if (type.IsGenericType)
            {
                Type genericTypeDefinition = type.GetGenericTypeDefinition();

                if (genericTypeDefinition == typeof(Nullable<>))
                {
                    typeInfo.IsNullable = true;
                }

                // continue with tests...
                if (HandleGenericArguments(genericTypeDefinition))
                {
              /*  if (genericTypeDefinition == typeof(Tuple<,>) // 2-tuple
                    || genericTypeDefinition == typeof(Tuple<,,>) // 3-tuple - more?
                    || genericTypeDefinition == typeof(Pair<,>)) // do Pairs also
                {*/
                    Type[] genericArguments = type.GetGenericArguments();

                    typeInfo.TupleTypeArguments = new TypeInformation[genericArguments.Length];
                    typeInfo.TupleProperties = type.GetProperties();


                    for (int i = 0; i < genericArguments.Length; i++)
                    {
                        Type typeOfArgument = genericArguments[i];
                        TypeInformation genericArgTypeInfo = new TypeInformation();

                        // call get type recursively on each of the type arguments:


                        GetStaticTypeInfo(typeOfArgument, out genericArgTypeInfo); 

                        typeInfo.TupleTypeArguments[i] = genericArgTypeInfo;
                    }

                    return;
                }
            }

            if (type == typeof(Type))
            {
                typeInfo.IsType = true; // the Component dictionary
                return;
            }

            //isSnapshot = typeof(ISnapshot).IsAssignableFrom(type); // fails for IComposite

            if (type is ISnapshot // the static type IMap in the LookUp<IMap> collection is not ISnapshot, but the instance ThreatMap is...
                || typeof(ISnapshot).IsAssignableFrom(type))
            {
                typeInfo.IsSnapshot = true;
                return;
            }

            if (typeof(IGameData).IsAssignableFrom(type))
            {
                typeInfo.IsGameData = true;
                return;
            }

        }


        /// <summary>
        /// put in the generic types that Snapshot should handle...
        /// 
        /// Another option for these classes is to not snapshot the generic type at all, instead rebuild the object post load.
        /// A different option is to implement ISnapshot (see DataPoint)
        /// </summary>
        /// <param name="genericTypeDefinition"></param>
        /// <returns></returns>
        private static bool HandleGenericArguments(Type genericTypeDefinition)
        {
            return (genericTypeDefinition == typeof(Tuple<,>) // 2-tuple
                    || genericTypeDefinition == typeof(Tuple<,,>) // 3-tuple - add more?
                    || genericTypeDefinition == typeof(Pair<,>)
                    /*|| genericTypeDefinition == typeof(DataPoint<>)*/); 

        }


        /// <summary>
        /// Since all elements in a collection will be the same type, call this once for the whole collection. 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="type"></param>
        /// <param name="typeCode"></param>
        /// <param name="isNullable"></param>
        /// <param name="isEnum"></param>
        /// <param name="isSnapshot"></param>
        /// <param name="isType"></param>
      /*  private static void GetStaticTypeInfo(Type type, out TypeCode typeCode, out bool isNullable, 
            out bool isSnapshot, out bool isGameData, out bool isType, out PropertyInfo[] tupleProperties, out TypeInformation[] tupleTypeArguments)
        {           
            typeCode = Type.GetTypeCode(type);

            tupleTypeArguments = null;
            tupleProperties = null;
            isNullable = false;
            isSnapshot = false;
            isType = false;
            isGameData = false;

            if (type.IsGenericType)
            {
                Type genericTypeDefinition = type.GetGenericTypeDefinition();

                if (genericTypeDefinition == typeof(Nullable<>))
                {
                    isNullable = true;
                }

                // continue with tests...

                if (genericTypeDefinition == typeof(Tuple<,>) // 2-tuple
                    || genericTypeDefinition == typeof(Tuple<,,>) // 3-tuple - more?
                    || genericTypeDefinition == typeof(Pair<,>)) // do Pairs also
                {    
                    Type[] genericArguments = type.GetGenericArguments();

                    tupleTypeArguments = new TypeInformation[genericArguments.Length];
                    tupleProperties = type.GetProperties();

                   
                    for (int i = 0; i < genericArguments.Length; i++)
                    {
                        Type typeOfArgument = genericArguments[i];
                        TypeInformation typeInfo = new TypeInformation();

                        // call get type recursively on each of the type arguments:

                        TypeCode genericArgTypeCode;
                        bool genericArgIsNullable, genericArgIsSnapshot, genericArgIsGameData, genericArgIsType;
                        PropertyInfo[] genericArgTupleProperties; // not used
                        TypeInformation[] genericArgTypeArguments; // not used/supported

                        GetStaticTypeInfo(typeOfArgument, out genericArgTypeCode, out genericArgIsNullable, out genericArgIsSnapshot,
                            out genericArgIsGameData, out genericArgIsType, out genericArgTupleProperties, out genericArgTypeArguments);

                        // save the info:
                        typeInfo.Type = typeOfArgument;
                        typeInfo.TypeCode = Type.GetTypeCode(typeOfArgument);
                        typeInfo.IsNullable = genericArgIsNullable;
                        typeInfo.IsGameData = genericArgIsGameData;
                        typeInfo.IsSnapshot = genericArgIsSnapshot;
                        typeInfo.IsType = genericArgIsType;                        

                        tupleTypeArguments[i] = typeInfo;
                    }

                    return;
                }
            }

            if (type == typeof(Type))
            {
                isType = true; // the Component dictionary
                return;
            }
            
            //isSnapshot = typeof(ISnapshot).IsAssignableFrom(type); // fails for IComposite
            if (type is ISnapshot // ??
                || typeof(ISnapshot).IsAssignableFrom(type))
            {
                isSnapshot = true;
                return;
            }

            if (typeof(IGameData).IsAssignableFrom(type))
            {
                isGameData = true;
                return;
            }
                       
        }*/

        /// <summary>
        /// Any class that calls DoElement on an ISnapshot type is responsible for calling LoadPostProcess as well!
        /// </summary>
        /// <param name="type"></param>
        /// <param name="typeCode"></param>
        /// <param name="isNullable"></param>
        /// <param name="value"></param>
        /// <returns></returns>
      /*  private object DoElement(TypeInformation typeInfo, object value)
        //private object DoElement(Type type, TypeCode typeCode, bool isNullable, bool isSnapshot, bool isGameData, bool isType, PropertyInfo[] tupleProperties, TypeInformation[] tupleTypeArguments, object value)
        {
            // object value = null;   // i guess using an object type with value types will cause 'boxing'... but I don't see any way around it while this method is generic and the Do..() methods are not.

            if (isSnapshot)
            {
                //ISnapshot                
                value = DoISnapshot((ISnapshot)value, type);
            }            
            else if (isGameData)
            {
                value = DoGameData((IGameData)value);
            }
            else if (isType)
            {
                value = DoType((Type)value);
            }
            else if (tupleTypeArguments != null)
            {
                DoTuple(value, type, tupleProperties, tupleTypeArguments);
            }
            else
            {
                value = DoPrimitivesAndStructs(type, typeCode, isNullable, value);
            }

            return value;
        }*/


        public Pair<T, T> DoPair<T>(Pair<T, T> value)
        {

            if (mode == Mode.Load)
            {
                string readTypeName = m_reader.ReadString();
                if (readTypeName.Equals("null"))
                {                    
                    return null;
                }

                TypeInformation typeInfo;
                GetStaticTypeInfo<Pair<T, T>>(out typeInfo);

                return (Pair<T, T>)DoElement(typeInfo, value);
            }
            else if (mode == Mode.Save)
            {
                if (value != null)
                {
                    TypeInformation typeInfo;
                    GetStaticTypeInfo<Pair<T, T>>(out typeInfo);

                    return (Pair<T, T>)DoElement(typeInfo, value);
                }
                else
                {
                    m_writer.Write("null");
                }               
            }

            return value;
         }

        /// <summary>
        /// passing values to/from object type will cause boxing...
        /// 
        /// Any class that calls DoElement on an ISnapshot type is responsible for calling LoadPostProcess as well!
        /// </summary>
        /// <param name="type"></param>
        /// <param name="typeCode"></param>
        /// <param name="isNullable"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public object DoElement(TypeInformation typeInfo, object value)
        {             
            if (typeInfo.IsSnapshot)
            {                   
                value = DoISnapshot((ISnapshot)value, typeInfo.Type, false);
            }          
            else if (typeInfo.IsGameData)
            {              
                value = DoGameData(typeInfo.Type, (IGameData)value, false);
            }
            else if (typeInfo.IsType)
            {
                value = DoType((Type)value, false);
            }
            else if (typeInfo.TupleTypeArguments != null)
            {               
                value = DoGenericType(value, typeInfo.Type, typeInfo.TupleProperties, typeInfo.TupleTypeArguments, false);               
            }            
            else
            {
                value = DoPrimitivesAndStructs(typeInfo.Type, typeInfo.TypeCode, typeInfo.IsNullable, value);
            }

            return value;
        }

        /// <summary>
        /// uses reflection to handle an object instance - try to avoid using object...
        /// </summary>
        /// <returns></returns>
        public object DoObject(object value, bool verifyField = true)
        {
            if (verifyField)
                VerifyField(typeof(object));

            if (mode == Mode.Load)
            {                
                // we have no use for value when loading.

                string typeNameInFile = m_reader.ReadString();
                if (typeNameInFile == "null")
                {
                    return null;
                }

                // read the full type name:
                Type readType = Type.GetType(typeNameInFile);

                TypeInformation typeInfo;
                GetStaticTypeInfo(readType, out typeInfo);

                // with the type info, we can retrieve the data:
                return DoElement(typeInfo, value);    
            }
            else if (mode == Mode.Save)
            {
                if (value == null)
                {
                    m_writer.Write("null");
                    return value;
                }
                else
                {
                    Type type = value.GetType();

                    TypeInformation typeInfo;
                    GetStaticTypeInfo(type, out typeInfo);

                    // write the full type name for recreation during load:
                    m_writer.Write(type.FullName);

                    // now save the object itself:
                    return DoElement(typeInfo, value);           
                }
            }
            /*else if (mode == Mode.CRC) // TODO
            {
                if (value.HasValue)
                    CRC.AddData(BitConverter.GetBytes(value));
                else
                    CRC.AddData(BitConverter.GetBytes(false));
            }*/

            return value;

        }


      /*  private T DoElement<T>(TypeInformation typeInfo, T value)
        //private object DoElement(Type type, TypeCode typeCode, bool isNullable, bool isSnapshot, bool isGameData, bool isType, PropertyInfo[] tupleProperties, TypeInformation[] tupleTypeArguments, object value)
        {

            if (typeInfo.IsSnapshot)
            {
                //ISnapshot                
                value = (T)DoISnapshot((ISnapshot)value); //, typeInfo.Type);
            }
            else if (typeInfo.IsGameData)
            {
                value = (T)DoGameData((IGameData)value);
            }
            else if (typeInfo.IsType)
            {
                value = (T)DoType(
                    Convert.ChangeType(value, typeof(Type));
            }
            else if (typeInfo.TupleTypeArguments != null)
            {
                DoTuple(value, typeInfo.Type, typeInfo.TupleProperties, typeInfo.TupleTypeArguments);
            }
            else
            {
                value = (T)DoPrimitivesAndStructs(typeInfo.Type, typeInfo.TypeCode, typeInfo.IsNullable, 
                    (ValueType)Convert.ChangeType(value, typeof(ValueType))); // can't use cast...
            }

            return value;
        }*/


        /// <summary>
        /// using this struct makes it easier to make code changes to arguments
        /// </summary>
        public struct TypeInformation
        {
            public Type Type;
            public TypeCode TypeCode;           
            public bool IsNullable;
            public bool IsSnapshot;
            public bool IsGameData;
            public bool IsType;   
            public PropertyInfo[] TupleProperties;
            public TypeInformation[] TupleTypeArguments;
        }

        private object GetDefaultValue(Type t)
        {
            if (t.IsValueType)
                return Activator.CreateInstance(t);

            return null;
        }

        /// <summary>
        /// Tuple, Pair, DataPoint... does not support null!
        /// these types will be slower to handle...
        /// </summary>
        /// <param name="value"></param>
        /// <param name="type"></param>
        /// <param name="tupleProperties"></param>
        /// <param name="tupleTypeArguments"></param>
        /// <param name="verifyField"></param>
        /// <returns></returns>
        private object DoGenericType(object value, Type type,  PropertyInfo[] tupleProperties, TypeInformation[] tupleTypeArguments, bool verifyField = true)
        {
            if (verifyField)
                VerifyField(type);
                        

            object returnTuple;
            if (mode == Mode.Load)
            {
                // read the tuple values from file:
                object[] tupleItemValues = new object[tupleTypeArguments.Length];

                for (int i = 0; i < tupleProperties.Length; i++)  
                {
                    TypeInformation typeInfo = tupleTypeArguments[i];

                  
                    object propertyValue = DoElement(typeInfo, GetDefaultValue(typeInfo.Type));


                    if (typeInfo.Type.IsEnum)
                    {
                        // convert from ulong to the correct enum type, otherwise CreateInstance will fail to find the correct constructor:
                        tupleItemValues[i] = System.Enum.ToObject(typeInfo.Type, propertyValue);
                    }
                    else
                    {
                        tupleItemValues[i] = propertyValue;
                    }

                }

                // create the tuple:
                returnTuple = Activator.CreateInstance(type, tupleItemValues);

                return returnTuple;
            }
            else if (mode == Mode.Save)
            {

                for (int i = 0; i < tupleProperties.Length; i++)
                {
                    PropertyInfo property = tupleProperties[i];
                    object itemValue = property.GetValue(value, null); // uses reflection to get the value... will be slow

                    TypeInformation typeInfo = tupleTypeArguments[i]; //?

                    DoElement(typeInfo, itemValue);

                }


                return value;
            }
            else //if (mode == Mode.CRC)
            {
                //CRC.AddData(BitConverter.GetBytes(val)); TODO

                return value;
            }
                       

        }

        private object DoPrimitivesAndStructs(Type type, TypeCode typeCode, bool isNullable, object value)
        {
            if (type.IsEnum)
            {
                // get the true underlying type to avoid cast exceptions when loading:
                Type underlyingType = Enum.GetUnderlyingType(type);

                // if we save an enum byte as ulong, we cannot later cast it to the enum type when we need to populate the array/dictionary...
                value = DoPrimitive(typeCode, isNullable, value);

                return value;
            }
            else if (typeCode == TypeCode.Object)
            {
                Func<object, object> doFunction;
                if (TypeAndDoMappings.TryGetValue(type, out doFunction))
                {
                    value = doFunction(value); // won't work with List, Tuple...                   
                }
                else if (isNullable && type.IsGenericType)
                {
                    Type genericType = type.GetGenericArguments()[0];
                    if (genericType.IsEnum)
                    {
                        try
                        {
                            value = DoUnknownObject(value, type, false);
                            return value;
                        }
                        catch (Exception e)
                        {
                            throw new Exception("Nullable enum type is not supported as a list member, add a TypeAndDoMappings entry.", e);
                        }
                   
                    }

                    value = DoUnknownObject(value, type, false);
                }                
                else
                {
                    value = DoUnknownObject(value, type, false);
                }
            }
            else
            {
                return DoPrimitive(typeCode, isNullable, value);
            }

            return value;
        }

        private object DoPrimitive(TypeCode typeCode, bool isNullable, object value)
        {
            switch (typeCode) // do primitive types here:
            {
                case TypeCode.Int64:
                    {
                        if (isNullable)
                        {
                            value = DoInt64Nullable((long?)value, false);
                        }
                        else
                        {
                            value = DoInt64((long)value, false);
                        }
                        break;
                    }
                case TypeCode.Int32:
                    {
                        if (isNullable)
                        {
                            value = DoInt32Nullable((int?)value, false); // parameter is don't care...
                        }
                        else
                        {
                            value = DoInt32((int)value, false);
                        }

                        break;
                    }
                case TypeCode.Boolean:
                    {
                        if (isNullable)
                        {
                            value = DoBoolNullable((bool?)value, false);
                        }
                        else
                        {
                            value = DoBool((bool)value, false);
                        }

                        break;
                    }
                case TypeCode.String:
                    {
                        value = DoString((string)value, false);
                        break;
                    }
                case TypeCode.Single:
                    {
                        value = DoFloat((float)value, false);
                        break;
                    }
                case TypeCode.Double:
                    {
                        if (isNullable)
                        {
                            value = DoDoubleNullable((double?)value, false);
                        }
                        else
                        {
                            value = DoDouble((double)value, false);
                        }

                        break;
                    }
                case TypeCode.UInt16:
                    {
                        value = DoUInt16((ushort)value, false);

                        break;
                    }                
                case TypeCode.UInt64:
                    {
                        if (isNullable)
                        {
                            value = DoUInt64Nullable((ulong?)value, false);
                        }
                        else
                        {
                            value = DoUInt64((ulong)value, false);
                        }

                        break;
                    }
                case TypeCode.Byte:
                    {
                        value = DoByte((byte)value, false);
                        break;
                    }
              
                default:
                    throw new Exception("Missing primitive type case");
                   
            }

            return value;

        }


       /* private T DoPrimitivesAndStructs<T>(Type type, TypeCode typeCode, bool isNullable, T value) 
            where T: struct
        {
            ValueType valueAsValueType = value as ValueType;

            if (type.IsEnum)
            {
                if (isNullable)
                {
                    value = (T)(ValueType)DoEnumNullable((ulong?)valueAsValueType);
                }
                else
                {
                    value = (T)(ValueType)DoEnum((Enum)valueAsValueType);
                }

                return value;
            }

            switch (typeCode) // do primitive types here:
            {
                case TypeCode.Int32:
                    {
                        if (isNullable)
                        {
                            value = (T)(ValueType)DoInt32Nullable((int?)valueAsValueType); // parameter is don't care...
                        }
                        else
                        {
                          
                            value = (T)(ValueType)DoInt32((int)valueAsValueType);
                        }

                        break;
                    }
                case TypeCode.Boolean:
                    {
                        if (isNullable)
                        {
                            value = (T)(ValueType)DoBoolNullable((bool?)valueAsValueType);
                        }
                        else
                        {
                            value = (T)(ValueType)DoBool((bool)valueAsValueType);
                        }

                        break;
                    }
                case TypeCode.String:
                    {
                        value = DoString((string)value); // not a value type...
                        break;
                    }
                case TypeCode.Single:
                    {
                        value = (T)(ValueType)DoFloat((float)valueAsValueType);
                        break;
                    }
                case TypeCode.Object:  // Point, Vector etc.

                    Func<object, object> doFunction;
                    if (TypeAndDoMappings.TryGetValue(type, out doFunction))
                    {
                        value = doFunction(value); // won't work with List, Tuple...
                        break;
                    }
                    else
                    {
                        value = DoObject_______NotYetSupported_______(value, type);

                        break;
                    }
                case TypeCode.UInt16:
                    {
                        value = (T)(ValueType)DoUInt16((ushort)valueAsValueType);

                        break;
                    }
                default:
                    throw new Exception("Missing primitive type case");
            }

            return value;
        }*/


        /// <summary>
        /// for hashset... others too?
        /// </summary>
        /// <typeparam name="K"></typeparam>
        /// <typeparam name="V"></typeparam>
        /// <param name="dict"></param>
        /// <returns></returns>
        public Dictionary<K, HashSet<V>> DoMultiMapHashSet<K, V>(Dictionary<K, HashSet<V>> dict, bool verifyField = true)
        {     
            TypeInformation typeInfo;
            GetStaticTypeInfo<K>(out typeInfo);

            Type collectionType = typeof(Dictionary<K, HashSet<V>>);
            string thisTypeName = collectionType.Name + typeInfo.Type.Name + typeof(HashSet<V>).Name;

            if (verifyField)
                VerifyField(collectionType);

            if (mode == Mode.Load)
            {
                string readTypeName = m_reader.ReadString();

                if (readTypeName == "null")
                    return null;

                Debug.Assert(readTypeName == thisTypeName, string.Format("Expected a {0}, but read a {1}", thisTypeName, readTypeName));

                int count = DoInt32(0, false); //reads the number of elements

                Dictionary<K, HashSet<V>> result = (Dictionary<K, HashSet<V>>)Activator.CreateInstance(collectionType);

                for (int c = 0; c < count; ++c)
                {
                    // call into the same methods as DoCollection here...

                    object key = DoElement(typeInfo, default(K)); // value is don't care when loading
                    var value = DoHashSet<V>(null, false); // create the list

                    result.Add((K)key, value);

                }

                return result;
            }
            else
            {
                if (mode == Mode.Save)
                {
                    if (dict == null)
                    {
                        m_writer.Write("null");
                    }
                    else
                    {
                        m_writer.Write(thisTypeName);
                    }
                }

                if (dict == null)
                {
                    return null; // bailout if instance is null
                }

                int count = dict.Count;
                count = DoInt32(count, false);//gets or sets the count


                foreach (var pair in dict)
                {
                    DoElement(typeInfo, pair.Key);
                    DoHashSet<V>(pair.Value, false);               
                }
            }

            return dict;
        }

        public Dictionary<K, List<V>> DoMultiMap<K, V>(Dictionary<K, List<V>> dict, bool verifyField = true)
        {
            TypeInformation typeInfo;
            GetStaticTypeInfo<K>(out typeInfo);

           
            Type collectionType = typeof(Dictionary<K, List<V>>);
            string thisTypeName = collectionType.Name + typeInfo.Type.Name + typeof(List<V>).Name;
                    
            if (verifyField)
                VerifyField(collectionType);

            if (mode == Mode.Load)
            {
                string readTypeName = m_reader.ReadString();

                if (readTypeName == "null")
                    return null;

                Debug.Assert(readTypeName == thisTypeName, string.Format("Expected a {0}, but read a {1}", thisTypeName, readTypeName));


                int count = DoInt32(0, false); //reads the number of elements


                Dictionary<K, List<V>> temp = (Dictionary<K, List<V>>)Activator.CreateInstance(collectionType);

                for (int c = 0; c < count; ++c)
                {
                    // call into the same methods as DoCollection here...

                    object key = DoElement(typeInfo, default(K)); // value is don't care when loading
                    var value = DoList<V>(null, false); // create the list

                    temp.Add((K)key, value);

                }

                return temp;
            }
            else
            {
                if (mode == Mode.Save)
                {
                    if (dict == null)
                    {
                        m_writer.Write("null");
                    }
                    else
                    {
                        m_writer.Write(thisTypeName); 
                    }
                }

                if (dict == null)
                {
                    return null; // bailout if instance is null
                }

                int count = dict.Count;
                count = DoInt32(count, false);//gets or sets the count


                foreach (var pair in dict)
                {
                    DoElement(typeInfo, pair.Key);
                    DoList<V>(pair.Value, false); 
                }
            }

            return dict;
        }

        public Dictionary<K, V[][]> DoMultiArray<K, V>(Dictionary<K, V[][]> dict, bool verifyField = true)
        {
            TypeInformation typeInfo;
            GetStaticTypeInfo<K>(out typeInfo);


            Type collectionType = typeof(Dictionary<K, V[][]>);
            string thisTypeName = collectionType.Name + typeInfo.Type.Name + typeof(V[][]).Name;

            if (verifyField)
                VerifyField(collectionType);

            if (mode == Mode.Load)
            {
                string readTypeName = m_reader.ReadString();

                if (readTypeName == "null")
                    return null;

                Debug.Assert(readTypeName == thisTypeName, string.Format("Expected a {0}, but read a {1}", thisTypeName, readTypeName));


                int count = DoInt32(0, false); //reads the number of elements


                Dictionary<K, V[][]> temp = (Dictionary<K, V[][]>)Activator.CreateInstance(collectionType);

                for (int c = 0; c < count; ++c)
                {
                    // call into the same methods as DoCollection here...

                    object key = DoElement(typeInfo, default(K)); // value is don't care when loading
                    var value = DoJaggedArray<V>(null, false); // create the list

                    
                    temp.Add((K)key, value);

                }

                return temp;
            }
            else
            {
                if (mode == Mode.Save)
                {
                    if (dict == null)
                    {
                        m_writer.Write("null");
                    }
                    else
                    {
                        m_writer.Write(thisTypeName);
                    }
                }

                if (dict == null)
                {
                    return null; // bailout if instance is null
                }

                int count = dict.Count;
                count = DoInt32(count, false);//gets or sets the count


                foreach (var pair in dict)
                {
                    DoElement(typeInfo, pair.Key);
                    DoJaggedArray<V>(pair.Value, false);          
                }
            }

            return dict;
        }


        public Dictionary<K, Dictionary<V, Dictionary<U, T>>> DoDoubleNestedDictionary<K, V, U, T>(Dictionary<K, Dictionary<V, Dictionary<U, T>>> dict, bool verifyField = true)
        {
            TypeInformation typeInfo;
            GetStaticTypeInfo<K>(out typeInfo);

            Type collectionType = typeof(Dictionary<K, Dictionary<V, Dictionary<U, T>>>);
            string thisTypeName = collectionType.Name + typeInfo.Type.Name + typeof(IDictionary<V, Dictionary<U, T>>).Name;

            if (verifyField)
                VerifyField(collectionType);


            if (mode == Mode.Load)
            {
                string readTypeName = m_reader.ReadString();

                if (readTypeName == "null")
                    return null;

                Debug.Assert(readTypeName == thisTypeName, string.Format("Expected a {0}, but read a {1}", thisTypeName, readTypeName));


                int count = DoInt32(0, false); //reads the number of elements


                Dictionary<K, Dictionary<V, Dictionary<U, T>>> temp = (Dictionary<K, Dictionary<V, Dictionary<U, T>>>)Activator.CreateInstance(collectionType);

                for (int c = 0; c < count; ++c)
                {
                    // call into the same methods as DoCollection here...

                    object key = DoElement(typeInfo, default(K));
                    var value = DoNestedDictionary<V, U, T>(null, false); // create the nested dictionary - works???

                    temp.Add((K)key, value);

                }

                return temp;
            }
            else
            {
                if (mode == Mode.Save)
                {
                    if (dict == null)
                    {
                        m_writer.Write("null");
                    }
                    else
                    {
                        m_writer.Write(thisTypeName); 
                    }
                }

                if (dict == null)
                {
                    return null; // bailout if instance is null
                }

                int count = dict.Count;
                count = DoInt32(count, false);//gets or sets the count


                foreach (var pair in dict)
                {
                    DoElement(typeInfo, pair.Key);
                    DoNestedDictionary<V, U, T>(pair.Value, false); 
                }
            }

            return dict;
        }


        public Dictionary<K, Dictionary<V, List<U>>> DoNestedMultiMap<K, V, U>(Dictionary<K, Dictionary<V, List<U>>> dict, bool verifyField = true)
        {
            TypeInformation typeInfo;
            GetStaticTypeInfo<K>(out typeInfo);

            Type collectionType = typeof(Dictionary<K, Dictionary<V, List<U>>>);
            string thisTypeName = collectionType.Name + typeInfo.Type.Name + typeof(Dictionary<V, List<U>>).Name;

            if (verifyField)
                VerifyField(collectionType);

            if (mode == Mode.Load)
            {
                string readTypeName = m_reader.ReadString();

                if (readTypeName == "null")
                    return null;

                Debug.Assert(readTypeName == thisTypeName, string.Format("Expected a {0}, but read a {1}", thisTypeName, readTypeName));


                int count = DoInt32(0, false); //reads the number of elements


                Dictionary<K, Dictionary<V, List<U>>> temp = (Dictionary<K, Dictionary<V, List<U>>>)Activator.CreateInstance(collectionType);

                for (int c = 0; c < count; ++c)
                {
                    // call into the same methods as DoCollection here...

                    object key = DoElement(typeInfo, default(K));
                    var value = DoMultiMap<V, U>(null, false); 

                    temp.Add((K)key, value);

                }

                return temp;
            }
            else
            {
                if (mode == Mode.Save)
                {
                    if (dict == null)
                    {
                        m_writer.Write("null");
                    }
                    else
                    {
                        m_writer.Write(thisTypeName);
                    }
                }

                if (dict == null)
                {
                    return null; // bailout if instance is null
                }

                int count = dict.Count;
                count = DoInt32(count, false);//gets or sets the count


                foreach (var pair in dict)
                {
                    DoElement(typeInfo, pair.Key);
                    DoMultiMap<V, U>(pair.Value, false);                 
                }
            }

            return dict;
        }


        //public IDictionary<K, IDictionary<V, U>> DoNestedDictionary<K, V, U>(IDictionary<K, IDictionary<V, U>> dict)
        public Dictionary<K, Dictionary<V, U>> DoNestedDictionary<K, V, U>(Dictionary<K, Dictionary<V, U>> dict, bool verifyField = true)
        {
            TypeInformation typeInfo;
            GetStaticTypeInfo<K>(out typeInfo);


            Type collectionType = typeof(Dictionary<K, Dictionary<V, U>>);
            string thisTypeName = collectionType.Name + typeInfo.Type.Name + typeof(IDictionary<V, U>).Name;

            if (verifyField)
                VerifyField(collectionType);


            if (mode == Mode.Load)
            {
                string readTypeName = m_reader.ReadString();

                if (readTypeName == "null")
                    return null;

                Debug.Assert(readTypeName == thisTypeName, string.Format("Expected a {0}, but read a {1}", thisTypeName, readTypeName));


                int count = DoInt32(0, false); //reads the number of elements

                Dictionary<K, Dictionary<V, U>> temp = (Dictionary<K, Dictionary<V, U>>)Activator.CreateInstance(collectionType);

                for (int c = 0; c < count; ++c)
                {
                    // call into the same methods as DoCollection here...

                    object key = DoElement(typeInfo, default(K));
                    var value = DoDictionary<V, U>(null, false); // create the nested dictionary - works???

                    temp.Add((K)key, value);

                }

                return temp;
            }
            else
            {
                if (mode == Mode.Save)
                {
                    if (dict == null)
                    {
                        m_writer.Write("null");
                    }
                    else
                    {

                        m_writer.Write(thisTypeName); 
                    }
                }

                if (dict == null)
                {
                    return null; // bailout if instance is null
                }

                int count = dict.Count;
                count = DoInt32(count, false);//gets or sets the count


                foreach (var pair in dict)
                {
                    DoElement(typeInfo, pair.Key);
                    DoDictionary<V, U>(pair.Value, false); // works???                  
                }
            }


            return dict;
        }


        public SortedList<K, V> DoSortedList<K,V>(SortedList<K, V> sortedList, bool verifyField = true)
        {
           
            TypeInformation keyTypeInfo, valueTypeInfo;
            GetStaticTypeInfo<K>(out keyTypeInfo);
            GetStaticTypeInfo<V>(out valueTypeInfo);


            Type collectionType = typeof(SortedList<K, V>);
            string thisTypeName = collectionType.Name + keyTypeInfo.Type.Name + valueTypeInfo.Type.Name; // include arguments also for more clarity...

            if (verifyField)
                VerifyField(collectionType);

            if (mode == Mode.Load)
            {
                string readTypeName = m_reader.ReadString();

                if (readTypeName == "null")
                    return null;

                Debug.Assert(readTypeName == thisTypeName, string.Format("Expected a {0}, but read a {1}", thisTypeName, readTypeName));

                int count = DoInt32(0, false); //reads the number of elements

                SortedList<K, V> temp = (SortedList<K, V>)Activator.CreateInstance(collectionType);

                for (int c = 0; c < count; ++c)
                {
                    // was object:
                    object key = DoElement(keyTypeInfo, default(K));
                    object value = DoElement(valueTypeInfo, default(V));

                    if (keyTypeInfo.Type.IsEnum)
                    {
                        temp.Add((K)key, (V)value);
                    }
                    else
                    {
                        temp.Add((K)key, (V)value);
                    }

                }

                return temp;
            }
            else
            {
                if (mode == Mode.Save)
                {
                    if (sortedList == null)
                    {
                        m_writer.Write("null");
                    }
                    else
                    {
                        m_writer.Write(thisTypeName);
                    }
                }

                if (sortedList == null)
                {
                    return null; // bailout if instance is null
                }

                int count = sortedList.Count;
                count = DoInt32(count, false); //gets or sets the count

                foreach (KeyValuePair<K, V> pair in sortedList)
                {
                    DoElement(keyTypeInfo, pair.Key);
                    DoElement(valueTypeInfo, pair.Value);
                }
            }

            return sortedList;

        }

       
        /// <summary>
        ///  Generic Dictionaries - remember, there's also a NestedDictionary, a MultiList func and many more
        /// </summary>
        /// <typeparam name="K"></typeparam>
        /// <typeparam name="V"></typeparam>
        /// <param name="dict"></param>
        /// <returns></returns>
        public Dictionary<K, V> DoDictionary<K, V>(Dictionary<K, V> dict, bool verifyField = true)
        {                   

            TypeInformation keyTypeInfo, valueTypeInfo;
            GetStaticTypeInfo<K>(out keyTypeInfo);
            GetStaticTypeInfo<V>(out valueTypeInfo);


            Type collectionType = typeof(Dictionary<K, V>);
            string thisTypeName = collectionType.Name + keyTypeInfo.Type.Name + valueTypeInfo.Type.Name; // include arguments also for more clarity...

            if (verifyField)
                VerifyField(collectionType);


            if (mode == Mode.Load)
            {
                string readTypeName = m_reader.ReadString();
                
                if (readTypeName == "null")
                    return null;

                Debug.Assert(readTypeName == thisTypeName, string.Format("Expected a {0}, but read a {1}", thisTypeName, readTypeName));

                int count = DoInt32(0, false); //reads the number of elements

                Dictionary<K, V> temp = (Dictionary<K, V>)Activator.CreateInstance(collectionType); 

                for (int c = 0; c < count; ++c)
                {                    
                    // was object:
                    object key = DoElement(keyTypeInfo, default(K));
                    object value = DoElement(valueTypeInfo, default(V));

                    if (keyTypeInfo.Type.IsEnum)
                    {
                        temp.Add((K)key, (V)value);
                    }
                    else
                    {
                        temp.Add((K)key, (V)value);
                    }

                   /* temp.Add((K)Convert.ChangeType(key, keyTypeInfo.Type),
                             (V)Convert.ChangeType(value, valueTypeInfo.Type));*/
                   // 
                                        
                }

                return temp;
            }
            else
            {
                if (mode == Mode.Save)
                {
                    if (dict == null)
                    {                       
                        m_writer.Write("null");                     
                    }
                    else
                    {
                        m_writer.Write(thisTypeName); 
                    }
                }

                if (dict == null)
                {
                    return null; // bailout if instance is null
                }

                int count = dict.Count;
                count = DoInt32(count, false); //gets or sets the count

                foreach (KeyValuePair<K, V> pair in dict)
                {
                    DoElement(keyTypeInfo, pair.Key);
                    DoElement(valueTypeInfo, pair.Value);    
                }
            }

            return dict;
        }


        public SortedDictionary<K, V> DoSortedDictionary<K, V>(SortedDictionary<K, V> dict, bool verifyField = true)
        {

            TypeInformation keyTypeInfo, valueTypeInfo;
            GetStaticTypeInfo<K>(out keyTypeInfo);
            GetStaticTypeInfo<V>(out valueTypeInfo);


            Type collectionType = typeof(SortedDictionary<K, V>);
            string thisTypeName = collectionType.Name + keyTypeInfo.Type.Name + valueTypeInfo.Type.Name; // include arguments also for more clarity...

            if (verifyField)
                VerifyField(collectionType);


            if (mode == Mode.Load)
            {
                string readTypeName = m_reader.ReadString();

                if (readTypeName == "null")
                    return null;

                Debug.Assert(readTypeName == thisTypeName, string.Format("Expected a {0}, but read a {1}", thisTypeName, readTypeName));

                int count = DoInt32(0, false); //reads the number of elements

                SortedDictionary<K, V> temp = (SortedDictionary<K, V>)Activator.CreateInstance(collectionType);

                for (int c = 0; c < count; ++c)
                {
                    // was object:
                    object key = DoElement(keyTypeInfo, default(K));
                    object value = DoElement(valueTypeInfo, default(V));

                    if (keyTypeInfo.Type.IsEnum)
                    {
                        temp.Add((K)key, (V)value);
                    }
                    else
                    {
                        temp.Add((K)key, (V)value);
                    }

                    /* temp.Add((K)Convert.ChangeType(key, keyTypeInfo.Type),
                              (V)Convert.ChangeType(value, valueTypeInfo.Type));*/
                    // 

                }

                return temp;
            }
            else
            {
                if (mode == Mode.Save)
                {
                    if (dict == null)
                    {
                        m_writer.Write("null");
                    }
                    else
                    {
                        m_writer.Write(thisTypeName);
                    }
                }

                if (dict == null)
                {
                    return null; // bailout if instance is null
                }

                int count = dict.Count;
                count = DoInt32(count, false); //gets or sets the count

                foreach (KeyValuePair<K, V> pair in dict)
                {
                    DoElement(keyTypeInfo, pair.Key);
                    DoElement(valueTypeInfo, pair.Value);
                }
            }

            return dict;
        }

        public SerializableDictionary<K, V> DoSerializableDictionary<K, V>(SerializableDictionary<K, V> dict, bool verifyField = true)
        {

            TypeInformation keyTypeInfo, valueTypeInfo;
            GetStaticTypeInfo<K>(out keyTypeInfo);
            GetStaticTypeInfo<V>(out valueTypeInfo);


            Type collectionType = typeof(SerializableDictionary<K, V>);
            string thisTypeName = collectionType.Name + keyTypeInfo.Type.Name + valueTypeInfo.Type.Name; // include arguments also for more clarity...

            if (verifyField)
                VerifyField(collectionType);


            if (mode == Mode.Load)
            {
                string readTypeName = m_reader.ReadString();

                if (readTypeName == "null")
                    return null;

                Debug.Assert(readTypeName == thisTypeName, string.Format("Expected a {0}, but read a {1}", thisTypeName, readTypeName));

                int count = DoInt32(0, false); //reads the number of elements

                SerializableDictionary<K, V> temp = (SerializableDictionary<K, V>)Activator.CreateInstance(collectionType);

                for (int c = 0; c < count; ++c)
                {
                    // was object:
                    object key = DoElement(keyTypeInfo, default(K));
                    object value = DoElement(valueTypeInfo, default(V));

                    if (keyTypeInfo.Type.IsEnum)
                    {
                        temp.Add((K)key, (V)value);
                    }
                    else
                    {
                        temp.Add((K)key, (V)value);
                    }

                    /* temp.Add((K)Convert.ChangeType(key, keyTypeInfo.Type),
                              (V)Convert.ChangeType(value, valueTypeInfo.Type));*/
                    // 

                }

                return temp;
            }
            else
            {
                if (mode == Mode.Save)
                {
                    if (dict == null)
                    {
                        m_writer.Write("null");
                    }
                    else
                    {
                        m_writer.Write(thisTypeName);
                    }
                }

                if (dict == null)
                {
                    return null; // bailout if instance is null
                }

                int count = dict.Count;
                count = DoInt32(count, false); //gets or sets the count

                foreach (KeyValuePair<K, V> pair in dict)
                {
                    DoElement(keyTypeInfo, pair.Key);
                    DoElement(valueTypeInfo, pair.Value);
                }
            }

            return dict;
        }


        public SerializableDictionary<K, List<V>> DoSerializableMultiMap<K, V>(SerializableDictionary<K, List<V>> dict, bool verifyField = true)
        {
            TypeInformation typeInfo;
            GetStaticTypeInfo<K>(out typeInfo);


            Type collectionType = typeof(SerializableDictionary<K, List<V>>);
            string thisTypeName = collectionType.Name + typeInfo.Type.Name + typeof(List<V>).Name;

            if (verifyField)
                VerifyField(collectionType);

            if (mode == Mode.Load)
            {
                string readTypeName = m_reader.ReadString();

                if (readTypeName == "null")
                    return null;

                Debug.Assert(readTypeName == thisTypeName, string.Format("Expected a {0}, but read a {1}", thisTypeName, readTypeName));


                int count = DoInt32(0, false); //reads the number of elements


                SerializableDictionary<K, List<V>> temp = (SerializableDictionary<K, List<V>>)Activator.CreateInstance(collectionType);

                for (int c = 0; c < count; ++c)
                {
                    // call into the same methods as DoCollection here...

                    object key = DoElement(typeInfo, default(K)); // value is don't care when loading
                    var value = DoList<V>(null, false); // create the list

                    temp.Add((K)key, value);

                }

                return temp;
            }
            else
            {
                if (mode == Mode.Save)
                {
                    if (dict == null)
                    {
                        m_writer.Write("null");
                    }
                    else
                    {
                        m_writer.Write(thisTypeName);
                    }
                }

                if (dict == null)
                {
                    return null; // bailout if instance is null
                }

                int count = dict.Count;
                count = DoInt32(count, false);//gets or sets the count


                foreach (var pair in dict)
                {
                    DoElement(typeInfo, pair.Key);
                    DoList<V>(pair.Value, false);
                }
            }

            return dict;
        }

        private class SnapshotClassVerification
        {
            public Type Type;
           
            public bool IsVerified;

            public List<FieldInfo> Fields = new List<FieldInfo>();
            public Dictionary<Type, List<FieldInfo>> FieldsByType = new Dictionary<Type, List<FieldInfo>>();
         //   public Dictionary<string, List<FieldInfo>> FieldsByType = new Dictionary<string, List<FieldInfo>>();

            /// <summary>
            /// this list gets removed from as Do and Ignore calls are made.
            /// </summary>
            public Dictionary<Type, int> TypesUnaccountedFor = new Dictionary<Type, int>();

            /// <summary>
            /// types that we are not ignoring, but want to keep in the log in order to do them later...
            /// </summary>
            public Dictionary<Type, int> PostponedTypes = new Dictionary<Type, int>();

          //  public Dictionary<string, int> TypesUnaccountedFor = new Dictionary<string, int>();
           
            public SnapshotClassVerification(Type snapshotType)
            {
                this.Type = snapshotType;

                // first time we encounter this type, create a list of its properties and fields and store them:
                // the flags don't work as expected, why..?
                BindingFlags flags = 
                    BindingFlags.Public | 
                    BindingFlags.NonPublic |
                    BindingFlags.Static | 
                    BindingFlags.Instance |                  
                    BindingFlags.DeclaredOnly;

              
                //FieldInfo[] fields = snapshotType.GetFields(flags);
                FieldInfo[] fields = GetFieldInfosIncludingBaseClasses(snapshotType, flags); 

              //  MemberInfo[] members = snapshotType.GetMembers();  // Members include properties, methods, fields, events, and so on.

                //foreach (var field in Fields)

                // filter the fields of some of the garbage we don't care about:
                FieldInfo field;
                for (int i = fields.Length - 1; i >= 0; i--)
                {
                    field = fields[i];

                    int noOfTypes = 0;
                        
                    // now filter:
                    if (field.FieldType != typeof(Version)
                        && field.FieldType != typeof(Regulator)
                        && field.FieldType != typeof(Renderable)
                        && !field.IsLiteral                         // ignore constants
                        && !field.Name.Contains("IsSnapshotted")
                        && !field.Name.Contains("CachedAnonymousMethod"))
                    {
                        Type keyAsType = field.FieldType;
                        string key = field.FieldType.Name;
                        Fields.Add(field);

                     /*   if (!TypesUnaccountedFor.TryGetValue(key, out noOfTypes))
                        {
                            TypesUnaccountedFor.Add(key, 1); // save the type name and count
                        }
                        else
                        {
                            noOfTypes++; // increase current count
                            TypesUnaccountedFor[key] = noOfTypes;
                        }*/

                        if (!TypesUnaccountedFor.TryGetValue(keyAsType, out noOfTypes))
                        {
                            TypesUnaccountedFor.Add(keyAsType, 1); // save the type name and count
                        }
                        else
                        {
                            noOfTypes++; // increase current count
                            TypesUnaccountedFor[keyAsType] = noOfTypes;
                        }

                        Common.AddToMultiList(FieldsByType, keyAsType, field);
                    }
                }

            }

            public void SetTypeAsAccountedFor(Type type, bool postpone = false) 
            {
                int noOfTypes;
                if (TypesUnaccountedFor.TryGetValue(type, out noOfTypes))
                {
                    noOfTypes--;
                    if (noOfTypes <= 0)
                    {
                        TypesUnaccountedFor.Remove(type);
                    }
                    else
                    {
                        TypesUnaccountedFor[type] = noOfTypes;
                    }

                    if (postpone)
                    {
                        Common.AddToDictWithSums(PostponedTypes, type);
                    }
                }   
            }

            

        }

        public static FieldInfo[] GetFieldInfosIncludingBaseClasses(Type type, BindingFlags bindingFlags)
        {
            FieldInfo[] fieldInfos = type.GetFields(bindingFlags);

            // If this class doesn't have a base, don't waste any time
            if (type.BaseType == typeof(object))
            {
                return fieldInfos;
            }
            else
            {   // Otherwise, collect all types up to the furthest base class
                var currentType = type;
                var fieldComparer = new FieldInfoComparer();
                var fieldInfoList = new HashSet<FieldInfo>(fieldInfos, fieldComparer);
                while (currentType != typeof(object))
                {
                    fieldInfos = currentType.GetFields(bindingFlags);
                    fieldInfoList.UnionWith(fieldInfos);
                    currentType = currentType.BaseType;
                }
                return fieldInfoList.ToArray();
            }
        }

        private class FieldInfoComparer : IEqualityComparer<FieldInfo>
        {
            public bool Equals(FieldInfo x, FieldInfo y)
            {
                return x.DeclaringType == y.DeclaringType && x.Name == y.Name;
            }

            public int GetHashCode(FieldInfo obj)
            {
                return obj.Name.GetHashCode() ^ obj.DeclaringType.GetHashCode();
            }
        }




        private void EndVerifyISnapshot() 
        {
            #if DEBUG         

            SnapshotClassVerification snapshotClass = snapshotClassBeingVerified;

            // look up the field names for this type - the list should be empty if all fields are accounted for:
            if (snapshotClass != null) 
            {                
                if (snapshotClass.TypesUnaccountedFor.Count > 0)
                {
                    // some fields were unaccounted for:

                    Log(snapshotClass.Type.Name + " had the following types unaccounted for in DoSnapshot: ", LogPriority.high);
                    
                    foreach (var item in snapshotClass.TypesUnaccountedFor)
                    {
                        string listOfFieldCandidates = string.Join(", ", snapshotClass.FieldsByType[item.Key].Select(f => f.Name));
                        
                        Log(string.Format("{0} ({1}), possible fields: {2}", item.Key, item.Value, listOfFieldCandidates));
                        
                    }

                    Log("");
                }

                if (snapshotClass.PostponedTypes.Count > 0)
                {
                    Log(snapshotClass.Type.Name + " postponed snapshotting the following types (perhaps review these later): ", LogPriority.high);
                  
                    foreach (var item in snapshotClass.PostponedTypes)
                    {
                        Log(string.Format("{0} ({1})", item.Key, item.Value));                        
                    }

                    Log("");
                }
                
                snapshotClass.IsVerified = true;
               
            }

            snapshotClassesBeingVerified.Pop(); // done with this class - continue with the one we left off from (if any)   
             
            ReturnToVerifyingPreviousClass();

            #endif
        }


        private bool VerifyParameterlessConstructorsExist()
        {
#if DEBUG
            bool hasLoggedHeader = false;

            BindingFlags flags = 
                    BindingFlags.Public | 
                    BindingFlags.NonPublic |
                 //   BindingFlags.Static | // don't look for static ctors
                    BindingFlags.Instance |                  
                    BindingFlags.DeclaredOnly;

            // get all types that implement ISnapshot
            foreach (Type snapshotType in System.Reflection.Assembly.GetExecutingAssembly().GetTypes().Where(mytype => mytype.GetInterfaces().Contains(typeof(ISnapshot))))
            {
                if (!snapshotType.IsInterface)
                {
                    // get the parameterless ctor:
                    ConstructorInfo ctorInfo = snapshotType.GetConstructor(flags, null, System.Type.EmptyTypes, null);

                
                    if (ctorInfo == null)
                    {
                        // no paramless ctor found. this will be alright as long as no other ctors are defined:
                        ConstructorInfo[] allPublicCtors = snapshotType.GetConstructors();
                        if (allPublicCtors != null && allPublicCtors.Length > 0)
                        {
                            if (!hasLoggedHeader)
                            {
                                Log("The following ISnapshot types do not include a parameterless constructor: ", LogPriority.high);
                                hasLoggedHeader = true;
                            }

                            Log(string.Format("{0}", snapshotType.FullName), LogPriority.high);
                        }
                    }
                }
            }

            return !hasLoggedHeader;
#endif

            return true;
        }

        private void ReturnToVerifyingPreviousClass()
        {
            if (snapshotClassesBeingVerified.Count > 0)
            {
                snapshotClassBeingVerified = snapshotClassesBeingVerified.Peek();
            }
            else
            {
                snapshotClassBeingVerified = null;
            }
        }

               

        /// <summary>
        /// this generic overload will give the correct type for save/load, even when the instance is null.
        /// retrieving the type info statically is also supposedly faster than doing it dynamically with GetType()
        /// 
        /// Any class that calls DoISnapshot is responsible for calling LoadPostProcess as well!
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="snap"></param>
        /// <returns></returns>
        public ISnapshot DoISnapshot<T>(T snap, bool verifyField = true) where T : ISnapshot
        {
            Type type = typeof(T); // static type... often different from instance type

            return DoISnapshot(snap, type, verifyField);

        }

        /// <summary>    
        /// this is the method for snapshotting complex types.
        /// The method will call the instance's DoSnapshot method, so the type is responsible for snapshotting itself.
        /// 
        /// Also calls DoVersion on the object.
        /// </summary>
        /// <param name="instanceToSnapshot"></param>
        /// <returns></returns>
        public ISnapshot DoISnapshot(ISnapshot instanceToSnapshot, Type staticOrDynamicType, bool verifyField = true) 
        {            
            
            ISnapshot instanceToReturn = null;


            if (mode == Mode.Load)
            {                
                // we have no use for instanceToSnapshot when loading.

                string typeNameInFile = m_reader.ReadString();
                if (typeNameInFile == "null")
                {
                    return null;
                }

                if (staticOrDynamicType.IsInterface //"ICyclable"
                    || staticOrDynamicType.IsAbstract) //"ResourceContainer"
                {
                    // dynamic case - for interface types, use the concrete class name that was written in order to create an instance:
                    instanceToReturn = Activator.CreateInstance(Type.GetType(typeNameInFile)) as ISnapshot; // will throw no empty ctor exception if none has been defined
                }               
                else
                {
                    // static case - create a new instance that can be filled with loaded data
                    instanceToReturn = Activator.CreateInstance(staticOrDynamicType) as ISnapshot; // will throw no empty ctor exception if none has been defined
               }

                Version version = instanceToReturn.DoVersion(this); // read in the version number. sets the version before DoSnapshot gets called, so it can branch/repair itself if necessary

                instanceToReturn = instanceToReturn.DoSnapshot(this); // this will fill the instance with loaded data
               
            }
            else if (mode == Mode.Save)
            {
                // we only self-verify during save
                if (verifyField)
                    VerifyField(staticOrDynamicType);

                Type dynamicType = null;
                StartVerifyISnapshot(instanceToSnapshot, out dynamicType);
            


                if (instanceToSnapshot == null)
                {
                    m_writer.Write("null");

                    instanceToReturn = null;
                    //return null;
                }
                else
                {
                    
                    if (instanceToSnapshot.IsSnapshotted)
                    {
                        //   Log(instanceToSnapshot.ToString() + " was already snapshotted once. Skipping..");
                        // the object was already snapshotted once. most likely an error here...
                        throw new Exception("calling Do(ISnapshot) on the same object twice. That's wrong.");
                    }

                    if (staticOrDynamicType.IsInterface // "ICyclable"
                        || staticOrDynamicType.IsAbstract) //"ResourceContainer"
                    {
                        if (dynamicType == null)
                        {
                            // we can save this op if done previously:
                            dynamicType = instanceToSnapshot.GetType();
                        }

                        m_writer.Write(dynamicType.FullName); // for interface types, write the full class name that will be needed to create an instance when loading.
                    }
                    else
                    {
                        m_writer.Write(staticOrDynamicType.Name); // write the short name... this will not be used during deserialization - it is only for debugging/verification purposes.
                    }

                    instanceToSnapshot.DoVersion(this); //Save the version. the version is a separate abstract method, to ensure that it is implemented by every ISnapshot class
                    instanceToReturn = instanceToSnapshot.DoSnapshot(this);

                    instanceToSnapshot.IsSnapshotted = true; // the same..?
                    instanceToReturn.IsSnapshotted = true;

                    RegisterSnapshottedISnapshot(instanceToReturn, dynamicType ?? staticOrDynamicType);
                }

                EndVerifyISnapshot(); //instanceToSnapshot, dynamicType);

            }
                      

            return instanceToReturn;
        }

        /// <summary>
        /// Call this from LoadPostProcess to help in verifying snapshot completeness and catch bugs
        /// </summary>
        /// <param name="instance"></param>
        public void RegisterLoadPostProcessCall(ISnapshot instance) 
        {
#if DEBUG
            Type type = instance.GetType();
        
            Common.RemoveFromDictWithSums(allSnapshottedISnapshots, type);

            //instance.IsSnapshotted = false; // reset for next save...
#endif
        }

        private void RegisterSnapshottedISnapshot(ISnapshot instance, Type type)
        {
#if DEBUG
            Common.AddToDictWithSums(allSnapshottedISnapshots, type);
           // Common.AddToMultiSet(allSnapshottedISnapshots, type, instance);
          
            /*if (!loadPostProcessCallPending.ContainsKey(type))
            {
                loadPostProcessCallPending.Add(type, type);
            }*/
#endif
        }


        private void VerifyLoadPostProcessCallsComplete()
        {
#if DEBUG
            bool hasLoggedHeader = false;
            if (allSnapshottedISnapshots.Count > 0)
            {
                                 
                foreach (var item in allSnapshottedISnapshots)
                {
                    if (item.Value > 0)
                    {
                        if (!hasLoggedHeader)
                        {
                            Log("Snapshot incomplete. The following ISnapshot types did not receive a LoadPostProcess() call: ", LogPriority.high);
                            hasLoggedHeader = true;
                        }

                        Log(string.Format("{0} ({1})", item.Key.FullName, item.Value), LogPriority.high);
                    }
                }
            }

            allSnapshottedISnapshots.Clear();
#endif

        }


        /// <summary>
        /// only in Save, and in Debug mode!
        /// 
        /// save the short type names of the all the properties? fields (not constants), also statics
        /// then, in each Do call, remove a type name from the list
        /// also Ignore calls
        /// 
        /// at the end of DoISnapshot, the list should be empty. Otherwise, the class has new fields which are not handled.
        /// </summary>
        /// <param name="iSnapshotType"></param>
        private void StartVerifyISnapshot(ISnapshot instanceToSnapshot, out Type dynamicType)
        {
            dynamicType = null;
#if DEBUG               

            if (verifyCompleteness == false)
                return;

            if (instanceToSnapshot == null) // we need an instance to get the correct type. so this proc does not work during Load.
            {
                snapshotClassBeingVerified = null; // set to currently not verifying

                snapshotClassesBeingVerified.Push(null); // NEW: push null on the stack..

                return;
            }

           
            dynamicType = instanceToSnapshot.GetType();

            SnapshotClassVerification snapshotClass;

            // look up the info for this type:
            if (snapshotClasses.TryGetValue(dynamicType, out snapshotClass)
               /* && snapshotClass.IsVerified == true*/) 
            {  
                snapshotClassBeingVerified = null; // set to currently not verifying

                snapshotClassesBeingVerified.Push(null); // NEW: push null on the stack..
                
                return; // only verify the class once
            }
            
            
            if (snapshotClass == null) 
            {
                snapshotClass = new SnapshotClassVerification(dynamicType);
                snapshotClasses.Add(dynamicType, snapshotClass);
            }

            snapshotClassesBeingVerified.Push(snapshotClass); // put in on the stack so we can verify other classes in a nested fashion in between
            snapshotClassBeingVerified = snapshotClass; // start verifying this class
#endif
        }


       
        /// <summary>
        /// We have an Ignore Method to foil the instrumentation that detects broken saves
        /// </summary>
        /// <param name="a"></param>
       /* public void Ignore(Object a)
        {
            if (a != null)
            {
                //verification code
                names.Remove(a.GetType().Name);
            }

            // won't work for null type
      
        }*/

        public void Postpone<T>(T a)
        {
#if DEBUG
            if (snapshotClassBeingVerified != null)
            {
                //  Type dynamicType = a.GetType();
                Type staticType = typeof(T);

                snapshotClassBeingVerified.SetTypeAsAccountedFor(staticType, true);
            }
#endif
        }

        public void Ignore<T>(T a)
        {
#if DEBUG
            if (snapshotClassBeingVerified != null)
            {
              //  Type dynamicType = a.GetType();
                Type staticType = typeof(T);

                snapshotClassBeingVerified.SetTypeAsAccountedFor(staticType); //.Name);
            }
#endif
        }

        public static void Log(string message, LogPriority p = LogPriority.infinity)
        {
           // return; // this silences all logs

            string builtMessage = "";
            for (int t = 0; t < (int)LogPriority.infinity - (int)p; ++t)
                builtMessage += "    ";

            builtMessage += message;

            if (p >= m_logPriority)
                Console.WriteLine(builtMessage);
        }

       /* public static List<Id> GetIDs<T, Id>(IEnumerable<T> instance)
            where T : ILookUp<T, Id>          
        {
            if (instance != null)
            {
                return instance.Select(i => i.ID).ToList();
            }
            else return null;
        }*/

       

        /// <summary>
        /// Save: if instance is not null, extracts an ID and snapshots it as a nullable ulong (ulong?)
        /// Load: don't care about instance, returns a loaded id
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="Id"></typeparam>
        /// <param name="instance"></param>
        /// <returns></returns>
        public Id? SnapshotID<T, Id>(T instance)
            where T : ILookUp<T, Id>
            where Id : struct,  IComparable, IFormattable, IConvertible // the closest constraint to enum we can get..
           // where Id : struct // nullable is a struct
        {
            //Let's verify that both the ILookUp type and the id type are accounted for:
            VerifyField(typeof(T)); // won't work if it is an interface type like ICyclable
            VerifyField(typeof(Id));
                       
            if (instance != null)
            {
                return DoEnumNullable<Id>(instance.ID, false);

               /* ulong? enumAsULong = (ulong)((ValueType)(instance.ID)); // cannot cast to ulong?, because we can only unbox to the same type (ulong)...

                ulong? returnedULong = DoEnumNullable(enumAsULong, false);

                ValueType returnedValue = (ValueType)returnedULong;

                return (Id)returnedValue; // cannot cast to Id?, probably for the same reason? Will always have a value
                */
            }
            else
            {

                return DoEnumNullable<Id>((Id?)null, false); 

               /* ulong? returnedULong = DoEnumNullable((ulong?)null, false); 

                if (returnedULong.HasValue)
                {
                    return (Id)((ValueType)returnedULong);
                }
                else
                {
                    return null;
                }*/
            }
        }






        
    }

}

