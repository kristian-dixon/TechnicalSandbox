using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Credit: https://catlikecoding.com/unity/tutorials/advanced-rendering/bloom/


[ExecuteInEditMode, ImageEffectAllowedInSceneView]
public class BloomEffect : MonoBehaviour
{
	[Range(1, 16)]
	public int iterations = 1;

	RenderTexture[] textures = new RenderTexture[16];
	public Shader bloomShader;

	[NonSerialized]
	Material bloom;

	const int BoxDownPrefillterPass = 0;
	const int BoxDownPass = 1;
	const int BoxUpPass = 2;
	const int ApplyBloomPass = 3;

	[Range(0, 10)]
	public float threshold = 1;

	void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		if (bloomShader == null) return;

		if (bloom == null)
		{
			bloom = new Material(bloomShader);
			bloom.hideFlags = HideFlags.HideAndDontSave;
		}

		bloom.SetFloat("_Threshold", threshold);

		int width = source.width / 2;
		int height = source.height / 2;
		RenderTextureFormat format = source.format;

		RenderTexture currentDestination = textures[0] =
			RenderTexture.GetTemporary(width, height, 0, format);

		Graphics.Blit(source, currentDestination, bloom, BoxDownPrefillterPass);
		RenderTexture currentSource = currentDestination;

		int i = 1;
		for (; i < iterations; i++)
		{
			width /= 2;
			height /= 2;
			if (height < 2 || width < 2)
			{
				break;
			}

			currentDestination = textures[i] =
				RenderTexture.GetTemporary(width, height, 0, format);
			Graphics.Blit(currentSource, currentDestination, bloom, BoxDownPass);
			currentSource = currentDestination;
		}

		for (i -= 2; i >= 0; i--)
		{
			currentDestination = textures[i];
			textures[i] = null;
			Graphics.Blit(currentSource, currentDestination, bloom, BoxUpPass);
			RenderTexture.ReleaseTemporary(currentSource);
			currentSource = currentDestination;
		}

		bloom.SetTexture("_SourceTex", source);
		Graphics.Blit(currentSource, destination, bloom, ApplyBloomPass);
		RenderTexture.ReleaseTemporary(currentSource);
	}
}