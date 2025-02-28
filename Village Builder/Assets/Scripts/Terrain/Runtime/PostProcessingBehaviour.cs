using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine.Rendering;

namespace UnityEngine.PostProcessing
{
    using DebugMode = BuiltinDebugViewsModel.Mode;

#if UNITY_5_4_OR_NEWER
    [ImageEffectAllowedInSceneView]
#endif
    [RequireComponent(typeof(Camera))]
    [DisallowMultipleComponent]
    [ExecuteInEditMode]
    [AddComponentMenu("Effects/Post-Processing Behaviour", -1)]
    public class PostProcessingBehaviour : MonoBehaviour
    {
        // Inspector fields
        public PostProcessingProfile profile;

        public Func<Vector2, Matrix4x4> jitteredMatrixFunc;
        private AmbientOcclusionComponent m_AmbientOcclusion;
        private BloomComponent m_Bloom;
        private Camera m_Camera;
        private ChromaticAberrationComponent m_ChromaticAberration;
        private ColorGradingComponent m_ColorGrading;

        // Internal helpers
        private Dictionary<Type, KeyValuePair<CameraEvent, CommandBuffer>> m_CommandBuffers;
        private List<PostProcessingComponentBase> m_Components;
        private Dictionary<PostProcessingComponentBase, bool> m_ComponentStates;
        private PostProcessingContext m_Context;

        // Effect components
        private BuiltinDebugViewsComponent m_DebugViews;
        private DepthOfFieldComponent m_DepthOfField;
        private DitheringComponent m_Dithering;
        private EyeAdaptationComponent m_EyeAdaptation;
        private FogComponent m_FogComponent;
        private FxaaComponent m_Fxaa;
        private GrainComponent m_Grain;

        private MaterialFactory m_MaterialFactory;
        private MotionBlurComponent m_MotionBlur;
        private PostProcessingProfile m_PreviousProfile;

        private bool m_RenderingInSceneView;
        private RenderTextureFactory m_RenderTextureFactory;
        private ScreenSpaceReflectionComponent m_ScreenSpaceReflection;
        private TaaComponent m_Taa;
        private UserLutComponent m_UserLut;
        private VignetteComponent m_Vignette;

        private void OnEnable()
        {
            m_CommandBuffers = new Dictionary<Type, KeyValuePair<CameraEvent, CommandBuffer>>();
            m_MaterialFactory = new MaterialFactory();
            m_RenderTextureFactory = new RenderTextureFactory();
            m_Context = new PostProcessingContext();

            // Keep a list of all post-fx for automation purposes
            m_Components = new List<PostProcessingComponentBase>();

            // Component list
            m_DebugViews = AddComponent(new BuiltinDebugViewsComponent());
            m_AmbientOcclusion = AddComponent(new AmbientOcclusionComponent());
            m_ScreenSpaceReflection = AddComponent(new ScreenSpaceReflectionComponent());
            m_FogComponent = AddComponent(new FogComponent());
            m_MotionBlur = AddComponent(new MotionBlurComponent());
            m_Taa = AddComponent(new TaaComponent());
            m_EyeAdaptation = AddComponent(new EyeAdaptationComponent());
            m_DepthOfField = AddComponent(new DepthOfFieldComponent());
            m_Bloom = AddComponent(new BloomComponent());
            m_ChromaticAberration = AddComponent(new ChromaticAberrationComponent());
            m_ColorGrading = AddComponent(new ColorGradingComponent());
            m_UserLut = AddComponent(new UserLutComponent());
            m_Grain = AddComponent(new GrainComponent());
            m_Vignette = AddComponent(new VignetteComponent());
            m_Dithering = AddComponent(new DitheringComponent());
            m_Fxaa = AddComponent(new FxaaComponent());

            // Prepare state observers
            m_ComponentStates = new Dictionary<PostProcessingComponentBase, bool>();

            foreach (var component in m_Components)
                m_ComponentStates.Add(component, false);

            useGUILayout = false;
        }

