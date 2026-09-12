#region File Description
//-----------------------------------------------------------------------------
// SpritePacker.cs
//
// Microsoft Game Technology Group
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;
#endregion

namespace SpriteSheetPipeline
{
    /// <summary>
    /// Helper for arranging many small sprites into a single larger sheet.
    /// </summary>
    public static class SpritePacker
    {
        /// <summary>
        /// Packs a list of sprites into a single big texture,
        /// recording where each one was stored.
        /// </summary>
        public static void PackSprites(IList<BitmapContent> sourceSprites,
                                                Dictionary<string, int> SpriteNames, // for debugging only
                                                IList<PixelBitmapContent<NormalizedByte4>> sourceSpritesNormalMaps,
                                                ICollection<Rectangle> outputSprites,
                                                ContentProcessorContext context, 
                                                out BitmapContent packedDiffuse,
                                                out PixelBitmapContent<NormalizedByte4> packedNormalMaps)          
        {
            if (sourceSprites.Count == 0)
                throw new InvalidContentException("There are no sprites to arrange");

            packedNormalMaps = null;
            
            // Build up a list of all the sprites needing to be arranged.
            List<ArrangedSprite> sprites = new List<ArrangedSprite>();

            string[] spriteNamesAsArray = new string[SpriteNames.Count];
            SpriteNames.Keys.CopyTo(spriteNamesAsArray, 0);

            for (int i = 0; i < sourceSprites.Count; i++)
            {
                ArrangedSprite sprite = new ArrangedSprite();

                sprite.SpriteName = spriteNamesAsArray[i];

                if (sourceSprites != null && sourceSpritesNormalMaps != null)
                {
                    // NEW - check dimensions on depth maps and sprites:
                    if (sourceSprites[i] != null && sourceSpritesNormalMaps[i] != null &&
                        (sourceSprites[i].Width != sourceSpritesNormalMaps[i].Width
                        || sourceSprites[i].Height != sourceSpritesNormalMaps[i].Height))
                    {
                        throw new Exception("Sprite and depth map dimension mismatch: " + sprite.SpriteName);
                    }
                }

                // Include a single pixel padding around each sprite, to avoid
                // filtering problems if the sprite is scaled or rotated.
                sprite.Width = sourceSprites[i].Width + 2;
                sprite.Height = sourceSprites[i].Height + 2;

                sprite.Index = i;

                sprites.Add(sprite);
            }
            
            // Sort so the largest sprites get arranged first.
            sprites.Sort(CompareSpriteSizes);
                  

            // Work out how big the output bitmap should be.
            /* http://forums.xna.com/forums/p/25948/141687.aspx#141687
             Every card that can run XNA games support at least 2048x2048 textures. 
             * Every card with shader model 3 support support at least 4096x4096. 
             * I think that the DX10 cards support 8192x8192, but I don't know of that's mandatory or if there's some Intel Integrated stuff that's DX10 but only does 4096.
             */
            int outputWidth = GuessOutputWidth(sourceSprites);
            int maxWidth = 4096;
            if (outputWidth > maxWidth) //4096)//
            {
                throw new Exception(string.Format("Sprite sheet would probably exceed {0} pixels!", maxWidth));
            }

            int outputHeight = 0;
            int totalSpriteSize = 0;

            // Choose positions for each sprite, one at a time.
            for (int i = 0; i < sprites.Count; i++)
            {
                PositionSprite(sprites, i, outputWidth);

                outputHeight = Math.Max(outputHeight, sprites[i].Y + sprites[i].Height);

                totalSpriteSize += sprites[i].Width * sprites[i].Height;


                context.Logger.LogImportantMessage(
                    "Packed sprite named {0}  X: {1} Y: {2} W: {3}  H: {4} ",
                    sprites[i].SpriteName, sprites[i].X, sprites[i].Y, sprites[i].Width, sprites[i].Height);
                

            }

            // Sort the sprites back into index order.
            sprites.Sort(CompareSpriteIndices);

            context.Logger.LogImportantMessage(
                "Packed {0} sprites into a {1}x{2} sheet, {3}% efficiency",
                sprites.Count, outputWidth, outputHeight,
                totalSpriteSize * 100 / outputWidth / outputHeight);

            
            packedDiffuse = new PixelBitmapContent<Color>(outputWidth, outputHeight);
            CopySpritesToOutput(sprites, sourceSprites, outputSprites, packedDiffuse, context);
            context.Logger.LogImportantMessage("Packed diffuse maps. ");

            if (sourceSpritesNormalMaps != null)
            {
                packedNormalMaps = new PixelBitmapContent<NormalizedByte4>(outputWidth, outputHeight);
                CopyNormalMapsToOutput(sprites, sourceSpritesNormalMaps, null, packedNormalMaps, context);
                context.Logger.LogImportantMessage("Packed normal maps. ");
            }
                        
        }

