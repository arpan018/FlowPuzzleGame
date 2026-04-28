using UnityEngine;
using UnityEngine.Events;
using DG.Tweening;

namespace Game.UI
{
    public enum TweenEaseSource
    {
        AnimationCurve,
        DOTweenEase
    }

    public class CurrentAnimation
    {
        private AnimationParts animationPart;
        private RectTransform targetRectTransform;
        private Sequence activeSequence;
        private bool endedCalled;
        private bool finalEndCalled;

        public CurrentAnimation(AnimationParts animationPart)
        {
            this.animationPart = animationPart;
        }

        public void SetTarget(RectTransform rectTransform)
        {
            targetRectTransform = rectTransform;
        }

        public void AnimationFrame(RectTransform rectTransform)
        {
            SetTarget(rectTransform);
        }

        public void SetAnimationOnFrame(RectTransform rectTransform, float percentage)
        {
            SetTarget(rectTransform);
            ApplyAnimationValues(percentage, !animationPart.IsObjectOpened());
        }

        public void LateAnimationFrame(RectTransform rectTransform)
        {
            SetTarget(rectTransform);
            animationPart.FrameCheck();
        }

        public void PlayOpenAnimations()
        {
            PlayAnimations(true);
            animationPart.ChangeStatus();
            animationPart.CheckCallbackStatus();
        }

        public void SetStatus(bool status)
        {
            animationPart.SetStatus(status);
        }

        public void PlayCloseAnimations()
        {
            PlayAnimations(false);
            animationPart.ChangeStatus();
            animationPart.CheckCallbackStatus();
        }

        public void SetAnimationPos(Vector2 StartAnchoredPos, Vector2 EndAnchoredPos, AnimationCurve EntryTween, AnimationCurve ExitTween, RectTransform rectTransform)
        {
            animationPart.PositionPropetiesAnim.SetPositionEnable(true);
            animationPart.PositionPropetiesAnim.SetPosStart(StartAnchoredPos, rectTransform);
            animationPart.PositionPropetiesAnim.SetPosEnd(EndAnchoredPos, rectTransform.transform);
            animationPart.PositionPropetiesAnim.SetAniamtionsCurve(EntryTween, ExitTween);
        }

        public void SetAnimationScale(Vector2 StartAnchoredScale, Vector2 EndAnchoredScale, AnimationCurve EntryTween, AnimationCurve ExitTween)
        {
            animationPart.ScalePropetiesAnim.StartScale = StartAnchoredScale;
            animationPart.ScalePropetiesAnim.SetScaleEnable(true);
            animationPart.ScalePropetiesAnim.EndScale = EndAnchoredScale;
            animationPart.ScalePropetiesAnim.SetAniamtionsCurve(EntryTween, ExitTween);
        }

        public void SetAnimationRotation(Vector2 StartAnchoredEulerAng, Vector2 EndAnchoredEulerAng, AnimationCurve EntryTween, AnimationCurve ExitTween)
        {
            animationPart.RotationPropetiesAnim.SetRotationEnable(true);
            animationPart.RotationPropetiesAnim.StartRot = StartAnchoredEulerAng;
            animationPart.RotationPropetiesAnim.EndRot = EndAnchoredEulerAng;
            animationPart.RotationPropetiesAnim.SetAniamtionsCurve(EntryTween, ExitTween);
        }

        public void SetFade(bool OverrideFade)
        {
            animationPart.FadePropetiesAnim.SetFadeEnable(true);
            animationPart.FadePropetiesAnim.SetFadeOverride(OverrideFade);
        }

        public void SetFadeValuesStartEnd(float startAlphaValue, float endAlphaValue)
        {
            animationPart.FadePropetiesAnim.SetFadeValues(startAlphaValue, endAlphaValue);
        }

        public bool IsObjectOpened()
        {
            return animationPart.IsObjectOpened();
        }

        public void SetAniamtioDuration(float duration)
        {
            animationPart.SetAniamtioDuration(duration);
        }

        public float GetAnimationDuration()
        {
            return animationPart.GetAnimationDuration();
        }

        public void Kill()
        {
            KillActiveSequence();
        }

