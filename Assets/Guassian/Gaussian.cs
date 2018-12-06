using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode] 
public class Gaussian : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        
        string s = "";
        float total = 0;
        CreateTexture();
        for (int i = 0; i < gaussian.width; i++)
        {
            float v = gaussian.GetPixel(i, 0).r;
            s += v + ", ";
            total += v;
        }
	}

    static Texture2D gaussian;
    static Material mat;
    static Material grey;
    static Material sobel;
    static Material edgeSupressor;
    static Material hough;

    public void OnRenderImage(RenderTexture src, RenderTexture dest){
        CreateTexture();
        CreateMaterial();

        var temp = RenderTexture.GetTemporary(src.width, src.height);
        var temp2 = RenderTexture.GetTemporary(src.width, src.height);
        temp.wrapMode = TextureWrapMode.Clamp;
        temp2.wrapMode = TextureWrapMode.Clamp;

        var houghBuffer = RenderTexture.GetTemporary(1024, 1024, 0, RenderTextureFormat.RFloat);
        mat.SetInt("_KernelSize", gaussian.width);
        mat.SetTexture("_Kernel", gaussian);
        mat.SetVector("_Direction", new Vector2(1, 0));
        Graphics.Blit(src, temp, grey);
        Graphics.Blit(temp, temp2, mat);
        mat.SetVector("_Direction", new Vector2(0, 1));
        Graphics.Blit(temp2, temp, mat);

        Graphics.Blit(temp, temp2, sobel);

        Graphics.Blit(temp2, temp, edgeSupressor);
        hough.SetVector("_Dest_TexelSize",new Vector4(1f / houghBuffer.width, 1f / houghBuffer.height, houghBuffer.width, houghBuffer.height));
        Graphics.Blit(temp, houghBuffer, hough);
        Graphics.Blit(houghBuffer, dest);


    }
    public static void CreateMaterial(){
        if (mat == null)
        {
            mat = new Material(Shader.Find("Hidden/Gauss"));
        }
        if (grey == null)
        {
            grey = new Material(Shader.Find("Hidden/Grey"));
        }
        if (sobel == null)
        {
            sobel = new Material(Shader.Find("Hidden/Sobel"));
        }
        if (edgeSupressor == null)
        {
            edgeSupressor = new Material(Shader.Find("Hidden/EdgeSupressor"));
        }
        if (hough == null)
        {
            hough = new Material(Shader.Find("Hidden/Hough"));
        }
    }

    public static void CreateTexture(){
        var g = CreateGaussian(1,0.01f);
      
        if (gaussian == null)
        {
            gaussian = new Texture2D(g.Length,1);
            for (int i = 0; i<g.Length; i++)
            {
                gaussian.SetPixel(i, 0, new Color(g[i], 0, 0));
            }
            gaussian.Apply();
        }
        
    }

    public static float[] CreateGaussian(float width, float threshold){
        List<float> vals = new List<float>();
        float v = 1;
        float x = 0;
        for(x=0;;x++){
            v=1f/(Mathf.Sqrt(2*Mathf.PI)*width)*Mathf.Exp(-Mathf.Pow(x,2)/(2*Mathf.Pow(width,2)));
            if (v > threshold)
            {
                
                vals.Add(v);
            }
            else
            {
                break;
            }
        }
        var g = vals.ToArray();
        float total = 0;
        var full = new float[(g.Length - 1) * 2 + 1];
        for (int i = 0; i < g.Length; i++)
        {
            if (i == 0)
            {
                full[g.Length - 1] = g[i];
            }
            else
            {
                full[g.Length - 1 + i] = g[i];
                full[g.Length - 1 - i] = g[i];
            }
        }
        for (int i = 0; i < full.Length; i++)
        {
            total += full[i];
        }
        for (int i = 0; i < full.Length; i++)
        {
            full[i] = full[i] / total;
        }

        return full;
    }
}
