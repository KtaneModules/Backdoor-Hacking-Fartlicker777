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
   public Camera[] MiniCams;

   public GameObject Cube;

   public KMSelectable Button;

   public VideoPlayer Background;

   bool AnkhaZone;
   public TextMesh[] ZoneText;
   int[] ZoneWallCorrectSpots = new int[5];
   int ZoneClicks = -1;
   int ZoneClickIndex;

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
      //Stops an error
      //Main.GetComponent<AudioListener>().enabled = false;
      //Switches camera
      MiniCams[0].gameObject.SetActive(true);

      //Determines where the good spots are
      for (int i = 0; i < 5; i++) {
         ZoneText[i].text = "";
         ZoneWallCorrectSpots[i] = UnityEngine.Random.Range(1, 29);
      }
      Audio.PlaySoundAtTransform("GettingGotAudio", Cube.transform);
      Background.Play();
      yield return new WaitForSeconds(11.9f);
      Background.Stop();
      MiniCams[0].gameObject.SetActive(false);

      //Switches to Zonewall cam
      MiniCams[1].gameObject.SetActive(true);
      StartCoroutine(ZoneWall());
   }

   IEnumerator ZoneWall () { //Generates the 5 lines of [......]
      StartCoroutine(ZoneWallTextGenerate(0));
      StartCoroutine(ZoneWallTextGenerate(1));
      yield return new WaitForSeconds(.2f);
      Audio.PlaySoundAtTransform("Countdown", Cube.transform);
      StartCoroutine(ZoneWallTextGenerate(2));
      yield return new WaitForSeconds(.2f);
      StartCoroutine(ZoneWallTextGenerate(3));
      yield return new WaitForSeconds(.2f);
      StartCoroutine(ZoneWallTextGenerate(4));
      yield return new WaitForSeconds(4.9f);
      StartCoroutine(ZoneWallCursorIndex());
   }

   IEnumerator ZoneWallTextGenerate (int index) {
      for (int i = 0; i < 32; i++) {
         switch (i) {
            case 0:
               ZoneText[index].text += "[";
               break;
            case 31:
               ZoneText[index].text += "]";
               break;
            default:
               if (i - ZoneWallCorrectSpots[index] >= 0 && i - ZoneWallCorrectSpots[index] <= 2) {
                  ZoneText[index].text += "<color=red>.</color>";
               }
               else {
                  ZoneText[index].text += "<color=green>.</color>";
               }
               break;
         }
         yield return new WaitForSeconds(.01f);
      }
   }

   IEnumerator ZoneWallCursorIndex () {
      for (int i = 0; i < 5; i++) {
         while (ZoneClicks != i) {
            for (int j = 1; j < ZoneText[i].text.Length - 1; j++) {
               if (ZoneText[i].text[j] == '.') {
                  ZoneText[i].text = ZoneText[i].text.Substring(0, j) + "|" + ZoneText[i].text.Substring(j + 1);

                  for (int k = 0; k < j; k++) {
                     if (ZoneText[i].text[k] == '|') {
                        ZoneText[i].text = ZoneText[i].text.Substring(0, k) + "." + ZoneText[i].text.Substring(k + 1);
                     }
                  }
                  Debug.Log("what");
                  ZoneClickIndex++;
                  yield return new WaitForSecondsRealtime(.01f);
               }
               else {   //Prevents infinite loop in case of fuckup
                  yield return null;
                  continue;
               }
            }
            for (int j = ZoneText[i].text.Length - 2; j > 0; j--) {
               if (ZoneText[i].text[j] == '.') {
                  ZoneText[i].text = ZoneText[i].text.Substring(0, j) + "|" + ZoneText[i].text.Substring(j + 1);
                  int temp = ZoneText[i].text.LastIndexOf("|");
                  ZoneText[i].text = ZoneText[i].text.Remove(temp, 1).Insert(temp, ".");
                  ZoneClickIndex--;
                  yield return new WaitForSecondsRealtime(.01f);
               }
               else {   //Prevents infinite loop in case of fuckup
                  yield return null;
                  continue;
               }
            }
         }
      }
   }

   void Update () {

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
