using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public static class LogDebugHelper
{
    public static void printVectors(List<Vector3> vecs, List<String> vectorNames) {
        StringBuilder str = new StringBuilder();

        //str.Append(String.Format("{0,13}", ""));
        //str.Append(String.Format(" X{0,8}Y{0,8}Z{0,8}\n", ""));

        for (int i = 0; i < vecs.Count; i++) {
            if (i < vectorNames.Count) 
                str.Append(String.Format("{0,-13} ", vectorNames[i]));
            else 
                str.Append(String.Format("{0,13} ", ""));
            
            str.Append(
                "X"+(vecs[i].x>=0 ? " "+vecs[i].x.ToString("F8") : vecs[i].x.ToString("F8")) + "    " + 
                "Y"+(vecs[i].y>=0 ? " "+vecs[i].y.ToString("F8") : vecs[i].y.ToString("F8")) + "    " +  
                "Z"+(vecs[i].z>=0 ? " "+vecs[i].z.ToString("F8") : vecs[i].z.ToString("F8")) + "\n"
            );
        }
        
        Debug.Log(str.ToString());
    }

    public static void printVectors(Vector3 vec, String Name) {
        printVectors(new List<Vector3>{vec}, new List<String>{Name});
    }
    
}