        private void PlayAnimations(bool opening)
        {
            if (targetRectTransform == null)
                return;

            KillActiveSequence();

            endedCalled = false;
            finalEndCalled = false;

            ApplyAnimationValues(0f, opening);

            float duration = Mathf.Max(0.01f, animationPart.GetAnimationDuration());
            // TODO: Later option - replace with direction-specific Start Delay and Exit Delay.
            // float delay = Mathf.Max(0f, animationPart.GetAnimationDelay());
            float delay = 0f;
            activeSequence = DOTween.Sequence().Pause();
            activeSequence.SetTarget(targetRectTransform);
            activeSequence.SetUpdate(animationPart.UnscaledTimeAnimation);

            bool hasTween = false;

            if (animationPart.PositionPropetiesAnim.IsPositionEnabled())
            {
                Tween moveTween = DOTween.To(() => 0f, value => ApplyPositionValue(value, opening), 1f, duration);
                moveTween.SetTarget(targetRectTransform);
                ApplyEase(moveTween,
                    opening ? animationPart.PositionPropetiesAnim.TweenCurveEnterPos : animationPart.PositionPropetiesAnim.TweenCurveExitPos,
                    opening ? animationPart.PositionPropetiesAnim.EnterEaseSource : animationPart.PositionPropetiesAnim.ExitEaseSource,
                    opening ? animationPart.PositionPropetiesAnim.EnterEase : animationPart.PositionPropetiesAnim.ExitEase);
                activeSequence.Insert(delay, moveTween);
                hasTween = true;
            }

            if (animationPart.RotationPropetiesAnim.IsRotationEnabled())
            {
                Tween rotateTween = DOTween.To(() => 0f, value => ApplyRotationValue(value, opening), 1f, duration);
                rotateTween.SetTarget(targetRectTransform);
                ApplyEase(rotateTween,
                    opening ? animationPart.RotationPropetiesAnim.TweenCurveEnterRot : animationPart.RotationPropetiesAnim.TweenCurveExitRot,
                    opening ? animationPart.RotationPropetiesAnim.EnterEaseSource : animationPart.RotationPropetiesAnim.ExitEaseSource,
                    opening ? animationPart.RotationPropetiesAnim.EnterEase : animationPart.RotationPropetiesAnim.ExitEase);
                activeSequence.Insert(delay, rotateTween);
                hasTween = true;
            }

            if (animationPart.ScalePropetiesAnim.IsScaleEnabled())
            {
                Tween scaleTween = DOTween.To(() => 0f, value => ApplyScaleValue(value, opening), 1f, duration);
                scaleTween.SetTarget(targetRectTransform);
                ApplyEase(scaleTween,
                    opening ? animationPart.ScalePropetiesAnim.TweenCurveEnterScale : animationPart.ScalePropetiesAnim.TweenCurveExitScale,
                    opening ? animationPart.ScalePropetiesAnim.EnterEaseSource : animationPart.ScalePropetiesAnim.ExitEaseSource,
                    opening ? animationPart.ScalePropetiesAnim.EnterEase : animationPart.ScalePropetiesAnim.ExitEase);
                activeSequence.Insert(delay, scaleTween);
                hasTween = true;
            }

            if (animationPart.FadePropetiesAnim.IsFadeEnabled())
            {
                CanvasGroup canvasGroup = targetRectTransform.GetComponent<CanvasGroup>();

                if (canvasGroup != null)
                {
                    Tween fadeTween = DOTween.To(() => 0f, value => ApplyFadeValue(value, opening), 1f, duration);
                    fadeTween.SetTarget(canvasGroup);
                    ApplyEase(fadeTween,
                        opening ? animationPart.FadePropetiesAnim.TweenCurveEnterFade : animationPart.FadePropetiesAnim.TweenCurveExitFade,
                        opening ? animationPart.FadePropetiesAnim.EnterEaseSource : animationPart.FadePropetiesAnim.ExitEaseSource,
                        opening ? animationPart.FadePropetiesAnim.EnterEase : animationPart.FadePropetiesAnim.ExitEase);
                    activeSequence.Insert(delay, fadeTween);
                    hasTween = true;
                }
            }

            if (!hasTween)
            {
                activeSequence.AppendInterval(delay + duration);
            }

            if (!animationPart.AtomicAnimation)
            {
                activeSequence.InsertCallback(delay + duration * 0.9f, TriggerEnded);
            }

            activeSequence.OnUpdate(animationPart.FrameCheck);
            activeSequence.OnComplete(() =>
            {
                if (animationPart.AtomicAnimation)
                {
                    TriggerEnded();
                }

                TriggerFinalEnd();
            });

            activeSequence.Play();
        }

