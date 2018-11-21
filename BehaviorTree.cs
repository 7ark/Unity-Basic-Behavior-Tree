using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum NodeResult { Failed, Running, Succeeded }

public class BehaviorTree
{
    BehaviorNode tree;
    public bool Running { get; private set; }

    public BehaviorTree(BehaviorNode startingNode)
    {
        tree = startingNode;
        Run();
    }

    public void Run()
    {
        Running = true;
    }

    /// <summary>
    /// Should be called in a MonoBehaviour's Update()
    /// </summary>
    public void Tick()
    {
        if(Running)
        {
            NodeResult result = tree.ConductAction();
            if(result == NodeResult.Running)
            {
                return;
            }

            Running = result == NodeResult.Succeeded;
        }
    }
}

public abstract class BehaviorNode
{
    protected float runTime = 0;
    
    public List<BehaviorNode> children = new List<BehaviorNode>();

    public abstract NodeResult ConductAction();

    public BehaviorNode(BehaviorNode[] nodes)
    {
        for (int i = 0; i < nodes.Length; i++)
        {
            children.Add(nodes[i]);
        }
    }
}

public class RepeaterNode : BehaviorNode
{
    public bool RunForever = true;
    public int StepsToRun = 1;
    public bool StopOnFailure = false;

    private int timesRun = 0;

    public RepeaterNode(params BehaviorNode[] nodes) : base(nodes)
    {
    }

    public override NodeResult ConductAction()
    {
        for (int i = 0; i < children.Count; i++)
        {
            switch(children[i].ConductAction())
            {
                case NodeResult.Running:
                    return NodeResult.Running;
                case NodeResult.Failed:
                    if(StopOnFailure)
                    {
                        return NodeResult.Failed;
                    }
                    break;
            }
        }

        if(!RunForever)
        {
            timesRun++;
            if (timesRun >= StepsToRun)
            {
                return NodeResult.Failed;
            }
        }

        return NodeResult.Succeeded;
    }
}

/// <summary>
/// AND Functionality
/// </summary>
public class SequenceNode : BehaviorNode
{
    public SequenceNode(params BehaviorNode[] nodes) : base(nodes)
    {
    }

    public override NodeResult ConductAction()
    {
        for (int i = 0; i < children.Count; i++)
        {
            switch(children[i].ConductAction())
            {
                case NodeResult.Running:
                    return NodeResult.Running;
                case NodeResult.Failed:
                    return NodeResult.Failed;
                case NodeResult.Succeeded:
                    break;
            }
        }
        return NodeResult.Succeeded;
    }
}

/// <summary>
/// OR Functionality
/// </summary>
public class AnyNode : BehaviorNode
{
    public AnyNode(params BehaviorNode[] nodes) : base(nodes)
    {
    }

    public override NodeResult ConductAction()
    {
        for (int i = 0; i < children.Count; i++)
        {
            switch (children[i].ConductAction())
            {
                case NodeResult.Running:
                    return NodeResult.Running;
                case NodeResult.Succeeded:
                    return NodeResult.Succeeded;
                case NodeResult.Failed:
                    break;
            }
        }
        return NodeResult.Failed;
    }
}

public class RunNode : BehaviorNode
{
    public Action<Action<NodeResult>> behavior;
    bool running = false;
    bool finishedRunning = false;
    NodeResult finalResult = NodeResult.Running;

    public RunNode(Action<Action<NodeResult>> action, params BehaviorNode[] nodes) : base(nodes)
    {
        behavior = action;
    }

    public override NodeResult ConductAction()
    {
        if (running)
        {
            if(finishedRunning)
            {
                running = false;
                return finalResult;
            }
            return NodeResult.Running;
        }
        else
        {
            running = true;
            finishedRunning = false;
            behavior(DoneRunning);
            if(finishedRunning)
            {
                return finalResult;
            }
            return NodeResult.Running;
        }
    }

    private void DoneRunning(NodeResult result)
    {
        finishedRunning = true;
        finalResult = result;
    }
}

public class WaitNode : BehaviorNode
{
    float timer = 0;
    bool running = false;

    public WaitNode(float timeToRun, params BehaviorNode[] nodes) : base(nodes)
    {
        runTime = timeToRun;
    }

    public override NodeResult ConductAction()
    {
        if (running)
        {
            timer -= Time.deltaTime;
            if(timer <= 0)
            {
                running = false;
                return NodeResult.Succeeded;
            }
            return NodeResult.Running;
        }
        else
        {
            running = true;
            timer = runTime;
            return NodeResult.Running;
        }
    }
}
