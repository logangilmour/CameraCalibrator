using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class Matrix{
    public readonly int n;
    public readonly int m;

    public Matrix(int n, int m){
        this.n = n;
        this.m = m;
        data = new double[m * n];
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

    public Matrix(params double[] entries) : this(1,entries.Length){
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

    public double x {
        get{
            AssertVector();

            return this[0];
        }
    }


    public double y {
        get{
            AssertVector();
            if (Length < 2)
            {
                throw new System.IndexOutOfRangeException("Doesn't have a y component");
            }
            return this[1];
        }
    }

    public double z {
        get{
            AssertVector();
            if (Length < 3)
            {
                throw new System.IndexOutOfRangeException("Doesn't have a z component");
            }
            return this[2];
        }
    }

    public double w {
        get{
            AssertVector();
            if (Length < 3)
            {
                throw new System.IndexOutOfRangeException("Doesn't have a w component");
            }
            return this[Length==3?2:3];
        }
    }

    public Matrix SetData(params double[] entries){
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

    double[] data;

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

    public void SetCol(int j,Matrix v){
        if (j < 0 || j >= m)
        {
            throw new System.IndexOutOfRangeException();
        }
        v.AssertVector();
        if (v.Length != n)
        {
            throw new System.ArgumentOutOfRangeException();
        }
        for (int i = 0; i < n; i++)
        {
            this[i, j] = v[i];
        }
    }

    public double this[int i, int j]{
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

    public double this[int index]{
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

    public static Matrix operator * (Matrix A, double s){
        Matrix output = new Matrix(A.n, A.m);
        for (int i = 0; i < output.Length; i++)
        {
            output[i] = A[i] * s;
        }
        return output;
    }

    public static Matrix operator * (double s, Matrix A){
        return A * s;
    }

    public static Matrix operator / (Matrix A, double s){
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

    public double norm{
        get{
            double total = 0;
            for (int i = 0; i < Length; i++)
            {
                total += this[i] * this[i];
            }
            return Math.Sqrt(total);
        }
    }

    public static implicit operator Vector4(Matrix A){
        A.AssertVector();
        var v = new Vector4();
        for (int i = 0; i < A.Length; i++)
        {
            v[i] = (float)A[i];
        }
        return v;
    }

    public static implicit operator double(Matrix A){
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
            return A * (double)B;
        }
        if (A.IsScalar())
        {
            return (double)A * B;
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
                double entry = 0;
                for (int k = 0; k < A.m; k++)
                {
                    entry += A[i, k] * B[k, j];
                }
                output.data[i + j*A.n] = entry;
            }
        }
        return output;
    }

    public static Matrix operator + (Matrix A, Matrix B){
        if (A.n != B.n || A.m != B.m)
        {
            throw new System.InvalidOperationException("Matrices must be of same dimensions");
        }
        Matrix output = new Matrix(A.n, A.m);
        for (int i = 0; i < A.n; i++)
        {
            for (int j = 0; j < A.m; j++)
            {
                output[i, j] = A[i, j] + B[i, j];
            }
        }
        return output;
    }

    public static Matrix operator - (Matrix A, Matrix B){
        return A + B * -1;
    }
        
    public Matrix Diag{
        get{
            
            if (IsRowVector() || IsColumnVector())
            {
                Matrix diag = new Matrix(this.Length, this.Length);
                for (int i = 0; i < this.Length; i++)
                {
                    diag[i, i] = this[i];
                }
                return diag;
            }
            else
            {
                Matrix diag = new Matrix(Math.Min(this.n, this.m), 1);
                for (int i = 0; i < diag.Length; i++)
                {
                    diag[i] = this[i, i];
                }
                return diag;
            }
        }
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

    public delegate Matrix Function(Matrix parameters);

    public static Matrix Optimize(Function jac, Function res, Matrix p){
        int MAX_ITER = 100;
        double lambda = 1f;
        double ftol = 1e-12;
        double gtol = 1e-12;
        double prevCost = double.MaxValue;
        Matrix cp = p;
        var Jr= new Matrix(1,1);
        var H = new Matrix(1,1);
        var g = new Matrix(1,1);
        for (int i = 0; i < MAX_ITER; i++)
        {
            var r = res(cp);
            double cost = r.norm;

            if( cost <= prevCost ) { // new place is better (always true iteration 1)
                Debug.Log("IMPROVED: "+cost);
                Debug.DrawLine(new Vector3((float)p.x,0,(float)p.y), new Vector3((float)cp.x,0,(float)cp.y), Color.red, 30f);

                // check for convergence
                // ftol <= (cost(k) - cost(k+1))/cost(k)
                if (ftol * prevCost >= prevCost-cost)
                {
                    Debug.Log("Converged by cost");
                    return p;
                }

                prevCost = cost;
                lambda /= 10.0;

                p = cp;
                Jr = jac(p);
                H = Jr.T * Jr;
                g = Jr.T * r;

                bool converged = true;
                for (int j = 0; j < g.Length; j++) converged &= Math.Abs(g[j]) < gtol;
                
                if (converged)
                {
                    Debug.Log("Converged by gradient");
                    return p;
                }

            } else {
                Debug.Log("WORSE: " + cost);

                lambda *= 10.0;
            }
                
            var hinv = (H + (lambda * H.Diag).Diag).Inv;
            Debug.Log("H: "+H);
            Debug.Log("Hinv: " + hinv);
            var step = hinv * g;
            Debug.Log("Lambda: " + lambda);
            Debug.Log("STEP: " + step);

            cp = p - step;
        }
        return null;

    }

    public Matrix Inv{
        get{
            return GaussianElimination();
        }
    }

    public Matrix PInv{
        get{
            Matrix U, S, V;

            SVD(out U, out S, out V);
            var s = S.Diag;
            for (int i = 0; i < s.Length; i++)
            {
                if (s[i] != 0)
                {
                    s[i] = 1f / s[i];
                }
            }
            return V * s.Diag * U.T;
        }
    }

    public Matrix GaussianElimination(){
        Matrix A = new Matrix(n, m * 2);
        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < m; j++)
            {
                A[i, j] = this[i, j];
            }
        }
        for (int i = 0; i < n; i++)
        {
            A[i, i + m] = 1;
        }

        int h = 0;
        int k = 0;
        while (h < n && k < m)
        {
            int i_max = 0;
            double i_max_val = double.NegativeInfinity;
            for (int i = h; i < n; i++)
            {
                if (i_max_val < A[i, k])
                {
                    i_max = i;
                    i_max_val = A[i, k];
                }
            }
            if (A[i_max, k] == 0)
            {
                k++;
            }
            else
            {
                var temp = A.Row(i_max);
                A.SetRow(i_max, A.Row(h));
                A.SetRow(h, temp);

                for (int i = h + 1; i < n; i++)
                {
                    var f = A[i, k] / A[h, k];
                    A[i, k] = 0;

                    for (int j = k + 1; j < m*2; j++)
                    {
                        A[i, j] = A[i, j] - A[h, j] * f;
                    }
                }
                h++;
                k++;
            }
        }

        for (int i = 0; i < n; i++)
        {
            double e = A[i, i];
            double f = 1.0 / A[i, i];
            A[i, i] = 1;
            for (int j = i+1; j < m * 2; j++)
            {
                A[i, j] *= f;
            }
        }
        for (h = 0; h < n; h++)
        {
            for (k = h + 1; k < m; k++)
            {
                double f = A[h, k];
                A[h, k] = 0;
                for (int j = k+1; j < m * 2; j++)
                {
                    A[h, j] -= A[k, j]*f;
                }
            }
        }
        Matrix B = new Matrix(n, m);
        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < m; j++)
            {
                B[i, j] = A[i, j + m];
            }
        }
        return B;
    }

    public void SVD(out Matrix U, out Matrix S, out Matrix V)
    {
        var A = this;
        U = new Matrix(A.n, A.n);
        S = new Matrix(A.n, A.m);
        V = new Matrix(A.m, A.m);
        for (int i = 0; i < Math.Min(A.m, A.n); i++)
        {
            var M1D = A.copy();
            for (int j = 0; j < i; j++)
            {
                M1D -= U.Col(j) * V.Col(j).T * S[j, j];
            }
            Matrix u;
            Matrix v;
            double s;

            {
                Matrix B = M1D.T * M1D;

                v = new Matrix(M1D.m, 1);

                for (int k = 0; k < v.Length; k++)
                {
                    v[k] = UnityEngine.Random.value;
                }
                v = v / v.norm;


                double dot = 0;
                while (dot < 1 - 1e-12)
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
