using EmberaEngine.Engine.Core;
using EmberaEngine.Engine.Utilities;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmberaEngine.Engine.Components
{
    public class ButtonComponent : Component
    {

        public override string Type => nameof(ButtonComponent);

        public event Action OnButtonPress;


        
        public ButtonComponent() { }
        private RectTransform RectTransform;
        private CanvasComponent canvasComponent;

        public override void OnStart()
        {
            RectTransform = UIManager.AddUIComponent(ref gameObject);
            canvasComponent = IterateGetParentComponent(gameObject) ?? gameObject.scene.GetComponent<CanvasComponent>();

        }

        CanvasComponent IterateGetParentComponent(GameObject gameObject)
        {
            CanvasComponent component = gameObject.GetComponent<CanvasComponent>();

            if (component != null)
            {
                return component;
            }
            else if (gameObject.parentObject != null)
            {
                return IterateGetParentComponent(gameObject.parentObject);
            }

            return null;
        }

        public override void OnUpdate(float dt)
        {

            Vector2 mousePos = Input.GetMousePos();

            if (canvasComponent.innerMousePos.X > RectTransform.Position.X && canvasComponent.innerMousePos.Y > RectTransform.Position.Y)
            {
                if (canvasComponent.innerMousePos.X < RectTransform.Position.X + RectTransform.Size.X && canvasComponent.innerMousePos.Y < RectTransform.Position.Y + RectTransform.Size.Y)
                {
                    if (Input.GetMouseButtonDown() == MouseButtonEvent.Left)
                    {
                        OnButtonPress?.Invoke();
                    }
                }
            }
        }

    }
}
