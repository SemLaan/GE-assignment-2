using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room
{
    public Vector2Int position;
    public Vector2Int size;
    public Vector2Int shrunkPosition;
    public Vector2Int shrunkSize;
    public Room[] children;
    public bool isLeaf;

    public Room(Vector2Int position, Vector2Int size)
    {
        this.position = position;
        this.size = size;
        isLeaf = true;
    }

    public void Split()
    {
        children = new Room[2];
        isLeaf = false;

        if (size.x > size.y)
        {
            int randomSplit = (int)(size.x * Random.Range(0.3f, 0.7f));

            children[0] = new Room(position, new Vector2Int(randomSplit, size.y));
            children[1] = new Room(new Vector2Int(position.x + randomSplit, position.y), new Vector2Int(size.x - randomSplit, size.y));
        } else
        {
            int randomSplit = (int)(size.y * Random.Range(0.3f, 0.7f));
            children[0] = new Room(position, new Vector2Int(size.x, randomSplit));
            children[1] = new Room(new Vector2Int(position.x, position.y + randomSplit), new Vector2Int(size.x, size.y - randomSplit));
        }
    }

    public void ShrinkRoom()
    {
        shrunkSize = new Vector2Int((int)(size.x * Random.Range(0.5f, 0.8f)), (int)(size.y * Random.Range(0.5f, 0.8f)));
        Vector2Int sizeDifference = size - shrunkSize;
        shrunkPosition = position + new Vector2Int((int)((sizeDifference.x -2) * Random.Range(0f, 1f)) + 1, 
            (int)((sizeDifference.y - 2) * Random.Range(0f, 1f)) + 1);
    }

    public Room GetLeafRoom(bool random = false)
    {
        if (!random)
        {
            if (isLeaf)
                return this;
            else
                return children[0].GetLeafRoom();
        }
        else
        {
            if (isLeaf)
                return this;
            else
                return children[Random.Range(0, 2)].GetLeafRoom(true);
        }
    }
}
