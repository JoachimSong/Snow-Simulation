# Snow-Simulation
![Game Screenshot](https://songjc-portfolio-1323252154.cos.ap-shanghai.myqcloud.com/snow_simulation_1.png)
## Overview

Physical-based real-time snow simulation made with Unity.

## Table of Contents
- [Project Description](#project-description)
- [Demo Video](#video)
- [Features](#features)
- [Implementation Details](#implementation-details)
- [Installation](#installation)
- [Controls](#controls)
- [Screenshots](#screenshots)

## Project Description

This is a collaborative project involving three team members. During the simulation, it is necessary to calculate the adhesive force between snow particles, as well as inelastic compression. Additionally, the simulation incorporates thermodynamic principles to model the phase transition between water and ice. Building upon these aspects, the goal is to develop a real-time interactive application. My responsibilities include implementing force calculations between particles, designing the interaction between characters and snow, as well as designing and creating the UI.

## Video

Click here to enter: [**Video Link**](https://www.feicut.com/case-link/#/1739729132885495810)

## Features

- Diverse Interactive Methods: Players can control characters, push boxes, and shoot balls to interact with snow, observing the compression of snow.
- Free Scene Creation: Players can use a mechanism similar to Minecraft to distribute snow particles in the desired way.
- Simulate Phase Transition between Water and Ice Using Thermodynamic Principles: The scene includes heat sources and cold sources. Snow will melt into water in warmer areas, and water will freeze into ice in colder regions.

## Implementation Details
Based on Paper [Particle Simulation using CUDA](https://developer.download.nvidia.cn/assets/cuda/files/particles.pdf), I designed the algorithm for our project.
I wrote a compute shader that used the GPU to accelerate the neighbor-searching process between particles and thus accelerate the computation of inter-particle forces.

1. Utilize grids for spatial partitioning.
2. Apply the Bitonic sorting algorithm to sort grid hash values.
3. Conduct neighbor search using indices.
4. Calculate the cohesive force, tangential resistance, gravity, and damping force acting on particles.
5. Update the velocity and position of particles.

## Installation

Download and unzip the **Snow Simulation_Build.zip**, then open the **Snow Simulation.exe** to launch the game.

## Controls

- **W/A/S/D:** Move character.
- **Space:** Jump.
- **Right Mouse Click:** Place the snow cube.
- **Left Mouse Click:** Destroy the snow cube.
- **Mouse Move:** Rotate view.
- **E:** Start simulation. The cubes will turn into snow particles.
- **R:** Stop simulation and return to build mode.
- **T:** Launch a ball that will collide with the snow.
- **C:** Pause. Players can adjust the volume and the temperature in the scene.

## Screenshots

![Screenshot 1](https://songjc-portfolio-1323252154.cos.ap-shanghai.myqcloud.com/snow_simulation_4.png)
![Screenshot 2](https://songjc-portfolio-1323252154.cos.ap-shanghai.myqcloud.com/snow_simulation_2.png)
![Screenshot 3](https://songjc-portfolio-1323252154.cos.ap-shanghai.myqcloud.com/snow_simulation_3.png)
![Screenshot 4](https://songjc-portfolio-1323252154.cos.ap-shanghai.myqcloud.com/snow_simulation_1.png)

## Copyright Notice

All materials used in this project are sourced from the internet or Unity Asset Store and are intended for creative and showcase purposes. If you are the original creator of any material used in this project and have any objections, please feel free to contact me, and I will promptly remove the relevant content.

Contact Information:
- Email: pmatsemit@gmail.com