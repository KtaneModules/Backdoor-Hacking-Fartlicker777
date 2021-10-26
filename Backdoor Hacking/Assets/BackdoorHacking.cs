using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using KModkit;
using Rnd = UnityEngine.Random;
using UnityEngine.Video;

/*
 * I really should start using separate scripts. 
 */

public class BackdoorHacking : MonoBehaviour {

   public KMBombInfo Bomb;
   public KMAudio Audio;

   public Camera Main;
   public Camera[] MiniCams;

   public GameObject[] Cube;

   public KMSelectable Button;

   public VideoPlayer Background;

   public GameObject[] TheWholeAssModMinusTheMod;

   public GameObject[] InnerNodes;

   public KMSelectable[] BuyButtons;

   public GameObject Bar;
   public Material[] BarColors;
   public TextMesh ConnectionText;

   static bool BeingHacked;

   enum HackState {
      Idle,
      GettingHacked,
      ZoneWall,
      MemoryFragger,
      NodeHacker,
      StackPusher
   }

   HackState CurrentState;

   public TextMesh CurrentVBucks;

   //Result shit
   bool BlockedHack;
   public TextMesh HackResultText;
   static double DOSCoinAmount;
   static int DOSCointGoal;
   bool Multiplier;
   bool DecreaseCooldown;
   bool Connected;
   bool Waiting;

   //Zonewall Shit
   bool AnkhaZone;
   public TextMesh[] ZoneText;
   int[] ZoneWallCorrectSpots = new int[5];
   int ZoneClicks = -1;
   int ZoneCorrectClicks;
   //int ZoneClickIndex;

   //Memory Fragger shit
   public TextMesh MainNodeText;
   public TextMesh[] MiniNodeTexts;
   public GameObject[] Nodes;
   string Numerical = "0123456789";
   string Alphabetical = "qwertyuiopasdfghjklzxcvbnm".ToUpper();
   string Alphanumerical = "0123456789qwertyuiopasdfghjklzxcvbnm".ToUpper();
   string Goal = "";
   bool[] Taken = new bool[8];
   string MemoryInput = "";
   public Material[] ColorsForMemory;

   //Node Hacker Shit
   public GameObject[] Centers;
   public GameObject[] AlsoCenterGodDamnIt;
   public GameObject[] Diagonals;
   public GameObject[] Orthogonals;
   public GameObject[] Highlights;
   public Material[] NodeHackerColors;
   bool[] OorD = new bool[25];
   bool[] Visited = new bool[25];
   List<int> Path = new List<int> { };
   int CurrentNode = 0;
   int[] Spots = new int[25];

   //Stack Pusher Shit
   public SpriteRenderer[] StackNodes;
   public Sprite[] StackPictures;
   enum StackNodeStates {
      Empty,
      EmptyH,
      Goal,
      GoalH,
      You,
      YouH,
      Stack,
      StackH
   }
   StackNodeStates[] StackGrid =
   {
      StackNodeStates.Empty, StackNodeStates.Empty, StackNodeStates.Empty, StackNodeStates.Empty, StackNodeStates.Empty,
      StackNodeStates.Empty, StackNodeStates.Empty, StackNodeStates.Empty, StackNodeStates.Empty, StackNodeStates.Empty,
      StackNodeStates.Empty, StackNodeStates.Empty, StackNodeStates.Goal, StackNodeStates.Empty, StackNodeStates.Empty,
      StackNodeStates.Empty, StackNodeStates.Empty, StackNodeStates.Empty, StackNodeStates.Empty, StackNodeStates.Empty,
      StackNodeStates.Empty, StackNodeStates.Empty, StackNodeStates.Empty, StackNodeStates.Empty, StackNodeStates.Empty
   };
   int[] OpenStackSpots = new int[24];
   int StackYou;
   int ActualSelection;
   public GameObject StackPusherThing;
   bool IsMovingYou;
   bool CanMoveStacks;
   enum TypesOfItems {
      Empty,
      Move,
      Stack
   }
   TypesOfItems HeldItem;
   int IndexOfHeldStack = -1;
   int NodesDunked;

   static int ModuleIdCounter = 1;
   int ModuleId;
   private bool ModuleSolved;

   private KeyCode[] TheKeys =
  {
        //KeyCode.Backspace, KeyCode.Return,
        KeyCode.Q, KeyCode.W, KeyCode.E, KeyCode.R, KeyCode.T, KeyCode.Y, KeyCode.U, KeyCode.I, KeyCode.O, KeyCode.P,
        KeyCode.A, KeyCode.S, KeyCode.D, KeyCode.F, KeyCode.G, KeyCode.H, KeyCode.J, KeyCode.K, KeyCode.L,
        KeyCode.Z, KeyCode.X, KeyCode.C, KeyCode.V, KeyCode.B, KeyCode.N, KeyCode.M,
        KeyCode.Space, KeyCode.Alpha1, KeyCode.Keypad1, KeyCode.Alpha2, KeyCode.Keypad2, KeyCode.Alpha3, KeyCode.Keypad3,
        KeyCode.Alpha4, KeyCode.Keypad4, KeyCode.Alpha5, KeyCode.Keypad5, KeyCode.Alpha6, KeyCode.Keypad6, KeyCode.Alpha7, KeyCode.Keypad7,
        KeyCode.Alpha8, KeyCode.Keypad8, KeyCode.Alpha9, KeyCode.Keypad9, KeyCode.Alpha0, KeyCode.Keypad0
  };

   string TheLetters = "qwertyuiopasdfghjklzxcvbnm 11223344556677889900".ToUpper();


   void Awake () {
      ModuleId = ModuleIdCounter++;

      foreach (KMSelectable Button in BuyButtons) {
         Button.OnInteract += delegate () { Buy(Button); return false; };
      }

      Button.OnInteract += delegate () { ButtonPress(); return false; };
   }//-139.157 -139.725

