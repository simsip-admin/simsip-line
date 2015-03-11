using Cocos2D;

namespace Simsip.LineRunner.Utils
{
    public class CCTintToWithAlpha : CCTintTo
    {
        protected byte m_fromOpacity;
        protected byte m_toOpacity;

        public CCTintToWithAlpha(float duration, byte red, byte green, byte blue, ICCRGBAProtocol target)
            : base(duration, red, green, blue)
        {
            this.m_pTarget = target as CCNode;
        }

        protected CCTintToWithAlpha(CCTintToWithAlpha tintTo) : base(tintTo)
        {
            InitWithDuration(tintTo.m_fDuration, tintTo.m_to.R, tintTo.m_to.G, tintTo.m_to.B);
            m_fromOpacity = tintTo.m_fromOpacity;
            m_toOpacity = tintTo.m_toOpacity;
        }

        public override object Copy(ICCCopyable zone)
        {
            if (zone != null && zone != null)
            {
                var ret = zone as CCTintToWithAlpha;
                if (ret == null)
                {
                    return null;
                }

                base.Copy(zone);

                ret.InitWithDuration(m_fDuration, m_to.R, m_to.G, m_to.B);
                ret.m_fromOpacity = m_fromOpacity;
                ret.m_toOpacity = m_toOpacity;

                return ret;
            }
            else
            {
                return new CCTintToWithAlpha(this);
            }
        }

        public byte Opacity 
        { 
            set
            {
                var protocol = m_pTarget as ICCRGBAProtocol;
                m_fromOpacity = protocol.Opacity;
                m_toOpacity = value;
            }
        }

        public override void Update(float time)
        {
            var protocol = m_pTarget as ICCRGBAProtocol;
            if (protocol != null)
            {
                protocol.Color = new CCColor3B((byte) (m_from.R + (m_to.R - m_from.R) * time),
                                               (byte) (m_from.G + (m_to.G - m_from.G) * time),
                                               (byte) (m_from.B + (m_to.B - m_from.B) * time));

                protocol.Opacity = (byte)(m_fromOpacity + (m_toOpacity - m_fromOpacity) * time);
            }
        }
    }
}