using EmberaEngine.Engine.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmberaEngine.Engine.Components
{
    public class AnimationSprite
    {
        public float appearAtTime;
        public float deltaTime;
        public Texture Sprite;
    }

    public class Animation2D
    {
        public float totalTime = 2f;
        public AnimationSprite[] AnimationSprites;
        public float totalTimeSincePlay = 0;
        public AnimationSprite CurrentAnimationSprite;
        public bool isPlaying;

        public void LoadSprites(Texture[] textures)
        {
            AnimationSprites = new AnimationSprite[textures.Length];

            for (int i = 0; i < textures.Length; i++)
            {
                AnimationSprites[i] = new AnimationSprite()
                {
                    deltaTime = totalTime / textures.Length,
                    appearAtTime = (totalTime/textures.Length) * i,
                    Sprite = textures[i],
                };

            }
        }

        public void Play()
        {
            if (isPlaying == true) { return; }
            totalTimeSincePlay = 0;
            isPlaying = true;
        }

        public void UpdateAnimation(float dt)
        {
            totalTimeSincePlay += dt;
            if (totalTimeSincePlay > totalTime)
            {
                isPlaying = false;
                totalTimeSincePlay = 0;
            }
        }

        public AnimationSprite GetAnimationFrame()
        {
            for (int i = 0; i < AnimationSprites.Length - 1; i++)
            {
                if (i != AnimationSprites.Length)
                {
                    if (totalTimeSincePlay > AnimationSprites[i].appearAtTime && totalTimeSincePlay < AnimationSprites[i+1].appearAtTime)
                    {
                        return AnimationSprites[i];
                    }
                } else
                {
                    if (totalTimeSincePlay > AnimationSprites[i].appearAtTime)
                    {
                        return AnimationSprites[i];
                    }
                }
            }
            return AnimationSprites[0];
        }
    }

    public class Animator2D : Component
    {
        public override string Type => nameof(Animator2D);

        public Animation2D Animation;

        public SpriteRenderer spriteRenderer;
        public bool isRendererFound;

        public override void OnStart()
        {
            spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
        }

        public override void OnUpdate(float dt)
        {
            if (Animation.isPlaying)
            {
                Animation.UpdateAnimation(dt);

                if (spriteRenderer != null)
                {
                    spriteRenderer.Sprite = Animation.GetAnimationFrame().Sprite;
                }
                else if (!isRendererFound)
                {
                    spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
                    isRendererFound = true;
                }
            }
        }

        public void Play()
        {
            if (Animation != null)
            {
                Animation.Play();
            }
        }

    }
}
