using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(SphereCollider))]
public class TheBall : MonoBehaviour
{
    public GameObject CollideParticleSysInstance;

    public Camera cam;
    Vector3 camIdlePos;
    float radius = 0.5f;
    collisionPoint[] resistPoints;
    // Start is called before the first frame update
    MeshFilter meshFilt;

    class collisionPoint
    {
        public Vector3 pos;
        public Vector3 normal;

        public float deformationCoef;
        public collisionPoint(ContactPoint contactPoint, Vector3 position, Quaternion rotation)
        {
            pos = contactPoint.point - position;
            normal = contactPoint.normal;
            //to local values
            normal = rotVector(rotation, normal + pos);
            pos = rotVector(rotation, pos);
            normal = normal - pos;
        }
    }
    Vector3[] defaultVert;
    float distToFullRot;

    SphereCollider col;
    private void Start()
    {
        camIdlePos = cam.transform.position - transform.position;
        meshFilt = GetComponent<MeshFilter>();
        Mesh m = meshFilt.mesh;
        defaultVert = m.vertices;

        col = GetComponent<SphereCollider>();
        radius = col.radius;
        distToFullRot = 2 * Mathf.PI * Mathf.Pow(radius, 2);
    }
    void FixedUpdate()
    {
        DefaultControls();
        Move();
        Deforme(resistPoints);
        resistPoints = null;
        CamMove();
        
    }

    public float pushPower;
    public float maxSpeed;
    Vector3 impuls;
    public float rotSpeedCoef;



    public LineRenderer rend;

    float multFactor = 1;

    Vector3 default_powerVect;
    bool keyUp = false;
    public void DefaultControls()
    {
        float XAxis = 0;
        float YAxis = 0;
        impuls += new Vector3(XAxis, 0, YAxis) * pushPower * Time.fixedDeltaTime;
        impuls -= impuls.normalized * Time.fixedDeltaTime * 0.01f;
        Vector3 dir = new Vector3();

        if (Input.GetKey(KeyCode.Mouse0))
        {
            rend.enabled = true;
            Ray r = cam.ScreenPointToRay(Input.mousePosition);
            RaycastHit H;
            Physics.Raycast(r, out H, 100, LayerMask.GetMask("Floor"));
            if (H.collider != null)
            {
                dir = H.point;
                dir.y = transform.position.y;
                default_powerVect = dir - transform.position;
                showPushDirection(default_powerVect);
                keyUp = true;
            }
        }
        else
        {
            if (keyUp == true)
            {
                Push(default_powerVect);
                keyUp = false;
            }
            rend.enabled = false;
        }


    }

    //drawing line thet shows push direction
    public void showPushDirection(Vector3 dirVect)
    {

        Vector3 targetPos = dirVect + transform.position;
        RaycastHit Hh;
        Physics.Raycast(transform.position, dirVect, out Hh, 100, LayerMask.GetMask("Default"));
        if (Hh.collider != null && dirVect.magnitude > Hh.distance)
        {
            rend.SetPosition(1, Hh.point);
        }
        else
        {
            rend.SetPosition(1, targetPos);
        }
        //set impuls line color and width
        float powerCoef = dirVect.magnitude / 3 * radius;
        if (powerCoef > radius)
        {
            powerCoef = radius;
        }
        AnimationCurve curve = new AnimationCurve();
        curve.AddKey(1, 0.05f);
        curve.AddKey(0, powerCoef);
        rend.widthCurve = curve;

        rend.startColor = Color.Lerp(Color.green, Color.red, powerCoef);
        rend.endColor = Color.green;

    }
    //apply push
    public void Push(Vector3 powerVect)
    {
        impuls += new Vector3(powerVect.x, 0, powerVect.z) * pushPower * Time.fixedDeltaTime;
    }
    public void Move()
    {
        //preventing passage through walls
        RaycastHit hit;
        Physics.Raycast(transform.position, impuls.normalized, out hit, impuls.magnitude * 1.1f, LayerMask.GetMask("Default"));
        if (hit.collider != null && hit.distance < impuls.magnitude + (deformation * radius + 0.05f))
        {
            multFactor = 0.001f;
            transform.position += impuls * (hit.distance / (impuls.magnitude + (deformation * radius + 0.05f))) * multFactor;
        }
        else//default movement
        {
            multFactor = 1;
            transform.position += impuls;
        }

        float halfY = impuls.y / 2;

        Vector3 eulerAngls = transform.rotation.eulerAngles;
        transform.RotateAround(transform.position, new Vector3(0, 0, 1), -(impuls.x + halfY / distToFullRot) * 360 * rotSpeedCoef);
        transform.RotateAround(transform.position, new Vector3(1, 0, 0), (impuls.z + halfY / distToFullRot) * 360 * rotSpeedCoef);
        if (impuls.magnitude / Time.fixedDeltaTime > maxSpeed)
        {
            impuls = impuls.normalized * maxSpeed * Time.fixedDeltaTime;
        }
        rend.SetPosition(0, transform.position);
    }