   void Start () {
      DOSCointGoal = 30 + Bomb.GetSolvableModuleNames().Count() * 15;
      Debug.LogFormat("[Backdoor Hacking #{0}] Listen to Shitty Beats today! https://www.youtube.com/playlist?list=PL6giE1a_sXZxLMIpgOvrprJqx26XipcEz", ModuleId);
      StartCoroutine(Timer());
   }

   #region Buttons

   void Buy (KMSelectable Button) {
      for (int i = 0; i < 3; i++) {
         if (Button == BuyButtons[0]) {
            if (DOSCoinAmount >= 100 && Multiplier) {
               Multiplier = true;
               DOSCoinAmount -= 100;
            }
         }
         if (Button == BuyButtons[1]) {
            if (DOSCoinAmount >= 75 && DecreaseCooldown) {
               DecreaseCooldown = true;
               DOSCoinAmount -= 75;
            }
         }
         if (Button == BuyButtons[2]) {
            if (DOSCoinAmount >= DOSCointGoal) {
               GetComponent<KMBombModule>().HandlePass();
               DOSCoinAmount -= DOSCointGoal;
               ModuleSolved = true;
            }
         }
      }
   }

   void ButtonPress () {
      if (Waiting) {
         return;
      }
      //StartCoroutine(HackResult());
      //StartCoroutine(IBeViewingTheseBitches());
      if (!Connected) {
         if (Rnd.Range(0, 5) == 0) {
            CallHack();
         }
         else {
            StartCoroutine(Wait());
         }
      }
      else {
         StartCoroutine(Wait());
      }
      Connected ^= true;
      if (Connected) {
         ConnectionText.text = "Disconnect";
      }
      else {
         ConnectionText.text = "Connect";
      }
   }

   #endregion

   #region General Animations

   IEnumerator Timer () {
      int time = Rnd.Range(60, 151);
      yield return new WaitForSeconds(time);
      CallHack();
   }

   void CallHack () {
      StopAllCoroutines();
      Bar.GetComponent<MeshRenderer>().material = BarColors[2];
      StartCoroutine(IBeViewingTheseBitches());
   }

   IEnumerator Wait () {
      Waiting = true;
      Bar.GetComponent<MeshRenderer>().material = BarColors[0];
      if (DecreaseCooldown) {
         yield return new WaitForSeconds(2.5f);
         Bar.GetComponent<MeshRenderer>().material = BarColors[1];
         yield return new WaitForSeconds(2.5f);
         Bar.GetComponent<MeshRenderer>().material = BarColors[2];
      }
      else {
         yield return new WaitForSeconds(5f);
         Bar.GetComponent<MeshRenderer>().material = BarColors[1];
         yield return new WaitForSeconds(5f);
         Bar.GetComponent<MeshRenderer>().material = BarColors[2];
      }
      Waiting = false;
   }

   IEnumerator HackResult () {
      for (int j = 0; j < MiniCams.Length; j++) {
         MiniCams[j].gameObject.SetActive(false);
      }
      MiniCams[3].gameObject.SetActive(true);
      CurrentState = HackState.GettingHacked;
      Audio.PlaySoundAtTransform("ResultsSound", Cube[3].transform);
      HackResultText.text = BlockedHack ? "Hack Blocked" : "Hacked";
      double doubletemp = 0;

      if (HackResultText.text == "Hacked") {
         doubletemp = Rnd.Range(20f, 30f);
         doubletemp -= doubletemp % .001;
         HackResultText.text += "\n-" + doubletemp.ToString();
         DOSCoinAmount -= doubletemp;
      }
      else {
         switch (ZoneCorrectClicks) {
            case 0:
               switch (Rnd.Range(0, 3)) {
                  case 0:
                     doubletemp = Rnd.Range(70f, 110f) * (Multiplier ? 1.1 : 1.0);
                     doubletemp -= doubletemp % .001;
                     HackResultText.text += "\n+" + doubletemp.ToString();
                     DOSCoinAmount += doubletemp;
                     break;
                  case 1:
                     doubletemp = Rnd.Range(60f, 70f) * (Multiplier ? 1.1 : 1.0);
                     doubletemp -= doubletemp % .001;
                     HackResultText.text += "\n+" + doubletemp.ToString();
                     DOSCoinAmount += doubletemp;
                     break;
                  case 2:
                     doubletemp = Rnd.Range(55f, 60f) * (Multiplier ? 1.1 : 1.0);
                     doubletemp -= doubletemp % .001;
                     HackResultText.text += "\n+" + doubletemp.ToString();
                     DOSCoinAmount += doubletemp;
                     break;
               }
               break;
            case 1:
               switch (Rnd.Range(0, 3)) {
                  case 0:
                     doubletemp = Rnd.Range(70f, 90f) * (Multiplier ? 1.1 : 1.0);
                     doubletemp -= doubletemp % .001;
                     HackResultText.text += "\n+" + doubletemp.ToString();
                     DOSCoinAmount += doubletemp;
                     break;
                  case 1:
                     doubletemp = Rnd.Range(50f, 70f) * (Multiplier ? 1.1 : 1.0);
                     doubletemp -= doubletemp % .001;
                     HackResultText.text += "\n+" + doubletemp.ToString();
                     DOSCoinAmount += doubletemp;
                     break;
                  case 2:
                     doubletemp = Rnd.Range(45f, 50f) * (Multiplier ? 1.1 : 1.0);
                     doubletemp -= doubletemp % .001;
                     HackResultText.text += "\n+" + doubletemp.ToString();
                     DOSCoinAmount += doubletemp;
                     break;
               }
               break;
            case 2:
               switch (Rnd.Range(0, 3)) {
                  case 0:
                     doubletemp = Rnd.Range(40f, 50f) * (Multiplier ? 1.1 : 1.0);
                     doubletemp -= doubletemp % .001;
                     HackResultText.text += "\n+" + doubletemp.ToString();
                     DOSCoinAmount += doubletemp;
                     break;
                  case 1:
                     doubletemp = Rnd.Range(30f, 40f) * (Multiplier ? 1.1 : 1.0);
                     doubletemp -= doubletemp % .001;
                     HackResultText.text += "\n+" + doubletemp.ToString();
                     DOSCoinAmount += doubletemp;
                     break;
                  case 2:
                     doubletemp = Rnd.Range(20f, 30f) * (Multiplier ? 1.1 : 1.0);
                     doubletemp += doubletemp % .001;
                     HackResultText.text += "\n+" + doubletemp.ToString();
                     DOSCoinAmount += doubletemp;
                     break;
               }
               break;
            case 4:
               switch (Rnd.Range(0, 3)) {
                  case 0:
                     doubletemp = Rnd.Range(20f, 30f) * (Multiplier ? 1.1 : 1.0);
                     doubletemp -= doubletemp % .001;
                     HackResultText.text += "\n+" + doubletemp.ToString();
                     DOSCoinAmount += doubletemp;
                     break;
                  case 1:
                     doubletemp = Rnd.Range(15f, 20f) * (Multiplier ? 1.1 : 1.0);
                     doubletemp -= doubletemp % .001;
                     HackResultText.text += "\n+" + doubletemp.ToString();
                     DOSCoinAmount += doubletemp;
                     break;
                  case 2:
                     doubletemp = Rnd.Range(10f, 15f) * (Multiplier ? 1.1 : 1.0);
                     doubletemp -= doubletemp % .001;
                     HackResultText.text += "\n+" + doubletemp.ToString();
                     DOSCoinAmount += doubletemp;
                     break;
               }
               break;
         }
      }
      HackResultText.transform.localPosition = new Vector3(0, 1f, -139.157f);
      for (int i = 0; i < 5; i++) {
         HackResultText.transform.localPosition -= new Vector3(0, 0, 0.1f);
         yield return new WaitForSeconds(.01f);
      }
      yield return new WaitForSeconds(2.3f);
      BlockedHack = false;
      ResetAll();
      CurrentState = HackState.Idle;
      MiniCams[3].gameObject.SetActive(false);
      if (Application.isEditor) {
         Main.GetComponent<AudioListener>().enabled = true;
      }
      StartCoroutine(Timer());
   }

