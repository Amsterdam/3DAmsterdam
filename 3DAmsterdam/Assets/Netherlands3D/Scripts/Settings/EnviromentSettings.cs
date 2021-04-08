using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnviromentSettings : MonoBehaviour
{
    [SerializeField]
    private EnviromentProfile enviromentProfile;

    [SerializeField]
    private EnviromentProfile[] selectableProfiles;

    public static EnviromentSettings Instance;

	public EnviromentProfile[] SelectableProfiles { get => selectableProfiles; private set => selectableProfiles = value; }

	private void Awake()
	{
        Instance = this;
    }

	void Start()
    {
        ApplyEnviromentProfile();
    }

    void ApplyEnviromentProfile()
    {
        
    }
}
