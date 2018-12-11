using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Matrix{
    public readonly int n;
    public readonly int m;

    public Matrix(int n, int m){
        this.n = n;
        this.m = m;
        data = new float[m * n];
    }

    float[] data;

    public Matrix Row(int i){
        Matrix R = new Matrix(1, m);
        for (int j = 0; j < m; j++)
        {
            R[0, j] = this[i, j];
        }
        return R;
    }

    public Matrix Col(int j){
        Matrix C = new Matrix(n, 1);
        for (int i = 0; i < n; i++)
        {
            C[i, 0] = this[i, j];
        }
        return C;
    }

    public void SetRow(int i,Matrix R){
        if (i < 0 || i >= n)
        {
            throw new System.IndexOutOfRangeException();
        }
        if (R.m != m)
        {
            throw new System.ArgumentOutOfRangeException();
        }
        for (int j = 0; j < m; j++)
        {
            this[i, j] = R[0, j];
        }
    }

    public void SetCol(int j,Matrix C){
        if (j < 0 || j >= m)
        {
            throw new System.IndexOutOfRangeException();
        }
        if (C.n != n)
        {
            throw new System.ArgumentOutOfRangeException();
        }
        for (int i = 0; i < n; i++)
        {
            this[i, j] = C[i, 0];
        }
    }

    public float this[int i, int j]{
        get{
            if (i<0 || i >= n || j<0 || j >= m)
            {
                throw new System.IndexOutOfRangeException();
            }
            return data[i + j * n];
        }
        set{
            if (i<0 || i >= n || j<0 || j >= m)
            {
                throw new System.IndexOutOfRangeException();
            }
            data[i + j * n] = value;
        }
    }

    public Matrix T{
        get{
            Matrix B = new Matrix(m, n);
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < m; j++)
                {
                    B[j, i] = this[i, j];
                }
            }
            return B;
        }
    }

    public float this[int index]{
        get{
            
            return data[index];
        }
        set{
            data[index] = value;
        }
    }



    public int Length{
        get{
            return m * n;
        }
    }

    public static Matrix operator * (Matrix A, float s){
        Matrix output = new Matrix(A.n, A.m);
        for (int i = 0; i < output.Length; i++)
        {
            output[i] = A[i] * s;
        }
        return output;
    }

    public static Matrix operator / (Matrix A, float s){
        return A * (1f / s);
    }

    public Matrix copy(){
        Matrix output = new Matrix(n, m);
        for (int i = 0; i < Length; i++)
        {
            output[i] = this[i];
        }
        return output;
    }

    public float norm{
        get{
            float total = 0;
            for (int i = 0; i < Length; i++)
            {
                total += Mathf.Pow(this[i], 2);
            }
            return Mathf.Sqrt(total);
        }
    }

    public static implicit operator Vector4(Matrix A){
        if (A.n <= 4 && A.m == 1)
        {
            var v = new Vector4();
            for (int i = 0; i < A.n; i++)
            {
                v[i] = A[i, 0];
            }
            return v;
        }
        throw new System.InvalidCastException("Not a Vector!");
    }

    public static implicit operator float(Matrix A){
        if (A.n == 1 && A.m == 1)
        {
            return A[0];
        }
        throw new System.InvalidCastException("Not a 1x1 matrix!");
    }

    public static Matrix operator * (Matrix A, Matrix B){
        if (A.m != B.n)
        {
            throw new System.InvalidOperationException("Matrix Dimensions Incompatible");
        }
        Matrix output = new Matrix(A.n,B.m);
        for (int i = 0; i < A.n; i++)
        {
            for (int j = 0; j < B.m; j++)
            {
                float entry = 0;
                for (int k = 0; k < A.m; k++)
                {
                    entry += A[i, k] * B[k, j];
                }
                output.data[i + j*A.n] = entry;
            }
        }
        return output;
    }

    public static Vector4 operator * (Matrix A, Vector4 v){
        Matrix B = new Matrix(4, 1);
        B[0, 0] = v.x;
        B[1, 0] = v.y;
        B[2, 0] = v.z;
        B[3, 0] = v.w;
        var ret = A * B;
        return new Vector4(ret[0, 0], ret[1, 0], ret[2, 0], ret[3, 0]);
    }

    public static Matrix operator - (Matrix A, Matrix B){
        if (A.n != B.n || A.m != B.m)
        {
            throw new System.InvalidOperationException("Matrices must be of same dimensions");
        }
        Matrix output = new Matrix(A.n, A.m);
        for (int i = 0; i < A.n; i++)
        {
            for (int j = 0; j < A.m; j++)
            {
                output[i, j] = A[i, j] - B[i, j];
            }
        }
        return output;
    }


    public override string ToString()
    {
        string s = "";
        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < m; j++)
            {
                s += this[i, j] + "\t";
            }
            s += "\n";
        }
        return s;
    }


   

}
