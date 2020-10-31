﻿using System;
using System.Linq;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.IO;

namespace RaaiVan.Modules.GlobalUtilities
{
    public class RVGraphics
    {
        public static Color get_color(string strColor)
        {
            //strColor can be like "#a4f2bb:50" (50 is opacity) or "r.g.b:opacity" or "red" (Color Name)

            if (string.IsNullOrEmpty(strColor)) return Color.Empty;

            int r = 0, g = 0, b = 0, alpha = 255;
            int index = strColor.IndexOf(':');
            int opacity = index < 0 ? 100 : int.Parse(strColor.Substring(index));
            alpha = (opacity * alpha) / 100;

            if (strColor.First() == '#')
            {
                r = Convert.ToInt32(strColor.Substring(1, 2), 16);
                g = Convert.ToInt32(strColor.Substring(3, 2), 16);
                b = Convert.ToInt32(strColor.Substring(5, 2), 16);
            }
            else if (strColor.IndexOf('.') >= 0)
            {
                int firstDotIndex = strColor.IndexOf('.');
                int lastDotIndex = strColor.LastIndexOf('.');

                r = int.Parse(strColor.Substring(0, firstDotIndex));
                g = int.Parse(strColor.Substring(firstDotIndex, lastDotIndex - firstDotIndex));
                b = int.Parse(strColor.Substring(lastDotIndex, index - lastDotIndex));
            }
            else
                return Color.FromName(strColor);

            return Color.FromArgb(alpha, r, g, b);
        }

        public static void strings_array_to_image(ref string[] strArray, int width, int height, bool ltr,
            string fontFamily, int fontSize, bool bold, Color backgroundColor, Color brushColor, 
            ref string contentType, ref string responseText)
        {
            Font desiredFont = new Font(fontFamily == string.Empty ? "Tahoma" : fontFamily, fontSize <= 0 ? 10 : fontSize,
                bold ? FontStyle.Bold : FontStyle.Regular);
            Brush desiredColor = brushColor == Color.Empty ? Brushes.Black : new SolidBrush(brushColor);
            Bitmap bmp = new Bitmap(1, 1);
            Graphics gfx = Graphics.FromImage(bmp);

            int maxWidth = 0;
            if (width <= 0)
            {
                for (int i = 0; i < strArray.Length; ++i)
                {
                    int tempWidth = (int)gfx.MeasureString(strArray[i], desiredFont).Width;
                    if (tempWidth > maxWidth) maxWidth = tempWidth;
                }
            }
            else maxWidth = width;

            int maxHeight = 0;
            if (height <= 0)
            {
                for (int i = 0; i < strArray.Length; ++i)
                {
                    int tempHeight = (int)gfx.MeasureString(strArray[i], desiredFont).Height;
                    if (tempHeight > maxHeight) maxHeight = tempHeight;
                }
            }
            else maxHeight = height;

            contentType = "image/png";
            bmp = new Bitmap(maxWidth + 1, maxHeight * strArray.Length);
            gfx = Graphics.FromImage(bmp);

            float curY = 0;
            foreach (string str in strArray)
            {
                int itemWidth = (int)gfx.MeasureString(str, desiredFont).Width;
                if (backgroundColor != Color.Empty)
                    gfx.FillRectangle(new SolidBrush(backgroundColor), 0, curY, maxWidth, maxHeight);
                float _x = 0;
                if (!ltr) _x = maxWidth - itemWidth;
                gfx.DrawString(str, desiredFont, desiredColor, _x, curY);
                curY += maxHeight;
            }

            System.IO.MemoryStream strm = new System.IO.MemoryStream();
            bmp.Save(strm, ImageFormat.Png);

            responseText = "data:image/png;base64," + Convert.ToBase64String(strm.ToArray());
        }

