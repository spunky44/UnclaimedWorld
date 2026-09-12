using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Processes;

namespace UWGame.SimSide.AllGameData
{
    public class ToolsLoader
    {
        private const float HighDegrade = 0.05f;
        private const float MediumDegrade = 0.02f;
        private const float LowDegrade = 0.005f;
        private const float NoDegrade = 0f;

        public static List<ProcessToolSet> InitProcessToolSets()
        {
            /* NOTE ON PRODUCTIVITY: We should pick a tech level for the number 1 and stick with it. For instance 2014 hand tools tech like chainsaw, powergrinder, electric drill... 
          * Then we can scale stone age tech and future tech accordingly. (Please do not consider mass production, robots, big workshops or molecular machines yet, that will only confuse us even more.) ONLY hand tools for now.
 
            * note that knife, string and machete are future materials and have extremely* good properties compared to the today versions. (*Lars says the knife blade will just be a bit better than the best of todays blades.)
            * 
            * 
          */
            ToolAlternatives harvestFieldCropsTools =                   
                        new ToolAlternatives() { Tools = new []
                        {                                           //BENCHMARK: weedingRobotTool 1f most everything else copied from "toolSetHarvestSmallBranches"
                            new Tool(){ UsesToolKeyName = "item:weedingRobotTool", ProductivityFactor = 1.0f, DegradePerSecondOfUse = NoDegrade },
                            new Tool(){ UsesToolKeyName = "item:advancedMachete", ProductivityFactor = 0.5f, DegradePerSecondOfUse = NoDegrade  },//was LowDegrade 
                            new Tool(){ UsesToolKeyName = "item:steelMachete", ProductivityFactor = 0.3f, DegradePerSecondOfUse = NoDegrade  }, 

                                                                                       //   "item:flintKnife" // mp a flint knife cannot take much abuse...best for small, single objects that can be whittled on.
                             new Tool(){ UsesToolKeyName = "item:improvisedKnife", ProductivityFactor = 0.05f, DegradePerSecondOfUse = NoDegrade  },
                            new Tool(){ UsesToolKeyName = "item:steelKnife", ProductivityFactor = 0.10f, DegradePerSecondOfUse = NoDegrade  },
                            new Tool(){ UsesToolKeyName = "item:advancedKnife", ProductivityFactor = 0.15f, DegradePerSecondOfUse = NoDegrade  },

                            new Tool(){ UsesToolKeyName = "item:steelHandAxe", ProductivityFactor = 0.08f, DegradePerSecondOfUse = NoDegrade  },
                            new Tool(){ UsesToolKeyName = "item:improvisedHandAxe", ProductivityFactor = 0.06f, DegradePerSecondOfUse = NoDegrade  }
                        }                         
                        
                    };

            // sowing, weeding and harvesting?
            ToolAlternatives sowingWeedingFertilizingFarmPlotTools = new ToolAlternatives() //tilling soil which is already established as farm plot. hoe is good for this.
            {
                Tools = new[]
                            { 
                                new Tool(){ UsesToolKeyName = "item:steelHoe", ProductivityFactor = 0.6f, DegradePerSecondOfUse = NoDegrade },
                                new Tool(){ UsesToolKeyName = "item:farmingHoe", ProductivityFactor = 0.5f, DegradePerSecondOfUse = NoDegrade },
                                new Tool(){ UsesToolKeyName = "item:weedingRobotTool", ProductivityFactor = 1.0f, DegradePerSecondOfUse = NoDegrade },
                                new Tool(){ UsesToolKeyName = "item:steelSpade", ProductivityFactor = 0.15f, DegradePerSecondOfUse = NoDegrade }, 
                                new Tool(){ UsesToolKeyName = "item:improvisedSpade", ProductivityFactor = 0.1f, DegradePerSecondOfUse = NoDegrade },
                              
                            }
            };

            ToolAlternatives harvestGreenhouseTools = new ToolAlternatives()
            {
                Tools = new[]
                            {  
                                new Tool(){ UsesToolKeyName = "item:flintKnife", ProductivityFactor = 0.3f, DegradePerSecondOfUse = NoDegrade },
                                new Tool(){ UsesToolKeyName = "item:improvisedKnife", ProductivityFactor = 0.65f, DegradePerSecondOfUse = NoDegrade },
                                new Tool(){ UsesToolKeyName = "item:steelKnife", ProductivityFactor = 0.80f, DegradePerSecondOfUse = NoDegrade },
                                new Tool(){ UsesToolKeyName = "item:advancedKnife", ProductivityFactor = 0.85f, DegradePerSecondOfUse = NoDegrade },
                                new Tool(){ UsesToolKeyName = "item:improvisedTrowel", ProductivityFactor = 0.4f, DegradePerSecondOfUse = NoDegrade },
                            }
            };


                List<ProcessToolSet> listOfToolProfiles = new List<ProcessToolSet>();
            
                #region Pseudo tools, farming and fishing
                listOfToolProfiles.Add(new ProcessToolSet()
                {
                    KeyName = "toolSetPseudoFarmplot",
                    Tools = new[] 
                    {                    
                        new ToolAlternatives() 
                        { Tools = new []
                            { 
                                new Tool(){ UsesToolKeyName = "structure:smallPlot", ProductivityFactor = 0.5f, DegradePerSecondOfUse = NoDegrade  }, 
                                new Tool(){ UsesToolKeyName = "structure:largePlot", ProductivityFactor = 0.5f, DegradePerSecondOfUse = NoDegrade  }                              
                            }                         
                        },                      
                        sowingWeedingFertilizingFarmPlotTools, // sowing/planting
                        harvestFieldCropsTools, // harvesting
                    }
                });

                listOfToolProfiles.Add(new ProcessToolSet()
                {
                    KeyName = "toolSetPseudoGreenhouse",
                    Tools = new[] 
                    {                    
                        new ToolAlternatives() 
                        { Tools = new []
                            {                                
                                new Tool(){ UsesToolKeyName = "structure:improvisedGreenhouse", ProductivityFactor = 0.6f, DegradePerSecondOfUse = NoDegrade  },
                                new Tool(){ UsesToolKeyName = "structure:greenhouse", ProductivityFactor = 0.7f, DegradePerSecondOfUse = NoDegrade  }   
                            }                         
                        },
                        harvestGreenhouseTools   // harvesting                    
                    }
                });

                listOfToolProfiles.Add(new ProcessToolSet()
                {
                    KeyName = "toolSetCarbonTailTraps",
                    Tools = new[] {                    
                        new ToolAlternatives() { Tools = new []
                        { 
                            new Tool(){ UsesToolKeyName = "structure:fishTrapCreekSticks", ProductivityFactor = 0.6f, DegradePerSecondOfUse = NoDegrade  }, 
                            new Tool(){ UsesToolKeyName = "structure:fishTrapCreekNet", ProductivityFactor = 0.8f, DegradePerSecondOfUse = NoDegrade  }, 
                            new Tool(){ UsesToolKeyName = "structure:fishTrapShoreBasket", ProductivityFactor = 0.5f, DegradePerSecondOfUse = NoDegrade  },
                            new Tool(){ UsesToolKeyName = "structure:fishTrapShoreHoopNet", ProductivityFactor = 0.6f, DegradePerSecondOfUse = NoDegrade  }                            
                        }                         
                        }
                    }
                });

                listOfToolProfiles.Add(new ProcessToolSet()
                {
                    KeyName = "toolSetStreakFinTraps",
                    Tools = new[] {                    
                        new ToolAlternatives() { Tools = new []
                        { 
                            new Tool(){ UsesToolKeyName = "structure:fishTrapCoast", ProductivityFactor = 0.6f, DegradePerSecondOfUse = NoDegrade  }                                       
                        }                         
                        }
                    }
                });
                #endregion
            

                #region toolSetButcherTurnip
                ProcessToolSet processToolSet = new ProcessToolSet()
                {
                    KeyName = "toolSetButcherTurnip",
                    Tools = new[] {  //mp: why use tags when it evens out and gives every tool the same productivity???  
                        new ToolAlternatives() { Tools = new []
                        {
                            new Tool(){ UsesToolKeyName = "item:turnipCracker", ProductivityFactor = 0.8f, DegradePerSecondOfUse = NoDegrade  },//was MediumDegrade
                            new Tool(){ UsesToolKeyName = "item:metalWorkersToolbox", ProductivityFactor = 0.3f, DegradePerSecondOfUse = NoDegrade  },
                        }},
                        new ToolAlternatives() { Tools = new []
                        {       
                            new Tool(){ UsesToolKeyName = "item:flintKnife", ProductivityFactor = 0.2f, DegradePerSecondOfUse = NoDegrade  }, //ok for cutting meat
                            new Tool(){ UsesToolKeyName = "item:improvisedKnife", ProductivityFactor = 0.3f, DegradePerSecondOfUse = NoDegrade  },//was MediumDegrade
                            new Tool(){ UsesToolKeyName = "item:steelKnife", ProductivityFactor = 0.5f, DegradePerSecondOfUse = NoDegrade  },
                            new Tool(){ UsesToolKeyName = "item:advancedKnife", ProductivityFactor = 0.7f, DegradePerSecondOfUse = NoDegrade  },// knifeblade is a bit too broad to be ideal
                            new Tool(){ UsesToolKeyName = "item:advancedMachete", ProductivityFactor = 0.4f, DegradePerSecondOfUse = NoDegrade  },//was MediumDegrade  
                             new Tool(){ UsesToolKeyName = "item:steelHandAxe", ProductivityFactor = 0.2f, DegradePerSecondOfUse = NoDegrade  },  
                        }
                        }
                    }

                };

                listOfToolProfiles.Add(processToolSet);
                #endregion

                #region toolSetButcherTwinkler
                processToolSet = new ProcessToolSet()
                {
                    KeyName = "toolSetButcherTwinkler",
                    Tools = new[] {            //mp: why use tags when it evens out and gives every tool the same productivity???          

                        new ToolAlternatives() { Tools = new []
                        {
                            new Tool(){ UsesToolTag = "cutThinShell", ProductivityFactor = 1f, DegradePerSecondOfUse = NoDegrade }//was LowDegrade
                        }},
                        new ToolAlternatives() { Tools = new []
                        {     
                            new Tool(){ UsesToolKeyName = "item:flintKnife", ProductivityFactor = 0.2f, DegradePerSecondOfUse = NoDegrade  }, //ok for cutting meat
                            new Tool(){ UsesToolKeyName = "item:improvisedKnife", ProductivityFactor = 0.3f, DegradePerSecondOfUse = NoDegrade  },//was MediumDegrade
                            new Tool(){ UsesToolKeyName = "item:steelKnife", ProductivityFactor = 0.5f, DegradePerSecondOfUse = NoDegrade  },
                            new Tool(){ UsesToolKeyName = "item:advancedKnife", ProductivityFactor = 0.7f, DegradePerSecondOfUse = NoDegrade  },// knifeblade is a bit too broad to be ideal
                            new Tool(){ UsesToolKeyName = "item:advancedMachete", ProductivityFactor = 0.4f, DegradePerSecondOfUse = NoDegrade  },//was MediumDegrade  
                             new Tool(){ UsesToolKeyName = "item:steelHandAxe", ProductivityFactor = 0.2f, DegradePerSecondOfUse = NoDegrade  },  
                        }
                        }
                    }

                };


                listOfToolProfiles.Add(processToolSet);
                #endregion

                #region toolSetPrepareMeal
                processToolSet = new ProcessToolSet()
                {
                    KeyName = "toolSetPrepareMeal",  //make (cut) meals/servings out of a piece of prepared food
                    Tools = new[] {                      

                        new ToolAlternatives() { Tools = new []
                        {
                            new Tool(){ UsesToolTag = "knife", ProductivityFactor = 1f, DegradePerSecondOfUse = NoDegrade }//was LowDegrade
                        }},
                      
                    }

                };


                listOfToolProfiles.Add(processToolSet);
                #endregion

                #region toolSetButcherFlesh
                processToolSet = new ProcessToolSet()
                {
                    KeyName = "toolSetButcherFlesh",  //professional uses 3 tools: Bone saw, boning knife (thin+narrow) and meat cleaver ... = 1f    http://www.motherearthnews.com/diy/butchers-blades-which-to-own-and-how-to-hone.aspx#axzz36nv6qTHm
                    Tools = new[] {                    
                        new ToolAlternatives() { Tools = new []
                        {
                            new Tool(){ UsesToolKeyName = "item:flintKnife", ProductivityFactor = 0.2f, DegradePerSecondOfUse = NoDegrade  }, //ok for cutting meat
                            new Tool(){ UsesToolKeyName = "item:improvisedKnife", ProductivityFactor = 0.3f, DegradePerSecondOfUse = NoDegrade  },//was MediumDegrade
                            new Tool(){ UsesToolKeyName = "item:steelKnife", ProductivityFactor = 0.5f, DegradePerSecondOfUse = NoDegrade  },
                            new Tool(){ UsesToolKeyName = "item:advancedKnife", ProductivityFactor = 0.7f, DegradePerSecondOfUse = NoDegrade  },// knifeblade is a bit too broad to be ideal
                            new Tool(){ UsesToolKeyName = "item:advancedMachete", ProductivityFactor = 0.4f, DegradePerSecondOfUse = NoDegrade  },//was MediumDegrade  
                             new Tool(){ UsesToolKeyName = "item:steelHandAxe", ProductivityFactor = 0.2f, DegradePerSecondOfUse = NoDegrade  },                        
                    //        new Tool(){ UsesToolKeyName = "item:handSaw", ProductivityFactor = 0.2f, DegradePerSecondOfUse = NoDegrade  },//was MediumDegrade
                    //        new Tool(){ UsesToolKeyName = "item:electricalSaw", ProductivityFactor = 0.2f, DegradePerSecondOfUse = NoDegrade  },//was LowDegrade
    



                        }
                        }
                    }

                };



                listOfToolProfiles.Add(processToolSet);
                #endregion

                #region toolSetClimbingRope
                processToolSet = new ProcessToolSet()
                {
                    KeyName = "toolSetClimbingRope",
                    Tools = new[] {                    
                        new ToolAlternatives() { Tools = new []
                        {                                               //full climbing gear 1f
                            new Tool(){ UsesToolKeyName = "item:vine", ProductivityFactor = 0.2f, DegradePerSecondOfUse = NoDegrade  },//was MediumDegrade

                        }
                        }
                    }

                };

          


                listOfToolProfiles.Add(processToolSet);
                #endregion

                #region toolSetStrongBugNet
                processToolSet = new ProcessToolSet()
                {
                    KeyName = "toolSetStrongBugNet",
                    Tools = new[] {                    
                        new ToolAlternatives() { Tools = new []
                        {
                        
                                                                                //since it's improvised it's below 1f but close to the best kind of hand tool for this.
                             new Tool(){ UsesToolKeyName = "item:strongBugNet", ProductivityFactor = 0.8f, DegradePerSecondOfUse = NoDegrade  },//was LowDegrade
                             new Tool(){ UsesToolKeyName = "item:shadeleafResin", ProductivityFactor = 0.3f, DegradePerSecondOfUse = NoDegrade  }// they use the sticky resin on a branch to catch them..
                        }
                        }
                    }

                };

                listOfToolProfiles.Add(processToolSet);
                #endregion


                /*
                                processToolSet = new ProcessToolSet()
                                {
                                    KeyName = "fishingHook",
                                    Tools = new[] {                    
                                        new ToolAlternatives() { Tools = new []
                                        {
                                            new Tool(){ UsesToolKeyName = "item:thornHooks", ProductivityFactor = 0.5f, DegradePerSecondOfUse = NoDegrade  },
                                            new Tool(){ UsesToolKeyName = "item:improvisedMetalHooks", ProductivityFactor = 0.5f, DegradePerSecondOfUse = NoDegrade  },
                                            new Tool(){ UsesToolKeyName = "item:woodenHooks", ProductivityFactor = 0.4f, DegradePerSecondOfUse = NoDegrade  },
 
                                        }
                                        }
                                    }
                                };
                */


                #region toolSetCatchStinkpup
                processToolSet = new ProcessToolSet()
                {
                    KeyName = "toolSetCatchStinkpup",  //needs one of each 3
                    Tools = new[] {                      

                        new ToolAlternatives() { Tools = new []
                        {
                            new Tool(){ UsesToolTag = "fishingHook", ProductivityFactor = 0.7f, DegradePerSecondOfUse = NoDegrade }//was LowDegrade
                        }},
                        new ToolAlternatives() { Tools = new []
                        {      
                                             
                             new Tool(){ UsesToolKeyName = "item:neonHornetsLive", ProductivityFactor = 1f, DegradePerSecondOfUse = NoDegrade  },
                        
                        }},
                        new ToolAlternatives() { Tools = new []
                        {   
                                             
                             new Tool(){ UsesToolKeyName = "item:advancedString", ProductivityFactor = 0.6f, DegradePerSecondOfUse = NoDegrade  }, //was HighDegrade
                            new Tool(){ UsesToolKeyName = "item:superconductingWire", ProductivityFactor = 0.4f, DegradePerSecondOfUse = NoDegrade  },
                            new Tool(){ UsesToolKeyName = "item:rawhideString", ProductivityFactor = 0.2f, DegradePerSecondOfUse = NoDegrade  },//cannot use rawhide string in water, but this is for "fishing" on dry land 
                            new Tool(){ UsesToolKeyName = "item:cottonString", ProductivityFactor = 0.4f, DegradePerSecondOfUse = NoDegrade  }                        
                        }}
                    }

                };

                listOfToolProfiles.Add(processToolSet);
                #endregion

                #region toolSetFishMinnows
                processToolSet = new ProcessToolSet()
                {
                    KeyName = "toolSetFishMinnows",
                    Tools = new[] {                    
                        new ToolAlternatives() { Tools = new []
                        {
                        
                                                                                   //real fishing net 1f
                             new Tool(){ UsesToolKeyName = "item:strongBugNet", ProductivityFactor = 0.6f, DegradePerSecondOfUse = NoDegrade  },
                             new Tool(){ UsesToolKeyName = "item:ursinixVenomGland", ProductivityFactor = 0.8f, DegradePerSecondOfUse = NoDegrade  }, //poisons the water
                        }
                        }
                    }

                };
               

                listOfToolProfiles.Add(processToolSet);
                #endregion

                #region toolSetFishAlabasterRay
                processToolSet = new ProcessToolSet()
                {
                    KeyName = "toolSetFishAlabasterRay",  //gig/trident: any long pole which has been tipped with a multi-pronged spear:  http://en.wikipedia.org/wiki/Gigging#Flounder_gigging
                                                 // spear gun: http://en.wikipedia.org/wiki/Spear_gun
                                                  // spear fishing without diving: http://en.wikipedia.org/wiki/Spearfishing
                    Tools = new[] {                    
                        new ToolAlternatives() { Tools = new []
                        {                        
                         
                                                                     //spear gun ProductivityFactor = 1f,  gig/trident: 0.6f 
                             new Tool(){ UsesToolKeyName = "item:improvisedBasicSpear", ProductivityFactor = 0.2f, DegradePerSecondOfUse = NoDegrade  },
                            new Tool(){ UsesToolKeyName = "item:improvisedFlintSpear", ProductivityFactor = 0.35f, DegradePerSecondOfUse = NoDegrade  },
                             new Tool(){ UsesToolKeyName = "item:improvisedGoodSpear", ProductivityFactor = 0.5f, DegradePerSecondOfUse = NoDegrade  },
                             new Tool(){ UsesToolKeyName = "item:ironSpear", ProductivityFactor = 0.7f, DegradePerSecondOfUse = NoDegrade  },

                        }
                        }
                    }

                };


                listOfToolProfiles.Add(processToolSet);
                #endregion

                #region toolSetFishStreakFin
                processToolSet = new ProcessToolSet()
                {
                    KeyName = "toolSetFishStreakFin", // a good fishing rod with correct hooks and optimum artificial bait: 1f
                    Tools = new[] {                      

                        new ToolAlternatives() { Tools = new []
                        {
                            new Tool(){ UsesToolTag = "fishingHook", ProductivityFactor = 0.7f, DegradePerSecondOfUse = NoDegrade }//was LowDegrade
                        }},
                        new ToolAlternatives() { Tools = new []
                        {                                       
                             new Tool(){ UsesToolKeyName = "item:pigFliesLive", ProductivityFactor = 0.8f, DegradePerSecondOfUse = NoDegrade  },
                             new Tool(){ UsesToolKeyName = "item:pigFliesDead", ProductivityFactor = 0.5f, DegradePerSecondOfUse = NoDegrade  },
                            
                        }},
                        new ToolAlternatives() { Tools = new []
                        {                                 
                            new Tool(){ UsesToolKeyName = "item:advancedString", ProductivityFactor = 0.65f, DegradePerSecondOfUse = NoDegrade  }, //was HighDegrade
                            new Tool(){ UsesToolKeyName = "item:superconductingWire", ProductivityFactor = 0.5f, DegradePerSecondOfUse = NoDegrade  },
                            new Tool(){ UsesToolKeyName = "item:cottonString", ProductivityFactor = 0.5f, DegradePerSecondOfUse = NoDegrade  } 
                         //cannot use rawhide string in water
                        }}
                    }

                };
                listOfToolProfiles.Add(processToolSet);
                #endregion

                #region toolSetFishCarbonTail
                processToolSet = new ProcessToolSet()
                {
                    KeyName = "toolSetFishCarbonTail", // a good fishing rod with correct hooks and optimum artificial bait: 1f
                    Tools = new[] {                      

                        new ToolAlternatives() { Tools = new []
                        {
                            new Tool(){ UsesToolTag = "fishingHook", ProductivityFactor = 0.7f, DegradePerSecondOfUse = NoDegrade }//was LowDegrade
                        }},
                        new ToolAlternatives() { Tools = new []
                        {      
                                             
                             new Tool(){ UsesToolKeyName = "item:pigFliesLive", ProductivityFactor = 0.4f, DegradePerSecondOfUse = NoDegrade  },
                             new Tool(){ UsesToolKeyName = "item:pigFliesDead", ProductivityFactor = 0.2f, DegradePerSecondOfUse = NoDegrade  },
                           
                              new Tool(){ UsesToolKeyName = "item:neonHornetsLive", ProductivityFactor = 0.8f, DegradePerSecondOfUse = NoDegrade  },
                             new Tool(){ UsesToolKeyName = "item:neonHornetsDead", ProductivityFactor = 0.4f, DegradePerSecondOfUse = NoDegrade  },
                        }},
                        new ToolAlternatives() { Tools = new []
                        {   
                                             
                            new Tool(){ UsesToolKeyName = "item:advancedString", ProductivityFactor = 0.65f, DegradePerSecondOfUse = NoDegrade  }, //was HighDegrade
                            new Tool(){ UsesToolKeyName = "item:superconductingWire", ProductivityFactor = 0.5f, DegradePerSecondOfUse = NoDegrade  },
                            new Tool(){ UsesToolKeyName = "item:cottonString", ProductivityFactor = 0.5f, DegradePerSecondOfUse = NoDegrade  } 
                         //cannot use rawhide string in water
                        }}
                    }

                };


                listOfToolProfiles.Add(processToolSet);
                #endregion

                #region toolSetSharpenBlade
                processToolSet = new ProcessToolSet()
                {

                    KeyName = "toolSetSharpenBlade",  // http://en.wikipedia.org/wiki/Grind  http://en.wikipedia.org/wiki/Sharpening  http://en.wikipedia.org/wiki/Sharpening_stone http://en.wikipedia.org/wiki/Knife_sharpening
                                            //with a rock: http://www.ehow.com/how_4550406_sharpen-knife-rock.html
                    Tools = new[] {                    
                        new ToolAlternatives() { Tools = new []
                        {
                            new Tool(){ UsesToolKeyName = "item:smoothSandstone", ProductivityFactor = 0.15f, DegradePerSecondOfUse = NoDegrade  },//was MediumDegrade
                       
                     //        new Tool(){ UsesToolKeyName = "item:powerGrinder", ProductivityFactor = 1f, DegradePerSecondOfUse = NoDegrade  },//was LowDegrade
                             new Tool(){ UsesToolKeyName = "item:file", ProductivityFactor = 0.4f, DegradePerSecondOfUse = NoDegrade  },//was LowDegrade
                                new Tool(){ UsesToolKeyName = "item:blacksmithsToolbox", ProductivityFactor = 0.5f, DegradePerSecondOfUse = NoDegrade}, //includes a file
                                new Tool(){ UsesToolKeyName = "item:metalWorkersToolbox", ProductivityFactor = 0.6f, DegradePerSecondOfUse = NoDegrade}, //includes a file
                           //  new Tool(){ Tag = "sharpenMetal", ProductivityFactor = 1f, DegradePerSecondOfUse = MediumDegrade}
                        }
                         
                        }
                    }

                };

                listOfToolProfiles.Add(processToolSet);
                #endregion

                #region toolSetSharpenBladeAndtoolSetAttachWoodAndMetal
                processToolSet = new ProcessToolSet()
                {

                    KeyName = "toolSetSharpenBladeAndtoolSetAttachWoodAndMetal", // made from toolSetSharpenBlade and toolSetAttachWoodAndMetal
                    Tools = new[] {                    
                        new ToolAlternatives() { Tools = new []
                        {
                            new Tool(){ UsesToolKeyName = "item:smoothSandstone", ProductivityFactor = 0.15f, DegradePerSecondOfUse = NoDegrade  },//was MediumDegrade
                       
                     //        new Tool(){ UsesToolKeyName = "item:powerGrinder", ProductivityFactor = 1f, DegradePerSecondOfUse = NoDegrade  },//was LowDegrade
                             new Tool(){ UsesToolKeyName = "item:file", ProductivityFactor = 0.4f, DegradePerSecondOfUse = NoDegrade  },//was LowDegrade
                             new Tool(){ UsesToolKeyName = "item:blacksmithsToolbox", ProductivityFactor = 0.5f, DegradePerSecondOfUse = NoDegrade}, //includes a file
                             new Tool(){ UsesToolKeyName = "item:metalWorkersToolbox", ProductivityFactor = 0.6f, DegradePerSecondOfUse = NoDegrade}, //includes a file
                           //  new Tool(){ Tag = "sharpenMetal", ProductivityFactor = 1f, DegradePerSecondOfUse = MediumDegrade}
                        }
                         
                        },
                        new ToolAlternatives() { Tools = new []
                        {
                            new Tool(){ UsesToolTag = "improvisedGlue", ProductivityFactor = 0.1f, DegradePerSecondOfUse = NoDegrade  },     
                            new Tool(){ UsesToolKeyName = "item:advancedString", ProductivityFactor = 0.3f, DegradePerSecondOfUse = NoDegrade  },//was MediumDegrade
                             new Tool(){ UsesToolKeyName = "item:exaGlue", ProductivityFactor = 1f, DegradePerSecondOfUse = NoDegrade  },//was LowDegrade
                            new Tool(){ UsesToolKeyName = "item:metalWire", ProductivityFactor = 0.15f, DegradePerSecondOfUse = NoDegrade  },
                            new Tool(){ UsesToolKeyName = "item:cottonString", ProductivityFactor = 0.15f, DegradePerSecondOfUse = NoDegrade  },
                            new Tool(){ UsesToolKeyName = "item:rawhideString", ProductivityFactor = 0.1f, DegradePerSecondOfUse = NoDegrade  },
                        }                         
                        }
                    }

                };

                listOfToolProfiles.Add(processToolSet);
                #endregion


                /*  
                processToolSet = new ProcessToolSet()
                {
                    KeyName = "farmingTool",
                    Tools = new[] {                    
                        new ToolAlternatives() { Tools = new []
                            {
                            new Tool(){ UsesToolKeyName = "item:improvisedFarmingTool", ProductivityFactor = 0.7f, DegradePerSecondOfUse = NoDegrade  }
                            }
                        }
                    }
                };
                listOfToolProfiles.Add(processToolSet);
            */


                #region toolSetMold
                processToolSet = new ProcessToolSet()
                {

                    KeyName = "toolSetMold",
                    Tools = new[] 
                    {
                        new ToolAlternatives() 
                        { 
                            Tools = new []
                            {//productivity could be improved by having a larger multi-mold...or something
                                new Tool(){ UsesToolKeyName = "item:brickMold", ProductivityFactor = 0.6f, DegradePerSecondOfUse = NoDegrade  },
                            }
                         
                        }
                    }

                };
                listOfToolProfiles.Add(processToolSet);
                #endregion

                #region toolSetDryingPeat
                processToolSet = new ProcessToolSet()
                {

                    KeyName = "toolSetDryingPeat",
                    Tools = new[] 
                    {
                        new ToolAlternatives() 
                        { 
                            Tools = new []
                            {
                                new Tool(){ UsesToolKeyName = "structure:peatStack", ProductivityFactor = 0.6f, DegradePerSecondOfUse = NoDegrade  },
                            }
                         
                        }
                    }

                };
                listOfToolProfiles.Add(processToolSet);
                #endregion

                #region toolSetDryingFirewood
                processToolSet = new ProcessToolSet()
                {

                    KeyName = "toolSetDryingFirewood",
                    Tools = new[] 
                    {
                        new ToolAlternatives() 
                        { 
                            Tools = new []
                            {
                                new Tool(){ UsesToolKeyName = "structure:firewoodStack", ProductivityFactor = 0.6f, DegradePerSecondOfUse = NoDegrade  },
                            }
                         
                        }
                    }

                };
                listOfToolProfiles.Add(processToolSet);
                #endregion

                #region toolSetKilnBigAndSmall
                processToolSet = new ProcessToolSet()
                {

                    KeyName = "toolSetKilnBigAndSmall", //for small pottery which can fit in both a small and large kiln
                    Tools = new[] 
                    {
                        new ToolAlternatives() 
                        { 
                            Tools = new []
                            {
                                new Tool(){ UsesToolKeyName = "structure:kiln", ProductivityFactor = 0.7f, DegradePerSecondOfUse = NoDegrade  }, //warning. tool productivity has a huge effect on fuel consumption. if more fuel is needed than what one agent can carry, you will get a behavior bug (sep 2015)
                                new Tool(){ UsesToolKeyName = "structure:kilnImprovisedSmall", ProductivityFactor = 0.45f, DegradePerSecondOfUse = NoDegrade  },
                            }
                         
                        }
                    }

                };
                listOfToolProfiles.Add(processToolSet);
                #endregion

                #region toolSetKilnBig
                processToolSet = new ProcessToolSet()
                {

                    KeyName = "toolSetKilnBig", //for stuff which is too big to go in a small oven-kiln.
                    Tools = new[] 
                    {
                        new ToolAlternatives() 
                        { 
                            Tools = new []
                            {
                                new Tool(){ UsesToolKeyName = "structure:kiln", ProductivityFactor = 0.6f, DegradePerSecondOfUse = NoDegrade  }, //warning. tool productivity has a huge effect on fuel consumption. if more fuel is needed than what one agent can carry, you will get a behavior bug (sep 2015)
                            }
                         
                        }
                    }

                };
                listOfToolProfiles.Add(processToolSet);
                #endregion

                #region toolSetOven
                processToolSet = new ProcessToolSet()
                {

                    KeyName = "toolSetOven", //you cannot  "bake" stuff with a campfire 
                    Tools = new[] 
                    {
                        new ToolAlternatives() 
                        { 
                            Tools = new []
                            {
                                new Tool(){ UsesToolKeyName = "structure:kilnImprovisedSmall", ProductivityFactor = 0.6f, DegradePerSecondOfUse = NoDegrade  }, //warning. tool productivity has a huge effect on fuel consumption. if more fuel is needed than what one agent can carry, you will get a behavior bug (sep 2015)
                           //     new Tool(){ UsesToolKeyName = "structure:campfire", ProductivityFactor = 0.1f, DegradePerSecondOfUse = NoDegrade  }, // not campfire for this.
                                new Tool(){ UsesToolKeyName = "structure:fieldKitchen", ProductivityFactor = 0.7f, DegradePerSecondOfUse = NoDegrade  } ,
                                new Tool(){ UsesToolKeyName = "item:simpleStoveUpgrade", ProductivityFactor = 0.7f, DegradePerSecondOfUse = NoDegrade  } ,
                     //           new Tool(){ UsesToolKeyName = "structure:improvisedKitchen", ProductivityFactor = 0.4f, DegradePerSecondOfUse = NoDegrade  }, //mp not sure about this. but I figure there should be some reward for having the kitchen? No, because kitchen is already being used a lot for fermenting.
                    //        new Tool(){ UsesToolKeyName = "structure:mudBrickKitchen", ProductivityFactor = 0.4f, DegradePerSecondOfUse = NoDegrade  },
                            }
                         
                        }
                    }

                };
                listOfToolProfiles.Add(processToolSet);
                #endregion

                #region toolSetCordage

                processToolSet = new ProcessToolSet()
                {

                    KeyName = "toolSetCordage",   //MP: hack until we get multiple options for material input to processes. High degrade because I want materials to be used up. this is any kind of string used for building shelter (so we don't need even more versions of each shelter)
                    Tools = new[] {                   //We need better feedback for when a tool breaks and until then we set the degrade factors low to prevent interruption of construction work . note, string is future tech and doesnt ever really break
                        new ToolAlternatives() { Tools = new []
                            {
                                new Tool(){ UsesToolKeyName = "item:advancedString", ProductivityFactor = 0.55f, DegradePerSecondOfUse = NoDegrade  },                              //was 1f
                                new Tool(){ UsesToolKeyName = "item:superconductingWire", ProductivityFactor = 0.4f, DegradePerSecondOfUse = NoDegrade  },                     //was 0.4f
                                new Tool(){ UsesToolKeyName = "item:metalWire", ProductivityFactor = 0.5f, DegradePerSecondOfUse = NoDegrade  },                            //was 0.4f
                                new Tool(){ UsesToolKeyName = "item:vine", ProductivityFactor = 0.45f, DegradePerSecondOfUse = NoDegrade  },                                  //was 0.2f
                                new Tool(){ UsesToolKeyName = "item:rawhideString", ProductivityFactor = 0.4f, DegradePerSecondOfUse = NoDegrade  } ,                       //was 0.15f
                                new Tool(){ UsesToolKeyName = "item:cottonString", ProductivityFactor = 0.45f, DegradePerSecondOfUse = NoDegrade  },
  
              

                            }
                         
                        }
                    }

                };
                listOfToolProfiles.Add(processToolSet);
                #endregion

                #region toolSetLightString

                processToolSet = new ProcessToolSet()
                {

                    KeyName = "toolSetLightString",   
                    Tools = new[] {                    
                        new ToolAlternatives() { Tools = new []
                        {
                            new Tool(){ UsesToolKeyName = "item:advancedString", ProductivityFactor = 0.55f, DegradePerSecondOfUse = NoDegrade  },              //was 1f
                            new Tool(){ UsesToolKeyName = "item:superconductingWire", ProductivityFactor = 0.5f, DegradePerSecondOfUse = NoDegrade  }, //was 0.5f
                            new Tool(){ UsesToolKeyName = "item:metalWire", ProductivityFactor = 0.5f, DegradePerSecondOfUse = NoDegrade  },        //was 0.5f
                            new Tool(){ UsesToolKeyName = "item:rawhideString", ProductivityFactor = 0.4f, DegradePerSecondOfUse = NoDegrade  },     //was 0.2f
                            new Tool(){ UsesToolKeyName = "item:cottonString", ProductivityFactor = 0.5f, DegradePerSecondOfUse = NoDegrade  },
                          
                        }
                         
                        }
                    }

                };
                listOfToolProfiles.Add(processToolSet);
                #endregion

                #region toolSetChopWeakWood




                processToolSet = new ProcessToolSet()
                {
                    KeyName = "toolSetChopWeakWood", //  cutting soft, pliable, bending wood etc.  (Note: this is for cutting single items. slashing jungle brush, cutting hedge is below, under "toolSetHarvestSmallBranches")
                    Tools = new[] {
                        new ToolAlternatives() {Tools = new []
                        {                                       //    "bypass lopper" (=havesaks) 1f
                                                                // 
                            new Tool(){ UsesToolKeyName = "item:flintKnife", ProductivityFactor = 0.08f, DegradePerSecondOfUse = NoDegrade  }, //
                            new Tool(){ UsesToolKeyName = "item:improvisedKnife", ProductivityFactor = 0.15f, DegradePerSecondOfUse = NoDegrade  },//was MediumDegrade
                            new Tool(){ UsesToolKeyName = "item:steelKnife", ProductivityFactor = 0.2f, DegradePerSecondOfUse = NoDegrade  },
                            new Tool(){ UsesToolKeyName = "item:advancedKnife", ProductivityFactor = 0.3f, DegradePerSecondOfUse = NoDegrade  },//was MediumDegrade

                            new Tool(){ UsesToolKeyName = "item:steelHandAxe", ProductivityFactor = 0.35f, DegradePerSecondOfUse = NoDegrade  }, 
                            new Tool(){ UsesToolKeyName = "item:improvisedHandAxe", ProductivityFactor = 0.25f, DegradePerSecondOfUse = NoDegrade  },

                            new Tool(){ UsesToolKeyName = "item:advancedMachete", ProductivityFactor = 0.6f, DegradePerSecondOfUse = NoDegrade  },
                            new Tool(){ UsesToolKeyName = "item:steelMachete", ProductivityFactor = 0.45f, DegradePerSecondOfUse = NoDegrade  },  
                        }

                        }
                    }
                };

                listOfToolProfiles.Add(processToolSet);
                #endregion

                #region toolSetChopToughWood
                processToolSet = new ProcessToolSet()
                {
                    KeyName = "toolSetChopToughWood", //chopping hard wood
                    Tools = new[] {                    
                        new ToolAlternatives() { Tools = new []
                        {                                                              //
                            new Tool(){ UsesToolKeyName = "item:improvisedKnife", ProductivityFactor = 0.06f, DegradePerSecondOfUse = NoDegrade  },
                            new Tool(){ UsesToolKeyName = "item:steelKnife", ProductivityFactor = 0.1f, DegradePerSecondOfUse = NoDegrade  },
                            new Tool(){ UsesToolKeyName = "item:advancedKnife", ProductivityFactor = 0.2f, DegradePerSecondOfUse = NoDegrade  },//was MediumDegrade

                            new Tool(){ UsesToolKeyName = "item:advancedMachete", ProductivityFactor = 0.3f, DegradePerSecondOfUse = NoDegrade  },//was MediumDegrade 
                            new Tool(){ UsesToolKeyName = "item:steelMachete", ProductivityFactor = 0.15f, DegradePerSecondOfUse = NoDegrade  },                         

                            new Tool(){ UsesToolKeyName = "item:steelHandAxe", ProductivityFactor = 0.7f, DegradePerSecondOfUse = NoDegrade  },
                            new Tool(){ UsesToolKeyName = "item:improvisedHandAxe", ProductivityFactor = 0.5f, DegradePerSecondOfUse = NoDegrade  },

                //            new Tool(){ UsesToolKeyName = "item:chainsaw", ProductivityFactor = 1f, DegradePerSecondOfUse = NoDegrade  },//BENCHMARK
               //             new Tool(){ UsesToolKeyName = "item:electricalSaw", ProductivityFactor = 1f, DegradePerSecondOfUse = NoDegrade  },//BENCHMARK

               //             new Tool(){ UsesToolKeyName = "item:handSaw", ProductivityFactor = 0.4f, DegradePerSecondOfUse = NoDegrade  },//was MediumDegrade

                        }
                         
                        }
                    }

                };

                listOfToolProfiles.Add(processToolSet);
                #endregion

                #region toolSetHarvestSmallBranches

                processToolSet = new ProcessToolSet()
                {
                    KeyName = "toolSetHarvestSmallBranches", //slashing jungle brush, cutting hedge.suited for a machete... for cutting away soft/thin wood and lots of brush etc, meanwhile, an axe is for chopping single pieces of hardwood (not this)
                                                     //mp we should make a toolset for harvesting crops from a field...
                    Tools = new[] {                    
                        new ToolAlternatives() { Tools = new []
                        {                                           //BENCHMARK: hedge cutter power tool 1f
                            new Tool(){ UsesToolKeyName = "item:advancedMachete", ProductivityFactor = 0.5f, DegradePerSecondOfUse = NoDegrade  },//was LowDegrade 
                            new Tool(){ UsesToolKeyName = "item:steelMachete", ProductivityFactor = 0.3f, DegradePerSecondOfUse = NoDegrade  }, 

                                                                                       //   "item:flintKnife" // mp a flint knife cannot take much abuse...best for small, single objects that can be whittled on.
                             new Tool(){ UsesToolKeyName = "item:improvisedKnife", ProductivityFactor = 0.05f, DegradePerSecondOfUse = NoDegrade  },
                            new Tool(){ UsesToolKeyName = "item:steelKnife", ProductivityFactor = 0.10f, DegradePerSecondOfUse = NoDegrade  },
                            new Tool(){ UsesToolKeyName = "item:advancedKnife", ProductivityFactor = 0.15f, DegradePerSecondOfUse = NoDegrade  },//was MediumDegrade

                            new Tool(){ UsesToolKeyName = "item:steelHandAxe", ProductivityFactor = 0.1f, DegradePerSecondOfUse = NoDegrade  },
                            new Tool(){ UsesToolKeyName = "item:improvisedHandAxe", ProductivityFactor = 0.08f, DegradePerSecondOfUse = NoDegrade  },

                 //           new Tool(){ UsesToolKeyName = "item:handSaw", ProductivityFactor = 0.1f, DegradePerSecondOfUse = NoDegrade  },//was MediumDegrade
                  //           new Tool(){ UsesToolKeyName = "item:chainsaw", ProductivityFactor = 0.6f, DegradePerSecondOfUse = NoDegrade  },//was LowDegrade

                        }
                         
                        }
                    }

                };

                listOfToolProfiles.Add(processToolSet);
                #endregion

                #region toolSetHarvestFieldCrops                       

                listOfToolProfiles.Add(new ProcessToolSet()
                {
                    KeyName = "toolSetHarvestFieldCrops", //for harvesting crops from a field...
                     
                    Tools = new ToolAlternatives[]
                    {
                        harvestFieldCropsTools
                    }
                });

                #endregion

                #region toolSetShapenSmallWoodImprovisedWithFire


                processToolSet = new ProcessToolSet()
                {
                    KeyName = "toolSetShapenSmallWoodImprovisedWithFire",
                    Tools = new[] {                    
                        new ToolAlternatives() { Tools = new []
                        {
                            new Tool(){ UsesToolKeyName = "structure:campfire", ProductivityFactor = 0.35f, DegradePerSecondOfUse = NoDegrade  }, //
                            new Tool(){ UsesToolKeyName = "structure:fieldKitchen", ProductivityFactor = 0.3f, DegradePerSecondOfUse = NoDegrade  } ,
                            new Tool(){ UsesToolKeyName = "structure:improvisedKitchen", ProductivityFactor = 0.35f, DegradePerSecondOfUse = NoDegrade  },
                            new Tool(){ UsesToolKeyName = "structure:mudBrickKitchen", ProductivityFactor = 0.35f, DegradePerSecondOfUse = NoDegrade  },
                            new Tool(){ UsesToolKeyName = "structure:improvisedWorkbench", ProductivityFactor = 0.7f, DegradePerSecondOfUse = NoDegrade  }, //has no fire, but it has a place for holding and planing objects such as arrow shafts
                            new Tool(){ UsesToolKeyName = "structure:mudBrickWorkbench", ProductivityFactor = 0.7f, DegradePerSecondOfUse = NoDegrade  }, //has no fire, but it has a place for holding and planing objects such as arrow shafts
                            new Tool(){ UsesToolKeyName = "item:carpenterWorkshopUpgrade", ProductivityFactor = 0.85f, DegradePerSecondOfUse = NoDegrade  },//because stationary item. //has no fire, but it has a place for holding and planing objects such as arrow shafts
                           
                        }           //fire AND knife needed
                        },
                          new ToolAlternatives() { Tools = new []
                        {
                            new Tool(){ UsesToolKeyName = "item:carpentersToolbox", ProductivityFactor = 0.7f, DegradePerSecondOfUse = NoDegrade  },
                            new Tool(){ UsesToolKeyName = "item:flintKnife", ProductivityFactor = 0.05f, DegradePerSecondOfUse = NoDegrade  },
                            new Tool(){ UsesToolKeyName = "item:improvisedKnife", ProductivityFactor = 0.3f, DegradePerSecondOfUse = NoDegrade  },//was MediumDegrade
                            new Tool(){ UsesToolKeyName = "item:steelKnife", ProductivityFactor = 0.45f, DegradePerSecondOfUse = NoDegrade  },
                            new Tool(){ UsesToolKeyName = "item:advancedKnife", ProductivityFactor = 0.6f, DegradePerSecondOfUse = NoDegrade  },//was MediumDegrade

                            new Tool(){ UsesToolKeyName = "item:advancedMachete", ProductivityFactor = 0.2f, DegradePerSecondOfUse = NoDegrade  },//was MediumDegrade  
                            new Tool(){ UsesToolKeyName = "item:steelMachete", ProductivityFactor = 0.15f, DegradePerSecondOfUse = NoDegrade  }, 

                            new Tool(){ UsesToolKeyName = "item:steelHandAxe", ProductivityFactor = 0.08f, DegradePerSecondOfUse = NoDegrade  },
                            new Tool(){ UsesToolKeyName = "item:improvisedHandAxe", ProductivityFactor = 0.05f, DegradePerSecondOfUse = NoDegrade  },
                          
                        }
                         
                        }
                    
                    }
                };
                listOfToolProfiles.Add(processToolSet);
                #endregion

                #region toolSetMakeSpoakShingles
                processToolSet = new ProcessToolSet()
                {
                    KeyName = "toolSetMakeSpoakShingles",
                    Tools = new[] {                    
                        new ToolAlternatives() { Tools = new []
                        {
             //             
                             new Tool(){ UsesToolKeyName = "item:carpenterWorkshopUpgrade", ProductivityFactor = 0.8f, DegradePerSecondOfUse = NoDegrade  }, //because stationary item.
                            new Tool(){ UsesToolKeyName = "structure:mudBrickWorkbench", ProductivityFactor = 0.5f, DegradePerSecondOfUse = NoDegrade  },
                            new Tool(){ UsesToolKeyName = "structure:improvisedWorkbench", ProductivityFactor = 0.5f, DegradePerSecondOfUse = NoDegrade  }, //

                           
                        }           
                        },
                          new ToolAlternatives() { Tools = new []
                        {
                      //      new Tool(){ UsesToolKeyName = "item:marshcotSap", ProductivityFactor = 0.8f, DegradePerSecondOfUse = NoDegrade  }, no longer used for this.
                            new Tool(){ UsesToolKeyName = "item:shadeleafResin", ProductivityFactor = 0.8f, DegradePerSecondOfUse = NoDegrade  },//
                          
                        }
                         
                        }
                    
                    }
                };
                listOfToolProfiles.Add(processToolSet);

                #endregion

                #region toolSetShapenSmallWood

                processToolSet = new ProcessToolSet()
                {
                    KeyName = "toolSetShapenSmallWood",
                    Tools = new[] {                    
                        new ToolAlternatives() { Tools = new []
                        {    
                            new Tool(){ UsesToolKeyName = "item:carpentersToolbox", ProductivityFactor = 0.65f, DegradePerSecondOfUse = NoDegrade  },
                            new Tool(){ UsesToolKeyName = "item:flintKnife", ProductivityFactor = 0.05f, DegradePerSecondOfUse = NoDegrade  }, //mp flint is best for soft materials, and this category could hold harder wood as well..
                            new Tool(){ UsesToolKeyName = "item:improvisedKnife", ProductivityFactor = 0.3f, DegradePerSecondOfUse = NoDegrade  },//was MediumDegrade
                            new Tool(){ UsesToolKeyName = "item:steelKnife", ProductivityFactor = 0.35f, DegradePerSecondOfUse = NoDegrade  },
                            new Tool(){ UsesToolKeyName = "item:advancedKnife", ProductivityFactor = 0.5f, DegradePerSecondOfUse = NoDegrade  },//was MediumDegrade

                            new Tool(){ UsesToolKeyName = "item:advancedMachete", ProductivityFactor = 0.3f, DegradePerSecondOfUse = NoDegrade  },//was MediumDegrade 
                            new Tool(){ UsesToolKeyName = "item:steelMachete", ProductivityFactor = 0.20f, DegradePerSecondOfUse = NoDegrade  },

                            new Tool(){ UsesToolKeyName = "item:steelHandAxe", ProductivityFactor = 0.18f, DegradePerSecondOfUse = NoDegrade  },    
                            new Tool(){ UsesToolKeyName = "item:improvisedHandAxe", ProductivityFactor = 0.15f, DegradePerSecondOfUse = NoDegrade  },   
                 

                        }
                         
                        }
                    }

                };

                listOfToolProfiles.Add(processToolSet);
                #endregion

                #region toolSetCombineLightImprovisedObjects

                processToolSet = new ProcessToolSet()
                {
                    KeyName = "toolSetCombineLightImprovisedObjects",
                    Tools = new[] {                    
                        new ToolAlternatives() { Tools = new []
                        {
                            new Tool(){ UsesToolTag = "improvisedGlue", ProductivityFactor = 0.3f, DegradePerSecondOfUse = NoDegrade  },        //was 0.2f
                            new Tool(){ UsesToolKeyName = "item:advancedString", ProductivityFactor = 0.5f, DegradePerSecondOfUse = NoDegrade  },       //was 0.5f
                             new Tool(){ UsesToolKeyName = "item:exaGlue", ProductivityFactor = 0.9f, DegradePerSecondOfUse = NoDegrade  },       //was 1f
                             new Tool(){ UsesToolKeyName = "item:superconductingWire", ProductivityFactor = 0.3f, DegradePerSecondOfUse = NoDegrade  },//was 0.3f
                            new Tool(){ UsesToolKeyName = "item:metalWire", ProductivityFactor = 0.4f, DegradePerSecondOfUse = NoDegrade  },    //was 0.4f
                            new Tool(){ UsesToolKeyName = "item:rawhideString", ProductivityFactor = 0.2f, DegradePerSecondOfUse = NoDegrade  },//was 0.05f
                            new Tool(){ UsesToolKeyName = "item:cottonString", ProductivityFactor = 0.4f, DegradePerSecondOfUse = NoDegrade  },
                        }
                         
                        }
                    }

                };
                listOfToolProfiles.Add(processToolSet);
                #endregion

                #region toolSetAttachLightObjectsAndShapeSmallWood

                // NA: copy pasted from toolSetCombineLightImprovisedObjects and toolSetShapenSmallWood
                processToolSet = new ProcessToolSet()
                {
                    KeyName = "toolSetAttachLightObjectsAndShapeSmallWood",
                    Tools = new[] {                    
                        new ToolAlternatives() { Tools = new [] 
                        {
                            new Tool(){ UsesToolTag = "improvisedGlue", ProductivityFactor = 0.3f, DegradePerSecondOfUse = NoDegrade  },        //was 0.2f
                            new Tool(){ UsesToolKeyName = "item:advancedString", ProductivityFactor = 0.5f, DegradePerSecondOfUse = NoDegrade  },       //was 0.5f
                             new Tool(){ UsesToolKeyName = "item:exaGlue", ProductivityFactor = 0.9f, DegradePerSecondOfUse = NoDegrade  },       //was 1f
                             new Tool(){ UsesToolKeyName = "item:superconductingWire", ProductivityFactor = 0.3f, DegradePerSecondOfUse = NoDegrade  },//was 0.3f
                            new Tool(){ UsesToolKeyName = "item:metalWire", ProductivityFactor = 0.4f, DegradePerSecondOfUse = NoDegrade  },    //was 0.4f
                            new Tool(){ UsesToolKeyName = "item:rawhideString", ProductivityFactor = 0.2f, DegradePerSecondOfUse = NoDegrade  },//was 0.05f
                            new Tool(){ UsesToolKeyName = "item:cottonString", ProductivityFactor = 0.4f, DegradePerSecondOfUse = NoDegrade  },
                        }
                                    // glue and knife needed
                        },
                        new ToolAlternatives() { Tools = new []
                        {    
                            new Tool(){ UsesToolKeyName = "item:carpentersToolbox", ProductivityFactor = 0.65f, DegradePerSecondOfUse = NoDegrade  },
                            new Tool(){ UsesToolKeyName = "item:flintKnife", ProductivityFactor = 0.05f, DegradePerSecondOfUse = NoDegrade  }, //mp flint is best for soft materials, and this category could hold harder wood as well..
                            new Tool(){ UsesToolKeyName = "item:improvisedKnife", ProductivityFactor = 0.3f, DegradePerSecondOfUse = NoDegrade  },//was MediumDegrade
                            new Tool(){ UsesToolKeyName = "item:steelKnife", ProductivityFactor = 0.35f, DegradePerSecondOfUse = NoDegrade  },
                            new Tool(){ UsesToolKeyName = "item:advancedKnife", ProductivityFactor = 0.5f, DegradePerSecondOfUse = NoDegrade  },//was MediumDegrade

                            new Tool(){ UsesToolKeyName = "item:advancedMachete", ProductivityFactor = 0.3f, DegradePerSecondOfUse = NoDegrade  },//was MediumDegrade 
                            new Tool(){ UsesToolKeyName = "item:steelMachete", ProductivityFactor = 0.20f, DegradePerSecondOfUse = NoDegrade  },

                            new Tool(){ UsesToolKeyName = "item:steelHandAxe", ProductivityFactor = 0.18f, DegradePerSecondOfUse = NoDegrade  },    
                            new Tool(){ UsesToolKeyName = "item:improvisedHandAxe", ProductivityFactor = 0.15f, DegradePerSecondOfUse = NoDegrade  },   
                 
                            new Tool(){ UsesToolKeyName = "item:file", ProductivityFactor = 0.1f, DegradePerSecondOfUse = NoDegrade  },//was LowDegrade Dont use...not suited for most.

                         }
                         
                        }
                    }

                };
                listOfToolProfiles.Add(processToolSet);
                #endregion

                #region toolSetMakeImprovisedTool
                listOfToolProfiles.Add(new ProcessToolSet()
                {
                    KeyName = "toolSetMakeImprovisedTool",
                    Tools = new[] 
                    {                          
                        new ToolAlternatives() 
                        { Tools = new []// making the blade
                            {
                                new Tool(){ UsesToolKeyName = "item:smoothSandstone", ProductivityFactor = 0.15f, DegradePerSecondOfUse = NoDegrade  },//was MediumDegrade                       
                                 new Tool(){ UsesToolKeyName = "item:file", ProductivityFactor = 0.4f, DegradePerSecondOfUse = NoDegrade  },//was LowDegrade 
                                new Tool(){ UsesToolKeyName = "item:blacksmithsToolbox", ProductivityFactor = 0.5f, DegradePerSecondOfUse = NoDegrade}, //includes a file
                                new Tool(){ UsesToolKeyName = "item:metalWorkersToolbox", ProductivityFactor = 0.6f, DegradePerSecondOfUse = NoDegrade}, //includes a file                         
                            }                         
                        },
                        new ToolAlternatives() { Tools = new []// for joining the handle
                        {
                            new Tool(){ UsesToolTag = "improvisedGlue", ProductivityFactor = 0.1f, DegradePerSecondOfUse = NoDegrade  },     
                            new Tool(){ UsesToolKeyName = "item:advancedString", ProductivityFactor = 0.3f, DegradePerSecondOfUse = NoDegrade  },//was MediumDegrade
                             new Tool(){ UsesToolKeyName = "item:exaGlue", ProductivityFactor = 1f, DegradePerSecondOfUse = NoDegrade  },//was LowDegrade
                            new Tool(){ UsesToolKeyName = "item:metalWire", ProductivityFactor = 0.15f, DegradePerSecondOfUse = NoDegrade  },
                            new Tool(){ UsesToolKeyName = "item:rawhideString", ProductivityFactor = 0.1f, DegradePerSecondOfUse = NoDegrade  },
                            new Tool(){ UsesToolKeyName = "item:cottonString", ProductivityFactor = 0.4f, DegradePerSecondOfUse = NoDegrade  },
                        }                         
                        }
                    }

                });
                #endregion

                #region toolSetAttachWoodAndMetal
                processToolSet = new ProcessToolSet()
                {
                    KeyName = "toolSetAttachWoodAndMetal",
                    Tools = new[] {                    
                        new ToolAlternatives() { Tools = new []
                        {
                            new Tool(){ UsesToolTag = "improvisedGlue", ProductivityFactor = 0.1f, DegradePerSecondOfUse = NoDegrade  },     
                            new Tool(){ UsesToolKeyName = "item:advancedString", ProductivityFactor = 0.3f, DegradePerSecondOfUse = NoDegrade  },//was MediumDegrade
                             new Tool(){ UsesToolKeyName = "item:exaGlue", ProductivityFactor = 1f, DegradePerSecondOfUse = NoDegrade  },//was LowDegrade
                            new Tool(){ UsesToolKeyName = "item:metalWire", ProductivityFactor = 0.15f, DegradePerSecondOfUse = NoDegrade  },
                            new Tool(){ UsesToolKeyName = "item:cottonString", ProductivityFactor = 0.15f, DegradePerSecondOfUse = NoDegrade  },
                            new Tool(){ UsesToolKeyName = "item:rawhideString", ProductivityFactor = 0.1f, DegradePerSecondOfUse = NoDegrade  },
                        }                         
                        }
                    }
                };
                listOfToolProfiles.Add(processToolSet);
                #endregion

                #region toolSetNone
                processToolSet = new ProcessToolSet()
                {
                    KeyName = "toolSetNone",
                    Tools = new[] {                    
                        new ToolAlternatives() { Tools = new []
                        {     
                            new Tool(){ UsesToolKeyName = "item:advancedString", ProductivityFactor = 0.1f, DegradePerSecondOfUse = NoDegrade  },//was MediumDegrade //string er placeholder, burde være "human body" MP: men "none" er jo defineret som et item i itemloader, dvs kan vi ikke bare bruge toolkeyname=item:none, hvorfor er det nødv med en tag også? 
                        }
                         
                        }
                    }

                };

                listOfToolProfiles.Add(processToolSet);
                #endregion

                #region toolSetCutMetal
                processToolSet = new ProcessToolSet()
                {
                    KeyName = "toolSetCutMetal",
                    Tools = new[] {                    
                        new ToolAlternatives() { Tools = new []
                        {
                            new Tool(){ UsesToolKeyName = "item:metalCutter", ProductivityFactor = 2f, DegradePerSecondOfUse = NoDegrade  },//was MediumDegrade
                            new Tool(){ UsesToolKeyName = "item:advancedSnips", ProductivityFactor = 1f, DegradePerSecondOfUse = NoDegrade  },//was MediumDegrade
                     //        new Tool(){ UsesToolKeyName = "item:plasmaCutter", ProductivityFactor = 10f, DegradePerSecondOfUse = NoDegrade  },//was LowDegrade
                              new Tool(){ UsesToolKeyName = "item:tinnerSnips", ProductivityFactor = 0.5f, DegradePerSecondOfUse = NoDegrade  },
                        }
                         
                        }
                    }

                };
                listOfToolProfiles.Add(processToolSet);

                #endregion

                #region toolSetShapenSimpleSmallMetal

                processToolSet = new ProcessToolSet()
                {
                    KeyName = "toolSetShapenSimpleSmallMetal",
                    Tools = new[] {                    
                        new ToolAlternatives() { Tools = new []
                        {
                           // new Tool(){ UsesToolKeyName = "item:metalCutter", ProductivityFactor = 2f, DegradePerSecondOfUse = NoDegrade  },
                            new Tool(){ UsesToolKeyName = "item:advancedSnips", ProductivityFactor = 1f, DegradePerSecondOfUse = NoDegrade  },
                             new Tool(){ UsesToolKeyName = "item:smoothSandstone", ProductivityFactor = 0.08f, DegradePerSecondOfUse = NoDegrade  },
                              new Tool(){ UsesToolKeyName = "item:tinnerSnips", ProductivityFactor = 0.6f, DegradePerSecondOfUse = NoDegrade  },
                              new Tool(){ UsesToolKeyName = "item:steelHandAxe", ProductivityFactor = 0.08f, DegradePerSecondOfUse = NoDegrade  },

                           
                             new Tool(){ UsesToolKeyName = "item:file", ProductivityFactor = 0.2f, DegradePerSecondOfUse = NoDegrade  },
                                new Tool(){ UsesToolKeyName = "item:blacksmithsToolbox", ProductivityFactor = 0.5f, DegradePerSecondOfUse = NoDegrade}, //includes a file
                                new Tool(){ UsesToolKeyName = "item:metalWorkersToolbox", ProductivityFactor = 0.6f, DegradePerSecondOfUse = NoDegrade}, //includes a file
                        }
                         
                        }
                    }

                };
                listOfToolProfiles.Add(processToolSet);
                #endregion

                #region toolSetShapenSimpleSmallMetalAndtoolSetAttachWoodAndMetal
                processToolSet = new ProcessToolSet()
                {
                    KeyName = "toolSetShapenSimpleSmallMetalAndtoolSetAttachWoodAndMetal",
                    Tools = new[] {                    
                        new ToolAlternatives() { Tools = new []
                        {
                           // new Tool(){ UsesToolKeyName = "item:metalCutter", ProductivityFactor = 2f, DegradePerSecondOfUse = NoDegrade  },
                            new Tool(){ UsesToolKeyName = "item:advancedSnips", ProductivityFactor = 1f, DegradePerSecondOfUse = NoDegrade  },
                             new Tool(){ UsesToolKeyName = "item:smoothSandstone", ProductivityFactor = 0.08f, DegradePerSecondOfUse = NoDegrade  },
                              new Tool(){ UsesToolKeyName = "item:tinnerSnips", ProductivityFactor = 0.6f, DegradePerSecondOfUse = NoDegrade  },
                              new Tool(){ UsesToolKeyName = "item:steelHandAxe", ProductivityFactor = 0.08f, DegradePerSecondOfUse = NoDegrade  },

                            
                             new Tool(){ UsesToolKeyName = "item:file", ProductivityFactor = 0.2f, DegradePerSecondOfUse = NoDegrade  },
                                new Tool(){ UsesToolKeyName = "item:blacksmithsToolbox", ProductivityFactor = 0.5f, DegradePerSecondOfUse = NoDegrade}, //includes a file
                                new Tool(){ UsesToolKeyName = "item:metalWorkersToolbox", ProductivityFactor = 0.6f, DegradePerSecondOfUse = NoDegrade}, //includes a file
                        }
                         
                        },
                        new ToolAlternatives() { Tools = new []
                        {
                            new Tool(){ UsesToolTag = "improvisedGlue", ProductivityFactor = 0.1f, DegradePerSecondOfUse = NoDegrade  },     
                            new Tool(){ UsesToolKeyName = "item:advancedString", ProductivityFactor = 0.3f, DegradePerSecondOfUse = NoDegrade  },//was MediumDegrade
                             new Tool(){ UsesToolKeyName = "item:exaGlue", ProductivityFactor = 1f, DegradePerSecondOfUse = NoDegrade  },//was LowDegrade
                            new Tool(){ UsesToolKeyName = "item:metalWire", ProductivityFactor = 0.15f, DegradePerSecondOfUse = NoDegrade  },
                            new Tool(){ UsesToolKeyName = "item:rawhideString", ProductivityFactor = 0.1f, DegradePerSecondOfUse = NoDegrade  },
                            new Tool(){ UsesToolKeyName = "item:cottonString", ProductivityFactor = 0.15f, DegradePerSecondOfUse = NoDegrade  },
                        }                         
                        }
                    }

                };
                listOfToolProfiles.Add(processToolSet);
                #endregion

                #region toolSetShapenSimpleSmallMetalAndCombineLightImprovisedObjects
                processToolSet = new ProcessToolSet()
                {
                    KeyName = "toolSetShapenSimpleSmallMetalAndCombineLightImprovisedObjects",
                    Tools = new[] {                    
                        new ToolAlternatives() { Tools = new []
                        {
                           // new Tool(){ UsesToolKeyName = "item:metalCutter", ProductivityFactor = 2f, DegradePerSecondOfUse = NoDegrade  },
                            new Tool(){ UsesToolKeyName = "item:advancedSnips", ProductivityFactor = 1f, DegradePerSecondOfUse = NoDegrade  },
                             new Tool(){ UsesToolKeyName = "item:smoothSandstone", ProductivityFactor = 0.08f, DegradePerSecondOfUse = NoDegrade  },
                              new Tool(){ UsesToolKeyName = "item:tinnerSnips", ProductivityFactor = 0.6f, DegradePerSecondOfUse = NoDegrade  },
                              new Tool(){ UsesToolKeyName = "item:steelHandAxe", ProductivityFactor = 0.08f, DegradePerSecondOfUse = NoDegrade  },

                           
                             new Tool(){ UsesToolKeyName = "item:file", ProductivityFactor = 0.2f, DegradePerSecondOfUse = NoDegrade  },
                                new Tool(){ UsesToolKeyName = "item:blacksmithsToolbox", ProductivityFactor = 0.5f, DegradePerSecondOfUse = NoDegrade}, //includes a file
                                new Tool(){ UsesToolKeyName = "item:metalWorkersToolbox", ProductivityFactor = 0.6f, DegradePerSecondOfUse = NoDegrade}, //includes a file
                        }
                          // NA need clipping tools and binding tools
                        },
                        new ToolAlternatives() { Tools = new []
                        {
                            new Tool(){ UsesToolTag = "improvisedGlue", ProductivityFactor = 0.2f, DegradePerSecondOfUse = NoDegrade  },//was MediumDegrade
                            new Tool(){ UsesToolKeyName = "item:advancedString", ProductivityFactor = 0.5f, DegradePerSecondOfUse = NoDegrade  },//was MediumDegrade
                             new Tool(){ UsesToolKeyName = "item:exaGlue", ProductivityFactor = 1f, DegradePerSecondOfUse = NoDegrade  },//was LowDegrade
                             new Tool(){ UsesToolKeyName = "item:superconductingWire", ProductivityFactor = 0.3f, DegradePerSecondOfUse = NoDegrade  },
                            new Tool(){ UsesToolKeyName = "item:metalWire", ProductivityFactor = 0.4f, DegradePerSecondOfUse = NoDegrade  },
                            new Tool(){ UsesToolKeyName = "item:rawhideString", ProductivityFactor = 0.1f, DegradePerSecondOfUse = NoDegrade  },
                            new Tool(){ UsesToolKeyName = "item:cottonString", ProductivityFactor = 0.15f, DegradePerSecondOfUse = NoDegrade  },
                        }
                         
                        }
                    }

                };
                listOfToolProfiles.Add(processToolSet);
                #endregion

                #region toolSetFireplace
                processToolSet = new ProcessToolSet()
                {
                    KeyName = "toolSetFireplace",
                    Tools = new[] {                    
                        new ToolAlternatives() { Tools = new []
                        {   new Tool(){ UsesToolKeyName = "item:simpleStoveUpgrade", ProductivityFactor = 1f, DegradePerSecondOfUse = NoDegrade  },
                            new Tool(){ UsesToolKeyName = "structure:fieldKitchen", ProductivityFactor = 1f, DegradePerSecondOfUse = NoDegrade  }, 
                            new Tool(){ UsesToolKeyName = "structure:campfire", ProductivityFactor = 0.4f, DegradePerSecondOfUse = NoDegrade  }, //mp feb 2015 was 0.85f
                            new Tool(){ UsesToolKeyName = "structure:improvisedKitchen", ProductivityFactor = 0.8f, DegradePerSecondOfUse = NoDegrade  } ,
                            new Tool(){ UsesToolKeyName = "structure:mudBrickKitchen", ProductivityFactor = 0.8f, DegradePerSecondOfUse = NoDegrade  },

                        }
                         
                        }
                    }

                };
                listOfToolProfiles.Add(processToolSet);

                #endregion

                #region toolSetWorkbench
                //not used
                /*
                                processToolSet = new ProcessToolSet()
                                {
                                    KeyName = "toolSetWorkbench",
                                    Tools = new[] {                    
                                        new ToolAlternatives() { Tools = new []
                                        {
                                            new Tool(){ UsesToolKeyName = "structure:improvisedWorkbench", ProductivityFactor = 1f, DegradePerSecondOfUse = NoDegrade}, 
                                            new Tool(){ UsesToolKeyName = "structure:mudBrickWorkbench", ProductivityFactor = 1f, DegradePerSecondOfUse = NoDegrade},
                            
                                        } 
                                        }
                                    }
                                };
                                listOfToolProfiles.Add(processToolSet);
                */

                #endregion

                #region toolSetBluntTool


                processToolSet = new ProcessToolSet()
                {
                    KeyName = "toolSetBluntTool",
                    Tools = new[] 
                    {
                        new ToolAlternatives()
                        {   Tools = new []
                            {
                                new Tool(){ UsesToolKeyName = "item:blacksmithsToolbox", ProductivityFactor = 0.85f, DegradePerSecondOfUse = NoDegrade}, //has tongs and file added...not terribly relevant
                                new Tool(){ UsesToolKeyName = "item:metalWorkersToolbox", ProductivityFactor = 0.85f, DegradePerSecondOfUse = NoDegrade}, //added drill and hacksaw
                                new Tool(){ UsesToolKeyName = "item:hammer", ProductivityFactor = 0.8f, DegradePerSecondOfUse = NoDegrade}, 
                                new Tool(){ UsesToolKeyName = "item:stoneHammer", ProductivityFactor = 0.20f, DegradePerSecondOfUse = NoDegrade}, 
                            }
                        }
                    }
                };
                listOfToolProfiles.Add(processToolSet);
                #endregion

                
                #region ImprovisedForging
                processToolSet = new ProcessToolSet()
                {
                    KeyName = "toolSetImprovisedForging", //
                    Tools = new[] 
                    {                    
                        new ToolAlternatives()
                        {   Tools = new []
                            {           //mechanized forge with a power hammer and a powered grinder would be 1f
                                new Tool(){ UsesToolKeyName = "structure:improvisedSmithy", ProductivityFactor = 0.1f, DegradePerSecondOfUse = NoDegrade}, //was 0.1f
                                new Tool(){ UsesToolKeyName = "structure:simpleSmithy", ProductivityFactor = 0.3f, DegradePerSecondOfUse = NoDegrade}, //was 0.3f
                            }
                        },
                        new ToolAlternatives()
                        {   Tools = new []
                            { //mp. was UsesToolTag = "blowTool" but this made the bellows and blowpipe equally effective.
                                new Tool(){ UsesToolKeyName = "item:bellows", ProductivityFactor = 0.6f, DegradePerSecondOfUse = NoDegrade}, //1f would be an electrical ventilator
                                new Tool(){ UsesToolKeyName = "item:blowpipe", ProductivityFactor = 0.15f, DegradePerSecondOfUse = NoDegrade}, 
                            }
                        },
                        new ToolAlternatives()
                        {   Tools = new []
                            {//modern handtools are 1f.
                                new Tool(){ UsesToolKeyName = "item:blacksmithsToolbox", ProductivityFactor = 0.85f, DegradePerSecondOfUse = NoDegrade},
                                new Tool(){ UsesToolKeyName = "item:metalWorkersToolbox", ProductivityFactor = 0.9f, DegradePerSecondOfUse = NoDegrade}, //the hacksaw and drill add little extra value to these processees.
                                new Tool(){ UsesToolKeyName = "item:hammer", ProductivityFactor = 0.4f, DegradePerSecondOfUse = NoDegrade}, //was 0.85f
                                new Tool(){ UsesToolKeyName = "item:stoneHammer", ProductivityFactor = 0.10f, DegradePerSecondOfUse = NoDegrade}, 
                            }
                        }
                    
                    }

                };
                listOfToolProfiles.Add(processToolSet);
                #endregion

                #region HarderImprovisedForging //for tasks that require a file for filing. primarily for making the vise (which is upgrading from improvised to simple smithy)
                processToolSet = new ProcessToolSet()
                {
                    KeyName = "toolSetHarderImprovisedForging", //
                    Tools = new[] 
                    {                    
                        new ToolAlternatives()
                        {   Tools = new []
                            {           //mechanized forge with a power hammer and a powered grinder would be 1f
                                new Tool(){ UsesToolKeyName = "structure:improvisedSmithy", ProductivityFactor = 0.1f, DegradePerSecondOfUse = NoDegrade}, //was 0.1f
                                new Tool(){ UsesToolKeyName = "structure:simpleSmithy", ProductivityFactor = 0.3f, DegradePerSecondOfUse = NoDegrade}, //was 0.3f
                            }
                        },
                        new ToolAlternatives()
                        {   Tools = new []
                            { //mp. was UsesToolTag = "blowTool" but this made the bellows and blowpipe equally effective.
                                new Tool(){ UsesToolKeyName = "item:bellows", ProductivityFactor = 0.6f, DegradePerSecondOfUse = NoDegrade}, //1f would be an electrical ventilator
                                new Tool(){ UsesToolKeyName = "item:blowpipe", ProductivityFactor = 0.15f, DegradePerSecondOfUse = NoDegrade}, 
                            }
                        },
                        new ToolAlternatives()
                        {   Tools = new []
                            {//modern handtools are 1f.
                                new Tool(){ UsesToolKeyName = "item:blacksmithsToolbox", ProductivityFactor = 0.8f, DegradePerSecondOfUse = NoDegrade}, //contains a file
                                new Tool(){ UsesToolKeyName = "item:metalWorkersToolbox", ProductivityFactor = 0.9f, DegradePerSecondOfUse = NoDegrade},

                            }
                        }
                    
                    }

                };
                listOfToolProfiles.Add(processToolSet);
                #endregion

                #region simpleForging
                processToolSet = new ProcessToolSet()
                {
                    KeyName = "toolSetSimpleForging", //a more advanced forging than the one above.
                    Tools = new[] 
                    {                    
                        new ToolAlternatives()
                        {   
                            Tools = new []
                            {
                                new Tool(){ UsesToolKeyName = "structure:simpleSmithy", ProductivityFactor = 0.3f, DegradePerSecondOfUse = NoDegrade}, 
                            }
                        },
                        new ToolAlternatives()
                        {   
                            Tools = new []
                            {
                                new Tool(){ UsesToolKeyName = "item:bellows", ProductivityFactor = 0.6f, DegradePerSecondOfUse = NoDegrade}, //1f would be an electrical ventilator
                                new Tool(){ UsesToolKeyName = "item:blowpipe", ProductivityFactor = 0.15f, DegradePerSecondOfUse = NoDegrade}, 
                            }
                        },
                        new ToolAlternatives()
                        {   Tools = new []
                            {
                                new Tool(){ UsesToolKeyName = "item:blacksmithsToolbox", ProductivityFactor = 0.7f, DegradePerSecondOfUse = NoDegrade}, //mp stone hammer cannot be used at this stage.
                                new Tool(){ UsesToolKeyName = "item:metalWorkersToolbox", ProductivityFactor = 0.9f, DegradePerSecondOfUse = NoDegrade},

                            }
                        }                    
                    }
                };
                listOfToolProfiles.Add(processToolSet);
                #endregion

                #region barrelBoring
                processToolSet = new ProcessToolSet()
                {
                    KeyName = "toolSetBarrelBoring", //smooth boring can be done by blacksmith with a drill set up, without a fully fledged lathe. but takes longer.
                    Tools = new[] 
                    {                    
                        new ToolAlternatives()
                        {   
                            Tools = new []
                            {  
                                new Tool(){ UsesToolKeyName = "structure:simpleSmithy", ProductivityFactor = 0.13f, DegradePerSecondOfUse = NoDegrade}, //uses the bar clamps
                                new Tool(){ UsesToolKeyName = "item:metalLatheShopHumanPoweredUpgrade", ProductivityFactor = 0.6f, DegradePerSecondOfUse = NoDegrade}, //0.9f is engine powered metallathe. // 1f is  engine powered machine shop
                            }
                        },
                        new ToolAlternatives()
                        {   Tools = new []
                            {
                                new Tool(){ UsesToolKeyName = "item:metalWorkersToolbox", ProductivityFactor = 0.6f, DegradePerSecondOfUse = NoDegrade}, //primarily for The hand drill. 1f is hand power tools 

                            }
                        }                    
                    }
                };
                listOfToolProfiles.Add(processToolSet);
                #endregion

                #region metalLathe
                processToolSet = new ProcessToolSet()
                {
                    KeyName = "toolSetMetalLathe", //
                    Tools = new[] 
                    {                    
                        new ToolAlternatives()
                        {   
                            Tools = new []
                            {  
                                new Tool(){ UsesToolKeyName = "item:metalLatheShopHumanPoweredUpgrade", ProductivityFactor = 0.6f, DegradePerSecondOfUse = NoDegrade}, //0.9f is engine powered metallathe. // 1f is  engine powered machine shop
                            }
                        },                 
                    }
                };
                listOfToolProfiles.Add(processToolSet);
                #endregion

                #region BulletCasting
                processToolSet = new ProcessToolSet()
                {
                    KeyName = "toolSetBulletCasting",
                    Tools = new[] 
                    {                    
                        new ToolAlternatives()
                        {   
                            Tools = new []
                            {       //1f is an industrial furnace
                                new Tool(){ UsesToolKeyName = "structure:goldFurnace", ProductivityFactor = 0.45f, DegradePerSecondOfUse = NoDegrade}, 

                            }
                        },
                        new ToolAlternatives()
                        {   
                            Tools = new []
                            { //1f is a modern mold.
                                new Tool(){ UsesToolKeyName = "item:bulletMold", ProductivityFactor = 0.6f, DegradePerSecondOfUse = NoDegrade},
                            }
                        }                    
                    }
                };
                listOfToolProfiles.Add(processToolSet);
                #endregion

                #region PrimitiveCasting
                processToolSet = new ProcessToolSet()
                {
                    KeyName = "toolSetPrimitiveCasting", 
                    Tools = new[] 
                    {                    
                        new ToolAlternatives()
                        {   
                            Tools = new []
                            {       //1f is an industrial furnace
                                new Tool(){ UsesToolKeyName = "structure:goldFurnace", ProductivityFactor = 0.45f, DegradePerSecondOfUse = NoDegrade}, 

                            }
                        },
                        new ToolAlternatives()
                        {   
                            Tools = new []
                            // patterns / the template is not simulated. it is included in the work time for casting.. to simplify this process.
                            { //1f is a modern casting flask made of metal. 
                                
                                new Tool(){ UsesToolKeyName = "item:sandMold", ProductivityFactor = 0.6f, DegradePerSecondOfUse = NoDegrade},
                            }
                        },
                        new ToolAlternatives()
                        {   
                            Tools = new [] //  simply removing sprues. the metal channels that stick out. 1f is a handheld power grinder
                            { //1f is ....
                                new Tool(){ UsesToolKeyName = "item:file", ProductivityFactor = 0.3f, DegradePerSecondOfUse = NoDegrade},
                                new Tool(){ UsesToolKeyName = "item:blacksmithsToolbox", ProductivityFactor = 0.5f, DegradePerSecondOfUse = NoDegrade},
                                new Tool(){ UsesToolKeyName = "item:metalWorkersToolbox", ProductivityFactor = 0.75f, DegradePerSecondOfUse = NoDegrade},

                            }
                        }                        
                    }
                };
                listOfToolProfiles.Add(processToolSet);
                #endregion

                #region SimpleCasting
                processToolSet = new ProcessToolSet()
                {
                    KeyName = "toolSetSimpleCasting",
                    Tools = new[] 
                    {                    
                        new ToolAlternatives()
                        {   
                            Tools = new []
                            {       //1f is an industrial furnace
                                new Tool(){ UsesToolKeyName = "structure:goldFurnace", ProductivityFactor = 0.45f, DegradePerSecondOfUse = NoDegrade}, 

                            }
                        },
                        new ToolAlternatives()
                        {   
                            Tools = new []
                            // patterns / the template is not simulated. it is included in the work time for casting.. to simplify this process.
                            { //1f is a modern casting flask made of metal. 
                                
                                new Tool(){ UsesToolKeyName = "item:sandMold", ProductivityFactor = 0.6f, DegradePerSecondOfUse = NoDegrade},
                            }
                        },
                        new ToolAlternatives()
                        {   
                            Tools = new [] //  removing sprues and also shaping and drilling the cast objects to some extent. 1f is a handheld power grinder
                            { //1f is ....
                                new Tool(){ UsesToolKeyName = "item:metalWorkersToolbox", ProductivityFactor = 0.75f, DegradePerSecondOfUse = NoDegrade},

                            }
                        }                        
                    }
                };
                listOfToolProfiles.Add(processToolSet);
                #endregion

                #region AssembleMetalMachine
                processToolSet = new ProcessToolSet()
                {
                    KeyName = "toolSetAssembleMetalMachine", //such as the metal lathe
                    Tools = new[] {                    
                        new ToolAlternatives() { Tools = new []
                        { //1f is Hand power tools
                            new Tool(){ UsesToolKeyName = "item:metalWorkersToolbox", ProductivityFactor = 0.75f, DegradePerSecondOfUse = NoDegrade  }, 
                           
                        }           
                        },
                          new ToolAlternatives() { Tools = new [] //for attaching bits and pieces. the belt drive is an input material.
                        {
                                new Tool(){ UsesToolKeyName = "item:advancedString", ProductivityFactor = 0.7f, DegradePerSecondOfUse = NoDegrade  }, //
                                new Tool(){ UsesToolKeyName = "item:superconductingWire", ProductivityFactor = 0.4f, DegradePerSecondOfUse = NoDegrade  },//
                            new Tool(){ UsesToolTag = "improvisedGlue", ProductivityFactor = 0.4f, DegradePerSecondOfUse = NoDegrade  },//
                             new Tool(){ UsesToolKeyName = "item:exaGlue", ProductivityFactor = 1f, DegradePerSecondOfUse = NoDegrade  },//
                                new Tool(){ UsesToolKeyName = "item:metalWire", ProductivityFactor = 0.4f, DegradePerSecondOfUse = NoDegrade  }, //
                        }                         
                        }                    
                    }
                };
                listOfToolProfiles.Add(processToolSet);
                #endregion

              /*  #region BuildMachineWorkshop
                processToolSet = new ProcessToolSet()
                {
                    KeyName = "toolSetBuildMachineWorkshop", //such as polymer workshop
                    Tools = new[] {                    
                        new ToolAlternatives() { Tools = new []
                        { //1f is Hand power tools
                            new Tool(){ UsesToolKeyName = "item:metalWorkersToolbox", ProductivityFactor = 0.75f, DegradePerSecondOfUse = NoDegrade  }, 
                           
                        }           
                        },
                         new ToolAlternatives() { Tools = new [] //for attaching bits and pieces. 
                        {
                                new Tool(){ UsesToolKeyName = "item:advancedString", ProductivityFactor = 0.7f, DegradePerSecondOfUse = NoDegrade  }, //
                                new Tool(){ UsesToolKeyName = "item:superconductingWire", ProductivityFactor = 0.4f, DegradePerSecondOfUse = NoDegrade  },//
                                new Tool(){ UsesToolTag = "improvisedGlue", ProductivityFactor = 0.4f, DegradePerSecondOfUse = NoDegrade  },//
                                new Tool(){ UsesToolKeyName = "item:exaGlue", ProductivityFactor = 1f, DegradePerSecondOfUse = NoDegrade  },//
                                new Tool(){ UsesToolKeyName = "item:metalWire", ProductivityFactor = 0.4f, DegradePerSecondOfUse = NoDegrade  }, //
                        }                         
                        }                                             
                    }
                };
                listOfToolProfiles.Add(processToolSet);
                #endregion*/


                // TB Farming 19.03.2015
                #region smallPlot
                    #region plowingTools
                processToolSet = new ProcessToolSet()
                {
                    KeyName = "toolSetPlowingTools", // MP: I split this into tilling+plowing and weeding but weedingRobotTool doesn't work???!
                    Tools = new[]
                    {                    
                        new ToolAlternatives() //mp preparing hard soil for cultivation. a bit like digging. 1f should be an electric tiller/cultivator: https://www.google.dk/search?q=electric+tiller&espv=2&biw=1456&bih=761&tbm=isch&tbo=u&source=univ&sa=X&ved=0CDkQ7AlqFQoTCMm96b6M-8YCFWSacgodpDIImw
                                          //mp also used for making peat bank and digging peat since it is basically the same grassland soil being worked in. not used for gathering firegrass sod. maybe should?
                        { 
                            Tools = new []
                            { 
                                new Tool(){ UsesToolKeyName = "item:steelSpade", ProductivityFactor = 0.75f, DegradePerSecondOfUse = NoDegrade }, //mp we could set this even lower, compared to electric tool...
                                new Tool(){ UsesToolKeyName = "item:improvisedSpade", ProductivityFactor = 0.7f, DegradePerSecondOfUse = NoDegrade },
                                new Tool(){ UsesToolKeyName = "item:steelHoe", ProductivityFactor = 0.5f, DegradePerSecondOfUse = NoDegrade },
                                new Tool(){ UsesToolKeyName = "item:farmingHoe", ProductivityFactor = 0.45f, DegradePerSecondOfUse = NoDegrade },
                         
//
                              
                            }
                        }
                    }
                };
                listOfToolProfiles.Add(processToolSet);
                    #endregion
                #region weedingTools

                
                processToolSet = new ProcessToolSet()
                {                    
                    KeyName = "toolSetSowingWeedingFertilizingFarmPlot", //"toolSetWeedingTools", 
                    Comments = "Used for sowing, weeding and fertilizing in farm plots.",
                    Tools = new[]
                    {                    
                        sowingWeedingFertilizingFarmPlotTools
                    }
                };
                listOfToolProfiles.Add(processToolSet);
                #endregion

                    #region unPlowingTools
                processToolSet = new ProcessToolSet()
                {
                    KeyName = "toolSetUnPlowingTools",
                    Tools = new[]
                    {                    
                        new ToolAlternatives() 
                        { 
                            Tools = new []
                            {    
                                new Tool(){ UsesToolTag = "plowingTools", ProductivityFactor = 1.0f, DegradePerSecondOfUse = NoDegrade },
                            }
                        }
                    }
                };
                listOfToolProfiles.Add(processToolSet);
                    #endregion
                #endregion

                #region Clay pit
                processToolSet = new ProcessToolSet()
                {
                    KeyName = "toolSetClayPit",
                    Comments = "the spade is best because it can break and move the material",
                    Tools = new[] {                    
                        new ToolAlternatives() { Tools = new []
                        {
                            new Tool(){ UsesToolKeyName = "structure:clayPit", ProductivityFactor = 1f, DegradePerSecondOfUse = NoDegrade  }                        
                        } ,
                        
                        },
                        new ToolAlternatives() { Tools = new [] 
                        {
                            new Tool(){ UsesToolKeyName = "item:improvisedTrowel", ProductivityFactor = 0.15f, DegradePerSecondOfUse = NoDegrade  },                                                    
                            new Tool(){ UsesToolKeyName = "item:farmingHoe", ProductivityFactor = 0.27f, DegradePerSecondOfUse = NoDegrade },                           
                            new Tool(){ UsesToolKeyName = "item:steelHoe", ProductivityFactor = 0.3f, DegradePerSecondOfUse = NoDegrade },
                            new Tool(){ UsesToolKeyName = "item:improvisedPickaxe", ProductivityFactor = 0.4f, DegradePerSecondOfUse = NoDegrade  },
                            new Tool(){ UsesToolKeyName = "item:improvisedSpade", ProductivityFactor = 0.5f, DegradePerSecondOfUse = NoDegrade  },                            
                            new Tool(){ UsesToolKeyName = "item:steelPickaxe", ProductivityFactor = 0.6f, DegradePerSecondOfUse = NoDegrade  },                             
                            new Tool(){ UsesToolKeyName = "item:steelSpade", ProductivityFactor = 0.9f, DegradePerSecondOfUse = NoDegrade  },                     
                        } ,
                        
                        }
                    }

                };
                listOfToolProfiles.Add(processToolSet);
                #endregion

                #region Salt mine
                processToolSet = new ProcessToolSet()
                {
                    KeyName = "toolSetSaltMine",
                    Comments = "salt is harder to break so the pickaxe is more suited, even though it cannot move the material",
                    Tools = new[] {                    
                        new ToolAlternatives() { Tools = new []
                        {
                            new Tool(){ UsesToolKeyName = "structure:saltMine", ProductivityFactor = 1f, DegradePerSecondOfUse = NoDegrade  }                        
                        }                         
                        },

                        new ToolAlternatives() { Tools = new [] 
                        {
                            // 
                            new Tool(){ UsesToolKeyName = "item:improvisedTrowel", ProductivityFactor = 0.15f, DegradePerSecondOfUse = NoDegrade  },                            
                            new Tool(){ UsesToolKeyName = "item:farmingHoe", ProductivityFactor = 0.27f, DegradePerSecondOfUse = NoDegrade },                               
                            new Tool(){ UsesToolKeyName = "item:steelHoe", ProductivityFactor = 0.3f, DegradePerSecondOfUse = NoDegrade },                            
                            new Tool(){ UsesToolKeyName = "item:improvisedSpade", ProductivityFactor = 0.5f, DegradePerSecondOfUse = NoDegrade  },
                            new Tool(){ UsesToolKeyName = "item:improvisedPickaxe", ProductivityFactor = 0.55f, DegradePerSecondOfUse = NoDegrade  },                        
                            new Tool(){ UsesToolKeyName = "item:steelSpade", ProductivityFactor = 0.8f, DegradePerSecondOfUse = NoDegrade  },                     
                            new Tool(){ UsesToolKeyName = "item:steelPickaxe", ProductivityFactor = 0.9f, DegradePerSecondOfUse = NoDegrade  }                           
                        } ,
                        
                        }

                    },


                };
                listOfToolProfiles.Add(processToolSet);
                #endregion

                #region Bog ore pit
                processToolSet = new ProcessToolSet()
                {
                    KeyName = "toolSetBogOrePit",
                    Tools = new[] {                    
                        new ToolAlternatives() { Tools = new []
                        {
                            new Tool(){ UsesToolKeyName = "structure:bogOrePit", ProductivityFactor = 1f, DegradePerSecondOfUse = NoDegrade  }                        
                        }                         
                        },

                        new ToolAlternatives() { Tools = new [] //copy pasted from "toolSetDiggingSoil":
                        {
                            new Tool(){ UsesToolKeyName = "item:improvisedTrowel", ProductivityFactor = 0.15f, DegradePerSecondOfUse = NoDegrade  },
                                new Tool(){ UsesToolKeyName = "item:improvisedPickaxe", ProductivityFactor = 0.28f, DegradePerSecondOfUse = NoDegrade  },
                                new Tool(){ UsesToolKeyName = "item:steelPickaxe", ProductivityFactor = 0.33f, DegradePerSecondOfUse = NoDegrade  }, 
                                new Tool(){ UsesToolKeyName = "item:steelHoe", ProductivityFactor = 0.3f, DegradePerSecondOfUse = NoDegrade },
                                new Tool(){ UsesToolKeyName = "item:farmingHoe", ProductivityFactor = 0.27f, DegradePerSecondOfUse = NoDegrade },                           
                            new Tool(){ UsesToolKeyName = "item:improvisedSpade", ProductivityFactor = 0.75f, DegradePerSecondOfUse = NoDegrade  },
                            new Tool(){ UsesToolKeyName = "item:steelSpade", ProductivityFactor = 0.9f, DegradePerSecondOfUse = NoDegrade  },                     
                        } ,
                        
                        }

                    },


                };
                listOfToolProfiles.Add(processToolSet);
                #endregion
                #region RareMetal ore pit
                //NA MINING CAMP MATERIAL
                processToolSet = new ProcessToolSet()
                {
                    KeyName = "toolSetRareMetalOrePit",
                    Tools = new[] {                    
                        new ToolAlternatives() { Tools = new []
                        {
                            new Tool(){ UsesToolKeyName = "structure:rareMetalOrePit1", ProductivityFactor = 1f, DegradePerSecondOfUse = NoDegrade  }                        
                        }                         
                        },

                        new ToolAlternatives() { Tools = new [] //NA Gotta make diffrent tool list for haul robot to be the one doing it?
                        {
                            new Tool(){ UsesToolKeyName = "item:diggingRobotTool", ProductivityFactor = 0.15f, DegradePerSecondOfUse = NoDegrade  },
                                
                       } ,
                        
                        }

                    },


                };
                listOfToolProfiles.Add(processToolSet);
                #endregion
                #region RareMetal ore pit 2
                //NA MINING CAMP MATERIAL
                processToolSet = new ProcessToolSet()
                {
                    KeyName = "toolSetRareMetalOrePit2",
                    Tools = new[] {                    
                        new ToolAlternatives() { Tools = new []
                        {
                            new Tool(){ UsesToolKeyName = "structure:rareMetalOrePit2", ProductivityFactor = 1f, DegradePerSecondOfUse = NoDegrade  }                        
                        }                         
                        },

                        new ToolAlternatives() { Tools = new [] //NA Gotta make diffrent tool list for haul robot to be the one doing it?
                        {
                            new Tool(){ UsesToolKeyName = "item:diggingRobotTool", ProductivityFactor = 0.15f, DegradePerSecondOfUse = NoDegrade  },
                                
                       } ,
                        
                        }

                    },


                };
                listOfToolProfiles.Add(processToolSet);
                #endregion
               
              
                #region Peat bank
                processToolSet = new ProcessToolSet()
                {
                    KeyName = "toolSetPeatBank",
                    Tools = new[] {                    
                        new ToolAlternatives() { Tools = new []
                        {
                            new Tool(){ UsesToolKeyName = "structure:peatBank", ProductivityFactor = 1f, DegradePerSecondOfUse = NoDegrade  }                        
                        }                         
                        },

                        new ToolAlternatives() { Tools = new [] //copy pasted from "toolSetPlowingTools" since it's firegrass turf
                        {
                                new Tool(){ UsesToolKeyName = "item:steelSpade", ProductivityFactor = 0.75f, DegradePerSecondOfUse = NoDegrade }, //mp we could set this even lower, compared to electric tool...
                                new Tool(){ UsesToolKeyName = "item:improvisedSpade", ProductivityFactor = 0.7f, DegradePerSecondOfUse = NoDegrade },
                                new Tool(){ UsesToolKeyName = "item:steelHoe", ProductivityFactor = 0.5f, DegradePerSecondOfUse = NoDegrade },
                                new Tool(){ UsesToolKeyName = "item:farmingHoe", ProductivityFactor = 0.45f, DegradePerSecondOfUse = NoDegrade },                   
                        } ,
                        
                        }

                    },


                };
                listOfToolProfiles.Add(processToolSet);
                #endregion

                #region Favor bread farm
                processToolSet = new ProcessToolSet()
                {
                    KeyName = "toolSetFavorbreadFarm",
                    Tools = new[] {                    
                        new ToolAlternatives() { Tools = new []
                        {
                            new Tool(){ UsesToolKeyName = "structure:favorbreadFarm", ProductivityFactor = 1f, DegradePerSecondOfUse = NoDegrade  }                        
                        }                         
                        },
                        new ToolAlternatives() { Tools = new [] //copied from greenhouseharvest
                            {  
                                new Tool(){ UsesToolKeyName = "item:flintKnife", ProductivityFactor = 0.3f, DegradePerSecondOfUse = NoDegrade },
                                new Tool(){ UsesToolKeyName = "item:improvisedKnife", ProductivityFactor = 0.65f, DegradePerSecondOfUse = NoDegrade },
                                new Tool(){ UsesToolKeyName = "item:steelKnife", ProductivityFactor = 0.80f, DegradePerSecondOfUse = NoDegrade },
                                new Tool(){ UsesToolKeyName = "item:advancedKnife", ProductivityFactor = 0.85f, DegradePerSecondOfUse = NoDegrade },
                                new Tool(){ UsesToolKeyName = "item:improvisedTrowel", ProductivityFactor = 0.4f, DegradePerSecondOfUse = NoDegrade },
                            }                       
                        },
                    },

                };
                listOfToolProfiles.Add(processToolSet);
                #endregion




                #region SmokeOven
                processToolSet = new ProcessToolSet()
                {
                    KeyName = "toolSetSmokeOven",
                    Tools = new[] {                    
                        new ToolAlternatives() { Tools = new []
                        {
                            new Tool(){ UsesToolKeyName = "structure:smokeOven", ProductivityFactor = 0.8f, DegradePerSecondOfUse = NoDegrade  }, //mp june 2016 was: ProductivityFactor = 1f
                            new Tool(){ UsesToolKeyName = "item:smokeOvenUpgrade", ProductivityFactor = 1f, DegradePerSecondOfUse = NoDegrade  }                             
                        }
                         
                        }
                    }

                };
                listOfToolProfiles.Add(processToolSet);
                #endregion
                #region Kitchen
                processToolSet = new ProcessToolSet()
                {
                    KeyName = "toolSetKitchen", //rotated on a spit at improvised kitchen
                    Tools = new[] {                    
                        new ToolAlternatives() { Tools = new []
                        {
                            new Tool(){ UsesToolKeyName = "item:simpleStoveUpgrade", ProductivityFactor = 1f, DegradePerSecondOfUse = NoDegrade  }, 
                            new Tool(){ UsesToolKeyName = "structure:fieldKitchen", ProductivityFactor = 1f, DegradePerSecondOfUse = NoDegrade  },  
                            new Tool(){ UsesToolKeyName = "structure:improvisedKitchen", ProductivityFactor = 0.8f, DegradePerSecondOfUse = NoDegrade  },
                            new Tool(){ UsesToolKeyName = "structure:mudBrickKitchen", ProductivityFactor = 0.8f, DegradePerSecondOfUse = NoDegrade  }, //
                           
                        }           
                        },
                          new ToolAlternatives() { Tools = new [] 
                        {
                            new Tool(){ UsesToolKeyName = "item:improvisedKnife", ProductivityFactor = 0.4f, DegradePerSecondOfUse = NoDegrade  },
                            new Tool(){ UsesToolKeyName = "item:steelKnife", ProductivityFactor = 0.6f, DegradePerSecondOfUse = NoDegrade  },
                            new Tool(){ UsesToolKeyName = "item:advancedKnife", ProductivityFactor = 0.7f, DegradePerSecondOfUse = NoDegrade  },
                            new Tool(){ UsesToolKeyName = "item:advancedMachete", ProductivityFactor = 0.1f, DegradePerSecondOfUse = NoDegrade  }, 
                            new Tool(){ UsesToolKeyName = "item:steelMachete", ProductivityFactor = 0.06f, DegradePerSecondOfUse = NoDegrade  }, 
                        }
                         
                        }
                    
                    }

                };
                listOfToolProfiles.Add(processToolSet);
                #endregion


                #region toolSetFermentWithKitchenAndJar
                processToolSet = new ProcessToolSet()
                {
                    KeyName = "toolSetFermentWithKitchenAndJar", //extra hygiene is needed for this, a clean kitchen surface.
                    Tools = new[] {                    
                        new ToolAlternatives() { Tools = new []
                        {
                            new Tool(){ UsesToolKeyName = "item:simpleStoveUpgrade", ProductivityFactor = 1f, DegradePerSecondOfUse = NoDegrade  },                             
                            new Tool(){ UsesToolKeyName = "structure:fieldKitchen", ProductivityFactor = 1f, DegradePerSecondOfUse = NoDegrade  },  
                            new Tool(){ UsesToolKeyName = "structure:improvisedKitchen", ProductivityFactor = 0.7f, DegradePerSecondOfUse = NoDegrade  }, //
                            new Tool(){ UsesToolKeyName = "structure:mudBrickKitchen", ProductivityFactor = 0.7f, DegradePerSecondOfUse = NoDegrade  }                          
                        }           
                        },
                          new ToolAlternatives() { Tools = new [] 
                        {
                            new Tool(){ UsesToolKeyName = "item:clayJar", ProductivityFactor = 0.8f, DegradePerSecondOfUse = NoDegrade  },
                            new Tool(){ UsesToolKeyName = "item:improvisedPlasticJar", ProductivityFactor = 0.8f, DegradePerSecondOfUse = NoDegrade  },
 
                        }
                         
                        }
                    
                    }

                };
                listOfToolProfiles.Add(processToolSet);
                #endregion

                #region toolSetMakeVinegar
                processToolSet = new ProcessToolSet()
                {
                    KeyName = "toolSetMakeVinegar", //vinegar is 10 X quicker to make once you already have vinegar.('mother of vinegar') but field lab also should have a role to play!!
                    Tools = new[] {                    
                        new ToolAlternatives() { Tools = new []
                        {
                            new Tool(){ UsesToolKeyName = "item:simpleStoveUpgrade", ProductivityFactor = 0.4f, DegradePerSecondOfUse = NoDegrade  }, 
                            new Tool(){ UsesToolKeyName = "structure:fieldKitchen", ProductivityFactor = 0.3f, DegradePerSecondOfUse = NoDegrade  },  
                            new Tool(){ UsesToolKeyName = "structure:improvisedKitchen", ProductivityFactor = 0.25f, DegradePerSecondOfUse = NoDegrade  }, //
                            new Tool(){ UsesToolKeyName = "structure:mudBrickKitchen", ProductivityFactor = 0.25f, DegradePerSecondOfUse = NoDegrade  },  
                             new Tool(){ UsesToolKeyName = "structure:fieldLab", ProductivityFactor = 1f, DegradePerSecondOfUse = NoDegrade  },     //the field lab should make a noticeable difference in crafting speed                 
                        }           
                        },

                          new ToolAlternatives() { Tools = new [] 
                        {
                            new Tool(){ UsesToolKeyName = "item:clayJar", ProductivityFactor = 0.8f, DegradePerSecondOfUse = NoDegrade  },
                            new Tool(){ UsesToolKeyName = "item:improvisedPlasticJar", ProductivityFactor = 0.8f, DegradePerSecondOfUse = NoDegrade  },
 
                        }
                         
                        },

                         new ToolAlternatives() { Tools = new [] 
                        {
                            new Tool(){ UsesToolKeyName = "item:vinegar", ProductivityFactor = 1f, DegradePerSecondOfUse = NoDegrade  }, //'mother of vinegar', preferably makes it 10 X quicker but we have to balance it so that field lab also has a big effect.
                            new Tool(){ UsesToolKeyName = "item:advancedKnife", ProductivityFactor = 0.3f, DegradePerSecondOfUse = NoDegrade  }, //the knives cut up the fruit but are really mostly an alternative to 'mother of vinegar'
                            new Tool(){ UsesToolKeyName = "item:improvisedKnife", ProductivityFactor = 0.3f, DegradePerSecondOfUse = NoDegrade  },
                            new Tool(){ UsesToolKeyName = "item:steelKnife", ProductivityFactor = 0.3f, DegradePerSecondOfUse = NoDegrade  }, 
                        }
                         
                        }
                    
                    }

                };
                listOfToolProfiles.Add(processToolSet);
                #endregion

                #region toolSetJarNoHeating

                processToolSet = new ProcessToolSet()
                {
                    KeyName = "toolSetJarNoHeating",//no heating needed, just a big container used for food.. 
                    Tools = new[] {                    
                        new ToolAlternatives() { Tools = new []
                        {
                            new Tool(){ UsesToolKeyName = "item:clayJar", ProductivityFactor = 0.8f, DegradePerSecondOfUse = NoDegrade  }, 
                            new Tool(){ UsesToolKeyName = "item:improvisedPlasticJar", ProductivityFactor = 0.8f, DegradePerSecondOfUse = NoDegrade  }, 
                        }                         
                        }                    
                    }
                };
                listOfToolProfiles.Add(processToolSet);
                #endregion

                #region toolSetMakeBrandy
                processToolSet = new ProcessToolSet()
                {
                    KeyName = "toolSetMakeBrandy", //
                    Tools = new[] {                    
                        new ToolAlternatives() { Tools = new []
                        {
                            new Tool(){ UsesToolKeyName = "structure:still", ProductivityFactor = 0.8f, DegradePerSecondOfUse = NoDegrade  }, 
                //mp should we also use the field lab for this?? should probalby be less efficient (lower quantities)
                        }           
                        },

                          new ToolAlternatives() { Tools = new [] 
                        {
                            new Tool(){ UsesToolKeyName = "item:clayJar", ProductivityFactor = 0.8f, DegradePerSecondOfUse = NoDegrade  },
                            new Tool(){ UsesToolKeyName = "item:improvisedPlasticJar", ProductivityFactor = 0.8f, DegradePerSecondOfUse = NoDegrade  },
 
                        }                         
                        },                    
                    }
                };
                listOfToolProfiles.Add(processToolSet);
                #endregion

                #region toolSetMeatDrying

                processToolSet = new ProcessToolSet()
                {
                    KeyName = "toolSetMeatDrying", //
                    Tools = new[] {                    
                        new ToolAlternatives() { Tools = new []
                        {
 
                            new Tool(){ UsesToolKeyName = "structure:meatDryingRack", ProductivityFactor = 0.5f, DegradePerSecondOfUse = NoDegrade  },
                            new Tool(){ UsesToolKeyName = "structure:dryingShed", ProductivityFactor = 0.8f, DegradePerSecondOfUse = NoDegrade  },
                            new Tool(){ UsesToolKeyName = "item:dryingShedUpgrade", ProductivityFactor = 0.9f, DegradePerSecondOfUse = NoDegrade  }
                           
                        }           
                        },
                          new ToolAlternatives() { Tools = new [] //the meat needs to be cut in very narrow strips.
                        {
                            new Tool(){ UsesToolKeyName = "item:improvisedKnife", ProductivityFactor = 0.4f, DegradePerSecondOfUse = NoDegrade  },
                            new Tool(){ UsesToolKeyName = "item:steelKnife", ProductivityFactor = 0.6f, DegradePerSecondOfUse = NoDegrade  },
                            new Tool(){ UsesToolKeyName = "item:advancedKnife", ProductivityFactor = 0.7f, DegradePerSecondOfUse = NoDegrade  },
                            new Tool(){ UsesToolKeyName = "item:advancedMachete", ProductivityFactor = 0.1f, DegradePerSecondOfUse = NoDegrade  }, 
                            new Tool(){ UsesToolKeyName = "item:steelMachete", ProductivityFactor = 0.06f, DegradePerSecondOfUse = NoDegrade  }, 
                        }
                         
                        }
                    
                    }

                };
                listOfToolProfiles.Add(processToolSet);
                #endregion

                #region toolSetSalamiDrying
                processToolSet = new ProcessToolSet()
                {
                    KeyName = "toolSetSalamiDrying", // no knife needed
                    Tools = new[] {                    
                        new ToolAlternatives() { Tools = new []
                        {
 
                            new Tool(){ UsesToolKeyName = "structure:meatDryingRack", ProductivityFactor = 0.5f, DegradePerSecondOfUse = NoDegrade  },
                            new Tool(){ UsesToolKeyName = "structure:dryingShed", ProductivityFactor = 0.8f, DegradePerSecondOfUse = NoDegrade  }, //
                            new Tool(){ UsesToolKeyName = "item:dryingShedUpgrade", ProductivityFactor = 0.9f, DegradePerSecondOfUse = NoDegrade  }
                        }
                         
                        }
                    
                    }

                };
                listOfToolProfiles.Add(processToolSet);
                #endregion

                #region toolSetHarvestSap

                processToolSet = new ProcessToolSet()
                {
                    KeyName = "toolSetHarvestSap", // a stationary container placed underneath a cut in the plant.
                    Tools = new[] {                    
                        new ToolAlternatives() { Tools = new []
                        {
                            new Tool(){ UsesToolKeyName = "item:plasticTappingBucket", ProductivityFactor = 0.9f, DegradePerSecondOfUse = NoDegrade  },
                            new Tool(){ UsesToolKeyName = "item:tappingBucket", ProductivityFactor = 0.9f, DegradePerSecondOfUse = NoDegrade  },
                            new Tool(){ UsesToolKeyName = "item:clayJar", ProductivityFactor = 0.4f, DegradePerSecondOfUse = NoDegrade  },
                            new Tool(){ UsesToolKeyName = "item:improvisedPlasticJar", ProductivityFactor = 0.4f, DegradePerSecondOfUse = NoDegrade  },
                            new Tool(){ UsesToolKeyName = "item:vat", ProductivityFactor = 0.4f, DegradePerSecondOfUse = NoDegrade  },   
                            new Tool(){ UsesToolKeyName = "item:woodenCookingPot", ProductivityFactor = 0.4f, DegradePerSecondOfUse = NoDegrade  }
                           
                        }           
                        },
                    
                    }

                };
                listOfToolProfiles.Add(processToolSet);
                #endregion

                #region toolSetMakeSulfurSmokeBomb

                processToolSet = new ProcessToolSet()
                {
                    KeyName = "toolSetMakeSulfurSmokeBomb",
                    Tools = new[] {                    
                        new ToolAlternatives() { Tools = new []
                        {
                            new Tool(){ UsesToolKeyName = "item:simpleStoveUpgrade", ProductivityFactor = 0.8f, DegradePerSecondOfUse = NoDegrade  },
                            new Tool(){ UsesToolKeyName = "structure:campfire", ProductivityFactor = 0.5f, DegradePerSecondOfUse = NoDegrade  },//mp feb 2015 was 0.8f 
                            new Tool(){ UsesToolKeyName = "structure:fieldKitchen", ProductivityFactor = 1f, DegradePerSecondOfUse = NoDegrade  },  
                            new Tool(){ UsesToolKeyName = "structure:improvisedKitchen", ProductivityFactor = 0.8f, DegradePerSecondOfUse = NoDegrade  },
                            new Tool(){ UsesToolKeyName = "structure:mudBrickKitchen", ProductivityFactor = 0.8f, DegradePerSecondOfUse = NoDegrade  },
                           
                        }           //fire AND pot needed
                        },
                          new ToolAlternatives() { Tools = new []
                        {   new Tool(){ UsesToolKeyName = "item:goldPot", ProductivityFactor = 0.8f, DegradePerSecondOfUse = NoDegrade  },
                            new Tool(){ UsesToolKeyName = "item:advancedCookingPot", ProductivityFactor = 0.8f, DegradePerSecondOfUse = NoDegrade  },//was NoDegrade   
                            new Tool(){ UsesToolKeyName = "item:improvisedCookingPot", ProductivityFactor = 0.6f, DegradePerSecondOfUse = NoDegrade  }, //
                            new Tool(){ UsesToolKeyName = "item:clayPotUnglazed", ProductivityFactor = 0.35f, DegradePerSecondOfUse = NoDegrade  },
                        }
                         
                        }
                    
                    }

                };
                listOfToolProfiles.Add(processToolSet);
                #endregion

                #region toolSetMakeRubber
                listOfToolProfiles.Add(new ProcessToolSet()
                {
                    KeyName = "toolSetMakeRubber",
                    Tools = new[] {                    
                        new ToolAlternatives() { Tools = new []
                        {                          
                            new Tool(){ UsesToolKeyName = "item:polymerWorkshopUpgrade", ProductivityFactor = 0.7f, DegradePerSecondOfUse = NoDegrade  }                           
                        }           
                        },
                          new ToolAlternatives() { Tools = new []
                        {   new Tool(){ UsesToolKeyName = "item:vinegar", ProductivityFactor = 0.7f, DegradePerSecondOfUse = NoDegrade  }, //a mild acid for coagulating the sap into rubber sheets
                        }                         
                        }                    
                    }
                });
                #endregion

                #region toolSetMakeTextile
                listOfToolProfiles.Add(new ProcessToolSet()
                {
                    KeyName = "toolSetMakeTextile",
                    Tools = new[] {                    
                        new ToolAlternatives() { Tools = new []
                        {                          
                            new Tool(){ UsesToolKeyName = "item:textileWorkshopUpgrade", ProductivityFactor = 0.7f, DegradePerSecondOfUse = NoDegrade  }                           
                        }           
                        }                   
                    }
                });
                #endregion

                #region toolSetCarpentry
                listOfToolProfiles.Add(new ProcessToolSet()
                {
                    KeyName = "toolSetCarpentry",
                    Tools = new[] {                    
                        new ToolAlternatives() { Tools = new []
                        {                          
                            new Tool(){ UsesToolKeyName = "item:carpenterWorkshopUpgrade", ProductivityFactor = 0.7f, DegradePerSecondOfUse = NoDegrade  }                           
                        }},
                        new ToolAlternatives() { Tools = new []
                        {                          
                            new Tool(){ UsesToolKeyName = "item:carpentersToolbox", ProductivityFactor = 0.7f, DegradePerSecondOfUse = NoDegrade  }                           
                        }}                   
                    }
                });
                #endregion

                #region toolSetMakeStew
                processToolSet = new ProcessToolSet()
                {
                    KeyName = "toolSetMakeStew",
                    Tools = new[] {                    
                        new ToolAlternatives() { Tools = new []
                        {
                            new Tool(){ UsesToolKeyName = "structure:campfire", ProductivityFactor = 0.4f, DegradePerSecondOfUse = NoDegrade  },//mp feb 2015 was 0.8f   
                            new Tool(){ UsesToolKeyName = "structure:fieldKitchen", ProductivityFactor = 1f, DegradePerSecondOfUse = NoDegrade  }, //mp feb 2015: was not added until now.
                            new Tool(){ UsesToolKeyName = "structure:improvisedKitchen", ProductivityFactor = 0.8f, DegradePerSecondOfUse = NoDegrade  },
                            new Tool(){ UsesToolKeyName = "structure:mudBrickKitchen", ProductivityFactor = 0.8f, DegradePerSecondOfUse = NoDegrade  },
                            new Tool(){ UsesToolKeyName = "item:simpleStoveUpgrade", ProductivityFactor = 1f, DegradePerSecondOfUse = NoDegrade  }
                        }           //fire AND pot needed
                        },
                          new ToolAlternatives() { Tools = new []
                        {   new Tool(){ UsesToolKeyName = "item:goldPot", ProductivityFactor = 0.8f, DegradePerSecondOfUse = NoDegrade  },
                            new Tool(){ UsesToolKeyName = "item:advancedCookingPot", ProductivityFactor = 0.8f, DegradePerSecondOfUse = NoDegrade  },//was NoDegrade   
                            new Tool(){ UsesToolKeyName = "item:improvisedCookingPot", ProductivityFactor = 0.6f, DegradePerSecondOfUse = NoDegrade  },
                            new Tool(){ UsesToolKeyName = "item:clayPotUnglazed", ProductivityFactor = 0.4f, DegradePerSecondOfUse = NoDegrade  },
                            new Tool(){ UsesToolKeyName = "item:woodenCookingPot", ProductivityFactor = 0.15f, DegradePerSecondOfUse = NoDegrade  }
                        }
                         
                        }
                    
                    }

                };
                listOfToolProfiles.Add(processToolSet);

                #endregion

                #region toolSetMakeBlendedFood
                processToolSet = new ProcessToolSet()
                {
                    KeyName = "toolSetMakeBlendedFood",//no heating needed, just a container 
                    Tools = new[] {                    
                        new ToolAlternatives() { Tools = new []
                        {   new Tool(){ UsesToolKeyName = "item:goldPot", ProductivityFactor = 0.3f, DegradePerSecondOfUse = NoDegrade  },
                            new Tool(){ UsesToolKeyName = "item:advancedCookingPot", ProductivityFactor = 0.3f, DegradePerSecondOfUse = NoDegrade  },                            
                            new Tool(){ UsesToolKeyName = "item:improvisedCookingPot", ProductivityFactor = 0.3f, DegradePerSecondOfUse = NoDegrade  },
                            new Tool(){ UsesToolKeyName = "item:clayPotUnglazed", ProductivityFactor = 0.3f, DegradePerSecondOfUse = NoDegrade  },
                            new Tool(){ UsesToolKeyName = "item:woodenCookingPot", ProductivityFactor = 0.3f, DegradePerSecondOfUse = NoDegrade  },
     
                        }
                         
                        }
                    
                    }

                };
                listOfToolProfiles.Add(processToolSet);
                #endregion

                #region toolSetDissolveInedibleMatter
                processToolSet = new ProcessToolSet()
                {
                    KeyName = "toolSetDissolveInedibleMatter",//no heating needed, just a big container preferably not used for food.. 
                    Tools = new[] {                    
                        new ToolAlternatives() { Tools = new []
                        {
                            new Tool(){ UsesToolKeyName = "item:vat", ProductivityFactor = 0.8f, DegradePerSecondOfUse = NoDegrade  },

                  //add pottery     
                        }                         
                        }                    
                    }
                };
                listOfToolProfiles.Add(processToolSet);

                #endregion

                #region toolSetShapePottery



                processToolSet = new ProcessToolSet()
                {
                    KeyName = "toolSetShapePottery",// add potter's wheel here
                    Tools = new[] {                    
                        new ToolAlternatives() { Tools = new []
                        {
                            new Tool(){ UsesToolKeyName = "item:bluntKnife", ProductivityFactor = 0.6f, DegradePerSecondOfUse = NoDegrade  }, //dull, wooden tool. 
                            new Tool(){ UsesToolKeyName = "item:improvisedKnife", ProductivityFactor = 0.45f, DegradePerSecondOfUse = NoDegrade  },
                            new Tool(){ UsesToolKeyName = "item:steelKnife", ProductivityFactor = 0.4f, DegradePerSecondOfUse = NoDegrade  },
                            new Tool(){ UsesToolKeyName = "item:advancedKnife", ProductivityFactor = 0.3f, DegradePerSecondOfUse = NoDegrade  },//too sharp. must be dull.
 
                        }                         
                        }                    
                    }
                };
                listOfToolProfiles.Add(processToolSet);
                #endregion

                #region toolSetMakeEnzyme
                processToolSet = new ProcessToolSet()
                {
                    KeyName = "toolSetMakeEnzyme",
                    Tools = new[] {                    
                        new ToolAlternatives() { Tools = new []
                        {
                            new Tool(){ UsesToolKeyName = "structure:fieldLab", ProductivityFactor = 1f, DegradePerSecondOfUse = NoDegrade}, 
                        } 
                        }
                    }
                };
                listOfToolProfiles.Add(processToolSet);

                #endregion

                
                #region Molecular assembly

                listOfToolProfiles.Add(new ProcessToolSet()
                {
                    KeyName = "toolSetUseAssemblerPlateA",
                    Tools = new[] {                    
                        new ToolAlternatives() { Tools = new []
                        {
                            new Tool(){ UsesToolKeyName = "structure:molecularAssembler", ProductivityFactor = 1f, DegradePerSecondOfUse = NoDegrade  } 
                           
                        }           
                        },
                        new ToolAlternatives() { Tools = new []
                        {
                            new Tool(){ UsesToolKeyName = "item:assemblerPlateA", ProductivityFactor = 1f, DegradePerSecondOfUse = NoDegrade  } 
                        }                         
                        }                    
                    }

                });

                listOfToolProfiles.Add(new ProcessToolSet()
                {
                    KeyName = "toolSetUseMasterAssemblerPlateA",
                    Tools = new[] {                    
                        new ToolAlternatives() { Tools = new []
                        {
                            new Tool(){ UsesToolKeyName = "structure:molecularAssembler", ProductivityFactor = 1f, DegradePerSecondOfUse = NoDegrade  } 
                           
                        }           
                        },
                        new ToolAlternatives() { Tools = new []
                        {
                            new Tool(){ UsesToolKeyName = "item:masterAssemblerPlateA", ProductivityFactor = 1f, DegradePerSecondOfUse = NoDegrade  } 
                        }                         
                        }                    
                    }

                });


                listOfToolProfiles.Add(new ProcessToolSet()
                {
                    KeyName = "toolSetUseAssemblerPlateB",
                    Tools = new[] {                    
                        new ToolAlternatives() { Tools = new []
                        {
                            new Tool(){ UsesToolKeyName = "structure:molecularAssembler", ProductivityFactor = 1f, DegradePerSecondOfUse = NoDegrade  } 
                           
                        }           
                        },
                        new ToolAlternatives() { Tools = new []
                        {
                            new Tool(){ UsesToolKeyName = "item:assemblerPlateB", ProductivityFactor = 1f, DegradePerSecondOfUse = NoDegrade  } 
                        }                         
                        }                    
                    }

                });

                listOfToolProfiles.Add(new ProcessToolSet()
                {
                    KeyName = "toolSetUseMasterAssemblerPlateB",
                    Tools = new[] {                    
                        new ToolAlternatives() { Tools = new []
                        {
                            new Tool(){ UsesToolKeyName = "structure:molecularAssembler", ProductivityFactor = 1f, DegradePerSecondOfUse = NoDegrade  } 
                           
                        }           
                        },
                        new ToolAlternatives() { Tools = new []
                        {
                            new Tool(){ UsesToolKeyName = "item:masterAssemblerPlateB", ProductivityFactor = 1f, DegradePerSecondOfUse = NoDegrade  } 
                        }                         
                        }                    
                    }

                });

                #endregion



                #region toolSetMixingBlackPowder
                processToolSet = new ProcessToolSet()
                {
                    KeyName = "toolSetMixingBlackPowder", //for black powder, means no metal!
                    Tools = new[]
                    {
                        new ToolAlternatives()
                        { 
                            Tools = new []
                            {
                                new Tool(){ UsesToolKeyName = "item:woodenCookingPot", ProductivityFactor = 0.8f, DegradePerSecondOfUse = NoDegrade},
                            new Tool(){ UsesToolKeyName = "item:clayPotUnglazed", ProductivityFactor = 0.8f, DegradePerSecondOfUse = NoDegrade  },
                                new Tool(){ UsesToolKeyName = "item:vat", ProductivityFactor = 0.6f, DegradePerSecondOfUse = NoDegrade  },
                            }
                        }
                    }
                };
                listOfToolProfiles.Add(processToolSet);
                #endregion

                
                #region cleanHide


                processToolSet = new ProcessToolSet()
                {
                    KeyName = "toolSetCleanHide",  //dull tool which doesnt cut through the hide accidentally. tanning agent. rack.
                    Tools = new[] 
                    {                    
                        new ToolAlternatives() 
                        { 
                            Tools = new []
                            {
                            new Tool(){ UsesToolKeyName = "item:flintKnife", ProductivityFactor = 0.15f, DegradePerSecondOfUse = NoDegrade  }, //has irregular edge, less precise.
                            new Tool(){ UsesToolKeyName = "item:improvisedKnife", ProductivityFactor = 0.3f, DegradePerSecondOfUse = NoDegrade  },
                            new Tool(){ UsesToolKeyName = "item:steelKnife", ProductivityFactor = 0.4f, DegradePerSecondOfUse = NoDegrade  },
                            new Tool(){ UsesToolKeyName = "item:advancedKnife", ProductivityFactor = 0.3f, DegradePerSecondOfUse = NoDegrade  },//too sharp. must be dull.
                            new Tool(){ UsesToolKeyName = "item:bluntKnife", ProductivityFactor = 0.8f, DegradePerSecondOfUse = NoDegrade  }, //dull, wooden tool is good
                            }
                        },                  
                        new ToolAlternatives() 
                        { 
                            Tools = new []
                            {
                            new Tool(){ UsesToolTag = "hideRack", ProductivityFactor = 1f, DegradePerSecondOfUse = NoDegrade  },
                            }
                        },
                        new ToolAlternatives() 
                        { 
                            Tools = new []
                            {
                            new Tool(){ UsesToolKeyName = "item:turnipBrain", ProductivityFactor = 1f, DegradePerSecondOfUse = NoDegrade  }, //
                            new Tool(){ UsesToolKeyName = "item:thunderChickenBrain", ProductivityFactor = 1f, DegradePerSecondOfUse = NoDegrade  },
                            new Tool(){ UsesToolKeyName = "item:megapodBrain", ProductivityFactor = 1f, DegradePerSecondOfUse = NoDegrade  },
                            new Tool(){ UsesToolKeyName = "item:whipjawBrain", ProductivityFactor = 1f, DegradePerSecondOfUse = NoDegrade  },
                            }
                        }
                    }
                };

                listOfToolProfiles.Add(processToolSet);
                #endregion

                #region Refine Rare Metal

                // NA MINING CAMP MATERIAL
                processToolSet = new ProcessToolSet()
                {
                    KeyName = "toolSetRefineRareMetal",  
                    Tools = new[] 
                    {                    
                        new ToolAlternatives() 
                        { 
                            Tools = new []
                            {
                            new Tool(){ UsesToolKeyName = "item:steelSpade", ProductivityFactor = 0.15f, DegradePerSecondOfUse = NoDegrade  }, //has irregular edge, less precise.
                            
                            }
                        },                  
                        new ToolAlternatives() 
                        { 
                            Tools = new []
                            {
                            new Tool(){ UsesToolTag = "rareMetalRefinery", ProductivityFactor = 1f, DegradePerSecondOfUse = NoDegrade  },
                            }
                        },
                        
                    }
                };

                listOfToolProfiles.Add(processToolSet);
                #endregion

                #region Digging Construction
                processToolSet = new ProcessToolSet()
                {

                    KeyName = "toolSetDiggingConstruction",
                    Tools = new[] 
                    {
                        new ToolAlternatives() 
                        { 
                            Tools = new [] //for measuring out the structure, and binding the frame together. copypasted from "toolSetCordage"
                            {
                                new Tool(){ UsesToolKeyName = "item:advancedString", ProductivityFactor = 0.55f, DegradePerSecondOfUse = NoDegrade  },                              //was 1f
                                new Tool(){ UsesToolKeyName = "item:superconductingWire", ProductivityFactor = 0.4f, DegradePerSecondOfUse = NoDegrade  },                     //was 0.4f
                                new Tool(){ UsesToolKeyName = "item:metalWire", ProductivityFactor = 0.5f, DegradePerSecondOfUse = NoDegrade  },                            //was 0.4f
                                new Tool(){ UsesToolKeyName = "item:vine", ProductivityFactor = 0.45f, DegradePerSecondOfUse = NoDegrade  },                                  //was 0.2f
                                new Tool(){ UsesToolKeyName = "item:rawhideString", ProductivityFactor = 0.4f, DegradePerSecondOfUse = NoDegrade  } ,                       //was 0.15f   
                                new Tool(){ UsesToolKeyName = "item:cottonString", ProductivityFactor = 0.45f, DegradePerSecondOfUse = NoDegrade  }           
                            }
                        },
                        new ToolAlternatives() 
                        { 
                            Tools = new [] //mp copypasted from "diggingSoil" (Except hoe and trowel) for digging foundation out  
                            {
                                new Tool(){ UsesToolKeyName = "item:improvisedPickaxe", ProductivityFactor = 0.28f, DegradePerSecondOfUse = NoDegrade  },
                                new Tool(){ UsesToolKeyName = "item:steelPickaxe", ProductivityFactor = 0.33f, DegradePerSecondOfUse = NoDegrade  },                            
                            new Tool(){ UsesToolKeyName = "item:improvisedSpade", ProductivityFactor = 0.75f, DegradePerSecondOfUse = NoDegrade  },
                            new Tool(){ UsesToolKeyName = "item:steelSpade", ProductivityFactor = 0.9f, DegradePerSecondOfUse = NoDegrade  },
                            }
                        },
                        //mp thought about also having a 3rd tool improvisedTrowel  for digging  small hole and smearing plaster but then how would anim look and also maybe too much.
                    }
                };
                listOfToolProfiles.Add(processToolSet);
                #endregion

                #region Small Digging Construction 
                processToolSet = new ProcessToolSet()
                {

                    KeyName = "toolSetSmallDiggingConstruction",
                    Tools = new[] 
                    {
                        new ToolAlternatives() 
                        { 
                            Tools = new [] //for measuring out the structure, and binding the frame together. copypasted from "toolSetCordage"
                            {
                                new Tool(){ UsesToolKeyName = "item:advancedString", ProductivityFactor = 0.55f, DegradePerSecondOfUse = NoDegrade  },                              //was 1f
                                new Tool(){ UsesToolKeyName = "item:superconductingWire", ProductivityFactor = 0.4f, DegradePerSecondOfUse = NoDegrade  },                     //was 0.4f
                                new Tool(){ UsesToolKeyName = "item:metalWire", ProductivityFactor = 0.5f, DegradePerSecondOfUse = NoDegrade  },                            //was 0.4f
                                new Tool(){ UsesToolKeyName = "item:vine", ProductivityFactor = 0.45f, DegradePerSecondOfUse = NoDegrade  },                                  //was 0.2f
                                new Tool(){ UsesToolKeyName = "item:rawhideString", ProductivityFactor = 0.4f, DegradePerSecondOfUse = NoDegrade  } ,                       //was 0.15f   
                                new Tool(){ UsesToolKeyName = "item:cottonString", ProductivityFactor = 0.45f, DegradePerSecondOfUse = NoDegrade  }           
                            }
                        },
                        new ToolAlternatives() 
                        { 
                            Tools = new [] //mp uses a compact trowel for digging a small hole and smearing plaster
                            {               // LPE: it's a problem that a trowel is needed to maintian the pit, but it can be constructed with a spade
                            new Tool(){ UsesToolKeyName = "item:improvisedTrowel", ProductivityFactor = 0.4f, DegradePerSecondOfUse = NoDegrade  },                                      

                            }
                        }
                    }
                };
                listOfToolProfiles.Add(processToolSet);
                #endregion



                #region DiggingSoil
                processToolSet = new ProcessToolSet()
                {
                    Comments = "digging holes, collecting grass sod, guano etc.  A tool is needed to loosen the soil, then it can either be moved with hands or with a shovel. A spade can do both.",
                    KeyName = "toolSetDiggingSoil",
                    Tools = new[] {                    
                        new ToolAlternatives() { Tools = new []
                        {
                            new Tool(){ UsesToolKeyName = "item:improvisedTrowel", ProductivityFactor = 0.15f, DegradePerSecondOfUse = NoDegrade  },
                            new Tool(){ UsesToolKeyName = "item:improvisedPickaxe", ProductivityFactor = 0.28f, DegradePerSecondOfUse = NoDegrade  },
                            new Tool(){ UsesToolKeyName = "item:steelPickaxe", ProductivityFactor = 0.33f, DegradePerSecondOfUse = NoDegrade  }, 
                            new Tool(){ UsesToolKeyName = "item:steelHoe", ProductivityFactor = 0.3f, DegradePerSecondOfUse = NoDegrade },
                            new Tool(){ UsesToolKeyName = "item:farmingHoe", ProductivityFactor = 0.27f, DegradePerSecondOfUse = NoDegrade },                           
                            new Tool(){ UsesToolKeyName = "item:improvisedSpade", ProductivityFactor = 0.75f, DegradePerSecondOfUse = NoDegrade  },
                            new Tool(){ UsesToolKeyName = "item:steelSpade", ProductivityFactor = 0.9f, DegradePerSecondOfUse = NoDegrade  },
                        }                         
                        }                    
                    }
                };
                listOfToolProfiles.Add(processToolSet);
                #endregion

                #region DiggingClay
                processToolSet = new ProcessToolSet()
                {
                    KeyName = "toolSetDiggingClay",
                    Tools = new[] {                    
                        new ToolAlternatives() { Tools = new []
                        {
                            new Tool(){ UsesToolKeyName = "item:improvisedTrowel", ProductivityFactor = 0.15f, DegradePerSecondOfUse = NoDegrade  },                                                    
                            new Tool(){ UsesToolKeyName = "item:farmingHoe", ProductivityFactor = 0.27f, DegradePerSecondOfUse = NoDegrade },                           
                            new Tool(){ UsesToolKeyName = "item:steelHoe", ProductivityFactor = 0.3f, DegradePerSecondOfUse = NoDegrade },
                            new Tool(){ UsesToolKeyName = "item:improvisedPickaxe", ProductivityFactor = 0.4f, DegradePerSecondOfUse = NoDegrade  },
                            new Tool(){ UsesToolKeyName = "item:improvisedSpade", ProductivityFactor = 0.5f, DegradePerSecondOfUse = NoDegrade  },                            
                            new Tool(){ UsesToolKeyName = "item:steelPickaxe", ProductivityFactor = 0.6f, DegradePerSecondOfUse = NoDegrade  },                             
                            new Tool(){ UsesToolKeyName = "item:steelSpade", ProductivityFactor = 0.9f, DegradePerSecondOfUse = NoDegrade  }     
                        }                         
                        }                    
                    }
                };
                listOfToolProfiles.Add(processToolSet);
                #endregion

                #region DiggingSalt
                processToolSet = new ProcessToolSet()
                {
                    KeyName = "toolSetDiggingSalt",
                    Tools = new[] {                    
                        new ToolAlternatives() { Tools = new []
                        {
                            new Tool(){ UsesToolKeyName = "item:improvisedTrowel", ProductivityFactor = 0.15f, DegradePerSecondOfUse = NoDegrade  },                            
                            new Tool(){ UsesToolKeyName = "item:farmingHoe", ProductivityFactor = 0.27f, DegradePerSecondOfUse = NoDegrade },                               
                            new Tool(){ UsesToolKeyName = "item:steelHoe", ProductivityFactor = 0.3f, DegradePerSecondOfUse = NoDegrade },                            
                            new Tool(){ UsesToolKeyName = "item:improvisedSpade", ProductivityFactor = 0.5f, DegradePerSecondOfUse = NoDegrade  },
                            new Tool(){ UsesToolKeyName = "item:improvisedPickaxe", ProductivityFactor = 0.55f, DegradePerSecondOfUse = NoDegrade  },                        
                            new Tool(){ UsesToolKeyName = "item:steelSpade", ProductivityFactor = 0.8f, DegradePerSecondOfUse = NoDegrade  },                     
                            new Tool(){ UsesToolKeyName = "item:steelPickaxe", ProductivityFactor = 0.9f, DegradePerSecondOfUse = NoDegrade  }                          
                        }                         
                        }                    
                    }
                };
                listOfToolProfiles.Add(processToolSet);
                #endregion
               
                #region greenhouseHarvest
                processToolSet = new ProcessToolSet()
                {
                    KeyName = "toolSetGreenhouseHarvest",
                    Tools = new[] 
                    {                    
                        harvestGreenhouseTools
                    }

                };
                listOfToolProfiles.Add(processToolSet);
                #endregion

                return listOfToolProfiles;       
        }
    }
}
