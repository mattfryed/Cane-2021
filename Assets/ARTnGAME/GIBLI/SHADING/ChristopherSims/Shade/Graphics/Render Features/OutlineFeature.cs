using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
namespace Artngame.GIBLI
{
    public class OutlineFeature : ScriptableRendererFeature
    {
        class OutlinePass : ScriptableRenderPass
        {
            private RenderTargetIdentifier source { get; set; }

#if UNITY_2022_1_OR_NEWER
            private RTHandle destination { get; set; } //v0.1
#else
            private RenderTargetHandle destination { get; set; } //v0.1
#endif

            public Material outlineMaterial = null;

#if UNITY_2022_1_OR_NEWER
            public void Setup(RenderTargetIdentifier source, RTHandle destination)//v0.1
            {
                this.source = source;
                this.destination = destination;
                //temporaryColorTexture = RTHandles.Alloc("temporaryColorTexture", name: "temporaryColorTexture"); //v0.1
            }
#else
            RenderTargetHandle temporaryColorTexture; //v0.1
            public void Setup(RenderTargetIdentifier source, RenderTargetHandle destination)//v0.1
            {
                this.source = source;
                this.destination = destination;
            }
#endif

            public OutlinePass(Material outlineMaterial)
            {
                this.outlineMaterial = outlineMaterial;
            }

            //v1.5
#if UNITY_2020_2_OR_NEWER
            public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
            {
                // get a copy of the current camera’s RenderTextureDescriptor
                // this descriptor contains all the information you need to create a new texture
                //RenderTextureDescriptor cameraTextureDescriptor = renderingData.cameraData.cameraTargetDescriptor;

                // _handle = RTHandles.Alloc(settings.textureId, name: settings.textureId); //v0.1

                var renderer = renderingData.cameraData.renderer;
#if UNITY_2022_1_OR_NEWER
                destination = renderingData.cameraData.renderer.cameraColorTargetHandle; //UnityEngine.Rendering.Universal.RenderTargetHandle.CameraTarget //v0.1                          
                source = renderingData.cameraData.renderer.cameraColorTargetHandle; 
#else
                destination = UnityEngine.Rendering.Universal.RenderTargetHandle.CameraTarget; //v0.1                          
                source = renderingData.cameraData.renderer.cameraColorTarget;
#endif

            }
#endif

            // This method is called before executing the render pass.
            // It can be used to configure render targets and their clear state. Also to create temporary render target textures.
            // When empty this render pass will render to the active camera render target.
            // You should never call CommandBuffer.SetRenderTarget. Instead call <c>ConfigureTarget</c> and <c>ConfigureClear</c>.
            // The render pipeline will ensure target setup and clearing happens in an performance manner.
            //public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
            //{

            // }

            // Here you can implement the rendering logic.
            // Use <c>ScriptableRenderContext</c> to issue drawing commands or execute command buffers
            // https://docs.unity3d.com/ScriptReference/Rendering.ScriptableRenderContext.html
            // You don't have to call ScriptableRenderContext.submit, the render pipeline will call it at specific points in the pipeline.
            public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
            {
                CommandBuffer cmd = CommandBufferPool.Get("Outline Pass");

                RenderTextureDescriptor opaqueDescriptor = renderingData.cameraData.cameraTargetDescriptor;
                opaqueDescriptor.depthBufferBits = 0;

                //v1.6
                if (Camera.main != null && renderingData.cameraData.camera == Camera.main)
                {
                    //if (destination == renderingData.cameraData.renderer.cameraColorTargetHandle)//RenderTargetHandle.CameraTarget) //v0.1
                    //{

                    //temporaryColorTexture = RTHandles.Alloc("temporaryColorTexture", name: "temporaryColorTexture"); //v0.1

                    //cmd.GetTemporaryRT(Shader.PropertyToID(temporaryColorTexture.name), opaqueDescriptor, FilterMode.Point); //v0.1
                    //cmd.Blit( source, temporaryColorTexture, outlineMaterial, 0); //v0.1
                    //cmd.Blit( temporaryColorTexture, destination); //v0.1

#if UNITY_2022_1_OR_NEWER
                    cmd.Blit(source, destination, outlineMaterial, 0); //v0.1
#else
                    cmd.GetTemporaryRT(temporaryColorTexture.id, opaqueDescriptor, FilterMode.Point);
                    Blit(cmd, source, temporaryColorTexture.Identifier(), outlineMaterial, 0);
                    Blit(cmd, temporaryColorTexture.Identifier(), source);
#endif

                    //}
                    //else cmd.Blit( source, destination, outlineMaterial, 0); //v0.1

                    //cmd.ReleaseTemporaryRT(Shader.PropertyToID(temporaryColorTexture.name));//v0.1
                    context.ExecuteCommandBuffer(cmd);
                    CommandBufferPool.Release(cmd);
                }


            }

            /// Cleanup any allocated resources that were created during the execution of this render pass.
            public override void FrameCleanup(CommandBuffer cmd)
            {
#if UNITY_2022_1_OR_NEWER
                
#else
                if (destination == RenderTargetHandle.CameraTarget)
                { //v0.1
                    cmd.ReleaseTemporaryRT(temporaryColorTexture.id);
                }
#endif
            }
        }

        [System.Serializable]
        public class OutlineSettings
        {
            public Material outlineMaterial = null;
        }

        public OutlineSettings settings = new OutlineSettings();
        OutlinePass outlinePass;

#if UNITY_2022_1_OR_NEWER
        RTHandle outlineTexture; //v0.1
#else
        RenderTargetHandle outlineTexture; //v0.1
#endif

        [SerializeField] private RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingOpaques;//v1.6a
        public override void Create()
        {
            outlinePass = new OutlinePass(settings.outlineMaterial);
            outlinePass.renderPassEvent = renderPassEvent; //RenderPassEvent.AfterRenderingTransparents;//v1.6a

            //
#if UNITY_2022_1_OR_NEWER
            outlineTexture = RTHandles.Alloc("_OutlineTexture", name: "_OutlineTexture"); //v0.1
#else
            outlineTexture.Init("_OutlineTexture"); //v0.1
#endif

        }

        // Here you can inject one or multiple render passes in the renderer.
        // This method is called when setting up the renderer once per-camera.
        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (settings.outlineMaterial == null)
            {
                Debug.LogWarningFormat("Missing Outline Material");
                return;
            }
            //outlinePass.Setup(renderer.cameraColorTarget, RenderTargetHandle.CameraTarget);//v1.5
            renderer.EnqueuePass(outlinePass);
        }
    }


}
