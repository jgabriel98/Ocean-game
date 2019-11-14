using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Waves : MonoBehaviour
{
    public static int dimension = 80;
    public static Octave[] Octaves;
    
    protected MeshFilter meshFilter;
    protected Mesh mesh;
    // Start is called before the first frame update
    void Start()
    {
        mesh = new Mesh();
        mesh.name = gameObject.name;
        mesh.vertices = GenerateVerts();
        mesh.triangles = GenerateTries();

        mesh.RecalculateBounds();
        mesh.RecalculateNormals(); 
        mesh.OptimizeIndexBuffers();
        
        meshFilter = gameObject.AddComponent<MeshFilter>();
        meshFilter.mesh = mesh;
    }


    // Update is called once per frame
    void Update() {
        Vector3[] verts = mesh.vertices;
        for (int x = 0; x <= dimension; x++) {
            for (int z = 0; z <= dimension; z++) {
                
                var y = 0f;
                for (int o = 0; o < Octaves.Length; o++) {
                    var position = gameObject.transform.position;
                    if (Octaves[o].alternate) {
                        var noise = Mathf.PerlinNoise(  ((x + position.x) * Octaves[o].scale.x) / dimension, 
                                                        ((z + position.z) * Octaves[o].scale.y) / dimension)
                                    * Mathf.PI *2.0f;
                        y += Mathf.Cos(noise + Octaves[o].speed.magnitude * Time.time) * Octaves[o].height;
                    } else {
                        var perl = Mathf.PerlinNoise(((x + position.x) * Octaves[o].scale.x + Time.time * Octaves[o].speed.x) / dimension,
                                                     ((z + position.z) * Octaves[o].scale.y + Time.time * Octaves[o].speed.y) / dimension) 
                                   - 0.5f;
                        y += perl * Octaves[o].height;
                    }
                }

                verts[index(x, z)] = new Vector3(x, y, z);
            }
        }

        mesh.vertices = verts;
        mesh.RecalculateNormals();
    }

    
    int index(int i, int j) {
        return i * (dimension+1) + j;
    }

    private Vector3[] GenerateVerts() {
        var verts = new Vector3[(dimension+1) * (dimension+1)];

        //equaly distributed verts
        for(int x = 0; x <= dimension; x++)
            for(int z = 0; z <= dimension; z++)
                verts[index(x, z)] = new Vector3(x, 0, z);

        return verts;
    }

    private int[] GenerateTries() {
        var tries = new int[mesh.vertices.Length * 6];

        //two triangles are one tile
        for (int x = 0; x < dimension; x++) {
            for (int z = 0; z < dimension; z++) {
                tries[index(x, z) * 6 + 0] = index(x, z);
                tries[index(x, z) * 6 + 1] = index(x + 1, z + 1);
                tries[index(x, z) * 6 + 2] = index(x + 1, z);
                tries[index(x, z) * 6 + 3] = index(x, z);
                tries[index(x, z) * 6 + 4] = index(x, z + 1);
                tries[index(x, z) * 6 + 5] = index(x + 1, z + 1);
            }
        }

        return tries;
    }
    
    public float GetHeight(Vector3 position) {
        Vector3 lossyScale = transform.lossyScale;
        
        //scale factor and position in local space
        var scale = new Vector3(1 / lossyScale.x, 0, 1 / lossyScale.z);
        var localPos = Vector3.Scale((position - transform.position), scale);

        //get edge points
        var p1 = new Vector3(Mathf.Floor(localPos.x), 0, Mathf.Floor(localPos.z));
        var p2 = new Vector3(Mathf.Floor(localPos.x), 0, Mathf.Ceil(localPos.z));
        var p3 = new Vector3(Mathf.Ceil(localPos.x), 0, Mathf.Floor(localPos.z));
        var p4 = new Vector3(Mathf.Ceil(localPos.x), 0, Mathf.Ceil(localPos.z));

        //clamp if the position is outside the plane
        p1.x = Mathf.Clamp(p1.x, 0, dimension);     p1.z = Mathf.Clamp(p1.z, 0, dimension);
        p2.x = Mathf.Clamp(p2.x, 0, dimension);     p2.z = Mathf.Clamp(p2.z, 0, dimension);
        p3.x = Mathf.Clamp(p3.x, 0, dimension);     p3.z = Mathf.Clamp(p3.z, 0, dimension);
        p4.x = Mathf.Clamp(p4.x, 0, dimension);     p4.z = Mathf.Clamp(p4.z, 0, dimension);

        //get the max distance to one of the edges and take that to compute max - dist
        var max = Mathf.Max(Vector3.Distance(p1, localPos), Vector3.Distance(p2, localPos), Vector3.Distance(p3, localPos), Vector3.Distance(p4, localPos) + Mathf.Epsilon);
        var dist = (max - Vector3.Distance(p1, localPos))
                 + (max - Vector3.Distance(p2, localPos))
                 + (max - Vector3.Distance(p3, localPos))
                 + (max - Vector3.Distance(p4, localPos) + Mathf.Epsilon);
        //weighted sum
        var height = mesh.vertices[index((int)p1.x, (int)p1.z)].y * (max - Vector3.Distance(p1, localPos))
                   + mesh.vertices[index((int)p2.x, (int)p2.z)].y * (max - Vector3.Distance(p2, localPos))
                   + mesh.vertices[index((int)p3.x, (int)p3.z)].y * (max - Vector3.Distance(p3, localPos))
                   + mesh.vertices[index((int)p4.x, (int)p4.z)].y * (max - Vector3.Distance(p4, localPos));

        //scale
        return height * lossyScale.y / dist;

    }
    
    [Serializable]
    public struct Octave
    {
        public Vector2 speed;
        public Vector2 scale;
        public float height;
        public bool alternate;
    }


}