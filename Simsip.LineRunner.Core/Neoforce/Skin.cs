////////////////////////////////////////////////////////////////
//                                                            //
//  Neoforce Controls                                         //
//                                                            //
////////////////////////////////////////////////////////////////
//                                                            //
//         File: Skin.cs                                      //
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

////////////////////////////////////////////////////////////////////////////
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System.Reflection;
#if NETFX_CORE
using System.Threading.Tasks;
#endif
////////////////////////////////////////////////////////////////////////////

#endregion

namespace TomShane.Neoforce.Controls
{

    #region //// Structs ///////////

    ////////////////////////////////////////////////////////////////////////////
    public struct SkinStates<T>
    {
        public T Enabled;
        public T Hovered;
        public T Pressed;
        public T Focused;
        public T Disabled;

        public SkinStates(T enabled, T hovered, T pressed, T focused, T disabled)
        {
            Enabled = enabled;
            Hovered = hovered;
            Pressed = pressed;
            Focused = focused;
            Disabled = disabled;
        }
    }
    ////////////////////////////////////////////////////////////////////////////   

    ////////////////////////////////////////////////////////////////////////////
    public struct LayerStates
    {
        public int Index;
        public Color Color;
        public bool Overlay;
    }
    ////////////////////////////////////////////////////////////////////////////

    ////////////////////////////////////////////////////////////////////////////
    public struct LayerOverlays
    {
        public int Index;
        public Color Color;
    }
    ////////////////////////////////////////////////////////////////////////////

    ////////////////////////////////////////////////////////////////////////////
    public struct SkinInfo
    {
        public string Name;
        public string Description;
        public string Author;
        public string Version;
    }
    ////////////////////////////////////////////////////////////////////////////

    #endregion

    #region //// Classes ///////////

    ////////////////////////////////////////////////////////////////////////////  
    public class SkinList<T> : List<T>
    {
        #region //// Indexers //////////

        ////////////////////////////////////////////////////////////////////////////
        public T this[string index]
        {
            get
            {
                for (int i = 0; i < this.Count; i++)
                {
                    SkinBase s = (SkinBase)(object)this[i];
                    if (s.Name.ToLower() == index.ToLower())
                    {
                        return this[i];
                    }
                }
                return default(T);
            }

            set
            {
                for (int i = 0; i < this.Count; i++)
                {
                    SkinBase s = (SkinBase)(object)this[i];
                    if (s.Name.ToLower() == index.ToLower())
                    {
                        this[i] = value;
                    }
                }
            }
        }
        ////////////////////////////////////////////////////////////////////////////

        #endregion

        #region //// Constructors //////


        ////////////////////////////////////////////////////////////////////////////
        public SkinList()
            : base()
        {
        }
        ////////////////////////////////////////////////////////////////////////////

        ////////////////////////////////////////////////////////////////////////////
        public SkinList(SkinList<T> source)
            : base()
        {
            for (int i = 0; i < source.Count; i++)
            {
                Type[] t = new Type[1];
                t[0] = typeof(T);

                object[] p = new object[1];
                p[0] = source[i];

#if NETFX_CORE
                var info = t[0].GetTypeInfo();
                var ctr = info.DeclaredConstructors.ToList()[0];
                this.Add( (T)ctr.Invoke(p) );
#else
                this.Add((T)t[0].GetConstructor(t).Invoke(p));
#endif
            }
        }
        ////////////////////////////////////////////////////////////////////////////

        #endregion
    }
    ////////////////////////////////////////////////////////////////////////////    

    //////////////////////////////////////////////////////////////////////////// 
    public class SkinBase
    {
        #region //// Fields ////////////

        ////////////////////////////////////////////////////////////////////////////
        public string Name;
        public bool Archive;
        ////////////////////////////////////////////////////////////////////////////

        #endregion

        #region //// Constructors //////

        ////////////////////////////////////////////////////////////////////////////
        public SkinBase()
            : base()
        {
            Archive = false;
        }
        ////////////////////////////////////////////////////////////////////////////    

        ////////////////////////////////////////////////////////////////////////////
        public SkinBase(SkinBase source)
            : base()
        {
            if (source != null)
            {
                this.Name = source.Name;
                this.Archive = source.Archive;
            }
        }
        ////////////////////////////////////////////////////////////////////////////

        #endregion
    }
    //////////////////////////////////////////////////////////////////////////// 

    ////////////////////////////////////////////////////////////////////////////
    public class SkinLayer : SkinBase
    {
        #region //// Fields ////////////

        ////////////////////////////////////////////////////////////////////////////
        public SkinImage Image = new SkinImage();
        public int Width;
        public int Height;
        public int OffsetX;
        public int OffsetY;
        public Alignment Alignment;
        public Margins SizingMargins;
        public Margins ContentMargins;
        public SkinStates<LayerStates> States;
        public SkinStates<LayerOverlays> Overlays;
        public SkinText Text = new SkinText();
        public SkinList<SkinAttribute> Attributes = new SkinList<SkinAttribute>();
        ////////////////////////////////////////////////////////////////////////////

        #endregion

        #region //// Constructors //////

        ////////////////////////////////////////////////////////////////////////////
        public SkinLayer()
            : base()
        {
            States.Enabled.Color = Color.White;
            States.Pressed.Color = Color.White;
            States.Focused.Color = Color.White;
            States.Hovered.Color = Color.White;
            States.Disabled.Color = Color.White;

            Overlays.Enabled.Color = Color.White;
            Overlays.Pressed.Color = Color.White;
            Overlays.Focused.Color = Color.White;
            Overlays.Hovered.Color = Color.White;
            Overlays.Disabled.Color = Color.White;
        }
        ////////////////////////////////////////////////////////////////////////////

