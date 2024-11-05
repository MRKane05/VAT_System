using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class VATAnim
{
	public string name = "";
	public int frameStart = -1;
	public int frameEnd = -1;
	
	public VATAnim(string newName, int newStart, int newEnd)
    {
		name = newName;
		frameStart = newStart;
		frameEnd = newEnd;
    }
}

//Just a simple class to handle different animations
public class VATCharacterAnimator : MonoBehaviour {
	[Header("Vertex Animation Lists")]
	[Tooltip("List of animations with frame keys")]
	public List<VATAnim> VATAnims = new List<VATAnim>();
	[Space]
	[Tooltip("The Mesh we will Animate")]
	public GameObject AnimatedObject;   //The object that we'll be "driving
	MeshRenderer objectRenderer;
	[Space]
	[Header("Animation Details")]
	[Tooltip("Material that's used for the VAT process. Will be assigned as a clone to the Animated Mesh")]
	public Material VATMaterial; //What material we'll be "driving"
	[Space]
	[Header("VAT Textures")]
	public Texture2D vertexAnimationTexture; //Set as a reference when the animation is created, and we use this to set our material
	public Texture2D normalsAnimationTexture; //Set as a reference when the animation is created, and we use this to set our material

	VATAnim currentVATAnimation;	//Which animation are we playing presently?
	VATAnim blendVATAnimation;   //Used for blending frame sequences
	[Tooltip("VAT Shader for handling Blends")]
	public Shader noBlendShader;
	[Tooltip("Standard VAT Shader")]
	public Shader BlendShader;

	float blendDuration = 0.5f;
	float blendStart = 0f;
	int isBlending = 0;	//This is a state-check of sorts. So 0 == no blend, 1 == blending, 2 == post blend tidyup phase

	//Our internal tickers for animating
	float targetFrame = 0;
	float blendTargetFrame = 0;
	[Tooltip("Target Framerate. This should be set by the VATTool")]
	public float targetFramerate = 25f;
	[Tooltip("Bounds for this animation. This should be set by the VATTool")]
	public Vector2 animBounds;	//Set as a reference when the animation is created, and we use this to set our material

	void Awake()
    {
		//Setup our object to be driven by an animated vertex shader
		objectRenderer = AnimatedObject.GetComponent<MeshRenderer>();
		MeshFilter meshFilter = AnimatedObject.GetComponent<MeshFilter>();
		meshFilter.mesh.bounds = new Bounds(Vector3.zero, Vector3.one * 3f);
		
		VATMaterial = Instantiate(objectRenderer.sharedMaterial) as Material;
		if (VATMaterial)
        {
			objectRenderer.material = VATMaterial;	//Set our magic material so that we can drive this object
			if (noBlendShader)
			{
				VATMaterial.shader = noBlendShader;
			}

			VATMaterial.SetFloat("_BoundsMin", animBounds.x);
			VATMaterial.SetFloat("_BoundsMax", animBounds.y);

			//Set our textures
			VATMaterial.SetTexture("_AnimationTex", vertexAnimationTexture);
			VATMaterial.SetTexture("_NormalTex", normalsAnimationTexture);

			VATMaterial.SetFloat("_TexHeight", vertexAnimationTexture.height);

		} else
        {
			Debug.LogError("Material singular failed on: " + gameObject.name);
        }

		SetCurrentAnimation("ZombieIdleAlert");
    }

	public void SetCurrentAnimation(string animName)
    {
		foreach(VATAnim thisAnim in VATAnims)
        {
			if (thisAnim.name == animName)
            {
				currentVATAnimation = new VATAnim(animName, thisAnim.frameStart, thisAnim.frameEnd);
				targetFrame = currentVATAnimation.frameStart;
            }
        }
    }

	public void CrossFade(string animName, float blendTime)
	{
		foreach (VATAnim thisAnim in VATAnims)
		{
			if (thisAnim.name == animName)
			{
				blendVATAnimation = new VATAnim(animName, thisAnim.frameStart, thisAnim.frameEnd);
				blendTargetFrame = blendVATAnimation.frameStart;
				blendStart = Time.time;
				blendDuration = blendTime;
				isBlending = 1;
				VATMaterial.shader = BlendShader;
			}
		}
	}

	void LateUpdate()
    {
		targetFrame += Time.deltaTime * targetFramerate;
		if (isBlending == 2)	//Cleanup from our last frame
        {
			isBlending = 0;
			VATMaterial.shader = noBlendShader;
		}

		if (currentVATAnimation != null)	//because default is zero
        {
			if (targetFrame > currentVATAnimation.frameEnd)
            {
				targetFrame -= (currentVATAnimation.frameEnd - currentVATAnimation.frameStart);	//We don't have any higher logic with this system and will need drivers
            }

			//For the moment lets hardcode to 256
			VATMaterial.SetFloat("_AnimTime", targetFrame / (float)vertexAnimationTexture.width);
		}

		if (isBlending == 1)//We're doing a wonky AF blend
		{ 

			float blendTarget = (Time.time - blendStart) / blendDuration;    //Seeing as we're blending TO animation B

			VATMaterial.SetFloat("_AnimBlend", Mathf.Clamp01(blendTarget));

			blendTargetFrame += Time.deltaTime * targetFramerate;
			if (blendVATAnimation != null)    //because default is zero
			{
				if (blendTargetFrame > blendVATAnimation.frameEnd)
				{
					blendTargetFrame -= (blendVATAnimation.frameEnd - blendVATAnimation.frameStart); //We don't have any higher logic with this system and will need drivers
				}

				//For the moment lets hardcode to 256
				VATMaterial.SetFloat("_AnimTimeB", blendTargetFrame / (float)vertexAnimationTexture.width);
			}

			//Lets check and see if we've "finished" our blend
			if (Time.time > blendStart + blendDuration)
            {
				isBlending = 2;	//Switch off our blend checking
				currentVATAnimation = new VATAnim(blendVATAnimation.name, blendVATAnimation.frameStart, blendVATAnimation.frameEnd);
				targetFrame = blendTargetFrame;
				clearBlendStat();
			}
		}
	}

	void clearBlendStat()	//Nulls our blend animation
    {
		blendVATAnimation.name = "";
		blendVATAnimation.frameStart = -1;
		blendVATAnimation.frameEnd = -1;
    }
}