   IEnumerator Instablock () {
      for (int j = 0; j < MiniCams.Length; j++) {
         MiniCams[j].gameObject.SetActive(false);
      }
      MiniCams[3].gameObject.SetActive(true);
      CurrentState = HackState.GettingHacked;
      Audio.PlaySoundAtTransform("ResultsSound", Cube[3].transform);
      HackResultText.text = "Instablocked";
      double doubletemp = 0;

      if (HackResultText.text == "Hacked") {
         doubletemp = Rnd.Range(20f, 30f);
         doubletemp -= doubletemp % .001;
         HackResultText.text += "\n-" + doubletemp.ToString();
         DOSCoinAmount -= doubletemp;
      }
      HackResultText.transform.localPosition = new Vector3(0, 1f, -139.157f);
      for (int i = 0; i < 5; i++) {
         HackResultText.transform.localPosition -= new Vector3(0, 0, 0.1f);
         yield return new WaitForSeconds(.01f);
      }
      yield return new WaitForSeconds(2.3f);
      BlockedHack = false;
      ResetAll();
      CurrentState = HackState.Idle;
      MiniCams[3].gameObject.SetActive(false);
      if (Application.isEditor) {
         Main.GetComponent<AudioListener>().enabled = true;
      }
      StartCoroutine(Timer());
   }

   void ResetAll () {
      ResetZoneWallInfo();
      ResetMemoryInfo();
      ResetNodeInfo();
      StackReset();
   }

   IEnumerator IBeViewingTheseBitches () {
      if (BeingHacked) {
         StopAllCoroutines();
         Bar.GetComponent<MeshRenderer>().material = BarColors[2];
         StartCoroutine(Timer());
      }
      else {
         BeingHacked = true;
      }
      CurrentState = HackState.GettingHacked;
      //Makes it so it does not spam editor. This should not be ingame.
      if (Application.isEditor) {
         Main.GetComponent<AudioListener>().enabled = false;
      }
      //Switches camera
      MiniCams[0].gameObject.SetActive(true);

      //Determines where the good spots are
      for (int i = 0; i < 5; i++) {
         ZoneText[i].text = "";
         ZoneWallCorrectSpots[i] = UnityEngine.Random.Range(0, 28);
      }
      Audio.PlaySoundAtTransform("GettingGotAudio", Cube[0].transform);
      Background.Play();
      yield return new WaitForSeconds(11.9f);
      Background.Stop();
      MiniCams[0].gameObject.SetActive(false);

      //Switches to Zonewall cam
      MiniCams[1].gameObject.SetActive(true);
      StartCoroutine(ZoneWall());
   }

   #endregion

   #region Zonewalling

   IEnumerator ZoneWall () { //Generates the 5 lines of [......]
      StartCoroutine(ZoneWallTextGenerate(0));
      StartCoroutine(ZoneWallTextGenerate(1));
      yield return new WaitForSeconds(.2f);
      Audio.PlaySoundAtTransform("Countdown", Cube[1].transform);
      StartCoroutine(ZoneWallTextGenerate(2));
      yield return new WaitForSeconds(.2f);
      StartCoroutine(ZoneWallTextGenerate(3));
      yield return new WaitForSeconds(.2f);
      StartCoroutine(ZoneWallTextGenerate(4));
      yield return new WaitForSeconds(4.9f);
      StartCoroutine(ZoneWallCursorIndex());
   }

