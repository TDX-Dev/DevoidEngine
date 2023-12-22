using EmberaEngine.Engine.Core;
using EmberaEngine.Engine.Rendering;
using System;
using System.Collections.Generic;
using OpenTK.Mathematics;
using EmberaEngine.Engine.Utilities;

namespace EmberaEngine.Engine.Components
{
    public class SpriteRenderer : Component
    {
        public override string Type => nameof(SpriteRenderer);

        public Texture Sprite = Texture.GetWhite2D();
        public Vector4 SolidColor = Vector4.Zero;
        public float Rotation;
        public int sortingOrder = 0;
        public bool centerSprite;

        private RenderSprite renderSprite;
        private RectTransform rectTransform;

        public SpriteRenderer()
        {

        }

        CanvasComponent IterateGetParentComponent(GameObject gameObject)
        {
            CanvasComponent component = gameObject.GetComponent<CanvasComponent>();

            if (component != null)
            {
                return component;
            } else if (gameObject.parentObject != null)
            {
                return IterateGetParentComponent(gameObject.parentObject);
            }

            return null;
        }

        public override void OnStart()
        {
            rectTransform = UIManager.AddUIComponent(ref gameObject);
            renderSprite = new RenderSprite()
            {
                transform = rectTransform.Position,
                scale = rectTransform.Size,
                rotationAngle = Rotation,
                Sprite = Sprite,
                SolidColor = SolidColor
            };
            
            CanvasComponent component = IterateGetParentComponent(gameObject) ?? gameObject.scene.GetComponent<CanvasComponent>();

            if (component != null)
            {
                SpriteManager.AddRenderSprite(component.canvas.id, renderSprite);
            }
        }

        public override void OnUpdate(float dt)
        {
            if (centerSprite)
            {
                renderSprite.transform = rectTransform.Position;
                renderSprite.scale = rectTransform.Size;
            }
            else
            {
                renderSprite.transform = new Vector2(rectTransform.Position.X + rectTransform.Size.X / 2, rectTransform.Position.Y + rectTransform.Size.Y / 2);
                renderSprite.scale = rectTransform.Size;
            }
            renderSprite.rotationAngle = gameObject.transform.rotation.X;
            renderSprite.Sprite = Sprite;
            renderSprite.order = sortingOrder;
        }

        public override void OnDestroy()
        {
            SpriteManager.RemoveRenderSprite(renderSprite);
        }


    }
}
