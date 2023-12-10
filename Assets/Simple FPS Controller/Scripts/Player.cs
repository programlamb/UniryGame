using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Rigidbody))]
public class Player : MonoBehaviour
{
    public string HaveCard = "";
    public KeyCode UseCard;
    public KeyCode Restart = KeyCode.R;
    public KeyCode DiscardCard;
    private static Vector3 vecZero = Vector3.zero;
    private Rigidbody rb;

    private bool enableMovement = true;

    [Header("Movement properties")]
    public float walkSpeed = 8.0f;
    public float runSpeed = 12.0f;
    public float changeInStageSpeed = 10.0f; // Lerp from walk to run and backwards speed
    public float maximumPlayerSpeed = 150.0f;
    [HideInInspector] public float vInput, hInput;
    public Transform groundChecker;
    public float groundCheckerDist = 0.2f;

    [Header("Jump")]
    public float jumpForce = 500.0f;
    public float jumpCooldown = 1.0f;
    private bool jumpBlocked = false;

    private void Start()
    {
        rb = this.GetComponent<Rigidbody>();
    }

    private bool isGrounded = false;
    public bool IsGrounded { get { return isGrounded; } }

    private Vector3 inputForce;
    private int i = 0;
    private float prevY;

    private void Update()
    {
        if (Input.GetKey(Restart))
        {
            LoadLevel(0);
        }
        if (Input.GetKey(UseCard) && HaveCard != "")
        {
            rb.velocity = vecZero;
            rb.AddForce(-jumpForce * rb.mass * Vector3.down);
            jumpBlocked = true;
            Invoke("UnblockJump", jumpCooldown);
            rb.velocity = Vector3.Lerp(rb.velocity, inputForce, changeInStageSpeed * Time.fixedDeltaTime);

            HaveCard = "";
        }
        if (Input.GetKey(DiscardCard) && HaveCard != "")
        {
            HaveCard = "";
        }
    }
    private void FixedUpdate()
    {
        isGrounded = (Mathf.Abs(rb.velocity.y - prevY) < .1f) && (Physics.OverlapSphere(groundChecker.position, groundCheckerDist).Length > 1); // > 1 because it also counts the player
        prevY = rb.velocity.y;

        // Input
        vInput = Input.GetAxisRaw("Vertical");
        hInput = Input.GetAxisRaw("Horizontal");

        // Clamping speed
        rb.velocity = ClampMag(rb.velocity, maximumPlayerSpeed);

        if (!enableMovement)
            return;
        inputForce = (transform.forward * vInput + transform.right * hInput).normalized * (Input.GetKey(SFPSC_KeyManager.Run) ? runSpeed : walkSpeed);

        if (isGrounded)
        {
            // Jump
            if ((Input.GetButton("Jump") && !jumpBlocked))
            {
                rb.AddForce(-jumpForce * rb.mass * Vector3.down);
                jumpBlocked = true;
                Invoke("UnblockJump", jumpCooldown);
            }
            // Ground controller
            rb.velocity = Vector3.Lerp(rb.velocity, inputForce, changeInStageSpeed * Time.fixedDeltaTime);
        }
        else
            // Air control
            rb.velocity = ClampSqrMag(rb.velocity + inputForce * Time.fixedDeltaTime, rb.velocity.sqrMagnitude);
    }

    private static Vector3 ClampSqrMag(Vector3 vec, float sqrMag)
    {
        if (vec.sqrMagnitude > sqrMag)
            vec = vec.normalized * Mathf.Sqrt(sqrMag);
        return vec;
    }

    private static Vector3 ClampMag(Vector3 vec, float maxMag)
    {
        if (vec.sqrMagnitude > maxMag * maxMag)
            vec = vec.normalized * maxMag;
        return vec;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("CardBox"))
        {
            HaveCard = "Jump";
            other.GetComponent<MeshRenderer>().enabled = false;
            Destroy(other, 0.1f);
        }
        if (other.CompareTag("DeathZone"))
        {
            LoadLevel(0);
        }
        if (other.CompareTag("Finish"))
        {
            LoadLevel(SceneManager.GetActiveScene().buildIndex + 1);
        }
    }

    private void LoadLevel(int i) {
        if (i >= 0 && i < 2)
        {
            SceneManager.LoadScene(i);
        }
    }

    private void UnblockJump()
    {
        jumpBlocked = false;
    }
    
    
    // Enables jumping and player movement
    public void EnableMovement()
    {
        enableMovement = true;
    }

    // Disables jumping and player movement
    public void DisableMovement()
    {
        enableMovement = false;
    }
}
