using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityTemplateProjects;

public class BoatMovement : MonoBehaviour
{
    protected WiiInputControler WiiControler;
    private Vector3 root_rotation;
    private Vector3 ControlerRotation;
    private Vector3 prevControlerRotation;

    public Transform RemoEsq;
    public Transform RemoDir;
    private Rigidbody _rigidbody;
    private WaterFloat _waterFloatScript;

    private Vector3 deltaRotation { get { return ControlerRotation - prevControlerRotation; } }

    public float Force = 1f;
    public float MaxSpeed = 10f;
    public float MinMovementPerSecTrigger = 30.0f;    //mínimo de 30° por segundo para considerar que remou 
    private float _minMovementPerSecTriggersin;
    private float[] movPersec;    //lista circular dos ultimos 'angulos por segundo' realizados
    private int movPersec_it = 0;
    private float movPersecAvg = 0;
    private int passedFrames = 0;    //conta até Application.targetFrameRate apenas, depois para
    
    // Start is called before the first frame update
    void Start() {
        Application.targetFrameRate = 60;
        movPersec = new float[30];
        gameObject.AddComponent<WiiInputControler>();
        WiiControler = gameObject.GetComponent<WiiInputControler>();

        _minMovementPerSecTriggersin = Mathf.Sin(Mathf.Deg2Rad * MinMovementPerSecTrigger);
        root_rotation = new Vector3(0, 1, 0);
        prevControlerRotation = ControlerRotation = WiiControler.GetAccelVectorNormalized();
    }

    public void Awake() {
        _rigidbody = GetComponent<Rigidbody>();
        _waterFloatScript = GetComponent<WaterFloat>();
    }

    public void OnCollisionStay(Collision other) {
        if (other.gameObject.CompareTag("Rock")) {
            Vector3 direction = _rigidbody.velocity;
            direction.y = 0;
            direction = Vector3.Reflect(direction, other.contacts[0].normal);
            _rigidbody.velocity = direction;
            _rigidbody.AddForce(direction/2);
        }
    }

    // Update is called once per frame
    void Update() {
        prevControlerRotation = ControlerRotation;
        ControlerRotation = WiiControler.GetAccelVectorNormalized();

        //calculando a primeira média de 'angulos por segundo'
        if (passedFrames < movPersec.Length) {
            movPersec[movPersec_it] = deltaRotation.z / Time.deltaTime;
            movPersecAvg += movPersec[movPersec_it] / movPersec.Length;
            passedFrames++;
        }
        else {
            movPersecAvg -= movPersec[movPersec_it] / movPersec.Length;    //remove o peso do 'angulos por segundo' mais antigo
            movPersec[movPersec_it] = deltaRotation.z / Time.deltaTime;    //grava o 'angulos por segundo' desde frame
            movPersecAvg += movPersec[movPersec_it] / movPersec.Length; //adiciona o peso do novo 'angulos por segundo'
        }
        
        movPersec_it = (++movPersec_it) % movPersec.Length;
        
        //Debug.Log("Avg movement: "+movPersecAvg + "        deltaRotation.z: " + deltaRotation.z);

        bool isRemoCabecaPraBaixo = ControlerRotation.y < 0;
        
        if ( _waterFloatScript.PointUnderWater && !isRemoCabecaPraBaixo && movPersecAvg >= _minMovementPerSecTriggersin) {
            if (ControlerRotation.x > 0.0075)
                Remar(Remada.ESQ);
            if (ControlerRotation.x < 0.0075)
                Remar(Remada.DIR);
        }
    }
    

    void Remar(Remada lado) {
        //Debug.Log("Remando no lado "+ (lado == Remada.ESQ? "esquerdo" : "direito"));
        //Vector3 finalTarget = transform.rotation * Vector3.up;
        //Vector3 target = Vector3.SmoothDamp()
        //Vector3.Scale( (transform.rotation * Vector3.one), Vector3.one)
        if(lado == Remada.DIR)
            _rigidbody.AddForceAtPosition(transform.forward * 10*Force, RemoDir.position);
        if(lado == Remada.ESQ)
            _rigidbody.AddForceAtPosition(transform.forward * 10*Force, RemoEsq.position);

        PhysicsHelper.ApplyForceToReachVelocity(_rigidbody, transform.forward * MaxSpeed, 5*Force);
    }
    

    public enum Remada
    {
        ESQ = 0, 
        DIR = 1
    };
    
}
