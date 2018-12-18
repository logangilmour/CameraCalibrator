using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FunctionVis : MonoBehaviour {

	// Use this for initialization
    delegate float func2D(Vector2 p);
    func2D f = (Vector2 p) => p.x * p.x + p.y * p.y;
	void Start () {
        Mesh mesh = new Mesh();
        int RES = 100;

        var verts = new Vector3[RES*RES];
        var tris = new int[(RES - 1) * (RES - 1) * 6];

        for (int x = 0; x < RES; x++)
        {
            for (int y = 0; y < RES; y++)
            {
                int voff = x + y * RES;
                Vector2 p = new Vector2(x / (RES-1f),y/(RES-1f))*2-Vector2.one;

                verts[voff] = new Vector3(p.x, f(p), p.y);
                if (x < RES - 1 && y < RES - 1)
                {
                    int toff = x * 6 + y * 6 * (RES - 1);
                    tris[toff] = voff;
                    tris[toff + 1] = voff + RES + 1;
                    tris[toff + 2] = voff + 1;
                    tris[toff + 3] = voff;
                    tris[toff + 4] = voff + RES;
                    tris[toff + 5] = voff + RES + 1;
                }
            }
        }

        mesh.vertices = verts;
        mesh.triangles = tris;

        mesh.RecalculateNormals();

        GetComponent<MeshFilter>().sharedMesh = mesh;

        Matrix par = new Matrix(0.5f,0.5f).T;

        Debug.Log(Matrix.Optimize(Jacobian,Fun,par));
       

	}
    Matrix Fun(Matrix p){
        return new Matrix(p.x*p.x+p.y*p.y);
    }

    Matrix Jacobian(Matrix p){
        double DELTA = 1e-8f;

        var temp0 = Fun(p);
        Matrix J = new Matrix(temp0.Length,p.Length);
        // compute the jacobian by perturbing the parameters slightly
        // then seeing how it effects the results.
        for( int i = 0; i < p.Length; i++ ) {
            var par = p.copy();

            par[i] += DELTA;
            var temp1 = Fun(par);
            // compute the difference between the two parameters and divide by the delta
            // temp1 = (temp1 - temp0)/delta
            temp1 = (temp1-temp0)/DELTA;

            J.SetCol(i, temp1);
        }
        return J;
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
