# Curve Builder
## A project about bezier curves

### Description
Unity editor tool to create Bezier curves and evaluate points along the path. The main intention is on building non-orthogonal roads between intersections. Other uses include dropping fence prefabs along a path to match grounded height.

Later, this would also be cool to use for projectile trajectories. At the moment, I'm happy leaving this as an editor tool primarily. Provided as-is.

![overview](/imgs/overview.png)


## TODO
* multiple selection doesn't work
* fix tools for placing intersections
* add 'current intersection' tool to quickly swap between intersections and curves
* fence builder window
* extrude shape editor / extrude mesh
* fix uv coordinates on stretched roads
* add second uv channel to meshes for baking lightmaps

## KNOWN BUGS
* When generating roads from the road window, you usually need to press the button twice
* Sometimes curves get detached from anchors. You may have to remove these from the hierarchy manually
* Building individual sections of road doesn't appear to save the mesh correctly. Build all roads when you are happy with everything.


### References
* [Joachim Holmér at Unite 2015](https://www.youtube.com/watch?v=o9RK6O2kOKo)
* [BezierPath.cs](http://www.devmag.org.za/downloads/bezier_curves/BezierPath.cs)
* [catlikecoding](http://catlikecoding.com/unity/tutorials/curves-and-splines/)
