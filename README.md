A basic Vertex Animation System (VAT) for Unity 2018, with an emphasis on Vita Development.

Overview:
The system uses a "donor" Skinned Mesh Renderer, equipped with a VAT Animation Generator, to generate animations and also sets up an animation driver that can be used by the system from the VAT Character Animator.

Setup:
Put a VAT Animation Generator on the base of the object you want to use to generate the animations. You will need to populate the SATAnimations (yes I know that's mis-named) list with Animation Clips that you wish to have included in your VAT Texture
The next step is to put a VAT Character Animator on the object you wish to be populated with all the details for you. In this case you'll need to remove the skinned mesh renderer and assign a MeshRenderer and MeshFilter, as well as a material with one of the Vertex Animation shaders
Select Tools>Vertex Animation Texture Tool. Under this tool you'll need to assign your Donor to the "Animated GameObject" and VAT Character Animator. Set the target framerate you'd like, and press "Generate"
The system will populate animation details and make both a vertex animation texture and normals texture for each vertex also

Explaining the other settings on the VAT Tool:
"Power of Two" will make the texture into a power of two size, which compresses better
"Use Imported UV2s" will have the tool use the UVs2 on the mesh for the pixel assignments for animations
"Textheight (if > 0)" is for setting a target text height, and can be instrumental for making sure that the system doesn't accidentally create massive textures becasue edges are split by import, when you've set the UV2s in a clever way outside of Unity

Tool>UV2 Modifier
This is a quick tool to assign UV2s to vertex index numbers. It works fine, but Unity will often re-import assets without prompts so it doesn't actually work for more than a stand-in solution.
