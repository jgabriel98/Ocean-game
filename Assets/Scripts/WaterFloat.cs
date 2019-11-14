 using System;
 using System.Collections;
using System.Collections.Generic;
using UnityEngine;
 using UnityEngine.Rendering;
 using UnityTemplateProjects;

 public class WaterFloat : MonoBehaviour
{

    public float AirDrag = 1;
    public float WaterDrag = 10;
    public bool AttachToSurface = false;
    public bool AffectDirection = true;
    public bool SlideInWaves = false;
    public Transform[] FloatPoints;
    public bool PointUnderWater;

    protected Rigidbody rigidbody;
    protected Ocean ocean;

    protected float waterLine;
    protected Vector3[] waterLinePoints;


    protected Vector3 centerOffset;
    protected Vector3 targetUp;
    protected Vector3 smoothVectorRotation;

    protected Vector3 center
    {
        get { return transform.position + centerOffset; }
    }
    void Awake() {
        ocean = FindObjectOfType<Ocean>();
        rigidbody = GetComponent<Rigidbody>();
        rigidbody.useGravity = false;
        
        waterLinePoints = new Vector3[FloatPoints.Length];
        for (int i = 0; i < FloatPoints.Length; i++) {
            waterLinePoints[i] = FloatPoints[i].position;
        }

        centerOffset = PhysicsHelper.GetCenter(waterLinePoints) - transform.position;
    }
    

    
    private void Update() {
        var newWaterLine = 0.0f;
        var pointUnderWater = false;
        

        for (int i = 0; i < FloatPoints.Length; i++) {
            //altura
            waterLinePoints[i] = FloatPoints[i].position;
            waterLinePoints[i].y = ocean.getHeight(FloatPoints[i].position);
            newWaterLine += waterLinePoints[i].y;
            if (waterLinePoints[i].y >= FloatPoints[i].position.y)
                pointUnderWater = true;
        }

        PointUnderWater = pointUnderWater;

        newWaterLine /= FloatPoints.Length;

        float waterLinerDelta = newWaterLine - waterLine;
        waterLine = newWaterLine;
        
        targetUp = PhysicsHelper.GetNormal(waterLinePoints);

        //gravidade
        var gravity = Physics.gravity;
        rigidbody.drag = AirDrag;
        if (waterLine > center.y) {
            rigidbody.drag = WaterDrag;
            if (AttachToSurface) {
                rigidbody.position = new Vector3(rigidbody.position.x, waterLine - centerOffset.y, rigidbody.position.z);
            } else {
                gravity = AffectDirection ? targetUp * -Physics.gravity.y : -Physics.gravity;
                transform.Translate(0.9f * waterLinerDelta * Vector3.up);
            }
        }

        //LogDebugHelper.printVectors(gravity, "gravity");


        //rotação por ponto dentro da agua
        if (pointUnderWater) {
            if(SlideInWaves) gravity += Vector3.Scale(targetUp, new Vector3(0.5f, 0, 0.5f)) * -Physics.gravity.y;    //escorrega pela onda um pouco
            targetUp = Vector3.SmoothDamp(transform.up, targetUp, ref smoothVectorRotation, 0.5f);
            rigidbody.rotation = Quaternion.FromToRotation(transform.up, targetUp) * rigidbody.rotation;
        }
        if( !(waterLine > center.y) || !AttachToSurface)
            rigidbody.AddForce(2*gravity * Mathf.Clamp(Mathf.Abs(waterLine - center.y), 0, 1));
       
    }
    

    private void OnDrawGizmos(){
        Gizmos.color = Color.green;
        if (FloatPoints == null)
            return;

        for (int i = 0; i < FloatPoints.Length; i++){
            if (FloatPoints[i] == null)
                continue;

            if (ocean != null){
                //draw cube
                Gizmos.color = Color.red;
                Gizmos.DrawCube(waterLinePoints[i], Vector3.one * 0.3f);
            }
            //draw sphere
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(FloatPoints[i].position, 0.1f);
        }

        //draw center
        if (Application.isPlaying){
            Gizmos.color = Color.red;
            Gizmos.DrawCube(new Vector3(center.x, waterLine, center.z), Vector3.one * 1f);
            //Gizmos.DrawRay(new Vector3(center.x, waterLine, center.z), TargetUp * 1f);
        }
    }
}