        ////////////////////////////////////////////////////////////////////////////
        public SkinLayer(SkinLayer source)
            : base(source)
        {
            if (source != null)
            {
                this.Image = new SkinImage(source.Image);
                this.Width = source.Width;
                this.Height = source.Height;
                this.OffsetX = source.OffsetX;
                this.OffsetY = source.OffsetY;
                this.Alignment = source.Alignment;
                this.SizingMargins = source.SizingMargins;
                this.ContentMargins = source.ContentMargins;
                this.States = source.States;
                this.Overlays = source.Overlays;
                this.Text = new SkinText(source.Text);
                this.Attributes = new SkinList<SkinAttribute>(source.Attributes);
            }
            else
            {
                throw new Exception("Parameter for SkinLayer copy constructor cannot be null.");
            }
        }
        ////////////////////////////////////////////////////////////////////////////

        #endregion
    }
    //////////////////////////////////////////////////////////////////////////// 

    ////////////////////////////////////////////////////////////////////////////
    public class SkinText : SkinBase
    {
        #region //// Fields ////////////

        ////////////////////////////////////////////////////////////////////////////
        public SkinFont Font;
        public int OffsetX;
        public int OffsetY;
        public Alignment Alignment;
        public SkinStates<Color> Colors;
        ////////////////////////////////////////////////////////////////////////////

        #endregion

        #region //// Constructors //////

        ////////////////////////////////////////////////////////////////////////////
        public SkinText()
            : base()
        {
            Colors.Enabled = Color.White;
            Colors.Pressed = Color.White;
            Colors.Focused = Color.White;
            Colors.Hovered = Color.White;
            Colors.Disabled = Color.White;
        }
        ////////////////////////////////////////////////////////////////////////////

        ////////////////////////////////////////////////////////////////////////////
        public SkinText(SkinText source)
            : base(source)
        {
            if (source != null)
            {
                this.Font = new SkinFont(source.Font);
                this.OffsetX = source.OffsetX;
                this.OffsetY = source.OffsetY;
                this.Alignment = source.Alignment;
                this.Colors = source.Colors;
            }
        }
        ////////////////////////////////////////////////////////////////////////////

        #endregion
    }
    ////////////////////////////////////////////////////////////////////////////  

    ////////////////////////////////////////////////////////////////////////////
    public class SkinFont : SkinBase
    {
        #region //// Fields ////////////

        ////////////////////////////////////////////////////////////////////////////
        public SpriteFont Resource = null;
        public string Asset = null;
        public string Addon = null;
        ////////////////////////////////////////////////////////////////////////////

        #endregion

        #region //// Properties ////////

        ////////////////////////////////////////////////////////////////////////////
        public int Height
        {
            get
            {
                if (Resource != null)
                {
                    return (int)Resource.MeasureString("AaYy").Y;
                }
                return 0;
            }
        }
        ////////////////////////////////////////////////////////////////////////////

        #endregion

        #region //// Constructors //////

        ////////////////////////////////////////////////////////////////////////////
        public SkinFont()
            : base()
        {
        }
        ////////////////////////////////////////////////////////////////////////////

        ////////////////////////////////////////////////////////////////////////////
        public SkinFont(SkinFont source)
            : base(source)
        {
            if (source != null)
            {
                this.Resource = source.Resource;
                this.Asset = source.Asset;
            }
        }
        ////////////////////////////////////////////////////////////////////////////

        #endregion
    }
    ////////////////////////////////////////////////////////////////////////////  

    ////////////////////////////////////////////////////////////////////////////
    public class SkinImage : SkinBase
    {
        #region //// Fields ////////////

        ////////////////////////////////////////////////////////////////////////////
        public Texture2D Resource = null;
        public string Asset = null;
        public string Addon = null;
        ////////////////////////////////////////////////////////////////////////////

        #endregion

        #region //// Constructors //////

        ////////////////////////////////////////////////////////////////////////////
        public SkinImage()
            : base()
        {
        }
        ////////////////////////////////////////////////////////////////////////////

        ////////////////////////////////////////////////////////////////////////////
        public SkinImage(SkinImage source)
            : base(source)
        {
            this.Resource = source.Resource;
            this.Asset = source.Asset;
        }
        ////////////////////////////////////////////////////////////////////////////

        #endregion
    }
    ////////////////////////////////////////////////////////////////////////////

    ////////////////////////////////////////////////////////////////////////////
    public class SkinCursor : SkinBase
    {
        #region //// Fields ////////////


        public Cursor Resource = null;

        public string Asset = null;
        public string Addon = null;
        ////////////////////////////////////////////////////////////////////////////

        #endregion

        #region //// Constructors //////

        ////////////////////////////////////////////////////////////////////////////
        public SkinCursor()
            : base()
        {
        }
        ////////////////////////////////////////////////////////////////////////////

        ////////////////////////////////////////////////////////////////////////////
        public SkinCursor(SkinCursor source)
            : base(source)
        {
            this.Resource = source.Resource;

            this.Asset = source.Asset;
        }
        ////////////////////////////////////////////////////////////////////////////

        #endregion
    }
    ////////////////////////////////////////////////////////////////////////////

    ////////////////////////////////////////////////////////////////////////////
    public class SkinControl : SkinBase
    {
        #region //// Fields ////////////

