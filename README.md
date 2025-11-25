# Milestone 1

## Wave Function Collapse

Implemented a basic Wave Function Collapse algorithm in Unity, with a flat 2D map. Two main types of components are used: tiles, which store lists of possible adjacent tiles, and the generator itself, which has parameters for the map dimensions, grid cell size, and a list of possible tile types to appear in the map. Currently tiles only check orthogonally-adjacent tiles (i.e. tiles within the von Neumann neighborhood of that tile) but this could be easily expanded (e.g. to a Moore neighborhood) depending on the needs of the tileset by adding additional arrays to the tile class and additional steps to the propogation function used in the generator. Another potential feature which is not currently included is varied tile weights, as each possible tile currently has an equal chance of being selected in a given step (though their effective frequency of appearance varies depending on how their connections occur within the grid).

<img width="410" height="537" alt="Screen Shot 2025-11-13 at 12 01 07 AM" src="https://github.com/user-attachments/assets/f034fcfc-8b3c-4e49-9c23-c1d2bcbe4ed6" />
<img width="410" height="539" alt="Screen Shot 2025-11-13 at 12 00 06 AM" src="https://github.com/user-attachments/assets/05a71e4a-be3f-497d-934d-6f1c8fb6e0a0" />
<img width="410" height="535" alt="Screen Shot 2025-11-12 at 11 58 54 PM" src="https://github.com/user-attachments/assets/76926983-0907-4d84-a17c-bca175f9bfff" />
<img width="410" height="537" alt="Screen Shot 2025-11-13 at 12 00 43 AM" src="https://github.com/user-attachments/assets/4d70ce51-203f-414f-a9c1-b1f3c28627ee" />

The above images show example outputs using a set of castle parts made in Maya. 

<img width="992" height="762" alt="Screen Shot 2025-11-13 at 12 05 43 AM" src="https://github.com/user-attachments/assets/5e7e9bc3-da3b-47ce-bf04-08c48cb68e39" />

<img width="1178" height="641" alt="Screen Shot 2025-11-13 at 12 00 20 AM" src="https://github.com/user-attachments/assets/16c3ad75-1788-4c98-ad0b-e79f17c0c59d" />


## Terrain Generation

