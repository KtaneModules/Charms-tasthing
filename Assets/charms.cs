using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;
using System.Text.RegularExpressions;
using rnd = UnityEngine.Random;

public class charms : MonoBehaviour
{
    public new KMAudio audio;
    public KMBombInfo bomb;
    public KMBombModule module;

    public KMSelectable[] castingHexagons;
    public KMSelectable[] leftTiles;
    public KMSelectable[] rightTiles;
    public KMSelectable[] submitButtons;
    public KMSelectable[] ledButtons;
    private Renderer[] leftTileRenders;
    private Renderer[] rightTileRenders;
    public Renderer[] orbs;
    public Renderer[] stageLeds;
    public Light[] lights;
    public TextMesh[] colorblindTexts;
    public Transform[] arrows;
    public Color lightGray;
    public Color[] tileColors;
    public Color[] basicOrbColors;
    public Color[] castingOrbColors;

    private List<int> inputs = new List<int>();
    private int?[] leftConfiguration = new int?[6] { 0, null, 1, 2, 3, 4 };
    private int?[] rightConfiguration = new int?[6] { 0, null, 1, 2, 3, 4 };
    private int?[] leftSolution = new int?[6];
    private int?[] rightSolution = new int?[6];
    private int[] directions = new int[2];
    private bool[] puzzlesSolved = new bool[2];
    private int[] solutions = new int[3];
    private int stage;

    private static readonly string[] colorNames = new string[] { "red", "yellow", "green", "blue", "purple" };
    private int[][] adjacencyIndices = new int[6][];
    private static readonly float[] tileXs = new float[] { -0.2520195f, 0.2520195f };
    private static readonly float[] tileZs = new float[] { 0.4893236f, 0f, -0.4893236f };
    private static readonly string laCroixTorus = "PYPPPBGGGBPRRGGRRRYBRBRRGRPYYRRBYYGPPYBGRBBYYPBBBGYPPPBGGRRGYPGBBYGGRGYGGBRYPPRYPGRRRYYPPBPYYYPBBBGB";
    private static readonly string[] positionConversions = new string[] { "AGPU38", "DHMX29", "BIQS16", "EJNV07", "CKRWZ4", "FLOTY5" };
    private static readonly string[] spellColors = new string[] { "RR", "RY", "RG", "RB", "RP", "YR", "YY", "YG", "YB", "YP", "GR", "GY", "GG", "GB", "GP", "BR", "BY", "BG", "BB", "BP", "PR", "PY", "PG", "PB", "PP" };
    private static readonly string[] spellNames = new string[] { "Conformare Series", "Vestigium Ostendere", "Apage", "Aenigma Insulsus", "Fortis Facini", "Id Isea", "Arierae Factura", "Fulices Terreat", "Abrogo", "Pomi Ambrosia", "Urbs Cruminis", "Pincerna", "Mortuos Defendisse", "Incurrere Machinationes", "Elutorium", "Pluviarum Versicolorium", "Tessellam Adcessio", "Refocillatrix", "Sicut Pater", "Vendicare Compositum", "Speculi Expiatio", "Praestigiator Fabarum", "Silentium Coeli", "Daedalae Orbitae", "Vastator Metallis Tesquorum" };
    private static readonly List<int>[] spellPatterns = new List<int>[]
    {
        new List<int> { 5, 7, 6, 3, 1, 2 },
        new List<int> { 7, 4, 1, 4, 2, 4, 3, 4 },
        new List<int> { 3, 1, 2, 5, 7, 6 },
        new List<int> { 5, 4, 3, 6, 7 },
        new List<int> { 7, 4, 1, 4, 1 },
        new List<int> { 5, 4, 1, 3, 6, 4 },
        new List<int> { 1, 3, 6, 7, 5, 2, 1, 4 },
        new List<int> { 7, 5, 4, 6, 7 },
        new List<int> { 2, 1, 3, 4, 7 },
        new List<int> { 2, 4, 3, 6, 7, 5, 4, 1 },
        new List<int> { 1, 4, 5, 7, 6, 4 },
        new List<int> { 1, 4, 7, 6, 3 },
        new List<int> { 7, 4, 5, 4, 6, 4 },
        new List<int> { 2, 5, 4, 7, 4, 3, 6 },
        new List<int> { 2, 4, 3, 4, 7 },
        new List<int> { 4, 5, 7, 6, 4, 1 },
        new List<int> { 2, 5, 7, 6, 3, 1, 4 },
        new List<int> { 2, 4, 6, 4, 2, 4, 6 },
        new List<int> { 2, 1, 3, 4, 5, 7, 6 },
        new List<int> { 2, 5, 4, 6, 3},
        new List<int> { 1, 3, 4, 2, 4, 6 },
        new List<int> { 1, 4, 3, 1, 4, 3 },
        new List<int> { 5, 2, 4, 7, 4, 3, 6 },
        new List<int> { 5, 4, 3, 4, 5, 4, 3 },
        new List<int> { 7, 4, 1, 2, 1, 3 }
    };

