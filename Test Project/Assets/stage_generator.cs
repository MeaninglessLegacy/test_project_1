/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
///GENERATE_ROOMS
/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
///MAIN_PROGRAM
/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

public class Room
{
    /// <summary>
    /// a room object that is centered around a midpoint
    /// <para>
    /// :attribute: (double, double) midpoint
    /// </para>
    /// <para>
    /// :attribute: (double, double) size - the size attribute the total x length or y length from one of the room to the other.
    /// </para>
    /// <para>
    /// :constructor: Point(double x, double y)
    /// </para>
    /// </summary>
    public (double, double) midpoint;
    public (double, double) size;

    public Room(double x_cord = 0, double y_cord = 0)
    {
        midpoint = (x_cord,y_cord);
    }
}

public class Vector
{
    /// <summary>
    /// a 2 dimensional vector with attributes x & y
    /// <para>
    /// :constructor: Vector(double x, double y)
    /// </para>
    /// </summary>
    public double x;
    public double y;

    public Vector(double x_value = 0, double y_value = 0)
    {
        x = x_value;
        y = y_value;
    }
}

public class Program
{
    /// <summary>
    /// </summary>

    public static (double, double) GetCirclePoint(double circle_radius, double angle)
    {
        // returns the cartesian point within a circle given the circle radius and the angle from 0
        // :return: tuple value (double, double)
        (double, double) point = (circle_radius * Math.Cos(angle), circle_radius * Math.Sin(angle));

        return point;
    }

    public static bool ComputeOverlap(Room A, Room B)
    {
        // checks if the x and y ranges that A and B occupy overlap
        // :return: bool
        bool x_overlap = A.midpoint.Item1 - A.size.Item1 / 2 < B.midpoint.Item1 + B.size.Item1 / 2 && B.midpoint.Item1 - B.size.Item1 / 2 < A.midpoint.Item1 + A.size.Item1 / 2;
        bool y_overlap = A.midpoint.Item2 - A.size.Item2 / 2 < B.midpoint.Item2 + B.size.Item2 / 2 && B.midpoint.Item2 - B.size.Item2 / 2 < A.midpoint.Item2 + A.size.Item2 / 2;
        return x_overlap == true && y_overlap == true;
    }

    public static Vector ComputeSeperation(Room room, Room[] all_rooms)
    {
        // loops through each of the rooms and seperates them a tiny bit based on the relationship between it
        // and all other rooms that are touching or within it
        // :return: (double, double)
        Vector seperation_vector = new Vector(0,0);
        int neighborCount = 0;
        foreach (Room i in all_rooms)
        {
            if (i != room)
            {
                if (ComputeOverlap(room, i))
                {
                    seperation_vector.x += i.midpoint.Item1 - room.midpoint.Item1;
                    seperation_vector.y += i.midpoint.Item2 - room.midpoint.Item2;
                    neighborCount++;
                }
            }
        }
        if (neighborCount == 0) return seperation_vector;

        //make the seperation vector point away from it's neighbors
        //normalize the seperation vector
        double vector_magnitude = Math.Sqrt(seperation_vector.x * seperation_vector.x + seperation_vector.y * seperation_vector.y);
        seperation_vector.x *= -1/vector_magnitude;
        seperation_vector.y *= -1 / vector_magnitude;

        return seperation_vector;
    }