        ////////////////////////////////////////////////////////////////////////////
        public string Inherits = null;
        public Size DefaultSize;
        public int ResizerSize;
        public Size MinimumSize;
        public Margins OriginMargins;
        public Margins ClientMargins;
        public SkinList<SkinLayer> Layers = new SkinList<SkinLayer>();
        public SkinList<SkinAttribute> Attributes = new SkinList<SkinAttribute>();
        ////////////////////////////////////////////////////////////////////////////

        #endregion

        #region //// Constructors //////

        ////////////////////////////////////////////////////////////////////////////
        public SkinControl()
            : base()
        {
        }
        ////////////////////////////////////////////////////////////////////////////

        ////////////////////////////////////////////////////////////////////////////
        public SkinControl(SkinControl source)
            : base(source)
        {
            this.Inherits = source.Inherits;
            this.DefaultSize = source.DefaultSize;
            this.MinimumSize = source.MinimumSize;
            this.OriginMargins = source.OriginMargins;
            this.ClientMargins = source.ClientMargins;
            this.ResizerSize = source.ResizerSize;
            this.Layers = new SkinList<SkinLayer>(source.Layers);
            this.Attributes = new SkinList<SkinAttribute>(source.Attributes);
        }
        ////////////////////////////////////////////////////////////////////////////

        #endregion
    }
    ////////////////////////////////////////////////////////////////////////////

    ////////////////////////////////////////////////////////////////////////////
    public class SkinAttribute : SkinBase
    {
        #region //// Fields ////////////

        ////////////////////////////////////////////////////////////////////////////
        public string Value;
        ////////////////////////////////////////////////////////////////////////////

        #endregion

        #region //// Constructors //////

        ////////////////////////////////////////////////////////////////////////////
        public SkinAttribute()
            : base()
        {
        }
        ////////////////////////////////////////////////////////////////////////////

        ////////////////////////////////////////////////////////////////////////////
        public SkinAttribute(SkinAttribute source)
            : base(source)
        {
            this.Value = source.Value;
        }
        ////////////////////////////////////////////////////////////////////////////

        #endregion
    }
    //////////////////////////////////////////////////////////////////////////// 

    //////////////////////////////////////////////////////////////////////////// 
    public class Skin : Component
    {
        #region //// Fields ////////////

        ////////////////////////////////////////////////////////////////////////////           
        SkinXmlDocument doc = null;
        private string name = null;
        private Version version = null;
        private SkinInfo info;
        private SkinList<SkinControl> controls = null;
        private SkinList<SkinFont> fonts = null;
        private SkinList<SkinCursor> cursors = null;
        private SkinList<SkinImage> images = null;
        private SkinList<SkinAttribute> attributes = null;
        private ArchiveManager content = null;
        ////////////////////////////////////////////////////////////////////////////

        #endregion

        #region //// Properties ////////

        ////////////////////////////////////////////////////////////////////////////
        public virtual string Name { get { return name; } }
        public virtual Version Version { get { return version; } }
        public virtual SkinInfo Info { get { return info; } }
        public virtual SkinList<SkinControl> Controls { get { return controls; } }
        public virtual SkinList<SkinFont> Fonts { get { return fonts; } }
        public virtual SkinList<SkinCursor> Cursors { get { return cursors; } }
        public virtual SkinList<SkinImage> Images { get { return images; } }
        public virtual SkinList<SkinAttribute> Attributes { get { return attributes; } }
        ////////////////////////////////////////////////////////////////////////////        

        #endregion

        #region //// Construstors //////

        ////////////////////////////////////////////////////////////////////////////       
        public Skin(Manager manager, string name)
            : base(manager)
        {
            this.name = name;
#if !NETFX_CORE
            content = new ArchiveManager(Manager.Game.Services, GetArchiveLocation(name + Manager.SkinExtension));
            content.RootDirectory = GetFolder();
            doc = new SkinXmlDocument(name);
            controls = new SkinList<SkinControl>();
            fonts = new SkinList<SkinFont>();
            images = new SkinList<SkinImage>();
            cursors = new SkinList<SkinCursor>();
            attributes = new SkinList<SkinAttribute>();

            LoadSkin(null, content.UseArchive);

            string folder = GetAddonsFolder();
            if (folder == "")
            {
                content.UseArchive = true;
                folder = "Addons\\";
            }
            else
            {
                content.UseArchive = false;
            }

            string[] addons = content.GetDirectories(folder);

            if (addons != null && addons.Length > 0)
            {
                for (int i = 0; i < addons.Length; i++)
                {
                    DirectoryInfo d = new DirectoryInfo(GetAddonsFolder() + addons[i]);
                    if (!((d.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden) || content.UseArchive)
                    {
                        LoadSkin(addons[i].Replace("\\", ""), content.UseArchive);
                    }
                }
            }
#endif
        }

#if NETFX_CORE
        public static async Task<Skin> FactorySkin(Manager manager, string name)
        {
            var skin = new Skin(manager, name);
            await skin.FactorySkinInitialize(name);

            return skin;
        }

        private async Task FactorySkinInitialize(string name)
        {
            content = await ArchiveManager.FactoryArchiveManager(Manager.Game.Services, GetArchiveLocation(name + Manager.SkinExtension));
            content.RootDirectory = GetFolder();
            doc = new SkinXmlDocument(name);
            controls = new SkinList<SkinControl>();
            fonts = new SkinList<SkinFont>();
            images = new SkinList<SkinImage>();
            cursors = new SkinList<SkinCursor>();
            attributes = new SkinList<SkinAttribute>();

            LoadSkin(null, content.UseArchive);

            string folder = GetAddonsFolder();
            if (folder == "")
            {
                content.UseArchive = true;
                folder = "Addons\\";
            }
            else
            {
                content.UseArchive = false;
            }

            /* TODO: Simsip
            string[] addons = await content.GetDirectories(folder);
            
            if (addons != null && addons.Length > 0)
            {
                for (int i = 0; i < addons.Length; i++)
                {
                    DirectoryInfo d = new DirectoryInfo(GetAddonsFolder() + addons[i]);
                    if (!((d.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden) || content.UseArchive)
                    {
                        LoadSkin(addons[i].Replace("\\", ""), content.UseArchive);
                    }
                }
            }
            */
        }
#endif
        ////////////////////////////////////////////////////////////////////////////

        #endregion

        #region //// Destructors ///////

        ////////////////////////////////////////////////////////////////////////////
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (content != null)
                {
                    content.Unload();
                    content.Dispose();
                    content = null;
                }
            }

