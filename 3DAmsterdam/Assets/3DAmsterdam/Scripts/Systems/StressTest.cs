using Netherlands3D.Events;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class StressTest : MonoBehaviour
{
    [SerializeField]
    StringEvent startStressTestEvent;

    [SerializeField]
    Vector3Event drawParticleEvent;

    [SerializeField]
    private float spawnRadius = 5000;

    void Awake()
    {
        startStressTestEvent.AddListenerStarted(SpawnParticles);
    }

	private void SpawnParticles(string amount)
	{
        int amountOfParticles = int.Parse(amount);

		for (int i = 0; i < amountOfParticles; i++)
		{
            var randomPosition = new Vector3(
                Mathf.Lerp(-spawnRadius, spawnRadius, Random.value), 
                0,
                Mathf.Lerp(-spawnRadius, spawnRadius, Random.value)
            );
            drawParticleEvent.InvokeStarted(randomPosition);
        }
    }
}
