using UnityEngine;
using UnityEngine.InputSystem;

public class PointGrabber : Grabber
{
    public LineRenderer laserPointer;
    public Material grabbablePointerMaterial;

    public InputActionProperty touchAction;
    public InputActionProperty grabAction;

    Material lineRendererMaterial;
    Transform grabPoint;
    Grabbable grabbedObject;

    Vector3 velocity;
    Vector3 previousPosition;

    // Start is called before the first frame update
    void Start()
    {
        laserPointer.enabled = false;
        lineRendererMaterial = laserPointer.material;

        grabPoint = new GameObject().transform;
        grabPoint.name = "Grab Point";
        grabPoint.parent = this.transform;
        grabbedObject = null;

        grabAction.action.performed += Grab;
        grabAction.action.canceled += Release;

        touchAction.action.performed += TouchDown;
        touchAction.action.canceled += TouchUp;
    }

    private void OnDestroy()
    {
        grabAction.action.performed -= Grab;
        grabAction.action.canceled -= Release;

        touchAction.action.performed -= TouchDown;
        touchAction.action.canceled -= TouchUp;
    }

    // Update is called once per frame
    void Update()
    {
        if (laserPointer.enabled)
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, Mathf.Infinity))
            {
                laserPointer.SetPosition(1, new Vector3(0, 0, hit.distance));

                if (hit.collider.GetComponent<Grabbable>())
                {
                    laserPointer.material = grabbablePointerMaterial;
                }
                else
                {
                    laserPointer.material = lineRendererMaterial;
                }
            }
            else
            {
                laserPointer.SetPosition(1, new Vector3(0, 0, 100));
                laserPointer.material = lineRendererMaterial;
            }
        }

        // smooth the velocity to allow for late button releases
        Vector3 newVelocity = (grabPoint.position - previousPosition) / Time.deltaTime;
        velocity = .5f * velocity + .5f * newVelocity;
        previousPosition = grabPoint.position;
    }

    public override void Grab(InputAction.CallbackContext context)
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, Mathf.Infinity))
        {
            if (hit.collider.GetComponent<Grabbable>())
            {
                grabPoint.localPosition = new Vector3(0, 0, hit.distance);

                if (hit.collider.GetComponent<Grabbable>().GetCurrentGrabber() != null)
                {
                    hit.collider.GetComponent<Grabbable>().GetCurrentGrabber().Release(new InputAction.CallbackContext());
                }

                grabbedObject = hit.collider.GetComponent<Grabbable>();
                grabbedObject.SetCurrentGrabber(this);

                if (grabbedObject.GetComponent<Rigidbody>())
                {
                    grabbedObject.GetComponent<Rigidbody>().isKinematic = true;
                    grabbedObject.GetComponent<Rigidbody>().useGravity = false;
                }

                grabbedObject.transform.parent = grabPoint;
            }
        }
    }

    public override void Release(InputAction.CallbackContext context)
    {
        if (grabbedObject)
        {
            if (grabbedObject.GetComponent<Rigidbody>())
            {
                grabbedObject.GetComponent<Rigidbody>().isKinematic = false;
                grabbedObject.GetComponent<Rigidbody>().useGravity = true;
                grabbedObject.GetComponent<Rigidbody>().velocity = velocity;
            }

            grabbedObject.SetCurrentGrabber(null);
            grabbedObject.transform.parent = null;
            grabbedObject = null;
        }
    }

    void TouchDown(InputAction.CallbackContext context)
    {
        laserPointer.enabled = true;
    }

    void TouchUp(InputAction.CallbackContext context)
    {
        laserPointer.enabled = false;
    }
}