            base.Dispose(disposing);
        }
        ////////////////////////////////////////////////////////////////////////////

        #endregion

        #region //// Methods ///////////

        ////////////////////////////////////////////////////////////////////////////     
        private string GetArchiveLocation(string name)
        {
#if !NETFX_CORE
            string path = Path.GetFullPath(Manager.SkinDirectory) + Path.GetFileNameWithoutExtension(name) + "\\";
            if (!Directory.Exists(path) || !File.Exists(path + "Skin.xnb"))
            {
                path = Path.GetFullPath(Manager.SkinDirectory) + name;
                return path;
            }
#endif
            return null;
        }
        ////////////////////////////////////////////////////////////////////////////

        ////////////////////////////////////////////////////////////////////////////     
        private string GetFolder()
        {
            string path = "";

#if !NETFX_CORE
            path = Path.GetFullPath(Manager.SkinDirectory) + name + "\\";
            if (!Directory.Exists(path) || !File.Exists(path + "Skin.xnb"))
            {
                path = "";
            }
#endif

            return path;
        }
        ////////////////////////////////////////////////////////////////////////////

        ////////////////////////////////////////////////////////////////////////////     
        private string GetAddonsFolder()
        {
            string path = "";

#if !NETFX_CORE
            path = Path.GetFullPath(Manager.SkinDirectory) + name + "\\Addons\\";
            if (!Directory.Exists(path))
            {
                path = Path.GetFullPath(".\\Content\\Skins\\") + name + "\\Addons\\";
                if (!Directory.Exists(path))
                {
                    path = Path.GetFullPath(".\\Skins\\") + name + "\\Addons\\";
                }
            }
#endif

            return path;
        }
        ////////////////////////////////////////////////////////////////////////////

        ////////////////////////////////////////////////////////////////////////////     
        private string GetFolder(string type)
        {
            return GetFolder() + type + "\\";
        }
        ////////////////////////////////////////////////////////////////////////////     

        ////////////////////////////////////////////////////////////////////////////     
        private string GetAsset(string type, string asset, string addon)
        {
            string ret = GetFolder(type) + asset;
            if (addon != null && addon != "")
            {
                ret = GetAddonsFolder() + addon + "\\" + type + "\\" + asset;
            }
            return ret;
        }
        ////////////////////////////////////////////////////////////////////////////     

        ////////////////////////////////////////////////////////////////////////////
        public override void Init()
        {
            base.Init();

#if !NETFX_CORE
            for (int i = 0; i < fonts.Count; i++)
            {
                content.UseArchive = fonts[i].Archive;
                string asset = GetAsset("Fonts", fonts[i].Asset, fonts[i].Addon);
                asset = content.UseArchive ? asset : Path.GetFullPath(asset);
                (fonts[i].Resource) = content.Load<SpriteFont>(asset);
            }

            for (int i = 0; i < cursors.Count; i++)
            {
                content.UseArchive = cursors[i].Archive;
                string asset = GetAsset("Cursors", cursors[i].Asset, cursors[i].Addon);
                asset = content.UseArchive ? asset : Path.GetFullPath(asset);
                cursors[i].Resource = content.Load<Cursor>(asset);
            }

            for (int i = 0; i < images.Count; i++)
            {
                content.UseArchive = images[i].Archive;
                string asset = GetAsset("Images", images[i].Asset, images[i].Addon);
                asset = content.UseArchive ? asset : Path.GetFullPath(asset);
                images[i].Resource = content.Load<Texture2D>(asset);
            }
#endif

            for (int i = 0; i < controls.Count; i++)
            {
                for (int j = 0; j < controls[i].Layers.Count; j++)
                {
                    if (controls[i].Layers[j].Image.Name != null)
                    {
                        controls[i].Layers[j].Image = images[controls[i].Layers[j].Image.Name];
                    }
                    else
                    {
                        controls[i].Layers[j].Image = images[0];
                    }

                    if (controls[i].Layers[j].Text.Name != null)
                    {
                        controls[i].Layers[j].Text.Font = fonts[controls[i].Layers[j].Text.Name];
                    }
                    else
                    {
                        controls[i].Layers[j].Text.Font = fonts[0];
                    }
                }
            }
        }
        ////////////////////////////////////////////////////////////////////////////