    private bool ableToCast;
    private bool castingInProgress;
    private bool animating;
    private bool striking;
    private bool easterEggUsed;
    private string[] colorStringsUsed = new string[3];
    private Coroutine[] ledCycleAnimations = new Coroutine[3];

    private static int moduleIdCounter = 1;
    private int moduleId;
    private bool moduleSolved;

    #region ModSettings
    charmsSettings settings = new charmsSettings();
#pragma warning disable 414
    private static Dictionary<string, object>[] TweaksEditorSettings = new Dictionary<string, object>[]
    {
      new Dictionary<string, object>
      {
        { "Filename", "Charms Settings.json"},
        { "Name", "Charms" },
        { "Listings", new List<Dictionary<string, object>>
        {
          new Dictionary<string, object>
          {
            { "Key", "UnendingSolveAnimation" },
            { "Text", "If false, the 3 stage LEDs fade to black once the module is solved as opposed to cycling forever.."}
          }
        }}
      }
    };
#pragma warning restore 414

    private class charmsSettings
    {
        public bool unendingSolveAnimation = true;
    }
    #endregion

    private void Awake()
    {
        moduleId = moduleIdCounter++;
        var modConfig = new modConfig<charmsSettings>("Charms Settings");
        settings = modConfig.Read();
        modConfig.Write(settings);
        module.OnActivate += delegate () { audio.PlaySoundAtTransform("start", transform); };
        leftTileRenders = leftTiles.Select(x => x.GetComponent<Renderer>()).ToArray();
        rightTileRenders = rightTiles.Select(x => x.GetComponent<Renderer>()).ToArray();
        adjacencyIndices = "12;03;034;125;25;34".Split(';').Select(str => str.Select(ch => int.Parse(ch.ToString())).ToArray()).ToArray();
        if (!GetComponent<KMColorblindMode>().ColorblindModeActive)
            foreach (TextMesh colorblindText in colorblindTexts)
                colorblindText.text = "";
        foreach (Renderer hex in castingHexagons.Select(x => x.GetComponent<Renderer>()))
            hex.material.color = Color.black;

        foreach (KMSelectable hexagon in castingHexagons)
        {
            var ix = Array.IndexOf(castingHexagons, hexagon);
            hexagon.OnHighlight += delegate ()
            {
                if (moduleSolved || striking)
                    return;
                if (!castingInProgress)
                    ColorOrbs(ix, basicOrbColors[1]);
                else
                {
                    if (inputs.Last() == ix + 1)
                        return;
                    var count = inputs.Count(x => x == ix + 1);
                    ColorOrbs(ix, count >= 3 ? castingOrbColors[3] : castingOrbColors[count]);
                    audio.PlaySoundAtTransform("tone" + inputs.Count(), hexagon.transform);
                    inputs.Add(ix + 1);
                }
            };
            hexagon.OnHighlightEnded += delegate ()
            {
                if (moduleSolved || striking)
                    return;
                if (!castingInProgress)
                    ColorOrbs(ix, basicOrbColors[0]);
            };
            hexagon.OnInteract += delegate ()
            {
                if (!moduleSolved && ableToCast)
                {
                    castingInProgress = true;
                    ColorOrbs(ix, castingOrbColors[0]);
                    inputs.Add(ix + 1);
                    audio.PlaySoundAtTransform("tone0", hexagon.transform);
                }
                return false;
            };
            hexagon.OnInteractEnded += delegate ()
            {
                if (moduleSolved || !ableToCast)
                    return;
                castingInProgress = false;
                for (int i = 0; i < 7; i++)
                    ColorOrbs(i, basicOrbColors[0]);
                Debug.LogFormat("[Charms #{0}] Spell cast: {1}.", moduleId, inputs.Join("-"));
                if (inputs.SequenceEqual(spellPatterns[solutions[stage]]))
                {
                    Debug.LogFormat("[Charms #{0}] That was correct.", moduleId);
                    StartCoroutine(LiterallyJustAnInteractionPunch());
                    lights[stage].enabled = true;
                    ledCycleAnimations[stage] = StartCoroutine(CycleLed(stageLeds[stage], lights[stage], colorStringsUsed[stage]));
                    audio.PlaySoundAtTransform("spell-" + colorStringsUsed[stage][0], transform);
                    stage++;
                    if (stage == 3)
                    {
                        Debug.LogFormat("[Charms #{0}] Module solved!", moduleId);
                        module.HandlePass();
                        moduleSolved = true;
                        for (int i = 0; i < 7; i++)
                            ColorOrbs(i, castingOrbColors[3]);
                        if (!settings.unendingSolveAnimation)
                            StartCoroutine(SolveThenFade());
                    }
                }
                else
                {
                    Debug.LogFormat("[Charms #{0}] That was incorrect. Strike!", moduleId);
                    module.HandleStrike();
                    StartCoroutine(StrikeAnimation());
                }
                inputs.Clear();
            };
        }
        foreach (KMSelectable tile in leftTiles)
            tile.OnInteract += delegate () { PressTile(tile); return false; };
        foreach (KMSelectable tile in rightTiles)
            tile.OnInteract += delegate () { PressTile(tile); return false; };
        foreach (KMSelectable button in submitButtons)
            button.OnInteract += delegate () { PressSubmitButton(button); return false; };
        foreach (KMSelectable button in ledButtons)
            button.OnInteract += delegate () { PressLed(button); return false; };
    }