        private void OnDisable()
        {
            // Clear command buffers
            foreach (var cb in m_CommandBuffers.Values)
            {
                m_Camera.RemoveCommandBuffer(cb.Key, cb.Value);
                cb.Value.Dispose();
            }

            m_CommandBuffers.Clear();

            // Clear components
            if (profile != null)
                DisableComponents();

            m_Components.Clear();

            // Factories
            m_MaterialFactory.Dispose();
            m_RenderTextureFactory.Dispose();
            GraphicsUtils.Dispose();
        }

        private void OnGUI()
        {
            if (Event.current.type != EventType.Repaint)
                return;

            if (profile == null || m_Camera == null)
                return;

            if (m_EyeAdaptation.active && profile.debugViews.IsModeActive(DebugMode.EyeAdaptation))
                m_EyeAdaptation.OnGUI();
            else if (m_ColorGrading.active && profile.debugViews.IsModeActive(DebugMode.LogLut))
                m_ColorGrading.OnGUI();
            else if (m_UserLut.active && profile.debugViews.IsModeActive(DebugMode.UserLut))
                m_UserLut.OnGUI();
        }

        private void OnPostRender()
        {
            if (profile == null || m_Camera == null)
                return;

            if (!m_RenderingInSceneView && m_Taa.active && !profile.debugViews.willInterrupt)
                m_Context.camera.ResetProjectionMatrix();
        }

        private void OnPreCull()
        {
            // All the per-frame initialization logic has to be done in OnPreCull instead of Update
            // because [ImageEffectAllowedInSceneView] doesn't trigger Update events...

            m_Camera = GetComponent<Camera>();

            if (profile == null || m_Camera == null)
                return;

#if UNITY_EDITOR
            // Track the scene view camera to disable some effects we don't want to see in the
            // scene view
            // Currently disabled effects :
            //  - Temporal Antialiasing
            //  - Depth of Field
            //  - Motion blur
            m_RenderingInSceneView = SceneView.currentDrawingSceneView != null
                                     && SceneView.currentDrawingSceneView.camera == m_Camera;
#endif

            // Prepare context
            var context = m_Context.Reset();
            context.profile = profile;
            context.renderTextureFactory = m_RenderTextureFactory;
            context.materialFactory = m_MaterialFactory;
            context.camera = m_Camera;

            // Prepare components
            m_DebugViews.Init(context, profile.debugViews);
            m_AmbientOcclusion.Init(context, profile.ambientOcclusion);
            m_ScreenSpaceReflection.Init(context, profile.screenSpaceReflection);
            m_FogComponent.Init(context, profile.fog);
            m_MotionBlur.Init(context, profile.motionBlur);
            m_Taa.Init(context, profile.antialiasing);
            m_EyeAdaptation.Init(context, profile.eyeAdaptation);
            m_DepthOfField.Init(context, profile.depthOfField);
            m_Bloom.Init(context, profile.bloom);
            m_ChromaticAberration.Init(context, profile.chromaticAberration);
            m_ColorGrading.Init(context, profile.colorGrading);
            m_UserLut.Init(context, profile.userLut);
            m_Grain.Init(context, profile.grain);
            m_Vignette.Init(context, profile.vignette);
            m_Dithering.Init(context, profile.dithering);
            m_Fxaa.Init(context, profile.antialiasing);

            // Handles profile change and 'enable' state observers
            if (m_PreviousProfile != profile)
            {
                DisableComponents();
                m_PreviousProfile = profile;
            }

            CheckObservers();

            // Find out which camera flags are needed before rendering begins
            // Note that motion vectors will only be available one frame after being enabled
            var flags = context.camera.depthTextureMode;
            foreach (var component in m_Components)
                if (component.active)
                    flags |= component.GetCameraFlags();

            context.camera.depthTextureMode = flags;

            // Temporal antialiasing jittering, needs to happen before culling
            if (!m_RenderingInSceneView && m_Taa.active && !profile.debugViews.willInterrupt)
                m_Taa.SetProjectionMatrix(jitteredMatrixFunc);
        }

