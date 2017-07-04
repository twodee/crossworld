using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PlayerController : MonoBehaviour {
  public float speed;
  public string declaration;
  public CodeInputController inputter;

  private FootController foot;
  private new Rigidbody2D rigidbody;
  private PlayerController other;
  private bool isBurden;
  private int otherMask;
  private bool isLocked;
  private float oomph;

  virtual public void Start() {
    rigidbody = GetComponent<Rigidbody2D>();
    foot = transform.Find("foot").GetComponent<FootController>();
    isLocked = false;
  }
  
  virtual public void Update() {
    if (isLocked) {
      return;
    }

    if (!inputter.IsFocused) {
      oomph = Input.GetAxis("Horizontal" + type);

      bool isGrounded = IsGrounded();
      if (Input.GetButtonDown("Jump" + type) && isGrounded) {
        if (other.isBurden) {
          other.rigidbody.mass = 0;
        }
        rigidbody.velocity = new Vector2(rigidbody.velocity.x, 6);
      }

      if (isGrounded && Input.GetButtonDown("Transmit" + type)) {
        isLocked = true;
        StartCoroutine(TransmitAndUnlock());
      }
    }
  }

  IEnumerator TransmitAndUnlock() {
    bool wasBurden = isBurden;
    rigidbody.velocity = Vector2.zero;
    other.rigidbody.velocity = Vector2.zero;

    // Squat
    Vector2 startPosition = gameObject.transform.position;
    Vector2 endPosition = (Vector2) gameObject.transform.position - Vector2.up * 0.1f;
    Vector3 startScale = gameObject.transform.localScale;
    Vector3 endScale = new Vector3(1.2f * Mathf.Sign(startScale.x), 0.8f, 1.0f);

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

    if (wasBurden && IsTransmittable()) {
      yield return StartCoroutine(Transmit());
    }

    isBurden = wasBurden;
    isLocked = false;
  }

  public void Assign(string command) {
    if (IsTransmittable()) {
      StartCoroutine(Transmit());
    } else {
      GameController.SINGLETON.Log("illegal: " + command);  
    }
  }

  void LateUpdate() {
    if (!isLocked) {
      if (isBurden) {
        oomph += other.oomph;
      }
      rigidbody.velocity = new Vector2(oomph * speed, rigidbody.velocity.y);
      if ((rigidbody.velocity.x < 0 && transform.localScale.x > 0) ||
          (rigidbody.velocity.x > 0 && transform.localScale.x < 0)) {
        transform.localScale = new Vector2(-transform.localScale.x, transform.localScale.y);
      }

      Transform sprite = transform.Find("sprite"); 
      if (sprite != null) {
        Animator animator = sprite.gameObject.GetComponent<Animator>(); 
        animator.SetBool("moving", Mathf.Abs(rigidbody.velocity.x) > 0.1f);
      }

      oomph = 0.0f;
    }
  }

  abstract public IEnumerator Transmit();
  abstract public bool IsTransmittable();

  bool IsGrounded() {
    Collider2D hit = Physics2D.OverlapBox(foot.position, new Vector2(foot.width, foot.height) * 0.5f, 0, Utilities.GROUND_MASK | Utilities.HOLE_MASK | otherMask);
    return hit != null;
  }

  void CheckBurden(Collision2D collision) {
    if ((1 << collision.gameObject.layer) == otherMask) {
      isBurden = transform.position.y > collision.gameObject.transform.position.y + 0.01;
    }
  }

  void OnCollisionEnter2D(Collision2D collision) {
    CheckBurden(collision);
  }
  
  void OnCollisionStay2D(Collision2D collision) {
    CheckBurden(collision);
  }

  void OnCollisionExit2D(Collision2D collision) {
    if (collision.gameObject == other.gameObject) {
      rigidbody.mass = 1;
      isBurden = false;
    }
  }

  virtual public void OnTriggerExit2D(Collider2D collider) {
    if (collider.gameObject.CompareTag("topEdge")) {
      GameController.SINGLETON.Through(declaration);
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

  public PlayerController Other {
    get {
      return other;
    }
    set {
      other = value;
    }
  }
}