   IEnumerator ZoneWallTextGenerate (int index) {
      ZoneText[index].text += "[";
      for (int i = 0; i < 30; i++) {
         if (i == ZoneWallCorrectSpots[index] || i == ZoneWallCorrectSpots[index] + 1 || i == ZoneWallCorrectSpots[index] + 2) {
            Debug.Log(ZoneWallCorrectSpots[index]);
            ZoneText[index].text += "<color=red>.</color>";
         }
         else {
            ZoneText[index].text += "<color=green>.</color>";
         }
         yield return new WaitForSeconds(.01f);
      }
      ZoneText[index].text += "]";
      Debug.Log(ZoneText[index].text);
   }

   IEnumerator ZoneWallCursorIndex () {
      CurrentState = HackState.ZoneWall;
      for (int i = 0; i < 5; i++) {
         while (ZoneClicks != i) {
            for (int j = 1; j < ZoneText[i].text.Length - 1; j++) {
               if (ZoneText[i].text[j] == '.') {
                  if (ZoneClicks == i) {
                     break;
                  }
                  ZoneText[i].text = ZoneText[i].text.Substring(0, j) + "|" + ZoneText[i].text.Substring(j + 1);
                  for (int k = 0; k < j; k++) { //Changes previous | bar to be .
                     if (ZoneText[i].text[k] == '|') {
                        ZoneText[i].text = ZoneText[i].text.Substring(0, k) + "." + ZoneText[i].text.Substring(k + 1);
                        break;
                     }
                  }
                  if (false) {
                     /*
                   * 
                   * Was supposed to have small bar but I can't be arsed to fucking make it work
                   * 
                   * 
                  for (int k = j; k < ZoneText[i].text.Length; k++) {//Below one doesn't work
                     if (ZoneText[i].text[k] == '.' ) {
                        ZoneText[i].text = ZoneText[i].text.Substring(0, k) + "⃓" + ZoneText[i].text.Substring(k + 1);
                        break;
                     }
                  }

                  for (int k = 0; k < j; k++) { //Changes previous tall bar to be small
                     if (ZoneText[i].text[k] == '|') {
                        ZoneText[i].text = ZoneText[i].text.Substring(0, k) + "." + ZoneText[i].text.Substring(k + 1);
                        break;
                     }
                  }

                  for (int k = 0; k < j; k++) { //Changes last small bar to be .
                     if (ZoneText[i].text[k] == '⃓') {
                        ZoneText[i].text = ZoneText[i].text.Substring(0, k) + "." + ZoneText[i].text.Substring(k + 1);
                        break;
                     }
                  }
                  //Debug.Log("what");*/
                  }

                  //Debug.Log(ZoneClickIndex);
                  yield return new WaitForSecondsRealtime(.015f);
               }
            }
            if (ZoneClicks == i) {
               break;
            }
            Audio.PlaySoundAtTransform("RightBump", Cube[1].transform);
            for (int j = ZoneText[i].text.Length - 2; j > 0; j--) {
               if (ZoneClicks == i) {
                  break;
               }
               if (ZoneText[i].text[j] == '.') {
                  ZoneText[i].text = ZoneText[i].text.Substring(0, j) + "|" + ZoneText[i].text.Substring(j + 1);
                  int temp = ZoneText[i].text.LastIndexOf("|");
                  ZoneText[i].text = ZoneText[i].text.Remove(temp, 1).Insert(temp, ".");
                  //Debug.Log(ZoneClickIndex);
                  yield return new WaitForSecondsRealtime(.015f);
               }
            }
            Audio.PlaySoundAtTransform("LeftBump", Cube[1].transform);
         }
      }
      //Debug.Log(ZoneCorrectClicks);
      //StartCoroutine(MemoryFraggerDisplay());
      
      if (ZoneCorrectClicks == 5) {
         StartCoroutine(Instablock());
      }
      else {
         switch (Rnd.Range(0, 10)) {
            case 0:
            case 1:
            case 2:
               NodeMazeGeneration();
               break;
            case 3:
            case 4:
            case 5:
            case 6:
               StartCoroutine(MemoryFraggerDisplay());
               break;
            case 7:
            case 8:
            case 9:
               GenerateStackPusher();
               break;
         }
      }
   }

   void ResetZoneWallInfo () {
      for (int i = 0; i < 5; i++) {
         ZoneWallCorrectSpots[i] = 0;
      }
      ZoneClicks = -1;
      ZoneCorrectClicks = 0;
   }

   #endregion

   #region Memory Fragger

