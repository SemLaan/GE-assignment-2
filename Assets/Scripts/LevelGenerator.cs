using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils;

public enum TileType {
    Empty = 0,
    Player,
    Enemy,
    Wall,
    Door,
    Key,
    Dagger,
    End
}

public class LevelGenerator : MonoBehaviour
{
    public GameObject[] tiles;
    [SerializeField] private int rooms = 16;

    protected void Start()
    {        
        int width = 64;
        int height = 64;
        TileType[,] grid = new TileType[height, width];

        /*FillBlock(grid, 0, 0, width, height, TileType.Wall);
        FillBlock(grid, 26, 26, 12, 12, TileType.Empty);
        FillBlock(grid, 32, 28, 1, 1, TileType.Player);
        FillBlock(grid, 30, 30, 1, 1, TileType.Dagger);
        FillBlock(grid, 34, 30, 1, 1, TileType.Key);
        FillBlock(grid, 32, 32, 1, 1, TileType.Door);
        FillBlock(grid, 32, 36, 1, 1, TileType.Enemy);
        FillBlock(grid, 32, 34, 1, 1, TileType.End);*/

        GenerateLevel(width, height);

        Debugger.instance.AddLabel(32, 26, "Room 1");
    }

    private void GenerateLevel(int width, int height)
    {
        // Creating a binary tree of rooms
        Room root = new Room(Vector2Int.zero, new Vector2Int(width, height));
        Queue<Room> roomQueue = new Queue<Room>();
        roomQueue.Enqueue(root);
        for (int i = 0; i < rooms - 1; i++)
        {
            Room splitRoom = roomQueue.Dequeue();
            splitRoom.Split();
            roomQueue.Enqueue(splitRoom.children[0]);
            roomQueue.Enqueue(splitRoom.children[1]);
        }

        // Looping through the binary tree of rooms and shrinking all the leaf rooms
        roomQueue.Clear();
        roomQueue.Enqueue(root);
        while(roomQueue.Count != 0)
        {
            Room currentRoom = roomQueue.Dequeue();
            if (currentRoom.isLeaf)
            {
                currentRoom.ShrinkRoom();
            }
            else
            {
                roomQueue.Enqueue(currentRoom.children[0]);
                roomQueue.Enqueue(currentRoom.children[1]);
            }
        }

        // Making the grid in the scene
        TileType[,] grid = new TileType[height, width];
        FillBlock(grid, 0, 0, width, height, TileType.Wall);

        roomQueue.Clear();
        roomQueue.Enqueue(root);
        while (roomQueue.Count != 0)
        {
            Room currentRoom = roomQueue.Dequeue();
            if (currentRoom.isLeaf)
            {
                FillBlock(grid, currentRoom.shrunkPosition, currentRoom.shrunkSize, TileType.Empty);
            }
            else
            {
                roomQueue.Enqueue(currentRoom.children[0]);
                roomQueue.Enqueue(currentRoom.children[1]);
            }
        }

        // Connecting the rooms
        roomQueue.Clear();
        roomQueue.Enqueue(root);
        while (roomQueue.Count != 0)
        {
            Room currentRoom = roomQueue.Dequeue();
            if (!currentRoom.isLeaf)
            {
                roomQueue.Enqueue(currentRoom.children[0]);
                roomQueue.Enqueue(currentRoom.children[1]);

                // create link between child rooms
                Room room1 = currentRoom.GetLeafRoom(), room2 = currentRoom.children[1].GetLeafRoom();
                Vector2Int middleOfRoom1 = room1.shrunkPosition + room1.shrunkSize / 2;
                Vector2Int middleOfRoom2 = room2.shrunkPosition + room2.shrunkSize / 2;
                int horizontalDistance = Mathf.Abs(middleOfRoom1.x - middleOfRoom2.x);
                int verticalDistance = Mathf.Abs(middleOfRoom1.y - middleOfRoom2.y);

                if (middleOfRoom1.y < middleOfRoom2.y)
                    FillBlock(grid, middleOfRoom2 - new Vector2Int(0, verticalDistance), new Vector2Int(1, verticalDistance), TileType.Empty);
                else
                    FillBlock(grid, middleOfRoom2, new Vector2Int(1, verticalDistance), TileType.Empty);

                if (middleOfRoom1.x > middleOfRoom2.x)
                    FillBlock(grid, middleOfRoom1 - new Vector2Int(horizontalDistance, 0), new Vector2Int(horizontalDistance, 1), TileType.Empty);
                else
                    FillBlock(grid, middleOfRoom1, new Vector2Int(horizontalDistance + 1, 1), TileType.Empty);
                /*if (middleOfRoom1.y > middleOfRoom2.y)
                    FillBlock(grid, middleOfRoom2, new Vector2Int(1, middleOfRoom1.y - middleOfRoom2.y), TileType.Empty);
                else
                    FillBlock(grid, middleOfRoom1, new Vector2Int(1, middleOfRoom2.y - middleOfRoom1.y), TileType.Empty);
                if (middleOfRoom1.x > middleOfRoom2.x)
                    FillBlock(grid, middleOfRoom2, new Vector2Int(middleOfRoom1.x - middleOfRoom2.x, 1), TileType.Empty);
                else
                    FillBlock(grid, middleOfRoom1, new Vector2Int(middleOfRoom2.x - middleOfRoom1.x, 1), TileType.Empty);*/
            }
        }

        // Adding unique tiles
        Room startingRoom = root.children[0].children[1].children[0].children[1];
        Room swordRoom = root.children[0].children[1].children[0].children[0];
        Room finishRoom = root.children[1].children[1].children[1].children[1];
        Room nextToFinishRoom = root.children[1].children[1].children[1].children[0];
        if (finishRoom.position.x == nextToFinishRoom.position.x)
        {
            FillBlock(grid, finishRoom.shrunkPosition + new Vector2Int(finishRoom.shrunkSize.x, -2) / 2, Vector2Int.one, TileType.Door);
        }
        else // if finishRoom.position.y == nextToFinishRoom.position.y
        {
            FillBlock(grid, nextToFinishRoom.shrunkPosition + new Vector2Int(1 + nextToFinishRoom.shrunkSize.x * 2, nextToFinishRoom.shrunkSize.y) / 2, Vector2Int.one, TileType.Door);
        }

        Room keyRoom = root.GetLeafRoom(true);
        while (keyRoom == finishRoom || keyRoom == swordRoom || keyRoom == startingRoom)
            keyRoom = root.GetLeafRoom(true); 
        Room enemyRoom = root.GetLeafRoom(true);
        while (enemyRoom == finishRoom || enemyRoom == swordRoom || enemyRoom == startingRoom || enemyRoom == keyRoom)
            enemyRoom = root.GetLeafRoom(true);
        FillBlock(grid, startingRoom.shrunkPosition + startingRoom.shrunkSize / 2, Vector2Int.one, TileType.Player);
        FillBlock(grid, swordRoom.shrunkPosition + swordRoom.shrunkSize / 2, Vector2Int.one, TileType.Dagger);
        FillBlock(grid, finishRoom.shrunkPosition + finishRoom.shrunkSize / 2, Vector2Int.one, TileType.End);
        FillBlock(grid, keyRoom.shrunkPosition + keyRoom.shrunkSize / 2, Vector2Int.one, TileType.Key);
        FillBlock(grid, enemyRoom.shrunkPosition + enemyRoom.shrunkSize / 2, Vector2Int.one, TileType.Enemy);

        CreateTilesFromArray(grid);
    }

