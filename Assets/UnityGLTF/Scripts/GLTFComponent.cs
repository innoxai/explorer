using System;
using System.Collections;
using System.IO;
using GLTF;
using GLTF.Schema;
using UnityEngine;
using UnityGLTF.Loader;

namespace UnityGLTF {

  /// <summary>
  /// Component to load a GLTF scene with
  /// </summary>
  public class GLTFComponent : MonoBehaviour {
    public string GLTFUri = null;
    public bool Multithreaded = true;
    public bool UseStream = false;

    public int MaximumLod = 300;
    public int Timeout = 8;
    public GLTFSceneImporter.ColliderType Collider = GLTFSceneImporter.ColliderType.None;

    [HideInInspector] public bool alreadyLoadedAsset = false;

    [SerializeField] bool loadOnStart = true;
    [SerializeField] Shader shaderOverride = null;

    Coroutine loadingRoutine = null;

    public delegate void GLTFComponentAction(float loadingTime);
    public event GLTFComponentAction OnModelFinishedLoading;

    void Start() {
      if (loadOnStart)
        LoadAsset();
    }

    public void LoadAsset(bool loadEvenIfAlreadyLoaded = false, string gltfURI = "") {

      if (alreadyLoadedAsset && !loadEvenIfAlreadyLoaded) return;

      if (gltfURI != "")
        GLTFUri = gltfURI;

      if (GLTFUri == "") {
        Debug.Log("Couldn't load GLTF since the URI is empty");

        return;
      }

      if (loadingRoutine != null)
        StopCoroutine(loadingRoutine);

      loadingRoutine = StartCoroutine(LoadAssetCoroutine());
    }

    public IEnumerator LoadAssetCoroutine() {
      GLTFSceneImporter sceneImporter = null;
      ILoader loader = null;
      float loadingStartTime = Time.time;
      float loadingFinishTime;

      try {
        if (UseStream) {
          // Path.Combine treats paths that start with the separator character
          // as absolute paths, ignoring the first path passed in. This removes
          // that character to properly handle a filename written with it.
          GLTFUri = GLTFUri.TrimStart(new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar });
          string fullPath = Path.Combine(Application.streamingAssetsPath, GLTFUri);
          string directoryPath = URIHelper.GetDirectoryName(fullPath);
          loader = new FileLoader(directoryPath);
          sceneImporter = new GLTFSceneImporter(
            Path.GetFileName(GLTFUri),
            loader
            );
        } else {
          string directoryPath = URIHelper.GetDirectoryName(GLTFUri);
          loader = new WebRequestLoader(directoryPath);
          sceneImporter = new GLTFSceneImporter(
            URIHelper.GetFileFromUri(new Uri(GLTFUri)),
            loader
            );
        }

        sceneImporter.SceneParent = gameObject.transform;
        sceneImporter.Collider = Collider;
        sceneImporter.MaximumLod = MaximumLod;
        sceneImporter.Timeout = Timeout;
        sceneImporter.isMultithreaded = Multithreaded;
        sceneImporter.CustomShaderName = shaderOverride ? shaderOverride.name : null;

        alreadyLoadedAsset = true;

        yield return sceneImporter.LoadScene(-1);

        // Override the shaders on all materials if a shader is provided
        if (shaderOverride != null) {
          Renderer[] renderers = gameObject.GetComponentsInChildren<Renderer>();
          foreach (Renderer renderer in renderers) {
            renderer.sharedMaterial.shader = shaderOverride;
          }
        }
      } finally {
        if (loader != null) {
            if (sceneImporter == null)
                Debug.Log("sceneImporter is null, could be due to an invalid URI.", this);

            sceneImporter.Dispose();
            sceneImporter = null;

          loader = null;
        }

        loadingFinishTime = Time.time;

        if (OnModelFinishedLoading != null)
          OnModelFinishedLoading(loadingFinishTime - loadingStartTime);

        //Debug.Log("GLTF (" + GLTFUri + ") loading took: " + (loadingFinishTime - loadingStartTime) + " seconds.");
      }

      loadingRoutine = null;
    }
  }
}
