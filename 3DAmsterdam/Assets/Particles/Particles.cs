using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Particles : MonoBehaviour
{
    private ParticleSystem particles;
    protected StreamReader reader = null;

    private bool first = true;
    private Vector3 position;
    private Color color;

    void Start()
    {
        particles = GetComponent<ParticleSystem>();

        string[] lines = File.ReadAllLines("C:/Users/Sam/Desktop/tree.txt");

        foreach(string line in lines){
            double[] values = Array.ConvertAll(line.Split(' '), double.Parse);

            position = ConvertCoordinates.CoordConvert.RDtoUnity(new ConvertCoordinates.Vector3RD((float)values[0], (float)values[1], (float)values[2]));
            if (first)
            {
                Camera.main.transform.position = position;
                first = false;
            }

            color = new Color((float)values[3], (float)values[4], (float)values[5], 1.0f);

            var emitParams = new ParticleSystem.EmitParams();
            emitParams.position = position;
            emitParams.startColor = color;

            particles.Emit(emitParams, 1);
        }
    }
}