        private void ApplyAnimationValues(float percentage, bool opening)
        {
            if (targetRectTransform == null)
                return;

            if (animationPart.PositionPropetiesAnim.IsPositionEnabled())
            {
                ApplyPositionValue(percentage, opening);
            }

            if (animationPart.RotationPropetiesAnim.IsRotationEnabled())
            {
                ApplyRotationValue(percentage, opening);
            }

            if (animationPart.ScalePropetiesAnim.IsScaleEnabled())
            {
                ApplyScaleValue(percentage, opening);
            }

            if (animationPart.FadePropetiesAnim.IsFadeEnabled())
            {
                ApplyFadeValue(percentage, opening);
            }
        }

        private void ApplyPositionValue(float percentage, bool opening)
        {
            Vector3 startValue = opening ? animationPart.PositionPropetiesAnim.StartPos : animationPart.PositionPropetiesAnim.EndPos;
            Vector3 endValue = opening ? animationPart.PositionPropetiesAnim.EndPos : animationPart.PositionPropetiesAnim.StartPos;
            targetRectTransform.anchoredPosition = (Vector2)Vector3.LerpUnclamped(startValue, endValue, percentage);
        }

        private void ApplyRotationValue(float percentage, bool opening)
        {
            Vector3 startValue = opening ? animationPart.RotationPropetiesAnim.StartRot : animationPart.RotationPropetiesAnim.EndRot;
            Vector3 endValue = opening ? animationPart.RotationPropetiesAnim.EndRot : animationPart.RotationPropetiesAnim.StartRot;
            targetRectTransform.localEulerAngles = Vector3.LerpUnclamped(startValue, endValue, percentage);
        }

        private void ApplyScaleValue(float percentage, bool opening)
        {
            Vector3 startValue = opening ? animationPart.ScalePropetiesAnim.StartScale : animationPart.ScalePropetiesAnim.EndScale;
            Vector3 endValue = opening ? animationPart.ScalePropetiesAnim.EndScale : animationPart.ScalePropetiesAnim.StartScale;
            targetRectTransform.localScale = Vector3.LerpUnclamped(startValue, endValue, percentage);
        }

        private void ApplyFadeValue(float percentage, bool opening)
        {
            CanvasGroup canvasGroup = targetRectTransform.GetComponent<CanvasGroup>();

            if (canvasGroup != null)
            {
                float startValue = opening ? animationPart.FadePropetiesAnim.GetStartFadeValue() : animationPart.FadePropetiesAnim.GetEndFadeValue();
                float endValue = opening ? animationPart.FadePropetiesAnim.GetEndFadeValue() : animationPart.FadePropetiesAnim.GetStartFadeValue();
                canvasGroup.alpha = Mathf.LerpUnclamped(startValue, endValue, percentage);
            }
        }

        private void ApplyEase(Tween tween, AnimationCurve curve, TweenEaseSource easeSource, Ease ease)
        {
            if (easeSource == TweenEaseSource.AnimationCurve && curve != null && curve.length > 0)
            {
                tween.SetEase(curve);
            }
            else
            {
                tween.SetEase(ease);
            }
        }

        private void TriggerEnded()
        {
            if (endedCalled)
                return;

            endedCalled = true;
            animationPart.Ended();
        }

        private void TriggerFinalEnd()
        {
            if (finalEndCalled)
                return;

            finalEndCalled = true;
            animationPart.FinalEnd();
        }

        private void KillActiveSequence()
        {
            if (activeSequence == null)
                return;

            activeSequence.Kill();
            activeSequence = null;
        }
    }

    [System.Serializable]
    public class PositionPropetiesAnim
    {
        #region PositionEditor

        [SerializeField]
        [HideInInspector]
        private bool positionEnabled;

        public void SetPositionEnable(bool enabled)
        {
            positionEnabled = enabled;
        }

        public bool IsPositionEnabled()
        {
            return positionEnabled;
        }

