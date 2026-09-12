using System.IO;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using Microsoft.Xna.Framework.Graphics;


namespace SpriteSheetPipeline
{
    /// <summary>
    /// This class will be instantiated by the XNA Framework Content Pipeline
    /// to apply custom processing to content data, converting an object of
    /// type TInput to TOutput. The input and output types may be the same if
    /// the processor wishes to alter data without changing its type.
    ///
    /// This should be part of a Content Pipeline Extension Library project.
    ///
    /// TODO: change the ContentProcessor attribute to specify the correct
    /// display name for this processor.
    /// </summary>
    [ContentProcessor(DisplayName = "SpriteSheetPipeline.SpriteSheetProcessor")]
    public class SpriteSheetProcessor : ContentProcessor<string[], SpriteSheetContent>
    {
        public enum Actions { None, MakeGhostImage }

        public Actions Action
        {
            get { return action; }
            set { action = value; }
        }

        Actions action = Actions.None;

        private bool processGroundSprites = true;
        public bool ProcessGroundSprites
        {
            get { return processGroundSprites; }
            set { processGroundSprites = value; }
        }

        private bool processOrdinarySprites = true;
        public bool ProcessOrdinarySprites
        {
            get { return processOrdinarySprites; }
            set { processOrdinarySprites = value; }
        }

      /*  private bool processConstructionSprites = true;
        public bool ProcessConstructionSprites
        {
            get { return processConstructionSprites; }
            set { processConstructionSprites = value; }
        }

        private bool processBurntSprites = true;
        public bool ProcessBurntSprites
        {
            get { return processBurntSprites; }
            set { processBurntSprites = value; }
        }*/