        public static void PackSpritesNoNormalize(IList<BitmapContent> sourceSprites,
                                               Dictionary<string, int> SpriteNames, // for debugging only
                                               IList<PixelBitmapContent<Vector4>> sourceSpritesNormalMaps,
                                               ICollection<Rectangle> outputSprites,
                                               ContentProcessorContext context,
                                               out BitmapContent packedDiffuse,
                                               out PixelBitmapContent<Vector4> packedNormalMaps)
        {
            if (sourceSprites.Count == 0)
                throw new InvalidContentException("There are no sprites to arrange");

            packedNormalMaps = null;

            // Build up a list of all the sprites needing to be arranged.
            List<ArrangedSprite> sprites = new List<ArrangedSprite>();

            string[] spriteNamesAsArray = new string[SpriteNames.Count];
            SpriteNames.Keys.CopyTo(spriteNamesAsArray, 0);

            for (int i = 0; i < sourceSprites.Count; i++)
            {
                ArrangedSprite sprite = new ArrangedSprite();

                sprite.SpriteName = spriteNamesAsArray[i];

                if (sourceSprites != null && sourceSpritesNormalMaps != null)
                {
                    // NEW - check dimensions on depth maps and sprites:
                    if (sourceSprites[i] != null && sourceSpritesNormalMaps[i] != null &&
                        (sourceSprites[i].Width != sourceSpritesNormalMaps[i].Width
                        || sourceSprites[i].Height != sourceSpritesNormalMaps[i].Height))
                    {
                        throw new Exception("Sprite and depth map dimension mismatch: " + sprite.SpriteName);
                    }
                }

                // Include a single pixel padding around each sprite, to avoid
                // filtering problems if the sprite is scaled or rotated.
                sprite.Width = sourceSprites[i].Width + 2;
                sprite.Height = sourceSprites[i].Height + 2;

                sprite.Index = i;

                sprites.Add(sprite);
            }

            // Sort so the largest sprites get arranged first.
            sprites.Sort(CompareSpriteSizes);


            // Work out how big the output bitmap should be.
            /* http://forums.xna.com/forums/p/25948/141687.aspx#141687
             Every card that can run XNA games support at least 2048x2048 textures. 
             * Every card with shader model 3 support support at least 4096x4096. 
             * I think that the DX10 cards support 8192x8192, but I don't know of that's mandatory or if there's some Intel Integrated stuff that's DX10 but only does 4096.
             */
            int outputWidth = GuessOutputWidth(sourceSprites);
            int maxWidth = 4096;
            if (outputWidth > maxWidth) //4096)//
            {
                throw new Exception(string.Format("Sprite sheet would probably exceed {0} pixels!", maxWidth));
            }

            int outputHeight = 0;
            int totalSpriteSize = 0;

            // Choose positions for each sprite, one at a time.
            for (int i = 0; i < sprites.Count; i++)
            {
                PositionSprite(sprites, i, outputWidth);

                outputHeight = Math.Max(outputHeight, sprites[i].Y + sprites[i].Height);

                totalSpriteSize += sprites[i].Width * sprites[i].Height;


                context.Logger.LogImportantMessage(
                    "Packed sprite named {0}  X: {1} Y: {2} W: {3}  H: {4} ",
                    sprites[i].SpriteName, sprites[i].X, sprites[i].Y, sprites[i].Width, sprites[i].Height);


            }

            // Sort the sprites back into index order.
            sprites.Sort(CompareSpriteIndices);

            context.Logger.LogImportantMessage(
                "Packed {0} sprites into a {1}x{2} sheet, {3}% efficiency",
                sprites.Count, outputWidth, outputHeight,
                totalSpriteSize * 100 / outputWidth / outputHeight);


            packedDiffuse = new PixelBitmapContent<Color>(outputWidth, outputHeight);
            CopySpritesToOutput(sprites, sourceSprites, outputSprites, packedDiffuse, context);
            context.Logger.LogImportantMessage("Packed diffuse maps. ");

             
            if (sourceSpritesNormalMaps != null)
            {
                packedNormalMaps = new PixelBitmapContent<Vector4>(outputWidth, outputHeight);
                CopyNormalMapsToOutputNoNormalize(sprites, sourceSpritesNormalMaps, null, packedNormalMaps, context);
                context.Logger.LogImportantMessage("Packed normal maps. ");
            }

        }

