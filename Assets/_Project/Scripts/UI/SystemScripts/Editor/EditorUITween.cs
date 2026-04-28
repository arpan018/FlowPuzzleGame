using UnityEngine;
using UnityEditor;
using DG.Tweening;

namespace Game.UI
{

    [CustomEditor(typeof(ReferencedFrom))]
    public class ReferencedProxy : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            EditorGUILayout.HelpBox("This object is an UI Object", MessageType.Info);
        }
    }

    [CustomEditor(typeof(EasyTween))]
    public class EditorUITween : Editor
    {
        private bool positionEnabled;
        private bool scaleEnabled;
        private bool rotationEnabled;
        private GUIStyle easeHeaderStyle;
        EasyTween tweenScript;

        public void OnEnable()
        {
            tweenScript = ((EasyTween)target);
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.BeginVertical();

            DrawDefaultInspector();

            if (tweenScript != null)
            {
                if (tweenScript.rectTransform)
                {
                    if (!Application.isPlaying)
                    {
                        tweenScript.animationParts.SetAniamtioDuration(EditorGUILayout.Slider("Animation Duration (Sec)", tweenScript.animationParts.GetAnimationDuration(), 0.01f, 10f));
                        // TODO: Later option - add separate Start Delay and Exit Delay controls.
                        // tweenScript.animationParts.SetAnimationDelay(EditorGUILayout.Slider("Animation Delay (Sec)", tweenScript.animationParts.GetAnimationDelay(), 0f, 10f));

                        EditorFade();
                        EditorPos();
                        EditorRot();
                        EditorScale();
                        GetButtonPos();

                        if (!tweenScript.rectTransform.gameObject.GetComponent<ReferencedFrom>())
                        {
                            tweenScript.rectTransform.gameObject.AddComponent<ReferencedFrom>();
                        }

                        if (GUI.changed)
                        {
                            EditorUtility.SetDirty(tweenScript);
                        }
                    }
                    else
                    {
                        if (GUILayout.Button("Animate"))
                        {
                            tweenScript.OpenCloseObjectAnimation();
                        }
                        EditorGUILayout.HelpBox("Editor Not Available in Play Mode", MessageType.Info);
                    }
                }
                else
                {
                    EditorGUILayout.HelpBox("Please set \"Rect Trasnform Variable\" that contains \"RectTransform\" component. UI Components", MessageType.Info);
                }
            }

            EditorGUILayout.EndVertical();
        }

        void GetAniamButtons()
        {
            if (positionEnabled || rotationEnabled || scaleEnabled || tweenScript.animationParts.FadePropetiesAnim.IsFadeEnabled())
            {
                if (GUILayout.Button("Animate"))
                {
                    tweenScript.OpenCloseObjectAnimation();
                }
            }
        }

        void GetButtonPos()
        {
            EditorGUILayout.BeginHorizontal();
            if (positionEnabled || rotationEnabled || scaleEnabled)
            {
                if (GUILayout.Button("Get Start Values"))
                {
                    GetStartValues();
                }

                if (GUILayout.Button("Get End Values"))
                {
                    GetEndValues();
                }
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();

            GetAniamButtons();

            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            if (positionEnabled || rotationEnabled || scaleEnabled || tweenScript.animationParts.FadePropetiesAnim.IsFadeEnabled())
            {
                if (GUILayout.Button("Set To Start Values"))
                {
                    SetStartValues();
                }

                if (GUILayout.Button("Set To End Values"))
                {
                    SetEndValues();
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        void GetStartValues()
        {
            RectTransform selectedTransform = tweenScript.rectTransform;

            tweenScript.animationParts.PositionPropetiesAnim.SetPosStart((Vector3)selectedTransform.anchoredPosition, selectedTransform);
            tweenScript.animationParts.ScalePropetiesAnim.StartScale = selectedTransform.localScale;
            tweenScript.animationParts.RotationPropetiesAnim.StartRot = selectedTransform.localEulerAngles;
        }

        void GetEndValues()
        {
            RectTransform selectedTransform = tweenScript.rectTransform;

            tweenScript.animationParts.PositionPropetiesAnim.SetPosEnd((Vector3)selectedTransform.anchoredPosition, selectedTransform.transform);
            tweenScript.animationParts.ScalePropetiesAnim.EndScale = selectedTransform.localScale;
            tweenScript.animationParts.RotationPropetiesAnim.EndRot = selectedTransform.localEulerAngles;
        }

        void SetStartValues()
        {
            RectTransform selectedTransform = tweenScript.rectTransform;

            if (tweenScript.animationParts.PositionPropetiesAnim.IsPositionEnabled())
                selectedTransform.anchoredPosition = (Vector2)tweenScript.animationParts.PositionPropetiesAnim.StartPos;

            if (tweenScript.animationParts.ScalePropetiesAnim.IsScaleEnabled())
                selectedTransform.localScale = tweenScript.animationParts.ScalePropetiesAnim.StartScale;

            if (tweenScript.animationParts.RotationPropetiesAnim.IsRotationEnabled())
                selectedTransform.localEulerAngles = tweenScript.animationParts.RotationPropetiesAnim.StartRot;

            if (tweenScript.animationParts.FadePropetiesAnim.IsFadeEnabled())
            {
                if (tweenScript.IsObjectOpened())
                    SetAlphaValue(selectedTransform.transform, tweenScript.animationParts.FadePropetiesAnim.GetEndFadeValue());
                else
                    SetAlphaValue(selectedTransform.transform, tweenScript.animationParts.FadePropetiesAnim.GetStartFadeValue());
            }
        }

        void SetEndValues()
        {
            RectTransform selectedTransform = tweenScript.rectTransform;

            if (tweenScript.animationParts.PositionPropetiesAnim.IsPositionEnabled())
                selectedTransform.anchoredPosition = (Vector2)tweenScript.animationParts.PositionPropetiesAnim.EndPos;

            if (tweenScript.animationParts.ScalePropetiesAnim.IsScaleEnabled())
                selectedTransform.localScale = tweenScript.animationParts.ScalePropetiesAnim.EndScale;

            if (tweenScript.animationParts.RotationPropetiesAnim.IsRotationEnabled())
                selectedTransform.localEulerAngles = tweenScript.animationParts.RotationPropetiesAnim.EndRot;

            if (tweenScript.animationParts.FadePropetiesAnim.IsFadeEnabled())
            {
                if (tweenScript.IsObjectOpened())
                    SetAlphaValue(selectedTransform.transform, tweenScript.animationParts.FadePropetiesAnim.GetStartFadeValue());
                else
                    SetAlphaValue(selectedTransform.transform, tweenScript.animationParts.FadePropetiesAnim.GetEndFadeValue());
            }
        }

        void SetAlphaValue(Transform _objectToSetAlpha, float alphaValue)
        {
            if (_objectToSetAlpha.GetComponent<CanvasGroup>())
            {
                CanvasGroup GraphicElement = _objectToSetAlpha.GetComponent<CanvasGroup>();
                GraphicElement.alpha = alphaValue;
            }

            if (_objectToSetAlpha.childCount > 0)
            {
                CanvasGroup GraphicElement = _objectToSetAlpha.GetComponent<CanvasGroup>();
                GraphicElement.alpha = alphaValue;
            }
        }

        void DrawEaseField(string directionLabel, ref TweenEaseSource easeSource, ref Ease ease, ref AnimationCurve animationCurve)
        {
            EditorGUILayout.BeginVertical(GUILayout.MinWidth(150f));
            EditorGUILayout.LabelField(directionLabel, GetEaseHeaderStyle());

            float previousLabelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 0f;

            easeSource = (TweenEaseSource)EditorGUILayout.EnumPopup(GUIContent.none, easeSource);

            if (easeSource == TweenEaseSource.AnimationCurve)
            {
                if (animationCurve == null)
                {
                    animationCurve = new AnimationCurve();
                }

                animationCurve = EditorGUILayout.CurveField(GUIContent.none, animationCurve);
            }
            else
            {
                ease = (Ease)EditorGUILayout.EnumPopup(GUIContent.none, ease);
            }

            EditorGUIUtility.labelWidth = previousLabelWidth;
            EditorGUILayout.EndVertical();
        }

        void DrawEaseFields(
                ref TweenEaseSource enterEaseSource,
                ref Ease enterEase,
                ref AnimationCurve enterAnimationCurve,
                ref TweenEaseSource exitEaseSource,
                ref Ease exitEase,
                ref AnimationCurve exitAnimationCurve)
        {
            EditorGUILayout.Space(10f);
            EditorGUILayout.BeginHorizontal();

            DrawEaseField("Start Ease Mode / Value", ref enterEaseSource, ref enterEase, ref enterAnimationCurve);
            EditorGUILayout.Space();
            DrawEaseField("Exit Ease Mode / Value", ref exitEaseSource, ref exitEase, ref exitAnimationCurve);

            EditorGUILayout.EndHorizontal();
        }

        GUIStyle GetEaseHeaderStyle()
        {
            if (easeHeaderStyle == null)
            {
                easeHeaderStyle = new GUIStyle(EditorStyles.miniBoldLabel);
                int baseFontSize = easeHeaderStyle.fontSize > 0 ? easeHeaderStyle.fontSize : 10;
                easeHeaderStyle.fontSize = Mathf.RoundToInt(baseFontSize * 1.2f);
            }

            return easeHeaderStyle;
        }

        void EditorFade()
        {
            tweenScript.animationParts.FadePropetiesAnim.SetFadeEnable(EditorGUILayout.BeginToggleGroup("Fade In & Out",
                    tweenScript.animationParts.FadePropetiesAnim.IsFadeEnabled()));


            if (tweenScript.animationParts.FadePropetiesAnim.IsFadeEnabled())
            {
                EditorGUILayout.LabelField("************************* Must Add CanvasGroup **************************");


                EditorGUILayout.LabelField("Fade Start and End Values");

                EditorGUILayout.BeginHorizontal();

                float fadeValueStart = EditorGUILayout.FloatField("Start Value", tweenScript.animationParts.FadePropetiesAnim.GetStartFadeValue());
                float fadeValueEnd = EditorGUILayout.FloatField("End Value", tweenScript.animationParts.FadePropetiesAnim.GetEndFadeValue());

                tweenScript.animationParts.FadePropetiesAnim.SetFadeValues(fadeValueStart, fadeValueEnd);

                EditorGUILayout.EndHorizontal();

                tweenScript.animationParts.FadePropetiesAnim.SetFadeOverride(EditorGUILayout.BeginToggleGroup("Fade Override",
                        tweenScript.animationParts.FadePropetiesAnim.IsFadeOverrideEnabled()));

                EditorGUILayout.EndToggleGroup();

                DrawEaseFields(
                        ref tweenScript.animationParts.FadePropetiesAnim.EnterEaseSource,
                        ref tweenScript.animationParts.FadePropetiesAnim.EnterEase,
                        ref tweenScript.animationParts.FadePropetiesAnim.TweenCurveEnterFade,
                        ref tweenScript.animationParts.FadePropetiesAnim.ExitEaseSource,
                        ref tweenScript.animationParts.FadePropetiesAnim.ExitEase,
                        ref tweenScript.animationParts.FadePropetiesAnim.TweenCurveExitFade);
            }

            EditorGUILayout.EndToggleGroup();
        }

        void EditorPos()
        {
            tweenScript.animationParts.PositionPropetiesAnim.SetPositionEnable(EditorGUILayout.BeginToggleGroup("Position Animation",
                    tweenScript.animationParts.PositionPropetiesAnim.IsPositionEnabled()));
            positionEnabled = tweenScript.animationParts.PositionPropetiesAnim.IsPositionEnabled();

            if (positionEnabled)
            {
                tweenScript.animationParts.PositionPropetiesAnim.SetPosStart(EditorGUILayout.Vector3Field("Start Move", tweenScript.animationParts.PositionPropetiesAnim.StartPos), tweenScript.rectTransform);
                tweenScript.animationParts.PositionPropetiesAnim.SetPosEnd(EditorGUILayout.Vector3Field("End Move", tweenScript.animationParts.PositionPropetiesAnim.EndPos), tweenScript.rectTransform.transform);

                DrawEaseFields(
                        ref tweenScript.animationParts.PositionPropetiesAnim.EnterEaseSource,
                        ref tweenScript.animationParts.PositionPropetiesAnim.EnterEase,
                        ref tweenScript.animationParts.PositionPropetiesAnim.TweenCurveEnterPos,
                        ref tweenScript.animationParts.PositionPropetiesAnim.ExitEaseSource,
                        ref tweenScript.animationParts.PositionPropetiesAnim.ExitEase,
                        ref tweenScript.animationParts.PositionPropetiesAnim.TweenCurveExitPos);

                EditorGUILayout.Space();
            }

            EditorGUILayout.EndToggleGroup();
        }

        void EditorScale()
        {
            tweenScript.animationParts.ScalePropetiesAnim.SetScaleEnable(EditorGUILayout.BeginToggleGroup("Scale Animation",
                    tweenScript.animationParts.ScalePropetiesAnim.IsScaleEnabled()));
            scaleEnabled = tweenScript.animationParts.ScalePropetiesAnim.IsScaleEnabled();

            if (scaleEnabled)
            {
                tweenScript.animationParts.ScalePropetiesAnim.StartScale = EditorGUILayout.Vector3Field("Start Scale", tweenScript.animationParts.ScalePropetiesAnim.StartScale);
                tweenScript.animationParts.ScalePropetiesAnim.EndScale = EditorGUILayout.Vector3Field("End Scale", tweenScript.animationParts.ScalePropetiesAnim.EndScale);

                DrawEaseFields(
                        ref tweenScript.animationParts.ScalePropetiesAnim.EnterEaseSource,
                        ref tweenScript.animationParts.ScalePropetiesAnim.EnterEase,
                        ref tweenScript.animationParts.ScalePropetiesAnim.TweenCurveEnterScale,
                        ref tweenScript.animationParts.ScalePropetiesAnim.ExitEaseSource,
                        ref tweenScript.animationParts.ScalePropetiesAnim.ExitEase,
                        ref tweenScript.animationParts.ScalePropetiesAnim.TweenCurveExitScale);

                EditorGUILayout.Space();
            }

            EditorGUILayout.EndToggleGroup();
        }

        void EditorRot()
        {
            tweenScript.animationParts.RotationPropetiesAnim.SetRotationEnable(EditorGUILayout.BeginToggleGroup("Rotation Animation",
                    tweenScript.animationParts.RotationPropetiesAnim.IsRotationEnabled()));
            rotationEnabled = tweenScript.animationParts.RotationPropetiesAnim.IsRotationEnabled();

            if (rotationEnabled)
            {
                tweenScript.animationParts.RotationPropetiesAnim.StartRot = EditorGUILayout.Vector3Field("Start Rotation", tweenScript.animationParts.RotationPropetiesAnim.StartRot);
                tweenScript.animationParts.RotationPropetiesAnim.EndRot = EditorGUILayout.Vector3Field("End Rotation", tweenScript.animationParts.RotationPropetiesAnim.EndRot);

                DrawEaseFields(
                        ref tweenScript.animationParts.RotationPropetiesAnim.EnterEaseSource,
                        ref tweenScript.animationParts.RotationPropetiesAnim.EnterEase,
                        ref tweenScript.animationParts.RotationPropetiesAnim.TweenCurveEnterRot,
                        ref tweenScript.animationParts.RotationPropetiesAnim.ExitEaseSource,
                        ref tweenScript.animationParts.RotationPropetiesAnim.ExitEase,
                        ref tweenScript.animationParts.RotationPropetiesAnim.TweenCurveExitRot);

                EditorGUILayout.Space();
            }

            EditorGUILayout.EndToggleGroup();
        }
    }
}