        /// <summary>
        /// Converts an array of sprite filenames into a sprite sheet object.
        /// </summary>
        public override SpriteSheetContent Process(string[] input,
                                                   ContentProcessorContext context)
        {
            context.Logger.LogImportantMessage("Start processing sprites.");

            SpriteSheetContent spriteSheet = new SpriteSheetContent();

            List<BitmapContent> sourceSprites = new List<BitmapContent>();

            bool hasLogged = false;

          //  bool placeFirstSpriteInTopLeft = false;

            // Loop over each input sprite filename.
            foreach (string inputFilename in input)
            {
                if (inputFilename.EndsWith("\\"))
                {
                    context.Logger.LogImportantMessage("Processing sprites in folder {0}", inputFilename);
                    string folderName = inputFilename.Substring(0, inputFilename.Length - 1);

                /*    if (folderName.Contains("GUI") && !placeFirstSpriteInTopLeft)
                    {
                        // place the window system bitmap first, at 0,0:
                        string filename = Path.Combine(folderName, "DefaultStyle.png");
                        string spriteName = Path.GetFileNameWithoutExtension(filename);

                        if (!filename.EndsWith(".db") && File.Exists(filename))
                        {
                            AddSprite(context, spriteSheet, sourceSprites, filename, spriteName, false, Action);
                            placeFirstSpriteInTopLeft = true;
                        }
                    }*/

                    string[] filesInFolder = System.IO.Directory.GetFiles(folderName);

                    bool processOrdinarySpritesInThisFolder = !folderName.ToLowerInvariant().Contains("billboards") || ProcessOrdinarySprites;

                    foreach (string filename in filesInFolder)
                    {
                        string spriteName = Path.GetFileNameWithoutExtension(filename);

                        context.Logger.LogImportantMessage("Look at file {0}, suffix: {1}, extension: {2}, spritename: {3}",
                            filename, spriteName.Substring(spriteName.Length - 2, 1), Path.GetExtension(filename), spriteName);

                        if (Path.GetExtension(filename) == ".db" || Path.GetExtension(filename) == ".txt")
                        {
                            continue;
                        }

                        if (spriteName.Substring(spriteName.Length - 2, 1) == "_")
                        {// omit "_d" and "_u" sprites.
                            if (spriteName.EndsWith("_d") || spriteName.EndsWith("_u"))
                            {
                                continue;
                            }
                            else if (spriteName.EndsWith("_g")) //|| spriteName.Substring(spriteName.Length - 2, 1) != "_") // why?
                            {
                                if (ProcessGroundSprites)
                                {
                                    // 'ground' sprite: _g
                                    AddSprite(context, spriteSheet, sourceSprites, filename, spriteName, false, Action);
                                }
                            }
                            else
                            {
                                if (processOrdinarySpritesInThisFolder)
                                {
                                    AddSprite(context, spriteSheet, sourceSprites, filename, spriteName, false, Action);
                                }
                            }
                        }
                        /* if (spriteName.Substring(spriteName.Length - 2, 1) == "_")
                        {
                            if (Path.GetExtension(filename) == ".tga")
                             {
                                 if (inputFilename.Contains("structureBillboards"))
                                 {
                            if (ProcessGroundSprites && spriteName.EndsWith("_g")) //|| spriteName.Substring(spriteName.Length - 2, 1) != "_") // why?
                            {   // we found a 'base' sprite - no suffix OR a 'ground' sprite: _g
                                AddSprite(context, spriteSheet, sourceSprites, filename, spriteName, false, Action);

                            }
                            //  }
                            
                        }*/                     
                       /* else if (inputFilename.Contains("itemBillboards"))
                        {   // item sprites for the GUI sheet!
                            if (spriteName != "cement" && spriteName != "planks" && spriteName != "metal") // special hand-crafted icons exist for these.
                            {
                                AddItemSprite(context, spriteSheet, sourceSprites, filename, spriteName);
                            }

                        }  */
                        else /*if (inputFilename.Contains("overlays"))*/
                        {
                            if (processOrdinarySpritesInThisFolder)
                            {
                                AddSprite(context, spriteSheet, sourceSprites, filename, spriteName, false, Action);
                            }

                        }
                    }
                }
                else
                {
                    // load single sprite
                    // Store the name of this sprite.
                    string spriteName = Path.GetFileNameWithoutExtension(inputFilename);
                    spriteSheet.SpriteNames.Add(spriteName, sourceSprites.Count);


                    LoadSpriteTexture(context, sourceSprites, inputFilename, false, Action);

                }
            }

            // Pack all the sprites into a single large texture.

            PixelBitmapContent<NormalizedByte4> packedSpritesNormalMaps = null;
            BitmapContent packedSprites = null;

            SpritePacker.PackSprites(sourceSprites, spriteSheet.SpriteNames, null,
                                     spriteSheet.SpriteRectangles, context,
                                     out packedSprites, out packedSpritesNormalMaps); //, placeFirstSpriteInTopLeft);

            spriteSheet.Texture.Mipmaps.Add(packedSprites);
            //context.Logger.LogImportantMessage("Added diffuse sprite sheet texture.");


            return spriteSheet;
        }

        private static void AddSprite(ContentProcessorContext context, SpriteSheetContent spriteSheet, 
            List<BitmapContent> sourceSprites, string filename, string spriteName, bool makeGrayscale, Actions action)
        {
            spriteSheet.SpriteNames.Add(spriteName, sourceSprites.Count);

            context.Logger.LogImportantMessage("Adding diffuse sprite {0}", filename);
            LoadSpriteTexture(context, sourceSprites, filename, makeGrayscale, action);
        }

        /*
        private static void AddItemSprite(ContentProcessorContext context, SpriteSheetContent spriteSheet, List<BitmapContent> sourceSprites, string filename, string spriteName)
        {
            spriteSheet.SpriteNames.Add(spriteName, sourceSprites.Count);

            context.Logger.LogImportantMessage("Adding GUI item sprite {0}", filename);
            LoadItemSpriteTexture(context, sourceSprites, filename);
        }*/