        /// <summary>
        /// Once the arranging is complete, copies the bitmap data for each
        /// sprite to its chosen position in the single larger output bitmap.
        /// </summary>
     /*   static BitmapContent CopySpritesToOutput(List<ArrangedSprite> sprites,
                                                 IList<BitmapContent> sourceSprites,
                                                 ICollection<Rectangle> outputSprites,
                                                 int width, int height)*/
        static void CopySpritesToOutput(List<ArrangedSprite> sprites,
                                                IList<BitmapContent> sourceSprites,
                                                ICollection<Rectangle> outputSprites,
                                                BitmapContent output,
                                                ContentProcessorContext context)
        {
            //BitmapContent output = new PixelBitmapContent<Color>(width, height);

            foreach (ArrangedSprite sprite in sprites)
            {
                BitmapContent source = sourceSprites[sprite.Index];

                if (source != null)
                {

                    int x = sprite.X;
                    int y = sprite.Y;

                    int w = source.Width;
                    int h = source.Height;

                  //  context.Logger.LogImportantMessage(string.Format("Packing sprite {0} x,y: {1},{2} w,h: {3},{4} into {5}*{6}", sprite.SpriteName, x, y, w, h, output.Width, output.Height));
                    

                    // Copy the main sprite data to the output sheet.
                    BitmapContent.Copy(source, new Rectangle(0, 0, w, h),
                                       output, new Rectangle(x + 1, y + 1, w, h));

                    // Copy a border strip from each edge of the sprite, creating
                    // a one pixel padding area to avoid filtering problems if the
                    // sprite is scaled or rotated.
                    BitmapContent.Copy(source, new Rectangle(0, 0, 1, h),
                                       output, new Rectangle(x, y + 1, 1, h));

                    BitmapContent.Copy(source, new Rectangle(w - 1, 0, 1, h),
                                       output, new Rectangle(x + w + 1, y + 1, 1, h));

                    BitmapContent.Copy(source, new Rectangle(0, 0, w, 1),
                                       output, new Rectangle(x + 1, y, w, 1));

                    BitmapContent.Copy(source, new Rectangle(0, h - 1, w, 1),
                                       output, new Rectangle(x + 1, y + h + 1, w, 1));

                    // Copy a single pixel from each corner of the sprite,
                    // filling in the corners of the one pixel padding area.
                    BitmapContent.Copy(source, new Rectangle(0, 0, 1, 1),
                                       output, new Rectangle(x, y, 1, 1));

                    BitmapContent.Copy(source, new Rectangle(w - 1, 0, 1, 1),
                                       output, new Rectangle(x + w + 1, y, 1, 1));

                    BitmapContent.Copy(source, new Rectangle(0, h - 1, 1, 1),
                                       output, new Rectangle(x, y + h + 1, 1, 1));

                    BitmapContent.Copy(source, new Rectangle(w - 1, h - 1, 1, 1),
                                       output, new Rectangle(x + w + 1, y + h + 1, 1, 1));

                    // Remember where we placed this sprite.
                    if (outputSprites != null)
                    {
                        outputSprites.Add(new Rectangle(x + 1, y + 1, w, h));
                    }
                }
            }

            //return output;
        }

