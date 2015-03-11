////////////////////////////////////////////////////////////////
//                                                            //
//  Neoforce Controls                                         //
//                                                            //
////////////////////////////////////////////////////////////////
//                                                            //
//         File: ContentReaders.cs                            //
//                                                            //
//      Version: 0.7                                          //
//                                                            //
//         Date: 11/09/2010                                   //
//                                                            //
//       Author: Tom Shane                                    //
//                                                            //
////////////////////////////////////////////////////////////////
//                                                            //
//  Copyright (c) by Tom Shane                                //
//                                                            //
////////////////////////////////////////////////////////////////

#region //// Using /////////////

//////////////////////////////////////////////////////////////////////////////
using System;
using System.IO;
using System.Xml.Linq;
using Microsoft.Xna.Framework.Content;

#if (!XBOX && !XBOX_FAKE)
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
#endif
//////////////////////////////////////////////////////////////////////////////

#endregion

namespace TomShane.Neoforce.Controls
{

    ////////////////////////////////////////////////////////////////////////////
    public class LayoutXmlDocument : XElement 
    {
        public LayoutXmlDocument(string name)
            : base(name)
        {
        }
        public LayoutXmlDocument(XElement input)
            : base(input)
        {
        }
    }

    public class SkinXmlDocument : XElement 
    {
        public SkinXmlDocument(string name)
            : base(name)
        {

        }

        public SkinXmlDocument(XElement input)
            : base(input)
        {
        }
    }
    ////////////////////////////////////////////////////////////////////////////


    public class SkinReader : ContentTypeReader<SkinXmlDocument>
    {

        #region //// Methods ///////////

        ////////////////////////////////////////////////////////////////////////////
        protected override SkinXmlDocument Read(ContentReader input, SkinXmlDocument existingInstance)
        {
            if (existingInstance == null)
            {
                var parsedXml = XElement.Parse(input.ReadString());
                SkinXmlDocument doc = new SkinXmlDocument(parsedXml);
                return doc;
            }
            else
            {
                var parsedXml = XElement.Parse(input.ReadString());
                existingInstance = new SkinXmlDocument(parsedXml);
            }

            return existingInstance;
        }
        ////////////////////////////////////////////////////////////////////////////

        #endregion

    }

    public class LayoutReader : ContentTypeReader<LayoutXmlDocument>
    {

        #region //// Methods ///////////

        ////////////////////////////////////////////////////////////////////////////
        protected override LayoutXmlDocument Read(ContentReader input, LayoutXmlDocument existingInstance)
        {
            if (existingInstance == null)
            {
                var parsedXml = XElement.Parse(input.ReadString());
                LayoutXmlDocument doc = new LayoutXmlDocument(parsedXml);
                return doc;
            }
            else
            {
                var parsedXml = XElement.Parse(input.ReadString());
                existingInstance = new LayoutXmlDocument(parsedXml);
            }

            return existingInstance;
        }
        ////////////////////////////////////////////////////////////////////////////

        #endregion

    }

#if (!XBOX && !XBOX_FAKE)

    /* TODO
    public class CursorReader : ContentTypeReader<Cursor>
    {

        #region //// Methods ///////////

        ////////////////////////////////////////////////////////////////////////////
        protected override Cursor Read(ContentReader input, Cursor existingInstance)
        {
            if (existingInstance == null)
            {
                int count = input.ReadInt32();
                byte[] data = input.ReadBytes(count);

                string path = Path.GetTempFileName();
                File.WriteAllBytes(path, data);
                string tPath = Path.GetTempFileName();
                using(System.Drawing.Icon i = System.Drawing.Icon.ExtractAssociatedIcon(path))
                {
                    using (System.Drawing.Bitmap b = i.ToBitmap())
                    {

                        b.Save(tPath, System.Drawing.Imaging.ImageFormat.Png);
                        b.Dispose();
                    }
                    
                    i.Dispose();
                }
                //TODO: Replace with xml based solution for getting hotspot and size instead
                IntPtr handle = NativeMethods.LoadCursor(path);
                System.Windows.Forms.Cursor c = new System.Windows.Forms.Cursor(handle);
                Vector2 hs = new Vector2(c.HotSpot.X, c.HotSpot.Y);
                int w = c.Size.Width;
                int h = c.Size.Height;
                c.Dispose();
                File.Delete(path);

                return new Cursor(tPath, hs, w, h);
            }
            else
            {
            }

            return existingInstance;
        }
        ////////////////////////////////////////////////////////////////////////////


    }
    */

#endif

}