        /// <summary>
        /// no longer used... we will have assets for all item icons.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="sourceSprites"></param>
        /// <param name="inputFilename"></param>
       /* public static void LoadItemSpriteTexture(ContentProcessorContext context, List<BitmapContent> sourceSprites, string inputFilename)
        {
            // Load the sprite texture into memory.
            ExternalReference<TextureContent> textureReference = new ExternalReference<TextureContent>(inputFilename);
            
            OpaqueDataDictionary data = new OpaqueDataDictionary(); // XNA 4 - with premultiply alpha!
            data.Add("PremultiplyAlpha", true);
            TextureContent texture = context.BuildAndLoadAsset<TextureContent, TextureContent>(textureReference, "TextureProcessor", data, null);

            PixelBitmapContent<Color> bitmap = (PixelBitmapContent<Color>)texture.Faces[0][0];

            PixelBitmapContent<Color> newBitmap = new PixelBitmapContent<Color>(28, 18);

            for (int y = 15; y < bitmap.Height - 15; y++)
            {
                for (int x = 10; x < bitmap.Width - 10; x++)
                {
                    Color value = bitmap.GetPixel(x, y);
                    // int gray = (value.R + value.G + value.B) / 3;

                    //create the grayscale version of the pixel
                    int grayScale = (int)((value.R * .3) + (value.G * .59)
                        + (value.B * .11));

                    //create the color object
                    System.Drawing.Color newColor = System.Drawing.Color.FromArgb(value.A, grayScale, grayScale, grayScale);

                    // desaturate
                    //Vector3 grayXfer = new Vector3(0.3f, 0.59f, 0.11f);
                    //   Vector3 normalized = value.ToVector3();
                    //   normalized.Normalize();
                    //   float gray = Vector3.Dot(grayXfer, normalized);

                    //bitmap.SetPixel(x, y, new Microsoft.Xna.Framework.Graphics.Color(newColor.R, newColor.G, newColor.B, newColor.A)); //new Microsoft.Xna.Framework.Graphics.Color(gray, gray, gray, value.A)); //(value.A / 255f)));
                    newBitmap.SetPixel(x - 10, y - 15, new Color(newColor.R, newColor.G, newColor.B, newColor.A));
                }
            }

            sourceSprites.Add(newBitmap); //texture.Faces[0][0]);

        }*/



        public static void LoadSpriteTexture(ContentProcessorContext context,
            List<BitmapContent> sourceSprites, string inputFilename, bool makeGrayscale, Actions action)
        {
            // Load the sprite texture into memory.
            ExternalReference<TextureContent> textureReference =
                            new ExternalReference<TextureContent>(inputFilename);
          

            //TextureContent texture = context.BuildAndLoadAsset<TextureContent, TextureContent>(textureReference, null); // xna 3

            OpaqueDataDictionary data = new OpaqueDataDictionary(); // XNA 4 - with premultiply alpha!
            data.Add("PremultiplyAlpha", true);
            TextureContent texture = context.BuildAndLoadAsset<TextureContent, TextureContent>(textureReference, "TextureProcessor", data, null);

            PixelBitmapContent<Color> bitmap = (PixelBitmapContent<Color>)texture.Faces[0][0];

            // NEW: Fix Phostoshop stupidity with transparent PNGs:
            // xna 4 made this unnecessary.
            //FixSpriteZeroAlphaColors(bitmap);

            if (action == Actions.MakeGhostImage || makeGrayscale)
            {
                
                for (int y = 0; y < bitmap.Height; y++)
                {
                    for (int x = 0; x < bitmap.Width; x++)
                    {
                        Color value = bitmap.GetPixel(x, y);
                        // int gray = (value.R + value.G + value.B) / 3;

                        //create the grayscale version of the pixel
                        int grayScale = (int)((value.R * .3) + (value.G * .59)
                            + (value.B * .11));

                        //create the color object
                        System.Drawing.Color newColor = System.Drawing.Color.FromArgb(value.A, grayScale, grayScale, grayScale);


                        // desaturate
                        /*   Vector3 grayXfer = new Vector3(0.3f, 0.59f, 0.11f);
                           Vector3 normalized = value.ToVector3();
                           normalized.Normalize();
                           float gray = Vector3.Dot(grayXfer, normalized);*/

                        bitmap.SetPixel(x, y, new Color(newColor.R, newColor.G, newColor.B, newColor.A)); //new Microsoft.Xna.Framework.Graphics.Color(gray, gray, gray, value.A)); //(value.A / 255f)));
                    }
                }
            }

            if (action == Actions.MakeGhostImage)
            {
                PixelBitmapContent<Color> blurredBitmap = DoBlur(bitmap, context);

                AlphaBlend(bitmap, blurredBitmap, context);
                
                sourceSprites.Add(blurredBitmap);
            }
            else
            {

                sourceSprites.Add(texture.Faces[0][0]);
            }



            /*  }
              catch (System.Exception e)
              {
                  context.Logger.LogImportantMessage("Adding utility sprite image {0} error: {1}", inputFilename, e.Message);
              } */

        }

