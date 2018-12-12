using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Matrix{
    public readonly int n;
    public readonly int m;

    public Matrix(int n, int m){
        this.n = n;
        this.m = m;
        data = new float[m * n];
    }

    public bool IsColumnVector(){
        return m == 1;
    }

    public bool IsRowVector(){
        return n == 1;
    }

    public bool IsScalar(){
        return n == 1 && m == 1;
    }

    public Matrix(params float[] entries) : this(1,entries.Length){
        for (int j = 0; j < m; j++)
        {
            this[0, j] = entries[j];
        }
    }

    public void AssertVector(){
        if (m != 1 && n!=1)
        {
            throw new System.InvalidOperationException("Not a Vector! "+n+"x"+m);
        }
    }

    public float x {
        get{
            AssertVector();

            return this[0];
        }
    }


    public float y {
        get{
            AssertVector();
            if (Length < 2)
            {
                throw new System.IndexOutOfRangeException("Doesn't have a y component");
            }
            return this[1];
        }
    }

    public float z {
        get{
            AssertVector();
            if (Length < 3)
            {
                throw new System.IndexOutOfRangeException("Doesn't have a z component");
            }
            return this[2];
        }
    }

    public float w {
        get{
            AssertVector();
            if (Length < 3)
            {
                throw new System.IndexOutOfRangeException("Doesn't have a w component");
            }
            return this[Length==3?2:3];
        }
    }

    public Matrix SetData(params float[] entries){
        if (entries.Length != this.Length)
        {
            throw new System.ArgumentOutOfRangeException("Wrong number of entries");
        }
        for (int j = 0; j < m; j++)
        {
            for (int i = 0; i < n; i++)
            {
                this[i, j] = entries[j + i * m];
            }
        }
        return this;
    }
    public Matrix(params Matrix[] vectors) : this(1,vectors.Select(s=>s.Length).Sum()){
        int offset = 0;
        foreach (var vec in vectors)
        {
            vec.AssertVector();

            for (int j = 0; j < vec.Length; j++)
            {
                this[0,j+offset] = vec[j];
            }
            offset += vec.Length;
        }
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

    public static Matrix operator * (float s, Matrix A){
        return A * s;
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
        A.AssertVector();
        var v = new Vector4();
        for (int i = 0; i < A.Length; i++)
        {
            v[i] = A[i];
        }
        return v;
    }

    public static implicit operator float(Matrix A){
        if (A.IsScalar())
        {
            return A[0];
        }
        throw new System.InvalidCastException("Not a 1x1 matrix!");
    }

    public static implicit operator Matrix(Vector4 v){
        return new Matrix(v.x, v.y, v.z, v.w).T;
    }

    public static implicit operator Matrix(Vector3 v){
        return new Matrix(v.x, v.y, v.z).T;
    }

    public static implicit operator Matrix(Vector2 v){
        return new Matrix(v.x, v.y).T;
    }

    public static Matrix operator * (Matrix A, Matrix B){
        if (B.IsScalar())
        {
            return A * (float)B;
        }
        if (A.IsScalar())
        {
            return (float)A * B;
        }
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

    public void SVD(out Matrix U, out Matrix S, out Matrix V)
    {
        var A = this;
        U = new Matrix(A.n, A.n);
        S = new Matrix(A.n, A.m);
        V = new Matrix(A.m, A.m);
        for (int i = 0; i < Mathf.Min(A.m, A.n); i++)
        {
            var M1D = A.copy();
            for (int j = 0; j < i; j++)
            {
                M1D -= U.Col(j) * V.Col(j).T * S[j, j];
            }
            Matrix u;
            Matrix v;
            float s;

            {
                Matrix B = M1D.T * M1D;

                v = new Matrix(M1D.m, 1);

                for (int k = 0; k < v.Length; k++)
                {
                    v[k] = Random.value;
                }
                v = v / v.norm;


                float dot = 0;
                while (dot < 1 - 0.000001f)
                {
                    var lastV = v.copy();

                    v = B * v;
                    v = v / v.norm;

                    dot = lastV.T * v;
                }
            }

            u = A * v;
            s = u.norm;
            u = u / s;

            U.SetCol(i, u);
            V.SetCol(i, v);
            S[i, i] = s;
        }
    }
   

}
