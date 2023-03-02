using Netherlands3D.Events;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace Netherlands3D.WebGL
{
    public class URLParameterTriggers : MonoBehaviour
    {
        [SerializeField]
        private List<StringEvent> urlParameterEvents = new List<StringEvent>();

        Dictionary<string, string> parameterNameAndValues = new Dictionary<string, string>();

        public Dictionary<string, string> ParameterNameAndValues { get => parameterNameAndValues; private set => parameterNameAndValues = value; }

        void Start()
        {
            ReadURLAndTrigger();
        }

        public void ReadURLAndTrigger()
        {
            ParameterNameAndValues = ReadURLParameters();
            TriggerEventsWithParameterName(ParameterNameAndValues);
        }

        public void TriggerEventsWithParameterName(Dictionary<string, string> parametersAndValues)
        {
            foreach (var paramAndValue in parametersAndValues)
            {
                var trigger = urlParameterEvents.Where(parameterEventTrigger => parameterEventTrigger.name == paramAndValue.Key);
                if (trigger.Any())
                {
                    var targetElement = trigger.First();
                    targetElement.InvokeStarted(paramAndValue.Value);
                }
            }
        }

        public Dictionary<string, string> ReadURLParameters(string customUrl = "")
        {
            Dictionary<string, string> nameAndValueCombination = new Dictionary<string, string>();
            var url = (customUrl != "") ? customUrl : Application.absoluteURL;
            if (url.Contains("#"))
            {
                var splitParametersAndHash = url.Split('#');
                url = splitParametersAndHash[0];
                if (splitParametersAndHash.Length == 2)
                    nameAndValueCombination.Add("#", splitParametersAndHash[1]);
            }

            var parameters = url.Replace("?", "&").Split('&');
            foreach (var parameter in parameters)
            {
                if (parameter.Contains("="))
                {
                    var nameAndValue = parameter.Split('=');
                    var parameterName = nameAndValue[0];
                    var value = (nameAndValue.Length == 2) ? nameAndValue[1] : "";
                    Debug.Log($"{parameterName}={value}");
                    nameAndValueCombination.Add(parameterName, value);
                }
            }

            return nameAndValueCombination;
        }
    }
}