        private static void AlphaBlend(PixelBitmapContent<Color> sourceBitmap, PixelBitmapContent<Color> destinationBitmap, ContentProcessorContext context)
        {
            int width = sourceBitmap.Width;
            int height = sourceBitmap.Height;
                        
            PixelBitmapContent<Color> outputBitmap = new PixelBitmapContent<Color>(width, height);

            // Horizontal blur
            for (int i = 0; i < width - 1; i++)
            {
                for (int j = 0; j < height - 1; j++)
                {
                    Vector4 source = sourceBitmap.GetPixel(i, j).ToVector4();
                    Vector4 dest = destinationBitmap.GetPixel(i, j).ToVector4();

                    Vector4 result = source.W * source + (1f - source.W) * dest;

                    float contrastGray = MathHelper.SmoothStep(0f, 1f, result.X);

                   // contrastGray = 1.5f * contrastGray; //1.2f * contrastGray; // lighten?

                    result.X = contrastGray;
                    result.Y = contrastGray;
                    result.Z = contrastGray;

                    destinationBitmap.SetPixel(i, j, new Color(result)); 
                }
            }
        }

        private static PixelBitmapContent<Color> DoBlur(PixelBitmapContent<Color> sourceBitmap, ContentProcessorContext context)
        {
            
            float sumAlpha = 0, sumRed = 0, sumGreen = 0, sumBlue = 0;

           // int[] GaussFact = new int[] { 1, 6, 15, 20, 15, 6, 1 }; 
            //   int gaussSum = 64;
            int[] GaussFact = new int[] {1, 2, 6, 10, 20, 30, 40, 50, 60, 50, 40, 30, 20, 10, 6, 2, 1 };
            int gaussSum = 378;

            int gaussWidth = GaussFact.Length; // 7;


            int width = sourceBitmap.Width;
            int height = sourceBitmap.Height;

            PixelBitmapContent<Color> intermediaryBitmap = new PixelBitmapContent<Color>(width, height);
            PixelBitmapContent<Color> outputBitmap = new PixelBitmapContent<Color>(width, height);

            // Horizontal blur
            for (int i = 1; i < width - 1; i++)
            {
                for (int j = 1; j < height - 1; j++)
                {
                    // Clear colour fields
                    sumRed = sumGreen = sumBlue = sumAlpha = 0;

                    for (int k = 0; k < gaussWidth; k++)
                    {
                        // INT x = i - ((GaussWidth - 1) >> 1) + k;
                        int x = (int)MathHelper.Max(0, MathHelper.Min(width - 1, i - ((gaussWidth - 1) >> 1) + k));
                    //    context.Logger.LogImportantMessage("DoBlur x: {0}", x);
             
                        int y = j;
                        Color color = sourceBitmap.GetPixel(x, y);
                        Vector4 colorValues = color.ToVector4();

                        sumAlpha += colorValues.W * GaussFact[k];
                        sumRed += colorValues.X * GaussFact[k];
                        sumGreen += colorValues.Y * GaussFact[k];
                        sumBlue += colorValues.Z * GaussFact[k];
                    }

                    // Draw blurred pixel
                    intermediaryBitmap.SetPixel(i, j,
                        new Color(
                            MathHelper.Clamp(sumRed / gaussSum, 0f, 1f),
                            MathHelper.Clamp(sumGreen / gaussSum, 0f, 1f),
                            MathHelper.Clamp(sumBlue / gaussSum, 0f, 1f),
                            MathHelper.Clamp(sumAlpha / gaussSum, 0f, 1f)));
                    // pdwPixels[j * Pitch + i] = D3DCOLOR_ARGB(dwAlpha / GaussSum, dwRed / GaussSum, dwGreen / GaussSum, dwBlue / GaussSum);
                }
            }

            // Vertical blur
            for (int i = 1; i < width - 1; i++)
            {
                for (int j = 1; j < height - 1; j++)
                {
                    // Clear colour fields
                    sumRed = sumGreen = sumBlue = sumAlpha = 0;

                    for (int k = 0; k < gaussWidth; k++)
                    {
                        int x = i;
                        // INT y = j - ((GaussWidth - 1) >> 1) + k;
                        int y = (int)MathHelper.Max(0, MathHelper.Min(height - 1, j - ((gaussWidth - 1) >> 1) + k));

                        Color color = intermediaryBitmap.GetPixel(x, y);
                        Vector4 colorValues = color.ToVector4();


                        sumAlpha += colorValues.W * GaussFact[k];
                        sumRed += colorValues.X * GaussFact[k];
                        sumGreen += colorValues.Y * GaussFact[k];
                        sumBlue += colorValues.Z * GaussFact[k];
                    }

                    // Draw blurred pixel                  
                    outputBitmap.SetPixel(i, j,
                        new Color(
                            MathHelper.Clamp(sumRed / gaussSum, 0f, 1f),
                            MathHelper.Clamp(sumGreen / gaussSum, 0f, 1f),
                            MathHelper.Clamp(sumBlue / gaussSum, 0f, 1f),
                            MathHelper.Clamp(sumAlpha / gaussSum, 0f, 1f)));
                }
            }

            return outputBitmap;
            
        }