    public static Room[] GenerateRooms(int total_rooms,
        double optional_generation_radius = 30.00,
        double optional_min_room_size = 1.00,
        double optional_max_room_size = 50.00)
    {
        // creates a series of rooms and connected them together
        // optional_generation_radius is how far from a center point can rooms be generated

        // create a series of midpoints for each of the rooms
        Room[] room_points = new Room[total_rooms];

        var rand = new System.Random();

        for (int i=0; i<total_rooms; i++)
        {
            double angle = 2 * Math.PI * rand.NextDouble();
            double radius = optional_generation_radius * rand.NextDouble();
            (double, double) room_midpoint = GetCirclePoint(radius, angle);
            room_points[i] = new Room(room_midpoint.Item1, room_midpoint.Item2);

            // from each of the midpoints create a room of random size
            // minimum room size is set to 1
            double room_width = rand.NextDouble() * (optional_max_room_size - optional_min_room_size) + optional_min_room_size;
            double room_height = rand.NextDouble() * (optional_max_room_size - optional_min_room_size) + optional_min_room_size;

            room_points[i].size = (room_width, room_height);
        }

        // create a list for seperation vectors
        Vector[] seperation_vectors = new Vector[total_rooms];
        // push room apart to solve collisons, current solution is to use a seperation steering behaviour algorithm
        bool simulate = true;
        int simulation_steps = 0;
        while(simulate == true && simulation_steps < 10000)
        {
            // if all points are seperated then the seperation vectors will be 0 and thus we can end the simulation
            // or if the simulation has reached 10000 steps
            bool continue_simulation = false;
            simulation_steps++;

            for (int i=0; i < room_points.Length; i++)
            {
                seperation_vectors[i] = ComputeSeperation(room_points[i], room_points);
                //if the magntiude of the result vector is zero
                if(Math.Sqrt(seperation_vectors[i].x * seperation_vectors[i].x + seperation_vectors[i].y * seperation_vectors[i].y) != 0)
                {
                    continue_simulation = true;
                }
            }

            // move all rooms by each corresponding seperation vector
            for (int i = 0; i < room_points.Length; i++)
            {
                room_points[i].midpoint.Item1 += seperation_vectors[i].x;
                room_points[i].midpoint.Item2 += seperation_vectors[i].y;
            }

            //break simulating if all vectors are zero
            if (continue_simulation == false) break;
        }
        // select major rooms

        // using a minimum spanning tree to connect rooms

        return room_points;

    }
}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
///UNITY_FUNCTION
/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

public class stage_generator : MonoBehaviour
{
    public GameObject cube;
    public GameObject corner_cube;

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Up and running and generating rooms");

        const int max_rooms = 100;

        Room[] temp_room_points = new Room[max_rooms];

        temp_room_points = Program.GenerateRooms(max_rooms, 50, 3, 50);

        DisplayRooms(temp_room_points);
    }

    // Update is called once per frame
    void Update()
    {
    }

    void DisplayRooms(Room[] rooms)
    {
        // temporary display
        for (int i = 0; i < rooms.Length; i++)
        {
            GameObject newCube = Instantiate(cube, new Vector3((float)rooms[i].midpoint.Item1, (float)rooms[i].midpoint.Item2, 0), Quaternion.identity);
            newCube.transform.localScale = new Vector3((float)rooms[i].size.Item1, (float)rooms[i].size.Item2, 1);

            Instantiate(corner_cube, new Vector3((float)rooms[i].midpoint.Item1 + (float)(rooms[i].size.Item1 / 2), (float)rooms[i].midpoint.Item2 + (float)(rooms[i].size.Item2 / 2), 0), Quaternion.identity);
            Instantiate(corner_cube, new Vector3((float)rooms[i].midpoint.Item1 + (float)(rooms[i].size.Item1 / 2), (float)rooms[i].midpoint.Item2 - (float)(rooms[i].size.Item2 / 2), 0), Quaternion.identity);
            Instantiate(corner_cube, new Vector3((float)rooms[i].midpoint.Item1 - (float)(rooms[i].size.Item1 / 2), (float)rooms[i].midpoint.Item2 - (float)(rooms[i].size.Item2 / 2), 0), Quaternion.identity);
            Instantiate(corner_cube, new Vector3((float)rooms[i].midpoint.Item1 - (float)(rooms[i].size.Item1 / 2), (float)rooms[i].midpoint.Item2 + (float)(rooms[i].size.Item2 / 2), 0), Quaternion.identity);
        }
    }
}
