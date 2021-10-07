using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class URLParameterTriggers : MonoBehaviour
{
    [SerializeField]
    private List<StringEvent> urlParameterEvents = new List<StringEvent>();
  
    void Start()
    {
        ReadURLParameters();        
    }

    public Dictionary<string,string> ReadURLParameters(string customUrl = "")
    {
        Dictionary<string, string> nameAndValueCombination = new Dictionary<string, string>();
        var url = (customUrl != "") ? customUrl : Application.absoluteURL;
        Debug.Log($"Getting parameters from url: {url}");
        var hash = url.Split('#');
        var hasValue = (hash.Length == 2) ? hash[1] : "";

        var parameters = url.Replace("?", "&").Split('&');

        foreach(var parameter in parameters)
        {
            Debug.Log($"Parameter: {parameter}");
            if (parameter.Contains("="))
            {
                var nameAndValue = parameter.Split('=');
                var parameterName = nameAndValue[0];
                var value = (nameAndValue.Length == 2) ? nameAndValue[1] : "";
                Debug.Log($"{parameterName}={value}");
                var trigger = urlParameterEvents.First(parameterEventTrigger => parameterEventTrigger.eventName == parameterName);
                if (trigger)
                {
                    nameAndValueCombination.Add(parameterName, value);
                    trigger.stringEvent?.Invoke(value);
                }
            }
		}

        return nameAndValueCombination;
    }
}