        private void OnPreRender()
        {
            if (profile == null)
                return;

            // Command buffer-based effects should be set-up here
            TryExecuteCommandBuffer(m_DebugViews);
            TryExecuteCommandBuffer(m_AmbientOcclusion);
            TryExecuteCommandBuffer(m_ScreenSpaceReflection);
            TryExecuteCommandBuffer(m_FogComponent);

            if (!m_RenderingInSceneView)
                TryExecuteCommandBuffer(m_MotionBlur);
        }

        // Classic render target pipeline for RT-based effects
        private void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            if (profile == null || m_Camera == null)
            {
                Graphics.Blit(source, destination);
                return;
            }

            // Uber shader setup
            var uberActive = false;
            var fxaaActive = m_Fxaa.active;
            var taaActive = m_Taa.active && !m_RenderingInSceneView;
            var dofActive = m_DepthOfField.active && !m_RenderingInSceneView;

            var uberMaterial = m_MaterialFactory.Get("Hidden/Post FX/Uber Shader");
            uberMaterial.shaderKeywords = null;

            var src = source;
            var dst = destination;

            if (taaActive)
            {
                var tempRT = m_RenderTextureFactory.Get(src);
                m_Taa.Render(src, tempRT);
                src = tempRT;
            }

#if UNITY_EDITOR
            // Render to a dedicated target when monitors are enabled so they can show information
            // about the final render.
            // At runtime the output will always be the backbuffer or whatever render target is
            // currently set on the camera.
            if (profile.monitors.onFrameEndEditorOnly != null)
                dst = m_RenderTextureFactory.Get(src);
#endif

            Texture autoExposure = GraphicsUtils.whiteTexture;
            if (m_EyeAdaptation.active)
            {
                uberActive = true;
                autoExposure = m_EyeAdaptation.Prepare(src, uberMaterial);
            }

            uberMaterial.SetTexture("_AutoExposure", autoExposure);

            if (dofActive)
            {
                uberActive = true;
                m_DepthOfField.Prepare(src, uberMaterial, taaActive, m_Taa.jitterVector,
                    m_Taa.model.settings.taaSettings.motionBlending);
            }

            if (m_Bloom.active)
            {
                uberActive = true;
                m_Bloom.Prepare(src, uberMaterial, autoExposure);
            }

            uberActive |= TryPrepareUberImageEffect(m_ChromaticAberration, uberMaterial);
            uberActive |= TryPrepareUberImageEffect(m_ColorGrading, uberMaterial);
            uberActive |= TryPrepareUberImageEffect(m_Vignette, uberMaterial);
            uberActive |= TryPrepareUberImageEffect(m_UserLut, uberMaterial);

            var fxaaMaterial = fxaaActive
                ? m_MaterialFactory.Get("Hidden/Post FX/FXAA")
                : null;

            if (fxaaActive)
            {
                fxaaMaterial.shaderKeywords = null;
                TryPrepareUberImageEffect(m_Grain, fxaaMaterial);
                TryPrepareUberImageEffect(m_Dithering, fxaaMaterial);

                if (uberActive)
                {
                    var output = m_RenderTextureFactory.Get(src);
                    Graphics.Blit(src, output, uberMaterial, 0);
                    src = output;
                }

                m_Fxaa.Render(src, dst);
            }
            else
            {
                uberActive |= TryPrepareUberImageEffect(m_Grain, uberMaterial);
                uberActive |= TryPrepareUberImageEffect(m_Dithering, uberMaterial);

                if (uberActive)
                {
                    if (!GraphicsUtils.isLinearColorSpace)
                        uberMaterial.EnableKeyword("UNITY_COLORSPACE_GAMMA");

                    Graphics.Blit(src, dst, uberMaterial, 0);
                }
            }

            if (!uberActive && !fxaaActive)
                Graphics.Blit(src, dst);

#if UNITY_EDITOR
            if (profile.monitors.onFrameEndEditorOnly != null)
            {
                Graphics.Blit(dst, destination);

                var oldRt = RenderTexture.active;
                profile.monitors.onFrameEndEditorOnly(dst);
                RenderTexture.active = oldRt;
            }
#endif