        // --------------------------------------------------------------------------------------------------------------------------------  
        // Adobe Photoshop will save a PNG file with all alpha=0 pixels as white, EVEN IF there are colors  
        // associated with them.  Since the GPU USES these colors, this function will re-create the colors for  
        // ALL alpha=0 pixels that are adjacent to alpha!=0 pixels to be the average color, by weight of alpha,  
        // of all alpha!=0 adjacent pixels.  So, the GPU will fade into the proper color while simultaneously  
        // fading into transparency, instead of into white which gives an undesired white glow!  
        public static void FixSpriteZeroAlphaColors(PixelBitmapContent<Color> bitmap) // Texture2D textSprite)
        {
            // 1. declare a uint array to hold the pixel data  
            int width = bitmap.Width; // textSprite.Width;
            int height = bitmap.Height; // textSprite.Height;

          //  bitmap.GetPixelData();

           /*  Color[] clrPixelData = new Color[width * height];

            // 2. populate the array  
            textSprite.GetData(clrPixelData, 0, width * height);*/


            // 3. fix up alpha=0 pixels that are adjacent to alpha!=0 pixels  

            // NOTE: it's ok to modify the array IN PLACE, since we're only changing the A = 0 pixels,  
            // which are NOT used in the calculations; they don't affect the outcome.  Only A != 0 pixels  
            // affect the outcome of the A = 0 pixels that reside next to visible pixels.  
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                   // int index = y * width + x;
                    
                    Color value = bitmap.GetPixel(x, y);

                    if (value.A == 0)
                    {
                        float r = 0;
                        float g = 0;
                        float b = 0;
                        int count = 0;
                        // go through all 8 pixels around  
                        // don't look at current pixel, but we ignore A=0, anyway, which the current pixel IS,  
                        // so we can just loop 3x3 around it, including it  
                        for (int yy = y - 1; yy <= y + 1; yy++)
                        {
                            for (int xx = x - 1; xx <= x + 1; xx++)
                            {
                                // don't go out of range  
                                if ((xx >= 0) && (yy >= 0) && (xx < width) && (yy < height))
                                {
                                    // look at only pixels A != 0  
                                   // int index2 = yy * width + xx;
                                    Color value2 = bitmap.GetPixel(xx, yy);

                                    byte alpha = value2.A;
                                    if (alpha != 0)
                                    {
                                        r += value2.R * alpha;
                                        g += value2.G * alpha;
                                        b += value2.B * alpha;
                                        count++;
                                    }
                                }
                            }
                        }
                        // did we get any non-alpha=0 info?  
                        if (count > 0)
                        {
                            // modify the A=0 pixel to be this average color  
                            bitmap.SetPixel(x, y, new Color(
                                (byte)(r / count / 255.0f),
                                (byte)(g / count / 255.0f),
                                (byte)(b / count / 255.0f),
                                (byte)0));
                        }
                    } 
                }
            }

            // 4. set back the information to the texture  
           // textSprite.SetData(clrPixelData, 0, width * height, SetDataOptions.None);
        } 

    }
}