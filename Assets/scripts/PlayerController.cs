using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PlayerController : MonoBehaviour {
  public float speed;

  private FootController foot;
  private new Rigidbody2D rigidbody;
  private GameObject otherPlayer;
  private bool isBurden;
  private int otherMask;
  private bool isLocked;
  private float oomph;

  virtual public void Start() {
    rigidbody = GetComponent<Rigidbody2D>();
    foot = transform.Find("foot").GetComponent<FootController>();
    otherPlayer = null;
    isLocked = false;
  }
  
  virtual public void Update() {
    if (isLocked) {
      return;
    }

    oomph = Input.GetAxis("Horizontal" + type);

    bool isGrounded = IsGrounded();
    if (Input.GetButton("Jump" + type) && isGrounded) {
      rigidbody.mass = 1;
      rigidbody.velocity = new Vector2(rigidbody.velocity.x, 6);
    }

    if (isGrounded && Input.GetButtonDown("Transmit" + type)) {
      isLocked = true;
      StartCoroutine(TransmitAndUnlock());
    }
  }

  IEnumerator TransmitAndUnlock() {
    // Squat
    Vector2 startPosition = gameObject.transform.position;
    Vector2 endPosition = (Vector2) gameObject.transform.position - Vector2.up * 0.1f;
    Vector3 startScale = gameObject.transform.localScale;
    Vector3 endScale = new Vector3(1.2f, 0.8f, 1.0f);

    float startTime = Time.time;
    float targetTime = 0.1f;
    float elapsedTime = 0.0f;

    // Squat down and widen.
    while (elapsedTime < targetTime) {
      gameObject.transform.position = Vector2.Lerp(startPosition, endPosition, elapsedTime / targetTime);
      gameObject.transform.localScale = Vector3.Lerp(startScale, endScale, elapsedTime / targetTime);
      yield return null;
      elapsedTime = Time.time - startTime;
    }

    // And return to form...
    startTime = Time.time;
    elapsedTime = 0.0f;
    while (elapsedTime < targetTime) {
      gameObject.transform.position = Vector2.Lerp(endPosition, startPosition, elapsedTime / targetTime);
      gameObject.transform.localScale = Vector3.Lerp(endScale, startScale, elapsedTime / targetTime);
      yield return null;
      elapsedTime = Time.time - startTime;
    }

    gameObject.transform.position = startPosition;
    gameObject.transform.localScale = startScale;

    if (isBurden && IsTransmittable()) {
      yield return StartCoroutine(Transmit());
    }

    isLocked = false;
  }

  void LateUpdate() {
    if (otherPlayer != null && isBurden) {
      oomph += otherPlayer.GetComponent<PlayerController>().oomph;
    }
    rigidbody.velocity = new Vector2(oomph * speed, rigidbody.velocity.y);
  }

  abstract public IEnumerator Transmit();
  abstract public bool IsTransmittable();

  bool IsGrounded() {
    Collider2D hit = Physics2D.OverlapBox(foot.position, new Vector2(foot.width, foot.height) * 0.5f, 0, Utilities.GROUND_MASK | otherMask);
    return hit != null;
  }

  void OnCollisionEnter2D(Collision2D collision) {
    // If we land on top of the other people, let's reduce our mass to 0 so we
    // don't impede that player's jump.
    if ((1 << collision.gameObject.layer) == otherMask) {
      otherPlayer = collision.gameObject;
      isBurden = transform.position.y > collision.gameObject.transform.position.y + 0.2;
      if (isBurden) {
        rigidbody.mass = 0;
      }
    }
  }

  void OnCollisionExit2D(Collision2D collision) {
    if (collision.gameObject == otherPlayer) {
      otherPlayer = null;
      rigidbody.mass = 1;
      isBurden = false;
    }
  }

  virtual public void OnTriggerExit2D(Collider2D collider) {
    if (collider.gameObject.CompareTag("topEdge")) {
      GameController.SINGLETON.Through();
    }
  }

  private string type;
  public string Type {
    get {
      return type;
    }

    set {
      type = value;
      if (type == "D") {
        otherMask = Utilities.PLAYER_R_MASK;
      } else {
        otherMask = Utilities.PLAYER_D_MASK;
      }
    }
  }
}
