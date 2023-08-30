<h1 align="center">Crowd Simulation With Flocking Birds</h1>

<h2 align="center">

   Personal Project

</h2>

<p align="center">

<img src="https://github.com/jonasvalvik/FlockingSimulation_Unity/assets/6436680/51ed9cad-4201-43a3-8fdf-e7662bd5705e" >
</p>


## Description

Flocking bird algorithm implemented in Unity. Based on Craig Reynolds' simulated flocking creatures - boids. 

## Tech Stack

- C#
- Unity3D (ver. 2021.3.3f1)

## Features

* Three adjustable steering behaviours:
    * **Seperation:** The boid steers away from other local boids to avoid collision.
    * **Alignment:** The boid steers in the average direction of other local boids.
    * **Cohesion:** The boid steers toward the average position of other local boids

<p align="center">
<img src="https://github.com/jonasvalvik/FlockingSimulation_Unity/assets/6436680/b3c5274e-53e2-47de-a766-fb8f75e8121e" alt="BoidBehavior" width="509"> 
   <p align="center">
      Illustrated boid behavior of seperation (left), alignment (middle), and cohesion (right).
   </p>
</p>

* Environmental collision detection (predator avoidance)
* Scalable flock sizes
* Easily applied to any gameobject (bird, fish, horse, etc.)
