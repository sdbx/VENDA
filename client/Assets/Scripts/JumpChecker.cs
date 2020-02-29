using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpChecker : MonoBehaviour
{
    public bool isGrounded = false;
    public bool isJumping = false;

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.gameObject.tag == "Ground")
        {
            isGrounded = true;
        }
    }
    
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.tag == "Ground")
        {
            isGrounded = false;
        }
    }
}
