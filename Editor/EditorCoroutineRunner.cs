using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

public class EditorCoroutineRunner
{
    [MenuItem("Window/Lotte's Coroutine Runner: Demo")]
    public static void DemoEditorCoroutines()
    {
        // adds a menu item to test the coroutine system. 
        if (!Application.isPlaying)
        {
            // lets fire off the demo coroutine with a UI so we can see what its doing. We could also run it without a UI by using EditorCoroutineRunner.StartCoroutine(...)
            EditorCoroutineRunner.StartCoroutineWithUI(DemoCoroutiune(), "Lotte's Coroutine Demo", true); 
        }
    }

    static IEnumerator DemoCoroutiune()
    {
        // You can code editor coroutines exactly like you would a normal unity coroutine
        Debug.Log("Step: 0");
        yield return null;

        // all the normal return types that work with regular Unity coroutines should work here! for example lets wait for a second
        Debug.Log("Step: 1");
        yield return new WaitForSeconds(1);

        // We can also yeild any type that extends Unitys CustomYieldInstruction class. here we are going to use EditorStatusUpdate. this allows us to yield and update the
        // editor coroutine UI at the same time!
        yield return new EditorStatusUpdate("coroutine is running", 0.2f);

        // We can also yield to nested coroutines
        Debug.Log("Step: 2");

        yield return EditorCoroutineRunner.StartCoroutine(DemoTwo());
        EditorCoroutineRunner.UpdateUIProgressBar(0.35f); // we can use the UpdateUI helper methods to update the UI whenever, without yielding a EditorStatusUpdate
        yield return DemoTwo(); // it shouldnt matter how we start the nested coroutine, the editor runner can hadle it

        // we can even yield a WWW object if we want to grab data from the internets!
        Debug.Log("Step: 3");

        // for example, lets as random.org to generate us a list of random numbers and shove it into the console
        var www = new WWW("https://www.random.org/integers/?num=100&min=1&max=1000&col=1&base=10&col=5&format=plain&rnd=new");
        yield return www;
        Debug.Log(www.text);
        
        EditorCoroutineRunner.UpdateUI("Half way!", 0.5f);
        yield return new WaitForSeconds(1); 

        // Finally lets do a long runnig task and split its updates over many frames to keep the editor responsive
        Debug.Log("Step: 4");
        var test = 1000;
        yield return new WaitUntil(() => {
            test--;
            EditorCoroutineRunner.UpdateUI("Crunching Numbers: " + test, 0.5f + (((1000 - test) / 1000f) * 0.5f));
            return (test <= 0);
        });
        Debug.Log("Done!!");
    }

    static IEnumerator DemoTwo()
    {
        Debug.Log("TESTTWO: Starting second test coroutine");
        yield return new WaitForSeconds(1.2f);
        Debug.Log("TESTTWO: finished second test coroutine");
    }

    [MenuItem("Window/Lotte's Coroutine Runner: Force kill coroutines")]
    public static void KillAllCoroutines()
    {
        // force kills all running coroutines if something goes wrong.
        EditorUtility.ClearProgressBar();
        uiCoroutineState = null;
        coroutineStates.Clear();
        finishedThisUpdate.Clear();
    }

    private static List<EditorCoroutineState> coroutineStates;
    private static List<EditorCoroutineState> finishedThisUpdate;
    private static EditorCoroutineState uiCoroutineState;

    /// <summary>
    /// Start a coroutine. equivilent of calling StartCoroutine on a mono behaviour
    /// </summary>
    public static EditorCoroutine StartCoroutine(IEnumerator coroutine)
    {
        return StoreCoroutine(new EditorCoroutineState(coroutine));
    }

    /// <summary>
    /// Start a coroutine and display a progress UI. only one EditorCoroutine can display a UI at once. equivilent of calling StartCoroutine on a mono behaviour
    /// </summary>
    /// <param name="coroutine">coroutine to run</param>
    /// <param name="title">Text to show in the UIs title bar</param>
    /// <param name="isCancelable">Displays a cancel button if true</param>
    public static EditorCoroutine StartCoroutineWithUI(IEnumerator coroutine, string title, bool isCancelable = false)
    {
        if (uiCoroutineState != null)
        {
            Debug.LogError("EditorCoroutineRunner only supports running one coroutine that draws a GUI! [" + title + "]");
            return null;
        }
        EditorCoroutineRunner.uiCoroutineState = new EditorCoroutineState(coroutine, title, isCancelable);
        return StoreCoroutine(uiCoroutineState);
    }

    // Creates objects to manage the coroutines lifecycle and stores them away to be processed
    private static EditorCoroutine StoreCoroutine(EditorCoroutineState state)
    {
        if (coroutineStates == null)
        {
            coroutineStates = new List<EditorCoroutineState>();
            finishedThisUpdate = new List<EditorCoroutineState>();
        }

        if (coroutineStates.Count == 0)
            EditorApplication.update += Runner;

        coroutineStates.Add(state);

        return state.editorCoroutineYieldInstruction;
    }

