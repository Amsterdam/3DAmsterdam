using ExLumina.SketchUp.API;
using UnityEngine;
using UnityEngine.Analytics;

namespace ExLumina.Examples.SketchUp.API
{
    class RedQuad : MonoBehaviour
    {
        SU.Point3D[] points =
        {
            new SU.Point3D(2, 0, -2),
            new SU.Point3D(2, 0, 2),
            new SU.Point3D(-2, 0, 2),
            new SU.Point3D(-2, 0, -2)
        };

    
        public void Start()
        {
            SU.EntitiesRef entities = SUHelper.Initialize();

            SU.GeometryInputRef geometry = new SU.GeometryInputRef();
            SU.GeometryInputCreate(geometry);

            foreach (SU.Point3D p in points)
            {
                SU.Point3D pc = p;

                pc.x *= SU.MetersToInches;
                pc.y *= SU.MetersToInches;
                pc.z *= SU.MetersToInches;

                SU.GeometryInputAddVertex(geometry, pc);
            }

            SU.LoopInputRef loop = new SU.LoopInputRef();
            SU.LoopInputCreate(loop);

            SU.LoopInputAddVertexIndex(loop, 0);
            SU.LoopInputAddVertexIndex(loop, 1);
            SU.LoopInputAddVertexIndex(loop, 2);
            SU.LoopInputAddVertexIndex(loop, 3);

            long faceIndex;

            SU.GeometryInputAddFace(geometry, loop, out faceIndex);

            SU.MaterialRef material = new SU.MaterialRef();
            SU.MaterialCreate(material);
            SU.MaterialSetName(material, "Pure Red");
            SU.MaterialSetColor(material, new SU.Color { red = 0xFF, alpha = 0xFF });
            SU.MaterialInput materialInput = new SU.MaterialInput();
            materialInput.materialRef = material;
            SU.GeometryInputFaceSetFrontMaterial(geometry, 0, materialInput);

            SU.EntitiesFill(entities, geometry, true);

            SUHelper.Finalize(Application.dataPath + "testRedQuad.skp");
        }
    }

    public static class SUHelper
    {
        static SU.ModelRef model;

        public static SU.EntitiesRef Initialize()
        {
            SU.Initialize();
            model = new SU.ModelRef();
            SU.ModelCreate(model);
            SU.EntitiesRef entities = new SU.EntitiesRef();
            SU.ModelGetEntities(model, entities);

            return entities;
        }

        public static void Finalize(string path)
        {
            SU.StylesRef styles = new SU.StylesRef();
            SU.ModelGetStyles(model, styles);
            //SU.StylesAddStyle(styles, "base.style", true);

            SU.CameraRef camera = new SU.CameraRef();

            SU.ModelGetCamera(model, camera);

            SU.CameraSetOrientation(
                camera,
                new SU.Point3D(10 * SU.MetersToInches, -10 * SU.MetersToInches, 10 * SU.MetersToInches),
                new SU.Point3D(0, 0, 0),
                new SU.Vector3D(0, 0, 1));

            //SU.model
            //SU.ModelSaveToFileWithVersion(model, path, SU.ModelVersion_SU2017);
            SU.ModelRelease(model);
            SU.Terminate();
        }

        internal static void ModelAddComponentDefinitions(SU.ComponentDefinitionRef[] defs)
        {
            SU.ModelAddComponentDefinitions(model, defs.Length, defs);
        }
    }
}