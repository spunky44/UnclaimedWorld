using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Processes;
using UWGame.SimSide.Snapshots;

namespace UWGame.ClientSide.Interface.Inventory
{
    /// <summary>
    /// create an instance of this when the user decides to track an entity type by clicking on the button
    /// 
    /// cache this???
    /// </summary>
    public class TrackTarget: ISnapshot
    {
        public EntityType EntityType;

        public bool ShowInputs
        {
            get;
            private set;
        }
        public bool ShowOutputs
        {
            get;
            private set;
        }
        public bool ShowTools
        {
            get;
            private set;
        }

        public Color Color;


        public HashSet<EntityType> InputForTrackTarget = null; 
        public HashSet<EntityType> OutputForTrackTarget = null; 
        public HashSet<EntityType> ToolsForTrackTarget = null; 
     
        public TrackTarget()
        { }

        public TrackTarget(EntityType entityType,
            bool showInputs,
            bool showOutputs,
            bool showTools)
        {
            EntityType = entityType;
            ShowInputs = showInputs;
            ShowOutputs = showOutputs;
            ShowTools = showTools;

            ComputeRelatedEntityTypes();

        }


        /// <summary>
        /// call this when salvage filter button changes...
        /// </summary>
        public void RecomputeRelatedEntityTypes()
        {
            InputForTrackTarget = null;
            OutputForTrackTarget = null;
            ToolsForTrackTarget = null;

            ComputeRelatedEntityTypes();
        }

        public void SetTrackingOptions(bool trackInputs, bool trackOutputs, bool trackTools)
        {
            bool hasChanged = false;
            if (ShowOutputs != trackOutputs)
            {
                ShowOutputs = trackOutputs;
                hasChanged = true;
            }

            if (ShowInputs != trackInputs)
            {
                ShowInputs = trackInputs;
                hasChanged = true;
            }

            if (ShowTools != trackTools)
            {
                ShowTools = trackTools;
                hasChanged = true;
            }

            if (hasChanged)
            {                
                The.InGameUI.InventorySettings.TrackTargetSettingsChanged();
            }
        }

        private void ComputeRelatedEntityTypes()
        {
            ComputeInputs();
            ComputeOutputs();
            ComputeTools();
        }

        private void ComputeInputs()
        {
            if (InputForTrackTarget == null)
            {
                InputForTrackTarget = new HashSet<EntityType>();

                HandleInput(EntityType);

            }
        }

        private void HandleInput(EntityType entityType)
        {
            List<ProcessType> processes;
            if (GameData.Instance.ProcessYieldsThisOutput.TryGetValue(entityType, out processes))
            {
                foreach (var process in processes)
                {
                    HandleProcessInput(process);
                }
            }

            // add salvage processes here:
          /*  if (The.InGameUI.InventorySettings.IncludeSalvageProcesses)
            {
                if (GameData.Instance.SalvageProcessYieldsThisOutput.TryGetValue(entityType, out processes))
                {
                    foreach (var process in processes)
                    {
                        HandleProcessInput(process);
                    }
                }
            }*/
        }

        private void HandleProcessInput(ProcessType process)
        {
            if (process.InputsByType != null)
            {
                foreach (var input in process.InputsByType)
                {
                    if (!InputForTrackTarget.Contains(input.Key))
                    {
                      
                        InputForTrackTarget.Add(input.Key);

                        HandleInput(input.Key); // recursion
                    }
                }
            }
        }

        private void ComputeOutputs()
        {
            //GameData.Instance.ProcessesUsingThisInput;

            if (OutputForTrackTarget == null)
            {
                OutputForTrackTarget = new HashSet<EntityType>();

                HandleOutput(EntityType);

                HandleAsToolOutput(EntityType);
                
            }
        }

        private void HandleOutput(EntityType entityType)
        {
            List<ProcessType> processes;
            if (GameData.Instance.ProcessesUsingThisInput.TryGetValue(entityType, out processes))
            {
                foreach (var process in processes)
                {
                    HandleProcessOutput(process);
                }
            }

            // add salvage processes here:
          /*  if (The.InGameUI.InventorySettings.IncludeSalvageProcesses)
            {
                if (GameData.Instance.SalvageProcessUsingThisInput.TryGetValue(entityType, out processes))
                {
                    foreach (var process in processes)
                    {
                        HandleProcessOutput(process);
                    }
                }
            }*/
        }

