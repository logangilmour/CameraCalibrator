using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingularValueDecomposition{

    public Matrix U;
    public Matrix S;
    public Matrix V;
    public GameObject[] ps;

    public IEnumerator Calculate(Matrix A){
        U = new Matrix(A.n, A.n);
        S = new Matrix(A.n, A.m);
        V = new Matrix(A.m, A.m);
        for (int i = 0; i < Mathf.Min(A.m, A.n); i++)
        {
            var M1D = A.copy();
            for (int j = 0; j < i; j++)
            {
                M1D -= U.Col(j) * V.Col(j).T * S[j,j];
            }
            for(int q=0; q<A.n; q++){
                ps[q].transform.position = (Vector4)M1D.Row(q).T;
            }
            Matrix u;
            Matrix v;
            float s;

            {
                Matrix B = M1D.T * M1D;

                v = new Matrix(M1D.m,1);

                for (int k = 0; k < v.Length; k++)
                {
                    v[k] = Random.value;
                }
                v = v / v.norm;


                float dot = 0;
                while (dot < 1 - 0.000001f)
                {
                    Debug.DrawRay(Vector3.zero, (Vector4)v,new Color[]{Color.red,Color.green,Color.cyan,Color.yellow}[i],2);
                    Debug.Log(v);
                    var lastV = v.copy();

                    v = B * v;
                    v = v / v.norm;

                    dot = lastV.T * v;

                    Debug.Log(dot);

                    yield return null;
                }
            }

            u = A * v;
            s = u.norm;
            u = u / s;
           
            U.SetCol(i, u);
            V.SetCol(i, v);
            S[i, i] = s;
            yield return null;
        }
        Debug.Log(V.Col(2));
        Debug.Log(Vector3.Cross((Vector4)V.Col(0), (Vector4)(V.Col(1))));

        S[2, 2] = 2;

        var AA = U * S * V.T;


        for(int q=0; q<AA.n; q++){
            ps[q].transform.position = (Vector4)AA.Row(q).T;
        }

    }
}