   IEnumerator MemoryFraggerDisplay () {
      for (int i = 0; i < 8; i++) {
         MiniNodeTexts[i].text = "";
      }
      string temp = "";
      string temp2 = "";
      for (int i = 0; i < MiniCams.Length; i++) {
         MiniCams[i].gameObject.SetActive(false);
      }
      MiniCams[2].gameObject.SetActive(true);
      for (int i = 0; i < 8; i++) {
         Nodes[i].gameObject.SetActive(false);
      }
      switch (ZoneCorrectClicks) {
         case 0:
            temp = Alphanumerical.ToCharArray().Shuffle().Join();
            for (int i = 0; i < temp.Length; i++) {
               if (temp[i] != ' ') {
                  temp2 += temp[i].ToString();
               }
            }
            temp = temp2;
            for (int i = 0; i < Rnd.Range(6, 8); i++) {
               Goal += temp[i].ToString();
            }
            break;
         case 1:
            temp = Alphanumerical.ToCharArray().Shuffle().Join();
            for (int i = 0; i < temp.Length; i++) {
               if (temp[i] != ' ') {
                  temp2 += temp[i].ToString();
               }
            }
            temp = temp2;
            for (int i = 0; i < Rnd.Range(5, 7); i++) {
               Goal += temp[i].ToString();
            }
            break;
         case 2:
            temp = Alphabetical.ToCharArray().Shuffle().Join();
            for (int i = 0; i < temp.Length; i++) {
               if (temp[i] != ' ') {
                  temp2 += temp[i].ToString();
               }
            }
            temp = temp2;
            for (int i = 0; i < Rnd.Range(4, 6); i++) {
               Goal += temp[i].ToString();
            }
            break;
         case 3:
            temp = Alphabetical.ToCharArray().Shuffle().Join();
            for (int i = 0; i < temp.Length; i++) {
               if (temp[i] != ' ') {
                  temp2 += temp[i].ToString();
               }
            }
            temp = temp2;
            for (int i = 0; i < Rnd.Range(3, 5); i++) {
               Goal += temp[i].ToString();
            }
            break;
         case 4:
            temp = Numerical.ToCharArray().Shuffle().Join();
            for (int i = 0; i < temp.Length; i++) {
               if (temp[i] != ' ') {
                  temp2 += temp[i].ToString();
               }
            }
            temp = temp2;
            for (int i = 0; i < Rnd.Range(2, 4); i++) {
               Goal += temp[i].ToString();
            }
            break;
      }
      for (int i = 0; i < Goal.Length; i++) {
         MainNodeText.text = Goal[i].ToString();
         Audio.PlaySoundAtTransform("MemoryDisplayBeep", Cube[2].transform);
         yield return new WaitForSeconds(1f);
      }
      MainNodeText.text = "";
      ShowOptions();
      CurrentState = HackState.MemoryFragger;
   }

   void ShowOptions () {
      Debug.Log(Goal);
      for (int i = 0; i < Goal.Length; i++) {
         Retry:
         switch (Rnd.Range(0, 4)) {
            case 0:
               if (!Taken[0]) {
                  MiniNodeTexts[0].text = Goal[i].ToString();
                  Taken[0] = true;
               }
               else if (!Taken[1]) {
                  MiniNodeTexts[1].text = Goal[i].ToString();
                  Taken[1] = true;
               }
               else {
                  goto Retry;
               }
               break;
            case 1:
               if (!Taken[2]) {
                  MiniNodeTexts[2].text = Goal[i].ToString();
                  Taken[2] = true;
               }
               else if (!Taken[3]) {
                  MiniNodeTexts[3].text = Goal[i].ToString();
                  Taken[3] = true;
               }
               else {
                  goto Retry;
               }
               break;
            case 2:
               if (!Taken[4]) {
                  MiniNodeTexts[4].text = Goal[i].ToString();
                  Taken[4] = true;
               }
               else if (!Taken[5]) {
                  MiniNodeTexts[5].text = Goal[i].ToString();
                  Taken[5] = true;
               }
               else {
                  goto Retry;
               }
               break;
            case 3:
               if (!Taken[6]) {
                  MiniNodeTexts[6].text = Goal[i].ToString();
                  Taken[6] = true;
               }
               else if (!Taken[7]) {
                  MiniNodeTexts[7].text = Goal[i].ToString();
                  Taken[7] = true;
               }
               else {
                  goto Retry;
               }
               break;
         }
      }
      for (int i = 0; i < 8; i++) {
         if (Taken[i]) {
            Nodes[i].gameObject.SetActive(true);
         }
      }
   }

   void ResetMemoryInfo () {
      Goal = "";
      MemoryInput = "";
      for (int i = 0; i < 8; i++) {
         Taken[i] = false;
      }
      for (int j = 0; j < 8; j++) {
         InnerNodes[j].GetComponent<MeshRenderer>().material = ColorsForMemory[0];
      }
   }

   #endregion

   #region Node Hacker

   void NodeMazeGeneration () {
      OorD[0] = Rnd.Range(0, 2) == 0;

      for (int i = 1; i < 25; i++) {
         OorD[i] = !OorD[i - 1];
         Spots[i] = i;
      }
      for (int i = 0; i < 4; i++) {
         OorD[Rnd.Range(0, 25)] = Rnd.Range(0, 2) == 1;
      }
      Spots.Shuffle();
      for (int i = 0; i < 25; i++) {
         Highlights[i].SetActive(false);
         Centers[i].SetActive(false);
         Diagonals[i].SetActive(false);
         Orthogonals[i].SetActive(false);
         AlsoCenterGodDamnIt[i].SetActive(false);
      }
      StartCoroutine(NodeMazeReveal());
   }

   IEnumerator NodeMazeReveal () {
      for (int i = 0; i < MiniCams.Length; i++) {
         MiniCams[i].gameObject.SetActive(false);
      }
      MiniCams[4].gameObject.SetActive(true);

      for (int i = 0; i < 25; i++) {
         if (OorD[i]) {
            Diagonals[i].SetActive(true);
         }
         else {
            Orthogonals[i].SetActive(true);
         }
      }
      for (int i = 0; i < 5; i++) {
         for (int j = 0; j < 5; j++) {
            Centers[i * 5 + j].SetActive(true);
            AlsoCenterGodDamnIt[i * 5 + j].SetActive(true);
         }
         Audio.PlaySoundAtTransform("NodeHackerSetup", Cube[4].transform);
         yield return new WaitForSecondsRealtime(.2f);
      }
      for (int i = 0; i < 5 - ZoneCorrectClicks; i++) {
         Highlights[Spots[i]].SetActive(true);
      }
      CurrentNode = Rnd.Range(0, 5);
      Path.Add(CurrentNode);
      Centers[CurrentNode].GetComponent<MeshRenderer>().material = NodeHackerColors[0];
      CurrentState = HackState.NodeHacker;
   }

