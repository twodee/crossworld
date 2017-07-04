﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Text.RegularExpressions;

public class CodeInputController : MonoBehaviour {
  public AmpersandController ampersand;
  public StarController star;

  private InputField input;

  private readonly Regex UNARY_INCREMENT = new Regex(@"^\s*(p\s*\+\+|\+\+\s*p)\s*;?\s*$");
  private readonly Regex UNARY_DECREMENT = new Regex(@"^\s*(--\s*p|p\s*--)\s*;?\s*$");
  private readonly Regex BINARY_INCREMENT = new Regex(@"^\s*p\s*\+=\s*(-?\d+)\s*;?\s*$");
  private readonly Regex BINARY_DECREMENT = new Regex(@"^\s*p\s*\-=\s*(-?\d+)\s*;?\s*$");
  private readonly Regex PULL = new Regex(@"^\s*value\s*=\s*\*\s*p\s*;?\s*$");
  private readonly Regex PUSH = new Regex(@"^\s*\*\s*p\s*=\s*value\s*;?\s*$");
  private readonly Regex POINT = new Regex(@"^\s*p\s*=\s*&word\[(\d+)\]\s*;?\s*$");
  private readonly Regex NULLIFY = new Regex(@"^\s*p\s*=\s*NULL\s*;?\s*$");

  void Start() {
    input = GetComponent<InputField>(); 
    input.onEndEdit.AddListener(OnInput);
  }

  void OnInput(string command) {
    bool isValid = true;

    if (UNARY_INCREMENT.IsMatch(command)) {
      ampersand.Increment(1, command); 
    } else if (UNARY_DECREMENT.IsMatch(command)) {
      ampersand.Increment(-1, command); 
    } else if (PULL.IsMatch(command)) {
      ampersand.Assign(command); 
    } else if (PUSH.IsMatch(command)) {
      star.Assign(command); 
    } else if (POINT.IsMatch(command)) {
      Match match = POINT.Match(command);
      int index = int.Parse(match.Groups[1].Value);
      Debug.Log("index: " + index);
      ampersand.TargetWord(index);
    } else if (BINARY_INCREMENT.IsMatch(command)) {
      Match match = BINARY_INCREMENT.Match(command);
      int delta = int.Parse(match.Groups[1].Value);
      ampersand.Increment(delta, command);
    } else if (BINARY_DECREMENT.IsMatch(command)) {
      Match match = BINARY_DECREMENT.Match(command);
      int delta = int.Parse(match.Groups[1].Value);
      ampersand.Increment(-delta, command);
    } else if (NULLIFY.IsMatch(command)) {
      ampersand.Depoint();
    } else {
      Debug.Log("I dunno " + command);
      isValid = false;
    }

    // If the input was okay, let's clear the field to get ready for
    // the next command. If it wasn't, we leave the text for them to
    // edit and resubmit.
    if (isValid) {
      input.text = "";
    }

    // Unity triggers this event on two occasions: hitting enter and losing focus.
    // If they hit enter, we expect the user to want to type in more -- like in a
    // REPL. We must actively retain focus in such a case, but not in the losing
    // focus case. That'd be bad.
    if (Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Return)) {
      input.ActivateInputField();
    }
  }

  public bool IsFocused {
    get {
      return input.isFocused;
    }
  }
}
