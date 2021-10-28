using UnityEngine;
using UnityEngine.InputSystem;

public class VirtualLocomotion : MonoBehaviour
{
    public Transform moveDirection;
    public float deadZone = 0.25f;
    public float maxVelocity = 3.0f;
    public bool flyMode = false;

    public enum TurnMode
    {
        CONTINUOUS_TURN,
        SNAP_TURN,
        NO_TURN
    }
    public TurnMode turnMode = TurnMode.CONTINUOUS_TURN;
    public float maxTurnSpeed = 45.0f;

    public InputActionProperty moveAction;

    bool snapTurnInitiated = false;

    // Start is called before the first frame update
    void Start()
    {
        snapTurnInitiated = false;
        moveAction.action.performed += Move;
    }

    private void OnDestroy()
    {
        moveAction.action.performed -= Move;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Move(InputAction.CallbackContext context)
    {
        Vector2 inputAxes = context.action.ReadValue<Vector2>();

        if (inputAxes.y >= deadZone || inputAxes.y <= -deadZone)
        {
            Vector3 moveVector = moveDirection.TransformDirection(Vector3.forward);

            if (!flyMode)
            {
                moveVector.y = 0;
                moveVector.Normalize();
            }

            float velocity = maxVelocity * inputAxes.y * Time.deltaTime;
            this.transform.localPosition += moveVector * velocity;
        }


        if (turnMode == TurnMode.CONTINUOUS_TURN)
        {
            if (inputAxes.x >= deadZone || inputAxes.x <= -deadZone)
            {
                float turnSpeed = maxTurnSpeed * inputAxes.x * Time.deltaTime;
                this.transform.Rotate(Vector3.up, turnSpeed);
            }
        }
        else if (turnMode == TurnMode.SNAP_TURN)
        {
            if (inputAxes.x >= .9f || inputAxes.x <= -.9f)
            {
                if (!snapTurnInitiated)
                {
                    if (inputAxes.x < 0)
                    {
                        this.transform.Rotate(Vector3.up, -maxTurnSpeed);
                    }
                    else
                    {
                        this.transform.Rotate(Vector3.up, maxTurnSpeed);
                    }
                    
                    snapTurnInitiated = true;
                }
            }
            else
            {
                snapTurnInitiated = false;
            }
        }
    }
}