   IEnumerator NodeCheck () {
      Centers[Path[0]].GetComponent<MeshRenderer>().material = NodeHackerColors[1];
      yield return new WaitForSeconds(.1f);
      for (int j = 1; j < Path.Count(); j++) {
         Centers[Path[j]].GetComponent<MeshRenderer>().material = NodeHackerColors[1];
         Audio.PlaySoundAtTransform("CheckNoise", Cube[4].transform);
         yield return new WaitForSeconds(.1f);
         if (OorD[Path[j - 1]] == OorD[Path[j]]) {
            /*Debug.Log(OorD[Path[j - 1]]);
            Debug.Log(OorD[Path[j]]);
            Debug.Log(Path[j - 1]);
            Debug.Log(Path[j]);*/
            StartCoroutine(HackResult());
            yield break;
         }
      }
      for (int j = 0; j < 5 - ZoneCorrectClicks; j++) {
         if (!Path.Contains(Spots[j])) {
            StartCoroutine(HackResult());
            yield break;
         }
      }
      BlockedHack = true;
      StartCoroutine(HackResult());
   }

   void ResetNodeInfo () {
      CurrentNode = 0;
      for (int i = 0; i < 25; i++) {
         Visited[i] = false;
         Centers[i].GetComponent<MeshRenderer>().material = NodeHackerColors[3];
         AlsoCenterGodDamnIt[i].GetComponent<MeshRenderer>().material = NodeHackerColors[2];
         Highlights[i].SetActive(false);
      }
      Path.Clear();
   }

   #endregion

   #region Stack Pusher

   void GenerateStackPusher () {// -138.28 1 -133.348

      for (int i = 0; i < MiniCams.Length; i++) {
         MiniCams[i].gameObject.SetActive(false);
      }
      MiniCams[5].gameObject.SetActive(true);
      for (int i = 0; i < 25; i++) {
         switch (i) {
            case 12:
               StackNodes[i].sprite = StackPictures[2];
               break;
            default:
               StackNodes[i].sprite = StackPictures[0];
               break;
         }
      }
      for (int i = 0; i < 12; i++) {
         OpenStackSpots[i] = i;
      }
      for (int i = 12; i < 24; i++) {
         OpenStackSpots[i] = i + 1;
      }
      OpenStackSpots.Shuffle();
      StackYou = OpenStackSpots[0];
      StackNodes[OpenStackSpots[0]].sprite = StackPictures[5];
      StackGrid[OpenStackSpots[0]] = StackNodeStates.YouH;
      ActualSelection = StackYou;
      for (int i = 1; i < (5 - ZoneCorrectClicks) + 1; i++) {
         StackGrid[OpenStackSpots[i]] = StackNodeStates.Stack;
         StackNodes[OpenStackSpots[i]].sprite = StackPictures[6];
      }
      StartCoroutine(StackPusher());
   }

   IEnumerator StackPusher () {  //Reused code but transparency is a fuck.
      StackPusherThing.transform.localPosition = new Vector3(0, 0, .5f);
      for (int i = 0; i < 5; i++) {
         StackPusherThing.transform.localPosition -= new Vector3(0, 0, 0.1f);
         yield return new WaitForSeconds(.01f);
      }
      CurrentState = HackState.StackPusher;
   }

   void StackReset () {
      HeldItem = TypesOfItems.Empty;
      NodesDunked = 0;
   }

   #endregion

