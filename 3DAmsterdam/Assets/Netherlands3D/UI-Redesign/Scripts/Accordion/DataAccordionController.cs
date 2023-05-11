using Netherlands3D.Events;
using Netherlands3D.Interface.Layers;
using Netherlands3D.JavascriptConnection;
using Netherlands3D.TileSystem;
using UnityEngine;

public class DataAccordionController : MonoBehaviour
{
    private DefaultAccordionController _controller;

    [SerializeField]
    private bool generateChildrenWithLinkedObject = false;

    [SerializeField]
    private GameObject accordionChildPrefab;

    // Start is called before the first frame update
    void Start()
    {
        _controller = GetComponent<DefaultAccordionController>();

        //If yes: parent to generated accordions
        if (generateChildrenWithLinkedObject & _controller.LinkedObject)
        {
            foreach (var material in _controller.LinkedObject.GetComponent<BinaryMeshLayer>().DefaultMaterialList)
            {
                var generatedAccordion = Instantiate(accordionChildPrefab, _controller.AccordionChildrenGroup.transform);

                var defaultController = generatedAccordion.GetComponent<DefaultAccordionController>();
                defaultController.Title = material.name;
                defaultController.ActivateCheckMark = false;

                var dataController = generatedAccordion.GetComponent<DataChildAccordionController>();
                dataController.LegendColorMaterial = material;
            }
        }

    }
}
