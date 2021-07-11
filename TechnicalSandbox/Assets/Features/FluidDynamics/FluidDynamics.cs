using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FluidDynamics : MonoBehaviour
{
    public int gridScale = 10;

    //Velocity arrays (stored seperately for reuse of functions)
    float[,] u; float[,] uPrev;
    float[,] v; float[,] vPrev;

    //Density
    float[,] dens; float[,] densPrev;

    public float diff = 0.5f;
    public float visc = 0.5f;

    Texture2D tex;

    public UnityEngine.UI.RawImage display;

    void Start()
    {
        tex = new Texture2D(gridScale, gridScale);
        display.texture = tex;
        u = new float[gridScale + 2, gridScale + 2];
        v = new float[gridScale + 2, gridScale + 2];
        uPrev = new float[gridScale + 2, gridScale + 2];
        vPrev = new float[gridScale + 2, gridScale + 2];
        dens = new float[gridScale + 2, gridScale + 2];
        densPrev = new float[gridScale + 2, gridScale + 2];

        for(int i = 0; i < gridScale + 2; i++)
        {
            for (int j = 0; j < gridScale + 2; j++)
            {
                u[i, j] = Random.Range(-100f, 100f);
                v[i, j] = Random.Range(-100f, 100f);

                uPrev[i, j] = Random.Range(-100f, 100f);
                vPrev[i, j] = Random.Range(-100f, 100f);
                dens[i, j] = 0;// Random.Range(0f, 1f);
                densPrev[i, j] = 0;// Random.Range(0f, 1f);
            }
        }
    }

    void Update()
    {
        for (int i = 1; i <= gridScale; i++)
        {
            for (int j = 1; j <= gridScale; j++)
            {
                densPrev[i, j] = 0.0f;
            }
        }

        if (Input.GetKey(KeyCode.Space))
        {
            for(int i = -5; i < 5; i++)
            {
                for (int j = -5; j < 5; j++)
                { 
                    densPrev[gridScale / 2 + i, gridScale / 2 + j] = 1;
                }
            }
        }

        if (Input.GetKey(KeyCode.Backspace))
        {
            for (int i = -5; i < 5; i++)
            {
                for (int j = -5; j < 5; j++)
                {
                    densPrev[gridScale / 2 + i, gridScale / 2 + j] = -1;
                }
            }
        }

        float h = Input.GetAxis("Horizontal");
        if (h != 0)
        {
            for (int i = -10; i < 10; i++)
            {
                for (int j = -10; j < 10; j++)
                {
                    uPrev[gridScale / 2 + i, gridScale / 2 + j] = h * 10;
                }
            }
        }

        float vI = Input.GetAxis("Vertical");
        if (vI != 0)
        {
            for (int i = -10; i < 10; i++)
            {
                for (int j = -10; j < 10; j++)
                {
                    vPrev[gridScale / 2 + i, gridScale / 2 + j] = vI * 10;
                }
            }
        }


        UpdateVelocity(u, v, uPrev, vPrev, visc);
        UpdateDensity(dens, densPrev, u, v, diff);

        for(int i = 0; i < tex.width; i++)
        {
            for(int j = 0; j < tex.height; j++)
            {
                tex.SetPixel(i,j, Color.black + new Color(0, 0, dens[i, j], 1));
            }
        }
        tex.Apply();
    }

    void UpdateDensity(float[,] x, float[,] x0, float[,] u, float[,] v, float diff)
    {
        AddSources(x, x0);
        swap(ref x0, ref x); Diffuse(x, x0, diff, 0);
        swap(ref x0, ref x); Advect(0, x, x0, u, v);
    }

    void UpdateVelocity(float[,] u, float[,] v, float[,] u0, float[,] v0, float visc)
    {

        AddSources(u, u0); AddSources(v, v0);
        swap(ref u0, ref u); Diffuse(u, u0, visc, 1);
        swap(ref v0, ref v); Diffuse(v, v0, visc, 2);
        Project(u, v, u0, v0);
        swap(ref u0, ref u); swap(ref v0,ref v);
        Advect(1, u, u0, u0, v0); Advect(2, v, v0, u0, v0);
        Project(u, v, u0, v0);

    }

    void AddSources(float[,] x, float[,] s)
    {
        //TODO:: Support multiple sources
        for (int i = 1; i < gridScale + 2; i++)
        {
            for (int j = 1; j < gridScale + 2; j++)
            {
                x[i,j] += Time.deltaTime * s[i,j];
            }
        }
    }

    void Diffuse(float[,] x, float[,] x0, float diff, int b)
    {
        float a = Time.deltaTime * diff * gridScale * gridScale;

        for (int k = 0; k < 20; k++)
        {
            for (int i = 1; i <= gridScale; i++)
            {
                for (int j = 1; j <= gridScale; j++)
                {
                    x[i, j] = (x0[i, j] + a * (x[i - 1, j] + x[i + 1, j] +
                               x[i, j - 1] + x[i, j + 1])) / (1 + 4 * a);
                }
            }
            set_bnd(gridScale, b, x);
        }
    }

    void Advect(int b, float[,] d, float[,] d0, float[,] u, float[,] v)
    {
        int i, j, i0, j0, i1, j1;
        float x, y, s0, t0, s1, t1, dt0;
        dt0 = Time.deltaTime * gridScale;
        for (i = 1; i <= gridScale; i++)
        {
            for (j = 1; j <= gridScale; j++)
            {
                x = i - dt0 * u[i, j]; y = j - dt0 * v[i, j];
                if (x < 0.5) x = 0.5f; if (x > gridScale + 0.5) x = gridScale + 0.5f; i0 = (int)x; i1 = i0 + 1;
                if (y < 0.5) y = 0.5f; if (y > gridScale + 0.5) y = gridScale + 0.5f; j0 = (int)y; j1 = j0 + 1;
                s1 = x - i0; s0 = 1 - s1; t1 = y - j0; t0 = 1 - t1;
                d[i, j] = s0 * (t0 * d0[i0, j0] + t1 * d0[i0, j1])+
                            s1 * (t0 * d0[i1, j0] + t1 * d0[i1, j1]);
            }
        }
        set_bnd(gridScale, b, d);
    }

    void Project(float[,] u, float[,] v, float[,] p, float[,] div)
    {
        int i, j, k;
        float h;
        h = 1.0f / gridScale;
        for (i = 1; i <= gridScale; i++)
        {
            for (j = 1; j <= gridScale; j++)
            {
                div[i, j] = -0.5f * h * (u[i + 1, j] - u[i - 1, j] +
                v[i, j + 1] - v[i, j - 1]);
                p[i, j] = 0;
            }
        }
        set_bnd(gridScale, 0, div); set_bnd(gridScale, 0, p);
        for (k = 0; k < 20; k++)
        {
            for (i = 1; i <= gridScale; i++)
            {
                for (j = 1; j <= gridScale; j++)
                {
                    p[i, j] = (div[i, j] + p[i - 1, j] + p[i + 1, j] +
                     p[i, j - 1] + p[i, j + 1]) / 4f;
                }
            }
            set_bnd(gridScale, 0, p);
        }
        for (i = 1; i <= gridScale; i++)
        {
            for (j = 1; j <= gridScale; j++)
            {
                u[i, j] -= 0.5f * (p[i + 1, j] - p[i - 1, j]) / h;
                v[i, j] -= 0.5f * (p[i, j + 1] - p[i, j - 1]) / h;
            }
        }
        set_bnd(gridScale, 1, u); set_bnd(gridScale, 2, v);
    }


    void set_bnd(int N, int b, float[,] x )
    {
        int i;

        for (i = 1; i <= N; i++)
        {
            x[0, i] = b == 1 ? -x[1, i] : x[1, i];
            x[N + 1, i] = b == 1 ? -x[N, i] : x[N, i];
            x[i, 0] = b == 2 ? -x[i, 1] : x[i, 1];
            x[i, N + 1] = b == 2 ? -x[i, N] : x[i, N];
        }

        x[0, 0] = 0.5f * (x[1, 0] + x[0, 1]);
        x[0, N + 1] = 0.5f * (x[1, N + 1] + x[0, N]);
        x[N + 1, 0] = 0.5f * (x[N, 0] + x[N + 1, 1]);
        x[N + 1, N + 1] = 0.5f * (x[N, N + 1] + x[N + 1, N]);
    }

    void swap(ref float[,] x0, ref float[,] x)
    {
        var temp = x0;
        x0 = x;
        x = temp;
    }

    private void OnDrawGizmos()
    {
        return;
        if (Application.isPlaying)
        {

            for(int i = 0; i < gridScale; i++)
            {
                for (int j = 0; j < gridScale; j++)
                {
                    Gizmos.color = Color.black + new Color(/*(u[i, j]), (v[i, j])*/0,0, dens[i,j],1);
                    Gizmos.DrawSphere(new Vector2(i, j), 0.5f);
                }
            }
        }
    }
}
