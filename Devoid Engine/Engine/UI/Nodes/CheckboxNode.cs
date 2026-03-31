using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace DevoidEngine.Engine.UI.Nodes
{
    class CheckboxInner : ContainerNode
    {
        public override string ThemeType => "CheckboxInner";
    }

    class CheckboxOuter : ContainerNode
    {
        public override string ThemeType => "CheckboxOuter";
    }

    public class CheckboxNode : FlexboxNode
    {
        public bool _value;

        public bool Value
        {
            get => _value;
            set
            {
                _value = value;
                checkboxThumb.Visible = _value;
                OnValueChanged?.Invoke(_value);
            }
        }

        public Action<bool> OnValueChanged;

        ContainerNode checkboxBG;
        ContainerNode checkboxThumb;

        public CheckboxNode()
        {
            BlockInput = true;

            checkboxBG = new CheckboxOuter();
            checkboxThumb = new CheckboxInner();

            Layout.FlexGrowMain = 0;
            Layout.FlexGrowCross = 0;

            checkboxThumb.Visible = _value;

            checkboxThumb.MinSize = new Vector2(20);
            checkboxBG.MinSize = new Vector2(20);
        }

        protected override void InitializeCore()
        {
            base.InitializeCore();


            checkboxBG.Size = Size;
            //checkboxThumb.MaxSize = MaxSize * 0.9f;

            checkboxBG.Layout.FlexGrowMain = 1;
            checkboxThumb.Layout.FlexGrowMain = 1;

            checkboxBG.Padding = Padding.GetAll(5);

            Add(checkboxBG);
            checkboxBG.Add(checkboxThumb);
        }

        public override void OnClick()
        {
            Value = !Value;
        }

        
    }
}
