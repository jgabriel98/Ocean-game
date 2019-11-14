using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;
using WiimoteApi;
using WiimoteApi.Util;


public class WiiInputControler : MonoBehaviour
{
    private Wiimote _wiimote;
    public ReadOnlyCollection<float> CalibratedAccel;
    
    
    void Start()
    {
        //Marshal.PrelinkAll(typeof(HIDapi));
       StartCoroutine( InitWiimotes() );
    }
    
    void Update() {
        if (_wiimote == null) return;
        
        _wiimote.SendDataReportMode(InputDataType.REPORT_BUTTONS_ACCEL);
        _wiimote.SendStatusInfoRequest();
       
        while( _wiimote.ReadWiimoteData() > 0 );    //read all data to get up to date values;

        CalibratedAccel = new ReadOnlyCollection<float>(_wiimote.Accel.GetCalibratedAccelData());
        
        //LogDebugHelper.printVectors(new Vector3(CalibratedAccel[0], CalibratedAccel[1], CalibratedAccel[2]), "Accelerometer");
    }
    
    
    public Vector3 GetAccelVectorNormalized() {
        if (CalibratedAccel == null) return Vector3.zero;
        float accel_x = CalibratedAccel[0];
        float accel_y = CalibratedAccel[1];
        float accel_z = CalibratedAccel[2];

        return new Vector3(accel_x, accel_y, accel_z).normalized;
    }
    
    

    

    IEnumerator searchForWiiControler() {
        Wiimote newWiimote;
        
        WiimoteManager.FindWiimotes();
        while (WiimoteManager.Wiimotes.Count() == 0) {
            yield return new WaitForSeconds(1.5f);
            WiimoteManager.FindWiimotes();
        }

        Debug.Log("found wiiMote");
        newWiimote = WiimoteManager.Wiimotes.First();
        newWiimote.SendPlayerLED(true, false, false, false);
        newWiimote.SendDataReportMode(InputDataType.REPORT_BUTTONS_ACCEL);

        _wiimote = newWiimote;
        Debug.Log("Assigned wiiMote");
        _wiimote.RumbleOn = true;
        yield return new WaitForSeconds(0.15f);
        _wiimote.RumbleOn = false;


        //Debug.Log("Calibratin accelerometer, put the controller facing up, and the A button side facing you");
        //_wiimote.Accel.CalibrateAccel((AccelCalibrationStep)0);
        //_wiimote.Accel.CalibrateAccel((AccelCalibrationStep)1);
        //_wiimote.Accel.CalibrateAccel((AccelCalibrationStep)2);
    }

    IEnumerator keepStatusUpdated() {
        while (true) {
            _wiimote.SendStatusInfoRequest();
            _wiimote.ReadWiimoteData();
            Debug.Log(_wiimote.Status.ToString());
            yield return new WaitForSeconds(10);
        }
    }
    

    IEnumerator InitWiimotes() {
        yield return StartCoroutine(searchForWiiControler());
        StartCoroutine(keepStatusUpdated());
    }
    
    
    
    void OnApplicationQuit() {
        if (_wiimote != null) {
            WiimoteManager.Cleanup(_wiimote);
            _wiimote = null;
        }
    }
    
}