        ////////////////////////////////////////////////////////////////////////////         
        private string ReadAttribute(XElement element, string attrib, string defval, bool needed)
        {
            if (element != null && 
                (string)element.Attribute(attrib) != null)
            {
                return (string)element.Attribute(attrib).Value;
            }
            else if (needed)
            {
                throw new Exception("Missing required attribute \"" + attrib + "\" in the skin file.");
            }
            return defval;
        }
        ////////////////////////////////////////////////////////////////////////////     

        ////////////////////////////////////////////////////////////////////////////         
        private void ReadAttribute(ref string retval, bool inherited, XElement element, string attrib, string defval, bool needed)
        {
            if (element != null && 
                (string)element.Attribute(attrib) != null)
            {
                retval = (string)element.Attribute(attrib).Value;
            }
            else if (inherited)
            {
            }
            else if (needed)
            {
                throw new Exception("Missing required attribute \"" + attrib + "\" in the skin file.");
            }
            else
            {
                retval = defval;
            }
        }
        ////////////////////////////////////////////////////////////////////////////     

        ////////////////////////////////////////////////////////////////////////////     
        private int ReadAttributeInt(XElement element, string attrib, int defval, bool needed)
        {
            return int.Parse(ReadAttribute(element, attrib, defval.ToString(), needed));
        }
        ////////////////////////////////////////////////////////////////////////////     

        ////////////////////////////////////////////////////////////////////////////     
        private void ReadAttributeInt(ref int retval, bool inherited, XElement element, string attrib, int defval, bool needed)
        {
            string tmp = retval.ToString();
            ReadAttribute(ref tmp, inherited, element, attrib, defval.ToString(), needed);
            retval = int.Parse(tmp);
        }
        ////////////////////////////////////////////////////////////////////////////     

        ////////////////////////////////////////////////////////////////////////////     
        private bool ReadAttributeBool(XElement element, string attrib, bool defval, bool needed)
        {
            return bool.Parse(ReadAttribute(element, attrib, defval.ToString(), needed));
        }
        ////////////////////////////////////////////////////////////////////////////     

        ////////////////////////////////////////////////////////////////////////////     
        private void ReadAttributeBool(ref bool retval, bool inherited, XElement element, string attrib, bool defval, bool needed)
        {
            string tmp = retval.ToString();
            ReadAttribute(ref tmp, inherited, element, attrib, defval.ToString(), needed);
            retval = bool.Parse(tmp);
        }
        ////////////////////////////////////////////////////////////////////////////     

        ////////////////////////////////////////////////////////////////////////////     
        private byte ReadAttributeByte(XElement element, string attrib, byte defval, bool needed)
        {
            return byte.Parse(ReadAttribute(element, attrib, defval.ToString(), needed));
        }
        ////////////////////////////////////////////////////////////////////////////     

        ////////////////////////////////////////////////////////////////////////////     
        private void ReadAttributeByte(ref byte retval, bool inherited, XElement element, string attrib, byte defval, bool needed)
        {
            string tmp = retval.ToString();
            ReadAttribute(ref tmp, inherited, element, attrib, defval.ToString(), needed);
            retval = byte.Parse(tmp);
        }
        ////////////////////////////////////////////////////////////////////////////     

        ////////////////////////////////////////////////////////////////////////////     
        private string ColorToString(Color c)
        {
            return string.Format("{0};{1};{2};{3}", c.R, c.G, c.B, c.A);
        }
        ////////////////////////////////////////////////////////////////////////////     

        ////////////////////////////////////////////////////////////////////////////     
        private void ReadAttributeColor(ref Color retval, bool inherited, XElement element, string attrib, Color defval, bool needed)
        {
            string tmp = ColorToString(retval);
            ReadAttribute(ref tmp, inherited, element, attrib, ColorToString(defval), needed);
            retval = Utilities.ParseColor(tmp);
        }
        //////////////////////////////////////////////////////////////////////////// 


        ////////////////////////////////////////////////////////////////////////////     
        private void LoadSkin(string addon, bool archive)
        {
#if !NETFX_CORE
            try
            {
                bool isaddon = addon != null && addon != "";
                string file = GetFolder();
                if (isaddon)
                {
                    file = GetAddonsFolder() + addon + "\\";
                }
                file += "Skin";

                file = archive ? file : Path.GetFullPath(file);
                doc = content.Load<SkinXmlDocument>(file);

                var e = (from el in doc.Descendants("Skin")
                            select el).First();


                // XElement e = doc("Skin");
                if (e != null)
                {
                    string xname = ReadAttribute(e, "Name", null, true);
                    if (!isaddon)
                    {
                        if (name.ToLower() != xname.ToLower())
                        {
                            throw new Exception("Skin name defined in the skin file doesn't match requested skin.");
                        }
                        else
                        {
                            name = xname;
                        }
                    }
                    else
                    {
                        if (addon.ToLower() != xname.ToLower())
                        {
                            throw new Exception("Skin name defined in the skin file doesn't match addon name.");
                        }
                    }

                    Version xversion = null;
                    try
                    {
                        xversion = new Version(ReadAttribute(e, "Version", "0.0.0.0", false));
                    }
                    catch (Exception x)
                    {
                        throw new Exception("Unable to resolve skin file version. " + x.Message);
                    }

                    if (xversion != Manager._SkinVersion)
                    {
                        throw new Exception("This version of Neoforce Controls can only read skin files in version of " + Manager._SkinVersion.ToString() + ".");
                    }
                    else if (!isaddon)
                    {
                        version = xversion;
                    }

                    if (!isaddon)
                    {
                        XElement ei = (from el in e.Descendants("Info")
                            select el).First();
                        if (ei != null)
                        {
                            if (ei.Element("Name") != null) info.Name = (string)ei.Element("Name");
                            if (ei.Element("Description") != null) info.Description = (string)ei.Element("Description");
                            if (ei.Element("Author") != null) info.Author = (string)ei.Element("Author");
                            if (ei.Element("Version") != null) info.Version = (string)ei.Element("Version");
                        }
                    }

                    LoadImages(addon, archive);
                    LoadFonts(addon, archive);
                    LoadCursors(addon, archive);
                    LoadSkinAttributes();
                    LoadControls();
                }
            }
            catch (Exception x)
            {
                throw new Exception("Unable to load skin file. " + x.Message);
            }
#endif
        }
        ////////////////////////////////////////////////////////////////////////////        