    /// <summary>
    /// Updates the status label in the EditorCoroutine runner UI
    /// </summary>
    public static void UpdateUILabel(string label)
    {
        if (uiCoroutineState != null && uiCoroutineState.showUI)
        {
            uiCoroutineState.Label = label; 
        }
    }

    /// <summary>
    /// Updates the progress bar in the EditorCoroutine runner UI
    /// </summary>
    public static void UpdateUIProgressBar(float percent)
    {
        if (uiCoroutineState != null && uiCoroutineState.showUI)
        {
            uiCoroutineState.PercentComplete = percent;
        }
    }

    /// <summary>
    /// Updates the status label and progress bar in the EditorCoroutine runner UI
    /// </summary>
    public static void UpdateUI(string label, float percent)
    {
        if (uiCoroutineState != null && uiCoroutineState.showUI)
        {
            uiCoroutineState.Label = label ;
            uiCoroutineState.PercentComplete = percent;
        }
    }

    // Manages running active coroutines!
    private static void Runner()
    {
        // Tick all the coroutines we have stored
        for (int i = 0; i < coroutineStates.Count; i++)
        {
            TickState(coroutineStates[i]);
        }

        // if a coroutine was finished whilst we were ticking, clear it out now
        for (int i = 0; i < finishedThisUpdate.Count; i++)
        {
            coroutineStates.Remove(finishedThisUpdate[i]);

            if (uiCoroutineState == finishedThisUpdate[i])
            {
                uiCoroutineState = null;
                EditorUtility.ClearProgressBar();
            }
        }
        finishedThisUpdate.Clear();

        // stop the runner if were done.
        if (coroutineStates.Count == 0)
        {
            EditorApplication.update -= Runner;
        }
    }

    private static void TickState(EditorCoroutineState state)
    {
        if (state.IsValid)
        {
            // This coroutine is still valid, give it a chance to tick!
            state.Tick();

            // if this coroutine is the active UI coroutine, give it a chance to update the UI
            if (state.showUI && uiCoroutineState == state)
            {
                uiCoroutineState.UpdateUI();
            }
        }
        else
        {
            // We have finished running the coroutine, lets scrap it
            finishedThisUpdate.Add(state);
        }
    }

    
}

internal class EditorCoroutineState
{
    private IEnumerator coroutine;
    public bool IsValid
    {
        get { return coroutine != null; }
    }
    public EditorCoroutine editorCoroutineYieldInstruction;

    // current state
    private object current;
    private Type currentType;
    private float timer; // for WaitForSeconds support    
    private EditorCoroutine nestedCoroutine; // for tracking nested coroutines that are not started with EditorCoroutineRunner.StartCoroutine
    private DateTime lastUpdateTime;

    // UI
    public bool showUI;
    private bool cancelable;
    private bool canceled;
    private string title;
    public string Label;
    public float PercentComplete;

    public EditorCoroutineState(IEnumerator coroutine)
    {
        this.coroutine = coroutine;
        editorCoroutineYieldInstruction = new EditorCoroutine();
        showUI = false;
        lastUpdateTime = DateTime.Now;
    }

    public EditorCoroutineState(IEnumerator coroutine, string title, bool isCancelable)
    {
        this.coroutine = coroutine;
        editorCoroutineYieldInstruction = new EditorCoroutine();
        showUI = true;
        cancelable = isCancelable;
        this.title = title;
        Label = "initializing....";
        PercentComplete = 0.0f;

        lastUpdateTime = DateTime.Now;
    }