        [HideInInspector]
        public AnimationCurve TweenCurveEnterPos;
        [HideInInspector]
        public AnimationCurve TweenCurveExitPos;
        [HideInInspector]
        public TweenEaseSource EnterEaseSource = TweenEaseSource.AnimationCurve;
        [HideInInspector]
        public TweenEaseSource ExitEaseSource = TweenEaseSource.AnimationCurve;
        [HideInInspector]
        public Ease EnterEase = Ease.Linear;
        [HideInInspector]
        public Ease ExitEase = Ease.Linear;
        [HideInInspector]
        public Vector3 StartPos;
        [HideInInspector]
        public Vector3 EndPos;
#if UNITY_EDITOR
        [SerializeField]
        [HideInInspector]
        public Vector3 StartWorldPos;
        [SerializeField]
        [HideInInspector]
        public Vector3 EndWorldPos;
#endif

        public void SetPosStart(Vector3 StartPos, RectTransform rectTr)
        {
            this.StartPos = StartPos;
#if UNITY_EDITOR
            float xMes = (rectTr.anchorMin.x + rectTr.anchorMax.x) / 2f;
            float yMes = (rectTr.anchorMin.y + rectTr.anchorMax.y) / 2f;

            Transform rootObject = rectTr.root;

            Rect rectangleScreen = rootObject.GetComponent<RectTransform>().rect;

            StartWorldPos.x = (xMes * rectangleScreen.width + StartPos.x) * rootObject.localScale.x;
            StartWorldPos.y = (yMes * rectangleScreen.height + StartPos.y) * rootObject.localScale.y;
#endif
        }

        public void SetPosEnd(Vector3 EndPos, Transform rectTr)
        {
            this.EndPos = EndPos;
#if UNITY_EDITOR
            EndWorldPos.x = StartWorldPos.x + (EndPos.x - StartPos.x) * rectTr.root.localScale.x;
            EndWorldPos.y = StartWorldPos.y + (EndPos.y - StartPos.y) * rectTr.root.localScale.y;
#endif
        }

        public void SetAniamtionsCurve(AnimationCurve EntryTween, AnimationCurve ExitTween)
        {
            TweenCurveEnterPos = EntryTween;
            TweenCurveExitPos = ExitTween;
        }

        #endregion
    }

    [System.Serializable]
    public class ScalePropetiesAnim
    {
        #region ScaleEditor

        [SerializeField]
        [HideInInspector]
        private bool scaleEnabled;

        public void SetScaleEnable(bool enabled)
        {
            scaleEnabled = enabled;
        }

        public bool IsScaleEnabled()
        {
            return scaleEnabled;
        }

        [HideInInspector]
        public AnimationCurve TweenCurveEnterScale;
        [HideInInspector]
        public AnimationCurve TweenCurveExitScale;
        [HideInInspector]
        public TweenEaseSource EnterEaseSource = TweenEaseSource.AnimationCurve;
        [HideInInspector]
        public TweenEaseSource ExitEaseSource = TweenEaseSource.AnimationCurve;
        [HideInInspector]
        public Ease EnterEase = Ease.Linear;
        [HideInInspector]
        public Ease ExitEase = Ease.Linear;
        [HideInInspector]
        public Vector3 StartScale;
        [HideInInspector]
        public Vector3 EndScale;

        public void SetAniamtionsCurve(AnimationCurve EntryTween, AnimationCurve ExitTween)
        {
            TweenCurveEnterScale = EntryTween;
            TweenCurveExitScale = ExitTween;
        }

        #endregion
    }

    [System.Serializable]
    public class RotationPropetiesAnim
    {
        #region RotationEditor

        [SerializeField]
        [HideInInspector]
        private bool rotationEnabled;

        public void SetRotationEnable(bool enabled)
        {
            rotationEnabled = enabled;
        }

        public bool IsRotationEnabled()
        {
            return rotationEnabled;
        }

        [HideInInspector]
        public AnimationCurve TweenCurveEnterRot;
        [HideInInspector]
        public AnimationCurve TweenCurveExitRot;
        [HideInInspector]
        public TweenEaseSource EnterEaseSource = TweenEaseSource.AnimationCurve;
        [HideInInspector]
        public TweenEaseSource ExitEaseSource = TweenEaseSource.AnimationCurve;
        [HideInInspector]
        public Ease EnterEase = Ease.Linear;
        [HideInInspector]
        public Ease ExitEase = Ease.Linear;
        [HideInInspector]
        public Vector3 StartRot;
        [HideInInspector]
        public Vector3 EndRot;

