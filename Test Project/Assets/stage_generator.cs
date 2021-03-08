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
    /// :attribute: int node - the node number of the room
    /// </para>
    /// <para>
    /// :attribute: string room_type - defines the room: "ROOM", "MAJOR_ROOM"
    /// </para>
    /// <para>
    /// :constructor: double x, double y
    /// </para>
    /// </summary>
    public (double, double) midpoint;
    public (double, double) size;

    public int node;
    public string room_type;

    public Room(double x_cord = 0, double y_cord = 0)
    {
        midpoint = (x_cord,y_cord);
    }
}

public class Vector
{
    /// <summary>
    /// a 2 dimensional vector with attributes x and y
    /// <para>
    /// :constructor: double x, double y
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

public class LineSegment
{
    /// <summary>
    /// a line segement that is defined by two vectors node_1_position and node_2_position
    /// <para>
    /// :attribute: Vector node_1_position
    /// </para>
    /// <para>
    /// :attribute: Vector node_2_position
    /// </para>
    /// <para>
    /// :attribute: int node_1_number
    /// </para>
    /// <para>
    /// :attribute: int node_2_number
    /// </para>
    /// <para>
    /// :constructor: Vector node_1, Vector node_2
    /// </para>
    /// </summary>
    public Vector node_1_position;
    public Vector node_2_position;

    public int node_1_number;
    public int node_2_number;

    public LineSegment(Vector node_1, Vector node_2)
    {
        node_1_position = node_1;
        node_2_position = node_2;
    }
}

public class Program
{
    /// <summary>
    /// </summary>
    /// 
    public static void DrawLine(Vector3 start, Vector3 end, Color color)
    {
        /// debugging function - remove this function when done
        GameObject myLine = new GameObject();
        myLine.transform.position = start;
        myLine.AddComponent<LineRenderer>();
        LineRenderer lr = myLine.GetComponent<LineRenderer>();
        lr.material = new Material(Shader.Find("Legacy Shaders/Diffuse"));
        lr.SetColors(color, color);
        lr.SetWidth(0.1f, 0.1f);
        lr.SetPosition(0, start);
        lr.SetPosition(1, end);
    }

    public static (double, double) GetCirclePoint(double circle_radius, double angle)
    {
        /// returns the cartesian point within a circle given the circle radius and the angle from 0
        /// :return: tuple value (double, double)
        (double, double) point = (circle_radius * Math.Cos(angle), circle_radius * Math.Sin(angle));

        return point;
    }

    public static bool ComputeRoomOverlap(Room A, Room B)
    {
        /// checks if the x and y ranges that A and B occupy overlap
        /// :return: bool
        bool x_overlap = A.midpoint.Item1 - A.size.Item1 / 2 < B.midpoint.Item1 + B.size.Item1 / 2 && B.midpoint.Item1 - B.size.Item1 / 2 < A.midpoint.Item1 + A.size.Item1 / 2;
        bool y_overlap = A.midpoint.Item2 - A.size.Item2 / 2 < B.midpoint.Item2 + B.size.Item2 / 2 && B.midpoint.Item2 - B.size.Item2 / 2 < A.midpoint.Item2 + A.size.Item2 / 2;
        return x_overlap == true && y_overlap == true;
    }

