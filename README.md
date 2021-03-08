# test_project_1  
a series of tests to create a procedurally generated 2-D level  

# random room layout generator
creates a 2 dimensional layout of rooms and hallways of random size, spawns them within a certain area and then connects them under certain rules  
  
The process starts by generating a series of random sized rooms within an area. Then seperating each of the rooms that overlap through flocking behaviour. After all rooms are seperated, create a spanning tree through triangulation by an incremental algorithm. Then connect the rooms using a minimum spanning tree.