        public void SetAniamtionsCurve(AnimationCurve EntryTween, AnimationCurve ExitTween)
        {
            TweenCurveEnterRot = EntryTween;
            TweenCurveExitRot = ExitTween;
        }

        #endregion
    }

    [System.Serializable]
    public class FadePropetiesAnim
    {
        #region FadeEditor

        [SerializeField]
        [HideInInspector]
        private bool fadeInOutEnabled;

        [SerializeField]
        [HideInInspector]
        private bool fadeOverride;

        [SerializeField]
        [HideInInspector]
        private float startFade = 0f;

        [SerializeField]
        [HideInInspector]
        private float endFade = 1f;

        [HideInInspector]
        public AnimationCurve TweenCurveEnterFade;
        [HideInInspector]
        public AnimationCurve TweenCurveExitFade;
        [HideInInspector]
        public TweenEaseSource EnterEaseSource = TweenEaseSource.AnimationCurve;
        [HideInInspector]
        public TweenEaseSource ExitEaseSource = TweenEaseSource.AnimationCurve;
        [HideInInspector]
        public Ease EnterEase = Ease.Linear;
        [HideInInspector]
        public Ease ExitEase = Ease.Linear;

        public void SetFadeEnable(bool enabled)
        {
            fadeInOutEnabled = enabled;
        }

        public void SetFadeValues(float startFade, float endFade)
        {
            if (endFade < startFade)
            {
                Debug.LogError("End Value should be greater than the start value, values not changed");
                return;
            }


            this.startFade = startFade;
            this.endFade = endFade;
        }

        public float GetStartFadeValue()
        {
            return startFade;
        }

        public float GetEndFadeValue()
        {
            return endFade;
        }

        public bool IsFadeEnabled()
        {
            return fadeInOutEnabled;
        }

        public void SetFadeOverride(bool enabled)
        {
            fadeOverride = enabled;
        }

        public bool IsFadeOverrideEnabled()
        {
            return fadeOverride;
        }

        #endregion
    }

    public interface IAniamtionPartProxy
    {
        bool IsObjectOpened();

        void ChangeStatus();

        void SetAniamtioDuration(float duration);

        float GetAnimationDuration();
    }

    [System.Serializable]
    public class AnimationParts : IAniamtionPartProxy
    {
        public delegate void DisableOrDestroy(bool disable, AnimationParts part);

        public static event DisableOrDestroy OnDisableOrDestroy;

        #region PositionEditor

        [HideInInspector]
        public PositionPropetiesAnim PositionPropetiesAnim = new PositionPropetiesAnim();

        #endregion

        #region ScaleEditor

        [HideInInspector]
        public ScalePropetiesAnim ScalePropetiesAnim = new ScalePropetiesAnim();

        #endregion

        #region RotationEditor

        [HideInInspector]
        public RotationPropetiesAnim RotationPropetiesAnim = new RotationPropetiesAnim();

        #endregion

        #region FadeEditor

        [HideInInspector]
        public FadePropetiesAnim FadePropetiesAnim = new FadePropetiesAnim();

        #endregion

        #region PUBLIC_Var

        public void SetAniamtioDuration(float duration)
        {
            if (duration > 0f)
                animationDuration = duration;
            else
                duration = 0.01f;
        }

        public float GetAnimationDuration()
        {
            return animationDuration;
        }

        // TODO: Later option - replace with direction-specific Start Delay and Exit Delay.
        // public void SetAnimationDelay(float delay)
        // {
        //     animationDelay = Mathf.Max(0f, delay);
        // }
        //
        // public float GetAnimationDelay()
        // {
        //     return animationDelay;
        // }

        public bool UnscaledTimeAnimation = false;
        public bool SaveState = false;
        public bool AtomicAnimation = false;

        public enum State
        {
            OPEN,
            CLOSE
        }

        ;

        public State ObjectState = State.CLOSE;


        public enum EndTweenClose
        {
            DEACTIVATE,
            DESTROY,
            NOTHING
        }

