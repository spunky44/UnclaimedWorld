/*
 * AnimatedModelProcessor.cs
 * Copyright (c) 2006 David Astle
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a
 * copy of this software and associated documentation files (the
 * "Software"), to deal in the Software without restriction, including
 * without limitation the rights to use, copy, modify, merge, publish,
 * distribute, sublicense, and/or sell copies of the Software, and to
 * permit persons to whom the Software is furnished to do so, subject to
 * the following conditions:
 *
 * The above copyright notice and this permission notice shall be included
 * in all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS
 * OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
 * MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
 * IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY
 * CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
 * TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
 * SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using System.IO;
using System.Collections;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
using Microsoft.Xna.Framework.Content;
using System.Globalization;
using System.Xml;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace Xclna.Xna.Animation.Content
{
    /// <summary>
    /// Processes a NodeContent object that was imported by SkinnedModelImporter
    /// and attaches animation data to its tag
    /// </summary>
    [ContentProcessor(DisplayName="Model - Animation Library")]
    public class AnimatedModelProcessor : ModelProcessor
    {

        private ContentProcessorContext context;
        
        // stores all animations for the model
        private AnimationContentDictionary animations = new AnimationContentDictionary();
        private NodeContent input;
        private SkinInfoContentCollection[] skinInfo = null;
        private bool modelSplit = false;
        private BoneIndexer[] indexers = null;
        List<Matrix> absoluteMeshTransforms = null;
        List<MeshContent> meshes = new List<MeshContent>();

        List<Vector3> vertices = new List<Vector3>();

        /// <summary>
        /// 'hidden' -> 'Models/thunderChicken_hidden.fbx'
        /// </summary>
        Dictionary<string, string> mergedAnimations;

        private string modelName;

        private int numMeshes = 0;

        /// <summary>
        /// Provide a list of fbx'es here, separated by semicolon. Is folder/extension agnostic.
        /// </summary>        
        public string MergeAnimations { get; set; }

        /// <summary>
        /// when changing this, change sampleDistance in the Runtime project as well!
        /// </summary>
        const long sampleDistance = ContentUtil.TICKS_PER_30FPS; // ContentUtil.TICKS_PER_60FPS;

        /// <summary>Processes a SkinnedModelImporter NodeContent root</summary>
        /// <param name="input">The root of the X file tree</param>
        /// <param name="context">The context for this processor</param>
        /// <returns>A model with animation data on its tag</returns>
        public override ModelContent Process(NodeContent input, ContentProcessorContext context)
        {
            ModelSplitter splitter;
            
            if (context.TargetPlatform != TargetPlatform.Xbox360)
            {
                splitter = new ModelSplitter(input, 56);
            }
            else
            {
                splitter = new ModelSplitter(input, 40);
            }

            modelSplit = splitter.Split();
            splitter = null;
            this.input = input;
            this.context = context;

            FindMeshes(input);

            indexers = new BoneIndexer[numMeshes];
            for (int i = 0; i < indexers.Length; i++)
            {
                indexers[i] = new BoneIndexer();
            }

            foreach (MeshContent meshContent in meshes)
                CreatePaletteIndices(meshContent);

            // Get the process model minus the animation data
            ModelContent c = base.Process(input, context);
                       
            BoneContent rootBone = null;

            try
            {
                // this call fails for utility vehicle (skimmer also?) because it has more than one skeleton!
                rootBone = MeshHelper.FindSkeleton(input);
            }
            catch (Exception e)
            {
                context.Logger.LogWarning(null, input.Identity, e.Message);
            }

            if (rootBone == null)
            {
                context.Logger.LogWarning(null, input.Identity, "Model has no root bone.");
                //return;
            }
            else
            {
                context.Logger.LogWarning(null, input.Identity, "Root bone found: " + rootBone.Name);
            }

            modelName = GetModelNameFromFileName(input.Identity.SourceFilename);

            // rename the animation from 'Take 001' (Maya generated name) to the name defined by the filename convention that we use:
            RenameStartingPointAnimation(rootBone);
                        

            // NEW: Merge animations...
            // ***************************************
            DoMergeAnimations(context, rootBone);
            // ***************************


            if (!modelSplit && input.OpaqueData.ContainsKey("AbsoluteMeshTransforms"))
            {
                absoluteMeshTransforms =
                    (List<Matrix>)input.OpaqueData["AbsoluteMeshTransforms"];
            }
            else
            {

                foreach (MeshContent mesh in meshes)
                {
                    if (!ValidateMeshSkeleton(mesh))
                    {
                        context.Logger.LogWarning(null, mesh.Identity, "Warning: Mesh found that has a parent that exists as "
                            + "one of the bones in the skeleton attached to the mesh.  Change the mesh "
                            + "skeleton structure or use X - File Animation Library importer if transforms are incorrect.");
                    }
                }

            }

            Dictionary<string, object> modelContentTagData = new Dictionary<string, object>();


           // context.Logger.LogImportantMessage("FindAnimations");

            // Attach the animation and skinning data to the models tag
            FindAnimations(input);

          /*  foreach (KeyValuePair<string, AnimationContent> anim in animations)
            {
                context.Logger.LogImportantMessage("Animation found, key: " + anim.Key + ", name: " + anim.Value.Name);
            }*/

            // Test to see if any animations have zero duration
            foreach (AnimationContent anim in animations.Values)
            {
               // context.Logger.LogImportantMessage("Animation found: " + anim.Name);

                string errorMsg = "One or more AnimationContent objects have an extremely small duration.  If the animation "
                        + "was intended to last more than one frame, please add \n AnimTicksPerSecond \n{0} \nY; \n{1}\n to your .X "
                        + "file, where Y is a positive integer.";

                if (anim.Duration.Ticks < sampleDistance)
                {
                    context.Logger.LogWarning("", anim.Identity, errorMsg, "{", "}");

                    break;
                }
            }

              if (string.IsNullOrEmpty(MergeAnimations))
              {
                  // only read the subdivision xml file if we are not merging fbxes:
                  XmlDocument xmlDoc = ReadAnimationXML(input);

                  if (xmlDoc != null)
                  {
                      SubdivideAnimations(animations, xmlDoc);
                  }
              }

            try
            {
                foreach (KeyValuePair<string, AnimationContent> animKey in animations)
                {                  
                    HandleExtraAnimationInfo(input, animKey);
                }
            }
            catch
            {
                throw new Exception("Error processing animations when handling xml files.");
            }


            AnimationContentDictionary processedAnims = new AnimationContentDictionary();
          //  ExtendedAnimationContentDictionary processedAnims = new ExtendedAnimationContentDictionary();

            int count = 0;
         /*   try
            {*/
                foreach (KeyValuePair<string, AnimationContent> animKey in animations)
                {
                    float? speedFactor = null;
                    if (animKey.Value.OpaqueData.ContainsKey("Speed"))
                    {
                        var speed = animKey.Value.OpaqueData["Speed"];
                        if (speed != null)
                        {
                            speedFactor = (float)speed;
                        }
                    }

                    bool okToResample = true;
                    if (animKey.Value.OpaqueData.ContainsKey("OKToResample"))
                    {
                        var okToResampleData = animKey.Value.OpaqueData["OKToResample"];
                        if (okToResampleData != null)
                        {
                            okToResample = (bool)okToResampleData;
                        }
                    }

                    bool okToCompress = true;
                    if (animKey.Value.OpaqueData.ContainsKey("OKToCompress"))
                    {
                        var okToCompressData = animKey.Value.OpaqueData["OKToCompress"];
                        if (okToCompressData != null)
                        {
                            okToCompress = (bool)okToCompressData;
                        }
                    }
                    

                    long durationOfFirstKeyframe;
                    AnimationContent processedAnim = ProcessAnimation(animKey.Value, out durationOfFirstKeyframe, okToResample, okToCompress, speedFactor);

                    if (!processedAnim.OpaqueData.ContainsKey("StartOffset"))
                    {
                        context.Logger.LogImportantMessage("Saving default value for StartOffset {0} for anim {1}", durationOfFirstKeyframe.ToString(), animKey.Key);

                        processedAnim.OpaqueData.Add("StartOffset", (long)(1.5f * durationOfFirstKeyframe));
                    }

                    count++;

                    processedAnims.Add(animKey.Key, processedAnim);                  
                }

                modelContentTagData.Add("Animations", processedAnims);
           /* }
            catch
            {
                throw new Exception("Error processing animations.");
            }*/

            foreach (ModelMeshContent meshContent in c.Meshes)
            {
                ReplaceBasicEffects(meshContent);
            }

            skinInfo = ProcessSkinInfo(c);

            context.Logger.LogImportantMessage("Adding SkinInfo: {0}", skinInfo.ToString());

            modelContentTagData.Add("SkinInfo", skinInfo);

            // From Triangle Picking Sample:
            // Look up the input vertex positions.
            FindVertices(input);

            context.Logger.LogImportantMessage("Adding Vertices - count: {0}", vertices.Count);
            // Store vertex information in the tag data, as an array of Vector3.
            modelContentTagData.Add("Vertices", vertices.ToArray());

            // Also store a custom bounding sphere.
            modelContentTagData.Add("BoundingSphere", BoundingSphere.CreateFromPoints(vertices));

            foreach (KeyValuePair<string, AnimationContent> animKey in processedAnims)
            {
                foreach (object o in animKey.Value.OpaqueData)
                {
                    context.Logger.LogImportantMessage("OpaqueData {0} for anim {1}", o.ToString(), animKey.Key);
                }
            }

            
            c.Tag = modelContentTagData;
            return c;
        }

        private void HandleExtraAnimationInfo(NodeContent input, KeyValuePair<string, AnimationContent> animKey)
        {
            List<string> channelsToRemove = null;
            List<string> channelsToKeep = null;

            float? actionPoint = null;
            float? speed = null;
            long? startoffset = null;
            bool? okToBlend = null;
            bool? hideHandAttachments = null;
            bool? isGait = null;
            bool? okToCompress = null;
            bool? okToResample = null;

            XmlDocument xmlDoc = ReadAnimationXML(input, animKey.Key);
            if (xmlDoc != null)
            {  
                ExtractAnimationInfo(xmlDoc, ref channelsToRemove, ref channelsToKeep, ref actionPoint, ref speed, ref startoffset, ref okToBlend,
                    ref hideHandAttachments, ref isGait, ref okToCompress, ref okToResample);
            }

            RemoveSpecifiedBoneChannelsFromAnimation(animKey, channelsToRemove, channelsToKeep);

            if (actionPoint.HasValue)
            {
                animKey.Value.OpaqueData.Add("ActionPoint", actionPoint.Value);
            }

            if (speed.HasValue)
            {
                animKey.Value.OpaqueData.Add("Speed", speed.Value);            
              //  context.Logger.LogImportantMessage("Speed: {0}", speed.Value.ToString());
            }

            if (startoffset.HasValue)
            {
                animKey.Value.OpaqueData.Add("StartOffset", startoffset.Value);
            }
           // animKey.Value.OpaqueData.Add("Speed", speed.HasValue ? speed.Value.ToString() : "");                         

            if (okToBlend.HasValue)
            {
                animKey.Value.OpaqueData.Add("OKToBlendAnim", okToBlend.Value);
            }

            if (hideHandAttachments.HasValue)
            {
                animKey.Value.OpaqueData.Add("HideHandAttachments", hideHandAttachments.Value);
            }

            if (isGait.HasValue)
            {
                animKey.Value.OpaqueData.Add("IsGait", isGait.Value);
            }

            if (okToCompress.HasValue)
            {
                animKey.Value.OpaqueData.Add("OKToCompress", okToCompress.Value);
            }

            if (okToResample.HasValue)
            {
                animKey.Value.OpaqueData.Add("OKToResample", okToResample.Value);
            }
        }

        private void RemoveSpecifiedBoneChannelsFromAnimation(KeyValuePair<string, AnimationContent> animKey, List<string> channelsToRemove, List<string> channelsToKeep)
        {
            if (channelsToRemove != null)
            {
                foreach (string key in channelsToRemove)
                {
                    context.Logger.LogImportantMessage("Removing channel (from excludebones): {0}", key);
                    if (animKey.Value.Channels.ContainsKey(key))
                    {
                        context.Logger.LogImportantMessage("Key found: {0}", key); 
                    }
                    animKey.Value.Channels.Remove(key);
                }
            }
            else if (channelsToKeep != null)
            {
                List<string> channelsNotToKeep = new List<string>();
                foreach (KeyValuePair<string, AnimationChannel> kvp in animKey.Value.Channels)
                {
                    if (!channelsToKeep.Contains(kvp.Key))
                    {
                        channelsNotToKeep.Add(kvp.Key);
                    }
                }

                foreach (string key in channelsNotToKeep)
                {
                    context.Logger.LogImportantMessage("Removing channel (from includebones): {0}", key);

                    animKey.Value.Channels.Remove(key);
                }
            }
           // return animKey;
        }


        private void ExtractAnimationInfo(XmlDocument xmlDoc, ref List<string> channelsToRemove, ref List<string> channelsToKeep,
            ref float? actionPoint, ref float? speed, ref long? startOffset, ref bool? okToBlendAnim, ref bool? hidehandattachments, ref bool? isGait, ref bool? okToCompress, ref bool? okToResample)
        {
            XmlNode root = xmlDoc.ChildNodes[0];
            
           

            foreach (XmlNode node in root) // xmlDoc)
            {
                if (node.Name == "excludebones")
                {
                    channelsToRemove = new List<string>();

                    foreach (XmlNode nameNode in node)
                    {
                        XmlElement child = nameNode as XmlElement;
                        if (child == null || child.Name != "name")
                            continue;

                        channelsToRemove.Add(child.InnerText);
                    }

                }
                else if (node.Name == "includebones")
                {
                    channelsToKeep = new List<string>();

                    foreach (XmlNode nameNode in node)
                    {
                        XmlElement child = nameNode as XmlElement;
                        if (child == null || child.Name != "name")
                            continue;

                        channelsToKeep.Add(child.InnerText);
                    }
                }

                if (node.Name == "actionpoint")
                {
                    actionPoint = float.Parse(node.InnerText, NumberStyles.Float, new CultureInfo("en-US")); // float.Parse(node.InnerText);

                    context.Logger.LogImportantMessage("Action point found: {0}, {1}", node.InnerText, actionPoint.Value.ToString("G"));
                }

               

                if (node.Name == "speed")
                {
                    speed = float.Parse(node.InnerText, NumberStyles.Float, new CultureInfo("en-US"));

                    context.Logger.LogImportantMessage("Speed found: {0}, parsed as: {1}", node.InnerText, speed.Value.ToString());

                    // let's modifty the action point by the speed modifier:
                    if (actionPoint.HasValue)
                    {
                        actionPoint /= speed;
                    }
                }

                if (node.Name.ToLowerInvariant() == "startoffset")
                {
                    startOffset = long.Parse(node.InnerText, NumberStyles.Float, new CultureInfo("en-US"));

                    context.Logger.LogImportantMessage("Start offset found: {0}", node.InnerText);

                    // let's modify the action point by the speed modifier:
                  /*  if (actionPoint.HasValue)
                    {
                        actionPoint /= speed;
                    }*/
                }

                if (node.Name.ToLowerInvariant() == "oktoblend")
                {
                    okToBlendAnim = bool.Parse(node.InnerText);

                    context.Logger.LogImportantMessage("OK to blend anim: {0}", node.InnerText);
                }

                if (node.Name.ToLowerInvariant() == "hidehandattachments")
                {
                    hidehandattachments = bool.Parse(node.InnerText);

                    context.Logger.LogImportantMessage("Hide Hand Attachments: {0}", node.InnerText);
                }

                if (node.Name.ToLowerInvariant() == "isgait")
                {
                    isGait = bool.Parse(node.InnerText);

                    context.Logger.LogImportantMessage("Is gait: {0}", node.InnerText);
                }

                if (node.Name == "resample")
                {
                    okToResample = bool.Parse(node.InnerText);

                    context.Logger.LogImportantMessage("OK to resample anim: {0}", node.InnerText);
                }

                if (node.Name == "compress")
                {
                    okToCompress = bool.Parse(node.InnerText);

                    context.Logger.LogImportantMessage("OK to compress anim: {0}", node.InnerText);
                }
            }


            /*
            foreach (XmlNode node in xmlDoc)
            {
                XmlElement child = node as XmlElement;
                if (child == null || child.Name != "animation")
                    continue;

                string animName = null;
                if (child["name"] != null)
                {
                    // The name of the animation to be split
                    animName = child["name"].InnerText;
                }
                else if (child["index"] != null)
                {
                    animName = animNames[int.Parse(child["index"].InnerText)];
                }
                else
                {
                    animName = animNames[0];
                }

                // If the tickspersecond node is filled, use that to calculate seconds per tick
                double animTicksPerSecond = 1.0, secondsPerTick = 0;
                try
                {
                    if (child["tickspersecond"] != null)
                    {
                        animTicksPerSecond = double.Parse(child["tickspersecond"].InnerText);
                    }
                }
                catch
                {
                    throw new Exception("Error parsing tickspersecond in xml file.");
                }

                if (animTicksPerSecond <= 0)
                    throw new InvalidDataException("AnimTicksPerSecond in XML file must be " +
                        "a positive number.");

                secondsPerTick = 1.0 / animTicksPerSecond;
            }*/
        }

        private void RenameStartingPointAnimation(BoneContent rootBone)
        {
            if (rootBone != null && rootBone.Animations.Count > 0)
            {
                
                string fileNameWithoutExtension = System.IO.Path.GetFileNameWithoutExtension(input.Identity.SourceFilename);
                string animationNameToUse = GetAnimationNameFromFileName(fileNameWithoutExtension);

                context.Logger.LogImportantMessage("Rename animation to Name: {0} ({1})", animationNameToUse, input.Identity.SourceFilename);

                
                string key = rootBone.Animations.ElementAt(0).Key; 
                AnimationContent anim = rootBone.Animations[key]; //[rootBone.Animations.Keys[0]];
                
                rootBone.Animations.Remove(key);

                anim.Name = animationNameToUse;

               // rootBone.Animations.rem
                rootBone.Animations.Add(animationNameToUse, anim); 
            }
        }

        private void DoMergeAnimations(ContentProcessorContext context, BoneContent rootBone)
        {
            mergedAnimations = new Dictionary<string, string>();

            if (rootBone != null && !string.IsNullOrEmpty(MergeAnimations))
            {
                foreach (string mergeFile in MergeAnimations.Split(';')
                                                            .Select(s => s.Trim())
                                                            .Where(s => !string.IsNullOrEmpty(s)))
                {
                    string filePath = ComposeFilePathFromAnimationName(mergeFile, ".fbx");

                    MergeAnimation(context, filePath, rootBone);
                }
            }
        }

        private static string ComposeFilePathFromAnimationName(string mergeFile, string extensionToUse)
        {
            string filePath = mergeFile;
            if (string.IsNullOrEmpty(Path.GetExtension(filePath)))
            {
                filePath += extensionToUse; 
            }

            if (string.IsNullOrEmpty(Path.GetDirectoryName(filePath)))
            {
                filePath = "Models/" + filePath;
            }
            return filePath;
        }


        void MergeAnimation(ContentProcessorContext context, string mergeFile, BoneContent rootBone)
        {
            NodeContent mergeModel = context.BuildAndLoadAsset<NodeContent, NodeContent>(
                                                new ExternalReference<NodeContent>(mergeFile), null);

            /*BoneContent rootBone = MeshHelper.FindSkeleton(input);

            if (rootBone == null)
            {
                context.Logger.LogWarning(null, input.Identity, "Source model has no root bone.");
                return;
            }
            else
            {
                context.Logger.LogWarning(null, input.Identity, "Root bone found: " + rootBone.Name);
            }*/

            BoneContent mergeRoot = MeshHelper.FindSkeleton(mergeModel);

            if (mergeRoot == null)
            {
                context.Logger.LogWarning(null, input.Identity, "Merge model '{0}' has no root bone.", mergeFile);
                return;
            }
            else
            {
                context.Logger.LogMessage("Merge model root bone found: " + mergeRoot.Name);
            }

            
            string fileNameWithoutExtension = System.IO.Path.GetFileNameWithoutExtension(mergeFile);

            string animationNameToUse = GetAnimationNameFromFileName(fileNameWithoutExtension);

            // there should only be 1 animation in each file, so this loop will only execute once.          
          //  foreach (string animationName in mergeRoot.Animations.Keys)
            foreach(KeyValuePair<string, AnimationContent> kvp in mergeRoot.Animations)
            {
                //context.Logger.LogImportantMessage("Merge looks at animation '{0}', '{1}', '{2}'.", animationName, animationNameToUse, mergeFile);
                //context.Logger.LogImportantMessage("Merge looks at animation '{0}', '{1}', '{2}'.", kvp.Key, animationNameToUse, mergeFile);

                if (rootBone.Animations.ContainsKey(animationNameToUse))
                {
                    context.Logger.LogWarning(null, input.Identity,
                        "Cannot merge animation '{0}' from '{1}', because this animation already exists.",
                        animationNameToUse, mergeFile);

                    continue;
                }

               /* if (rootBone.Animations.ContainsKey(animationName))
                {
                    context.Logger.LogWarning(null, input.Identity,
                        "Cannot merge animation '{0}' ('{1}') from '{2}', because this animation already exists.",
                        animationName, animationNameToUse, mergeFile);

                    continue;
                }*/

                // Merging animation 'hidden' from 'Models/thunderChicken_hidden.fbx'
                context.Logger.LogImportantMessage("Merging animation '{0}' from '{1}'.", animationNameToUse, mergeFile);

                mergedAnimations.Add(animationNameToUse, mergeFile);

                kvp.Value.Name = animationNameToUse;

                rootBone.Animations.Add(animationNameToUse, kvp.Value); // mergeRoot.Animations[animationName]);
            }
        }
         
        private static string GetAnimationNameFromFileName(string fileNameWithoutExtension)
        {
            int indexOfUnderscore = fileNameWithoutExtension.IndexOf('_');
            string animationNameToUse = fileNameWithoutExtension.Substring(indexOfUnderscore + 1, fileNameWithoutExtension.Length - (indexOfUnderscore + 1));
            return animationNameToUse;
        }

        private static string GetModelNameFromFileName(string fullPath)
        {
            string fileNameWithoutExtension = System.IO.Path.GetFileNameWithoutExtension(fullPath);
            if (fileNameWithoutExtension.Contains("_"))
            {
                int indexOfUnderscore = fileNameWithoutExtension.IndexOf('_');
                string modelNameToUse = fileNameWithoutExtension.Substring(0, indexOfUnderscore);

                return modelNameToUse;
            }
            else return fileNameWithoutExtension;                      

        }

        /// <summary>
        /// Helper for extracting a list of all the vertex positions in a model.
        /// </summary>
        private void FindVertices(NodeContent node)
        {
            
            // Is this node a mesh?
            MeshContent mesh = node as MeshContent;

            if (mesh != null)
            {
                // Look up the absolute transform of the mesh.
                Matrix absoluteTransform = mesh.AbsoluteTransform;

                // Loop over all the pieces of geometry in the mesh.
                foreach (GeometryContent geometry in mesh.Geometry)
                {
                    // Loop over all the indices in this piece of geometry.
                    // Every group of three indices represents one triangle.
                    foreach (int index in geometry.Indices)
                    {
                        // Look up the position of this vertex.
                        Vector3 vertex = geometry.Vertices.Positions[index];

                        // Transform from local into world space.
                        vertex = Vector3.Transform(vertex, absoluteTransform);

                        // Store this vertex.
                        vertices.Add(vertex);
                    }
                }
            }

            // Recursively scan over the children of this node.
            foreach (NodeContent child in node.Children)
            {
                FindVertices(child);
            }

        }

        private void FindMeshes (NodeContent root)
        {
            if (root is MeshContent)
            {
                MeshContent mesh = (MeshContent)root;
                mesh.OpaqueData.Add("MeshIndex", numMeshes);
                numMeshes++;
                meshes.Add(mesh);
            }
            foreach (NodeContent child in root.Children)
                FindMeshes(child);
        }

        private void CreatePaletteIndices(MeshContent mesh)
        {
            foreach (GeometryContent meshPart in mesh.Geometry)
            {
                int meshIndex = (int)mesh.OpaqueData["MeshIndex"];
                BoneIndexer indexer = indexers[meshIndex];
                foreach (VertexChannel channel in meshPart.Vertices.Channels)
                {
                    if (channel.Name == VertexChannelNames.Weights())
                    {
                        VertexChannel<BoneWeightCollection> vc =
                            (VertexChannel<BoneWeightCollection>)channel;
                        foreach (BoneWeightCollection boneWeights in vc)
                        {
                            foreach (BoneWeight weight in boneWeights)
                            {
                                indexer.GetBoneIndex(weight.BoneName);
                            }
                        }
                    }
                }
            }
        }


        // returns true if the model contains meshes that have a parent bone as a child of 
        // a bone in the skeleton attached to the mesh.
        private bool ValidateMeshSkeleton (MeshContent meshContent)
        {
            List<string> meshParentHierarchy = new List<string>();
            int meshIndex = (int)meshContent.OpaqueData["MeshIndex"];
            BoneIndexer indexer = indexers[meshIndex];
            if (indexer.SkinnedBoneNames.Contains(meshContent.Parent.Name))
            {
                // Warning
                return false;
            }
            // skeleton is fine
            return true;

        }


        private void CalculateAbsoluteTransforms(ModelBoneContent bone, Matrix[] transforms)
        {
            if (bone.Parent == null)
                transforms[bone.Index] = bone.Transform;
            else
            {
                transforms[bone.Index] = bone.Transform * transforms[bone.Parent.Index];
            }
            foreach (ModelBoneContent child in bone.Children)
                CalculateAbsoluteTransforms(child, transforms);
        }



        private SkinInfoContentCollection[] ProcessSkinInfo(ModelContent model)
        {
            SkinInfoContentCollection[] info = new SkinInfoContentCollection[model.Meshes.Count];
            Dictionary<string, int> boneDict = new Dictionary<string,int>();
            foreach (ModelBoneContent b in model.Bones)
            {
                if (b.Name != null && !boneDict.ContainsKey(b.Name))
                    boneDict.Add(b.Name, b.Index);
            }

            for (int i = 0; i < info.Length; i++)
            {
                info[i] = new SkinInfoContentCollection();
                BoneIndexer indexer = indexers[i];
                ReadOnlyCollection<string> skinnedBoneNames = indexer.SkinnedBoneNames;

                Matrix[] absoluteTransforms = new Matrix[model.Bones.Count];
                CalculateAbsoluteTransforms(model.Bones[0], absoluteTransforms);

                Matrix absoluteMeshTransform;
                if (absoluteMeshTransforms == null)
                {
                    absoluteMeshTransform = absoluteTransforms[model.Meshes[i].ParentBone.Index];
                }
                else
                {
                    absoluteMeshTransform = absoluteMeshTransforms[i];
                }

                for (int j = 0; j < skinnedBoneNames.Count; j++)
                {
                    string name = skinnedBoneNames[j];
                    SkinInfoContent content = new SkinInfoContent();
                    content.BoneIndex = boneDict[name];
                    content.PaletteIndex = indexer.GetBoneIndex(name);
                    content.InverseBindPoseTransform = absoluteMeshTransform *
                        Matrix.Invert(absoluteTransforms[boneDict[name]]);
                    content.BoneName = name;
                    info[i].Add(content);
                }

            }
            return info;
        }

        /// <summary>
        /// Gets the names of the bones that should be used by the palette.
        /// </summary>
        protected ReadOnlyCollection<SkinInfoContentCollection> SkinnedBones
        { get { return new ReadOnlyCollection<SkinInfoContentCollection>(skinInfo); } }



        /// <summary>
        /// Gets the processor context.
        /// </summary>
        protected ContentProcessorContext ProcessorContext
        { get { return context; } }

        /// <summary>
        /// Called when an AnimationContent is processed.
        /// </summary>
        /// <param name="animation">The AnimationContent to be processed.</param>
        /// <returns>The processed AnimationContent.</returns>
        protected virtual AnimationContent ProcessAnimation(AnimationContent animation, out long durationOfFirstKeyFrameAfterSpeedChange, bool resample, bool compress, float? speedFactor = null)
        {            
            AnimationProcessor ap = new AnimationProcessor();

            durationOfFirstKeyFrameAfterSpeedChange = GetDurationOfFirstKeyframe(animation);

            AnimationContent animToCompress;
            AnimationContent newAnim;

            if (speedFactor.HasValue && MathHelper.Distance(speedFactor.Value, 1f) > 0.01f)
            {
                context.Logger.LogImportantMessage("Change animation {0} speed by factor: {1}", animation.Name, speedFactor);

                durationOfFirstKeyFrameAfterSpeedChange = (long)(durationOfFirstKeyFrameAfterSpeedChange / speedFactor.Value);

                AnimationContent changedSpeedAnim = ap.ChangeAnimationSpeed(animation, speedFactor.Value, context);
                changedSpeedAnim.Name = animation.Name;

                animToCompress = changedSpeedAnim;

                //newAnim = ap.ResampleAndCompress(changedSpeedAnim, context, sampleDistance, resample, compress);       
            }
            else
            {
                animToCompress = animation;              
            }

            newAnim = ap.ResampleAndCompress(animToCompress, context, sampleDistance, resample, compress);        

            // copy over important info:
            foreach (var item in animation.OpaqueData)
            {
                newAnim.OpaqueData[item.Key] = item.Value;
            }

            newAnim.Name = animation.Name;
            return newAnim;
        }

        private long GetDurationOfFirstKeyframe(AnimationContent input)
        {
            long duration = 0;
            foreach (KeyValuePair<string, AnimationChannel> c in input.Channels)
            {
                if (c.Value[1].Time.Ticks > duration)
                {
                    duration = c.Value[1].Time.Ticks;
                }               

            }
            

          //  context.Logger.LogImportantMessage("Duration of first keyframe before speed factor: {0}", duration);

            return (long)(duration * 2.5f); // Maya has already interpolated from 24 fps to 60 fps, so multiply by factor 2.5 to get the position of the first frame.
        }

        /// <summary>
        /// Called when an XML document is read that specifies how animations
        /// should be split.
        /// </summary>
        /// <param name="animDict">The dictionary of animation name/AnimationContent
        /// pairs. </param>
        /// <param name="doc">The Xml document that contains info on how to split
        /// the animations.</param>
        protected virtual void SubdivideAnimations(AnimationContentDictionary animDict, XmlDocument doc)
        {
            string[] animNames = new string[animDict.Keys.Count];
            animDict.Keys.CopyTo(animNames, 0);
            if (animNames.Length == 0)
                return;

            // Traverse each xml node that represents an animation to be subdivided
            foreach (XmlNode node in doc)
            {
                XmlElement child = node as XmlElement;
                if (child == null || child.Name != "animation")
                    continue;

                string animName = null;
                if (child["name"] != null)
                {
                    // The name of the animation to be split
                    animName = child["name"].InnerText;
                }
                else if (child["index"] != null)
                {
                    animName = animNames[int.Parse(child["index"].InnerText)];
                }
                else
                {
                    animName = animNames[0];
                }

                // If the tickspersecond node is filled, use that to calculate seconds per tick
                double animTicksPerSecond = 1.0, secondsPerTick = 0;
                try
                {
                    if (child["tickspersecond"] != null)
                    {
                        animTicksPerSecond = double.Parse(child["tickspersecond"].InnerText);
                    }
                }
                catch
                {
                    throw new Exception("Error parsing tickspersecond in xml file.");
                }
                if (animTicksPerSecond <= 0)
                    throw new InvalidDataException("AnimTicksPerSecond in XML file must be " +
                        "a positive number.");
                secondsPerTick = 1.0 / animTicksPerSecond;

                AnimationContent anim = null;
                // Get the animation and remove it from the dict
                // Check to see if the animation specified in the xml file exists
                try
                {
                    anim = animDict[animName];
                }
                catch
                {
                    throw new Exception("Animation named " + animName + " specified in XML file does not exist in model.");
                }
                animDict.Remove(anim.Name);
                // Get the list of new animations
                XmlNodeList subAnimations = child.GetElementsByTagName("animationsubset");

                foreach (XmlElement subAnim in subAnimations)
                {
                    // Create the new sub animation
                    AnimationContent newAnim = new AnimationContent();
                    XmlElement subAnimNameElement = subAnim["name"];

                    
                    if (subAnimNameElement != null)
                        newAnim.Name = subAnimNameElement.InnerText;

                    // If a starttime node exists, use that to get the start time
                    long startTime, endTime;
                    if (subAnim["starttime"] != null)
                    {
                        try
                        {
                            startTime = TimeSpan.FromSeconds(double.Parse(subAnim["starttime"].InnerText)).Ticks;
                        }
                        catch
                        {
                            throw new Exception("Error parsing starttime node in XML file.  Node inner text "
                                + "must be a non negative number.");
                        }
                    }
                    else if (subAnim["startframe"] != null)// else use the secondspertick combined with the startframe node value
                    {
                        try
                        {
                            double seconds =
                                double.Parse(subAnim["startframe"].InnerText) * secondsPerTick;

                            startTime = TimeSpan.FromSeconds(
                                seconds).Ticks;
                        }
                        catch
                        {
                            throw new Exception("Error parsing startframe node in XML file.  Node inner text "
                              + "must be a non negative number.");
                        }
                    }
                    else
                        throw new Exception("Sub animation in XML file must have either a starttime or startframe node.");

                    // Same with endtime/endframe
                    if (subAnim["endtime"] != null)
                    {
                        try
                        {
                            endTime = TimeSpan.FromSeconds(double.Parse(subAnim["endtime"].InnerText)).Ticks;
                        }
                        catch
                        {
                            throw new Exception("Error parsing endtime node in XML file.  Node inner text "
                                + "must be a non negative number.");
                        }
                    }
                    else if (subAnim["endframe"] != null)
                    {
                        try
                        {
                            double seconds = double.Parse(subAnim["endframe"].InnerText)
                                * secondsPerTick;
                            endTime = TimeSpan.FromSeconds(
                                seconds).Ticks;
                        }
                        catch
                        {
                            throw new Exception("Error parsing endframe node in XML file.  Node inner text "
                                + "must be a non negative number.");
                        }
                    }
                    else
                        throw new Exception("Sub animation in XML file must have either an endtime or endframe node.");

                    if (endTime < startTime)
                        throw new Exception("Start time must be <= end time in XML file.");

                    // Now that we have the start and end times, we associate them with
                    // start and end indices for each animation track/channel
                    foreach (KeyValuePair<string, AnimationChannel> k in anim.Channels)
                    {
                        // The current difference between the start time and the
                        // time at the current index
                        long currentStartDiff;
                        // The current difference between the end time and the
                        // time at the current index
                        long currentEndDiff;
                        // The difference between the start time and the time
                        // at the start index
                        long bestStartDiff=long.MaxValue;
                        // The difference between the end time and the time at
                        // the end index
                        long bestEndDiff=long.MaxValue;

                        // The start and end indices
                        int startIndex = -1;
                        int endIndex = -1;

                        // Create a new channel and reference the old channel
                        AnimationChannel newChan = new AnimationChannel();
                        AnimationChannel oldChan = k.Value;

                        // Iterate through the keyframes in the channel
                        for (int i = 0; i < oldChan.Count; i++)
                        {
                            // Update the startIndex, endIndex, bestStartDiff,
                            // and bestEndDiff
                            long ticks = oldChan[i].Time.Ticks;
                            currentStartDiff = Math.Abs(startTime - ticks);
                            currentEndDiff = Math.Abs(endTime - ticks);
                            if (startIndex == -1 || currentStartDiff<bestStartDiff)
                            {
                                startIndex = i;
                                bestStartDiff = currentStartDiff;
                            }
                            if (endIndex == -1 || currentEndDiff<bestEndDiff)
                            {
                                endIndex = i;
                                bestEndDiff = currentEndDiff;
                            }
                        }


                        // Now we have our start and end index for the channel
                        for (int i = startIndex; i <= endIndex; i++)
                        {
                            AnimationKeyframe frame = oldChan[i];
                            long time;
                            // Clamp the time so that it can't be less than the
                            // start time
                            if (frame.Time.Ticks < startTime)
                                time = 0;
                            // Clamp the time so that it can't be greater than the
                            // end time
                            else if (frame.Time.Ticks > endTime)
                                time = endTime - startTime;
                            else // Else get the time
                                time = frame.Time.Ticks - startTime;

                            // Finally... create the new keyframe and add it to the new channel
                            AnimationKeyframe keyframe = new AnimationKeyframe(
                                TimeSpan.FromTicks(time),
                                frame.Transform);
                            
                            newChan.Add(keyframe);
                        }
                        
                        // Add the channel and update the animation duration based on the
                        // length of the animation track.
                        newAnim.Channels.Add(k.Key, newChan);
                        if (newChan[newChan.Count - 1].Time > newAnim.Duration)
                            newAnim.Duration = newChan[newChan.Count - 1].Time;


                    }

                    XmlNodeList includedBones = subAnim.GetElementsByTagName("includebone");
                    if (includedBones != null && includedBones.Count > 0)
                    {

                        context.Logger.LogImportantMessage("Included bones found: ");
                        foreach (XmlElement inclBone in includedBones)
                        {
                            context.Logger.LogImportantMessage(inclBone.InnerText + ", ");
                        }

                        List<string> channelsToRemove = new List<string>();
                        foreach (KeyValuePair<string, AnimationChannel> kvp in newAnim.Channels)
                        {
                            if (!ChannelIsIncluded(kvp.Key, includedBones))
                            {
                                channelsToRemove.Add(kvp.Key);
                            }
                        }

                        foreach (string key in channelsToRemove)
	                    {
		                    newAnim.Channels.Remove(key);
	                    }                       

                    }

                    try
                    {
                        // Add the subdived animation to the dictionary.
                        animDict.Add(newAnim.Name, newAnim);
                    }
                    catch
                    {
                        throw new Exception("Attempt to add an animation when one by the same name already exists. " +
                            "Name: " + newAnim.Name);
                    }
                }
                
            }
        }
        

        private bool ChannelIsIncluded(string channelBoneName, XmlNodeList includedBones)
        {
            foreach (XmlElement includedBone in includedBones)
            {
                if (includedBone.InnerText == channelBoneName)
                {
                    return true;
                }
            }
            return false;
        }

        // Reads the XML document associated with the model if it exists.
        private XmlDocument ReadAnimationXML(NodeContent root, string animKey = null)
        {
            XmlDocument doc = null;
            
            string filePath;

            if (!string.IsNullOrEmpty(animKey))
            {
                string animMergedFrom;
                if (mergedAnimations.TryGetValue(animKey, out animMergedFrom))
                {
                    filePath = Path.ChangeExtension(animMergedFrom, ".xml");
                }
                else
                {
                    filePath = "Models/" + modelName + "_" + animKey + ".xml"; // ComposeFilePathFromAnimationName(animKey, ".xml");
                }
            }
            else
            {
                // for subdividing animations...
                filePath = "Models/" + modelName + "animation.xml"; // ComposeFilePathFromAnimationName(animKey, ".xml");
            }

         //   string filePath = "Models/" + modelName + "animation" + ".xml";

         /*   string filePath = Path.GetFullPath(root.Identity.SourceFilename);
            string fileName = Path.GetFileName(filePath);
            fileName = Path.GetDirectoryName(filePath);

            if (fileName!="")
                fileName += "\\";
            */
           // fileName += System.IO.Path.GetFileNameWithoutExtension(filePath) + "xml"; // "animation.xml";

            context.Logger.LogImportantMessage("Attempt to read animation info at: {0}", filePath);

            bool animXMLExists = File.Exists(filePath);

            if (animXMLExists)
            {
                doc = new XmlDocument();
                doc.Load(filePath);
            }

            return doc;
        }



        /// <summary>
        /// Called when a basic effect is encountered and potentially replaced by
        /// BasicPaletteEffect (if not overridden).  This is called afer effects have been processed.
        /// </summary>
        /// <param name="skinningType">The the skinning type of the meshpart.</param>
        /// <param name="meshPart">The MeshPart that contains the BasicMaterialContent.</param>
        protected virtual void ReplaceBasicEffect(SkinningType skinningType,
            ModelMeshPartContent meshPart)
        {
            BasicMaterialContent basic = meshPart.Material as BasicMaterialContent;
            if (basic != null)
            {
                context.Logger.LogImportantMessage("ReplaceBasicEffect on material {0}", meshPart.Material.Name);

                // Create a new PaletteSourceCode object and set its palette size
                // based on the platform since xbox has fewer registers.
                PaletteSourceCode source;
                if (context.TargetPlatform != TargetPlatform.Xbox360)
                {
                    source = new PaletteSourceCode(56);
                }
                else
                {
                    source = new PaletteSourceCode(40);
                }
                // Process the material and set the meshPart material to the new
                // material.
                PaletteInfoProcessor processor = new PaletteInfoProcessor();
                meshPart.Material = processor.Process(
                    new PaletteInfo(source.SourceCode4BonesPerVertex,
                    source.PALETTE_SIZE, basic), context);
            }
        }

        // Go through the modelmeshes and replace all basic effects for skinned models
        // with BasicPaletteEffect.
        private void ReplaceBasicEffects(ModelMeshContent input)
        {
            foreach (ModelMeshPartContent part in input.MeshParts)
            {
                SkinningType skinType = ContentUtil.GetSkinningType(VertexPositionColor.VertexDeclaration.GetVertexElements());
                if (skinType != SkinningType.None)
                {
                    ReplaceBasicEffect(skinType, part);
                }
            }
        }


        /// <summary>
        /// Searches through the NodeContent tree for all animations and puts them in
        /// one AnimationContentDictionary
        /// </summary>
        /// <param name="node">The root of the tree</param>
        private void FindAnimations(NodeContent node)
        {
            
            foreach (KeyValuePair<string, AnimationContent> k in node.Animations)
            {
                if (animations.ContainsKey(k.Key))
                {
                    foreach (KeyValuePair<string, AnimationChannel> c in k.Value.Channels)
                    {
                        animations[k.Key].Channels.Add(c.Key, c.Value);
                    }
                }
                else
                {
                    animations.Add(k.Key, k.Value);
                }
            }
            
            foreach (NodeContent child in node.Children)
                FindAnimations(child);
        }
        
        /// <summary>
        /// Go through the vertex channels in the geometry and replace the 
        /// BoneWeightCollection objects with weight and index channels.
        /// </summary>
        /// <param name="geometry">The geometry to process.</param>
        /// <param name="vertexChannelIndex">The index of the vertex channel to process.</param>
        /// <param name="context">The processor context.</param>
        protected override void ProcessVertexChannel(GeometryContent geometry, int vertexChannelIndex, ContentProcessorContext context)
        {
            bool boneCollectionsWithZeroWeights = false;
            if (geometry.Vertices.Channels[vertexChannelIndex].Name == VertexChannelNames.Weights())
            {
                int meshIndex = (int)geometry.Parent.OpaqueData["MeshIndex"];
                BoneIndexer indexer = indexers[meshIndex];
                // Skin channels are passed in from importers as BoneWeightCollection objects
                VertexChannel<BoneWeightCollection> vc = 
                    (VertexChannel<BoneWeightCollection>)
                    geometry.Vertices.Channels[vertexChannelIndex];
                int maxBonesPerVertex = 0;
                for (int i = 0; i < vc.Count; i++)
                {
                    int count = vc[i].Count;
                    if (count > maxBonesPerVertex)
                        maxBonesPerVertex = count;
                }

                // Add weights as colors (Converts well to 4 floats)
                // and indices as packed 4byte vectors.
                Color[] weightsToAdd = new Color[vc.Count];
                Byte4[] indicesToAdd = new Byte4[vc.Count];

                // Go through the BoneWeightCollections and create a new
                // weightsToAdd and indicesToAdd array for each BoneWeightCollection.
                for (int i = 0; i < vc.Count; i++)
                {
                    
                    BoneWeightCollection bwc = vc[i];

                    if (bwc.Count == 0)
                    {
                        boneCollectionsWithZeroWeights = true;
                        continue;
                    }

                    bwc.NormalizeWeights(4);

                    int count = bwc.Count;
                    if (count>maxBonesPerVertex)
                        maxBonesPerVertex = count;

                    // Add the appropriate bone indices based on the bone names in the
                    // BoneWeightCollection
                    Vector4 bi = new Vector4();
                    bi.X = count > 0 ? indexer.GetBoneIndex(bwc[0].BoneName) : (byte)0;
                    bi.Y = count > 1 ? indexer.GetBoneIndex(bwc[1].BoneName) : (byte)0;
                    bi.Z = count > 2 ? indexer.GetBoneIndex(bwc[2].BoneName) : (byte)0;
                    bi.W = count > 3 ? indexer.GetBoneIndex(bwc[3].BoneName) : (byte)0;


                    indicesToAdd[i] = new Byte4(bi);
                    Vector4 bw = new Vector4();
                    bw.X = count > 0 ? bwc[0].Weight : 0;
                    bw.Y = count > 1 ? bwc[1].Weight : 0;
                    bw.Z = count > 2 ? bwc[2].Weight : 0;
                    bw.W = count > 3 ? bwc[3].Weight : 0;
                    weightsToAdd[i] = new Color(bw);
                }

                // Remove the old BoneWeightCollection channel
                geometry.Vertices.Channels.Remove(vc);
                // Add the new channels
                geometry.Vertices.Channels.Add<Byte4>(VertexElementUsage.BlendIndices.ToString(), indicesToAdd);
                geometry.Vertices.Channels.Add<Color>(VertexElementUsage.BlendWeight.ToString(), weightsToAdd);
            }
            else
            {
                // No skinning info, so we let the base class process the channel
                base.ProcessVertexChannel(geometry, vertexChannelIndex, context);
            }
            if (boneCollectionsWithZeroWeights)
                context.Logger.LogWarning("", geometry.Identity,
                    "BonesWeightCollections with zero weights found in geometry.");
        }


        protected override MaterialContent ConvertMaterial(MaterialContent material, ContentProcessorContext context)
        {
            //   System.Diagnostics.Debugger.Launch();
            context.Logger.LogImportantMessage("Skinned material encountered: {0}", material.Name);

            OpaqueDataDictionary processorParameters = new OpaqueDataDictionary();
          /*  processorParameters.Add("EnvironmentMap", EnvironmentMap);

            processorParameters.Add("DirtMap", DirtMap);
            processorParameters.Add("DetailsMap", DetailsMap);
            */
            //NEW:
            processorParameters.Add("GenerateMipmaps", GenerateMipmaps);
            processorParameters.Add("TextureFormat", this.TextureFormat);
            processorParameters.Add("ResizeTexturesToPowerOfTwo", this.ResizeTexturesToPowerOfTwo);


            return context.Convert<MaterialContent, MaterialContent>(material,
                                                "SkinnedMaterialProcessor",
                                                processorParameters);
        }


    }
}