        ////////////////////////////////////////////////////////////////////////////        
        private void LoadSkinAttributes()
        {
            var l = from el in doc
                        .Element("Skin")
                        .Element("Attributes")
                        .Elements("Attribute")
                    select el;

            foreach (XElement e in l)
            {
                SkinAttribute sa = new SkinAttribute();
                sa.Name = ReadAttribute(e, "Name", null, true);
                sa.Value = ReadAttribute(e, "Value", null, true);
                attributes.Add(sa);
            }
        }
        ////////////////////////////////////////////////////////////////////////////        

        ////////////////////////////////////////////////////////////////////////////        
        private void LoadControls()
        {
            var l = from el in doc
                        .Element("Skin")
                        .Element("Controls")
                        .Elements("Control")
                    select el;

            foreach (XElement e in l)
            {
                SkinControl sc = null;
                string parent = ReadAttribute(e, "Inherits", null, false);
                bool inh = false;

                if (parent != null)
                {
                    sc = new SkinControl(controls[parent]);
                    sc.Inherits = parent;
                    inh = true;
                }
                else
                {
                    sc = new SkinControl();
                }

                ReadAttribute(ref sc.Name, inh, e, "Name", null, true);

                ReadAttributeInt(ref sc.DefaultSize.Width, inh, e.Element("DefaultSize"), "Width", 0, false);
                ReadAttributeInt(ref sc.DefaultSize.Height, inh, e.Element("DefaultSize"), "Height", 0, false);

                ReadAttributeInt(ref sc.MinimumSize.Width, inh, e.Element("MinimumSize"), "Width", 0, false);
                ReadAttributeInt(ref sc.MinimumSize.Height, inh, e.Element("MinimumSize"), "Height", 0, false);

                ReadAttributeInt(ref sc.OriginMargins.Left, inh, e.Element("OriginMargins"), "Left", 0, false);
                ReadAttributeInt(ref sc.OriginMargins.Top, inh, e.Element("OriginMargins"), "Top", 0, false);
                ReadAttributeInt(ref sc.OriginMargins.Right, inh, e.Element("OriginMargins"), "Right", 0, false);
                ReadAttributeInt(ref sc.OriginMargins.Bottom, inh, e.Element("OriginMargins"), "Bottom", 0, false);

                ReadAttributeInt(ref sc.ClientMargins.Left, inh, e.Element("ClientMargins"), "Left", 0, false);
                ReadAttributeInt(ref sc.ClientMargins.Top, inh, e.Element("ClientMargins"), "Top", 0, false);
                ReadAttributeInt(ref sc.ClientMargins.Right, inh, e.Element("ClientMargins"), "Right", 0, false);
                ReadAttributeInt(ref sc.ClientMargins.Bottom, inh, e.Element("ClientMargins"), "Bottom", 0, false);

                ReadAttributeInt(ref sc.ResizerSize, inh, e.Element("ResizerSize"), "Value", 0, false);

                if (e.Element("Layers") != null)
                {
                    var l2 = from el in e
                                 .Element("Layers")
                                 .Elements("Layer")
                             select el;
                    if (l2.Count() > 0)
                    {
                        LoadLayers(sc, l2);
                    }
                }
                if (e.Element("Attributes") != null)
                {
                    var  l3 = from el in e
                                  .Element("Attributes")
                                  .Elements("Attribute")
                              select el;
                    if (l3.Count() > 0)
                    {
                        LoadControlAttributes(sc, l3);
                    }
                }
                controls.Add(sc);
            }
        }
        ////////////////////////////////////////////////////////////////////////////   

        ////////////////////////////////////////////////////////////////////////////        
        private void LoadFonts(string addon, bool archive)
        {
            if (doc.Element("Skin").Element("Fonts") == null) return;

            var l = from el in doc
                        .Element("Skin")
                        .Element("Fonts")
                        .Elements("Font")
                    select el;
            if (l.Count() > 0)
            {
                foreach (XElement e in l)
                {
                    SkinFont sf = new SkinFont();
                    sf.Name = ReadAttribute(e, "Name", null, true);
                    sf.Archive = archive;
                    sf.Asset = ReadAttribute(e, "Asset", null, true);
                    sf.Addon = addon;
                    fonts.Add(sf);
                }
            }
        }
        ////////////////////////////////////////////////////////////////////////////        

        ////////////////////////////////////////////////////////////////////////////        
        private void LoadCursors(string addon, bool archive)
        {
            if (doc.Element("Skin").Element("Cursors") == null) return;

            var l = from el in doc
                        .Element("Skin")
                        .Element("Cursors")
                        .Elements("Cursor")
                    select el;
            if (l.Count() > 0)
            {
                foreach (XElement e in l)
                {
                    SkinCursor sc = new SkinCursor();
                    sc.Name = ReadAttribute(e, "Name", null, true);
                    sc.Archive = archive;
                    sc.Asset = ReadAttribute(e, "Asset", null, true);
                    sc.Addon = addon;
                    cursors.Add(sc);
                }
            }
        }
        ////////////////////////////////////////////////////////////////////////////          

