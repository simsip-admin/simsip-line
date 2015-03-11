////////////////////////////////////////////////////////////////
//                                                            //
//  Neoforce Importers                                        //
//                                                            //
////////////////////////////////////////////////////////////////
//                                                            //
//         File: SkinImporter.cs                              //
//                                                            //
//      Version: 0.7                                          //
//                                                            //
//         Date: 15/02/2010                                   //
//                                                            //
//       Author: Tom Shane                                    //
//                                                            //
////////////////////////////////////////////////////////////////
//                                                            //
//  Skin file importer.                                       //
//                                                            //
////////////////////////////////////////////////////////////////


#region //// Using /////////////

using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
////////////////////////////////////////////////////////////////////////////
using System.Xml.Linq;
////////////////////////////////////////////////////////////////////////////

#endregion


namespace TomShane.Neoforce.Importers
{ 

  #region //// Importer //////////
  
  ////////////////////////////////////////////////////////////////////////////
  public class SkinXmlDocument : XElement 
  {
      public SkinXmlDocument(XElement content)
          : base(content)
      { 
      }
  }
  ////////////////////////////////////////////////////////////////////////////

  ////////////////////////////////////////////////////////////////////////////
  [ContentImporter(".xml", DisplayName = "Skin - Neoforce Controls")]
  class SkinImporter: ContentImporter<SkinXmlDocument>
  {
		#region //// Methods ///////////
			
		////////////////////////////////////////////////////////////////////////////
		public override SkinXmlDocument Import(string filename, ContentImporterContext context)
        {
            var xmlContent = XElement.Load(filename);
            SkinXmlDocument doc = new SkinXmlDocument(xmlContent);

            return doc;
        }
		////////////////////////////////////////////////////////////////////////////
			
  	#endregion  
  }  	
  ////////////////////////////////////////////////////////////////////////////

  #endregion

  #region //// Writer ////////////

  ////////////////////////////////////////////////////////////////////////////
  [ContentTypeWriter]
  class SkinWriter:ContentTypeWriter<SkinXmlDocument>
  {

    #region //// Methods ///////////
    
    ////////////////////////////////////////////////////////////////////////////
    protected override void Write(ContentWriter output, SkinXmlDocument value)
    {      
      output.Write(value.ToString());              
    }
    ///////////////////////////////////////////////////////////////////////////
    
    ////////////////////////////////////////////////////////////////////////////
    public override string GetRuntimeType(TargetPlatform targetPlatform)
    {
      return typeof(SkinXmlDocument).AssemblyQualifiedName;
    }
    ////////////////////////////////////////////////////////////////////////////
    
    ////////////////////////////////////////////////////////////////////////////    
    public override string GetRuntimeReader(TargetPlatform targetPlatform)
    {      
      if (targetPlatform == TargetPlatform.Xbox360)
      {
        return "TomShane.Neoforce.Controls.SkinReader, TomShane.Neoforce.Controls.360";
      } 
      else
      {
        return "TomShane.Neoforce.Controls.SkinReader, TomShane.Neoforce.Controls";
      } 
    }      
    ////////////////////////////////////////////////////////////////////////////

    #endregion
  } 
  ////////////////////////////////////////////////////////////////////////////

  #endregion
  
}