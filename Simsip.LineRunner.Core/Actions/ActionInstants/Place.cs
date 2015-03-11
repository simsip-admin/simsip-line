using Microsoft.Xna.Framework;
using Cocos2D;
using Simsip.LineRunner.GameObjects;

namespace Simsip.LineRunner.Actions
{
    public class Place : ActionInstant
    {
        private Vector3 m_tPosition;

        protected Place()
        {
        }

        protected Place(Place place) : base(place)
        {
            InitWithPosition(m_tPosition);
        }

        public Place(Vector3 pos)
        {
            InitWithPosition(pos);
        }

        protected virtual bool InitWithPosition(Vector3 pos)
        {
            m_tPosition = pos;
            return true;
        }

        public override object Copy(ICopyable pZone)
        {
            if (pZone != null)
            {
                var pRet = (Place) (pZone);
                base.Copy(pZone);
                pRet.InitWithPosition(m_tPosition);
                return pRet;
            }
            return new Place(this);
        }

        protected internal override void StartWithTarget(GameModel target)
        {
            base.StartWithTarget(target);
            m_pTarget.WorldOrigin = m_tPosition;
        }
    }
}