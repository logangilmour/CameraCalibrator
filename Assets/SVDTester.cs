using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SVDTester : MonoBehaviour {

    public Vector3 d1;
    public Vector3 d2;
    public Vector3 point;
    public GameObject copyCube;
    IEnumerator runner;
    GameObject[] points;

    SingularValueDecomposition SVD;
	// Use this for initialization
	void Start () {
        var normal = Vector3.Cross(d1,d2).normalized;
        Matrix A = new Matrix(100, 3);
        points = new GameObject[100];
        for (int i = 0; i < 100; i++)
        {
            Vector3 p = point + d1 * (Random.value*2-1) + d2 * (Random.value*2-1) + normal * (Random.value*2-1)*1;
            var obj = Instantiate(copyCube);
            points[i] = obj;
            obj.transform.position = p;
            A[i, 0] = p.x;
            A[i, 1] = p.y;
            A[i, 2] = p.z;
        }


        SVD = new SingularValueDecomposition();
        SVD.ps = points;
        runner = SVD.Calculate(A);


	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            runner.MoveNext();
        }

	}
}