        static void CopyNormalMapsToOutputNoNormalize(List<ArrangedSprite> sprites,
                                              IList<PixelBitmapContent<Vector4>> sourceSprites,
                                              ICollection<Rectangle> outputSprites,
                                              PixelBitmapContent<Vector4> output,
                                              ContentProcessorContext context)
        {

            foreach (ArrangedSprite sprite in sprites)
            {
                PixelBitmapContent<Vector4> source = sourceSprites[sprite.Index];

                if (source != null)
                {
                    int x = sprite.X;
                    int y = sprite.Y;

                    int w = source.Width;
                    int h = source.Height;

                    // context.Logger.LogImportantMessage(string.Format("Packing normal map {0} x,y: {1},{2} w,h: {3},{4} into {5}*{6}", sprite.SpriteName, x, y, w, h, output.Width, output.Height));

                    context.Logger.LogImportantMessage(string.Format("Copy started..."));

                    // Copy the main sprite data to the output sheet.
                    PixelBitmapContent<Vector4>.Copy(source, new Rectangle(0, 0, w, h), // this throws an exception http://community.monogame.net/t/pixelbitmapcontent-vector4-copy-throws-exception-but-xna-doesnt/8018
                                       output, new Rectangle(x + 1, y + 1, w, h));

                    context.Logger.LogImportantMessage(string.Format("Adding strips..."));

                    // Copy a border strip from each edge of the sprite, creating
                    // a one pixel padding area to avoid filtering problems if the
                    // sprite is scaled or rotated.
                    PixelBitmapContent<Vector4>.Copy(source, new Rectangle(0, 0, 1, h),
                                       output, new Rectangle(x, y + 1, 1, h));

                    PixelBitmapContent<Vector4>.Copy(source, new Rectangle(w - 1, 0, 1, h),
                                       output, new Rectangle(x + w + 1, y + 1, 1, h));

                    PixelBitmapContent<Vector4>.Copy(source, new Rectangle(0, 0, w, 1),
                                       output, new Rectangle(x + 1, y, w, 1));

                    PixelBitmapContent<Vector4>.Copy(source, new Rectangle(0, h - 1, w, 1),
                                       output, new Rectangle(x + 1, y + h + 1, w, 1));

                    //  context.Logger.LogImportantMessage(string.Format("Adding corners..."));

                    // Copy a single pixel from each corner of the sprite,
                    // filling in the corners of the one pixel padding area.
                    PixelBitmapContent<Vector4>.Copy(source, new Rectangle(0, 0, 1, 1),
                                       output, new Rectangle(x, y, 1, 1));

                    PixelBitmapContent<Vector4>.Copy(source, new Rectangle(w - 1, 0, 1, 1),
                                       output, new Rectangle(x + w + 1, y, 1, 1));

                    PixelBitmapContent<Vector4>.Copy(source, new Rectangle(0, h - 1, 1, 1),
                                       output, new Rectangle(x, y + h + 1, 1, 1));

                    PixelBitmapContent<Vector4>.Copy(source, new Rectangle(w - 1, h - 1, 1, 1),
                                       output, new Rectangle(x + w + 1, y + h + 1, 1, 1));

                    // Remember where we placed this sprite.
                    if (outputSprites != null)
                    {
                        outputSprites.Add(new Rectangle(x + 1, y + 1, w, h));
                    }
                }
            }

            //return output;
        }

        static void CopyNormalMapsToOutput(List<ArrangedSprite> sprites,
                                                IList<PixelBitmapContent<NormalizedByte4>> sourceSprites,
                                                ICollection<Rectangle> outputSprites,
                                                PixelBitmapContent<NormalizedByte4> output,
                                                ContentProcessorContext context)
        {
           
            foreach (ArrangedSprite sprite in sprites)
            {
                PixelBitmapContent<NormalizedByte4> source = sourceSprites[sprite.Index];

                if (source != null)
                {
                    int x = sprite.X;
                    int y = sprite.Y;

                    int w = source.Width;
                    int h = source.Height;

                   // context.Logger.LogImportantMessage(string.Format("Packing normal map {0} x,y: {1},{2} w,h: {3},{4} into {5}*{6}", sprite.SpriteName, x, y, w, h, output.Width, output.Height));
                    
                    // Copy the main sprite data to the output sheet.
                    PixelBitmapContent<NormalizedByte4>.Copy(source, new Rectangle(0, 0, w, h),
                                       output, new Rectangle(x + 1, y + 1, w, h));

                   // context.Logger.LogImportantMessage(string.Format("Adding strips..."));

                    // Copy a border strip from each edge of the sprite, creating
                    // a one pixel padding area to avoid filtering problems if the
                    // sprite is scaled or rotated.
                    PixelBitmapContent<NormalizedByte4>.Copy(source, new Rectangle(0, 0, 1, h),
                                       output, new Rectangle(x, y + 1, 1, h));

                    PixelBitmapContent<NormalizedByte4>.Copy(source, new Rectangle(w - 1, 0, 1, h),
                                       output, new Rectangle(x + w + 1, y + 1, 1, h));

                    PixelBitmapContent<NormalizedByte4>.Copy(source, new Rectangle(0, 0, w, 1),
                                       output, new Rectangle(x + 1, y, w, 1));

                    PixelBitmapContent<NormalizedByte4>.Copy(source, new Rectangle(0, h - 1, w, 1),
                                       output, new Rectangle(x + 1, y + h + 1, w, 1));

                  //  context.Logger.LogImportantMessage(string.Format("Adding corners..."));

                    // Copy a single pixel from each corner of the sprite,
                    // filling in the corners of the one pixel padding area.
                    PixelBitmapContent<NormalizedByte4>.Copy(source, new Rectangle(0, 0, 1, 1),
                                       output, new Rectangle(x, y, 1, 1));

                    PixelBitmapContent<NormalizedByte4>.Copy(source, new Rectangle(w - 1, 0, 1, 1),
                                       output, new Rectangle(x + w + 1, y, 1, 1));

                    PixelBitmapContent<NormalizedByte4>.Copy(source, new Rectangle(0, h - 1, 1, 1),
                                       output, new Rectangle(x, y + h + 1, 1, 1));

                    PixelBitmapContent<NormalizedByte4>.Copy(source, new Rectangle(w - 1, h - 1, 1, 1),
                                       output, new Rectangle(x + w + 1, y + h + 1, 1, 1));

                    // Remember where we placed this sprite.
                    if (outputSprites != null)
                    {
                        outputSprites.Add(new Rectangle(x + 1, y + 1, w, h));
                    }
                }
            }

            //return output;
        }

