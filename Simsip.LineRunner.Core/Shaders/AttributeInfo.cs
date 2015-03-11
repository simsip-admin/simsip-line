using OpenTK.Graphics.ES20;
using System;


namespace Simsip.LineRunner.Shaders
{
    public class AttributeInfo
    {
        public String name = "";
        public int address = -1;
        public int size = 0;
        public All type;
        // for ES30: public ActiveAttribType type;
    }
}