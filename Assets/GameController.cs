using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    public GameObject cubePrefab; // cube prefab
    private GameObject topCube; // cube that moves
    private GameObject botCube; // cube that's right underneath top cube
    private GameObject dropCube; // cube that fall (hangover piece)
    private float offset = 0.1f; // thickness of cube
    const float MOVE_SPEED = 1.5f; // normal speed
    private float speed = MOVE_SPEED;
    private bool gameOver = false;
    private bool toggle = true; // z-move or x-move
    private Color c1, c2; // two palette colors
    private float s = 0; // lerp (linear interpolation) parameter ranging in [0, 1]
    private float lerpDir = 1f; // lerp parameter moving direction {-1, 1}
    public Text winText;
    public Text scoreText;
    private int score = 0;
            
    // Start is called before the first frame update
    void Start()
    {
        winText.text = ""; // initially, empty string
        scoreText.text = score.ToString(); // display current score

        c1 = new Color(39 / 255f, 90 / 255f, 43 / 255f); // dark green
        c2 = new Color(250 / 255f, 250 / 255f, 76 / 255f); // bright yellow

        // create bottom cubes
        Transform t = cubePrefab.transform;
        t.localScale = new Vector3(1f, 0.1f, 1f);
        t.position = new Vector3(0, -0.1f, 0); // bottom cube position

        // create 10 cubes underneath bottom cube
        for (int i = 10; i >= 1; i--)
        {
            // create cube, then push down
            GameObject cube = Instantiate(cubePrefab);
            cube.transform.Translate(new Vector3(0, -offset * i, 0));
            SetCubeColor(cube, true); // also update s
        }
        // create bottom cube
        botCube = Instantiate(cubePrefab);
        SetCubeColor(botCube, true);

        // create top cube
        t.position = new Vector3(0, 0, -2f);
        topCube = Instantiate(cubePrefab);
        SetCubeColor(topCube, false);
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 dir = toggle ? new Vector3(0, 0, 1f) : new Vector3(1f, 0, 0); // z-move or x-move

        Transform bot = botCube.transform;
        Transform top = null;
        float hangover = 0f; // displacement between top cube and bottom cube 
        
        if (topCube) // if topCube still exists (game not over yet)
        {
            top = topCube.transform;
            hangover = toggle ? top.position.z - bot.position.z : top.position.x - bot.position.x; 
            if (hangover > 1.2f && speed > 0) speed *= -1f; // flip moving direction
            if (hangover < -1.2f && speed < 0) speed *= -1f; // flip moving direction

            top.Translate(dir * Time.deltaTime * speed); // move top cube
        }

        if (Input.GetButtonDown("Jump")) // space bar pressed
        {
            speed = 0; // stop moving top cube

            float sign = hangover >= 0 ? 1f : -1f; // 1f: front hangover; -1f: rear hangover 

            gameOver = toggle ? Mathf.Abs(hangover) > bot.localScale.z : Mathf.Abs(hangover) > bot.localScale.x;

            if (gameOver)
            {
                topCube.AddComponent<Rigidbody>(); // make topCube drop
                winText.text = "Game Over";
            }
            else
            {
                ////////////////////////////////////////////////////////////////////////////////////
                /// cut top cube
                top.localScale -= dir * Mathf.Abs(hangover); // chop hangover piece off
                if (toggle) top.Translate(dir * (bot.position.z + hangover * 0.5f - top.position.z)); // update position of top cube
                else top.Translate(dir * (bot.position.x + hangover * 0.5f - top.position.x)); // update position of top cube

                ////////////////////////////////////////////////////////////////////////////////////////
                /// create drop cube
                dropCube = Instantiate(cubePrefab, top.position, top.rotation);
                SetCubeColor(dropCube, true);
                Transform drop = dropCube.transform;
                if (toggle) drop.localScale = new Vector3(drop.localScale.x, drop.localScale.y, Mathf.Abs(hangover));
                else drop.localScale = new Vector3(Mathf.Abs(hangover), drop.localScale.y, drop.localScale.z);
                if (toggle) drop.Translate(dir * (bot.position.z + sign * bot.localScale.z * 0.5f + hangover * 0.5f - drop.position.z));
                else drop.Translate(dir * (bot.position.x + sign * bot.localScale.x * 0.5f + hangover * 0.5f - drop.position.x));
                dropCube.AddComponent<Rigidbody>(); // make it drop

                ////////////////////////////////////////////////////////////////////////////////////////
                /// update bottom cube
                botCube = topCube; // top cube becomes new bottom cube   
                toggle = !toggle; // flip between z-move and x-move

                ///////////////////////////////////////////////////////////////////////////////////////
                /// push bottom cubes down
                GameObject[] cubes = GameObject.FindGameObjectsWithTag("Cube");
                foreach (GameObject cube in cubes) cube.transform.Translate(new Vector3(0, -offset, 0));

                ////////////////////////////////////////////////////////////////////////////////////////
                /// create new top cube
                Transform t = cubePrefab.transform;
                t.position = botCube.transform.position; // copy position of bottom cube
                if (toggle) t.position += new Vector3(0, 0.1f, -2f); // move it up and away from bottom cube                
                else t.position += new Vector3(-2f, 0.1f, 0); // move it up and away from bottom cube                
                t.localScale = botCube.transform.localScale; // copy scale of bottom cube
                topCube = Instantiate(cubePrefab);
                SetCubeColor(topCube, false);

                speed = MOVE_SPEED; // restore moving speed

                score++; // increment score by 1
                scoreText.text = score.ToString(); // update score on screen

                AudioSource sound = GetComponent<AudioSource>();
                sound.Play(); // play chopping.mp3
            }
        }
    }

    private void UpdateLerpParam()
    {
        s += 0.1f * lerpDir; // either moving upward or downward
        if (s >= 1f)
        {
            s = 1f;
            lerpDir *= -1f; // flip direction
        }
        else if (s <= 0)
        {
            s = 0;
            lerpDir *= -1f; // flip direction
        }
    }  
    
    private void SetCubeColor(GameObject o, bool isUpdateLerpParam)
    {
        Renderer renderer = o.GetComponent<Renderer>();
        renderer.material.SetColor("_Color", Color.Lerp(c1, c2, s));
        if (isUpdateLerpParam) UpdateLerpParam();
    }
}