    private void Start()
    {
        var scalar = transform.lossyScale.x;
        foreach (Light light in lights)
        {
            light.range *= scalar;
            light.enabled = false;
        }
        for (int i = 0; i < 2; i++)
        {
            directions[i] = rnd.Range(0, 4);
            arrows[i].localEulerAngles = new Vector3(90f, 90f * directions[i], 0f);
        }
        var directionNames = new string[] { "up", "right", "down", "left" };
        Debug.LogFormat("[Charms #{0}] The left arrow points {1}, and the right arrow points {2}.", moduleId, directionNames[directions[0]], directionNames[directions[1]]);

        var sn = bomb.GetSerialNumber();
        var leftEmptyPosition = Array.IndexOf(positionConversions, positionConversions.First(x => x.Contains(sn[0])));
        var rightEmptyPosition = Array.IndexOf(positionConversions, positionConversions.First(x => x.Contains(sn[1])));
        var currentRow = bomb.GetModuleNames().Count() % 8;
        var currentColumn = (bomb.GetBatteryCount() + bomb.GetIndicators().Count() + bomb.GetPortCount()) % 8;
        Debug.LogFormat("[Charms #{0}] Starting position in the grid: row {1}, column {2}.", moduleId, currentRow, currentColumn);
        currentRow++;
        currentColumn++;
        var leftString = "";
        var rightString = "";
        while (!"RYGBP".All(x => leftString.Contains(x)))
        {
            var letter = laCroixTorus[(currentRow * 10) + currentColumn];
            if (!leftString.Contains(letter))
                leftString += letter;
            switch (directionNames[directions[0]])
            {
                case "up":
                    currentRow--;
                    if (currentRow == -1)
                        currentRow = 9;
                    break;
                case "right":
                    currentColumn++;
                    if (currentColumn == 10)
                        currentColumn = 0;
                    break;
                case "down":
                    currentRow++;
                    if (currentRow == 10)
                        currentRow = 0;
                    break;
                case "left":
                    currentColumn--;
                    if (currentColumn == -1)
                        currentColumn = 9;
                    break;
            }
        }
        currentRow = bomb.GetModuleNames().Count() % 8;
        currentColumn = (bomb.GetBatteryCount() + bomb.GetIndicators().Count() + bomb.GetPortCount()) % 8;
        currentRow++;
        currentColumn++;
        while (!"RYGBP".All(x => rightString.Contains(x)))
        {
            var letter = laCroixTorus[(currentRow * 10) + currentColumn];
            if (!rightString.Contains(letter))
                rightString += letter;
            switch (directionNames[directions[1]])
            {
                case "up":
                    currentRow--;
                    if (currentRow == -1)
                        currentRow = 9;
                    break;
                case "right":
                    currentColumn++;
                    if (currentColumn == 10)
                        currentColumn = 0;
                    break;
                case "down":
                    currentRow++;
                    if (currentRow == 10)
                        currentRow = 0;
                    break;
                case "left":
                    currentColumn--;
                    if (currentColumn == -1)
                        currentColumn = 9;
                    break;
            }
        }
        for (int i = 0; i < 2; i++)
        {
            var ix = 0;
            var arrayToModify = i == 0 ? leftSolution : rightSolution;
            for (int j = 0; j < 6; j++)
            {
                if (j == (i == 0 ? leftEmptyPosition : rightEmptyPosition))
                    arrayToModify[j] = null;
                else
                {
                    arrayToModify[j] = "RYGBP".IndexOf((i == 0 ? leftString : rightString)[ix]);
                    ix++;
                }
            }
        }
        Debug.LogFormat("[Charms #{0}] Left puzzle solution: {1}.", moduleId, leftSolution.Select(x => x == null ? "empty" : colorNames[(int)x]).Join(", "));
        Debug.LogFormat("[Charms #{0}] Right puzzle solution: {1}.", moduleId, rightSolution.Select(x => x == null ? "empty" : colorNames[(int)x]).Join(", "));

        for (int i = 0; i < 3; i++)
        {
            var colorString = "";
            switch (i)
            {
                case 0:
                    colorString = new string(leftSolution.Where(x => Array.IndexOf(leftSolution, x) % 2 == Array.IndexOf(leftSolution, null) % 2 && x != null).OrderBy(x => Array.IndexOf(leftSolution, x)).Select(x => "RYGBP"[(int)x]).ToArray());
                    break;
                case 1:
                    colorString = new string(rightSolution.Where(x => Array.IndexOf(rightSolution, x) % 2 == Array.IndexOf(rightSolution, null) % 2 && x != null).OrderByDescending(x => Array.IndexOf(rightSolution, x)).Select(x => "RYGBP"[(int)x]).ToArray());
                    break;
                case 2:
                    colorString = "RYGBP"[(int)leftSolution.First(x => Array.IndexOf(leftSolution, x) / 2 == Array.IndexOf(leftSolution, null) / 2 && x != null)].ToString() + "RYGBP"[(int)rightSolution.First(x => Array.IndexOf(rightSolution, x) / 2 == Array.IndexOf(rightSolution, null) / 2 && x != null)];
                    break;
            }
            colorStringsUsed[i] = colorString;
            solutions[i] = Array.IndexOf(spellColors, colorString);
            Debug.LogFormat("[Charms #{0}] Stage {1}: The colors are {2}, so the spell to cast is {3} ({4}).", moduleId, i + 1, colorString, spellNames[solutions[i]], spellPatterns[solutions[i]].Join("-"));
        }

        leftConfiguration = leftSolution.ToArray();
        rightConfiguration = rightSolution.ToArray();
        for (int i = 0; i < 5; i++)
        {
            leftTileRenders[i].material.color = tileColors[i];
            rightTileRenders[i].material.color = tileColors[i];
        }
        for (int i = 0; i < 100; i++)
        {
            HandleTileMovement(adjacencyIndices[Array.IndexOf(leftConfiguration, null)].PickRandom(), leftSet: true, animate: false);
            HandleTileMovement(adjacencyIndices[Array.IndexOf(rightConfiguration, null)].PickRandom(), leftSet: false, animate: false);
        }
        for (int i = 0; i < 6; i++)
        {
            if (leftConfiguration[i] == null)
                continue;
            var color = leftConfiguration[i];
            leftTiles[(int)color].transform.localPosition = new Vector3(tileXs[i % 2], .36f, tileZs[i / 2]);
        }
        for (int i = 0; i < 6; i++)
        {
            if (rightConfiguration[i] == null)
                continue;
            var color = rightConfiguration[i];
            rightTiles[(int)color].transform.localPosition = new Vector3(tileXs[i % 2], .36f, tileZs[i / 2]);
        }
        Debug.LogFormat("[Charms #{0}] Starting left sliding puzzle configuration: {1}.", moduleId, leftConfiguration.Select(x => x == null ? "empty" : colorNames[(int)x]).Join(", "));
        Debug.LogFormat("[Charms #{0}] Starting right sliding puzzle configuration: {1}.", moduleId, rightConfiguration.Select(x => x == null ? "empty" : colorNames[(int)x]).Join(", "));
        var leftArrowPos = Enumerable.Range(0, 6).Where(x => leftConfiguration[x] != null).PickRandom();
        var rightArrowPos = Enumerable.Range(0, 6).Where(x => rightConfiguration[x] != null).PickRandom();
        arrows[0].localPosition = new Vector3(tileXs[leftArrowPos % 2], 0.3539161f, tileZs[leftArrowPos / 2]);
        arrows[1].localPosition = new Vector3(tileXs[rightArrowPos % 2], 0.3539161f, tileZs[rightArrowPos / 2]);
    }