        ////////////////////////////////////////////////////////////////////////////
        private void LoadImages(string addon, bool archive)
        {
            if (doc.Element("Skin").Element("Images") == null) return;

            var l = from el in doc
                        .Element("Skin")
                        .Element("Images")
                        .Elements("Image")
                    select el;
            if (l.Count() > 0)
            {
                foreach (XElement e in l)
                {
                    SkinImage si = new SkinImage();
                    si.Name = ReadAttribute(e, "Name", null, true);
                    si.Archive = archive;
                    si.Asset = ReadAttribute(e, "Asset", null, true);
                    si.Addon = addon;
                    images.Add(si);
                }
            }
        }
        ////////////////////////////////////////////////////////////////////////////         

        ////////////////////////////////////////////////////////////////////////////        
        private void LoadLayers(SkinControl sc, IEnumerable<XElement> l)
        {
            foreach (XElement e in l)
            {
                string name = ReadAttribute(e, "Name", null, true);
                bool over = ReadAttributeBool(e, "Override", false, false);
                SkinLayer sl = sc.Layers[name];
                bool inh = true;

                if (sl == null)
                {
                    sl = new SkinLayer();
                    inh = false;
                }

                if (inh && over)
                {
                    sl = new SkinLayer();
                    sc.Layers[name] = sl;
                }

                ReadAttribute(ref sl.Name, inh, e, "Name", null, true);
                ReadAttribute(ref sl.Image.Name, inh, e, "Image", "Control", false);
                ReadAttributeInt(ref sl.Width, inh, e, "Width", 0, false);
                ReadAttributeInt(ref sl.Height, inh, e, "Height", 0, false);

                string tmp = sl.Alignment.ToString();
                ReadAttribute(ref tmp, inh, e, "Alignment", "MiddleCenter", false);
                sl.Alignment = (Alignment)Enum.Parse(typeof(Alignment), tmp, true);

                ReadAttributeInt(ref sl.OffsetX, inh, e, "OffsetX", 0, false);
                ReadAttributeInt(ref sl.OffsetY, inh, e, "OffsetY", 0, false);

                ReadAttributeInt(ref sl.SizingMargins.Left, inh, e.Element("SizingMargins"), "Left", 0, false);
                ReadAttributeInt(ref sl.SizingMargins.Top, inh, e.Element("SizingMargins"), "Top", 0, false);
                ReadAttributeInt(ref sl.SizingMargins.Right, inh, e.Element("SizingMargins"), "Right", 0, false);
                ReadAttributeInt(ref sl.SizingMargins.Bottom, inh, e.Element("SizingMargins"), "Bottom", 0, false);

                ReadAttributeInt(ref sl.ContentMargins.Left, inh, e.Element("ContentMargins"), "Left", 0, false);
                ReadAttributeInt(ref sl.ContentMargins.Top, inh, e.Element("ContentMargins"), "Top", 0, false);
                ReadAttributeInt(ref sl.ContentMargins.Right, inh, e.Element("ContentMargins"), "Right", 0, false);
                ReadAttributeInt(ref sl.ContentMargins.Bottom, inh, e.Element("ContentMargins"), "Bottom", 0, false);

                if (e.Element("States") != null)
                {
                    ReadAttributeInt(ref sl.States.Enabled.Index, inh, e.Element("States").Element("Enabled"), "Index", 0, false);
                    int di = sl.States.Enabled.Index;
                    ReadAttributeInt(ref sl.States.Hovered.Index, inh, e.Element("States").Element("Hovered"), "Index", di, false);
                    ReadAttributeInt(ref sl.States.Pressed.Index, inh, e.Element("States").Element("Pressed"), "Index", di, false);
                    ReadAttributeInt(ref sl.States.Focused.Index, inh, e.Element("States").Element("Focused"), "Index", di, false);
                    ReadAttributeInt(ref sl.States.Disabled.Index, inh, e.Element("States").Element("Disabled"), "Index", di, false);

                    ReadAttributeColor(ref sl.States.Enabled.Color, inh, e.Element("States").Element("Enabled"), "Color", Color.White, false);
                    Color dc = sl.States.Enabled.Color;
                    ReadAttributeColor(ref sl.States.Hovered.Color, inh, e.Element("States").Element("Hovered"), "Color", dc, false);
                    ReadAttributeColor(ref sl.States.Pressed.Color, inh, e.Element("States").Element("Pressed"), "Color", dc, false);
                    ReadAttributeColor(ref sl.States.Focused.Color, inh, e.Element("States").Element("Focused"), "Color", dc, false);
                    ReadAttributeColor(ref sl.States.Disabled.Color, inh, e.Element("States").Element("Disabled"), "Color", dc, false);

                    ReadAttributeBool(ref sl.States.Enabled.Overlay, inh, e.Element("States").Element("Enabled"), "Overlay", false, false);
                    bool dv = sl.States.Enabled.Overlay;
                    ReadAttributeBool(ref sl.States.Hovered.Overlay, inh, e.Element("States").Element("Hovered"), "Overlay", dv, false);
                    ReadAttributeBool(ref sl.States.Pressed.Overlay, inh, e.Element("States").Element("Pressed"), "Overlay", dv, false);
                    ReadAttributeBool(ref sl.States.Focused.Overlay, inh, e.Element("States").Element("Focused"), "Overlay", dv, false);
                    ReadAttributeBool(ref sl.States.Disabled.Overlay, inh, e.Element("States").Element("Disabled"), "Overlay", dv, false);
                }

                if (e.Element("Overlays") != null)
                {
                    ReadAttributeInt(ref sl.Overlays.Enabled.Index, inh, e.Element("Overlays").Element("Enabled"), "Index", 0, false);
                    int di = sl.Overlays.Enabled.Index;
                    ReadAttributeInt(ref sl.Overlays.Hovered.Index, inh, e.Element("Overlays").Element("Hovered"), "Index", di, false);
                    ReadAttributeInt(ref sl.Overlays.Pressed.Index, inh, e.Element("Overlays").Element("Pressed"), "Index", di, false);
                    ReadAttributeInt(ref sl.Overlays.Focused.Index, inh, e.Element("Overlays").Element("Focused"), "Index", di, false);
                    ReadAttributeInt(ref sl.Overlays.Disabled.Index, inh, e.Element("Overlays").Element("Disabled"), "Index", di, false);

                    ReadAttributeColor(ref sl.Overlays.Enabled.Color, inh, e.Element("Overlays").Element("Enabled"), "Color", Color.White, false);
                    Color dc = sl.Overlays.Enabled.Color;
                    ReadAttributeColor(ref sl.Overlays.Hovered.Color, inh, e.Element("Overlays").Element("Hovered"), "Color", dc, false);
                    ReadAttributeColor(ref sl.Overlays.Pressed.Color, inh, e.Element("Overlays").Element("Pressed"), "Color", dc, false);
                    ReadAttributeColor(ref sl.Overlays.Focused.Color, inh, e.Element("Overlays").Element("Focused"), "Color", dc, false);
                    ReadAttributeColor(ref sl.Overlays.Disabled.Color, inh, e.Element("Overlays").Element("Disabled"), "Color", dc, false);
                }

                if (e.Element("Text") != null)
                {
                    ReadAttribute(ref sl.Text.Name, inh, e.Element("Text"), "Font", null, true);
                    ReadAttributeInt(ref sl.Text.OffsetX, inh, e.Element("Text"), "OffsetX", 0, false);
                    ReadAttributeInt(ref sl.Text.OffsetY, inh, e.Element("Text"), "OffsetY", 0, false);

                    tmp = sl.Text.Alignment.ToString();
                    ReadAttribute(ref tmp, inh, e.Element("Text"), "Alignment", "MiddleCenter", false);
                    sl.Text.Alignment = (Alignment)Enum.Parse(typeof(Alignment), tmp, true);

                    LoadColors(inh, e.Element("Text"), ref sl.Text.Colors);
                }
                if (e.Element("Attributes") != null)
                {
                    var l2 = from el in e
                                 .Element("Attributes")
                                 .Elements("Attribute")
                             select el;
                    if (l2.Count() > 0)
                    {
                        LoadLayerAttributes(sl, l2);
                    }
                }
                if (!inh) sc.Layers.Add(sl);
            }
        }
        ////////////////////////////////////////////////////////////////////////////                            

