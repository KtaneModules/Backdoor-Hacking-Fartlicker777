using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using KModkit;
using UnityEngine.Video;

public class BackdoorHacking : MonoBehaviour {

   public KMBombInfo Bomb;
   public KMAudio Audio;

   public Camera Main;
   public Camera Animation;

   public GameObject Cube;

   public KMSelectable Button;

   public VideoPlayer Background;

   static int ModuleIdCounter = 1;
   int ModuleId;
   private bool ModuleSolved;

   void Awake () {
      ModuleId = ModuleIdCounter++;
      /*
      foreach (KMSelectable object in keypad) {
          object.OnInteract += delegate () { keypadPress(object); return false; };
      }
      */

      Button.OnInteract += delegate () { ButtonPress(); return false; };
      
   }

   void Start () {
      
   }

   void ButtonPress () {
      StartCoroutine(IBeViewingTheseBitches());
   }

   IEnumerator IBeViewingTheseBitches () {
      Animation.gameObject.SetActive(true);
      Audio.PlaySoundAtTransform("GettingGotAudio", Cube.transform);
      //Background.clip = VideoLoader.clips[0];
      Debug.Log("A");
      //Main.gameObject.SetActive(false);
      //Debug.Log("Main Failed");
      Background.Play();
      Debug.Log("B");
      yield return new WaitForSeconds(11.9f);
      Background.Stop();
      Debug.Log("C");
      //Main.gameObject.SetActive(true);
      Animation.gameObject.SetActive(false);
      Debug.Log("D");
   }

#pragma warning disable 414
   private readonly string TwitchHelpMessage = @"Use !{0} to do something.";
#pragma warning restore 414

   IEnumerator ProcessTwitchCommand (string Command) {
      yield return null;
   }

   IEnumerator TwitchHandleForcedSolve () {
      yield return null;
   }
}
