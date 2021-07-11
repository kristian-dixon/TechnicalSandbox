using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LSystem : MonoBehaviour
{
    Stack<System.Tuple<Vector3, float>> stack;

    Dictionary<char, string> rules;
    Dictionary<char, System.Action> operations;

    List<System.Tuple<Vector3, Vector3>> lines;

    string instructions;

    //List<Tuple<Vector3, Vector3>> lines;

    bool jobComplete = true;


    Vector3 cursorPosition = Vector3.zero;
    float angle = 0;

    public int depth = 5;

    public int rngSeed = 0;

    private void OnValidate()
    {
        Init();
    }

    // Start is called before the first frame update
    void Start()
    {
        Init();
    }

    void Init()
    {
        if (!jobComplete) return;
        jobComplete = false;
        stack = new Stack<System.Tuple<Vector3, float>>();

        rules = new Dictionary<char, string>();
        operations = new Dictionary<char, System.Action>();
        lines = new List<System.Tuple<Vector3, Vector3>>();
        cursorPosition = Vector3.zero;
        angle = 0;

        BarnsleyFern();
    }

    void FractalBinaryTree()
    {
        instructions = "0";
        rules.Add('1', "11");
        rules.Add('0', "1[0]0");
        rules.Add('[', "[");
        rules.Add(']', "]");

        operations.Add('1', () =>
        {
            Vector3 startPosition = cursorPosition;
            Vector3 endPosition = cursorPosition + (Quaternion.Euler(0, angle, 0) * (Vector3.forward * 1f/ depth));
            //Draw line
            //Gizmos.DrawLine(startPosition, endPosition);
            lines.Add(System.Tuple.Create(startPosition, endPosition));

            cursorPosition = endPosition;
        });

        operations.Add('0', () =>
        {
            Vector3 startPosition = cursorPosition;
            Vector3 endPosition = cursorPosition + (Quaternion.Euler(0, angle, 0) * (Vector3.forward * 1f / depth));
            //Draw line
            //Gizmos.DrawLine(startPosition, endPosition);
            lines.Add(System.Tuple.Create(startPosition, endPosition));
            //Gizmos.DrawSphere(endPosition, 0.1f);
        });

        operations.Add('[', () => {
            stack.Push(System.Tuple.Create(cursorPosition, angle));
            angle -= 45;
        });

        operations.Add(']', () => {
            var state = stack.Pop();
            cursorPosition = state.Item1;
            angle = state.Item2;
            angle += 45;

        });

        StartCoroutine(GenerateInstructions());
    }

    public float angleChange = 25f;
    public float ignoreRuleChange = 0.5f;
    void BarnsleyFern()
    {
        instructions = "X";
        rules.Add('X', "F+[[X]-X]-F[-FX]+X");
        rules.Add('F', "FF");

        rules.Add('+', "+");
        rules.Add('-', "-");
        rules.Add('[', "[");
        rules.Add(']', "]");

        operations.Add('X', () =>
        {
           
        });
        
        operations.Add('F', () =>
        {
            if (Random.Range(0f, 1f) > ignoreRuleChange) return;

            Vector3 startPosition = cursorPosition;
            Vector3 endPosition = cursorPosition + (Quaternion.Euler(0, angle, 0) * (Vector3.forward * Random.Range(1,5)));

            lines.Add(System.Tuple.Create(startPosition, endPosition));
            cursorPosition = endPosition;
        });

        operations.Add('[', () => {
            stack.Push(System.Tuple.Create(cursorPosition, angle));
        });

        operations.Add(']', () => {
            var state = stack.Pop();
            cursorPosition = state.Item1;
            angle = state.Item2;
        });

        operations.Add('+', () => {
            if (Random.Range(0f, 1f) > ignoreRuleChange) return;

            angle += angleChange;
        });
        
        operations.Add('-', () => {
            if (Random.Range(0f, 1f) > ignoreRuleChange) return;

            angle -= angleChange;
        });

        StartCoroutine(GenerateInstructions());
    }

    IEnumerator GenerateInstructions()
    {
        for (int i = 0; i < depth; i++)
        {
            string output = "";
            for (int j = 0; j < instructions.Length; j++)
            {

                if (rules.TryGetValue(instructions[j], out string res))
                {
                    output += res;
                }
            }

            instructions = output;
            //yield return null;

        }

        lines = new List<System.Tuple<Vector3, Vector3>>();
        for (int i = 0; i < instructions.Length; i++)
        {
            if(i % 10 == 0) yield return null;

            if (operations.TryGetValue(instructions[i], out System.Action res))
            {
                res.Invoke();
            }
        }
        jobComplete = true;
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;
        for(int i = 0; i < lines.Count; i++)
        {
            Gizmos.DrawLine(lines[i].Item1, lines[i].Item2);
        }
        
    }
}