    private void PressTile(KMSelectable tile)
    {
        tile.AddInteractionPunch(.1f);
        var isLeft = leftTiles.Contains(tile);
        var colorIx = Array.IndexOf(isLeft ? leftTiles : rightTiles, tile); // Index of tile's color in the RYGBP order
        var position = Array.IndexOf(isLeft ? leftConfiguration : rightConfiguration, colorIx); // Actual position on the sliding puzzle
        var adjacentToEmpty = adjacencyIndices[position].Any(x => (isLeft ? leftConfiguration : rightConfiguration)[x] == null);
        if (moduleSolved || animating || puzzlesSolved[isLeft ? 0 : 1] || !adjacentToEmpty)
            return;
        audio.PlaySoundAtTransform("click" + rnd.Range(1, 5), tile.transform);
        HandleTileMovement(position, isLeft);
    }

    private void HandleTileMovement(int newPosOfEmpty, bool leftSet, bool animate = true)
    {
        var setToModify = leftSet ? leftConfiguration : rightConfiguration;
        var oldPosOfEmpty = Array.IndexOf(setToModify, null);
        setToModify[oldPosOfEmpty] = setToModify[newPosOfEmpty];
        setToModify[newPosOfEmpty] = null;
        if (animate)
        {
            var tileArray = leftSet ? leftTiles.Select(x => x.transform).ToArray() : rightTiles.Select(x => x.transform).ToArray();
            var tileToMove = tileArray[(int)setToModify[oldPosOfEmpty]];
            var horizontal = newPosOfEmpty / 2 == oldPosOfEmpty / 2;
            var startPos = horizontal ? tileToMove.localPosition.x : tileToMove.localPosition.z;
            var endPos = horizontal ? tileXs[oldPosOfEmpty % 2] : tileZs[oldPosOfEmpty / 2];
            StartCoroutine(MoveTile(tileToMove, horizontal, startPos, endPos));
        }
    }

