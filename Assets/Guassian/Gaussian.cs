using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[ExecuteInEditMode] 
public class Gaussian : MonoBehaviour {
    List<Matrix> Homographies;
    List<Vector3> intersections;
	// Use this for initialization
	void Start () {
        Homographies = new List<Matrix>();
	}
	
	// Update is called once per frame
	void Update () {
        CreateTexture();
        if (Input.GetKeyDown(KeyCode.Space))
        {

            Matrix PointEquations = new Matrix(intersections.Count * 2, 9);
            for (int i = 0; i < intersections.Count; i++)
            {
                Matrix xp = new Matrix(intersections[i].x, intersections[i].y, 1).T;
                Matrix x = new Matrix((i % 9) / 9f, (i / 9) / 9f, 1).T;
                Matrix zero = new Matrix(1, 3);
                PointEquations.SetRow(i * 2, new Matrix(zero, -xp.w * x.T, xp.y * x.T));
                PointEquations.SetRow(i * 2 + 1, new Matrix(xp.w * x.T, zero, -xp.x * x.T));
            }

            Matrix U, S, V;

            PointEquations.SVD(out U, out S, out V);


            Matrix h = V.Col(V.m - 1);

            Matrix H = new Matrix(3, 3).SetData(h[0], h[1], h[2],
                h[3], h[4], h[5],
                h[6], h[7], h[8]);
            Homographies.Add(H);
            Debug.Log("ADDED " + Homographies.Count + "nth Homography");

        }
        if (Input.GetKeyDown(KeyCode.Return))
        {
            Matrix HEquations = new Matrix(Homographies.Count*2+(Homographies.Count<3?1:0)+(Homographies.Count<2?2:0), 6);
            for (int i = 0; i < Homographies.Count; i++)
            {
                var H = Homographies[i];
                HEquations.SetRow(i*2, V6(H, 0, 1));
                HEquations.SetRow(i*2+1, V6(H, 0, 0) - V6(H, 1, 1));
            }
            if (Homographies.Count < 3)
            {
                int off = Homographies.Count;
                HEquations.SetRow(off * 2, new Matrix(0, 1, 0, 0, 0, 0));
                if (Homographies.Count < 2)
                {
                    HEquations.SetRow(off*2 + 1, new Matrix(0, 0, 0, 1, 0, 0));
                    HEquations.SetRow(off*2 + 2, new Matrix(0, 0, 0, 0, 1, 0));
                }
            }
            Homographies.Clear();
            Matrix U, S, V;
            HEquations.SVD(out U, out S, out V);
            var b = V.Col(V.m - 1);

            float B11 = b[0];
            float B12 = b[1];
            float B22 = b[2];
            float B13 = b[3];
            float B23 = b[4];
            float B33 = b[5];

            float v = (B12 * B13 - B11 * B23) / (B11 * B22 - B12 * B12);
            float lambda = B33 - (B13 * B13 + v * (B12 * B13 - B11 * B23)) /B11;

            float alpha = Mathf.Sqrt(lambda / B11);
            float beta = Mathf.Sqrt(lambda*B11/(B11*B22-B12*B12));
            float gamma = -B12 * alpha * alpha * beta / lambda;
            float u = gamma * v / beta - B13 * alpha * alpha / lambda;

            Matrix A = new Matrix(3, 3).SetData(
                           alpha, gamma, u,
                           0, beta, v,
                0, 0, 1);

            Debug.Log(A);

            //var A = new Matrix(3,3).SetData(

        }
        if (Input.GetKeyDown(KeyCode.P))
        {
            Debug.Log(Camera.main.projectionMatrix);

        }
	}

    public Matrix V6(Matrix H, int i, int j){
        H = H.T;
        return new Matrix(
            H[i, 0] * H[j, 0],
            H[i, 0] * H[j, 1] + H[i, 1] * H[j, 0],
            H[i, 1] * H[j, 1],
            H[i, 2] * H[j, 0] + H[i, 0] * H[j, 2],
            H[i, 2] * H[j, 1] + H[i, 1] * H[j, 2],
            H[i, 2] * H[j, 2]);
    }

    static Texture2D gaussian;
    static Material mat;
    static Material grey;
    static Material sobel;
    static Material edgeSupressor;
    static Material hough;
    static Material lineDrawer;
    static Material maxima;
    static Material pointDrawer;
    struct Line{
        public Vector2 pnt;
        public Vector2 dir;
        public float theta;
        public float dist;
        public float mag;
        public float index;
        public Line(float theta, float dist, float mag){
            this.theta = theta;

            this.dist = dist;
            this.mag = mag;
            this.index=0;
            var norm = new Vector2(Mathf.Cos(theta), Mathf.Sin(theta));
            pnt = norm * dist;
            dir = Normal(norm);
        }

        public bool Intersects(Line other, out Vector2 p){

            float t;
            if (Intersects(other, out t))
            {
                p = pnt + dir * t;
                return true;
            }
            p = Vector2.zero;
            return false;
        }

