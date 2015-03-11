using Cocos2D;


namespace Simsip.LineRunner.Utils
{
    /// <summary>
    /// Abstracting away font utilities in case we need this level of indirection in future.
    /// (.e.g., i18n)
    /// </summary>
    public class FontUtils
    {
        #region Api

        public static CCLabelBMFont CreateCCLabelBMFont(string text, string font)
        {
            return new CCLabelBMFont(text, FormatFontFilePath(font));
        }

        #endregion


        #region Helper methods

        private static string FormatFontFilePath(string font)
        {
            // string fntfile = System.IO.Path.Combine("fonts", font);
            string fntfile = "Fonts/" + font;
            return fntfile;
        }

        #endregion

    }
}