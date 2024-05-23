using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformOneWay : MonoBehaviour
{
    [SerializeField] private float dropDelayTimer;
    [SerializeField] private float restoreDelay = 0.25f;
    private float dropDelayCounter;

    private BoxCollider2D boxCol;
    private BoxCollider2D platformCol;
    private bool isDropping = false;

    void Start() {
        boxCol = GetComponent<BoxCollider2D>();
        dropDelayCounter = dropDelayTimer;
    }

    void Update() {
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) {
            if (!isDropping) DropThroughPlatform();
        }

        if (Input.GetKeyUp(KeyCode.S) || Input.GetKeyUp(KeyCode.DownArrow)) {
            dropDelayCounter = dropDelayTimer;
        }
    }

    void DropThroughPlatform() {
        if (dropDelayCounter <= 0) {
            if (platformCol != null)
                Physics2D.IgnoreCollision(boxCol, platformCol);
            dropDelayCounter = dropDelayTimer;
            isDropping = true;

            Invoke("RestoreCollision", restoreDelay);
        }else {
            dropDelayCounter -= Time.deltaTime;
        }
    }

    void RestoreCollision() {
        if (platformCol != null)
            Physics2D.IgnoreCollision(boxCol, platformCol, false);
        isDropping = false;
    }

    void OnCollisionEnter2D(Collision2D col) {
        if (col.gameObject.tag == "Platform") {
            if (!isDropping) platformCol = col.gameObject.GetComponent<BoxCollider2D>();
        }
    }
}
