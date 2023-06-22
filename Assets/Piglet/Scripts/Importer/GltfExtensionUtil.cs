using Piglet.GLTF.Schema;

namespace Piglet {

    /// <summary>
    /// Provides methods for checking if particular
    /// glTF extensions (e.g. `KHR_draco_mesh_compression`)
    /// are supported by Piglet.
    /// </summary>
    public static class GltfExtensionUtil
    {
        /// <summary>
        /// <para>
        /// Return true if the `KHR_draco_mesh_compression` extension is
        /// supported, or false otherwise. `KHR_draco_mesh_compression` is supported
        /// when a compatible version of the DracoUnity package has been
        /// installed via the Unity Package Manager.
        /// </para>
        /// <para>
        /// Unity 2021.2+ projects require DracoUnity 4.0.0+ and
        /// Unity 2019.3-2021.1 projects require DracoUnity 1.1.0-3.3.2.
        /// </para>
        /// </summary>
        public static bool IsDracoSupported()
        {
#if UNITY_2021_2_OR_NEWER
    #if DRACO_UNITY_4_0_0_OR_NEWER
            return true;
    #else
            return false;
    #endif
#elif UNITY_2019_3_OR_NEWER
    #if DRACO_UNITY && !DRACO_UNITY_4_0_0_OR_NEWER
            return true;
    #else
            return false;
    #endif
#else
            return false;
#endif
        }

        /// <summary>
        /// <para>
        /// Return true if the `KHR_materials_basisu` extension is
        /// supported, or false otherwise. `KHR_materials_basisu` is supported
        /// when a compatible version of the KtxUnity package has been
        /// installed via the Unity Package Manager.
        /// </para>
        /// <para>
        /// Unity 2021.2+ projects require KtxUnity 2.0.0+ and
        /// Unity 2019.3-2020.1 projects require KtxUnity 0.9.1-1.1.2.
        /// </para>
        /// </summary>
        public static bool IsKtx2Supported()
        {
#if UNITY_2021_2_OR_NEWER
    #if KTX_UNITY_2_0_0_OR_NEWER
            return true;
    #else
            return false;
    #endif
#elif UNITY_2019_3_OR_NEWER
    #if KTX_UNITY_0_9_1_OR_NEWER && !KTX_UNITY_2_0_0_OR_NEWER
            return true;
    #else
            return false;
    #endif
#else
            return false;
#endif
        }

        /// <summary>
        /// <para>
        /// Return true if the given glTF extension is supported
        /// by Piglet, or false otherwise.
        /// </para>
        /// <para>
        /// For a complete list of glTF extensions, see:
        /// https://github.com/KhronosGroup/glTF/tree/main/extensions
        /// </para>
        /// </summary>
        public static bool IsExtensionSupported(string extension)
        {
            switch (extension)
            {
                case "KHR_materials_pbrSpecularGlossiness":
                case "KHR_materials_unlit":
                case "KHR_texture_transform":
                    return true;

                case "KHR_draco_mesh_compression":
                    return IsDracoSupported();

                case "KHR_texture_basisu":
                    return IsKtx2Supported();

                default:
                    return false;
            }
        }

        /// <summary>
        /// Return the `KHR_texture_basisu` glTF extension for
        /// the given texture, if present. Otherwise, return null.
        /// </summary>
        public static KHR_texture_basisuExtension GetKtx2Extension(
            Texture texture)
        {
            Extension extension;
            if (texture.Extensions != null && texture.Extensions.TryGetValue(
                "KHR_texture_basisu", out extension))
            {
                return (KHR_texture_basisuExtension)extension;
            }
            return null;
        }

        /// <summary>
        /// Return the `KHR_materials_unlit` glTF extension for
        /// a material, if present. Otherwise, return null.
        /// </summary>
        public static KHR_materials_unlitExtension
            GetUnlitExtension(Material def)
        {
            Extension extension;
            if (def.Extensions != null && def.Extensions.TryGetValue(
                "KHR_materials_unlit", out extension))
            {
                return (KHR_materials_unlitExtension)extension;
            }
            return null;
        }

        /// <summary>
        /// Return the `KHR_materials_pbrSpecularGlossiness` glTF extension
        /// for a material, if present. Otherwise, return null.
        /// </summary>
        /// <param name="def"></param>
        /// <returns></returns>
        public static KHR_materials_pbrSpecularGlossinessExtension
            GetSpecularGlossinessExtension(Material def)
        {
            Extension extension;
            if (def.Extensions != null && def.Extensions.TryGetValue(
                "KHR_materials_pbrSpecularGlossiness", out extension))
            {
                return (KHR_materials_pbrSpecularGlossinessExtension)extension;
            }
            return null;
        }

        /// <summary>
        /// Return the `KHR_texture_transform` glTF extension for
        /// a texture, if present. Otherwise, return null.
        /// </summary>
        public static KHR_texture_transformExtension
            GetTextureTransformExtension(TextureInfo textureInfo)
        {
            Extension extension;
            if (textureInfo.Extensions != null && textureInfo.Extensions.TryGetValue(
                "KHR_texture_transform", out extension))
            {
                return (KHR_texture_transformExtension)extension;
            }
            return null;
        }

        /// <summary>
        /// Return the `KHR_draco_mesh_compression` glTF extension for
        /// a mesh primitive, if present. Otherwise, return null.
        /// </summary>
        public static KHR_draco_mesh_compressionExtension
            GetDracoExtension(MeshPrimitive meshPrimitive)
        {
            Extension extension;
            if (meshPrimitive.Extensions != null
                && meshPrimitive.Extensions.TryGetValue(
                    "KHR_draco_mesh_compression", out extension))
            {
                return (KHR_draco_mesh_compressionExtension)extension;
            }
            return null;
        }
    }
}