        ////////////////////////////////////////////////////////////////////////////        
        private void LoadColors(bool inherited, XElement e, ref SkinStates<Color> colors)
        {
            if (e != null)
            {
                ReadAttributeColor(ref colors.Enabled, inherited, e.Element("Colors").Element("Enabled"), "Color", Color.White, false);
                ReadAttributeColor(ref colors.Hovered, inherited, e.Element("Colors").Element("Hovered"), "Color", colors.Enabled, false);
                ReadAttributeColor(ref colors.Pressed, inherited, e.Element("Colors").Element("Pressed"), "Color", colors.Enabled, false);
                ReadAttributeColor(ref colors.Focused, inherited, e.Element("Colors").Element("Focused"), "Color", colors.Enabled, false);
                ReadAttributeColor(ref colors.Disabled, inherited, e.Element("Colors").Element("Disabled"), "Color", colors.Enabled, false);
            }
        }
        ////////////////////////////////////////////////////////////////////////////            

        ////////////////////////////////////////////////////////////////////////////        
        private void LoadControlAttributes(SkinControl sc, IEnumerable<XElement> l)
        {
            foreach (XElement e in l)
            {
                string name = ReadAttribute(e, "Name", null, true);
                SkinAttribute sa = sc.Attributes[name];
                bool inh = true;

                if (sa == null)
                {
                    sa = new SkinAttribute();
                    inh = false;
                }

                sa.Name = name;
                ReadAttribute(ref sa.Value, inh, e, "Value", null, true);

                if (!inh) sc.Attributes.Add(sa);
            }
        }
        ////////////////////////////////////////////////////////////////////////////   

        ////////////////////////////////////////////////////////////////////////////        
        private void LoadLayerAttributes(SkinLayer sl, IEnumerable<XElement> l)
        {
            foreach (XElement e in l)
            {
                string name = ReadAttribute(e, "Name", null, true);
                SkinAttribute sa = sl.Attributes[name];
                bool inh = true;

                if (sa == null)
                {
                    sa = new SkinAttribute();
                    inh = false;
                }

                sa.Name = name;
                ReadAttribute(ref sa.Value, inh, e, "Value", null, true);

                if (!inh) sl.Attributes.Add(sa);
            }
        }
        ////////////////////////////////////////////////////////////////////////////  

        #endregion
    }
    //////////////////////////////////////////////////////////////////////////// 

    #endregion

}
