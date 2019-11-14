using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WiimoteApi;

public class Ocean : MonoBehaviour
{
    public int gridDimension = 2;
    public Waves wavesPrefab;
    public int WaveDimension;
    public Waves.Octave[] Octaves;

    private Waves[,] _waves;
    void Start()
    {
        Waves.dimension = WaveDimension;
        Waves.Octaves = Octaves;
        
        _waves = new Waves[gridDimension, gridDimension];

        for (int i = 0; i < gridDimension; i++) {
            for (int j = 0; j < gridDimension; j++) {
                _waves[i,j] = Instantiate(wavesPrefab, gameObject.transform);
                _waves[i,j].transform.position += new Vector3(i*WaveDimension, 0, j*WaveDimension);
            }
        }
    }

    private void Update() {
        if (Input.GetKey("escape")) {
            Application.Quit();
        }
    }
    

    public float getHeight(Vector3 position) {
        Waves currentWaves = findWavesAtPoint(position);

        //get position in the current waves
        return currentWaves.GetHeight(position);
    }

    //acha o waves no qual o ponto "position" está no momento
    Waves findWavesAtPoint(Vector3 position) {
        int i = (int)(position.x / (Waves.dimension));
        int j = (int)(position.z / (Waves.dimension));

        return _waves[i, j];
    }
    
}
