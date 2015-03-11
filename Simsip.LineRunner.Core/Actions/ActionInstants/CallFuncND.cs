using Simsip.LineRunner.GameObjects;
using System;


namespace Simsip.LineRunner.Actions
{
    public class CallFuncND : CallFuncN
    {
        protected Action<GameModel, object> m_pCallFuncND;
        protected object m_pData;

        public CallFuncND(Action<GameModel, object> selector, object d) : base()
        {
            InitWithTarget(selector, d);
        }

        public CallFuncND(CallFuncND callFuncND) : base(callFuncND)
        {
            InitWithTarget(callFuncND.m_pCallFuncND, callFuncND.m_pData);
        }

        public bool InitWithTarget(Action<GameModel, object> selector, object d)
        {
            m_pData = d;
            m_pCallFuncND = selector;
            return true;
        }

        public override object Copy(ICopyable zone)
        {
            if (zone != null)
            {
                //in case of being called at sub class
                var pRet = (CallFuncND) (zone);
                base.Copy(zone);
                pRet.InitWithTarget(m_pCallFuncND, m_pData);
                return pRet;
            }
            else
            {
                return new CallFuncND(this);
            }
        }

        public override void Execute()
        {
            if (null != m_pCallFuncND)
            {
                m_pCallFuncND(m_pTarget, m_pData);
            }

            //if (CCScriptEngineManager::sharedScriptEngineManager()->getScriptEngine()) {
            //    CCScriptEngineManager::sharedScriptEngineManager()->getScriptEngine()->executeCallFuncND(
            //            m_scriptFuncName.c_str(), m_pTarget, m_pData);
            //}
        }
    }
}