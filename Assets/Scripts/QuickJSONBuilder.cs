using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuickJSONBuilder
{
    public JObject Root { get { return root; } }
    private JObject root;
    private JToken currentNode;
    public QuickJSONBuilder()
    {
        root = new JObject();
        currentNode = root;
    }
    public JObject Current { get { return currentNode as JObject; } }
    public JArray CurrentTable { get { return currentNode as JArray; } }

    public void Push(string key, JToken subNode)
    {
        if (currentNode is JObject jObj)
        {
			if(jObj.ContainsKey(key))
				currentNode = jObj[key];
			else
			{
				jObj.Add(key, subNode);
				currentNode = subNode;
			}
        }
        else if (currentNode is JArray jArray)
        {
            jArray.Add(subNode);
            currentNode = subNode;
        }
    }
    public void Push(string key)
    {
        Push(key, new JObject());
    }
    public void Pop()
    {
        if (currentNode.Parent is JToken jParent)
        {
            currentNode = jParent;
            if (currentNode is JObject || currentNode is JArray)
            {

            }
            else
            {
                Pop();
            }
        }
    }
    public Indent TableEntry()
    {
        return new Indent("", this);
    }
    public Indent Indentation(string key)
    {
        return new Indent(key, this);
    }
    public Table Tabulation(string key)
    {
        return new Table(key, this);
    }

    public class Table : IDisposable
    {
        private QuickJSONBuilder builder;
        public Table(string key, QuickJSONBuilder build)
        {
            builder = build;
            builder.Push(key, new JArray());
        }
        public void Dispose()
        {
            builder.Pop();
        }
    }
    public class Indent : IDisposable
    {
        private QuickJSONBuilder builder;
        public Indent(string key, QuickJSONBuilder build)
        {
            builder = build;
            builder.Push(key);
        }
        public void Dispose()
        {
            builder.Pop();
        }
    }
}

public static class JSONHelpers
{
    public static JArray ToJSON(this Vector3 v)
    {
        JArray jV = new JArray();
        jV.Add(v.x); jV.Add(v.y); jV.Add(v.z);
        return jV;
    }
}