using UnityEngine;
using System.Collections;

// A simple camera which always moves parallel with the XZ plane
// Created by Mike Yeates for FIT1033

[RequireComponent (typeof (CharacterController))]
public class FlyingCamera : MonoBehaviour {

    public int boostMultiplier; // Affects both movement and height scroll
	public float catchupSpeed; // Speed at which camera reaches target pos (increase for less acceleration)

    [Header("Movement")]
    public float moveSpeed = 1.0f;

    [Header("Height")]
    public float scrollSpeed = 20.0f;
    public float scrollwheelSensitivity;

    [Header("Mouse Look")]
    public float rotateSpeedHorizontal = 5.0f;
	public float rotateSpeedVertical = 5.0f;

    [Header("Pitch Angle Clamp")]
	public float minVerticalRot = -80.0f;
	public float maxVerticalRot = 80.0f;

    private Vector3 xzPlaneForward;
	private CharacterController characterController;

	private Vector3 targetPosition;
	private float rotationX; // Need to store actual pitch so we can clamp it using min and max verticalRot vars
	
	private bool isResettingPosition;
	private Vector3 initialPosition;
	private Quaternion initialRotation;

	void OnEnable()
	{
		rotationX = transform.eulerAngles.x;
		if(rotationX > 180.0f)
		{
			// Stop camera rotation from jumping if camera starts by looking upwards
			// Ensures euler angle read matches what we see in the Inspector
			rotationX += -360.0f;
		}
	}

    private void Start()
    {
		characterController = GetComponent<CharacterController>();
		
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;

		isResettingPosition = false;
		initialPosition = transform.position;
		initialRotation = transform.rotation;

		targetPosition = transform.position;	
    }

    void Update () 
	{
		if (Input.GetKey(KeyCode.Escape))
		{
			Application.Quit();

			#if UNITY_EDITOR
				UnityEditor.EditorApplication.isPlaying = false; 
			#endif
		}

		calculateXZPane();

        float moveSpeedWithBoost = moveSpeed;
        float scrollSpeedWithBoost = scrollSpeed;

        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            moveSpeedWithBoost = moveSpeed * boostMultiplier;
            scrollSpeedWithBoost = scrollSpeed * boostMultiplier;
        }

        Vector3 velocity = Vector3.zero;

		// Forward and backwards
		if(Input.GetAxis("Vertical") != 0f)
		{
			velocity = xzPlaneForward * -Input.GetAxis("Vertical") * moveSpeedWithBoost * Time.unscaledDeltaTime;
		}
		
		// Straife
		if(Input.GetAxis("Horizontal") != 0f)
		{
			velocity += transform.right * Input.GetAxis("Horizontal") * moveSpeedWithBoost * Time.unscaledDeltaTime;
		}
		
		// Height
		if(Input.GetAxis("Mouse ScrollWheel") != 0)
		{
			velocity.y = Input.GetAxis("Mouse ScrollWheel") * scrollwheelSensitivity * scrollSpeedWithBoost * Time.unscaledDeltaTime;
		}
		
		// Alternative height controls (plus and minus or q and e)
		if(Input.GetKey(KeyCode.Equals) || Input.GetKey(KeyCode.E))
		{
			// Using equals so user doesn't have to hold shift
			velocity.y = scrollSpeedWithBoost * Time.unscaledDeltaTime;
		}
		if(Input.GetKey(KeyCode.Minus) || Input.GetKey(KeyCode.Q))
		{
			velocity.y = -scrollSpeedWithBoost * Time.unscaledDeltaTime;
		}

		// Rotation (pitch and heading)
		if (Input.GetAxis("Mouse X") != 0 || Input.GetAxis("Mouse Y") != 0)
		{
			// Heading is horizontal mouse
			float rotationY = transform.localEulerAngles.y + Input.GetAxis("Mouse X") * rotateSpeedHorizontal;

			// Pitch is vertical mouse
			rotationX += -Input.GetAxis("Mouse Y") * rotateSpeedVertical;
			rotationX = Mathf.Clamp(rotationX, minVerticalRot, maxVerticalRot);

			transform.localEulerAngles = new Vector3(rotationX, rotationY, 0);
		}

		// Keep moving target position, which we're always moving towards
		targetPosition += velocity;

		// Gradually try and catch up to target position
		Vector3 nextPosition = Vector3.Lerp(transform.position, targetPosition, Time.unscaledDeltaTime * catchupSpeed);
        
		// Get a velocity describing how to go from where we are to where we want to be
		characterController.Move(nextPosition - transform.position); // Using character controller as it gives us collisions

		if(Input.GetKeyDown(KeyCode.F))
		{
			if(!isResettingPosition)
			{
				StartCoroutine(ResetCameraPosition());
			}
		}
    }

    private void calculateXZPane()
	{
		// Essentially our local forward direction but ignoring pitch (always aligned with a plane on the x and z axis, or always parallel with the ground)
		// We want this camera to look up and down independently of where it's heading
		xzPlaneForward = Vector3.Cross(Vector3.up, transform.right);
	}

	IEnumerator ResetCameraPosition()
	{
		const float FADE_SPEED = 0.5f;
		isResettingPosition = true;

		CameraFader.FadeToBlack(FADE_SPEED);
		yield return new WaitForSeconds(FADE_SPEED);

        // Teleport camera now screen is fully black
        characterController.enabled = false;

        transform.position = initialPosition;
		targetPosition = initialPosition;

		transform.rotation = initialRotation;
		rotationX = transform.eulerAngles.x;

        characterController.enabled = true;

        CameraFader.FadeToClear(FADE_SPEED);
		yield return new WaitForSeconds(FADE_SPEED);

		isResettingPosition = false;
	}

}
