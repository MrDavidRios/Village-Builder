﻿using System.Collections;
using UnityEngine;

public class SelectionCursor : MonoBehaviour
{
    [Header("Animation")]
    //Booleans
    public bool animateBracketDistance;

    public bool animatePositionChange;
    public bool animateScaleChange;

    //Floats
    [LabelOverride("Side Distance Apart")] public float sideDist = 1f;

    [LabelOverride("Maximum Side Distance Apart")]
    public float sideDistMax;

    [LabelOverride("Minimum Side Distance Apart")]
    public float sideDistMin;

    public float bracketDistanceAnimationSpeed;
    public float positionChangeAnimationSpeed;

    public float bracketScaleAnimationSpeed;

    //Transforms
    public Transform bottomLeftBracket;
    public Transform bottomRightBracket;
    public Transform topLeftBracket;
    public Transform topRightBracket;
    public float timeTakenDuringLerp = 1f;

    //Whether we are currently interpolating or not
    private bool _isLerping;

    //The Time.time value when we started the interpolation
    private float _timeStartedLerping;
    private Vector3 endPos;

    //The amount of pixels of space in between the 'animationSpeed' and the 'bottomLeftBracket' variables in the inspector. 
    //This helps to make the inspector look a little neater.
    [Space(10)]

    //Vector3s
    private Vector3 oldPosition;

    public IEnumerator updateScale;

    private void Start()
    {
        //Default, and minimum, side distance value.
        sideDistMin = sideDist;

        //Maximum side distance value.
        sideDistMax = sideDist * 1.25f;
    }

    private void Update()
    {
        //Debug.Log(Time.time + " vs. " + Time.unscaledTime);

        UpdateBracketPosition();

        FixPos();

        //Change the distance that the brackets are apart if the selection cursor is being animated.
        if (animateBracketDistance)
            //The 'sideDist' variable is decreased until it reaches its lowest possible value, 'sideDistOriginal', and increased until it reaches its highest possible value, 'sideDistMax.'
            sideDist = Mathf.Lerp(sideDistMin, sideDistMax,
                Mathf.PingPong(Time.unscaledTime * bracketDistanceAnimationSpeed, 1));
    }

    private void FixPos()
    {
        if (_isLerping)
        {
            //We want percentage = 0.0 when Time.time = _timeStartedLerping
            //and percentage = 1.0 when Time.time = _timeStartedLerping + timeTakenDuringLerp
            //In other words, we want to know what percentage of "timeTakenDuringLerp" the value
            //"Time.time - _timeStartedLerping" is.
            var timeSinceStarted = Time.unscaledTime - _timeStartedLerping;

            var percentageComplete = timeSinceStarted / timeTakenDuringLerp;

            //Perform the actual lerping.  Notice that the first two parameters will always be the same
            //throughout a single lerp-processs (ie. they won't change until we hit the space-bar again
            //to start another lerp)
            transform.position = Vector3.Lerp(transform.position, endPos, percentageComplete);

            //When we've completed the lerp, we set _isLerping to false
            if (percentageComplete >= 1.0f) _isLerping = false;
        }
    }

    //Update the positions of each bracket based on the changes to the 'sideDist' variable made in the Update() function.
    private void UpdateBracketPosition()
    {
        bottomLeftBracket.localPosition = new Vector3(-sideDist, 0, -sideDist);
        bottomRightBracket.localPosition = new Vector3(sideDist, 0, -sideDist);

        topLeftBracket.localPosition = new Vector3(-sideDist, 0, sideDist);
        topRightBracket.localPosition = new Vector3(sideDist, 0, sideDist);
    }

    public void UpdateSideDist(float newSideDist)
    {
        sideDist = newSideDist;

        sideDistMin = newSideDist;
        sideDistMax = newSideDist * 1.25f;
    }

    public void UpdateCursorPos(Vector3 pos, bool isAnimating = true)
    {
        if (!isAnimating)
            animatePositionChange = false;
        else
            animatePositionChange = true;

        if (animatePositionChange)
        {
            endPos = pos;
            _isLerping = true;
            _timeStartedLerping = Time.unscaledTime;
        }
        else
        {
            transform.position = pos;
        }
    }

    public void UpdateCursorPosCustom(Vector3 pos)
    {
        transform.position = pos;
    }

    public IEnumerator UpdateScale(Vector3 newScale)
    {
        if (!animateScaleChange)
        {
            transform.localScale = newScale;
            yield break;
        }

        if (transform.localScale == newScale)
            yield break;

        var currentScale = transform.localScale;
        var t = 0f;

        while (t < 1)
        {
            t += Time.unscaledDeltaTime / bracketScaleAnimationSpeed;
            transform.localScale = Vector3.Lerp(currentScale, newScale, t);

            yield return null;
        }

        updateScale = null;
    }
}