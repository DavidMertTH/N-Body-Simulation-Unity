using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class ParticleFun : MonoBehaviour
{
    private Vector2 cursorPos;
    
    public float interactionStrength;
    public int particleCount = 100;
    public Material material;
    public ComputeShader shader;
    public FpsCounter fpsCounter;

    int kernelID;

    private ComputeBuffer _particlePositions;
    private ComputeBuffer _particleVelocity;
    private ComputeBuffer _particleMass;

    int groupSizeX;

    RenderParams rp;


    // Use this for initialization
    void Start()
    {
        Init();
    }

    void Init()
    {
        // initialize the particles
        float3[] posArray = new float3[particleCount];
        float3[] velocityArray = new float3[particleCount];
        float[] massArray = new float[particleCount];
        System.Random rng = new System.Random();
        float radius = 5;
        for (int i = 0; i < particleCount; i++)
        {
            float x = (float)((rng.NextDouble() - 0.5)) * 2;
            float y = (float)((rng.NextDouble() - 0.5)) * 2;
            float z = (float)((rng.NextDouble() - 0.5)) * 2;
            Vector3 dir = new Vector3(x, y, z).normalized;
            dir.z = 0;
            Vector3 pos = dir * radius;

            posArray[i].x = pos.x;
            posArray[i].y = pos.y;
            posArray[i].z = pos.z;

            Vector3 velocity = Vector3.zero;

            velocityArray[i] = velocity;
            massArray[i] = (float)(rng.NextDouble() * 10000);
        }

        // create compute buffer
        _particlePositions = new ComputeBuffer(particleCount, sizeof(float) * 3);
        _particleVelocity = new ComputeBuffer(particleCount, sizeof(float) * 3);
        _particleMass = new ComputeBuffer(particleCount, sizeof(float));

        _particlePositions.SetData(posArray);
        _particleVelocity.SetData(velocityArray);
        _particleMass.SetData(massArray);

        // find the id of the kernel
        kernelID = shader.FindKernel("CSParticle");

        uint threadsX;
        shader.GetKernelThreadGroupSizes(kernelID, out threadsX, out _, out _);
        groupSizeX = Mathf.CeilToInt((float)particleCount / (float)threadsX);

        // bind the compute buffer to the shader and the compute shader
        shader.SetBuffer(kernelID, "posBuffer", _particlePositions);
        shader.SetBuffer(kernelID, "velBuffer", _particleVelocity);
        shader.SetBuffer(kernelID, "massBuffer", _particleMass);

        shader.SetInt("numParticles", (int)particleCount); // explizit casten zur Sicherheit
        Debug.Log(particleCount);

        rp = new RenderParams(material);
        rp.worldBounds = new Bounds(Vector3.zero, 100000 * Vector3.one);
    }

    void OnDestroy()
    {
        if (_particlePositions != null) _particlePositions.Release();
        if (_particleVelocity != null) _particleVelocity.Release();
        if (_particleMass != null) _particleMass.Release();
    }


    void Update()
    {
        float mouseMass = 0;
        if (Input.GetMouseButton(0)) mouseMass = 10* interactionStrength;
        float[] mousePosition2D = { cursorPos.x, cursorPos.y };
        // Send data to the compute shader
        shader.SetFloat("deltaTime", Time.deltaTime);
        shader.SetFloats("mousePosition", mousePosition2D);
        shader.SetFloats("mouseMass", mouseMass);


        // Update the Particles
        shader.Dispatch(kernelID, groupSizeX, 1, 1);
        Graphics.RenderPrimitives(rp, MeshTopology.Points, 1, particleCount);

        material.SetBuffer("posBuffer", _particlePositions);
        material.SetBuffer("velBuffer", _particleVelocity);
        fpsCounter.particles.text = "Particles: "+particleCount;
        if (Input.GetButtonDown("Submit")) ;
    }

    public Vector3 GetMouseWorldPositionOnXYPlane(float z = 0f)
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Plane plane = new Plane(Vector3.forward, new Vector3(0, 0, z));

        if (plane.Raycast(ray, out float distance))
        {
            return ray.GetPoint(distance);
        }

        return Vector3.zero;
    }

    void OnGUI()
    {
        cursorPos = GetMouseWorldPositionOnXYPlane();
    }
}