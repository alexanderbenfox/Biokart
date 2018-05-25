using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class SuspensionPoint{
    public Vector3 point;
    public SuspensionDebug debugger;
    public SuspensionPoint(){
        point = Vector3.zero;
    }
}

struct SuspensionResults{
    public float compressionRatio;
    public Vector3 surfaceImpactPoint;
    public Vector3 surfaceImpactNormal;

    public static SuspensionResults zero(){
        SuspensionResults r;
        r.compressionRatio = 0f;
        r.surfaceImpactPoint = Vector3.zero;
        r.surfaceImpactNormal = Vector3.zero;
        return r;
    }
}

[RequireComponent(typeof(BoxCollider))]
[RequireComponent(typeof(Rigidbody))]
public class KartController : MonoBehaviour {
    private BoxCollider _collider;
    private Rigidbody _rb;
    private SuspensionPoint[] _suspensionPoints = new SuspensionPoint[4];
    private ParticleSystem _ps;

    public GameObject suspensionDebug;
    public float suspensionLength;
    public LayerMask groundLayer;
    [SerializeField]
    private float suspensionForce;
    public float dragFactor;
    public float accelerationParameter;
    public float brakingParameter;

    public bool debuggerTools = false;


    private void Awake()
    {
        _collider = this.GetComponent<BoxCollider>();
        _rb = this.GetComponent<Rigidbody>();
        _ps = this.GetComponentInChildren<ParticleSystem>();
        _ps.Stop();
        float com_y = _rb.centerOfMass.y - 1;
        _rb.centerOfMass = new Vector3(_rb.centerOfMass.x, _rb.centerOfMass.y - .5f, _rb.centerOfMass.z);
        for (int i = 0; i < 4; i++) _suspensionPoints[i] = new SuspensionPoint();
        UpdateSuspensionPoints();

        if (debuggerTools)
        {
            for (int i = 0; i < _suspensionPoints.Length; i++)
            {
                GameObject debug = Instantiate(suspensionDebug, _suspensionPoints[i].point, Quaternion.Euler(Vector3.zero));
                debug.transform.parent = this.transform;
                _suspensionPoints[i].debugger = debug.GetComponent<SuspensionDebug>();
                _suspensionPoints[i].debugger.Init();
            }
        }
    }

    private SuspensionResults GetCompressionRatio(ref Vector3 point){
        RaycastHit hit;
        SuspensionResults r;

        if(Physics.Raycast(point, -transform.up, out hit, suspensionLength, groundLayer)){
            r.surfaceImpactNormal = hit.normal;
            r.surfaceImpactPoint = hit.point;
            r.compressionRatio = 1f - hit.distance / suspensionLength;
            if(r.compressionRatio > .22 && r.compressionRatio < .28)
                Debug.DrawRay(point, -transform.up, Color.green, 0);
            else
                Debug.DrawRay(point, -transform.up, Color.red, 0);
            return r;
        }
        else{
            Debug.DrawRay(point, -transform.up, Color.red, 0);
            return SuspensionResults.zero();
        }
        
    }

    private void UpdateSuspensionPoints(){
        float xLeft = _collider.bounds.center.x - _collider.bounds.extents.x;
        float xRight = _collider.bounds.center.x + _collider.bounds.extents.x;
        float zFront = _collider.bounds.center.z + _collider.bounds.extents.z;
        float zBack = _collider.bounds.center.z - _collider.bounds.extents.z;
        float y = _collider.bounds.center.y - _collider.bounds.extents.y + 0.001f;

        //front left
        _suspensionPoints[0].point = new Vector3(xLeft, y, zFront);
        //front right
        _suspensionPoints[1].point = new Vector3(xRight, y, zFront);
        //back left
        _suspensionPoints[2].point = new Vector3(xLeft, y, zBack);
        //back right
        _suspensionPoints[3].point = new Vector3(xRight, y, zBack);
    }

    private Vector3 ResolveSuspension(){
        Vector3 normal = Vector3.zero;
        for (int i = 0; i < _suspensionPoints.Length; i++){
            //first get compression ratio between suspension and ground
            SuspensionResults results = GetCompressionRatio(ref _suspensionPoints[i].point);
            //apply upwards force to rigid body scaled by compression ratio at this point
            if (results.compressionRatio > 0)
            {
                normal = results.surfaceImpactNormal;
                suspensionForce = _rb.velocity.y + (_rb.mass * Physics.gravity.y);
                _rb.AddForceAtPosition(results.surfaceImpactNormal * results.compressionRatio * -suspensionForce, _suspensionPoints[i].point, ForceMode.Force);
            }
            if (debuggerTools)
                _suspensionPoints[i].debugger.UpdateData(results.compressionRatio);
                
        }
        return normal;
    }

    void UpdateSuspension(){
        for (int i = 0; i < _suspensionPoints.Length; i++){
            _suspensionPoints[i].point = _suspensionPoints[i].debugger.transform.position;
        }
    }

    Vector3 GetProjectionOntoPlane(Vector3 v, Vector3 n){
        return v - (Vector3.Dot(v, n) * n) / (n.sqrMagnitude);
    }

    void Accelerate(Vector3 normal){
        Vector3 accelerationOrigin = transform.position;
        Vector3 accelerationVector = GetProjectionOntoPlane(transform.forward, normal);
        _rb.AddForceAtPosition(accelerationVector*accelerationParameter, accelerationOrigin);
    }

    void Brake(Vector3 normal){
        Vector3 brakeOrigin = transform.position + transform.forward * .25f - transform.up * .25f;
        Vector3 accelerationVector = GetProjectionOntoPlane(-transform.forward, normal);
        _rb.AddForceAtPosition(accelerationVector * brakingParameter, brakeOrigin);
    }

    void ApplyDrag(){
        Vector3 currentVelocity = this.transform.InverseTransformVector(_rb.velocity);
        Vector3 dragForce = new Vector3(-currentVelocity.x, 0, 0) * dragFactor;
        dragForce = this.transform.TransformVector(dragForce);
        _rb.AddForce(dragForce);

    }

    // Update is called once per frame
    void FixedUpdate () {
        UpdateSuspension();
        Vector3 impactNormal = ResolveSuspension();
        /*if(Input.anyKeyDown){
            _rb.AddTorque((Vector3.left * 1f)+Vector3.back *100f + Vector3.up * 100f);
            _rb.AddForce(Vector3.up*100f);
        }*/
        if (impactNormal != Vector3.zero)
        {
            if (Input.GetKey(KeyCode.W))
            {
                Accelerate(impactNormal);
                _ps.Play();
            }
            else if (Input.GetKey(KeyCode.S))
            {
                Brake(impactNormal);
                _ps.Play();
            }
            else
                _ps.Stop();

            if(Input.GetKey(KeyCode.A)){
                _rb.AddTorque(-transform.up * 1f);
            }
            else if(Input.GetKey(KeyCode.D)){
                _rb.AddTorque(transform.up * 1f);
            }
        }
        ApplyDrag();
	}
}