    private IEnumerator MoveTile(Transform tile, bool horizontal, float startPos, float endPos)
    {
        animating = true;
        var elapsed = 0f;
        var duration = .1f;
        var initial = tile.localPosition;
        while (elapsed < duration)
        {
            if (horizontal)
                tile.localPosition = new Vector3(Easing.OutSine(elapsed, startPos, endPos, duration), .36f, initial.z);
            else
                tile.localPosition = new Vector3(initial.x, .36f, Easing.OutSine(elapsed, startPos, endPos, duration));
            yield return null;
            elapsed += Time.deltaTime;
        }
        if (horizontal)
            tile.localPosition = new Vector3(endPos, .36f, initial.z);
        else
            tile.localPosition = new Vector3(initial.x, .36f, endPos);
        animating = false;
    }

    private void PressSubmitButton(KMSelectable button)
    {
        button.AddInteractionPunch(.25f);
        audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, button.transform);
        if (moduleSolved || ableToCast)
            return;
        var left = Array.IndexOf(submitButtons, button) == 0;
        Debug.LogFormat("[Charms #{0}] Submitted {1} sliding puzzle: {2}.", moduleId, left ? "left" : "right", (left ? leftConfiguration : rightConfiguration).Select(x => x == null ? "empty" : colorNames[(int)x]).Join(", "));
        if ((left ? leftConfiguration.SequenceEqual(leftSolution) : rightConfiguration.SequenceEqual(rightSolution)) || Application.isEditor)
        {
            puzzlesSolved[left ? 0 : 1] = true;
            StartCoroutine(FadeSubmitButton(submitButtons[left ? 0 : 1].GetComponent<Renderer>()));
            Debug.LogFormat("[Charms #{0}] That was correct. {1} puzzle solved!", moduleId, left ? "left" : "right");
            if (puzzlesSolved[0] && puzzlesSolved[1])
            {
                ableToCast = true;
                Debug.LogFormat("[Charms #{0}] Spell casting unlocked!", moduleId);
                audio.PlaySoundAtTransform("unlock", transform);
                StartCoroutine(FadeHexagons());
            }
            else
                audio.PlaySoundAtTransform(left ? "puzzle1" : "puzzle2", button.transform);
        }
        else
        {
            module.HandleStrike();
            Debug.LogFormat("[Charms #{0}] That was incorrect. Strike!", moduleId);
        }
    }

    private IEnumerator CycleLed(Renderer led, Light light, string colorString)
    {
    restartCycle:
        for (int i = 0; i < 2; i++)
        {
            var startingColor = led.material.color;
            var endingColor = tileColors["RYGBP".IndexOf(colorString[i])];
            var elapsed = 0f;
            var duration = 2.5f;
            while (elapsed < duration)
            {
                led.material.color = Color.Lerp(startingColor, endingColor, elapsed / duration);
                light.color = Color.Lerp(startingColor, endingColor, elapsed / duration);
                yield return null;
                elapsed += Time.deltaTime;
            }
            led.material.color = endingColor;
            light.color = endingColor;
        }
        goto restartCycle;
    }

    private IEnumerator StrikeAnimation()
    {
        ableToCast = false;
        striking = true;
        for (int i = 0; i < 7; i++)
            ColorOrbs(i, Color.red);
        yield return new WaitForSeconds(1f);
        for (int i = 0; i < 7; i++)
            ColorOrbs(i, basicOrbColors[0]);
        ableToCast = true;
        striking = false;
    }

    private IEnumerator LiterallyJustAnInteractionPunch()
    {
        yield return null;
        castingHexagons[3].AddInteractionPunch(5f);
    }

    private IEnumerator FadeSubmitButton(Renderer button)
    {
        var elapsed = 0f;
        var duration = .75f;
        var startColor = button.material.color;
        while (elapsed < duration)
        {
            button.material.color = Color.Lerp(startColor, castingOrbColors[0], elapsed / duration);
            yield return null;
            elapsed += Time.deltaTime;
        }
        button.material.color = castingOrbColors[0];
    }

    private IEnumerator FadeHexagons()
    {
        var elapsed = 0f;
        var duration = .75f;
        var hexRenders = castingHexagons.Select(x => x.GetComponent<Renderer>()).ToArray();
        var startColor = hexRenders[0].material.color;
        while (elapsed < duration)
        {
            for (int i = 0; i < 7; i++)
                hexRenders[i].material.color = Color.Lerp(startColor, lightGray, elapsed / duration);
            yield return null;
            elapsed += Time.deltaTime;
        }
        for (int i = 0; i < 7; i++)
            hexRenders[i].material.color = lightGray;
    }

    private IEnumerator SolveThenFade()
    {
        yield return new WaitForSeconds(5f);
        for (int i = 0; i < 3; i++)
        {
            StopCoroutine(ledCycleAnimations[i]);
            ledCycleAnimations[i] = null;
            lights[i].enabled = false;
            StartCoroutine(LedOff(i));
        }
    }

    private IEnumerator LedOff(int i)
    {
        var start = stageLeds[i].material.color;
        var elapsed = 0f;
        var duration = 1f;
        while (elapsed < duration)
        {
            stageLeds[i].material.color = Color.Lerp(start, Color.black, elapsed / duration);
            yield return null;
            elapsed += Time.deltaTime;
        }
        stageLeds[i].material.color = Color.black;
    }

    private void PressLed(KMSelectable led)
    {
        if (!moduleSolved || easterEggUsed)
            return;
        audio.PlaySoundAtTransform("egg " + Array.IndexOf(spellColors, colorStringsUsed[Array.IndexOf(ledButtons, led)]), led.transform);
        easterEggUsed = true;
    }

    private void ColorOrbs(int ix, Color color)
    {
        foreach (Renderer orb in GetOrbs(ix))
            orb.material.color = color;
    }

    private Renderer[] GetOrbs(int ix)
    {
        return orbs.Skip(4 * ix).Take(4).ToArray();
    }


    // Twitch Plays
