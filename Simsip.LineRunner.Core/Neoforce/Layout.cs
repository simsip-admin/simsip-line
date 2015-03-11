////////////////////////////////////////////////////////////////
//                                                            //
//  Neoforce Controls                                         //
//                                                            //
////////////////////////////////////////////////////////////////
//                                                            //
//         File: Layout.cs                                    //
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
using Microsoft.Xna.Framework;
using System.Linq;
using System.Xml.Linq;
using System.Reflection;
using System.IO;
using Microsoft.Xna.Framework.Graphics;
////////////////////////////////////////////////////////////////////////////

#endregion

namespace TomShane.Neoforce.Controls
{ 


  public static class Layout
  {

    #region //// Fields ////////////

    ////////////////////////////////////////////////////////////////////////////                     
    ////////////////////////////////////////////////////////////////////////////

    #endregion

    #region //// Properties ////////

    ////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////

    #endregion

    #region //// Construstors //////

    ////////////////////////////////////////////////////////////////////////////       
    ////////////////////////////////////////////////////////////////////////////

    #endregion
    
    #region //// Methods ///////////    
    
    ////////////////////////////////////////////////////////////////////////////    
    public static Container Load(Manager manager, string asset)
    {
      Container win = null;
      LayoutXmlDocument doc = new LayoutXmlDocument("layout");
      ArchiveManager content = new ArchiveManager(manager.Game.Services);            
      
      try
      {      
        content.RootDirectory = manager.LayoutDirectory;
        
        string file = content.RootDirectory + asset;
        
        // TODO: SIMSiP
        /*
        if (File.Exists(file))
        {
          doc.Load(file);
        }
        else
        {
          doc = content.Load<LayoutXmlDocument>(asset);
        }
        */
        doc = content.Load<LayoutXmlDocument>(asset);
          
        
        if (doc != null && 
            doc.Element("Layout").Element("Controls") != null && 
            doc.Element("Layout").Element("Controls").HasElements)
        {       
          var node = doc.Element("Layout").Element("Controls").Element("Control");         
          string cls = (string)node.Attribute("Class");
          Type type = Type.GetType(cls);
          
          if (type == null)
          {
            cls = "TomShane.Neoforce.Controls." + cls;
            type = Type.GetType(cls);
          }
                    
          win = (Container)LoadControl(manager, node, type, null);                    
        }  
        
      }
      finally
      {
        content.Dispose();
      }                  
      
      return win;
    }
    ////////////////////////////////////////////////////////////////////////////

    ////////////////////////////////////////////////////////////////////////////
    private static Control LoadControl(Manager manager, XElement node, Type type, Control parent)
    {
      Control c = null;   
      
      Object[] args = new Object[] {manager};

#if NETFX_CORE
      var info = type.GetTypeInfo();
      var ctor = info.DeclaredConstructors.ToList()[0];
      c = (Control)ctor.Invoke(args);
#else
      c = (Control)type.InvokeMember(null, BindingFlags.CreateInstance, null, null, args);    
#endif
      if (parent != null) c.Parent = parent;
      c.Name = (string)node.Attribute("Name");            
      
      if (node != null && 
          node.Element("Properties") != null && 
          node.Element("Properties").HasElements)
      {
        var l = from el in node
                    .Element("Properties")
                    .Elements("Property")
                select el;

        LoadProperties(l, c);
      }  

      if (node != null && 
          node.Element("Controls") != null && 
          node.Element("Controls").HasElements)
      {
          var l = from el in node
                  .Element("Controls")
                  .Elements("Control")
                  select el;
        foreach (XElement e in l)
        {
          string cls = e.Attribute("Class").Value;
          Type t = Type.GetType(cls);
          
          if (t == null)
          {
            cls = "TomShane.Neoforce.Controls." + cls;
            t = Type.GetType(cls);
          }
          LoadControl(manager, e, t, c);
        }
      }                    
      
      return c;
    }
    ////////////////////////////////////////////////////////////////////////////

    ////////////////////////////////////////////////////////////////////////////
    private static void LoadProperties(IEnumerable<XElement> node, Control c)        
    {
      foreach (XElement e in node)
      {
        string name = e.Attribute("Name").Value;
        string val = e.Attribute("Value").Value;
#if NETFX_CORE
        var properties = c.GetType().GetRuntimeProperties();

        PropertyInfo i = (from p in properties
                         where p.Name == name
                         select p)
                         .FirstOrDefault();
#else
        PropertyInfo i = c.GetType().GetProperty(name);
#endif

        if (i != null)
        {                                       
          {  
            try
            {                          
              i.SetValue(c, Convert.ChangeType(val, i.PropertyType, null), null);
            }
            catch
            {              
            }  
          }  
        }                              
      }  
    }
    ////////////////////////////////////////////////////////////////////////////

    #endregion  

  }

}
