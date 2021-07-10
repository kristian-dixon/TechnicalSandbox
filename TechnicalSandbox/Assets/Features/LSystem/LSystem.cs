using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LSystem : MonoBehaviour
{
    Stack<System.Tuple<Vector3, float>> stack;

    Dictionary<char, string> rules;
    Dictionary<char, System.Action> operations;

    string instructions;

    //List<Tuple<Vector3, Vector3>> lines;
    


    Vector3 cursorPosition = Vector3.zero;
    float angle = 0;

    [Range(0,10)]
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
        stack = new Stack<System.Tuple<Vector3, float>>();

        rules = new Dictionary<char, string>();
        operations = new Dictionary<char, System.Action>();

        RandomisedFractalBinaryTree();
    }

    void FractalBinaryTree()
    {
        //Setup
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
            Gizmos.DrawLine(startPosition, endPosition);
            cursorPosition = endPosition;
        });

        operations.Add('0', () =>
        {
            Vector3 startPosition = cursorPosition;
            Vector3 endPosition = cursorPosition + (Quaternion.Euler(0, angle, 0) * (Vector3.forward * 1f / depth));
            //Draw line
            Gizmos.DrawLine(startPosition, endPosition);
            //Gizmos.DrawSphere(endPosition, 0.1f);
            //cursorPosition = endPosition;
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

        for(int i = 0; i < depth; i++)
        {
            string output = "";
            for(int j = 0; j < instructions.Length; j++)
            {
                if (rules.TryGetValue(instructions[j], out string res))
                {
                    output += res;
                }
            }

            instructions = output;
        }
    }

    void RandomisedFractalBinaryTree()
    {
        //Setup
        instructions = "0";
        rules.Add('1', "11");
        rules.Add('0', "1[0]0");
        rules.Add('[', "[");
        rules.Add(']', "]");

        operations.Add('1', () =>
        {
            Vector3 startPosition = cursorPosition;
            Vector3 endPosition = cursorPosition + (Quaternion.Euler(0, angle, 0) * (Vector3.forward * 1f / depth));
            //Draw line
            Gizmos.DrawLine(startPosition, endPosition);
            cursorPosition = endPosition;
        });

        operations.Add('0', () =>
        {
            Vector3 startPosition = cursorPosition;
            Vector3 endPosition = cursorPosition + (Quaternion.Euler(0, angle, 0) * (Vector3.forward * 1f / depth));
            //Draw line
            Gizmos.DrawLine(startPosition, endPosition);
            //Gizmos.DrawSphere(endPosition, 0.1f);
            //cursorPosition = endPosition;
        });

        operations.Add('[', () => {
            stack.Push(System.Tuple.Create(cursorPosition, angle));
            angle -= Random.Range(10, 90);
        });

        operations.Add(']', () => {
            var state = stack.Pop();
            cursorPosition = state.Item1;
            angle = state.Item2;
            angle += Random.Range(10, 90); ;

        });

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
        }
    }
    private void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;
        Random.InitState(rngSeed);
        cursorPosition = Vector3.zero;
        angle = 0;
        for (int i = 0; i < instructions.Length; i++)
        {
            if (operations.TryGetValue(instructions[i], out System.Action res))
            {
                res.Invoke();
            }
        }
    }
}