        public static bool make_thumbnail(Image image, int width, int height, int minWidth, int minHeight,
            ref Image retImage, ref string message, bool stretch = false)
        {
            int newWidth = width;
            int newHeight = height;

            if (!stretch && minWidth > 0 && minHeight > 0 && (image.Width < minWidth || image.Height < minHeight))
            {
                message = "{\"ErrorText\":\"" + Messages.ImageSizeIsNotValid.ToString() + "\"}";
                return false;
            }

            //new size recognition
            if (stretch || image.Width > newWidth || image.Height > newHeight)
            {
                int curWidth = image.Width;
                int curHeight = image.Height;

                double coeff = (double)newWidth / (double)curWidth;
                if ((int)((double)curHeight * coeff) > newHeight) coeff = (double)newHeight / (double)curHeight;

                newWidth = (int)(coeff * curWidth);
                newHeight = (int)(coeff * curHeight);

                if (minWidth > 0 && minHeight > 0 && (newWidth < minWidth || newHeight < minHeight))
                {
                    coeff = newWidth < minWidth ? (double)minWidth / (double)newWidth : (double)minHeight / (double)newHeight;
                    newWidth = (int)(coeff * (double)newWidth);
                    newHeight = (int)(coeff * (double)newHeight);

                    int x = 0, y = 0;
                    if (newWidth > width) x = (newWidth - width) / 2;
                    if (newHeight > height) y = (newHeight - height) / 2;

                    coeff = (double)curWidth / (double)newWidth; //equals to: (double)curHeight / (double)newHeight
                    x = (int)(x * coeff);
                    y = (int)(y * coeff);

                    Rectangle rect = new Rectangle(x, y,
                        (int)(Math.Min(width, newWidth) * coeff) - (2 * x), (int)(Math.Min(height, newHeight) * coeff) - (2 * y));

                    bool result = true;

                    using (Bitmap target = new Bitmap(rect.Width, rect.Height))
                    {
                        using (Graphics g = Graphics.FromImage(target))
                        {
                            g.DrawImage(image, new Rectangle(0, 0, target.Width, target.Height), rect, GraphicsUnit.Pixel);

                            result = make_thumbnail(target as Image, width, height, 0, 0, ref retImage, ref message, true);
                        }

                        return result;
                    }
                }
            }
            else
            {
                newWidth = image.Width;
                newHeight = image.Height;
            }
            //end of new size recognition

            //retImage = image.GetThumbnailImage(width, height, new Image.GetThumbnailImageAbort(ThumbnailCallback), IntPtr.Zero);
            Bitmap bmp = new Bitmap(newWidth, newHeight);
            using (Graphics gr = Graphics.FromImage(bmp))
            {
                gr.SmoothingMode = SmoothingMode.HighQuality;
                gr.InterpolationMode = InterpolationMode.HighQualityBicubic;
                gr.PixelOffsetMode = PixelOffsetMode.HighQuality;
                gr.DrawImage(image, new Rectangle(0, 0, newWidth, newHeight));
            }

            retImage = bmp as Image;

            return true;
        }