            m_RenderTextureFactory.ReleaseAll();
        }

        public void ResetTemporalEffects()
        {
            m_Taa.ResetHistory();
            m_MotionBlur.ResetHistory();
            m_EyeAdaptation.ResetHistory();
        }

        #region State management

        private readonly List<PostProcessingComponentBase> m_ComponentsToEnable =
            new List<PostProcessingComponentBase>();

        private readonly List<PostProcessingComponentBase> m_ComponentsToDisable =
            new List<PostProcessingComponentBase>();

        private void CheckObservers()
        {
            foreach (var cs in m_ComponentStates)
            {
                var component = cs.Key;
                var state = component.GetModel().enabled;

                if (state != cs.Value)
                {
                    if (state) m_ComponentsToEnable.Add(component);
                    else m_ComponentsToDisable.Add(component);
                }
            }

            for (var i = 0; i < m_ComponentsToDisable.Count; i++)
            {
                var c = m_ComponentsToDisable[i];
                m_ComponentStates[c] = false;
                c.OnDisable();
            }

            for (var i = 0; i < m_ComponentsToEnable.Count; i++)
            {
                var c = m_ComponentsToEnable[i];
                m_ComponentStates[c] = true;
                c.OnEnable();
            }

            m_ComponentsToDisable.Clear();
            m_ComponentsToEnable.Clear();
        }

        private void DisableComponents()
        {
            foreach (var component in m_Components)
            {
                var model = component.GetModel();
                if (model != null && model.enabled)
                    component.OnDisable();
            }
        }

        #endregion

        #region Command buffer handling & rendering helpers

        // Placeholders before the upcoming Scriptable Render Loop as command buffers will be
        // executed on the go so we won't need of all that stuff
        private CommandBuffer AddCommandBuffer<T>(CameraEvent evt, string name)
            where T : PostProcessingModel
        {
            var cb = new CommandBuffer {name = name};
            var kvp = new KeyValuePair<CameraEvent, CommandBuffer>(evt, cb);
            m_CommandBuffers.Add(typeof(T), kvp);
            m_Camera.AddCommandBuffer(evt, kvp.Value);
            return kvp.Value;
        }

        private void RemoveCommandBuffer<T>()
            where T : PostProcessingModel
        {
            KeyValuePair<CameraEvent, CommandBuffer> kvp;
            var type = typeof(T);

            if (!m_CommandBuffers.TryGetValue(type, out kvp))
                return;

            m_Camera.RemoveCommandBuffer(kvp.Key, kvp.Value);
            m_CommandBuffers.Remove(type);
            kvp.Value.Dispose();
        }

        private CommandBuffer GetCommandBuffer<T>(CameraEvent evt, string name)
            where T : PostProcessingModel
        {
            CommandBuffer cb;
            KeyValuePair<CameraEvent, CommandBuffer> kvp;

            if (!m_CommandBuffers.TryGetValue(typeof(T), out kvp))
            {
                cb = AddCommandBuffer<T>(evt, name);
            }
            else if (kvp.Key != evt)
            {
                RemoveCommandBuffer<T>();
                cb = AddCommandBuffer<T>(evt, name);
            }
            else
            {
                cb = kvp.Value;
            }

            return cb;
        }

        private void TryExecuteCommandBuffer<T>(PostProcessingComponentCommandBuffer<T> component)
            where T : PostProcessingModel
        {
            if (component.active)
            {
                var cb = GetCommandBuffer<T>(component.GetCameraEvent(), component.GetName());
                cb.Clear();
                component.PopulateCommandBuffer(cb);
            }
            else
            {
                RemoveCommandBuffer<T>();
            }
        }

        private bool TryPrepareUberImageEffect<T>(PostProcessingComponentRenderTexture<T> component, Material material)
            where T : PostProcessingModel
        {
            if (!component.active)
                return false;

            component.Prepare(material);
            return true;
        }

        private T AddComponent<T>(T component)
            where T : PostProcessingComponentBase
        {
            m_Components.Add(component);
            return component;
        }

        #endregion
    }
}