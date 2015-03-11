using System;

namespace Simsip.LineRunner.Actions
{
    public class CallFunc : ActionInstant
    {
        private System.Action m_pCallFunc;
        protected string m_scriptFuncName;

        public CallFunc()
        {
            m_scriptFuncName = "";
            m_pCallFunc = null;
        }

        public CallFunc(System.Action selector) : base()
        {
            m_pCallFunc = selector;
        }

        protected CallFunc(CallFunc callFunc) : base(callFunc)
        {
            m_pCallFunc = callFunc.m_pCallFunc;
            m_scriptFuncName = callFunc.m_scriptFuncName;
        }

        public virtual void Execute()
        {
            if (null != m_pCallFunc)
            {
                m_pCallFunc();
            }
            //if (m_nScriptHandler) {
            //    CCScriptEngineManager::sharedManager()->getScriptEngine()->executeCallFuncActionEvent(this);
            //}
        }

        public override void Update(float time)
        {
            Execute();
        }

        public override object Copy(ICopyable pZone)
        {
            if (pZone != null)
            {
                //in case of being called at sub class
                var pRet = (CallFunc) (pZone);
                base.Copy(pZone);
                pRet.m_pCallFunc = m_pCallFunc;
                pRet.m_scriptFuncName = m_scriptFuncName;
                return pRet;
            }
            else
            {
                return new CallFunc(this);
            }
        }
    }
}