    public void Tick()
    {
        if (coroutine != null)
        {
            // First check if we have been canceled by the UI. If so, we need to stop before doing any wait processing
            if (canceled)
            {
                Stop();
                return;
            }

            // Did the last Yield want us to wait?
            bool isWaiting = false;
            var now = DateTime.Now;
            if (current != null) 
            {
                if (currentType == typeof(WaitForSeconds))
                {
                    // last yield was a WaitForSeconds. Lets update the timer.
                    var delta = now - lastUpdateTime;
                    timer -= (float)delta.TotalSeconds;

                    if (timer > 0.0f)
                    {
                        isWaiting = true;
                    }
                }
                else if (currentType == typeof(WaitForEndOfFrame) || currentType == typeof(WaitForFixedUpdate))
                {
                    // These dont make sense in editor, so we will treat them the same as a null return...
                    isWaiting = false;
                }
                else if (currentType == typeof(WWW))
                {
                    // Web download request, lets see if its done!
                    var www = current as WWW;
                    if (!www.isDone)
                    {
                        isWaiting = true;
                    }
                }
                else if (currentType.IsSubclassOf(typeof(CustomYieldInstruction)))
                {
                    // last yield was a custom yield type, lets check its keepWaiting property and react to that
                    var yieldInstruction = current as CustomYieldInstruction;
                    if (yieldInstruction.keepWaiting)
                    {
                        isWaiting = true;
                    }
                }
                else if (currentType == typeof(EditorCoroutine))
                {
                    // Were waiting on another coroutine to finish
                    var editorCoroutine = current as EditorCoroutine;
                    if (!editorCoroutine.HasFinished)
                    {
                        isWaiting = true;
                    }
                }
                else if (typeof(IEnumerator).IsAssignableFrom(currentType))
                {
                    // if were just seeing an enumerator lets assume that were seeing a nested coroutine that has been passed in without calling start.. were start it properly here if we need to
                    if (nestedCoroutine == null)
                    {
                        nestedCoroutine = EditorCoroutineRunner.StartCoroutine(current as IEnumerator);
                        isWaiting = true;
                    }
                    else
                    {
                        isWaiting = !nestedCoroutine.HasFinished;
                    }

                }
                else if (currentType == typeof(Coroutine))
                {
                    // UNSUPPORTED
                    Debug.LogError("Nested Coroutines started by Unity's defaut StartCoroutine method are not supported in editor! please use EditorCoroutineRunner.Start instead. Canceling.");
                    canceled = true;
                } 
                else
                {
                    // UNSUPPORTED
                    Debug.LogError("Unsupported yield (" + currentType + ") in editor coroutine!! Canceling.");
                    canceled = true;
                }
            }
            lastUpdateTime = now;

            // have we been canceled?
            if (canceled)
            {
                Stop();
                return;
            }

            if (!isWaiting)
            {
                // nope were good! tick the coroutine!
                bool update = coroutine.MoveNext();

                if (update)
                {
                    // yup the coroutine returned true so its been ticked...

                    // lets see what it actually yielded
                    current = coroutine.Current;
                    if (current != null)
                    {
                        // is it a type we have to do extra processing on?
                        currentType = current.GetType();

                        if (currentType == typeof(WaitForSeconds))
                        {
                            // its a WaitForSeconds... lets use reflection to pull out how long the actual wait is for so we can process the wait
                            var wait = current as WaitForSeconds;
                            FieldInfo m_Seconds = typeof(WaitForSeconds).GetField("m_Seconds", BindingFlags.NonPublic | BindingFlags.Instance);
                            if (m_Seconds != null)
                            {
                                timer = (float)m_Seconds.GetValue(wait);
                            }
                        }
                        else if (currentType == typeof(EditorStatusUpdate))
                        {
                            // Special case yield that wants to update the UI!
                            var updateInfo = current as EditorStatusUpdate;
                            if (updateInfo.HasLabelUpdate)
                            {
                                Label = updateInfo.Label;
                            }
                            if (updateInfo.HasPercentUpdate)
                            {
                                PercentComplete = updateInfo.PercentComplete;
                            }
                        }
                    }
                }
                else
                {
                    // Coroutine returned false so its finally finished!!
                    Stop();
                }
            }
        }
    }

    private void Stop()
    {
        // Coroutine has finished! do some cleanup...
        coroutine = null;
        editorCoroutineYieldInstruction.HasFinished = true;
    }

    public void UpdateUI()
    {
        if (cancelable)
        {
            canceled = EditorUtility.DisplayCancelableProgressBar(title, Label, PercentComplete);
            if (canceled)
                Debug.Log("CANCLED");
        }
        else
        {
            EditorUtility.DisplayProgressBar(title, Label, PercentComplete);
        }
    }
}

/// <summary>
/// Coroutine Yield instruction that allows an Editor Coroutine to update the Coroutine runner UI
/// </summary>
public class EditorStatusUpdate : CustomYieldInstruction
{
    public string Label;
    public float PercentComplete;

    public bool HasLabelUpdate;
    public bool HasPercentUpdate;

    public override bool keepWaiting
    {
        get
        {
            // always go to the next update
            return false;
        }
    }

    public EditorStatusUpdate(string label)
    {
        HasPercentUpdate = false;

        HasLabelUpdate = true;
        Label = label;
    }

    public EditorStatusUpdate(float percent)
    {
        HasPercentUpdate = true;
        PercentComplete = percent;

        HasLabelUpdate = false;
    }

    public EditorStatusUpdate(string label, float percent)
    {
        HasPercentUpdate = true;
        PercentComplete = percent;

        HasLabelUpdate = true;
        Label = label;
    }
}

/// <summary>
/// Created when an Editor Coroutine is started, can be yielded to to allow another coroutine to finish first.
/// </summary>
public class EditorCoroutine : YieldInstruction
{
    public bool HasFinished;
}