        private void HandleProcessOutput(ProcessType process)
        {
            if (process.Outputs != null)
            {
                foreach (var input in process.Outputs)
                {
                    if (!OutputForTrackTarget.Contains(input.FinalEntityTypeToCreate))
                    {
                        OutputForTrackTarget.Add(input.FinalEntityTypeToCreate);

                        HandleOutput(input.FinalEntityTypeToCreate);
                    }
                }
            }
        }


        private void HandleAsToolOutput(EntityType entityType)
        {
            foreach (var process in GameData.Instance.AllProcessTypes)
            {
                if (process.Value.ProcessToolSet != null)
                {
                    HandleProcessForOutputsUsingTool(entityType, process.Value);
                }
            }
        }

        private void HandleProcessForOutputsUsingTool(EntityType entityType, ProcessType process)
        {
            foreach (var input in process.ProcessToolSet.Tools)
            {
                foreach (var tools in input.Tools)
                {
                    foreach (var tool in tools.ToolEntityTypes)
                    {
                        if (tool == entityType)
                        {
                            if (process.Outputs != null)
                            {
                                foreach (var output in process.Outputs)
                                {
                                    if (!OutputForTrackTarget.Contains(output.FinalEntityTypeToCreate))
                                    {
                                        OutputForTrackTarget.Add(output.FinalEntityTypeToCreate);
                                    }
                                }
                            }
                            // done with this process.
                            return;

                        }
                    }
                }
            }
           
        }

        private void ComputeTools()
        {
            if (ToolsForTrackTarget == null)
            {
                ToolsForTrackTarget = new HashSet<EntityType>();

                HandleTools(EntityType);
            }
        }
        private void HandleTools(EntityType entityType)
        {
             List<ProcessType> processes; 
             if (GameData.Instance.ProcessYieldsThisOutput.TryGetValue(entityType, out processes))
             {
                 foreach (var process in processes)
                 {
                     if (process.ProcessToolSet != null)
                     {
                         foreach (var input in process.ProcessToolSet.Tools)
                         {
                             foreach (var tools in input.Tools)
                             {
                                 foreach (var tool in tools.ToolEntityTypes)
                                 {
                                     if (!ToolsForTrackTarget.Contains(tool))
                                     {
                                         ToolsForTrackTarget.Add(tool);
                                         HandleTools(tool);
                                     }
                                 }
                             }                             
                         }
                     }
                 }
             } 
        }

    
        #region ISnapshot

        /// <summary>
        /// when changes are made to the fields that should be snapshotted, such as type changes, addition or removal of fields, increase this version number!
        /// </summary>
        Snapshotter.Version version = Snapshotter.Version.Original;
        public Snapshotter.Version DoVersion(Snapshotter sn)
        {
            version = sn.DoVersion(Snapshotter.Version.Original); // increase this number and make sure to add repair code in DoSnapshot to bring older versions up to this new version!
            return version;
        }

        public bool IsSnapshotted { get; set; }

        public ISnapshot DoSnapshot(Snapshotter sn)
        {
            this.ShowInputs = sn.DoBool(ShowInputs);
            this.ShowOutputs = sn.DoBool(ShowOutputs);
            this.ShowTools = sn.DoBool(ShowTools);

            this.Color = sn.DoColor(Color);

            this.EntityType = sn.DoGameData(EntityType);

            this.InputForTrackTarget = sn.DoHashSet(InputForTrackTarget);
            this.OutputForTrackTarget = sn.DoHashSet(OutputForTrackTarget);
            this.ToolsForTrackTarget = sn.DoHashSet(ToolsForTrackTarget);

            return this;
        }

        public void LoadPostProcess(Snapshotter sn)
        {
            sn.RegisterLoadPostProcessCall(this);
        }

        #endregion
    }
}
