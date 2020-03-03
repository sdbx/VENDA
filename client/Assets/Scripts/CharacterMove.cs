using UnityEngine;

///<summary>
///this class manages player's movement including jump and camera-following.
///</summary>
public class CharacterMove : MonoBehaviour
{
    public Transform mainCamera;
    private Rigidbody2D rb;
    private bool isJump;
    private bool keyDown;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    #region Jump&Move
    private void Update() { keyDown = Input.GetKey(KeyCode.UpArrow); }
    private void FixedUpdate()
    {
        rb.velocity = new Vector3(7f * Input.GetAxis("Horizontal"), rb.velocity.y);
        mainCamera.position = new Vector3(Mathf.Clamp(transform.position.x, 0, 18), Mathf.Max(transform.position.y, 0), -10);
        if (keyDown && !isJump && Mathf.Abs(rb.velocity.y) < 0.1f)
        {
            rb.AddForce(Vector2.up * 15, ForceMode2D.Impulse);
            isJump = true;
        }
    }
    private void OnCollisionEnter2D(Collision2D collision) { isJump = false;}
    #endregion
}
