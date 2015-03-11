using Cocos2D;
using System;


namespace Simsip.LineRunner.GameFramework
{
    public class KeyboardNotificationLayer : CCLayer
    {
        protected CCTextFieldTTF _trackNode;
        protected CCPoint _beginPos;

        public KeyboardNotificationLayer(CCTextFieldTTF textField)
        {
            base.TouchEnabled = true;

            _trackNode = textField;

            this.AddChild(_trackNode);
        }

        public CCTextFieldTTF TextField
        {
            get
            {
                return _trackNode;
            }
        }

        //
        // CCLayer
        //
        public override void RegisterWithTouchDispatcher()
        {
            CCDirector.SharedDirector.TouchDispatcher.AddTargetedDelegate(this, 0, false);
        }

        public override bool TouchBegan(CCTouch pTouch)
        {
            _beginPos = pTouch.Location;
            return true;
        }

        public override void TouchEnded(CCTouch pTouch)
        {
            if (_trackNode == null)
            {
                return;
            }

            var endPos = pTouch.Location;

            if (_trackNode.BoundingBox.ContainsPoint(_beginPos) && 
                _trackNode.BoundingBox.ContainsPoint(endPos))
            {
                OnClickTrackNode(true);
            }
            else
            {
                OnClickTrackNode(false);
            }
        }

        public virtual void OnClickTrackNode(bool bClicked)
        {
            if (bClicked && _trackNode != null)
            {
                _trackNode.Edit("CCTextFieldTTF test", "Enter text");
            }
        }

    }
}