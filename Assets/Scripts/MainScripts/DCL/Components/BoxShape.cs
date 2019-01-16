using System;
using System.Collections;
using System.Collections.Generic;
using DCL.Helpers;
using UnityEngine;

namespace DCL.Components
{
    public class BoxShape : BaseShape
    {
        [System.Serializable]
        public class Model
        {
            public string tag;
            public bool withCollisions;
        }

        Model model = new Model();

        protected override void Awake()
        {
            base.Awake();

            if (meshFilter == null)
            {
                meshFilter = meshGameObject.AddComponent<MeshFilter>();
            }

            if (meshRenderer == null)
            {
                meshRenderer = meshGameObject.AddComponent<MeshRenderer>();
            }

            meshRenderer.sharedMaterial = Resources.Load<Material>("Materials/Default");
        }

        public override IEnumerator ApplyChanges(string newJson)
        {
            model = Helpers.Utils.SafeFromJson<Model>(newJson); // We don't use FromJsonOverwrite() to default the model properties on a partial json.

            if (model == null)
                Debug.Log("box shape Model is null");

            meshFilter.mesh = PrimitiveMeshBuilder.BuildCube(1f);

            ConfigureCollision(model.withCollisions);

            return null;
        }
    }
}