        ;

        public EndTweenClose EndState = EndTweenClose.DEACTIVATE;

        public enum CallbackCall
        {
            END_OF_INTRO_ANIM,
            END_OF_EXIT_ANIM,
            END_OF_INTRO_AND_END_OF_EXIT_ANIM,
            START_INTRO_ANIM,
            START_INTRO_END_OF_EXIT_ANIM,
            START_INTRO_END_OF_INTRO_ANIM,
            START_INTRO_END_OF_INTRO_AND_END_OF_EXIT_ANIM,
            START_EXIT_ANIM,
            START_EXIT_START_INTRO_ANIM,
            START_EXIT_END_OF_EXIT_ANIM,
            START_EXIT_END_OF_INTRO_ANIM,
            START_EXIT_END_OF_INTRO_AND_END_OF_EXIT_ANIM,
            START_INTRO_AND_START_EXIT_END_OF_EXIT_ANIM,
            START_INTRO_AND_START_EXIT_END_OF_INTRO_ANIM,
            START_INTRO_AND_START_EXIT_END_OF_INTRO_AND_END_OF_EXIT_ANIM,
            NOTHING
        }

        ;

        public CallbackCall CallCallback = CallbackCall.END_OF_INTRO_ANIM;

        public UnityEvent IntroEvents = new UnityEvent();
        public UnityEvent ExitEvents = new UnityEvent();
        private UnityEvent CallBackObject;

        #endregion

        #region PRIVATE_Var

        private bool CheckNextFrame = false;
        private bool CallOnThisFrame = false;

        [SerializeField]
        [HideInInspector]
        private float animationDuration = 1f;

        // TODO: Later option - replace with direction-specific Start Delay and Exit Delay.
        // [SerializeField]
        // [HideInInspector]
        // private float animationDelay = 0f;

        #endregion

        #region PUBLIC_Methods

        public AnimationParts(State ObjectState, bool UnscaledTimeAnimation, bool SaveState, bool AtomicAnim, EndTweenClose EndState, CallbackCall CallCallback, UnityEvent IntroEvents, UnityEvent ExitEvents)
        {
            this.ObjectState = ObjectState;
            this.UnscaledTimeAnimation = UnscaledTimeAnimation;
            this.SaveState = SaveState;
            this.AtomicAnimation = AtomicAnim;
            this.EndState = EndState;
            this.CallCallback = CallCallback;
            this.IntroEvents = IntroEvents;
            this.ExitEvents = ExitEvents;
        }

        public void CheckCallbackStatus()
        {
            if (CallCallback != CallbackCall.NOTHING)
            {
                if ((CallCallback == CallbackCall.START_INTRO_END_OF_EXIT_ANIM
                    || CallCallback == CallbackCall.START_INTRO_ANIM
                    || CallCallback == CallbackCall.START_INTRO_END_OF_INTRO_ANIM
                    || CallCallback == CallbackCall.START_INTRO_END_OF_INTRO_AND_END_OF_EXIT_ANIM
                    || CallCallback == CallbackCall.START_INTRO_AND_START_EXIT_END_OF_EXIT_ANIM
                    || CallCallback == CallbackCall.START_INTRO_AND_START_EXIT_END_OF_INTRO_ANIM
                    || CallCallback == CallbackCall.START_INTRO_AND_START_EXIT_END_OF_INTRO_AND_END_OF_EXIT_ANIM
                    || CallCallback == CallbackCall.START_EXIT_START_INTRO_ANIM) && ObjectState == State.OPEN)
                {
                    CheckCallBack(IntroEvents);
                }
                else if ((CallCallback == CallbackCall.START_EXIT_END_OF_EXIT_ANIM
                         || CallCallback == CallbackCall.START_EXIT_ANIM
                         || CallCallback == CallbackCall.START_EXIT_END_OF_INTRO_ANIM
                         || CallCallback == CallbackCall.START_EXIT_END_OF_INTRO_AND_END_OF_EXIT_ANIM
                         || CallCallback == CallbackCall.START_INTRO_AND_START_EXIT_END_OF_EXIT_ANIM
                         || CallCallback == CallbackCall.START_INTRO_AND_START_EXIT_END_OF_INTRO_ANIM
                         || CallCallback == CallbackCall.START_INTRO_AND_START_EXIT_END_OF_INTRO_AND_END_OF_EXIT_ANIM
                         || CallCallback == CallbackCall.START_EXIT_START_INTRO_ANIM) && ObjectState == State.CLOSE)
                {
                    CheckCallBack(ExitEvents);
                }
            }
        }