Implemented basic Perlin-noise based terrain generation. I followed [this tutorial](https://www.youtube.com/watch?v=hNRFosb_3Tc) to set up the base of the terrain generation and it worked really well. To add onto it, I implemented chunking for infinite terrain. So, I added a basic player capsule and depending on the location of the player, chunks will be spawned (or destroyed) within a certain distance. As part of the tutorial I followed, I also added a basic gradient shader that colors the terrain based on height.

![Untitledvideo-MadewithClipchamp8-ezgif com-video-to-gif-converter](https://github.com/user-attachments/assets/30da67fa-9746-44f6-9dee-a9ce3c5dadc4)
![Recording2025-11-12232842-ezgif com-video-to-gif-converter](https://github.com/user-attachments/assets/93e1ca3f-a6f2-4d7b-8691-ee0b569e2c1d)

# Milestone 2

## Updating and Combining WFC with Terrain

The Wave Function Collapse-based city generation algorithm connects to the varied height of the terrain in two ways. WFC is still performed using a 2D grid of cells, but as we construct the grid we sample the height of the terrain for a grid of positions (using the X and Z coordinates of the corners, centers of each edge, and cell center) under each cell in order to get a minimum and maximum height for the terrain under that cell. Each cell then has the set of possible tiles for that cell updated based on a set of parameters configurable per each tile type: a set of allowed height bounds, where the cell must have its minimum and maximum lie within those bounds in order to use this type of tile, and a maximum height difference, where if the difference in height that occurs within that cell is greater than the maximum then that tile also cannot be used in this cell. After updating these possibilities, the changes are propagated just as when we select a cell's tile type in the main algorithm.

<img width="1339" height="827" alt="Screen Shot 2025-11-24 at 11 42 01 PM" src="https://github.com/user-attachments/assets/c6352b17-eaf7-4245-bad4-8cad0feefd48" />

For example, the above image shows a scene generated from a set of tiles that are split into two groups: one is the light-colored castle tiles, while the other is the dark-colored house tiles. Both tiles have their minimum height bound set so that they cannot appear below a certain value representing the water level, so that they cannot appear in any cell which has a height sample lying below the water level. The castle tiles have a slightly higher minimum than the house tiles, so the houses appear slightly more frequently in the low-lying areas as only they can appear at the lowest points above water. The house tiles however have a lower max height difference value than the castle tiles, meaning that the castle tiles can appear on steeper slopes than the house tiles, such as on the hills in the center and upper left of the above image.

The other form of connection between the systems is done in the creation of the models themselves after the tile types are chosen. The vertices of the model are all transformed based on the height of the terrain directly under that particular vertex. Components of the tile objects can configured to behave in two different ways: either all of the vertices map directly with the terrain height under them, or only vertices at or below a configurable ground level use the terrain directly under them, while the rest of the vertices use a center height shared across the whole tile. This allows features like the castle walls to fit closely to the landscape using the first configuration, enabling smooth connections between the upper walkways, while structures like the upper parts of towers don't slope and instead are simply raised evenly in order to not appear lopsided.

<img width="1339" height="835" alt="Screen Shot 2025-11-24 at 11 42 58 PM" src="https://github.com/user-attachments/assets/f5c4e976-837c-4344-a64a-eb6cd198705b" />

The configuration of tile types was also reworked and made simpler to modify than in the first milestone, with each tile being able to automatically generate rotated counterparts using a set of parameters controllable on the Unity side, as well as being able to use a set of flags to group together sets of tiles that are all valid neighbors in the same situations. For example, all of the castle wall tiles share the "Wall" flag, such that each wall can reference its valid neighbors for each direction it should connect to as simply being any tile which has that wall tag. This facilitates the easier creation of a larger set of tiles, such as the house tiles used in the above examples.

## Water Shader
I created a water shader, which is really just a plane to show the water. I did run into some issues with the render pipeline, and it does not show with the current post-process step.

![ScreenRecording2025-11-24013328-ezgif com-video-to-gif-converter](https://github.com/user-attachments/assets/1758e9e1-ad7b-4558-8abd-64c2bf0cd648)

## Post-Process Shader
For the post-process shader, I tried to mimic a watercolor, cute style. There weren't a lot of good resources to reference, but I loosely looked at [this example](https://www.bruteforcegame.com/post/watercolor-shader-devblog-13). Of course, all the work is different since this is an older blog. I basically split the post-process shadergraph into 4 subgraphs: PaperOverlay, EdgeDarkening, WaterBloom, and ColorQuantization. The goal was to bucket the colors in the scene and apply some effects to make it look more watercolory.

<img width="1366" height="781" alt="image" src="https://github.com/user-attachments/assets/831bb455-ee90-43c0-81ad-3e00ab924444" />

## First Person Player View
We also added a first person player view. This allows the player to move around in the terrain and get a better look at the environment.

![ScreenRecording2025-11-24173744-ezgif com-video-to-gif-converter (2)](https://github.com/user-attachments/assets/d341ab16-3485-4889-8256-95d0bb902f45)

# Design Document

## Introduction
We were inspired by the game Townscaper to create a procedurally generated, stylized environment where a player can explore and walk around in.

## Goal
The goal of this project is to procedurally generate a beautiful, explorable environment that combines algorithmic generation with aesthetic design. We want to use procedural rules to not only build geometry, but also be visually appealing and cohesive.

Specifically, we will:
- Generate a stylized terrain with water and elevation variation
- Populate it with procedurally generated buildings using wave function collapse 
- Allow the player to explore the scene in first person

## Inspiration/Reference
- Townscaper
<img width="1005" height="1007" alt="Screen Shot 2025-03-23 at 5 16 27 PM" src="https://github.com/user-attachments/assets/9d601148-04c8-40ec-992c-75c783f7448d" />
- [Infinite procedurally generated city with the Wave Function Collapse algorithm | Marian's Blog](https://marian42.de/article/wfc/)


## Specification
- Engine: Unity
- Camera: First-person exploration
- Programming Languages: C#
- Features:
  - Procedural terrain and water generation
  - Procedural building generation
  - Stylization
  - First-Person Controls

## Techniques
- Perlin Noise
- Wave Function Collapse
  - https://www.boristhebrave.com/2020/04/13/wave-function-collapse-explained/
  - https://www.boristhebrave.com/2021/06/06/driven-wavefunctioncollapse/
  - https://www.boristhebrave.com/2021/10/26/model-synthesis-and-modifying-in-blocks/ 
  - https://www.boristhebrave.com/2021/10/31/constraint-based-tile-generators/ 
- Stylization
  - https://github.com/jyguan18/hw02-stylization

## Design
<img width="1379" height="755" alt="Screen Shot 2025-11-05 at 2 52 37 PM" src="https://github.com/user-attachments/assets/089118cc-045f-4f36-b161-b7bf1e022c20" />


## Timeline

| |Griffin|Jackie|Deadline|
|--|--|--|--|
|**Milestone 1**|- Gather assets<br>- Wave Function Collapse for a flat plane|- Setup Github<br>- Gather assets<br>- Terrain Generation|Nov 12, 2025|
|**Milestone 2**|- First-Person Camera<br>- Adapt wave function collapse to the generated terrain|- Stylization (Post-Process Effects)<br>- Water Shading<br> - First-Person Camera|Nov 24, 2025|
|**Final**|- Final Tweaks<br>- Tune movement + collision|- Final Tweaks<br>- Tune colors, textures, etc.|Dec 1, 2025|


<br>



# Final Project!

This is it! The culmination of your procedural graphics experience this semester. For your final project, we'd like to give you the time and space to explore a topic of your choosing. You may choose any topic you please, so long as you vet the topic and scope with an instructor or TA. We've provided some suggestions below. The scope of your project should be roughly 1.5 homework assignments). To help structure your time, we're breaking down the project into 4 milestones:

## Project planning: Design Doc (due 11/5)
Before submitting your first milestone, _you must get your project idea and scope approved by Rachel, Adam or a TA._

### Design Doc
Start off by forking this repository. In your README, write a design doc to outline your project goals and implementation plan. It must include the following sections:

#### Introduction
- What motivates your project?

#### Goal
- What do you intend to achieve with this project?

#### Inspiration/reference:
- You must have some form of reference material for your final project. Your reference may be a research paper, a blog post, some artwork, a video, another class at Penn, etc.  
- Include in your design doc links to and images of your reference material.

#### Specification:
- Outline the main features of your project.

#### Techniques:
- What are the main technical/algorithmic tools you’ll be using? Give an overview, citing specific papers/articles.

#### Design:
- How will your program fit together? Make a simple free-body diagram illustrating the pieces.

#### Timeline:
- Create a week-by-week set of milestones for each person in your group. Make sure you explicitly outline what each group member's duties will be.

Submit your Design doc as usual via pull request against this repository.
## Milestone 1: Implementation part 1 (due 11/12)
Begin implementing your engine! Don't worry too much about polish or parameter tuning -- this week is about getting together the bulk of your generator implemented. By the end of the week, even if your visuals are crude, the majority of your generator's functionality should be done.

Put all your code in your forked repository.

Submission: Add a new section to your README titled: Milestone #1, which should include
- written description of progress on your project goals. If you haven't hit all your goals, what's giving you trouble?
- Examples of your generators output so far
We'll check your repository for updates. No need to create a new pull request.
## Milestone 2: Implementation part 2 (due 11/24)
We're over halfway there! This week should be about fixing bugs and extending the core of your generator. Make sure by the end of this week _your generator works and is feature complete._ Any core engine features that don't make it in this week should be cut! Don't worry if you haven't managed to exactly hit your goals. We're more interested in seeing proof of your development effort than knowing your planned everything perfectly. 

Put all your code in your forked repository.

Submission: Add a new section to your README titled: Milestone #3, which should include
- written description of progress on your project goals. If you haven't hit all your goals, what did you have to cut and why? 
- Detailed output from your generator, images, video, etc.
We'll check your repository for updates. No need to create a new pull request.

Come to class on the due date with a WORKING COPY of your project. We'll be spending time in class critiquing and reviewing your work so far.

## Final submission (due 12/1)
Time to polish! Spen this last week of your project using your generator to produce beautiful output. Add textures, tune parameters, play with colors, play with camera animation. Take the feedback from class critques and use it to take your project to the next level.

Submission:
- Push all your code / files to your repository
- Come to class ready to present your finished project
- Update your README with two sections 
  - final results with images and a live demo if possible
  - post mortem: how did your project go overall? Did you accomplish your goals? Did you have to pivot?

## Topic Suggestions

### Create a generator in Houdini

### A CLASSIC 4K DEMO
- In the spirit of the demo scene, create an animation that fits into a 4k executable that runs in real-time. Feel free to take inspiration from the many existing demos. Focus on efficiency and elegance in your implementation.
- Example: 
  - [cdak by Quite & orange](https://www.youtube.com/watch?v=RCh3Q08HMfs&list=PLA5E2FF8E143DA58C)

### A RE-IMPLEMENTATION
- Take an academic paper or other pre-existing project and implement it, or a portion of it.
- Examples:
  - [2D Wavefunction Collapse Pokémon Town](https://gurtd.github.io/566-final-project/)
  - [3D Wavefunction Collapse Dungeon Generator](https://github.com/whaoran0718/3dDungeonGeneration)
  - [Reaction Diffusion](https://github.com/charlesliwang/Reaction-Diffusion)
  - [WebGL Erosion](https://github.com/LanLou123/Webgl-Erosion)
  - [Particle Waterfall](https://github.com/chloele33/particle-waterfall)
  - [Voxelized Bread](https://github.com/ChiantiYZY/566-final)

### A FORGERY
Taking inspiration from a particular natural phenomenon or distinctive set of visuals, implement a detailed, procedural recreation of that aesthetic. This includes modeling, texturing and object placement within your scene. Does not need to be real-time. Focus on detail and visual accuracy in your implementation.
- Examples:
  - [The Shrines](https://github.com/byumjin/The-Shrines)
  - [Watercolor Shader](https://github.com/gracelgilbert/watercolor-stylization)
  - [Sunset Beach](https://github.com/HanmingZhang/homework-final)
  - [Sky Whales](https://github.com/WanruZhao/CIS566FinalProject)
  - [Snail](https://www.shadertoy.com/view/ld3Gz2)
  - [Journey](https://www.shadertoy.com/view/ldlcRf)
  - [Big Hero 6 Wormhole](https://2.bp.blogspot.com/-R-6AN2cWjwg/VTyIzIQSQfI/AAAAAAAABLA/GC0yzzz4wHw/s1600/big-hero-6-disneyscreencaps.com-10092.jpg)

### A GAME LEVEL
- Like generations of game makers before us, create a game which generates an navigable environment (eg. a roguelike dungeon, platforms) and some sort of goal or conflict (eg. enemy agents to avoid or items to collect). Aim to create an experience that will challenge players and vary noticeably in different playthroughs, whether that means procedural dungeon generation, careful resource management or an interesting AI model. Focus on designing a system that is capable of generating complex challenges and goals.
- Examples:
  - [Rhythm-based Mario Platformer](https://github.com/sgalban/platformer-gen-2D)
  - [Pokémon Ice Puzzle Generator](https://github.com/jwang5675/Ice-Puzzle-Generator)
  - [Abstract Exploratory Game](https://github.com/MauKMu/procedural-final-project)
  - [Tiny Wings](https://github.com/irovira/TinyWings)
  - Spore
  - Dwarf Fortress
  - Minecraft
  - Rogue

### AN ANIMATED ENVIRONMENT / MUSIC VISUALIZER
- Create an environment full of interactive procedural animation. The goal of this project is to create an environment that feels responsive and alive. Whether or not animations are musically-driven, sound should be an important component. Focus on user interactions, motion design and experimental interfaces.
- Examples:
  - [The Darkside](https://github.com/morganherrmann/thedarkside)
  - [Music Visualizer](https://yuruwang.github.io/MusicVisualizer/)
  - [Abstract Mesh Animation](https://github.com/mgriley/cis566_finalproj)
  - [Panoramical](https://www.youtube.com/watch?v=gBTTMNFXHTk)
  - [Bound](https://www.youtube.com/watch?v=aE37l6RvF-c)

### YOUR OWN PROPOSAL
- You are of course welcome to propose your own topic . Regardless of what you choose, you and your team must research your topic and relevant techniques and come up with a detailed plan of execution. You will meet with some subset of the procedural staff before starting implementation for approval.
