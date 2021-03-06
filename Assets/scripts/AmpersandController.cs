﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class AmpersandController : PlayerController {
  public float barbLength;

  private LineRenderer lineRenderer;
  private LineRenderer leftBarbRenderer;
  private LineRenderer rightBarbRenderer;
  private CellController targetCell;
  private Vector2 targetPosition;
  private Coroutine caster;
  private StarController star;

  override public void Start() {
    base.Start();
    this.Type = "R";
    base.declaration = "char *p;";
    lineRenderer = GetComponent<LineRenderer>();
    leftBarbRenderer = transform.Find("leftbarb").GetComponent<LineRenderer>();
    rightBarbRenderer = transform.Find("rightbarb").GetComponent<LineRenderer>();
    star = GameObject.Find("/players/star").GetComponent<StarController>();
    targetCell = null;
    caster = null;
  }

  public void OnClick() {
    if (caster != null) {
      StopCoroutine(caster);
    }

    Vector2 from = transform.position;
    Vector3 mousePixels = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 10);
    Vector2 to = Camera.main.ScreenToWorldPoint(mousePixels);
    caster = StartCoroutine(CastPointer(from, to));
  }

  IEnumerator StepPointer(int delta) {
    int sign = delta > 0 ? 1 : -1;
    delta = delta > 0 ? delta : -delta;
    int width = GameController.SINGLETON.getBoard().ColumnCount;
    int iCurrent = targetCell.Row * width + targetCell.Column;
    int oldR = targetCell.Row;
    int oldC = targetCell.Column;
    Debug.Log("delta: " + delta);
    Debug.Log("oldR: " + oldR);
    Debug.Log("oldC: " + oldC);
    Vector2 from = targetPosition;

    while (delta > 0) {
      iCurrent += sign;
      int r = iCurrent / width;
      int c = iCurrent % width;
      Debug.Log("r: " + r);
      Debug.Log("c: " + c);
      float deltaX = (c - oldC) * (1 + GameController.GAP);
      float deltaY = (r - oldR) * (1 + GameController.GAP);
      Vector2 to = from + new Vector2(deltaX, deltaY);
      caster = StartCoroutine(CastPointer(from, to, targetCell.gameObject));
      from = to;
      oldC = c;
      oldR = r;
      yield return caster;
      --delta;
    } 
  }

  public void Increment(int delta, string originalCommand) {
    if (targetCell != null) {
      GameController.SINGLETON.Log(originalCommand); 
      StartCoroutine(StepPointer(delta));
    } else {
      GameController.SINGLETON.Log("illegal: " + originalCommand); 
    }
  }
  
  override public void Update() {
    base.Update();

    if (!inputter.IsFocused) {
      if (Input.GetKeyDown(KeyCode.Equals) && Input.GetKey(KeyCode.LeftShift)) {
        Increment(GameController.SINGLETON.getBoard().ColumnCount, "p += " + GameController.SINGLETON.getBoard().ColumnCount);
      } else if (Input.GetKeyDown(KeyCode.Equals)) {
        Increment(1, "p++");
      } else if (Input.GetKeyDown(KeyCode.Minus) && Input.GetKey(KeyCode.LeftShift)) {
        Increment(GameController.SINGLETON.getBoard().ColumnCount, "p -= " + GameController.SINGLETON.getBoard().ColumnCount);
      } else if (Input.GetKeyDown(KeyCode.Minus)) {
        Increment(1, "p--");
      }
    }

    // Cancel pointer on right-click.
    if (Input.GetMouseButtonDown(1)) {
      Depoint();  
    }

    // Only update pointer if we're not currently sending out a feeler ray.
    if (IsPointerAttached()) {
      Vector2 diff = targetPosition - (Vector2) transform.position;
      float magnitude = diff.magnitude;
      diff.Normalize();
      RaycastHit2D hit = Physics2D.Raycast(transform.position, diff, magnitude, Utilities.GROUND_MASK);

      // If our ray hits a different object than it did before, that means some
      // other object got in the way.
      if (hit && hit.collider.gameObject != targetCell.gameObject) {
        Depoint();  
      } else {
        Vector2 perp = new Vector3(-diff.y, diff.x);
        lineRenderer.SetPosition(0, (Vector2) transform.position + diff * 0.3f); 
        leftBarbRenderer.SetPosition(0, targetPosition + barbLength * (perp - 1.5f * diff)); 
        rightBarbRenderer.SetPosition(0, targetPosition - barbLength * (perp + 1.5f * diff)); 
      }
    }
  }

  public void TargetWord(int i) {
    if (caster != null) {
      StopCoroutine(caster);
    }

    Clue clue = GameController.SINGLETON.getBoard().getClue(i);
    if (clue != null) {
      Vector2 from = transform.position;
      Vector2 to = new Vector2(clue.x * (1 + GameController.GAP), 1 + clue.y * (1 + GameController.GAP));
      Debug.Log("clue.x: " + clue.x);
      Debug.Log("clue.y: " + clue.y);
      Debug.Log("to: " + to);
      caster = StartCoroutine(CastPointer(from, to));
    }
  }

  public void Depoint() {
    lineRenderer.enabled = false;
    leftBarbRenderer.enabled = false;
    rightBarbRenderer.enabled = false;
    targetCell = null;
    GameController.SINGLETON.Log("p = NULL;");
  }

  void PointAt(Vector2 position) {
    Vector2 diff = position - (Vector2) transform.position;
    diff.Normalize();
    Vector2 perp = new Vector3(-diff.y, diff.x);
    lineRenderer.SetPosition(0, (Vector2) transform.position + diff * 0.3f); 
    leftBarbRenderer.SetPosition(0, position + barbLength * (perp - 1.5f * diff)); 
    rightBarbRenderer.SetPosition(0, position - barbLength * (perp + 1.5f * diff)); 
    lineRenderer.SetPosition(1, position); 
    leftBarbRenderer.SetPosition(1, position); 
    rightBarbRenderer.SetPosition(1, position); 
  }

  IEnumerator CastPointer(Vector2 from, Vector2 to, GameObject skipObject = null) {
    lineRenderer.enabled = true;
    leftBarbRenderer.enabled = true;
    rightBarbRenderer.enabled = true;

    float maximumLength = Vector2.Distance(from, to);
    Vector2 diff = to - from;
    diff.Normalize();

    float startTime = Time.time;
    float elapsedTime = 0.0f;
    float targetTime = 0.5f;
    targetCell = null;
    Vector2 rayStart = transform.position;

    bool hitGround = false;
    while (elapsedTime < targetTime && !hitGround) {
      float proportion = elapsedTime / targetTime;
      float length = proportion * maximumLength;

      Vector2 rayStop = from + diff * length;
      Collider2D collider = Physics2D.OverlapPoint(rayStop, Utilities.GROUND_MASK);
      if (collider != null) {
        hitGround = true;
      } else {
        PointAt(from + diff * length); 
      }
      yield return null;
      elapsedTime = Time.time - startTime;
    }

    if (!hitGround) {
      int mask = Utilities.BLANK_HEAD_MASK;
      if (skipObject != null) {
        mask |= Utilities.BLANK_TAIL_MASK;
      }
      Collider2D collider = Physics2D.OverlapPoint(to, mask);

      if (collider != null) {
        PointAt(to); 
        targetCell = collider.gameObject.GetComponent<CellController>();
        targetPosition = to;

        // Log our anchoring to a head element, but only if we arrived here by
        // a direct hit instead of pointer arithmetic.
        if (collider.gameObject.layer == Utilities.BLANK_HEAD_LAYER && skipObject == null) {
          GameController.SINGLETON.Log("p = &word[" + targetCell.Address + "];");
        }
      }
    }

    if (targetCell == null) {
      GameController.SINGLETON.Log("p = NULL;");
    }

    lineRenderer.enabled = targetCell != null;
    leftBarbRenderer.enabled = targetCell != null;
    rightBarbRenderer.enabled = targetCell != null;
    caster = null;
  }

  public CellController Target {
    get {
      return targetCell;
    }
  }

  bool IsPointerAttached() {
    return caster == null && lineRenderer.enabled;
  }

  override public bool IsTransmittable() {
    return IsPointerAttached() && targetCell.Label != "";
  }

  override public IEnumerator Transmit() {
    yield return StartCoroutine(star.Release());

    GameObject cell = targetCell.gameObject.transform.Find("canvas/text").gameObject;
    GameObject payload = Instantiate(cell);
    Text payloadText = payload.GetComponent<Text>();
    payload.transform.SetParent(targetCell.gameObject.transform.Find("canvas"));
    payload.transform.position = cell.transform.position;

    Vector2 startPosition = payload.transform.position;
    Vector2 endPosition = star.LootPosition;
    Color startFillColor = GameController.SINGLETON.letterInCellColor;
    Color endFillColor = GameController.SINGLETON.transmittingFillColor;
    Color startStrokeColor = GameController.SINGLETON.letterInCellColor;
    Color endStrokeColor = GameController.SINGLETON.transmittingStrokeColor;
    Vector3 startScale = payload.GetComponent<RectTransform>().localScale;
    Vector3 endScale = GameController.SINGLETON.star.Loot.gameObject.GetComponent<RectTransform>().localScale;

    float startTime = Time.time;
    float targetTime = 1.0f;
    float elapsedTime = 0.0f;

    Outline outline = payload.GetComponent<Outline>();
    outline.enabled = true;

    GameController.SINGLETON.Log("value = *p; // value = '" + targetCell.Label + "'");

    while (elapsedTime <= targetTime) {
      float proportion = elapsedTime / targetTime;
      payload.transform.position = Vector2.Lerp(startPosition, endPosition, proportion);
      payloadText.color = Color.Lerp(startFillColor, endFillColor, proportion);
      outline.effectColor = Color.Lerp(startStrokeColor, endStrokeColor, proportion);
      payload.GetComponent<RectTransform>().localScale = Vector3.Lerp(startScale, endScale, proportion);
      yield return null;
      elapsedTime = Time.time - startTime;
    }

    payload.transform.position = endPosition;
    payloadText.color = endFillColor;
    outline.effectColor = endStrokeColor;

    star.Acquire(targetCell.Label);
    Destroy(payload);
  }

  void OnTriggerEnter2D(Collider2D collider) {
    if (collider.gameObject.layer == Utilities.BLANK_HEAD_LAYER) {
      collider.gameObject.layer = Utilities.BLANK_PAUSED_LAYER;
    }
  }

  override public void OnTriggerExit2D(Collider2D collider) {
    if (collider.gameObject.layer == Utilities.BLANK_PAUSED_LAYER) {
      collider.gameObject.layer = Utilities.BLANK_HEAD_LAYER;
    } else {
      base.OnTriggerExit2D(collider);
    }
  }
}
