using Simsip.LineRunner.GameObjects;
using System;


namespace Simsip.LineRunner.Actions
{
    public class CallFuncN : CallFunc
    {
        private Action<GameModel> m_pCallFuncN;

        public CallFuncN() : base()
        {
            m_pCallFuncN = null;
        }


        public CallFuncN(Action<GameModel> selector)
        {
            InitWithTarget(selector);
        }

        public CallFuncN(CallFuncN callFuncN) : base(callFuncN)
        {
            InitWithTarget(callFuncN.m_pCallFuncN);
        }

        public bool InitWithTarget(Action<GameModel> selector)
        {
            m_pCallFuncN = selector;
            return false;
        }

        public override object Copy(ICopyable zone)
        {
            if (zone != null)
            {
                //in case of being called at sub class
                var pRet = (CallFuncN) (zone);
                base.Copy(zone);

                pRet.InitWithTarget(m_pCallFuncN);

                return pRet;
            }
            else
            {
                return new CallFuncN(this);
            }
        }

        public override void Execute()
        {
            if (null != m_pCallFuncN)
            {
                m_pCallFuncN(m_pTarget);
            }
            //if (m_nScriptHandler) {
            //    CCScriptEngineManager::sharedManager()->getScriptEngine()->executeFunctionWithobject(m_nScriptHandler, m_pTarget, "CCNode");
            //}
        }
    }
}