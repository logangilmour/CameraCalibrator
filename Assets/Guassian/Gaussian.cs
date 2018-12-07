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
    static Material lineDrawer;
    static Material maxima;

    public void OnRenderImage(RenderTexture src, RenderTexture dest){
        CreateTexture();
        CreateMaterial();

        var temp1 = RenderTexture.GetTemporary(src.width, src.height);
        var temp2 = RenderTexture.GetTemporary(src.width, src.height);

        temp1.wrapMode = TextureWrapMode.Clamp;
        temp2.wrapMode = TextureWrapMode.Clamp;

        var houghBuffer = RenderTexture.GetTemporary(1024, 1024, 0, RenderTextureFormat.RFloat);
        var hough2 = RenderTexture.GetTemporary(1024, 1024, 0, RenderTextureFormat.RFloat);
        mat.SetInt("_KernelSize", gaussian.width);
        mat.SetTexture("_Kernel", gaussian);
        mat.SetVector("_Direction", new Vector2(1, 0));
        Graphics.Blit(src, temp1, grey);
        Graphics.Blit(temp1, temp2, mat);
        mat.SetVector("_Direction", new Vector2(0, 1));
        Graphics.Blit(temp2, temp1, mat);

        Graphics.Blit(temp1, temp2, sobel);

        Graphics.Blit(temp2, temp1, edgeSupressor);
        hough.SetVector("_Dest_TexelSize",new Vector4(1f / houghBuffer.width, 1f / houghBuffer.height, houghBuffer.width, houghBuffer.height));

        ComputeBuffer lines = new ComputeBuffer(1000, 3 * sizeof(float), ComputeBufferType.Append);
        var countBuffer = new ComputeBuffer(1, sizeof(int), ComputeBufferType.IndirectArguments);


        Graphics.Blit(temp1, houghBuffer, hough);

       
        Graphics.SetRandomWriteTarget(1, lines);
        Graphics.Blit(houghBuffer, hough2,maxima);

        ComputeBuffer.CopyCount(lines, countBuffer, 0);

        int[] counter = new int[1] { 0 };
        countBuffer.GetData(counter);
        int count = counter[0];
        Debug.Log(count);
        Vector2[] ln = new Vector2[count];
        lines.GetData(ln,0,0,count);
        List<Vector2> intersections = new List<Vector2>();
        for (int i = 0; i < count; i++)
        {
            var norm1 = new Vector2(Mathf.Cos(ln[i].x), Mathf.Sin(ln[i].x));
            var dist1 = ln[i].y;
            var pnt1 = norm1 * dist1;
            var dir1 = new Vector2(norm1.y, -norm1.x);
            for (int j = 0; j < count; j++)
            {
                if (i != j)
                {
                var norm2 = new Vector2(Mathf.Cos(ln[j].x), Mathf.Sin(ln[j].x));
                var dist2 = ln[j].y;
                var pnt2 = norm2 * dist2;
                var dir2 = new Vector2(norm2.y, -norm2.x);

                    //pnt1+dir1*a = pnt2 * dir2 *b;
                    //pnt1-pnt2 = dir2*b-dir1*a;
                    //cx = dir2x*b-dir1x*a
                    //cy = dir2y*b-dir1y*a

                    //cx*dir2y = dir2x*dir2y*b-dir1x*dir2y*a
                    //cy*dir2x = dir2y*dir2x*b-dir1y*dir2x*a

                    //cx*dir2y-cy*dir2x = dir1y*dir2x*a-dir1x*dir2y*a = (dir1y*dir2x-dir1x*dir2y)*a

                    //(cx*dir2y-cy*dir2x) / (dir1y*dir2x-dir1x*dir2y) = a     !!!!!!

                    // cx*dir1y = dir2x*dir1y*b-dir1x*dir1y*a;
                    // cy*dir1x = dir2y*dir1x*b-dir1y*dir1x*a;

                    // cy*dir1x-cx*dir1y = dir2y*dir1x*b-dir2x*dir1y*b;

                    //(cy*dir1x-cx*dir1y)/(dir2y*dir1x-dir2x*dir1y)=b



                    /*
                    var c = new Vector2(pnt1-pnt2);

                    var denom = (dir1.y * dir2.x - dir1.x * dir2.y);

                    if(denom!=0){
                        var t1 = (c.x * dir2.y - c.y * dir2.x) / denom;
                   
                        Vector2 intersection = pnt1 + dir1 * t1;
                        intersections.Add(intersection);

                    }
                    */

                }
            }
        }

        var args = new ComputeBuffer(4, sizeof(int), ComputeBufferType.IndirectArguments);
        args.SetData(new int[]{ 2, counter[0], 0, 0 });

        Graphics.Blit(houghBuffer, dest);


        Shader.SetGlobalBuffer("lines", lines);
        lineDrawer.SetPass(0);
        Graphics.DrawProceduralIndirect(MeshTopology.Lines, args);



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
        if (lineDrawer == null)
        {
            lineDrawer = new Material(Shader.Find("Unlit/LineDrawer"));
        }
        if (maxima == null)
        {
            maxima = new Material(Shader.Find("Hidden/Maxima"));
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
