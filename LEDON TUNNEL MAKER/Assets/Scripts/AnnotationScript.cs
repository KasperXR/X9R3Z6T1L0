using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class AnnotationScript : MonoBehaviour
{
    public enum AnnotationType { Information, Warning, Error }

    public class AnnotationData
    {
        public GameObject annotation;

        public TMP_Text annotationText;
        public AnnotationType type;
    }

    public Transform scrollViewContent;
    public Transform scrollViewContent2;
    public Texture infoIcon;
    public Texture disabledinfoIcon;
    public RawImage buttonIcon;
    public GameObject informationPrefab;
    public GameObject warningPrefab;
    public GameObject errorPrefab;
    public GameObject warningPrefab2;
    public GameObject errorPrefab2;
    public GameObject rulesChecker;
    public TMP_Text summaryText;
    public bool rulesCheckerON = false;

    private Dictionary<string, List<AnnotationData>> annotations = new Dictionary<string, List<AnnotationData>>();
    private Dictionary<AnnotationType, List<AnnotationData>> sortedAnnotations = new Dictionary<AnnotationType, List<AnnotationData>>();
    private Dictionary<AnnotationType, int> annotationCounts = new Dictionary<AnnotationType, int>();
    private bool showAnnotations = true;

    public void CreateAnnotation(AnnotationType type, string message, string objectName)
    {
        GameObject annotation;
        GameObject annotation2 = null;
        Transform targetScrollView;
        switch (type)
        {
            case AnnotationType.Information:
                annotation = Instantiate(informationPrefab, scrollViewContent);
                targetScrollView = scrollViewContent;
                break;
            case AnnotationType.Warning:
                annotation = Instantiate(warningPrefab, scrollViewContent);
                annotation2 = Instantiate(warningPrefab2, scrollViewContent2);
                targetScrollView = scrollViewContent;
                break;
            case AnnotationType.Error:
                annotation = Instantiate(errorPrefab, scrollViewContent);
                annotation2 = Instantiate(errorPrefab2, scrollViewContent2);
                targetScrollView = scrollViewContent;
                break;
            default:
                return;
        }

        TMP_Text annotationText = annotation.GetComponentInChildren<TMP_Text>(true);
        annotationText.text = message;
        annotationText.enabled = true;

        AnnotationData annotationData = new AnnotationData
        {
            annotation = annotation,
            annotationText = annotationText,
            type = type
        };
        annotation.SetActive(showAnnotations);

        if (annotation2 != null)
        {
            TMP_Text annotationText2 = annotation2.GetComponentInChildren<TMP_Text>(true);
            annotationText2.text = message;
            annotationText2.enabled = true;

            AnnotationData annotationData2 = new AnnotationData
            {
                annotation = annotation2,
                annotationText = annotationText2,
                type = type
            };
            annotation2.SetActive(showAnnotations);

            if (!annotations.ContainsKey(objectName + "_2"))
            {
                annotations[objectName + "_2"] = new List<AnnotationData>();
            }

            if (!sortedAnnotations.ContainsKey(type))
            {
                sortedAnnotations[type] = new List<AnnotationData>();
            }

            annotations[objectName + "_2"].Add(annotationData2);
            sortedAnnotations[type].Add(annotationData2);
        }

        if (!annotations.ContainsKey(objectName))
        {
            annotations[objectName] = new List<AnnotationData>();
        }

        if (!sortedAnnotations.ContainsKey(type))
        {
            sortedAnnotations[type] = new List<AnnotationData>();
        }

        annotations[objectName].Add(annotationData);
        sortedAnnotations[type].Add(annotationData);

        if (!annotationCounts.ContainsKey(type))
        {
            annotationCounts[type] = 0;
        }

        annotationCounts[type]++;
        UpdateSummaryText();

        OrderAnnotations(targetScrollView);
        OrderAnnotations(scrollViewContent2);
    }


    public void HideAnnotation(string objectName)
    {
        if (annotations.ContainsKey(objectName))
        {
            foreach (var data in annotations[objectName])
            {
                if (sortedAnnotations.ContainsKey(data.type))
                {
                    sortedAnnotations[data.type].Remove(data);
                }

                if (annotationCounts[data.type] > 0)   // Check if count is greater than zero
                {
                    annotationCounts[data.type]--;
                }

                Destroy(data.annotation);
            }
            annotations.Remove(objectName);
        }

        // Check and remove annotation from second scroll view
        string objectName2 = objectName + "_2";
        if (annotations.ContainsKey(objectName2))
        {
            foreach (var data in annotations[objectName2])
            {
                if (sortedAnnotations.ContainsKey(data.type))
                {
                    sortedAnnotations[data.type].Remove(data);
                }

                if (annotationCounts[data.type] > 0)  // Check if count is greater than zero
                {
                    annotationCounts[data.type]--;
                }

                Destroy(data.annotation);
            }
            annotations.Remove(objectName2);
        }

        UpdateSummaryText();
        OrderAnnotations(scrollViewContent);
        OrderAnnotations(scrollViewContent2);
    }


    public void ToggleAnnotations()
    {
        if (rulesCheckerON == false)
        {
            rulesChecker.SetActive(true);
            rulesCheckerON = true;
            buttonIcon.texture = infoIcon;
        }
        else if (rulesCheckerON == true)
        {
            rulesChecker.SetActive(false);
            rulesCheckerON = false;
            buttonIcon.texture = disabledinfoIcon;
        }
    }

    public void UpdateSummaryText()
    {
        int errors = 0;
        int warnings = 0;

        if (annotationCounts.ContainsKey(AnnotationType.Error))
        {
            errors = annotationCounts[AnnotationType.Error];
        }
        if (annotationCounts.ContainsKey(AnnotationType.Warning))
        {
            warnings = annotationCounts[AnnotationType.Warning];
        }

        summaryText.text = $"Errors: {errors} | Warnings: {warnings}";
    }

    private void OrderAnnotations(Transform scrollView)
    {
        // Make all annotations in the specified scroll view inactive.
        foreach (var annotationList in sortedAnnotations.Values)
        {
            foreach (var annotation in annotationList)
            {
                if (annotation.annotation.transform.parent == scrollView)
                {
                    annotation.annotation.SetActive(false);
                }
            }
        }

        // Reorder and reactivate annotations in the specified scroll view based on the order: Errors -> Warnings -> Information.
        foreach (AnnotationType type in new[] { AnnotationType.Error, AnnotationType.Warning, AnnotationType.Information })
        {
            if (sortedAnnotations.ContainsKey(type))
            {
                foreach (var annotation in sortedAnnotations[type])
                {
                    if (annotation.annotation.transform.parent == scrollView)
                    {
                        annotation.annotation.transform.SetAsLastSibling();
                        annotation.annotation.SetActive(true);
                    }
                }
            }
        }
    }
    public void Start()
    {
        CreateAnnotation(AnnotationType.Warning, "This slide configuration assumes a ground/earth setup. For a concrete installation, additional requirements apply. Please let us know beforehand.", "CirclePlane");
        CreateAnnotation(AnnotationType.Information, "Each slide must incorporate a sit-down area of at least 35 cm, either before or after the starting plate. Please ensure your configuration adheres to this safety standard.", "CirclePlane");
        CreateAnnotation(AnnotationType.Information, "A slide must have access to its seating area via a ladder, stairs, a climbing area, or a climbing tool.", "CirclePlane");
        CreateAnnotation(AnnotationType.Information, "Maximum height for standalone slides without direction change or landings: 2,500 mm at narrowest access.", "CirclePlane");
        CreateAnnotation(AnnotationType.Information, "Max height for first set of stairs on standalone slides without direction change or landings: 2,500 mm at narrowest access.", "CirclePlane");
        CreateAnnotation(AnnotationType.Information, "The support area around the slide's exit area should extend at least 1,000 mm.", "CirclePlane");

        

    }
}
