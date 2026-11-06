using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class NeonPlayer : MonoBehaviour
{
    public float walkSpeed = 6f, sprintSpeed = 9f, jumpHeight = 1.2f, gravity = -22f, mouseSensitivity = 2.2f;
    public Transform cameraPivot;
    CharacterController controller;
    Vector3 velocity;
    float pitch;
    void Start()
    {
        controller = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    void Update()
    {
        float mx = Input.GetAxis("Mouse X") * mouseSensitivity;
        float my = Input.GetAxis("Mouse Y") * mouseSensitivity;
        transform.Rotate(Vector3.up * mx);
        pitch = Mathf.Clamp(pitch - my, -78f, 78f);
        if (cameraPivot) cameraPivot.localRotation = Quaternion.Euler(pitch,0,0);

        float x = Input.GetAxisRaw("Horizontal"), z = Input.GetAxisRaw("Vertical");
        Vector3 move = (transform.right*x + transform.forward*z).normalized;
        float speed = Input.GetKey(KeyCode.LeftShift) ? sprintSpeed : walkSpeed;
        controller.Move(move * speed * Time.deltaTime);

        if (controller.isGrounded && velocity.y < 0) velocity.y = -2f;
        if (controller.isGrounded && Input.GetKeyDown(KeyCode.Space)) velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
}