        public void FinalEnd()
        {
            if (ObjectState == State.CLOSE)
            {
                if (EndState == EndTweenClose.DEACTIVATE)
                {
                    if (OnDisableOrDestroy != null)
                    {
                        OnDisableOrDestroy(true, this);
                    }
                }
                else if (EndState == EndTweenClose.DESTROY)
                {
                    if (OnDisableOrDestroy != null)
                    {
                        OnDisableOrDestroy(false, this);
                    }
                }
            }

            if (SaveState)
            {
                ObjectState = (ObjectState == State.OPEN) ? State.CLOSE : State.OPEN;
            }
        }

        public void Ended()
        {
            if (CallCallback != CallbackCall.NOTHING)
            {
                if (ObjectState == State.CLOSE)
                {
                    if (CallCallback == CallbackCall.END_OF_EXIT_ANIM
                        || CallCallback == CallbackCall.END_OF_INTRO_AND_END_OF_EXIT_ANIM
                        || CallCallback == CallbackCall.START_INTRO_END_OF_EXIT_ANIM
                        || CallCallback == CallbackCall.START_INTRO_END_OF_INTRO_AND_END_OF_EXIT_ANIM
                        || CallCallback == CallbackCall.START_EXIT_END_OF_EXIT_ANIM
                        || CallCallback == CallbackCall.START_EXIT_END_OF_INTRO_AND_END_OF_EXIT_ANIM
                        || CallCallback == CallbackCall.START_INTRO_AND_START_EXIT_END_OF_EXIT_ANIM
                        || CallCallback == CallbackCall.START_INTRO_AND_START_EXIT_END_OF_INTRO_AND_END_OF_EXIT_ANIM)
                    {
                        CheckCallBack(ExitEvents);
                    }
                }

                if ((CallCallback == CallbackCall.END_OF_INTRO_ANIM
                    || CallCallback == CallbackCall.END_OF_INTRO_AND_END_OF_EXIT_ANIM
                    || CallCallback == CallbackCall.START_INTRO_END_OF_INTRO_ANIM
                    || CallCallback == CallbackCall.START_INTRO_END_OF_INTRO_AND_END_OF_EXIT_ANIM
                    || CallCallback == CallbackCall.START_EXIT_END_OF_INTRO_ANIM
                    || CallCallback == CallbackCall.START_EXIT_END_OF_INTRO_AND_END_OF_EXIT_ANIM
                    || CallCallback == CallbackCall.START_INTRO_AND_START_EXIT_END_OF_INTRO_ANIM
                    || CallCallback == CallbackCall.START_INTRO_AND_START_EXIT_END_OF_INTRO_AND_END_OF_EXIT_ANIM) && ObjectState == State.OPEN)
                {
                    CheckCallBack(IntroEvents);
                }
            }
        }

        public void FrameCheck()
        {
            if (CheckNextFrame)
            {
                if (CallOnThisFrame)
                {
                    CallCallbackObjects();
                }

                CallOnThisFrame = !CallOnThisFrame;
            }
        }

        public bool IsObjectOpened()
        {
            if (ObjectState == State.CLOSE)
            {
                return false;
            }

            return true;
        }

        public void ChangeStatus()
        {
            if (ObjectState == State.CLOSE)
            {
                ObjectState = State.OPEN;
            }
            else
            {
                ObjectState = State.CLOSE;
            }
        }

        public void SetStatus(bool open)
        {
            if (open)
            {
                ObjectState = State.OPEN;
            }
            else
            {
                ObjectState = State.CLOSE;
            }
        }

        #endregion

        #region PRIVATE_Methods

        private void CheckCallBack(UnityEvent CallbackObject)
        {
            this.CallBackObject = CallbackObject;
            CheckNextFrame = !CheckNextFrame;
        }

        private void CallCallbackObjects()
        {
            CheckNextFrame = !CheckNextFrame;

            CallBackObject.Invoke();
        }

        #endregion
    }
}