        public bool Intersects(Line other, out float t){
            var pnt1 = pnt;
            var pnt2 = other.pnt;
            var dir1 = dir;
            var dir2 = other.dir;

            var denom = Vector3.Dot(dir1, Normal(dir2));
            var num = Vector2.Dot((pnt2 - pnt1), Normal(dir2));
            if (denom < -0.001 || denom > 0.001)
            {

                t = num / denom;
                return true;
            }
            t = 0;
            return false;
        }


        public Vector4 Polar(){
            return new Vector4(theta, dist,mag,index);
        }
    }
    struct LinePoint{
        public float t;
        public Line line;
    }
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
        //Debug.Log(count);
        Vector3[] ln = new Vector3[count];
        lines.GetData(ln,0,0,count);
        intersections = new List<Vector3>();
        List<Line> vecLines = ln.Select(l=>new Line(l.x,l.y,l.z)).OrderBy(l=>l.theta).ToList();
        List<Line> filtered = new List<Line>();
        for (int i = 0; i < vecLines.Count; i++) // Filter paralell lines
        {
            var line1 = vecLines[i];
            for (int j = i + 1; j < vecLines.Count; j++)
            {
                var line2 = vecLines[j];
                var angle = Mathf.Abs(line1.theta - line2.theta);
                if (angle < 0.05f || angle > Mathf.PI - 0.05f) // if lines are nearly parallel
                {
                    Vector2 p;
                    if (line1.Intersects(line2, out p))
                    {
                        if (p.x < 2 && p.x > -2 && p.y > -2 && p.y < 2)
                        {

                            goto Found;
                        }
                    }
                }
            }
            filtered.Add(line1);

            Found:{}

        }


        Line baseLine = filtered[0];

        var linepoints = filtered.Select(l =>
            {
                float p;
                if (!l.Intersects(baseLine, out p))
                    p = float.PositiveInfinity;
                return new LinePoint(){
                    line = l,
                    t = p
                };
            }).OrderBy(lp=>lp.t).ToArray();


        if (linepoints[linepoints.Length-1].t==float.PositiveInfinity) // found a parallel line
            linepoints = linepoints.OrderBy(lp=>Mathf.Abs(lp.t)).ToArray();
                    

        Line baseu = linepoints[17].line;
        Line basev = linepoints[0].line;

        if (Vector2.Dot(baseu.dir, Vector2.one) < 0)
        {
            baseu.dir *= -1;
        }
        if (Vector2.Dot(basev.dir, Vector2.one) < 0)
        {
            basev.dir *= -1;
        }


        var u = linepoints.Take(9).OrderBy(l=>{float t; if(!baseu.Intersects(l.line,out t))Debug.Log("WTF"); return t;}).ToArray();
        var v = linepoints.Skip(9).OrderBy(l=>{float t; if(!basev.Intersects(l.line,out t))Debug.Log("WTF"); return t;}).ToArray();


        if (Mathf.Abs(Vector2.Dot(baseu.dir, Vector3.right)) < Mathf.Abs(Vector2.Dot(basev.dir, Vector3.right)))
        {
            var temp = u;
            u = v;
            v = temp;

        }

        filtered = u.Concat(v).Select(l => l.line).ToList();

        for (int i = 0; i < u.Length; i++)
        {
            var lineu = u[i].line;
            for (int j = 0; j < v.Length; j++)
            {
                var linev = v[j].line;
                   
                Vector2 p;
                lineu.Intersects(linev, out p);

                intersections.Add(new Vector3(p.x,p.y,i*9+j));
    
                    
            }
        }
      
        Graphics.Blit(src, dest);

        //Shader.SetGlobalBuffer("lines", lines);
        if (filtered.Count > 0)
        {
            var far = filtered.Select(l => l.Polar()).ToArray();
            var clines = new ComputeBuffer(filtered.Count, sizeof(float) * 4);
            clines.SetData(far, 0, 0, filtered.Count);
            Shader.SetGlobalBuffer("_Lines", clines);
            lineDrawer.SetPass(0);

            var args = new ComputeBuffer(4, sizeof(int), ComputeBufferType.IndirectArguments);
            args.SetData(new int[]{ 2, filtered.Count, 0, 0 });

            Graphics.DrawProceduralIndirect(MeshTopology.Lines, args);
            args.Release();
            clines.Release();

        }

       
       


        //Debug.Log(H);

        if (intersections.Count > 0)
        {

            var points = new ComputeBuffer(intersections.Count, sizeof(float) * 3, ComputeBufferType.Default);

            points.SetData(intersections.ToArray());
            var args = new ComputeBuffer(4, sizeof(int), ComputeBufferType.IndirectArguments);
            args.SetData(new int[]{ 2, intersections.Count, 0, 0 });

            Shader.SetGlobalBuffer("_Points", points);
            pointDrawer.SetPass(0);

            Graphics.DrawProceduralIndirect(MeshTopology.Lines, args);
            args.Release();
            points.Release();


        }



        lines.Release();

        countBuffer.Release();
        temp1.Release();
        temp2.Release();
        houghBuffer.Release();
        hough2.Release();

    }
        

    public static Vector2 Normal(Vector2 v){
        return new Vector2(v.y, -v.x);
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
        if (pointDrawer == null)
        {
            pointDrawer = new Material(Shader.Find("Unlit/PointDrawer"));
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