    //fill part of array with tiles
    private void FillBlock(TileType[,] grid, int x, int y, int width, int height, TileType fillType) {
        for (int tileY=0; tileY<height; tileY++) {
            for (int tileX=0; tileX<width; tileX++) {
                grid[tileY + y, tileX + x] = fillType;
            }
        }
    }

    private void FillBlock(TileType[,] grid, Vector2Int position, Vector2Int size, TileType fillType)
    {
        FillBlock(grid, position.x, position.y, size.x, size.y, fillType);
    }

    //use array to create tiles
    private void CreateTilesFromArray(TileType[,] grid) {
        int height = grid.GetLength(0);
        int width = grid.GetLength(1);
        for (int y=0; y<height; y++) {
            for (int x=0; x<width; x++) {
                 TileType tile = grid[y, x];
                 if (tile != TileType.Empty) {
                     CreateTile(x, y, tile);
                 }
            }
        }
    }

    //create a single tile
    private GameObject CreateTile(int x, int y, TileType type) {
        int tileID = ((int)type) - 1;
        if (tileID >= 0 && tileID < tiles.Length)
        {
            GameObject tilePrefab = tiles[tileID];
            if (tilePrefab != null) {
                GameObject newTile = GameObject.Instantiate(tilePrefab, new Vector3(x, y, 0), Quaternion.identity);
                newTile.transform.SetParent(transform);
                return newTile;
            }

        } else {
            Debug.LogError("Invalid tile type selected");
        }

        return null;
    }

}