   void Update () {
      if (ModuleSolved) {
         return;
      }
      if (CurrentState == HackState.Idle) {
         for (int i = 0; i < TheWholeAssModMinusTheMod.Length; i++) {
            TheWholeAssModMinusTheMod[i].gameObject.SetActive(false);
         }
      }
      else {
         for (int i = 0; i < TheWholeAssModMinusTheMod.Length; i++) {
            TheWholeAssModMinusTheMod[i].gameObject.SetActive(true);
         }
      }
      if (CurrentState == HackState.Idle || CurrentState == HackState.GettingHacked) {
         CurrentVBucks.text = DOSCoinAmount.ToString("000.000");
         return;
      }
      else if (CurrentState == HackState.ZoneWall) {
         if (Input.GetKeyDown(KeyCode.Space)) {
            if (ZoneText[ZoneClicks + 1].text.IndexOf("<color=red>|</color>") != -1) {
               ZoneCorrectClicks++;
            }
            ZoneClicks++;
         }
      }
      else if (CurrentState == HackState.MemoryFragger) {
         for (int i = 0; i < TheKeys.Count(); i++) {
            if (Input.GetKeyDown(TheKeys[i])) {
               if (MemoryInput.Length != Goal.Length) {
                  Audio.PlaySoundAtTransform("MemorySelectionBeep", Cube[2].transform);
               }
               else {
                  Audio.PlaySoundAtTransform("MemorySelectionBeepFinal", Cube[3].transform);
               }
               string temp = "";
               for (int k = 0; k < 8; k++) {
                  temp += MiniNodeTexts[k].text.ToString();
               }
               if (!temp.Contains(TheLetters[i].ToString().ToUpper())) {
                  return;
               }
               MemoryInput += TheLetters[i].ToString();
               for (int j = 0; j < 8; j++) {
                  if (MiniNodeTexts[j].text == MemoryInput[MemoryInput.Length - 1].ToString()) {
                     InnerNodes[j].GetComponent<MeshRenderer>().material = ColorsForMemory[1];
                  }
               }
               if (MemoryInput[MemoryInput.Length - 1] != Goal[MemoryInput.Length - 1]) {
                  StartCoroutine(HackResult());
               }
               if (Goal.Length == MemoryInput.Length) {
                  BlockedHack = true;
                  StartCoroutine(HackResult());
               }
            }
         }
      }
      else if (CurrentState == HackState.NodeHacker) {
         for (int i = 0; i < TheKeys.Count(); i++) {
            if (Input.GetKeyDown(TheKeys[i])) {
               switch (TheLetters[i].ToString()) {
                  case "Q":
                     if (CurrentNode > 4 && CurrentNode % 5 != 0) {
                        if (!Visited[CurrentNode - 6]) {
                           CurrentNode -= 6;
                           Path.Add(CurrentNode);
                           Centers[CurrentNode].GetComponent<MeshRenderer>().material = NodeHackerColors[0];
                        }
                     }
                     break;
                  case "A":
                     if (CurrentNode > 4) {
                        if (!Visited[CurrentNode - 5]) {
                           CurrentNode -= 5;
                           Path.Add(CurrentNode);
                           Centers[CurrentNode].GetComponent<MeshRenderer>().material = NodeHackerColors[0];
                        }
                     }
                     break;
                  case "Z":
                     if (CurrentNode > 4 && CurrentNode % 5 != 4) {
                        if (!Visited[CurrentNode - 4]) {
                           CurrentNode -= 4;
                           Path.Add(CurrentNode);
                           Centers[CurrentNode].GetComponent<MeshRenderer>().material = NodeHackerColors[0];
                        }
                     }
                     break;

                  case "W":
                     if (CurrentNode % 5 != 0) {
                        if (!Visited[CurrentNode - 1]) {
                           CurrentNode -= 1;
                           Path.Add(CurrentNode);
                           Centers[CurrentNode].GetComponent<MeshRenderer>().material = NodeHackerColors[0];
                        }
                     }
                     break;
                  case "S":
                     CurrentState = HackState.GettingHacked;
                     StartCoroutine(NodeCheck());
                     return;
                  case "X":
                     if (CurrentNode % 5 != 4) {
                        if (!Visited[CurrentNode + 1]) {
                           CurrentNode += 1;
                           Path.Add(CurrentNode);
                           Centers[CurrentNode].GetComponent<MeshRenderer>().material = NodeHackerColors[0];
                        }
                     }
                     break;

                  case "E":
                     if (CurrentNode < 20 && CurrentNode % 5 != 0) {
                        if (!Visited[CurrentNode + 4]) {
                           CurrentNode += 4;
                           Path.Add(CurrentNode);
                           Centers[CurrentNode].GetComponent<MeshRenderer>().material = NodeHackerColors[0];
                        }
                     }
                     break;
                  case "D":
                     if (CurrentNode < 20) {
                        if (!Visited[CurrentNode + 5]) {
                           CurrentNode += 5;
                           Path.Add(CurrentNode);
                           Centers[CurrentNode].GetComponent<MeshRenderer>().material = NodeHackerColors[0];
                        }
                     }
                     break;
                  case "C":
                     if (CurrentNode < 20 && CurrentNode % 5 != 4) {
                        if (!Visited[CurrentNode + 6]) {
                           CurrentNode += 6;
                           Path.Add(CurrentNode);
                           Centers[CurrentNode].GetComponent<MeshRenderer>().material = NodeHackerColors[0];
                        }
                     }
                     break;
               }
               bool WasHighlighted = false;
               for (int j = 0; j < ZoneCorrectClicks; j++) {
                  if (Spots[j] == Path[Path.Count() - 1]) {
                     WasHighlighted = true;
                  }
               }
               Audio.PlaySoundAtTransform(WasHighlighted ? "ExitedHighlight" : "Moved", Cube[4].transform);
            }
         }
      }
      else if (CurrentState == HackState.StackPusher) {
         if (Input.GetKeyDown(KeyCode.W) && ActualSelection > 4) {
            StackGrid[ActualSelection]--;
            StackNodes[ActualSelection].sprite = StackPictures[(int) StackGrid[ActualSelection]];
            ActualSelection -= 5;
            StackGrid[ActualSelection]++;
            StackNodes[ActualSelection].sprite = StackPictures[(int) StackGrid[ActualSelection]];
         }
         else if (Input.GetKeyDown(KeyCode.A) && ActualSelection % 5 != 0) {
            StackGrid[ActualSelection]--;
            StackNodes[ActualSelection].sprite = StackPictures[(int) StackGrid[ActualSelection]];
            ActualSelection--;
            StackGrid[ActualSelection]++;
            StackNodes[ActualSelection].sprite = StackPictures[(int) StackGrid[ActualSelection]];
         }
         else if (Input.GetKeyDown(KeyCode.S) && ActualSelection < 20) {
            StackGrid[ActualSelection]--;
            StackNodes[ActualSelection].sprite = StackPictures[(int) StackGrid[ActualSelection]];
            ActualSelection += 5;
            StackGrid[ActualSelection]++;
            StackNodes[ActualSelection].sprite = StackPictures[(int) StackGrid[ActualSelection]];
         }
         else if (Input.GetKeyDown(KeyCode.D) && ActualSelection % 5 != 4) {
            StackGrid[ActualSelection]--;
            StackNodes[ActualSelection].sprite = StackPictures[(int) StackGrid[ActualSelection]];
            ActualSelection++;
            StackGrid[ActualSelection]++;
            StackNodes[ActualSelection].sprite = StackPictures[(int) StackGrid[ActualSelection]];
         }
         else if (Input.GetKeyDown(KeyCode.Space)) {
            if (StackGrid[ActualSelection] == StackNodeStates.YouH) {
               if (HeldItem == TypesOfItems.Stack) {
                  StartCoroutine(HackResult());
               }
               else {
                  Audio.PlaySoundAtTransform("MovePickUp", Cube[5].transform);
                  HeldItem = HeldItem == TypesOfItems.Move ? TypesOfItems.Empty : TypesOfItems.Move;
                  if (HeldItem == TypesOfItems.Move) {
                     IndexOfHeldStack = ActualSelection;
                  }
               }
            }
            else if (StackGrid[ActualSelection] == StackNodeStates.StackH) {
               int YouPosition = 0;
               bool CanPickUp = false;
               for (int i = 0; i < 25; i++) {
                  if (StackGrid[i] == StackNodeStates.YouH || StackGrid[i] == StackNodeStates.You) {
                     YouPosition = i;
                  }
               }
               if (ActualSelection == 0) {
                  if (YouPosition == 1 || YouPosition == 5 || YouPosition == 6) {
                     CanPickUp = true;
                  }
               }
               else if (ActualSelection == 4) {
                  if (YouPosition == 3 || YouPosition == 8 || YouPosition == 9) {
                     CanPickUp = true;
                  }
               }
               else if (ActualSelection == 20) {
                  if (YouPosition == 15 || YouPosition == 16 || YouPosition == 21) {
                     CanPickUp = true;
                  }
               }
               else if (ActualSelection == 24) {
                  if (YouPosition == 18 || YouPosition == 19|| YouPosition == 23) {
                     CanPickUp = true;
                  }
               }
               else if (ActualSelection > 19) {
                  if (YouPosition == ActualSelection - 6 || YouPosition == ActualSelection - 5 || YouPosition == ActualSelection - 4 || YouPosition == ActualSelection - 1 || YouPosition == ActualSelection + 1) {
                     CanPickUp = true;
                  }
               }
               else if (ActualSelection < 5) {
                  if (YouPosition == ActualSelection - 1 || YouPosition == ActualSelection + 1 || YouPosition == ActualSelection + 4 || YouPosition == ActualSelection + 5 || YouPosition == ActualSelection + 6) {
                     CanPickUp = true;
                  }
               }
               else if (ActualSelection % 5 == 0) {
                  if (YouPosition == ActualSelection - 5 || YouPosition == ActualSelection + 5 || YouPosition == ActualSelection - 4 || YouPosition == ActualSelection + 1 || YouPosition == ActualSelection + 6) {
                     CanPickUp = true;
                  }
               }
               else if (ActualSelection % 5 == 4) {
                  if (YouPosition == ActualSelection - 5 || YouPosition == ActualSelection + 5 || YouPosition == ActualSelection + 4 || YouPosition == ActualSelection - 1 || YouPosition == ActualSelection - 6) {
                     CanPickUp = true;
                  }
               }
               else if (YouPosition == ActualSelection - 6 || YouPosition == ActualSelection - 5 || YouPosition == ActualSelection - 4 || YouPosition == ActualSelection - 1 || YouPosition == ActualSelection + 1 || YouPosition == ActualSelection + 4 || YouPosition == ActualSelection + 5 || YouPosition == ActualSelection + 6) {
                  CanPickUp = true;
               }

               if (HeldItem != TypesOfItems.Empty || !CanPickUp) {
                  StartCoroutine(HackResult());
               }
               else {
                  Audio.PlaySoundAtTransform("StackPickUp", Cube[5].transform);
                  HeldItem = TypesOfItems.Stack;
                  IndexOfHeldStack = ActualSelection;
               }
            }
            else if (StackGrid[ActualSelection] == StackNodeStates.EmptyH) {
               if (HeldItem == TypesOfItems.Stack) {
                  Audio.PlaySoundAtTransform("StackPlace", Cube[5].transform);
                  StackNodes[IndexOfHeldStack].sprite = StackPictures[0];
                  StackNodes[ActualSelection].sprite = StackPictures[7];
                  StackGrid[IndexOfHeldStack] = StackNodeStates.Empty;
                  StackGrid[ActualSelection] = StackNodeStates.StackH;
                  IndexOfHeldStack = -1;
                  HeldItem = TypesOfItems.Empty;
               }
               else if (HeldItem == TypesOfItems.Move) {
                  Audio.PlaySoundAtTransform("MovePlace", Cube[5].transform);
                  StackNodes[IndexOfHeldStack].sprite = StackPictures[0];
                  StackNodes[ActualSelection].sprite = StackPictures[5];
                  StackGrid[IndexOfHeldStack] = StackNodeStates.Empty;
                  StackGrid[ActualSelection] = StackNodeStates.YouH;
                  IndexOfHeldStack = -1;
                  HeldItem = TypesOfItems.Empty;
               }
            }
            else if (StackGrid[ActualSelection] == StackNodeStates.GoalH) {
               if (HeldItem != TypesOfItems.Stack) {
                  StartCoroutine(HackResult());
               }
               else {
                  Audio.PlaySoundAtTransform("Goal", Cube[5].transform);
                  HeldItem = TypesOfItems.Empty;
                  StackNodes[IndexOfHeldStack].sprite = StackPictures[0];
                  StackGrid[IndexOfHeldStack] = StackNodeStates.Empty;
                  IndexOfHeldStack = -1;
                  NodesDunked++;
                  //Debug.Log(NodesDunked);
                  //Debug.Log(ZoneCorrectClicks);
                  if (NodesDunked == 5 - ZoneCorrectClicks) {
                     BlockedHack = true;
                     StartCoroutine(HackResult());
                  }
               }
            }
         }
      }
   }

   #region Twitch Plays

   /* 
#pragma warning disable 414
   private readonly string TwitchHelpMessage = @"Use !{0} to do something.";
#pragma warning restore 414

   IEnumerator ProcessTwitchCommand (string Command) {
      
      yield return null;
   }

   IEnumerator TwitchHandleForcedSolve () {
      yield return null;
   }
   */
   #endregion
}