        /// <summary>
        /// Internal helper class keeps track of a sprite while it is being arranged.
        /// </summary>
        class ArrangedSprite
        {
            /// <summary>
            /// debugging only
            /// </summary>
            public string SpriteName; 

            public int Index;

            public int X;
            public int Y;

            public int Width;
            public int Height;
        }


        /// <summary>
        /// Works out where to position a single sprite.
        /// </summary>
        static void PositionSprite(List<ArrangedSprite> sprites,
                                   int index, int outputWidth)
        {
            int x = 0;
            int y = 0;

            while (true)
            {
                // Is this position free for us to use?
                int intersects = FindIntersectingSprite(sprites, index, x, y);

                if (intersects < 0)
                {
                    sprites[index].X = x;
                    sprites[index].Y = y;

                    return;
                }

                // Skip past the existing sprite that we collided with.
                x = sprites[intersects].X + sprites[intersects].Width;

                // If we ran out of room to move to the right,
                // try the next line down instead.
                if (x + sprites[index].Width > outputWidth)
                {
                    x = 0;
                    y++;
                }
            }
        }


        /// <summary>
        /// Checks if a proposed sprite position collides with anything
        /// that we already arranged.
        /// </summary>
        static int FindIntersectingSprite(List<ArrangedSprite> sprites,
                                          int index, int x, int y)
        {
            int w = sprites[index].Width;
            int h = sprites[index].Height;

            for (int i = 0; i < index; i++)
            {
                if (sprites[i].X >= x + w)
                    continue;

                if (sprites[i].X + sprites[i].Width <= x)
                    continue;

                if (sprites[i].Y >= y + h)
                    continue;

                if (sprites[i].Y + sprites[i].Height <= y)
                    continue;

                return i;
            }

            return -1;
        }


        /// <summary>
        /// Comparison function for sorting sprites by size.
        /// </summary>
        static int CompareSpriteSizes(ArrangedSprite a, ArrangedSprite b)
        {
            int aSize = a.Height * 1024 + a.Width;
            int bSize = b.Height * 1024 + b.Width;

            return bSize.CompareTo(aSize);
        }


        /// <summary>
        /// Comparison function for sorting sprites by their original indices.
        /// </summary>
        static int CompareSpriteIndices(ArrangedSprite a, ArrangedSprite b)
        {
            return a.Index.CompareTo(b.Index);
        }


        /// <summary>
        /// Heuristic guesses what might be a good output width for a list of sprites.
        /// </summary>
        static int GuessOutputWidth(IList<BitmapContent> sourceSprites)
        {
            int maxWidth = 0;
            int totalSize = 0;

            foreach (BitmapContent sprite in sourceSprites)
            {
                maxWidth = Math.Max(maxWidth, sprite.Width);
                totalSize += sprite.Width * sprite.Height;
            }

            int width = Math.Max((int)Math.Sqrt(totalSize), maxWidth);

            return RoundUpToPowerOfTwo(width);
        }


        /// <summary>
        /// Rounds a value up to the next larger power of two.
        /// </summary>
        static int RoundUpToPowerOfTwo(int value)
        {
            int powerOfTwo = 1;

            while (powerOfTwo < value)
                powerOfTwo <<= 1;

            return powerOfTwo;
        }
    }        
}
