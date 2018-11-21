# Unity-Basic-Behavior-Tree
Just a basic code-based behavior tree for Unity

Very simple to use. An example:
```cs
public class MyClass : Monobehaviour
{
  BehaviorTree tree;
  Vector2[] pathfindingPath = null;
  
  private void Awake()
  {
    //New tree
    tree = new BehaviorTree( 
      //New repeating segement, by default will run forever
      new RepeaterNode(
        //Starts new sequence, the first time it receives a failure from it's child, it will stop
        new Sequence(
          new RunNode(CheckIfPathNull),
          new RunNode(CreateNewPath)
        )
      )
    );
  }
  
  void Update()
  {
    tree.Tick();
  }
  
  private void CheckIfPathNull(Action<NodeResult> result)
  {
    result(pathfindingPath == null ? NodeResult.Succeeded : NodeResult.Failed);
  }
  
  private void CreateNewPath(Action<NodeResult> result)
  {
    pathfindingPath = AStar.CreateNewPath(); //Some function that returns an array of points for A* pathfinding
    result(NodeResult.Succeeded);
  }
```
This will simply keep running (via the RepeaterNode), then in sequence run a set of actions until it receives a failed result. If CheckIfPathNull returns failed, it wont move onto CreateNewPath. 
