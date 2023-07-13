using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

// Cartoon FX  - (c) 2015 Jean Moreno

// Script handling the Demo scene of CartoonFX particles.

public class CFX_Demo : MonoBehaviour
{
    public bool orderedSpawns = true;
    public float step = 1.0f;
    public float range = 5.0f;

    public Material groundMat, waterMat;
    public GameObject[] ParticleExamples;

    private int exampleIndex;
    private float order = -5.0f;

    private readonly Dictionary<string, float> ParticlesYOffsetD = new()
    {
        { "CFX_ElectricGround", 0.15f },
        { "CFX_ElectricityBall", 1.0f },
        { "CFX_ElectricityBolt", 1.0f },
        { "CFX_Explosion", 2.0f },
        { "CFX_SmallExplosion", 1.5f },
        { "CFX_SmokeExplosion", 2.5f },
        { "CFX_Flame", 1.0f },
        { "CFX_DoubleFlame", 1.0f },
        { "CFX_Hit", 1.0f },
        { "CFX_CircularLightWall", 0.05f },
        { "CFX_LightWall", 0.05f },
        { "CFX_Flash", 2.0f },
        { "CFX_Poof", 1.5f },
        { "CFX_Virus", 1.0f },
        { "CFX_SmokePuffs", 2.0f },
        { "CFX_Slash", 1.0f },
        { "CFX_Splash", 0.05f },
        { "CFX_Fountain", 0.05f },
        { "CFX_Ripple", 0.05f },
        { "CFX_Magic", 2.0f },
        { "CFX_SoftStar", 1.0f },
        { "CFX_SpikyAura_Sphere", 1.0f },
        { "CFX_Firework", 2.4f },
        { "CFX_GroundA", 0.05f }
    };

    private bool randomSpawns;
    private string randomSpawnsDelay = "0.5";

    private bool slowMo;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow))
            prevParticle();
        else if (Input.GetKeyDown(KeyCode.RightArrow)) nextParticle();
    }

    private void OnGUI()
    {
        GUILayout.BeginArea(new Rect(5, 20, Screen.width - 10, 30));
        GUILayout.BeginHorizontal();

        GUILayout.Label("Effect", GUILayout.Width(50));
        if (GUILayout.Button("<", GUILayout.Width(20))) prevParticle();
        GUILayout.Label(ParticleExamples[exampleIndex].name, GUILayout.Width(190));
        if (GUILayout.Button(">", GUILayout.Width(20))) nextParticle();

        GUILayout.Label("Click on the ground to spawn selected particles");

        if (GUILayout.Button(CFX_Demo_RotateCamera.rotating ? "Pause Camera" : "Rotate Camera", GUILayout.Width(140)))
            CFX_Demo_RotateCamera.rotating = !CFX_Demo_RotateCamera.rotating;


        if (GUILayout.Button(randomSpawns ? "Stop Random Spawns" : "Start Random Spawns", GUILayout.Width(140)))
        {
            randomSpawns = !randomSpawns;
            if (randomSpawns) StartCoroutine("RandomSpawnsCoroutine");
            else StopCoroutine("RandomSpawnsCoroutine");
        }

        randomSpawnsDelay = GUILayout.TextField(randomSpawnsDelay, 10, GUILayout.Width(42));
        randomSpawnsDelay = Regex.Replace(randomSpawnsDelay, @"[^0-9.]", "");

        if (GUILayout.Button(GetComponent<Renderer>().enabled ? "Hide Ground" : "Show Ground", GUILayout.Width(90)))
            GetComponent<Renderer>().enabled = !GetComponent<Renderer>().enabled;

        if (GUILayout.Button(slowMo ? "Normal Speed" : "Slow Motion", GUILayout.Width(100)))
        {
            slowMo = !slowMo;
            if (slowMo) Time.timeScale = 0.33f;
            else Time.timeScale = 1.0f;
        }

        GUILayout.EndHorizontal();
        GUILayout.EndArea();
    }

    private void OnMouseDown()
    {
        var hit = new RaycastHit();
        if (GetComponent<Collider>().Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 9999f))
        {
            var particle = spawnParticle();
            particle.transform.position = hit.point + particle.transform.position;
        }
    }

    private GameObject spawnParticle()
    {
        var particles = Instantiate(ParticleExamples[exampleIndex]);

#if UNITY_3_5
			particles.SetActiveRecursively(true);
#else
        particles.SetActive(true);
        for (var i = 0; i < particles.transform.childCount; i++)
            particles.transform.GetChild(i).gameObject.SetActive(true);
#endif

        var Y = 0.0f;
        foreach (var kvp in ParticlesYOffsetD)
            if (particles.name.StartsWith(kvp.Key))
            {
                Y = kvp.Value;
                break;
            }

        particles.transform.position = new Vector3(0, Y, 0);

        return particles;
    }

    private IEnumerator RandomSpawnsCoroutine()
    {
        LOOP:
        var particles = spawnParticle();

        if (orderedSpawns)
        {
            particles.transform.position = transform.position + new Vector3(order, particles.transform.position.y, 0);
            order -= step;
            if (order < -range) order = range;
        }
        else
        {
            particles.transform.position = transform.position +
                                           new Vector3(Random.Range(-range, range), 0, Random.Range(-range, range)) +
                                           new Vector3(0, particles.transform.position.y, 0);
        }

        yield return new WaitForSeconds(float.Parse(randomSpawnsDelay));

        goto LOOP;
    }

    private void prevParticle()
    {
        exampleIndex--;
        if (exampleIndex < 0) exampleIndex = ParticleExamples.Length - 1;

        if (ParticleExamples[exampleIndex].name.Contains("Splash") ||
            ParticleExamples[exampleIndex].name == "CFX_Ripple" ||
            ParticleExamples[exampleIndex].name == "CFX_Fountain")
            GetComponent<Renderer>().material = waterMat;
        else
            GetComponent<Renderer>().material = groundMat;
    }

    private void nextParticle()
    {
        exampleIndex++;
        if (exampleIndex >= ParticleExamples.Length) exampleIndex = 0;

        if (ParticleExamples[exampleIndex].name.Contains("Splash") ||
            ParticleExamples[exampleIndex].name == "CFX_Ripple" ||
            ParticleExamples[exampleIndex].name == "CFX_Fountain")
            GetComponent<Renderer>().material = waterMat;
        else
            GetComponent<Renderer>().material = groundMat;
    }
}