#pragma warning disable 414
    private readonly string TwitchHelpMessage = "!{0} <left/right> 1 2 3 [Presses those tiles in reading order on the right or left sliding puzzle, any amount can be used, l and r can be used as shorthands] !{0} <left/right> submit [Presses the left or right diamond button] !{0} cast 1 2 3 4 [Casts a spell by starting on hexagon 1 and moving to the rest in order]";
#pragma warning restore 414

    private IEnumerator ProcessTwitchCommand(string input)
    {
        input = input.Trim().ToLowerInvariant();
        if (input == "left submit" || input == "l submit")
        {
            if (puzzlesSolved[0])
            {
                yield return "sendtochaterror The left puzzle is already solved.";
                yield break;
            }
            yield return null;
            submitButtons[0].OnInteract();
        }
        else if (input == "right submit" || input == "r submit")
        {
            if (puzzlesSolved[1])
            {
                yield return "sendtochaterror The right puzzle is already solved.";
                yield break;
            }
            yield return null;
            submitButtons[1].OnInteract();
        }
        else if (input.StartsWith("left ") || input.StartsWith("l "))
        {
            if (puzzlesSolved[0])
            {
                yield return "sendtochaterror The left puzzle is already solved.";
                yield break;
            }
            yield return null;
            var numbers = input.Substring(input.StartsWith("left ") ? 5 : 2);
            for (int i = 0; i < numbers.Length; i++)
            {
                var nums = "123456 ".ToCharArray();
                var ix = Array.IndexOf(nums, numbers[i]);
                if (ix == 6)
                    continue;
                if (ix == -1)
                {
                    yield return "sendtochaterror \"" + numbers[i] + "\" is not a valid tile.";
                    yield break;
                }
                var thisTile = leftConfiguration[ix];
                if (thisTile == null)
                {
                    yield return "sendtochaterror You cannot press the blank space.";
                    yield break;
                }
                else
                {
                    yield return new WaitForSeconds(.1f);
                    leftTiles[(int)leftConfiguration[ix]].OnInteract();
                }
            }
        }
        else if (input.StartsWith("right ") || input.StartsWith("r "))
        {
            if (puzzlesSolved[1])
            {
                yield return "sendtochaterror The right puzzle is already solved.";
                yield break;
            }
            yield return null;
            var numbers = input.Substring(input.StartsWith("right ") ? 6 : 2);
            for (int i = 0; i < numbers.Length; i++)
            {
                var nums = "123456 ".ToCharArray();
                var ix = Array.IndexOf(nums, numbers[i]);
                if (ix == 6)
                    continue;
                if (ix == -1)
                {
                    yield return "sendtochaterror \"" + numbers[i] + "\" is not a valid tile.";
                    yield break;
                }
                var thisTile = rightConfiguration[ix];
                if (thisTile == null)
                {
                    yield return "sendtochaterror You cannot press the blank space.";
                    yield break;
                }
                else
                {
                    yield return new WaitForSeconds(.1f);
                    rightTiles[(int)rightConfiguration[ix]].OnInteract();
                }
            }
        }
        else if (input.StartsWith("cast "))
        {
            input = input.Substring(5);
            if (!ableToCast || castingInProgress)
            {
                yield return "sendtochaterror Hold your horses.";
                yield break;
            }
            var list = new List<int>();
            var nums = "1234567 ".ToCharArray();
            for (int i = 0; i < input.Length; i++)
            {
                var ix = Array.IndexOf(nums, input[i]);
                if (ix == 7)
                    continue;
                if (ix == -1)
                {
                    yield return "sendtochaterror \"" + input[i] + "\" is not a valid tile.";
                    yield break;
                }
                list.Add(ix);
            }
            yield return null;
            for (int i = 0; i < list.Count; i++)
            {
                yield return new WaitForSeconds(.1f);
                if (i == 0)
                    castingHexagons[list[i]].OnInteract();
                else
                    castingHexagons[list[i]].OnHighlight();
            }
            yield return new WaitForSeconds(.1f);
            castingHexagons[list[0]].OnInteractEnded();
        }
    }
}