    public static Vector ComputeSeperation(Room room, Room[] all_rooms)
    {
        /// loops through each of the rooms and seperates them a tiny bit based on the relationship between it
        /// and all other rooms that are touching or within it
        /// :return: (double, double)
        Vector seperation_vector = new Vector(0,0);
        int neighborCount = 0;
        foreach (Room i in all_rooms)
        {
            if (i != room)
            {
                if (ComputeRoomOverlap(room, i))
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

    public static bool ComputeIfBetween(double x, double x_1, double x_2)
    {
        /// check if a value lies between two values
        /// :return: bool
        return ((x_1 < x && x < x_2) || (x_2 < x && x < x_1));
    }

    public static bool ComputeIfIntersect(LineSegment line_1, LineSegment line_2)
    {
        /// check if two line segements intersect and returns a boolean
        /// :return: bool
        double m_1 = (line_1.node_2_position.y - line_1.node_1_position.y) / (line_1.node_2_position.x - line_1.node_1_position.x);
        double m_2 = (line_2.node_2_position.y - line_2.node_1_position.y) / (line_2.node_2_position.x - line_2.node_1_position.x);
        double b_1 = line_1.node_1_position.y - m_1 * line_1.node_1_position.x;
        double b_2 = line_2.node_1_position.y - m_2 * line_2.node_1_position.x;

        if (Math.Abs(m_1-m_2) < Double.Epsilon)
        {
            if (Math.Abs(b_1-b_2) < Double.Epsilon)
            {
                // same line case
                if (line_1.node_1_position.x < line_2.node_2_position.x && line_2.node_1_position.x < line_1.node_2_position.x) return true;
            }
            // no the same line but parallel
            return false;
        }

        // not parallel case
        double intersect_x = (b_2 - b_1) / (m_1 - m_2);

        if (ComputeIfBetween(intersect_x, line_1.node_1_position.x, line_1.node_2_position.x) && ComputeIfBetween(intersect_x, line_2.node_1_position.x, line_2.node_2_position.x)) return true;

        return false;
    }

    public static Room[] GenerateRooms(int total_rooms,
        double optional_generation_radius = 10.00,
        double optional_min_room_size = 1.00,
        double optional_max_room_size = 10.00,
        double optional_major_room_size_threshold = 70.00,
        int optional_major_room_max_count = 10)
    {
        /// creates a series of rooms and connected them together
        /// optional_generation_radius is how far from a center point can rooms be generated
        /// :return: Room[]

        // create an array to store all of the rooms
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
            double room_width = Math.Floor(rand.NextDouble() * (optional_max_room_size - optional_min_room_size) + optional_min_room_size);
            double room_height = Math.Floor(rand.NextDouble() * (optional_max_room_size - optional_min_room_size) + optional_min_room_size);

            room_points[i].size = (room_width, room_height);

            room_points[i].node = i;
            room_points[i].room_type = "ROOM";
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

        // snap all rooms back to grid alignment by flooring their final positions

        for (int i = 0; i < total_rooms; i++)
        {
            room_points[i].midpoint.Item1 = Math.Floor(room_points[i].midpoint.Item1);
            room_points[i].midpoint.Item2 = Math.Floor(room_points[i].midpoint.Item2);
        }

        // select major rooms by sorting all rooms above the area threshold
        List<int> major_room_indicies = new List<int>();

        for (int i=0; i < room_points.Length; i++)
        {
            double room_size = room_points[i].size.Item1 * room_points[i].size.Item2;
            if (room_size >= optional_major_room_size_threshold)
            {
                major_room_indicies.Add(i);
            }
        }

        //randomly select from the list of rooms that meet the requirements of being a major room
        List<int> selected_major_rooms = new List<int>();
        int z = 0;
        while(z < optional_major_room_max_count && z < major_room_indicies.Count)
        {
            int index = major_room_indicies[rand.Next(major_room_indicies.Count)];

            if(room_points[index].room_type != "MAJOR_ROOM")
            {
                selected_major_rooms.Add(index);
                room_points[index].room_type = "MAJOR_ROOM";
                z++;
            }
        }

        // we can bypass quasi triangulation of rooms if there are less than four major rooms
        if (selected_major_rooms.Count < 4)
        {
            return room_points;
        }

        // sort all the major rooms and x_cordinate left to right
        // transfer the indexes from selected_major_rooms to sorted_by_x_major_rooms
        // O(n^2) sorting, major rooms count most likely will always be < 100
        List<int> sorted_by_x_major_rooms = new List<int>();
        if(selected_major_rooms.Count > 0)
        {
            sorted_by_x_major_rooms.Add(selected_major_rooms[0]);
            for (int i = 1; i < selected_major_rooms.Count; i++)
            {
                // the integer z has already been declared before
                for (z = 0; z < sorted_by_x_major_rooms.Count; z++)
                {
                    if(room_points[selected_major_rooms[i]].midpoint.Item1 < room_points[sorted_by_x_major_rooms[z]].midpoint.Item1)
                    {
                        sorted_by_x_major_rooms.Insert(z, selected_major_rooms[i]);
                        break;
                    }
                    if(z == sorted_by_x_major_rooms.Count-1)
                    {
                        sorted_by_x_major_rooms.Add(selected_major_rooms[i]);
                        break;
                    }
                }
            }
        }

        // create a triangle with the first three points
        // n_# - node number x
        // s_1 - segment number x

        Vector n_1 = new Vector(room_points[sorted_by_x_major_rooms[0]].midpoint.Item1, room_points[sorted_by_x_major_rooms[0]].midpoint.Item2);
        Vector n_2 = new Vector(room_points[sorted_by_x_major_rooms[1]].midpoint.Item1, room_points[sorted_by_x_major_rooms[1]].midpoint.Item2);
        Vector n_3 = new Vector(room_points[sorted_by_x_major_rooms[2]].midpoint.Item1, room_points[sorted_by_x_major_rooms[2]].midpoint.Item2);
        LineSegment s_1 = new LineSegment(n_1, n_2);
        s_1.node_1_number = room_points[sorted_by_x_major_rooms[0]].node;
        s_1.node_2_number = room_points[sorted_by_x_major_rooms[1]].node;
        LineSegment s_2 = new LineSegment(n_1, n_3);
        s_2.node_1_number = room_points[sorted_by_x_major_rooms[0]].node;
        s_2.node_2_number = room_points[sorted_by_x_major_rooms[2]].node;
        LineSegment s_3 = new LineSegment(n_2, n_3);
        s_3.node_1_number = room_points[sorted_by_x_major_rooms[0]].node;
        s_3.node_2_number = room_points[sorted_by_x_major_rooms[2]].node;

        List<LineSegment> line_segments = new List<LineSegment>
        {
            s_1,
            s_2,
            s_3
        };

        // consider the next point in the set, the conditions are it must connect to the previous node and it must connect to another node that is visible
        // this next node will always see the previous node so line segment s_n is always guarenteed if it connects this next node and it's previous
        // line segment s_n2 will need to compute which previous nodes are visible to it and which nodes are connect to s_n
        // if all previous line segements are in a line then we have the case where no s_n2 cannot be computed and we only compute s_n
        // repeat until we reach the final node and we have a triangulation

        // as another note s_n can be removed if you connect the next node to all previously visible nodes. This removes the entire first section of the
        // function below as for each next node we just have to test which connections are possible and connect them all.

        for (int i = 3; i < sorted_by_x_major_rooms.Count; i++)
        {
            Vector node_1 = new Vector(room_points[sorted_by_x_major_rooms[i]].midpoint.Item1, room_points[sorted_by_x_major_rooms[i]].midpoint.Item2);
            Vector node_2 = new Vector(room_points[sorted_by_x_major_rooms[i-1]].midpoint.Item1, room_points[sorted_by_x_major_rooms[i - 1]].midpoint.Item2);
            LineSegment s_n = new LineSegment(node_1, node_2);
            s_n.node_1_number = room_points[sorted_by_x_major_rooms[i]].node;
            s_n.node_2_number = room_points[sorted_by_x_major_rooms[i - 1]].node;

            line_segments.Add(s_n);

            for(z = i-2; z >= 0; z--)
            {
                Vector node_3 = new Vector(room_points[sorted_by_x_major_rooms[z]].midpoint.Item1, room_points[sorted_by_x_major_rooms[z]].midpoint.Item2);
                LineSegment s_n2 = new LineSegment(node_1, node_3);
                s_n2.node_1_number = room_points[sorted_by_x_major_rooms[i]].node;
                s_n2.node_2_number = room_points[sorted_by_x_major_rooms[z]].node;

                bool check_new_segement_good = true;

                foreach(LineSegment l_n in line_segments)
                {
                    if(ComputeIfIntersect(l_n, s_n2))
                    {
                        check_new_segement_good = false;
                        break;
                    }
                }

                if (check_new_segement_good)
                {
                    line_segments.Add(s_n2);
                }
            }
        }

        // debuging loop remove later
        foreach(LineSegment l_n in line_segments)
        {
            Vector3 start = new Vector3((float)l_n.node_1_position.x, (float)l_n.node_1_position.y, -1);
            Vector3 end = new Vector3((float)l_n.node_2_position.x, (float)l_n.node_2_position.y, -1);
            DrawLine(start, end, Color.red);
        }

        // using a minimum spanning tree to connect rooms

        // make paths using the verticies of the minimum spanning tree

        // remove all rooms that do not touch the paths

        // fill in any holes along the path with hallways

        return room_points;

    }
}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
///UNITY_FUNCTION
/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

public class stage_generator : MonoBehaviour
{
    public GameObject cube;
    public GameObject room_cube;
    public GameObject corner_cube;

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Up and running and generating rooms");

        const int max_rooms = 50;

        Room[] temp_room_points = new Room[max_rooms];

        temp_room_points = Program.GenerateRooms(
            total_rooms : max_rooms,
            optional_generation_radius : 20.00,
            optional_min_room_size : 1.00,
            optional_max_room_size : 20.00,
            optional_major_room_size_threshold : 100.00,
            optional_major_room_max_count : 10
            );

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
            if(rooms[i].room_type == "MAJOR_ROOM")
            {
                GameObject newCube = Instantiate(room_cube, new Vector3((float)rooms[i].midpoint.Item1, (float)rooms[i].midpoint.Item2, 0), Quaternion.identity);
                newCube.transform.localScale = new Vector3((float)rooms[i].size.Item1, (float)rooms[i].size.Item2, 1);
            }
            else
            {
                GameObject newCube = Instantiate(cube, new Vector3((float)rooms[i].midpoint.Item1, (float)rooms[i].midpoint.Item2, 0), Quaternion.identity);
                newCube.transform.localScale = new Vector3((float)rooms[i].size.Item1, (float)rooms[i].size.Item2, 1);
            }

            Instantiate(corner_cube, new Vector3((float)rooms[i].midpoint.Item1 + (float)(rooms[i].size.Item1 / 2), (float)rooms[i].midpoint.Item2 + (float)(rooms[i].size.Item2 / 2), 0), Quaternion.identity);
            Instantiate(corner_cube, new Vector3((float)rooms[i].midpoint.Item1 + (float)(rooms[i].size.Item1 / 2), (float)rooms[i].midpoint.Item2 - (float)(rooms[i].size.Item2 / 2), 0), Quaternion.identity);
            Instantiate(corner_cube, new Vector3((float)rooms[i].midpoint.Item1 - (float)(rooms[i].size.Item1 / 2), (float)rooms[i].midpoint.Item2 - (float)(rooms[i].size.Item2 / 2), 0), Quaternion.identity);
            Instantiate(corner_cube, new Vector3((float)rooms[i].midpoint.Item1 - (float)(rooms[i].size.Item1 / 2), (float)rooms[i].midpoint.Item2 + (float)(rooms[i].size.Item2 / 2), 0), Quaternion.identity);
        }
    }
}
