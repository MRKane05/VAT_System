using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SATAnimation
{
	public AnimationClip animClip;
	//other information we need
	public int frameStart = 0;
	public int frameLength = 0;
	public int frameEnd = 0;
}

//So the theory is to put this on a character and our VAT tool will grab the necessary animations off of this
//We'll also do our calculations and populate another script which can be used to drive the animator
public class VATAnimationGenerator : MonoBehaviour {
	[Tooltip("Add your animations here as clips. The system will create indexes and complete the population process from what you've listed here")]
	public List<SATAnimation> SATAnimations = new List<SATAnimation>();
	[Tooltip("This will be set by the VATTool")]
	public Vector2 bounds;


	public int GetFrameCount(float minSamplingRate)
    {
		int frameCount = 0;
		foreach (SATAnimation thisAnim in SATAnimations)
        {
			frameCount += Mathf.CeilToInt(thisAnim.animClip.length * minSamplingRate) + 2;
        }

		return frameCount;
    }

	public void SetupAnimationFrames(float minSamplingRate, VATCharacterAnimator satCharacterAnimator)
	{

		if (satCharacterAnimator)	//Scrub our character animations
        {
			satCharacterAnimator.VATAnims.Clear();
			satCharacterAnimator.VATAnims = new List<VATAnim>();
		}

		int currentFrame = 0;
		foreach (SATAnimation thisAnim in SATAnimations)
		{
			thisAnim.frameStart = currentFrame;
			thisAnim.frameLength = Mathf.CeilToInt(thisAnim.animClip.length * minSamplingRate);
			currentFrame += thisAnim.frameLength + 2;	//For an exit and entry frame buffer
			thisAnim.frameEnd = currentFrame;

			if (satCharacterAnimator)
			{
				VATAnim newVATAnim = new VATAnim(thisAnim.animClip.name, thisAnim.frameStart+1, thisAnim.frameEnd-1);
				satCharacterAnimator.VATAnims.Add(newVATAnim);
			}			
		}
	}

	public AnimationClip GetClip(int currentFrame)
	{
		foreach (SATAnimation thisAnim in SATAnimations)
		{
			if (currentFrame >= thisAnim.frameStart && currentFrame < thisAnim.frameEnd)
            {
				return thisAnim.animClip;
            }
		}
		return null; //we have no animation for this frame
	}

	public float GetClipTime(int currentFrame)
    {
		foreach (SATAnimation thisAnim in SATAnimations)
		{
			if (currentFrame >= thisAnim.frameStart && currentFrame <= thisAnim.frameEnd)
			{

				//Quick little caveat check because our frameStart will include a buffer frame
				if (currentFrame == thisAnim.frameStart)
                {
					if (thisAnim.animClip.isLooping && false)
                    {
						return 1.0f;	//Buffer with our end frame
                    } else
                    {
						return 0f; //Buffer with our first frame
                    }
                }

				if (currentFrame == thisAnim.frameEnd)
				{
					if (thisAnim.animClip.isLooping && false)
					{
						return 0.0f;    //Buffer with our first frame
					}
					else
					{
						return 1f; //Buffer with our end frame
					}
				}

				//return Mathf.InverseLerp((float)thisAnim.frameStart+1f, (float)thisAnim.frameEnd-1f, (float)currentFrame);
				return (float)(currentFrame - 1 - thisAnim.frameStart) / (float)(thisAnim.frameEnd - thisAnim.frameStart - 2) * thisAnim.animClip.length;
			}
		}
		return 1f; //we have no animation for this frame
	}
}
