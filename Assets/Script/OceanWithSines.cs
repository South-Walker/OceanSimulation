﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Script;

[RequireComponent(typeof(MeshRenderer))]
public class OceanWithSines : MonoBehaviour
{
    public Material m;
    const int sinCount = 4;
    public float stdFrequency;
    public float stdSpeed;
    public float stdSteepness;
    private void Awake()
    {
        Random.InitState(96);
        ShaderWave.stdFrequency = stdFrequency;
        ShaderWave.stdSpeed = stdSpeed;
        ShaderWave.stdSteepness = stdSteepness;
        for (int i = 0; i < sinCount; i++)
        {
            ShaderWave.RandomWave();
        }
    }
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        var s = Random.state;

        bool ischange = false;
        if (ShaderWave.stdFrequency != stdFrequency ||
            ShaderWave.stdSpeed != stdSpeed || ShaderWave.stdSteepness != stdSteepness)
        {
            ischange = true;
            ShaderWave.stdFrequency = stdFrequency;
            ShaderWave.stdSpeed = stdSpeed;
            ShaderWave.stdSteepness = stdSteepness;
        }
        if (ischange)
        {
            ShaderWave.Clear();
            for (int i = 0; i < sinCount; i++)
            {
                ShaderWave.RandomWave();
            }
        }
        Random.state = s;


        m.SetFloatArray("_Dx", ShaderWave.Dxs);
        m.SetFloatArray("_Dy", ShaderWave.Dys);
        m.SetFloatArray("_Steepness", ShaderWave.Steepnesses);
        m.SetFloatArray("_Frequency", ShaderWave.Frequencys);
        m.SetFloatArray("_Speed", ShaderWave.Speeds);
    }
}