    public void CamMove()
    {
        float speed = (impuls.magnitude/Time.fixedDeltaTime);
        Vector3 heightBySpeed = new Vector3(0, speed / 5, 0);
        Vector3 targetPosition = transform.position + camIdlePos + heightBySpeed;
        cam.transform.position = Vector3.Lerp(cam.transform.position, targetPosition, Time.fixedDeltaTime * speed);
    }

    void Deforme(collisionPoint[] resistPoints)
    {
        Mesh m = meshFilt.mesh;
        Vector3[] vertices = (Vector3[])defaultVert.Clone();
        if (resistPoints == null || resistPoints.Length == 0)//no forces, shape recovery
        {
            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i] = Vector3.Lerp(vertices[i],defaultVert[i], Time.fixedDeltaTime);
            }
        }
        else//deformation
        {
            for (int a = 0; a < resistPoints.Length; a++)
            {
                for (int i = 0; i < vertices.Length; i++)
                {
                    float coef = 1 - ((resistPoints[a].pos - defaultVert[i]).magnitude/ radius);
                    if (coef < -1)
                    {
                        coef = -1;
                    }
                    vertices[i] += (resistPoints[a].normal * coef * resistPoints[a].deformationCoef);
                }
            }

        }
        m.vertices = vertices;
        meshFilt.mesh = m;
    }

    [Range(0, 0.9f)] public float minDeformation;
    [Range(0, 0.9f)] public float maxDeformation;
    float deformation;
    private void OnCollisionStay(Collision collision)
    {
        deformation = Mathf.Lerp(minDeformation, maxDeformation,(impuls.magnitude/Time.fixedDeltaTime/maxSpeed));
        ContactPoint[] collisionContacts = collision.contacts;
        resistPoints = new collisionPoint[collisionContacts.Length];
        Vector3 sum = new Vector3();
        for (int i = 0; i < resistPoints.Length;i++)
        {
            resistPoints[i] = new collisionPoint(collisionContacts[i], transform.position, transform.rotation);
            RaycastHit hit;
            Physics.Raycast(transform.position, collisionContacts[i].point - transform.position, out hit, 10,LayerMask.GetMask("Default"));
            if (hit.collider != null)
            {
                float deformC = 1 - (hit.distance / radius);
                if (deformC < 0)
                {
                    deformC = 0;
                }
                resistPoints[i].deformationCoef = deformC;
                if (deformC > deformation)
                {
                    sum += collisionContacts[i].normal;
                }
            }
        }


        if (sum.magnitude != 0 && Vector3.Angle(impuls,sum) > 90)
        {
            for (int i = 0; i < collisionContacts.Length; i++)
            {
                StartCoroutine(collisionParticleEffect(collisionContacts[i].point, collisionContacts[i].normal,impuls));
            }
            impuls = Vector3.Reflect(impuls * 0.9f, sum);
            impuls.y = 0;
        }
    }

    //rotates vector 
    public static Vector3 rotVector(Quaternion rotation, Vector3 vector)
    {
        float dist = vector.magnitude;
        vector.Normalize();
        Quaternion quat = new Quaternion(vector.x, vector.y, vector.z,0);
        quat = Quaternion.Inverse(rotation) * quat * rotation;
        return new Vector3(quat.x,quat.y,quat.z) * dist;
    }

    IEnumerator collisionParticleEffect(Vector3 pos,Vector3 normal,Vector3 impls)
    {
        //creating particleSystem in collision position
        GameObject obj = Instantiate(CollideParticleSysInstance);
        obj.transform.position = pos;
        obj.transform.rotation = Quaternion.FromToRotation(new Vector3(0, 0, 1), normal);
        ParticleSystem PSys = obj.GetComponent<ParticleSystem>();
        //particles size setup(faster movement - bigger particles)
        ParticleSystem.MainModule mModul = PSys.main;
        float LerpCoef = impls.magnitude / (maxSpeed*Time.fixedDeltaTime);
        ParticleSystem.MinMaxCurve curve = mModul.startSize;
        curve.constantMax = Mathf.Lerp(0.02f, 0.6f, LerpCoef);
        curve.constantMin = curve.constantMax/2;
        mModul.startSize = curve;
        //start
        PSys.Play();
        yield return new WaitForSeconds(PSys.main.startLifetimeMultiplier + PSys.main.duration);
        //end object destroy
        Destroy(obj);
    }
}