        public static bool make_thumbnail(Guid? applicationId, DocFileInfo sourceFile, DocFileInfo destFile,
            int width, int height, int minWidth, int minHeight, ref string errorMessage,
            string forceExtension = null, bool stretch = false)
        {
            if (!string.IsNullOrEmpty(forceExtension)) destFile.Extension = forceExtension;

            string sourceAddress = sourceFile.get_real_address(applicationId);
            if (string.IsNullOrEmpty(sourceAddress)) return false;

            Image retImage = null;

            try
            {
                bool sourceEncrypted = sourceFile.is_encrypted(applicationId);

                using (MemoryStream stream = !sourceEncrypted ? null :
                    new MemoryStream(DocumentUtilities.decrypt_file_aes(sourceAddress)))
                {
                    using (Image img = !sourceEncrypted ? Image.FromFile(sourceAddress) : Bitmap.FromStream(stream))
                    {
                        bool result = make_thumbnail(img, width, height, minWidth, minHeight, 
                            ref retImage, ref errorMessage, stretch);

                        if (img != null) img.Dispose();

                        if (!result)
                        {
                            if (retImage != null) retImage.Dispose();
                            if (img != null) retImage.Dispose();

                            return false;
                        }
                        
                        using (FileStream fileStream = 
                            new FileStream(destFile.get_address(applicationId, encrypted: false), FileMode.Create))
                        {
                            retImage.Save(fileStream, ImageFormat.Jpeg);
                            retImage.Dispose();
                            img.Dispose();
                        }

                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public static bool extract_thumbnail(Guid? applicationId, DocFileInfo sourceFile, DocFileInfo destFile, int x, int y,
            int width, int height, int thumbnailWidth, int thumbnailHeight, ref Image retImage, ref string message)
        {
            try
            {
                string sourceAddress = sourceFile.get_real_address(applicationId);
                if (string.IsNullOrEmpty(sourceAddress)) return false;

                bool result = true;

                bool sourceEncrypted = sourceFile.is_encrypted(applicationId);

                using (MemoryStream stream = !sourceEncrypted ? null :
                    new MemoryStream(DocumentUtilities.decrypt_file_aes(sourceAddress)))
                {
                    using (Bitmap image = (!sourceEncrypted ? Image.FromFile(sourceAddress) :
                        Bitmap.FromStream(stream)) as Bitmap)
                    {
                        if (image.Width < thumbnailWidth || image.Height < thumbnailHeight)
                        {
                            message = "{\"ErrorText\":\"" + Messages.ImageSizeIsNotValid.ToString() + "\"}";
                            image.Dispose();
                            return false;
                        }

                        if (x < 0 || y < 0 || width <= 0 || height <= 0)
                        {
                            double aspectRatio = (double)thumbnailWidth / (double)thumbnailHeight;
                            int min = ((double)image.Width / (double)thumbnailWidth) < ((double)image.Height / (double)thumbnailHeight) ?
                                image.Width : image.Height;

                            if (min == image.Width)
                            {
                                width = image.Width;
                                height = (int)Math.Floor((double)width / aspectRatio);
                            }
                            else
                            {
                                height = image.Height;
                                width = (int)Math.Floor((double)height * aspectRatio);
                            }

                            x = (image.Width - width) / 2;
                            y = (image.Height - height) / 2;
                        }

                        message = "{\"X\":" + x.ToString() + ",\"Y\":" + y.ToString() +
                            ",\"Width\":" + width.ToString() + ",\"Height\":" + height.ToString() +
                            ",\"HighQualityImageURL\":\"" + sourceFile.get_client_address(applicationId) + "\"" +
                            ",\"ImageURL\":\"" + destFile.get_client_address(applicationId) + "\"" +
                        "}";

                        Rectangle rect = new Rectangle(x, y, width, height);

                        using (Bitmap target = new Bitmap(rect.Width, rect.Height))
                        {
                            Graphics g = Graphics.FromImage(target);
                            g.DrawImage(image, new Rectangle(0, 0, target.Width, target.Height), rect, GraphicsUnit.Pixel);

                            result = make_thumbnail(target as Image,
                                thumbnailWidth, thumbnailHeight, 0, 0, ref retImage, ref message, true);
                        }
                        
                        if (!result) message = "{\"ErrorText\":\"" + Messages.OperationFailed + "\"}";
                        return result;
                    }
                }
            }
            catch { message = "{\"ErrorText\":\"" + Messages.OperationFailed + "\"}"; return false; }
        }

        public static bool extract_thumbnail(Guid? applicationId, DocFileInfo sourceFile, DocFileInfo destFile,
            int x, int y, int width, int height, int thumbnailWidth, int thumbnailHeight, ref string message)
        {
            try
            {
                string sourcePath = sourceFile.get_real_address(applicationId);
                if (string.IsNullOrEmpty(sourcePath)) return false;

                destFile.Extension = "jpg";
                string destPath = destFile.get_address(applicationId);

                if (!string.IsNullOrEmpty(sourceFile.Extension) && sourceFile.Extension != "jpg")
                {
                    sourceFile.Extension = "jpg";
                    string newPath = sourceFile.get_address(applicationId);
                    
                    bool sourceEncrypted = sourceFile.is_encrypted(applicationId);

                    using (MemoryStream stream = !sourceEncrypted ? null :
                        new MemoryStream(DocumentUtilities.decrypt_file_aes(sourcePath)))
                    {
                        using (Image img = !sourceEncrypted ? Image.FromFile(sourcePath) : Bitmap.FromStream(stream))
                        {
                            img.Save(newPath);
                            img.Dispose();

                            File.Delete(sourcePath);
                            sourcePath = newPath;
                        }
                    }
                }

                Image retImage = null;
                if (extract_thumbnail(applicationId, sourceFile, destFile, x, y, width, height,
                    thumbnailWidth, thumbnailHeight, ref retImage, ref message))
                {
                    FileStream fileStream = new FileStream(destFile.get_address(applicationId), FileMode.Create);
                    retImage.Save(fileStream, ImageFormat.Jpeg);
                    fileStream.Close();
                    fileStream.Dispose();
                    retImage.Dispose();
                }

                return true;
            }
            catch
            {
                message = "{\"ErrorText\":\"" + Messages.OperationFailed.ToString() + "\"}";
                return false;
            }
        }

        public static bool create_icon(Guid? applicationId, Guid iconId, IconType iconType,
            DocFileInfo uploadedFile, ref string errorMessage)
        {
            int width = 100, height = 100, highQualityWidth = 600, highQualityHeight = 600;

            FolderNames imageFolder = FolderNames.ProfileImages,
                highQualityImageFolder = FolderNames.HighQualityProfileImage;

            bool isValid = DocumentUtilities.get_icon_parameters(iconType, ref width, ref height,
                ref highQualityWidth, ref highQualityHeight, ref imageFolder, ref highQualityImageFolder);

            if (!isValid) return false;

            DocFileInfo highQualityFile = new DocFileInfo() { FileID = iconId, Extension = "jpg" };
            highQualityFile.set_folder_name(applicationId, highQualityImageFolder);

            DocFileInfo file = new DocFileInfo() { FileID = iconId, Extension = "jpg" };
            file.set_folder_name(applicationId, imageFolder);

            string highQualityFolderPath = highQualityFile.get_folder_address(applicationId);
            string folderPath = file.get_folder_address(applicationId);

            if (!Directory.Exists(highQualityFolderPath)) Directory.CreateDirectory(highQualityFolderPath);
            if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

            bool succeed = RVGraphics.make_thumbnail(applicationId,
                uploadedFile, highQualityFile, highQualityWidth, highQualityHeight,
                width, height, ref errorMessage, highQualityFile.Extension);

            return succeed && RVGraphics.extract_thumbnail(applicationId, highQualityFile, file,
                -1, -1, -1, -1, width, height, ref errorMessage);
        }
    }
}
