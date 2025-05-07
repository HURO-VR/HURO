using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Represents an obstacle in the game world.
/// </summary>
public class Obstacle : Circle, Entity
{
    #region Public Properties
    /// <summary>
    /// List of circles that approximate the obstacle's shape.
    /// </summary>
    public List<Circle> circleAbstraction { get; private set; }

    /// <summary>
    /// Width of the obstacle.
    /// </summary>
    public float width { get; private set; }

    /// <summary>
    /// Length of the obstacle.
    /// </summary>
    public float length { get; private set; }

    /// <summary>
    /// Whether the obstacle is dynamic (i.e., can move).
    /// </summary>
    public bool isDynamic { get; private set; }

    /// <summary>
    /// The GameObject associated with this obstacle.
    /// </summary>
    public GameObject go { get; private set; }

    #endregion

    /// <summary>
    /// Initializes a new Obstacle instance.
    /// </summary>
    /// <param name="go">The GameObject to associate with this obstacle.</param>
    public Obstacle(GameObject go) : base(new XYZ(go.transform.position.x, go.transform.position.y, go.transform.position.z), 0f)
    {
        this.isDynamic = go.name.ToLower().Contains("user");
        Renderer renderer = go.GetComponent<Renderer>();
        if (renderer == null) renderer = go.GetComponentInChildren<Renderer>();
        length = renderer.bounds.size.z;
        width = renderer.bounds.size.x;
        this.radius = Mathf.Max(width, length) / 2f;
        this.go = go;
        CreateCircleAbstraction();
        if (circleAbstraction != null) Debug.Log(go.name + " generated " + circleAbstraction.Count + " circles.");
    }

    #region Public Methods
    /// <summary>
    /// Creates the circle abstraction for this obstacle.
    /// </summary>
    public void CreateCircleAbstraction()
    {
        if (!(width < length + .2f && width > length - .2f)) 
            circleAbstraction = SceneDataUtils.GenerateCircles(position, width, length, go.transform.eulerAngles.y);
    }

    /// <summary>
    /// Converts this obstacle to a Circle instance.
    /// </summary>
    /// <returns>A Circle instance representing this obstacle.</returns>
    public Circle ToCircle()
    {
        return new Circle(position, radius);
    }

    /// <summary>
    /// Draws a gizmo for this obstacle in the Unity editor.
    /// </summary>
    public void DrawGizmo()
    {
        if (radius == 0) return;
        if (circleAbstraction != null && circleAbstraction.Count > 0)
        {
            foreach (var circle in circleAbstraction)
            {
                SceneDataUtils.DrawCircleGizmo(circle.position, circle.radius, Color.red);
            }
        }
        else SceneDataUtils.DrawCircleGizmo(position, radius, Color.red);
    }

    /// <summary>
    /// Unpacks the circle abstractions from an array of obstacles.
    /// </summary>
    /// <param name="obstacles">The array of obstacles to unpack.</param>
    /// <returns>An array of Circle instances representing the unpacked abstractions.</returns>
    public static Circle[] UnpackAbstractions(Obstacle[] obstacles)
    {
        List<Circle> list = new List<Circle>();
        foreach (var obstacle in obstacles)
        {
            if (obstacle.circleAbstraction != null && obstacle.circleAbstraction.Count > 0)
            {
                foreach (var circle in obstacle.circleAbstraction)
                {
                    list.Add(circle);
                }
            }
            else
            {
                list.Add(obstacle.ToCircle());
            }
        }
        Debug.Log("Unpacked " + obstacles.Length + " obstacles into " + list.Count + " circles.");

        return list.ToArray();
    }

    #endregion

    #region Private Methods

    #endregion
}